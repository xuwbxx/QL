// ============================================================
// site-utils.js 全站通用Vue工具挂载
// 加载范围：全部页面(父首页 + iframe子页面)
// 职责：接收vue app实例，挂载globalProperties全局工具
// ============================================================
const siteUtils = (function () {
  function append(_app) {
    _app.config.globalProperties.$formatIconName = function (icon) {
      if (!icon) return '';
      let name = icon.replace('el-icon-', '');
      // kebab‑case 转 PascalCase大驼峰
      name = name.split('-')
        .map(seg => seg.charAt(0).toUpperCase() + seg.slice(1))
        .join('');
      console.log('$formatIconName:', icon, name);
      return name;
    };

    _app.config.globalProperties.$formatDate = function (val) {
      if (!val) return '';
      return new Date(val).toLocaleDateString();
    };

    _app.config.globalProperties.$calcSize = function (num) {
      return (num / 1024).toFixed(2) + 'KB';
    };
  }

  async function getUserInfoAsync() {
    var userInfo = await HttpUtils.post('/api/sys/Auth/GetUserInfo');
    return userInfo;
  }
  function defError(error) {
    console.error(error);
  }
  function getUserInfo() {
    return HttpUtils.post('/api/sys/Auth/GetUserInfo');
  }
  return {
    append: append,
    getUserInfoAsync: getUserInfoAsync,
    getUserInfo: getUserInfo
  };
})();