document.addEventListener('DOMContentLoaded', function () {
  var Vue = window.Vue, ElementPlus = window.ElementPlus;
  var appI18n = window.appI18n; // 子页面全局i18n实例，同父页配置 legacy: false

  // 中英文切换（与父页面语言同步，含 ElementPlus 内置文案）
  initIframeLangSync({
    ElementPlus: ElementPlus,
    appI18n: appI18n,
    localeEn: ElementPlusLocaleEn,
    localeZh: ElementPlusLocaleZhCn
  });

  var app = Vue.createApp({
    data() { return {
      loading:false, rows:[], total:0, query:{name:'',code:'',status:null,pageIndex:1,pageSize:10},
      formVisible:false, form:{}, rules:{name:[{required:true,message:'',trigger:'blur'}],code:[{required:true,message:'',trigger:'blur'},{pattern:/^[a-zA-Z][a-zA-Z0-9_-]*$/,message:'',trigger:'blur'}]},
      active:{}, menuVisible:false, menuTreeData:[], userVisible:false, users:[], userKeyword:'', userSelection:[]
    }; },
    computed:{ filteredUsers(){var k=this.userKeyword.trim().toLowerCase();if(!k)return this.users;return this.users.filter(x=>[x.name,x.account,x.empNo].some(v=>(v||'').toLowerCase().includes(k)));} },
    mounted(){
      // 表单校验提示绑定多语言
      this.rules.name[0].message = this.$t('validation.required');
      this.rules.code[0].message = this.$t('validation.required');
      this.rules.code[1].message = this.$t('sys.role.codePattern');
      this.load();
    },
    methods:{
      indexNo(i){return (this.query.pageIndex-1)*this.query.pageSize+i+1;},
      async load(){this.loading=true;try{var r=await HttpUtils.post('/Sys/Role/List',this.query);this.rows=r.items||[];this.total=r.total||0;}finally{this.loading=false;}},
      search(){this.query.pageIndex=1;this.load();}, reset(){this.query={name:'',code:'',status:null,pageIndex:1,pageSize:10};this.load();},
      edit(row){this.form=row?JSON.parse(JSON.stringify(row)):{id:0,name:'',code:'',sort:0,remark:'',status:1};this.formVisible=true;this.$nextTick(()=>this.$refs.formRef&&this.$refs.formRef.clearValidate());},
      async save(){await this.$refs.formRef.validate();await HttpUtils.post('/Sys/Role/Save',this.form);ElementPlus.ElMessage.success(this.$t('message.saveSuccess'));this.formVisible=false;this.load();},
      async toggle(row,on){try{await HttpUtils.post('/Sys/Role/SetStatus',{id:row.id,status:on?1:0});row.status=on?1:0;ElementPlus.ElMessage.success(this.$t('sys.role.statusUpdated'));}catch(e){this.load();}},
      async remove(row){var ok=await ElementPlus.ElMessageBox.confirm(this.$t('sys.role.confirmDelete',{name:row.name,count:row.userCount}),this.$t('sys.role.deleteConfirmTitle'),{type:'warning'}).catch(()=>false);if(!ok)return;await HttpUtils.post('/Sys/Role/Delete',{id:row.id});ElementPlus.ElMessage.success(this.$t('message.deleteSuccess'));this.load();},
      tree(list){var map={},roots=[];list.forEach(x=>map[x.id]={...x,children:[]});list.forEach(x=>{var n=map[x.id];if(x.parentID&&map[x.parentID])map[x.parentID].children.push(n);else roots.push(n);});return roots;},
      // 菜单动态多语言组装胶水函数（与 home-index.js loadMenus 后的处理方案一致）
      mergeMenuToI18n(menuFlatList){if(!menuFlatList||!Array.isArray(menuFlatList)||menuFlatList.length===0){return;}var objZh={},objEn={};for(const item of menuFlatList){if(item.id==null)continue;var key=`menu_${item.id}`;objZh[key]=item.name||"";objEn[key]=item.enName||item.name||"";}i18nUtils.mergeToI18n(objZh,objEn,"dyn.menu");},
      // 树节点label：优先取 dyn.menu 动态翻译，key缺失时回退原名称
      menuNodeLabel(data) { var k = 'dyn.menu.menu_' + data.id; var t = this.$t(k); console.log('menuNodeLabel:', k, t); return t===k?(data.name||''):t;},
      async openMenus(row){this.active=row;var list=await HttpUtils.post('/Sys/Role/Menus',{id:row.id});try{this.mergeMenuToI18n(list);}catch(e){console.log('mergeMenuToI18n:error',e);}this.menuTreeData=this.tree(list);this.menuVisible=true;this.$nextTick(()=>{this.$refs.menuTree.setCheckedKeys([]);list.filter(x=>x.selected).forEach(x=>this.$refs.menuTree.setChecked(x.id,true,false));});},
      async saveMenus(){var t=this.$refs.menuTree,ids=[...new Set(t.getCheckedKeys(false).concat(t.getHalfCheckedKeys()))];await HttpUtils.post('/Sys/Role/SaveMenus',{id:this.active.id,menuIDs:ids});ElementPlus.ElMessage.success(this.$t('message.saveSuccess'));this.menuVisible=false;},
      async openUsers(row){this.active=row;this.userKeyword='';this.users=await HttpUtils.post('/Sys/Role/Users',{id:row.id});this.userVisible=true;this.$nextTick(()=>{this.$refs.userTable.clearSelection();this.users.filter(x=>x.selected).forEach(x=>this.$refs.userTable.toggleRowSelection(x,true));});},
      async saveUsers(){await HttpUtils.post('/Sys/Role/SaveUsers',{id:this.active.id,userIDs:this.userSelection.map(x=>x.id)});ElementPlus.ElMessage.success(this.$t('message.saveSuccess'));this.userVisible=false;this.load();}
    }
  });
    app.use(window.appI18n); app.use(ElementPlus, { locale: window.ElementPlusLocaleZhCn });
    if (window.ElementPlusIconsVue) {
        for (var key in window.ElementPlusIconsVue) {
            if (window.ElementPlusIconsVue[key]) {
                app.component(key, window.ElementPlusIconsVue[key]);
            }
        }
    }
    app.mount('#app');
});
