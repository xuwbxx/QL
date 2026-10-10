document.addEventListener('DOMContentLoaded', function () {
    if (typeof CryptoJS === 'undefined') {
        var s = document.createElement('script');
        s.src = '/lib/crypto-js/crypto-js.min.js';
        s.async = false;
        document.head.appendChild(s);
    }
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var ElMessageBox = ElementPlus.ElMessageBox;
    var axios = window.axios;
    var appI18n = window.appI18n; // 子页面全局i18n实例，同父页配置 legacy: false

    // 中英文切换（与父页面语言同步，含 ElementPlus 内置文案）
    initIframeLangSync({
        ElementPlus: ElementPlus,
        appI18n: appI18n,
        localeEn: ElementPlusLocaleEn,
        localeZh: ElementPlusLocaleZhCn
    });

    var app = Vue.createApp({
        data() {
            var validatePass = (rule, value, callback) => {
                if (this.formData.password && value !== this.formData.password) {
                    callback(new Error(this.$t('sys.user.pwdMismatch')));
                } else {
                    callback();
                }
            };
            return {
                searchName: '',
                searchMobile: '',
                loading: false,
                allData: [],
                tableData: [],
                total: 0,
                currentPage: 1,
                pageSize: 10,

                dialogVisible: false,
                isEditMode: false,
                submitting: false,
                formData: {
                    id: 0,
                    empNo: '',
                    account: '',
                    password: '',
                    confirmPassword: '',
                    name: '',
                    roleIds: [],
                    mobile: '',
                    email: '',
                    deptOID: '',
                    deptName: ''
                },
                formRules: {
                    account: [
                        { required: true, message: '', trigger: 'blur' }
                    ],
                    password: [
                        { required: true, message: '', trigger: 'blur' }
                    ],
                    confirmPassword: [
                        { required: true, message: '', trigger: 'blur' },
                        { validator: validatePass, trigger: 'blur' }
                    ]
                },
                roleOptions: [],

                queryDialogVisible: false,
                queryUserName: '',
                queryLoading: false,
                queryTableData: [],
                selectedQueryRow: null
            };
        },
        computed: {
            dialogTitle() {
                return this.isEditMode ? this.$t('sys.user.dialogEditTitle') : this.$t('sys.user.dialogAddTitle');
            }
        },
        mounted() {
            // 表单校验提示绑定多语言
            this.formRules.account[0].message = this.$t('validation.required');
            this.formRules.password[0].message = this.$t('app.passwordPlaceholder');
            this.formRules.confirmPassword[0].message = this.$t('sys.user.confirmPasswordPlaceholder');
            this.loadList();
            this.loadRoleOptions();
        },
        methods: {
            async loadList() {
                this.loading = true;
                try {
                    var req = { name: this.searchName, mobile: this.searchMobile };
                    var data = await HttpUtils.post('/Sys/User/List', req);
                    this.allData = data || [];
                    this.total = this.allData.length;
                    this.currentPage = 1;
                    this.buildPagedData();
                } catch (error) {
                    console.error('load user list error:', error);
                } finally {
                    this.loading = false;
                }
            },

            async loadRoleOptions() {
                try {
                    var data = await HttpUtils.get('/Sys/User/RoleList');
                    this.roleOptions = data || [];
                } catch (error) {
                    console.error('load role options error:', error);
                }
            },

            buildPagedData() {
                var start = (this.currentPage - 1) * this.pageSize;
                var end = start + this.pageSize;
                this.tableData = this.allData.slice(start, end);
            },

            handlePageChange(val) {
                this.currentPage = val;
                this.buildPagedData();
            },
            handleSizeChange(val) {
                this.pageSize = val;
                this.currentPage = 1;
                this.buildPagedData();
            },
            handleSearch() {
                this.currentPage = 1;
                this.loadList();
            },
            handleReset() {
                this.searchName = '';
                this.searchMobile = '';
                this.currentPage = 1;
                this.loadList();
            },

            // ========== 新增/编辑 ==========
            openAddDialog() {
                this.isEditMode = false;
                this.formData = {
                    id: 0,
                    empNo: '',
                    account: '',
                    password: '',
                    confirmPassword: '',
                    name: '',
                    roleIds: [],
                    mobile: '',
                    email: '',
                    deptOID: '',
                    deptName: ''
                };
                this.dialogVisible = true;
                this.$nextTick(() => {
                    if (this.$refs.userFormRef) {
                        this.$refs.userFormRef.clearValidate();
                    }
                });
            },

            async handleEdit(row) {
                this.isEditMode = true;
                try {
                    var data = await HttpUtils.get('/Sys/User/Detail?id=' + row.id);
                    if (data) {
                        this.formData = {
                            id: data.id || data.ID,
                            empNo: data.empNo || data.EmpNo || '',
                            account: data.account || data.Account || '',
                            password: '',
                            confirmPassword: '',
                            name: data.name || data.Name || '',
                            roleIds: data.roleIds || data.RoleIds || [],
                            mobile: data.mobile || data.Mobile || '',
                            email: data.email || data.Email || '',
                            deptOID: data.deptOID || data.DeptOID || '',
                            deptName: data.deptName || data.DeptName || ''
                        };
                    }
                } catch (error) {
                    console.error('get user detail error:', error);
                    ElMessage.error(this.$t('sys.user.loadDetailFailed'));
                    return;
                }
                this.dialogVisible = true;
                this.$nextTick(() => {
                    if (this.$refs.userFormRef) {
                        this.$refs.userFormRef.clearValidate();
                    }
                });
            },

            async saveData() {
                if (!this.$refs.userFormRef) return;
                this.$refs.userFormRef.validate(async (valid) => {
                    if (!valid) return;

                    this.submitting = true;
                    try {
                        if (typeof CryptoJS === 'undefined') {
                            ElMessage.error(this.$t('sys.user.cryptoNotLoaded'));
                            return;
                        }
                        var pwdHashed = CryptoJS.MD5(this.formData.password).toString();
                        var postData = {
                            id: this.formData.id,
                            empNo: this.formData.empNo,
                            account: this.formData.account,
                            password: pwdHashed,
                            confirmPassword: pwdHashed,
                            name: this.formData.name,
                            roleIds: this.formData.roleIds,
                            mobile: this.formData.mobile,
                            email: this.formData.email,
                            deptOID: this.formData.deptOID,
                            deptName: this.formData.deptName
                        };
                        await HttpUtils.post('/Sys/User/Save', postData);
                        ElMessage.success(this.$t('message.saveSuccess'));
                        this.dialogVisible = false;
                        this.loadList();
                    } catch (error) {
                        console.error('save user error:', error);
                        ElMessage.error(this.$t('common.failed') + ': ' + (error.message || error));
                    } finally {
                        this.submitting = false;
                    }
                });
            },

            // ========== 查询用户弹窗 ==========
            openQueryUserDialog() {
                this.queryDialogVisible = true;
                this.queryUserName = '';
                this.queryTableData = [];
                this.selectedQueryRow = null;
            },

            async searchMDUser() {
                if (!this.queryUserName.trim()) {
                    ElMessage.warning(this.$t('sys.user.namePlaceholder'));
                    return;
                }
                this.queryLoading = true;
                try {
                    var rawInstance = axios.create({
                        baseURL: '',
                        timeout: 30000,
                        headers: { 'Content-Type': 'application/json;charset=utf-8' }
                    });
                    var res = await rawInstance.post('/Sys/User/QueryMDUser?userName=' + encodeURIComponent(this.queryUserName.trim()));
                    var resData = res.data;
                    if (resData.code === 200) {
                        this.queryTableData = resData.data || [];
                        if (this.queryTableData.length === 0) {
                            ElMessage.info(this.$t('sys.user.noUserFound'));
                        }
                    } else {
                        ElMessage.warning(resData.msg || this.$t('common.failed'));
                    }
                } catch (error) {
                    console.error('search MD user error:', error);
                    ElMessage.error(this.$t('common.failed'));
                } finally {
                    this.queryLoading = false;
                }
            },

            handleQuerySelect(row) {
                this.selectedQueryRow = row;
            },

            confirmQueryUser() {
                if (!this.selectedQueryRow) {
                    ElMessage.warning(this.$t('sys.user.selectRowRequired'));
                    return;
                }
                this.formData.account = this.selectedQueryRow.userLogin;
                this.formData.empNo = this.selectedQueryRow.empcode;
                this.formData.name = this.selectedQueryRow.name;
                this.formData.deptOID = this.selectedQueryRow.roid || '';
                this.formData.deptName = this.selectedQueryRow.dept || '';
                this.queryDialogVisible = false;
                if (this.$refs.userFormRef) {
                    this.$refs.userFormRef.validateField('account');
                }
            },

            // ========== 其他操作 ==========
            async handleResetPwd(row) {
                try {
                    await ElMessageBox.confirm(
                        this.$t('sys.user.confirmResetPwd', { name: row.name }),
                        this.$t('sys.user.notice'),
                        { confirmButtonText: this.$t('common.confirm'), cancelButtonText: this.$t('common.cancel'), type: 'warning' }
                    );
                } catch (e) { return; }
                try {
                    await HttpUtils.post('/Sys/User/ResetPassword', { id: row.id });
                    ElMessage.success(this.$t('sys.user.resetPwdSuccess'));
                } catch (error) {
                    console.error('reset password error:', error);
                }
            },

            handleAssignRole(row) {
                ElMessage.info(this.$t('sys.user.assignRoleTodo', { name: row.name }));
            },

            async handleDelete(row) {
                try {
                    await ElMessageBox.confirm(
                        this.$t('sys.user.confirmDelete', { name: row.name }),
                        this.$t('sys.user.notice'),
                        { confirmButtonText: this.$t('common.confirm'), cancelButtonText: this.$t('common.cancel'), type: 'warning' }
                    );
                } catch (e) { return; }
                try {
                    await HttpUtils.post('/Sys/User/Delete', { id: row.id });
                    ElMessage.success(this.$t('message.deleteSuccess'));
                    this.loadList();
                } catch (error) {
                    console.error('delete user error:', error);
                }
            }
        }
    });

    // 注册 Vue I18n
    app.use(appI18n);

    app.use(ElementPlus, { locale: ElementPlusLocaleZhCn });
    if (window.ElementPlusIconsVue) {
        for (var key in window.ElementPlusIconsVue) {
            if (window.ElementPlusIconsVue[key]) {
                app.component(key, window.ElementPlusIconsVue[key]);
            }
        }
    }
    app.mount('#app');
});
