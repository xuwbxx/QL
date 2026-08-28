// ============================================================
// i18n-utils.js  i18n底层通用工具
// 加载范围：全部页面(父首页 + iframe子页面)
// 职责：语言切换、读取偏好、通用合并i18n资源、JS调用翻译
// 模式：闭包内部实现，双输出：i18nUtils(新规范) + window顶层别名(兼容旧代码，渐进迁移)
// 迁移标记：全部页面改造完成后，删除下方【兼容旧代码】区块
// 依赖：外部全局 appI18n (由页面创建Vue-i18n实例)
// ============================================================
(function () {

  function switchLanguage(lang) {
    if (!lang || (lang !== 'zh-cn' && lang !== 'en')) return;
    appI18n.global.locale.value = lang;
    localStorage.setItem('lang', lang);
    window.dispatchEvent(new CustomEvent('languageChanged', { detail: { lang: lang } }));
  }

  function loadLanguagePreference() {
    var savedLang = localStorage.getItem('lang');
    if (savedLang === 'zh-cn' || savedLang === 'en') {
      return savedLang;
    }
    var browserLang = navigator.language || navigator.userLanguage;
    if (browserLang && browserLang.toLowerCase().startsWith('zh')) {
      return 'zh-cn';
    }
    return 'zh-cn';
  }

  function getCurrentLanguage() {
    return appI18n.global.locale.value;
  }

  function $t(key, params) {
    return appI18n.global.t(key, params);
  }

  function setNestedMessage(messages, targetPath, data) {
    if (!targetPath || targetPath.trim() === "") {
      Object.assign(messages, data);
      return;
    }
    const keys = targetPath.split(".");
    let cur = messages;
    for (let i = 0; i < keys.length - 1; i++) {
      const k = keys[i];
      if (!cur) cur = {};
      if (!cur[k]) cur[k] = {};
      cur = cur[k];
    }
    const lastKey = keys[keys.length - 1];
    cur[lastKey] = Object.assign(cur[lastKey] || {}, data);
  }

  function mergeToI18n(objZh, objEn, path) {
    const i18nInstance = appI18n.global;
    const langZh = "zh-cn";
    const langEn = "en";

    // 1、先取出当前已经存在的语言完整消息
    let zhMsg = i18nInstance.getLocaleMessage(langZh) || {};
    let enMsg = i18nInstance.getLocaleMessage(langEn) || {};

    // 2、往普通js对象上面嵌套写入 dyn.menu，setNestedMessage操作普通对象，不是ref
    if (objZh) {
      setNestedMessage(zhMsg, path, objZh);
    }
    if (objEn) {
      setNestedMessage(enMsg, path, objEn);
    }

    // 3、使用官方API回写，这才是合法方式
    i18nInstance.setLocaleMessage(langZh, zhMsg);
    i18nInstance.setLocaleMessage(langEn, enMsg);
  }

  // -------- 新规范：推荐使用 i18nUtils.xxx --------
  const i18nUtils = {
    switchLanguage,
    loadLanguagePreference,
    getCurrentLanguage,
    $t,
    mergeToI18n
  };
  window.i18nUtils = i18nUtils;

  // -------- 【兼容旧代码，迁移完成后删除此块】 --------
  window.switchLanguage = switchLanguage;
  window.loadLanguagePreference = loadLanguagePreference;
  window.getCurrentLanguage = getCurrentLanguage;
  window.$t = $t;
  window.mergeToI18n = mergeToI18n;

})();