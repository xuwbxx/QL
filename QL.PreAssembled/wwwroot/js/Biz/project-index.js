/**
 * 项目管理 Vue 应用
 * 三级树形展示：项目 → 桥梁子项 → 浇筑分组
 * 混凝土梁下可新增浇筑分组子项，钢梁不可
 */
document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElementPlusIconsVue = window.ElementPlusIconsVue;

    var app = Vue.createApp({
        data() {
            return {
                // 搜索 & 分页
                searchName: '',
                searchProgressStatus: null,
                loading: false,
                allData: [],
                tableData: [],
                allBridges: [],
                allCastingGroups: [],
                total: 0,
                currentPage: 1,
                pageSize: 10,

                // 弹窗 - 项目
                dialogVisible: false,
                dialogTitle: '新增项目',
                isEditMode: false,
                submitting: false,

                // 弹窗 - 子项（桥梁）
                subDialogVisible: false,
                subDialogTitle: '新增子项',
                subEditMode: false,
                subSubmitting: false,

                // 弹窗 - 子项（浇筑分组）
                castDialogVisible: false,
                castDialogTitle: '新增子项',
                castEditMode: false,
                castSubmitting: false,

                // 表单数据 - 项目
                formData: {
                    id: 0,
                    projectId: 0,
                    name: '',
                    description: '',
                    managerId: null,
                    managerName: '',
                    progressStatus: 0,
                    status: 1
                },

                // 表单数据 - 桥梁子项
                subFormData: {
                    id: 0,
                    projID: 0,
                    projName: '',
                    name: '',
                    beamType: 0,
                    status: 1
                },

                // 表单数据 - 浇筑分组子项
                castFormData: {
                    id: 0,
                    bridgeID: 0,
                    projName: '',
                    bridgeName: '',
                    name: '',
                    status: 1
                },

                statusOptions: [
                    { label: '在建', value: 0 },
                    { label: '完工', value: 1 }
                ],

                beamTypeOptions: [
                    { label: '钢梁', value: 0 },
                    { label: '混凝土梁', value: 1 }
                ],

                managerOptions: [],

                // 授权弹窗
                authDialogVisible: false,
                authProjectId: 0,
                authProjectName: '',
                authSubmitting: false,
                authAllUsers: [],
                authSelected: [],
                authSearchKey: '',
                authLeftCurrentPage: 1,
                authLeftPageSize: 10,
                authRightCurrentPage: 1,
                authRightPageSize: 10
            };
        },

        computed: {
            authFilteredLeft() {
                var self = this;
                var key = (this.authSearchKey || '').trim().toLowerCase();
                var selectedIds = {};
                this.authSelected.forEach(function (u) { selectedIds[u.id] = true; });
                return this.authAllUsers.filter(function (u) {
                    if (selectedIds[u.id]) return false;
                    if (!key) return true;
                    return (u.name || '').toLowerCase().indexOf(key) >= 0 ||
                        (u.account || '').toLowerCase().indexOf(key) >= 0;
                });
            },

            authLeftPaged() {
                var start = (this.authLeftCurrentPage - 1) * this.authLeftPageSize;
                return this.authFilteredLeft.slice(start, start + this.authLeftPageSize);
            },

            authRightPaged() {
                var start = (this.authRightCurrentPage - 1) * this.authRightPageSize;
                return this.authSelected.slice(start, start + this.authRightPageSize);
            }
        },

        mounted() {
            this.loadManagerOptions();
            this.loadList();
        },

        methods: {
            async loadManagerOptions() {
                try {
                    const data = await HttpUtils.post('/Biz/Project/UserList', {});
                    this.managerOptions = data || [];
                } catch (error) {
                    console.error('load manager error:', error);
                }
            },

            handleManagerChange(val) {
                var selected = this.managerOptions.find(function (u) { return u.id === val; });
                this.formData.managerName = selected ? selected.name : '';
            },

            async loadList() {
                this.loading = true;
                try {
                    var req = {
                        name: this.searchName,
                        progressStatus: this.searchProgressStatus
                    };
                    // 1. 加载项目列表
                    const projects = await HttpUtils.post('/Biz/Project/List', req);
                    this.allData = projects || [];
                    this.total = this.allData.length;
                    this.currentPage = 1;

                    // 2. 加载桥梁列表
                    try {
                        const bridges = await HttpUtils.post('/Biz/Project/BridgeList', {});
                        this.allBridges = bridges || [];
                    } catch (bridgeErr) {
                        console.error('bridge list error:', bridgeErr);
                        this.allBridges = [];
                    }

                    // 3. 加载浇筑分组列表
                    try {
                        const groups = await HttpUtils.post('/Biz/Project/CastingGroupList', {});
                        this.allCastingGroups = groups || [];
                    } catch (castErr) {
                        console.error('casting group list error:', castErr);
                        this.allCastingGroups = [];
                    }

                    this.buildTree();
                } catch (error) {
                    console.error('load project error:', error);
                } finally {
                    this.loading = false;
                }
            },

            buildTree() {
                var self = this;
                var tree = [];
                var start = (this.currentPage - 1) * this.pageSize;
                var end = start + this.pageSize;
                var pagedProjects = this.allData.slice(start, end);

                pagedProjects.forEach(function (proj) {
                    var node = Object.assign({}, proj, {
                        rowKey: 'p_' + proj.id,
                        isProject: true,
                        isBridge: false,
                        isCastingGroup: false,
                        children: []
                    });
                    var childBridges = self.allBridges.filter(function (b) {
                        return b.projID === proj.id;
                    });
                    childBridges.forEach(function (bridge) {
                        var bridgeNode = Object.assign({}, bridge, {
                            rowKey: 'b_' + bridge.id,
                            isProject: false,
                            isBridge: true,
                            isCastingGroup: false,
                            children: []
                        });
                        // 混凝土梁下挂浇筑分组
                        var childGroups = self.allCastingGroups.filter(function (g) {
                            return g.bridgeID === bridge.id;
                        });
                        childGroups.forEach(function (group) {
                            bridgeNode.children.push(Object.assign({}, group, {
                                rowKey: 'c_' + group.id,
                                isProject: false,
                                isBridge: false,
                                isCastingGroup: true
                            }));
                        });
                        node.children.push(bridgeNode);
                    });
                    tree.push(node);
                });

                this.tableData = tree;
            },

            handlePageChange(val) {
                this.currentPage = val;
                this.buildTree();
            },

            handleSizeChange(val) {
                this.pageSize = val;
                this.currentPage = 1;
                this.buildTree();
            },

            handleSearch() {
                this.currentPage = 1;
                this.loadList();
            },

            handleReset() {
                this.searchName = '';
                this.searchProgressStatus = null;
                this.currentPage = 1;
                this.loadList();
            },

            // ==================== 项目 CRUD ====================

            openAddDialog() {
                this.isEditMode = false;
                this.dialogTitle = '新增项目';
                this.formData = {
                    id: 0,
                    projectId: 0,
                    name: '',
                    description: '',
                    managerId: null,
                    managerName: '',
                    progressStatus: 0,
                    status: 1
                };
                this.dialogVisible = true;
            },

            openEditDialog(row) {
                this.isEditMode = true;
                this.dialogTitle = '编辑项目';
                var pure = Object.assign({}, row);
                delete pure.children;
                delete pure.rowKey;
                delete pure.isProject;
                delete pure.isBridge;
                delete pure.isCastingGroup;
                this.formData = JSON.parse(JSON.stringify(pure));
                this.dialogVisible = true;
            },

            async saveData() {
                if (!this.formData.name) {
                    ElementPlus.ElMessage.warning('请输入项目名称');
                    return;
                }
                if (!this.formData.managerId) {
                    ElementPlus.ElMessage.warning('请选择项目负责人');
                    return;
                }
                this.submitting = true;
                try {
                    await HttpUtils.post('/Biz/Project/Save', this.formData);
                    ElementPlus.ElMessage.success(this.isEditMode ? '编辑成功' : '新增成功');
                    this.dialogVisible = false;
                    this.loadList();
                } catch (error) {
                    console.error('save error:', error);
                } finally {
                    this.submitting = false;
                }
            },

            async handleDelete(row) {
                try {
                    await ElementPlus.ElMessageBox.confirm(
                        '确定要删除项目"' + row.name + '"吗？',
                        '提示',
                        { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' }
                    );
                } catch (e) {
                    return;
                }
                try {
                    await HttpUtils.post('/Biz/Project/Delete', { id: row.id });
                    ElementPlus.ElMessage.success('删除成功');
                    this.loadList();
                } catch (error) {
                    console.error('delete error:', error);
                }
            },

            // ==================== 桥梁子项 CRUD ====================

            openAddSubDialog(row) {
                this.subEditMode = false;
                this.subDialogTitle = '新增子项';
                this.subFormData = {
                    id: 0,
                    projID: row.id,
                    projName: row.name,
                    name: '',
                    beamType: 0,
                    status: 1
                };
                this.subDialogVisible = true;
            },

            openEditSubDialog(row) {
                this.subEditMode = true;
                this.subDialogTitle = '编辑子项';
                var proj = this.allData.find(function (p) { return p.id === row.projID; });
                this.subFormData = {
                    id: row.id,
                    projID: row.projID,
                    projName: proj ? proj.name : '',
                    name: row.name,
                    beamType: row.beamType,
                    status: row.status
                };
                this.subDialogVisible = true;
            },

            async saveSubItem() {
                if (!this.subFormData.name) {
                    ElementPlus.ElMessage.warning('请输入桥梁名称');
                    return;
                }
                this.subSubmitting = true;
                try {
                    var postData = {
                        id: this.subFormData.id,
                        projID: this.subFormData.projID,
                        name: this.subFormData.name,
                        beamType: this.subFormData.beamType,
                        status: this.subFormData.status
                    };
                    await HttpUtils.post('/Biz/Project/BridgeSave', postData);
                    ElementPlus.ElMessage.success(this.subEditMode ? '编辑成功' : '新增成功');
                    this.subDialogVisible = false;
                    this.loadList();
                } catch (error) {
                    console.error('save sub error:', error);
                } finally {
                    this.subSubmitting = false;
                }
            },

            async handleDeleteSub(row) {
                try {
                    await ElementPlus.ElMessageBox.confirm(
                        '确定要删除桥梁"' + row.name + '"吗？',
                        '提示',
                        { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' }
                    );
                } catch (e) {
                    return;
                }
                try {
                    await HttpUtils.post('/Biz/Project/BridgeDelete', { id: row.id });
                    ElementPlus.ElMessage.success('删除成功');
                    this.loadList();
                } catch (error) {
                    console.error('delete sub error:', error);
                }
            },

            // ==================== 浇筑分组 CRUD ====================

            openAddCastDialog(row) {
                this.castEditMode = false;
                this.castDialogTitle = '新增子项';
                var proj = this.allData.find(function (p) { return p.id === row.projID; });
                this.castFormData = {
                    id: 0,
                    bridgeID: row.id,
                    projName: proj ? proj.name : '',
                    bridgeName: row.name,
                    name: '',
                    status: 1
                };
                this.castDialogVisible = true;
            },

            openEditCastDialog(row) {
                this.castEditMode = true;
                this.castDialogTitle = '编辑子项';
                var self = this;
                var bridge = this.allBridges.find(function (b) { return b.id === row.bridgeID; });
                var proj = bridge ? self.allData.find(function (p) { return p.id === bridge.projID; }) : null;
                this.castFormData = {
                    id: row.id,
                    bridgeID: row.bridgeID,
                    projName: proj ? proj.name : '',
                    bridgeName: bridge ? bridge.name : '',
                    name: row.name,
                    status: row.status
                };
                this.castDialogVisible = true;
            },

            async saveCastItem() {
                if (!this.castFormData.name) {
                    ElementPlus.ElMessage.warning('请输入浇筑分组名称');
                    return;
                }
                this.castSubmitting = true;
                try {
                    var postData = {
                        id: this.castFormData.id,
                        bridgeID: this.castFormData.bridgeID,
                        name: this.castFormData.name,
                        status: this.castFormData.status
                    };
                    await HttpUtils.post('/Biz/Project/CastingGroupSave', postData);
                    ElementPlus.ElMessage.success(this.castEditMode ? '编辑成功' : '新增成功');
                    this.castDialogVisible = false;
                    this.loadList();
                } catch (error) {
                    console.error('save cast error:', error);
                } finally {
                    this.castSubmitting = false;
                }
            },

            async handleDeleteCast(row) {
                try {
                    await ElementPlus.ElMessageBox.confirm(
                        '确定要删除浇筑分组"' + row.name + '"吗？',
                        '提示',
                        { confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning' }
                    );
                } catch (e) {
                    return;
                }
                try {
                    await HttpUtils.post('/Biz/Project/CastingGroupDelete', { id: row.id });
                    ElementPlus.ElMessage.success('删除成功');
                    this.loadList();
                } catch (error) {
                    console.error('delete cast error:', error);
                }
            },

            // ==================== 其他 ====================

            handleAuthorize(row) {
                this.authProjectId = row.id;
                this.authProjectName = row.name;
                this.authDialogVisible = true;
            },

            async onAuthDialogOpened() {
                this.authAllUsers = [];
                this.authSelected = [];
                this.authSearchKey = '';
                this.authLeftCurrentPage = 1;
                this.authRightCurrentPage = 1;

                try {
                    var users = await HttpUtils.post('/Sys/User/List', {});
                    this.authAllUsers = (users || []).map(function (u) {
                        return {
                            id: u.id || u.ID,
                            name: u.name || u.Name || '',
                            account: u.account || u.Account || '',
                            deptName: u.deptName || u.DeptName || ''
                        };
                    });

                    var authorizedIds = await HttpUtils.post('/Biz/Project/GetAuthorizedUsers', { projId: this.authProjectId });
                    var idSet = {};
                    (authorizedIds || []).forEach(function (id) { idSet[id] = true; });
                    this.authSelected = this.authAllUsers.filter(function (u) { return idSet[u.id]; });
                } catch (error) {
                    console.error('load auth data error:', error);
                    ElementPlus.ElMessage.error('加载授权数据失败');
                }
            },

            isAuthSelected(id) {
                return this.authSelected.some(function (u) { return u.id === id; });
            },

            addAuthUser(user) {
                if (this.isAuthSelected(user.id)) return;
                this.authSelected.push(user);
                this.authRightCurrentPage = 1;
            },

            removeAuthUser(user) {
                var idx = this.authSelected.findIndex(function (u) { return u.id === user.id; });
                if (idx >= 0) this.authSelected.splice(idx, 1);
                var maxPage = Math.ceil(this.authSelected.length / this.authRightPageSize) || 1;
                if (this.authRightCurrentPage > maxPage) this.authRightCurrentPage = maxPage;
            },

            clearAuthSelected() {
                this.authSelected = [];
                this.authRightCurrentPage = 1;
            },

            onAuthSearchInput() {
                this.authLeftCurrentPage = 1;
            },

            onAuthLeftPageChange(val) {
                this.authLeftCurrentPage = val;
            },

            onAuthRightPageChange(val) {
                this.authRightCurrentPage = val;
            },

            async saveAuthorization() {
                this.authSubmitting = true;
                try {
                    var userIds = this.authSelected.map(function (u) { return u.id; });
                    await HttpUtils.post('/Biz/Project/SaveAuthorize', {
                        projId: this.authProjectId,
                        userIds: userIds
                    });
                    ElementPlus.ElMessage.success('授权成功');
                    this.authDialogVisible = false;
                } catch (error) {
                    console.error('save authorization error:', error);
                    ElementPlus.ElMessage.error('授权保存失败');
                } finally {
                    this.authSubmitting = false;
                }
            },

            getStatusLabel(val) {
                if (val === 0 || val === '0') return '在建';
                if (val === 1 || val === '1') return '完工';
                return '未知';
            },

            getStatusTagType(val) {
                if (val === 0 || val === '0') return 'warning';
                if (val === 1 || val === '1') return 'success';
                return 'info';
            },

            getBeamTypeLabel(val) {
                if (val === 0 || val === '0') return '钢梁';
                if (val === 1 || val === '1') return '混凝土梁';
                return '混凝土梁';
            },

            getBeamTypeTagType(val) {
                if (val === 0 || val === '0') return '';
                if (val === 1 || val === '1') return 'success';
                return 'info';
            }
        }
    });

    app.use(ElementPlus, {
        locale: ElementPlusLocaleZhCn
    });

    if (ElementPlusIconsVue) {
        app.component('Check', ElementPlusIconsVue.Check);
        app.component('Delete', ElementPlusIconsVue.Delete);
    }

    app.mount('#app');
});