document.addEventListener('DOMContentLoaded', function () {
  // ---------------- 1. 环境/变量准备 ----------------
  var Vue = window.Vue;
  var ElementPlus = window.ElementPlus;
  var appI18n = window.appI18n; // 子页面全局i18n实例，同父页配置 legacy: false

  // 中英文切换
  initIframeLangSync({
    ElementPlus: ElementPlus,
    appI18n: appI18n,
    localeEn: ElementPlusLocaleEn,
    localeZh: ElementPlusLocaleZhCn
  });

  // 假设你已经引入了 ElementPlusIconsVue 全局对象
  var Icons = window.ElementPlusIconsVue;



  // 图标映射表（复用你之前的逻辑）
  const iconMap = {
    'Setting': Icons.Setting,
    'User': Icons.User,
    'Menu': Icons.Menu,
    'ArrowRight': Icons.ArrowRight
    // ... 如果新增了图标，确保这里也加上
  };

  //// 图标名字格式化工具
  //const formatIconName = (str) => {
  //    if (!str) return '';
  //    return str.replace('el-icon-', '').replace(/^[a-z]/, (match) => match.toUpperCase());
  //};

  // ---------------- 2. Vue 组件/实例 ----------------
  var app = Vue.createApp({
    data() {
      return {
        // 搜索与分页
        searchKeyword: '',
        tableData: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10,
        // 弹窗
        dialogVisible: false,
        // dialogTitle: '', // move to computed
        isEditMode: false,
        // 初始化表单数据结构
        formData: {
          id: 0,
          parentID: null,
          name: '',
          fullName: '',
          icon: '',
          description: '',
          sort: 0,
          permissionFlag: '',
          action: '',
          menuType: 'menu', // 默认选中菜单
          status: 1
        },
        // 定义校验规则
        formRules: {
          // 1. 菜单名称：必填
          name: [
            { required: true, message: '', trigger: 'blur' }
          ],
          // 2. 路由/地址： 声明规则，但先不给 required: true，因为它是动态的
          action: [
            { required: false, message: '', trigger: 'blur' }
          ]
        },
        // 图标选择弹窗
        openIconSelectDialog: false,
        iconSearchKeyword: '',
        // 全部ElementPlus图标名称列表
        allIconList: Object.keys(Icons),
      };
    },
    computed: {
      dialogTitle() {
        return this.isEditMode ? this.$t('sys.menu.dialogEditTitle') : this.$t('sys.menu.dialogAddTitle');
      },
      filteredIconList() {
        if (!this.iconSearchKeyword) return this.allIconList;
        const kw = this.iconSearchKeyword.toLowerCase();
        return this.allIconList.filter(name => name.toLowerCase().includes(kw))
      }
    },
    // 注意：因为使用 Options API 写 setup 容易混淆，我直接写在 methods 里
    mounted() {
      // 表单校验提示绑定多语言
      this.formRules.name[0].message = this.$t('validation.required');
      this.formRules.action[0].message = this.$t('sys.menu.routePlaceholder');
      this.loadMenuData();
    },
    methods: {
      fetchApi(params) {
        // 使用你封装好的全局 HttpUtils 发起 POST 请求
        // 注意：HttpUtils 返回的是 Promise，且拦截器已剥离 code 和 msg
        return HttpUtils.post('/Sys/Menu/List', params);
      },

      // ==========================================
      // 2. 注册 loadMenuData 到 Vue 实例
      // ==========================================
      async loadMenuData() {
        try {
          // 在 methods 内部调用另一个方法，必须使用 this.fetchApi
          const res = await this.fetchApi({
            keyword: this.searchKeyword
          });

          console.log('typeof res', typeof (res), res);

          const flatList = res;

          console.log('flatList:', flatList, !Array.isArray(flatList), flatList.length === 0);
          // 处理空数据的情况
          if (!flatList || !Array.isArray(flatList) || flatList.length === 0) {
            this.tableData = [];
            return;
          }

          console.log('start build tree');

          // 构建并赋值树形结构 (复用你写在 Vue 外部的 buildTree 函数)
          this.tableData = this.buildTree(flatList);

        } catch (error) {
          // 拦截器中已经处理了错误提示，这里只需在控制台输出
          console.error("加载菜单列表失败", error);
        }
      },

      // --- 你的树形构建器 (稍作适配) ---
      buildTree(list) {
        var map = {};
        var roots = [];

        list.forEach(function (item) {
          map[item.id] = { ...item, children: [] };
        });

        list.forEach(function (item) {
          var node = map[item.id];
          // 注意字段大小写 ParentID
          if (item.parentID && map[item.parentID]) {
            map[item.parentID].children.push(node);
          } else {
            roots.push(node);
          }
        });
        // 排序
        const sortRecursive = (nodes) => {
          nodes.sort((a, b) => a.sort - b.sort);
          nodes.forEach(n => {
            if (n.children && n.children.length > 0) sortRecursive(n.children);
          });
        };
        sortRecursive(roots);
        console.log('tree data', roots);
        return roots;
      },

      // --- 按钮操作 ---
      handleSearch() {
        this.currentPage = 1;
        this.loadMenuData();
      },
      handlePageChange(val) {
        this.currentPage = val;
        this.loadMenuData();
      },

      // ===== 2. 打开新增弹窗 =====
      openAddDialog() {
        this.isEditMode = false;
        // 重置表单，保留默认值
        this.formData = {
          id: 0,
          parentID: null,
          name: '',
          fullName: '',
          icon: '',
          description: '',
          sort: 0,
          permissionFlag: '',
          action: '',
          menuType: 'menu',
          status: 1
        };
        this.dialogVisible = true;
      },

      // ===== 3. 打开编辑弹窗 =====
      openEditDialog(row) {
        this.isEditMode = true;
        // 深拷贝行数据，避免编辑时影响表格显示
        // 注意：因为 tableData 是树形结构，row 里面可能带 children，我们要把 children 剔除，不然传参给后端会出错
        const { children, ...pureRow } = row;
        this.formData = JSON.parse(JSON.stringify(pureRow));
        this.dialogVisible = true;
      },

      // ===== 4. 保存数据 (新增 & 更新) =====
      async saveData() {
        // 1. 基础必填校验：名称不能为空
        if (!this.formData.name) {
          ElementPlus.ElMessage.warning(this.$t('sys.menu.namePlaceholder'));
          return;
        }

        // 2. 业务逻辑校验：如果是菜单类型，Action 不能为空
        if (this.formData.menuType === 'menu' && !this.formData.action) {
          ElementPlus.ElMessage.warning(this.$t('sys.menu.formRoute') + this.$t('validation.required'));
          return;
        }

        try {
          // 3. 发送统一的 POST 请求 (后端自动根据 ID 判断新增或更新)
          const result = await HttpUtils.post('/Sys/Menu/Save', this.formData);

          // 4. 成功后提示
          ElementPlus.ElMessage.success(this.$t('message.saveSuccess'));

          // 5. 关闭弹窗并刷新列表
          this.dialogVisible = false;
          this.loadMenuData();

        } catch (error) {
          // 拦截器已处理大部分错误，此处记录本地日志
          console.error('菜单保存失败:', error);
        }
      },
      async handleDelete(row) {
        // 1. 二次确认弹窗 (防止误触)
        const confirmText = this.$t('message.confirmDelete').replace('该记录', `菜单【${row.name}】`);
        const confirmResult = await ElementPlus.ElMessageBox.confirm(
          confirmText,
          this.$t('app.title'),
          {
            confirmButtonText: this.$t('common.confirm'),
            cancelButtonText: this.$t('common.cancel'),
            type: 'warning'
          }
        ).catch(() => false); // 捕获用户点击“取消”的情况

        // 如果用户点击了取消，则直接终止
        if (!confirmResult) {
          return;
        }

        try {
          // 2. 准备好要发送给后端的数据
          // 为了安全，我们不直接修改 row，而是构建一个纯粹用于更新的对象
          const updateData = {
            id: row.id,
            status: -1 // 设置为 -1 代表删除状态
            // 如果你后端在 Save 时要求必须校验完整字段，可能需要深拷贝 row
            // 但通常为了软删除，只传 ID 和 Status 就足够了
          };

          // 3. 调用接口 (复用统一的 Save 接口)
          const result = await HttpUtils.post('/Sys/Menu/Delete', updateData);

          // 4. 成功处理
          ElementPlus.ElMessage.success(this.$t('message.deleteSuccess'));

          // 5. 刷新菜单列表 (重新加载数据)
          this.loadMenuData();

        } catch (error) {
          // HttpUtils 的拦截器已经处理了业务错误和网络错误
          // 这里只需要在控制台记录一下即可
          console.error('菜单删除失败:', error);
        }
      },

      // ===== 5. 选中图标，回填表单，格式 el-icon-xxx
      onIconSelected(iconName) {
        // iconName: "Setting" → 转为 "el-icon-setting"
        const kebabName = iconName.replace(/([A-Z])/g, "-$1").toLowerCase().replace(/^-/, '');
        this.formData.icon = "el-icon-" + kebabName;
        this.openIconSelectDialog = false;
      },
    }
  });

  // ---------------- 3. 注册组件与启动 ----------------
  // 注册所有图标
  for (const [key, component] of Object.entries(Icons)) {
    app.component(key, component);
  }
  // 注册 Vue I18n
  app.use(appI18n);

  // 注册 Element Plus（中文）
  app.use(ElementPlus, {
    locale: ElementPlusLocaleZhCn
  });
  app.mount('#app');

  siteUtils.append(app, { iconMap: iconMap });
});
