document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var appI18n = window.appI18n;
    // 中英文切换（与父页面语言同步，含 ElementPlus 内置文案）
    initIframeLangSync({
        ElementPlus: ElementPlus,
        appI18n: appI18n,
        localeEn: ElementPlusLocaleEn,
        localeZh: ElementPlusLocaleZhCn
    });
    var app = Vue.createApp({
        setup: function () {
            var loading = Vue.ref(false);
            var tableData = Vue.ref([]);
            var total = Vue.ref(0);
            var pageIndex = Vue.ref(1);
            var pageSize = Vue.ref(10);
            var searchProjID = Vue.ref(null);
            var searchCastingGroupName = Vue.ref('');
            var projectOptions = Vue.ref([]);

            function indexMethod(index) {
                return (pageIndex.value - 1) * pageSize.value + index + 1;
            }

            async function loadProjectOptions() {
                try {
                    var res = await HttpUtils.get('/Biz/ConcreteBeam/ProjectOptions');
                    projectOptions.value = res || [];
                } catch (e) {
                    console.error('loadProjectOptions error', e);
                    projectOptions.value = [];
                }
            }

            async function loadData() {
                loading.value = true;
                try {
                    var req = {
                        projID: searchProjID.value,
                        castingGroupName: searchCastingGroupName.value,
                        pageIndex: pageIndex.value,
                        pageSize: pageSize.value
                    };
                    var res = await HttpUtils.post('/Biz/ConcreteBeam/List', req);
                    tableData.value = (res && res.list) || [];
                    total.value = (res && res.total) || 0;
                } catch (e) {
                    console.error('loadData error', e);
                } finally {
                    loading.value = false;
                }
            }

            function handleSearch() {
                pageIndex.value = 1;
                loadData();
            }

            function handleReset() {
                searchProjID.value = null;
                searchCastingGroupName.value = '';
                pageIndex.value = 1;
                loadData();
            }

            function handlePageChange(val) {
                pageIndex.value = val;
                loadData();
            }

            function handleSizeChange(val) {
                pageSize.value = val;
                pageIndex.value = 1;
                loadData();
            }

            function handleLinearControl(row) {
                var id = row.id || row.ID;
                var name = row.castingGroupName || row.CastingGroupName || i18nUtils.$t('biz.concreteBeam.linearControl');
                var url = '/Biz/ConcreteBeam/LinearControl?id=' + id;
                if (window.parent && window.parent.openInNewTab) {
                    window.parent.openInNewTab(name, url);
                } else {
                    window.location.href = url;
                }
            }

            Vue.onMounted(function () {
                loadProjectOptions();
                loadData();
            });

            return {
                loading: loading,
                tableData: tableData,
                total: total,
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchProjID: searchProjID,
                searchCastingGroupName: searchCastingGroupName,
                projectOptions: projectOptions,
                indexMethod: indexMethod,
                handleSearch: handleSearch,
                handleReset: handleReset,
                handlePageChange: handlePageChange,
                handleSizeChange: handleSizeChange,
                handleLinearControl: handleLinearControl
            };
        }
    });

    // 注册 Vue I18n
    app.use(appI18n);
    
    app.use(ElementPlus, {
        locale: ElementPlusLocaleZhCn
    });

    if (window.ElementPlusIconsVue) {
        for (var key in window.ElementPlusIconsVue) {
            if (window.ElementPlusIconsVue[key]) {
                app.component(key, window.ElementPlusIconsVue[key]);
            }
        }
    }

    app.mount('#app');
});
