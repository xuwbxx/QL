// ============================================================
// 组合所有语言包
// ============================================================

// 1. 加载各语言包（需要先引入对应的 JS 文件）
// 注意：zh-cn.js 和 en.js 必须在 index.js 之前引入

// 2. 构建 messages 对象
var messages = {
    'zh-cn': zhCnMessages,
    'en': enMessages
};

// 3. 创建 i18n 实例
var appI18n = VueI18n.createI18n({
    legacy: false,
    locale: 'zh-cn',
    messages: messages
});

// 4. 暴露到全局（方便其他页面使用）
window.appI18n = appI18n;
window.messages = messages;