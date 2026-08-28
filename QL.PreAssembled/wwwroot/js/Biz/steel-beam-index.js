document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;

    var app = Vue.createApp({
        setup: function () {
            var loading = Vue.ref(false);
            var tableData = Vue.ref([]);
            var total = Vue.ref(0);
            var pageIndex = Vue.ref(1);
            var pageSize = Vue.ref(20);
            var searchProjID = Vue.ref(null);
            var searchBridgeID = Vue.ref(null);
            var projectOptions = Vue.ref([]);
            var bridgeOptions = Vue.ref([]);

            function indexMethod(index) {
                return (pageIndex.value - 1) * pageSize.value + index + 1;
            }

            async function loadProjectOptions() {
                try {
                    var res = await HttpUtils.get('/Biz/SteelBeam/ProjectOptions');
                    projectOptions.value = res || [];
                } catch (e) {
                    console.error('loadProjectOptions error', e);
                    projectOptions.value = [];
                }
            }

            async function onProjectChange(val) {
                searchBridgeID.value = null;
                bridgeOptions.value = [];
                if (!val) return;
                try {
                    var res = await HttpUtils.get('/Biz/SteelBeam/BridgeOptions?projID=' + val);
                    bridgeOptions.value = res || [];
                } catch (e) {
                    console.error('onProjectChange error', e);
                }
            }

            async function loadData() {
                loading.value = true;
                try {
                    var req = {
                        projID: searchProjID.value,
                        bridgeID: searchBridgeID.value,
                        pageIndex: pageIndex.value,
                        pageSize: pageSize.value
                    };
                    var res = await HttpUtils.post('/Biz/SteelBeam/List', req);
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
                searchBridgeID.value = null;
                bridgeOptions.value = [];
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
                window.location.href = '/Biz/SteelBeam/LinearControl?id=' + id;
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
                searchBridgeID: searchBridgeID,
                projectOptions: projectOptions,
                bridgeOptions: bridgeOptions,
                indexMethod: indexMethod,
                onProjectChange: onProjectChange,
                handleSearch: handleSearch,
                handleReset: handleReset,
                handlePageChange: handlePageChange,
                handleSizeChange: handleSizeChange,
                handleLinearControl: handleLinearControl
            };
        }
    });

    app.use(ElementPlus, {
        locale: ElementPlusLocaleZhCn
    });
    app.mount('#app');
});
