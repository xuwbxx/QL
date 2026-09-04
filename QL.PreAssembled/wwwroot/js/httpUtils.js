// wwwroot/js/httpUtils.js
/**
 * Axios封装，非SPA UMD环境，支持自动refresh token
 * 依赖：已经加载 axios.min.js、element-plus
 * storage约定：
 *   authToken: accessToken(JWT)
 *   refreshToken: 刷新令牌
 */
const HttpUtils = (function () {
  const _options = {
    silent: true,
    log: false
  }
  /**
   * 合并配置：传入对象，仅覆盖同名key，不传的字段保留原有值
   * @param {Object} [opt] 部分配置对象
   * @returns {Object} 返回当前完整配置副本
   */
  function option(opt) {
    if (!opt || typeof opt !== 'object') {
      // 不传参数，直接返回当前配置副本
      return { ..._options };
    }
    // 遍历传入opt的自有属性，只覆盖已存在于_options中的字段
    for (const key in opt) {
      if (Object.prototype.hasOwnProperty.call(opt, key)
        && Object.prototype.hasOwnProperty.call(_options, key)) {
        _options[key] = opt[key];
      }
    }
    // 返回拷贝，防止外部直接修改内部_options
    return { ..._options };
  }
  const instance = axios.create({
    baseURL: "",      // 如果后端api有统一前缀可以填 "/api"
    timeout: 15000,   // 请求超时15秒
    headers: {
      "Content-Type": "application/json;charset=utf-8"
    }
  });

  // 刷新中标记：防止并发多个401重复调用refresh
  let isRefreshing = false;
  // 等待刷新的请求队列
  let pendingQueue = [];

  /**
   * 执行刷新token
   * @returns {Promise<string|null>} 新accessToken，失败返回null
   */
  async function doRefreshToken() {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) {
      return null;
    }
    try {
      // 调用你的刷新接口，根据实际后端接口调整url、入参格式
      const resp = await axios.post("/api/token/refresh", {
        refreshToken: refreshToken
      });
      const res = resp.data;
      if (res.code === 200) {
        const newAccessToken = res.accessToken;
        const newRefreshToken = res.refreshToken; // 一次性refresh会返回新refreshToken
        localStorage.setItem("authToken", newAccessToken);
        if (newRefreshToken) {
          localStorage.setItem("refreshToken", newRefreshToken);
        }
        return newAccessToken;
      } else {
        return null;
      }
    } catch (err) {
      // refresh接口返回401/403、网络异常，全部视为refresh失效
      return null;
    }
  }

  /**
   * 清空本地会话并跳转登录
   */
  function clearSessionAndGoLogin() {
    localStorage.removeItem("authToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("userName");
    site.Message.error("登录已失效，请重新登录");
    top.window.location.href = "/Home/Login";
  }

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
      if (_options.log) {
        console.log('response:', response);
      }
      const res = response.data;
      if (res.code !== 200 && res.success != true) {
        site.Message.warning(res.message || res.msg || "业务请求异常");
        return Promise.reject(res);
      }
      if (_options.silent == false && (res.msg || res.message)) {
        site.Message.info(res.msg || res.message);
      }
      return res.data ? res.data : res;
    },
    async function (error) {
      if (!error.response) {
        site.Message.error("网络异常，无法连接服务器");
        return Promise.reject(error);
      }

      const status = error.response.status;
      const originalRequest = error.config;

      // -------------------------- 401自动刷新token核心逻辑 --------------------------
      if (status === 401 && !originalRequest._retry) {
        if (isRefreshing) {
          // 正在刷新，把当前请求放入队列等待刷新结果
          return new Promise((resolve, reject) => {
            pendingQueue.push({ resolve, reject, originalRequest });
          });
        }

        originalRequest._retry = true;
        isRefreshing = true;

        const newAccessToken = await doRefreshToken();
        isRefreshing = false;

        if (newAccessToken) {
          // refresh成功：更新原始请求header，重放本次请求，同时执行队列里积压的请求
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
          pendingQueue.forEach(item => {
            item.originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
            item.resolve(instance(item.originalRequest));
          });
          pendingQueue = [];
          // 重放当前失败请求
          return instance(originalRequest);
        } else {
          // refreshToken 已经过期/无效，直接跳转登录，清空队列
          pendingQueue.forEach(item => {
            item.reject(error);
          });
          pendingQueue = [];
          clearSessionAndGoLogin();
          return Promise.reject(error);
        }
      }
      // -----------------------------------------------------------------------------

      switch (status) {
        case 403:
          site.Message.error("没有权限访问");
          break;
        case 404:
          site.Message.error("接口地址不存在 404");
          break;
        case 500:
          site.Message.error("服务器内部错误 500");
          break;
        default:
          site.Message.error(error.response.data?.message || error.response.data?.msg || `请求错误${status}`);
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
    option: option,
    raw: instance // 暴露原始axios实例，特殊场景直接调用
  };
})();
