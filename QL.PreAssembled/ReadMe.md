# 部署架构总结（Jexus托管 ASP.NET Core）
> 环境：麒麟系统，.NET8，dotnet应用由Jexus托管

## 环境

### 服务器
10.6.48.26

### 站点目录
/website/qlprea

### Jexus 站点
qlprea

### Jexus 命令
/usr/jexus/jws stop qlprea
/usr/jexus/jws start qlprea

## 发布步骤

### 打包上传方案步骤
- VS设置发布时，设置本地文件夹，比如../publish/qlprea
- 执行发布后，将../publish/qlprea中有变化的文件，打包成qlprea_yyyyMMdd_001.tar文件，注意，尽量保持文件夹树形结构
- 通过堡垒机，通过[H5运维]方式，登录10.6.48.26
- 展开右侧，打开【文件传输】，将打包好的qlprea_yyyyMMdd_001.tar文件，上传至/devops/qlprea
- 执行命令，脚本将自动解压缩文件，发布到站点，并执行站点重启加载本次更新：
    - cd /devops/qlprea
    - ./deploy.sh qlprea_yyyyMMdd_001.tar

### 直接ftp发布步骤
- VS设置发布时，指定ftp，发布到10.6.48.26的/website/qlprea目录
- 通过堡垒机，登录10.6.48.26
- 执行命令，重启站点，加载更新：
    - /usr/jexus/jws stop qlprea
    - /usr/jexus/jws start qlprea