// ============================================================
// iframe-page-utils.js
// 加载范围：仅被iframe嵌入的子业务页面加载，父首页不要引入
// 职责：iframe子页面多语言同步初始化
// 依赖：必须先引入 i18n-utils.js、site-utils.js
// ============================================================
(function () {
  var win = window;

  /**
   * 【仅iframe子页面调用！父页面禁止调用】iframe子页面多语言同步初始化方法
   * @param {Object} opts 配置项
   * @param {Object} opts.ElementPlus 全局ElementPlus实例
   * @param {Object} opts.appI18n 全局vue-i18n实例(legacy:false)
   * @param {Object} opts.localeEn 英文语言包 ElementPlusLocaleEn
   * @param {Object} opts.localeZh 中文语言包 ElementPlusLocaleZhCn
   */
  function initIframeLangSync(opts) {
    const { ElementPlus, appI18n, localeEn, localeZh } = opts || {};
    if (!ElementPlus || !appI18n) {
      console.error('initIframeLangSync：缺少 ElementPlus / appI18n 实例');
      return;
    }

    function iframeSwitchLang(lang) {
      // 后续重构：改为 i18nUtils.switchLanguage(lang);
      switchLanguage(lang);
      if (lang === 'en') {
        ElementPlus.useLocale(localeEn);
      } else {
        ElementPlus.useLocale(localeZh);
      }
    }

    // 后续重构：改为 const initLang = i18nUtils.loadLanguagePreference();
    const initLang = loadLanguagePreference();
    iframeSwitchLang(initLang);

    if (win.parent && win.parent !== win) {
      win.parent.postMessage({ type: 'query-parent-lang' }, '*');
    }

    win.addEventListener('message', function (event) {
      if (event.source !== win.parent) return;
      const msg = event.data;
      if (msg.type === 'update-child-lang' && msg.lang) {
        console.log('iframe 同步父页面语言：', msg.lang);
        iframeSwitchLang(msg.lang);
      }
    });
  }

  win.initIframeLangSync = initIframeLangSync;
})();