// ============================================================
// site-utils.js 全站通用Vue工具挂载
// 加载范围：全部页面(父首页 + iframe子页面)
// 职责：接收vue app实例，挂载globalProperties全局工具
// ============================================================
const siteUtils = (function () {
  function append(_app) {
    _app.config.globalProperties.$formatIconName = function (icon) {
      if (!icon) return '';
      return icon.replace('el-icon-', '');
    };

    _app.config.globalProperties.$formatDate = function (val) {
      if (!val) return '';
      return new Date(val).toLocaleDateString();
    };

    _app.config.globalProperties.$calcSize = function (num) {
      return (num / 1024).toFixed(2) + 'KB';
    };
  }

  return {
    append: append
  };
})();