document.addEventListener('DOMContentLoaded', function () {
  var Vue = window.Vue, ElementPlus = window.ElementPlus;
  var app = Vue.createApp({
    data() { return {
      loading:false, rows:[], total:0, query:{name:'',code:'',status:null,pageIndex:1,pageSize:10},
      formVisible:false, form:{}, rules:{name:[{required:true,message:'请输入角色名称',trigger:'blur'}],code:[{required:true,message:'请输入角色标识',trigger:'blur'},{pattern:/^[a-zA-Z][a-zA-Z0-9_-]*$/,message:'仅支持字母、数字、下划线和连字符',trigger:'blur'}]},
      active:{}, menuVisible:false, menuTreeData:[], userVisible:false, users:[], userKeyword:'', userSelection:[]
    }; },
    computed:{ filteredUsers(){var k=this.userKeyword.trim().toLowerCase();if(!k)return this.users;return this.users.filter(x=>[x.name,x.account,x.empNo].some(v=>(v||'').toLowerCase().includes(k)));} },
    mounted(){ this.load(); },
    methods:{
      indexNo(i){return (this.query.pageIndex-1)*this.query.pageSize+i+1;},
      async load(){this.loading=true;try{var r=await HttpUtils.post('/Sys/Role/List',this.query);this.rows=r.items||[];this.total=r.total||0;}finally{this.loading=false;}},
      search(){this.query.pageIndex=1;this.load();}, reset(){this.query={name:'',code:'',status:null,pageIndex:1,pageSize:10};this.load();},
      edit(row){this.form=row?JSON.parse(JSON.stringify(row)):{id:0,name:'',code:'',sort:0,remark:'',status:1};this.formVisible=true;this.$nextTick(()=>this.$refs.formRef&&this.$refs.formRef.clearValidate());},
      async save(){await this.$refs.formRef.validate();await HttpUtils.post('/Sys/Role/Save',this.form);ElementPlus.ElMessage.success('保存成功');this.formVisible=false;this.load();},
      async toggle(row,on){try{await HttpUtils.post('/Sys/Role/SetStatus',{id:row.id,status:on?1:0});row.status=on?1:0;ElementPlus.ElMessage.success('状态已更新');}catch(e){this.load();}},
      async remove(row){var ok=await ElementPlus.ElMessageBox.confirm(`删除角色【${row.name}】后，${row.userCount}名关联用户将立即失去该角色权限，是否继续？`,'删除确认',{type:'warning'}).catch(()=>false);if(!ok)return;await HttpUtils.post('/Sys/Role/Delete',{id:row.id});ElementPlus.ElMessage.success('删除成功');this.load();},
      tree(list){var map={},roots=[];list.forEach(x=>map[x.id]={...x,children:[]});list.forEach(x=>{var n=map[x.id];if(x.parentID&&map[x.parentID])map[x.parentID].children.push(n);else roots.push(n);});return roots;},
      async openMenus(row){this.active=row;var list=await HttpUtils.post('/Sys/Role/Menus',{id:row.id});this.menuTreeData=this.tree(list);this.menuVisible=true;this.$nextTick(()=>{this.$refs.menuTree.setCheckedKeys([]);list.filter(x=>x.selected).forEach(x=>this.$refs.menuTree.setChecked(x.id,true,false));});},
      async saveMenus(){var t=this.$refs.menuTree,ids=[...new Set(t.getCheckedKeys(false).concat(t.getHalfCheckedKeys()))];await HttpUtils.post('/Sys/Role/SaveMenus',{id:this.active.id,menuIDs:ids});ElementPlus.ElMessage.success('菜单权限已保存');this.menuVisible=false;},
      async openUsers(row){this.active=row;this.userKeyword='';this.users=await HttpUtils.post('/Sys/Role/Users',{id:row.id});this.userVisible=true;this.$nextTick(()=>{this.$refs.userTable.clearSelection();this.users.filter(x=>x.selected).forEach(x=>this.$refs.userTable.toggleRowSelection(x,true));});},
      async saveUsers(){await HttpUtils.post('/Sys/Role/SaveUsers',{id:this.active.id,userIDs:this.userSelection.map(x=>x.id)});ElementPlus.ElMessage.success('用户关联已保存');this.userVisible=false;this.load();}
    }
  });
  app.use(window.appI18n); app.use(ElementPlus,{locale:window.ElementPlusLocaleZhCn}); app.mount('#app');
});
