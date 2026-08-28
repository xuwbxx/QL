# 部署架构总结（Jexus + Systemd托管 ASP.NET Core）
> 环境：麒麟系统，.NET8，dotnet应用由systemd托管，Jexus作为反向代理网关

## 整体架构
- **qlprea.service**：.NET业务站点，仅监听本机回环 `127.0.0.1:5001`，不对外暴露
- **jws.service(Jexus)**：对外监听 `0.0.0.0:8666`，使用 `reproxy` 将全部流量反向代理到 `http://127.0.0.1:5001/`
- 用户访问：`8666` → Jexus → 转发至本机5001端口的dotnet程序

> ⚠️注意：Jexus反向代理关键字为 `reproxy`，**不是 Nginx 的 proxy_pass**。

## 1、dotnet systemd服务配置
文件路径：`/etc/systemd/system/qlprea.service`
```ini
[Unit]
Description=QL PreAssembled API
After=network.target

[Service]
WorkingDirectory=/var/www/qlprea
ExecStart=/root/.dotnet/dotnet QL.PreAssembled.dll --urls="http://127.0.0.1:5001"
Restart=always
RestartSec=3
Environment=DOTNET_ROOT=/root/.dotnet
Environment=ASPNETCORE_ENVIRONMENT=Production
User=root
Group=root

[Install]
WantedBy=multi-user.target
```

生效命令
```bash
systemctl daemon-reload
systemctl enable qlprea
systemctl start qlprea
systemctl status qlprea
```

## 2、Jexus站点配置
文件路径：`/usr/jexus/siteconf/qlprea`
```ini
port=8666
root=/ /var/www/qlprea
hosts=*
addr=0.0.0.0

reproxy=/ http://127.0.0.1:5001/

NoLog=false
ResponseHandler.Add=X-Frame-Options:SAMEORIGIN
```
> 文件内**不能保留任何 AppHost 相关配置**，全部删除，不要仅依靠注释。

单站点重载（无需重启整个jexus主进程）
```bash
/usr/jexus/jws stop qlprea
/usr/jexus/jws start qlprea
```

完整jexus服务操作
```bash
systemctl restart jws
systemctl status jws
```

## 3、日常运维命令
### 业务站点 qlprea
```bash
# 启停重启
systemctl start qlprea
systemctl stop qlprea
systemctl restart qlprea

# 实时查看dotnet控制台日志
journalctl -u qlprea -f

# 查看最近200行日志
journalctl -u qlprea -n 200
```

### Jexus网关
```bash
# jexus日志
tail -f /usr/jexus/log/jws.log

# 查看端口监听状态
ss -tulnp | grep 5001
ss -tulnp | grep 8666
```

### 功能验证
```bash
# 直接访问后端dotnet
curl 127.0.0.1:5001

# 通过jexus代理访问
curl 127.0.0.1:8666
```

## 4、备选方案（可移除Jexus，仅维护单个systemd服务）
> 内网场景可用，不需要网关时使用。
修改qlprea.service，将监听地址改为对外网卡：
```ini
ExecStart=/root/.dotnet/dotnet QL.PreAssembled.dll --urls="http://0.0.0.0:8666"
```
执行生效：
```bash
systemctl daemon-reload
systemctl restart qlprea
```
> 此时直接访问8666，不再依赖Jexus。

## 5、关键踩坑备忘
1. Jexus反代指令：`reproxy=/ http://127.0.0.1:5001/`，**末尾斜杠不可省略**。
2. 配置文件残留`AppHost`相关字符串会触发jws‑aspd模块，产生unix socket报错。(存疑)
3. systemd的环境独立于交互式shell，必须显式注入`DOTNET_ROOT`。
4. SELinux如果开启，需要放行http出站权限：`setsebool -P httpd_can_network_connect 1`；本环境SELinux为Disabled。
5. jexus出现卡死无响应时，可使用强制杀死：`systemctl kill --signal=SIGKILL jws`。