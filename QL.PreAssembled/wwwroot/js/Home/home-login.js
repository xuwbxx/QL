document.addEventListener('DOMContentLoaded', function () {
    const { createApp, ref, reactive, computed } = Vue;
    const { ElMessage } = ElementPlus;

    const app = createApp({
        computed: {
            loginRules() {
                return {
                    account: [
                        { required: true, message: $t('app.accountPlaceholder'), trigger: 'blur' },
                        { min: 2, max: 20, message: $t('app.accountRule'), trigger: 'blur' }
                    ],
                    password: [
                        { required: true, message: $t('app.passwordPlaceHolder'), trigger: 'blur' },
                        { min: 6, max: 30, message: $t('app.passwordRule'), trigger: 'blur' }
                    ]
                };
            }
        },
        setup() {
            // 表单引用
            const loginFormRef = ref(null);

            // 加载状态
            const loading = ref(false);

            // 当前语言（从公共函数读取偏好）
            var currentLocale = Vue.ref(loadLanguagePreference());

            // 登录表单数据
            const loginForm = reactive({
                account: '',
                password: ''
            });

            // 表单验证规则
            const loginRules = {
                account: [
                    { required: true, message: '请输入账号', trigger: 'blur' },
                    { min: 2, max: 20, message: '账号长度在 2 到 20 个字符', trigger: 'blur' }
                ],
                password: [
                    { required: true, message: '请输入密码', trigger: 'blur' },
                    { min: 6, max: 30, message: '密码长度在 6 到 30 个字符', trigger: 'blur' }
                ]
            };

            // 登录提交
            var handleLogin = function () {
                if (!loginFormRef.value) return;

                loginFormRef.value.validate(function (valid) {
                    if (!valid) return;

                    loading.value = true;
                    var lang = currentLocale.value;

                    // ✅ 前端 MD5 加密
                    var pwdHashed = CryptoJS.MD5(loginForm.password).toString();

                    fetch('/api/sys/Auth/Login', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({
                            account: loginForm.account,
                            password: pwdHashed
                        })
                    })
                        .then(function (response) {
                            return response.json().then(function (result) {
                                if (result.success === true) {
                                    if (result.user && result.user.name) {
                                        localStorage.setItem('userName', result.user.name);
                                        localStorage.setItem('userInfo', result.user);
                                    }
                                    window.location = result.redirectUrl || '/';
                                } else {
                                    ElMessage.error(result.message || '登录失败');
                                }
                            });
                        })
                        .catch(function (error) {
                            ElMessage.error('网络异常，请稍后重试');
                        })
                        .finally(function () {
                            loading.value = false;
                        });
                });
            };

            return {
                loginFormRef,
                loginForm,
                loginRules,
                loading,
                handleLogin
            };
        }
    });

    // 注册 Element Plus 图标组件
    app.component('User', ElementPlusIconsVue.User);
    app.component('Lock', ElementPlusIconsVue.Lock);

    // 挂载应用
    app.use(appI18n);
    app.use(ElementPlus, { locale: ElementPlusLocaleZhCn });
    app.mount('#app');
    siteUtils.append(app);
});