document.addEventListener('DOMContentLoaded', function () {
  var Vue = window.Vue;
  var axios = window.axios;
  var ElementPlus = window.ElementPlus;
  var ElMessage = ElementPlus.ElMessage;
  var ElementPlusIconsVue = window.ElementPlusIconsVue;

  // 图标映射字典
  const iconMap = {
    'Setting': ElementPlusIconsVue.Setting,
    'User': ElementPlusIconsVue.User,
    'Menu': ElementPlusIconsVue.Menu,
    'Expand': ElementPlusIconsVue.Expand,
    'Fold': ElementPlusIconsVue.Fold,
    'Document': ElementPlusIconsVue.Document
  }

  // 递归菜单组件（当前页面未使用，保留兼容）
  const RecursiveMenu = {
    name: 'RecursiveMenu',
    props: ['list'],
    template: `
        <template v-for="item in list" :key="item.id">
            <el-sub-menu v-if="item.menuType === 'directory' && item.children && item.children.length > 0" :index="String(item.id)">
                <template #title>
                    <el-icon v-if="item.icon">
                        <component :is="$parent.iconMap[$parent.formatIconName(item.icon)]" />
                    </el-icon>
                    <span>{{ item.name }}</span>
                </template>
                <RecursiveMenu :list="item.children" />
            </el-sub-menu>
            <el-menu-item v-else-if="item.menuType === 'menu'" :index="String(item.id)">
                <el-icon v-if="item.icon">
                    <component :is="$parent.iconMap[$parent.formatIconName(item.icon)]" />
                </el-icon>
                <span>{{ item.name }}</span>
            </el-menu-item>
        </template>`
  };

  var app = Vue.createApp({
    setup: function () {
      // ========== 响应式数据 ==========
      var userInfo = Vue.reactive({
        userName: ''
      });

      var elMenuRef = Vue.ref(null);
      var rawMenuList = Vue.ref([]);
      var menuTree = Vue.ref([]);
      var activeMenuId = Vue.ref('');

      // 动态iframe标签页列表（仅存放菜单打开的页面）
      var tabList = Vue.ref([]);
      // 当前激活Tab：默认首页固定标识 home
      var activeTabKey = Vue.ref('home');

      var isCollapsed = Vue.ref(false);
      var currentLang = Vue.ref(localStorage.getItem('app-lang') || 'zh-cn');

      // 修改密码弹窗
      var passwordDialogVisible = Vue.ref(false);
      var passwordLoading = Vue.ref(false);
      var passwordFormRef = Vue.ref(null);
      var passwordForm = Vue.reactive({
        oldPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
      var passwordRules = {
        oldPassword: [{ required: true, message: '请输入旧密码', trigger: 'blur' }],
        newPassword: [
          { required: true, message: '请输入新密码', trigger: 'blur' },
          { min: 6, max: 30, message: '密码长度在 6 到 30 个字符', trigger: 'blur' }
        ],
        confirmPassword: [
          { required: true, message: '请再次输入新密码', trigger: 'blur' },
          {
            validator: function (rule, value, callback) {
              if (value !== passwordForm.newPassword) {
                callback(new Error('两次输入密码不一致'));
              } else {
                callback();
              }
            },
            trigger: 'blur'
          }
        ]
      };

      // ========== Axios 请求实例 ==========
      var api = axios.create({ baseURL: '' });
      api.interceptors.request.use(function (config) {
        var token = localStorage.getItem('authToken');
        if (token) config.headers.Authorization = 'Bearer ' + token;
        return config;
      });

      // ========== 工具函数 ==========
      // 菜单动态多语言组装胶水函数
      function mergeMenuToI18n(menuFlatList) {
        if (!menuFlatList || !Array.isArray(menuFlatList) || menuFlatList.length === 0) {
          return;
        }
        const objZh = {};
        const objEn = {};
        for (const item of menuFlatList) {
          if (item.id == null) continue;
          const key = `menu_${item.id}`;
          objZh[key] = item.name || "";
          objEn[key] = item.enName || item.name || "";
        }
        i18nUtils.mergeToI18n(objZh, objEn, "dyn.menu");
      }
      // 构建菜单树
      function buildTree(list) {
        var map = {};
        var roots = [];
        list.forEach(function (item) {
          map[item.id] = { ...item, icon: item.icon || 'el-icon-menu', children: [] };
        });

        list.forEach(function (item) {
          var node = map[item.id];
          if (item.parentID && map[item.parentID]) {
            map[item.parentID].children.push(node);
          } else {
            roots.push(node);
          }
        });

        // 递归：对当前节点的children排序，再递归子节点
        function sortTree(nodes) {
          // 当前层级按sort升序，无sort默认为0
          nodes.sort((a, b) => {
            const sa = a.sort ?? 0;
            const sb = b.sort ?? 0;
            return sa - sb;
          });
          // 递归每一个子节点
          nodes.forEach(n => {
            if (n.children && n.children.length > 0) {
              sortTree(n.children);
            }
          });
        }

        // 从根节点数组开始递归排序整棵树
        sortTree(roots);

        window.menuroots = roots;
        return roots;
      }

      function findMenuItem(menuList, id) {
        for (var i = 0; i < menuList.length; i++) {
          var item = menuList[i];
          if (item.id === id) return item;
          if (item.children && item.children.length > 0) {
            var found = findMenuItem(item.children, id);
            if (found) return found;
          }
        }
        return null;
      }

      function hasChildren(item) {
        if (!item.children || !Array.isArray(item.children)) return false;
        var realMenus = item.children.filter(child => child.menuType !== 'button');
        return realMenus.length > 0;
      }

      // ========== 多语言切换（含iframe广播） ==========
      function switchGlobalLang(lang) {
        switchLanguage(lang);
        currentLang.value = lang;
        const iframeList = document.querySelectorAll('iframe');
        iframeList.forEach(iframe => {
          if (iframe.contentWindow) {
            iframe.contentWindow.postMessage({ type: 'update-child-lang', lang: lang }, '*');
          }
        })
      }

      // ========== 菜单加载 ==========
      async function loadMenus() {
        try {
          var res = {
            data: [
              { id: 1, parentID: 0, name: '系统管理', enName: 'sys mgmt', icon: 'el-icon-user' },
              { id: 2, parentID: 1, name: '用户管理', enName: 'user mgmt', icon: 'el-icon-user', action: '/Sys/User/Index' },
              { id: 3, parentID: 1, name: '菜单管理', enName: 'menu mgmt', icon: 'el-icon-user', action: '/Sys/Menu/Index' },
            ]
          };
          res = await api.post('/api/sys/User/Menu');
          try {
            mergeMenuToI18n(res.data);
          } catch (e) { console.log('mergeMenuToI18n:error', e) }
          rawMenuList.value = res.data;
          menuTree.value = buildTree(res.data);
          // 不再自动打开菜单，默认停留在首页
        } catch (err) {
          console.error('获取菜单失败', err);
          ElMessage.error('加载菜单失败，请重新登录');
        }
      }

      // 点击菜单打开iframe标签页
      function handleMenuSelect(index, indexPath, item) {
        var menuItem = findMenuItem(menuTree.value, Number(index));
        if (!menuItem || !menuItem.action) return;
        var actionUrl = menuItem.action;
        var menuName = menuItem.name;

        var existTab = tabList.value.find(t => t.action === actionUrl);
        if (existTab) {
          activeTabKey.value = existTab.action;
        } else {
          tabList.value.push({ id: menuItem.id, title: menuName, action: actionUrl });
          activeTabKey.value = actionUrl;
        }
      }

      // tab切换事件：同步左侧菜单高亮
      async function handleTabChange(tabKey) {
        if (tabKey === 'home') {
          // 首页：清空菜单选中
          activeMenuId.value = '';
          return;
        }
        // 找到当前tab对象
        const foundTab = tabList.value.find(t => t.action === tabKey);
        if (foundTab && foundTab.id != null && foundTab.id != undefined) {
          activeMenuId.value = String(foundTab.id);
        } else {
          activeMenuId.value = '';
        }
        await Vue.nextTick();

        if (elMenuRef.value && typeof elMenuRef.value.updateActiveIndex === 'function') {
          elMenuRef.value.updateActiveIndex(activeMenuId.value);
        }
        window.mymenu = elMenuRef;
        console.log('handleTabChange:', activeMenuId.value, elMenuRef)
      };
      ``

      // tab切换事件：同步左侧菜单高亮
      async function handleTabChange(tabKey) {
        if (tabKey === 'home') {
          // 首页：清空菜单选中
          activeMenuId.value = '';
          return;
        }
        // 找到当前tab对象
        const foundTab = tabList.value.find(t => t.action === tabKey);
        if (foundTab && foundTab.id != null && foundTab.id != undefined) {
          activeMenuId.value = String(foundTab.id);
        } else {
          activeMenuId.value = '';
        }
        await Vue.nextTick();

        if (elMenuRef.value && typeof elMenuRef.value.updateActiveIndex === 'function') {
          elMenuRef.value.updateActiveIndex(activeMenuId.value);
        }
        window.mymenu = elMenuRef;
        console.log('handleTabChange:', activeMenuId.value, elMenuRef)
      };

      // 关闭标签页：禁止删除首页home
      function handleTabRemove(targetKey) {
        // 拦截首页，不允许关闭
        if (targetKey === 'home') return;

        var idx = tabList.value.findIndex(t => t.action === targetKey);
        if (idx === -1) return;
        tabList.value.splice(idx, 1);

        // 如果关闭的是当前激活页，切换到首页
        if (activeTabKey.value === targetKey) {
          activeTabKey.value = 'home';
        }
      }

      // 退出登录
      async function logout() {
        try { await api.post('/api/Sys/Auth/logout'); } catch (err) { console.warn('登出接口异常', err); }
        finally {
          localStorage.removeItem('authToken');
          localStorage.removeItem('userName');
          window.location.href = '/Home/Login';
        }
      }

      // 用户下拉菜单命令处理
      function handleUserCommand(command) {
        if (command === 'changePassword') {
          passwordForm.oldPassword = '';
          passwordForm.newPassword = '';
          passwordForm.confirmPassword = '';
          passwordDialogVisible.value = true;
        } else if (command === 'logout') {
          logout();
        }
      }

      // 提交修改密码
      async function submitPasswordChange() {
        if (!passwordFormRef.value) return;
        passwordFormRef.value.validate(function (valid) {
          if (!valid) return;
          passwordLoading.value = true;
          var oldPwdHashed = CryptoJS.MD5(passwordForm.oldPassword).toString();
          var newPwdHashed = CryptoJS.MD5(passwordForm.newPassword).toString();
          api.post('/api/Sys/Auth/ChangePassword', {
            oldPassword: oldPwdHashed,
            newPassword: newPwdHashed
          })
            .then(function (res) {
              if (res.data.success) {
                ElMessage.success('密码修改成功，请重新登录');
                passwordDialogVisible.value = false;
                setTimeout(function () { logout(); }, 1500);
              } else {
                ElMessage.error(res.data.message || '密码修改失败');
              }
            })
            .catch(function (err) {
              ElMessage.error('密码修改失败');
            })
            .finally(function () {
              passwordLoading.value = false;
            });
        });
      }

      function toggleCollapse() {
        isCollapsed.value = !isCollapsed.value;
      }

      async function loadUserInfo() {
        var res = await siteUtils.getUserInfoAsync();
        console.log('loadUserInfo', res);
        localStorage.setItem('userName', res.user?.name);
        localStorage.setItem('userAccount', res.user?.account);
        userInfo.userName = res.user?.name;
      }

      // ========== 页面挂载初始化 ==========
      Vue.onMounted(function () {
        const initLang = i18nUtils.loadLanguagePreference();
        i18nUtils.switchLanguage(initLang);
        currentLang.value = initLang;
        loadMenus();

        var savedName = localStorage.getItem('userName');
        if (savedName) {
          userInfo.userName = savedName || '未知用户';
        } else {
          loadUserInfo().catch(function (e) {
            console.error('[ERROR] loadUserInfo', e);
          });
        }

        // 监听子页面请求语言
        window.addEventListener('message', (event) => {
          const msg = event.data;
          if (msg.type === 'query-parent-lang') {
            event.source.postMessage({
              type: 'update-child-lang',
              lang: currentLang.value
            }, '*');
          }
        })

        // 暴露全局方法，供iframe子页面调用打开新标签页
        window.openInNewTab = function (title, url) {
          var existTab = tabList.value.find(function (t) {
            return t.action === url;
          });
          if (existTab) {
            activeTabKey.value = existTab.action;
          } else {
            var newTab = { title: title, action: url };
            tabList.value.push(newTab);
            activeTabKey.value = url;
          }
        };

        // 暴露全局方法，供iframe子页面调用关闭当前标签页
        window.closeCurrentTab = function () {
          var currentKey = activeTabKey.value;
          if (currentKey) {
            handleTabRemove(currentKey);
          }
        };
      });

      function consoleLog(msg) {
        console.log('consoleLog:', msg);
      }

      // 对外暴露变量方法
      return {
        userInfo, elMenuRef, menuTree, activeMenuId, tabList, activeTabKey,
        isCollapsed, currentLang, iconMap,
        passwordDialogVisible, passwordLoading, passwordFormRef,
        passwordForm, passwordRules,
        hasChildren, toggleCollapse, handleMenuSelect,
        handleTabRemove, handleTabChange, logout, switchGlobalLang, consoleLog,
        handleUserCommand, submitPasswordChange
      };
    }
  });

  // 业务实际用到的图标白名单，新增图标在这里追加名字（大写）
  const exportIconList = []; // ['Search', 'Edit', 'Delete', 'View'];

  // 只把白名单内图标注册为Vue全局组件
  for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    if (exportIconList.includes(key)) {
      app.component(key, component);
    } else if (exportIconList.length == 0) {
      app.component(key, component);
    }
  }

  app.use(appI18n);
  app.use(ElementPlus, { locale: ElementPlusLocaleZhCn });

  app.mount('#app');
  siteUtils.append(app);
  console.log('主页 Vue 应用初始化完成');
});