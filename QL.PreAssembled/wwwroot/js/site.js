// // Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// // for details on configuring this project to bundle and minify static web assets.

// // Write your JavaScript code.
// (function () {
//   var win = window;
//   /**
//    * iframe 子页面多语言同步初始化方法
//    * @param {Object} opts 配置项
//    * @param {Object} opts.ElementPlus 全局ElementPlus实例
//    * @param {Object} opts.appI18n 全局vue-i18n实例(legacy:false)
//    * @param {Object} opts.localeEn 英文语言包 ElementPlusLocaleEn
//    * @param {Object} opts.localeZh 中文语言包 ElementPlusLocaleZhCn
//    */
//   function initIframeLangSync(opts) {
//     const { ElementPlus, appI18n, localeEn, localeZh } = opts || {};
//     if (!ElementPlus || !appI18n) {
//       console.error('initIframeLangSync：缺少 ElementPlus / appI18n 实例');
//       return;
//     }

//     // 公用语言切换逻辑（与父页面完全统一）
//     function switchLanguage(lang) {
//       if (!lang || (lang !== 'zh-cn' && lang !== 'en')) return;
//       // vue-i18n 响应式更新
//       appI18n.global.locale.value = lang;
//       localStorage.setItem('lang', lang);
//       // 全局事件通知页面内部业务
//       win.dispatchEvent(new CustomEvent('languageChanged', { detail: { lang: lang } }));
//       // ElementPlus 组件语言切换
//       if (lang === 'en') {
//         ElementPlus.useLocale(localeEn);
//       } else {
//         ElementPlus.useLocale(localeZh);
//       }
//     }

//     // 读取本地缓存语言
//     function loadLanguagePreference() {
//       var savedLang = localStorage.getItem('lang');
//       if (savedLang === 'zh-cn' || savedLang === 'en') {
//         return savedLang;
//       }
//       var browserLang = navigator.language || navigator.userLanguage;
//       if (browserLang && browserLang.toLowerCase().startsWith('zh')) {
//         return 'zh-cn';
//       }
//       return 'zh-cn';
//     }

//     // 初始化本地语言
//     const initLang = loadLanguagePreference();
//     switchLanguage(initLang);

//     // 页面加载后主动向父窗口查询当前语言
//     if (win.parent && win.parent !== win) {
//       win.parent.postMessage({ type: 'query-parent-lang' }, '*');
//     }

//     // 监听父页面下发的语言变更消息
//     win.addEventListener('message', function (event) {
//       // 仅接收父页面消息，过滤其他来源
//       if (event.source !== win.parent) return;
//       const msg = event.data;
//       if (msg.type === 'update-child-lang' && msg.lang) {
//         console.log('iframe 同步父页面语言：', msg.lang);
//         switchLanguage(msg.lang);
//       }
//     });
//   }

//   // 挂载到全局window，页面直接调用
//   win.initIframeLangSync = initIframeLangSync;
// })();

// const siteUtils = (function () {
//   var iconMap;
//   function append(_app, opt) {
//     opt = opt || {};
//     iconMap = opt.iconMap;
//     // 挂载到 globalProperties，模板用 $xxx 调用
//     _app.config.globalProperties.$formatIconName = function (icon) {
//       if (!icon) return '';
//       // 情况1：ep:Setting 格式
//       if (icon.startsWith('ep:')) {
//         return icon.replace('ep:', '');
//       }
//       // 情况2：el-icon-setting 老式css名称，要把短横线+小写，转回组件PascalCase名称 Setting
//       if (icon.startsWith('el-icon-')) {
//         let raw = icon.replace('el-icon-', '');
//         // kebab-case → PascalCase
//         return raw.split('-').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join('');
//       }
//       return icon;
//     };

//     // 可继续追加任意多个公共方法
//     _app.config.globalProperties.$formatDate = function (val) {
//       if (!val) return '';
//       return new Date(val).toLocaleDateString();
//     };

//     _app.config.globalProperties.$calcSize = function (num) {
//       return (num / 1024).toFixed(2) + 'KB';
//     };
//   }
//   return {
//     append: append
//   }
// })()