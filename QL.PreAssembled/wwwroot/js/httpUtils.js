// wwwroot/js/httpUtils.js
/**
 * Axios封装，非SPA UMD环境
 * 依赖：已经加载 axios.min.js、element-plus
 */
const HttpUtils = (function () {
    const instance = axios.create({
        baseURL: "",      // 如果后端api有统一前缀可以填 "/api"
        timeout: 15000,   // 请求超时15秒
        headers: {
            "Content-Type": "application/json;charset=utf-8"
        }
    });

    // 请求拦截器：附加Bearer Token
    instance.interceptors.request.use(
        function (config) {
            const token = localStorage.getItem("authToken");
            if (token) {
                config.headers.Authorization = `Bearer ${token}`;
            }
            return config;
        },
        function (error) {
            return Promise.reject(error);
        }
    );

    // 响应拦截器
    instance.interceptors.response.use(
        function (response) {
            // 后端业务数据
            const res = response.data;
            // 业务码约定：200代表成功；非200视为业务异常
            if (res.code !== 200) {
                ElementPlus.ElMessage.warning(res.msg || "业务请求异常");
                return Promise.reject(res);
            }
            // 直接返回data，业务页面不需要每次拿 .data.data
            return res.data;
        },
        function (error) {
            if (!error.response) {
                ElementPlus.ElMessage.error("网络异常，无法连接服务器");
                return Promise.reject(error);
            }
            const status = error.response.status;
            switch (status) {
                case 401:
                    // token失效/未登录，清空本地存储，跳登录页
                    localStorage.removeItem("authToken");
                    localStorage.removeItem("userName");
                    ElementPlus.ElMessage.error("登录已失效，请重新登录");
                    window.location.href = "/login.html";
                    break;
                case 403:
                    ElementPlus.ElMessage.error("没有权限访问");
                    break;
                case 404:
                    ElementPlus.ElMessage.error("接口地址不存在 404");
                    break;
                case 500:
                    ElementPlus.ElMessage.error("服务器内部错误 500");
                    break;
                default:
                    ElementPlus.ElMessage.error(error.response.data?.msg || `请求错误${status}`);
            }
            return Promise.reject(error);
        }
    );

    // 对外暴露快捷方法
    return {
        get(url, params) {
            return instance.get(url, { params });
        },
        post(url, data) {
            return instance.post(url, data);
        },
        put(url, data) {
            return instance.put(url, data);
        },
        del(url, params) {
            return instance.delete(url, { params });
        },
        raw: instance // 暴露原始axios实例，特殊场景直接调用
    };
})();