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

    var app = Vue.createApp({
        data() {
            var validatePass = (rule, value, callback) => {
                if (this.formData.password && value !== this.formData.password) {
                    callback(new Error('两次输入的密码不一致'));
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
                dialogTitle: '新增用户',
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
                        { required: true, message: '请输入登录名称', trigger: 'blur' }
                    ],
                    password: [
                        { required: true, message: '请输入登录密码', trigger: 'blur' }
                    ],
                    confirmPassword: [
                        { required: true, message: '请再次输入密码', trigger: 'blur' },
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
        mounted() {
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
                this.dialogTitle = '新增用户';
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
                this.dialogTitle = '编辑用户';
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
                    ElMessage.error('获取用户信息失败');
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
                            ElMessage.error('CryptoJS库未加载，请重新编译项目后再试');
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
                        ElMessage.success(this.isEditMode ? '编辑成功' : '新增成功');
                        this.dialogVisible = false;
                        this.loadList();
                    } catch (error) {
                        console.error('save user error:', error);
                        ElMessage.error('保存失败：' + (error.message || error));
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
                    ElMessage.warning('请输入用户名');
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
                            ElMessage.info('未查询到用户');
                        }
                    } else {
                        ElMessage.warning(resData.msg || '查询失败');
                    }
                } catch (error) {
                    console.error('search MD user error:', error);
                    ElMessage.error('查询用户失败');
                } finally {
                    this.queryLoading = false;
                }
            },

            handleQuerySelect(row) {
                this.selectedQueryRow = row;
            },

            confirmQueryUser() {
                if (!this.selectedQueryRow) {
                    ElMessage.warning('请选择一行数据');
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
                        '确定要重置用户"' + row.name + '"的密码吗？',
                        '提示',
                        { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' }
                    );
                } catch (e) { return; }
                try {
                    await HttpUtils.post('/Sys/User/ResetPassword', { id: row.id });
                    ElMessage.success('密码已重置为123456');
                } catch (error) {
                    console.error('reset password error:', error);
                }
            },

            handleAssignRole(row) {
                ElMessage.info('分配角色：' + row.name + '（功能待开发）');
            },

            async handleDelete(row) {
                try {
                    await ElMessageBox.confirm(
                        '确定要删除用户"' + row.name + '"吗？',
                        '提示',
                        { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' }
                    );
                } catch (e) { return; }
                try {
                    await HttpUtils.post('/Sys/User/Delete', { id: row.id });
                    ElMessage.success('删除成功');
                    this.loadList();
                } catch (error) {
                    console.error('delete user error:', error);
                }
            }
        }
    });

    app.use(ElementPlus, { locale: ElementPlusLocaleZhCn });
    app.mount('#app');
});
