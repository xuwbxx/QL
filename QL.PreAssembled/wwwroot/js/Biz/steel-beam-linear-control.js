document.addEventListener('DOMContentLoaded', function () {
    console.log('[LinearControl] DOMContentLoaded, starting init...');
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;

    if (!Vue) { console.error('[LinearControl] Vue not loaded!'); return; }
    if (!ElementPlus) { console.error('[LinearControl] ElementPlus not loaded!'); return; }

    var ElMessage = ElementPlus.ElMessage;

    var params = new URLSearchParams(window.location.search);
    var bridgeID = params.get('id') || 0;

    var app = Vue.createApp({
        setup: function () {
            var tabs = [
                { name: 'theoretical', label: '理论数据' },
                { name: 'measured', label: '实测数据' },
                { name: 'monitoring', label: '监控计算' },
                { name: 'analysis', label: '误差分析' }
            ];

            var pageTitle = Vue.ref('加载中...');
            var activeTab = Vue.ref('theoretical');
            var loading = Vue.ref(false);
            var tableData = Vue.ref([]);
            var total = Vue.ref(0);
            var pageIndex = Vue.ref(1);
            var pageSize = Vue.ref(10);
            var searchPointCode = Vue.ref(null);
            var searchSegment = Vue.ref(null);
            var searchPushCount = Vue.ref(null);
            var searchMeasureTime = Vue.ref(null);
            var pointOptions = Vue.ref([]);
            var segmentOptions = Vue.ref([]);
            var pushCountOptions = Vue.ref([0, 1, 2, 3]);
            var importDialogVisible = Vue.ref(false);
            var importLoading = Vue.ref(false);
            var importFile = Vue.ref(null);
            var importDialogTitle = Vue.ref('导入理论数据');
            var importPushCount = Vue.ref(null);
            var importMeasureTime = Vue.ref(null);

            function goBack() {
                window.history.back();
            }

            async function loadBridgeInfo() {
                try {
                    var resp = await axios.get('/Biz/SteelBeam/BridgeInfo?id=' + bridgeID);
                    var res = resp.data;
                    if (res && res.code === 200 && res.data) {
                        pageTitle.value = (res.data.projectName || '') + ' - ' + (res.data.bridgeName || '');
                    } else {
                        pageTitle.value = '线性管控';
                    }
                } catch (e) {
                    console.error('loadBridgeInfo error', e);
                    pageTitle.value = '线性管控';
                }
            }

            function generateMockData(tab) {
                var data = [];
                // var segments = ['A', 'B', 'C', 'D', 'E', 'F'];
                // var positions = ['测位1', '测位2', '测位3'];

                // for (var i = 0; i < 6; i++) {
                //     var row = {
                //         pointCode: 'GS-Z-1-' + (i + 1),
                //         segmentNo: segments[i],
                //         positionName: positions[i % positions.length],
                //         weight: 1
                //     };

                //     if (tab === 'theoretical') {
                //         row.designX = (12.55 + i * 0.1).toFixed(3);
                //         row.designY = (5.2 + i * 0.1).toFixed(3);
                //         row.designZ = (8.0 + i * 0.05).toFixed(3);
                //         row.preCamber = (568 - i * 2);
                //     } else if (tab === 'measured') {
                //         row.pointCode = 'GS-Z-1-4';
                //         row.measuredX = '12.55';
                //         row.measuredY = '12.55';
                //         row.measuredZ = '12.55';
                //     } else if (tab === 'monitoring') {
                //         row.designX = (12.55 + i * 0.1).toFixed(3);
                //         row.designY = (5.2 + i * 0.1).toFixed(3);
                //         row.designZ = (8.0 + i * 0.05).toFixed(3);
                //         row.measuredX = (12.552 + i * 0.1).toFixed(3);
                //         row.measuredY = (5.198 + i * 0.1).toFixed(3);
                //         row.measuredZ = (8.005 + i * 0.05).toFixed(3);
                //         row.deltaX = (2.0).toFixed(1);
                //         row.deltaY = (-2.0).toFixed(1);
                //         row.deltaZ = (0.5).toFixed(1);
                //     } else if (tab === 'analysis') {
                //         row.deltaX = (2.0).toFixed(1);
                //         row.deltaY = (-2.0).toFixed(1);
                //         row.deltaZ = (0.5).toFixed(1);
                //         row.tolerance = 5.0;
                //         row.qualified = (Math.abs(2.0) <= 5.0 && Math.abs(-2.0) <= 5.0 && Math.abs(0.5) <= 5.0);
                //     }

                //     data.push(row);
                // }

                // if (segmentOptions.value.length === 0) {
                //     segments.forEach(function (s) { segmentOptions.value.push(s); });
                // }
                // if (pointOptions.value.length === 0) {
                //     for (var j = 1; j <= 6; j++) {
                //         pointOptions.value.push('GS-Z-1-' + j);
                //     }
                // }

                return data;
            }

            function loadData() {
                loading.value = true;
                setTimeout(function () {
                    tableData.value = generateMockData(activeTab.value);
                    total.value = 50;
                    loading.value = false;
                }, 300);
            }

            function onTabChange(tabName) {
                activeTab.value = tabName;
                pageIndex.value = 1;
                searchPointCode.value = null;
                searchSegment.value = null;
                searchPushCount.value = null;
                searchMeasureTime.value = null;
                loadData();
            }

            function handleSearch() {
                pageIndex.value = 1;
                loadData();
            }

            function handleReset() {
                searchPointCode.value = null;
                searchSegment.value = null;
                searchPushCount.value = null;
                searchMeasureTime.value = null;
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

            function handleImport() {
                importFile.value = null;
                importPushCount.value = null;
                importMeasureTime.value = null;
                importDialogTitle.value = activeTab.value === 'measured' ? '导入实测数据' : '导入理论数据';
                importDialogVisible.value = true;
            }

            function onFileChange(file) {
                importFile.value = file;
            }

            function onFileExceed() {
                ElMessage.warning('只能上传一个文件，请先移除已选文件');
            }

            function confirmImport() {
                if (!importFile.value || !importFile.value.raw) {
                    ElMessage.warning('请先选择要导入的文件');
                    return;
                }
                if (activeTab.value === 'measured') {
                    if (importPushCount.value === null || importPushCount.value === undefined) {
                        ElMessage.warning('请选择顶推次数');
                        return;
                    }
                    if (!importMeasureTime.value) {
                        ElMessage.warning('请选择测量时间');
                        return;
                    }
                }
                importLoading.value = true;
                setTimeout(function () {
                    importLoading.value = false;
                    importDialogVisible.value = false;
                    ElMessage.success('导入成功');
                    loadData();
                }, 1500);
            }

            function handleCalc() {
                ElMessage.info('执行计算功能开发中');
            }

            function handleExport() {
                ElMessage.info('导出报告功能开发中');
            }

            function handleDownloadMeasured() {
                ElMessage.info('下载实测数据功能开发中');
            }

            function handleDownloadTemplate() {
                ElMessage.info('下载模板功能开发中');
            }

            function handleEdit(row) {
                ElMessage.info('编辑功能开发中');
            }

            Vue.onMounted(function () {
                loadBridgeInfo().catch(function () { });
                loadData();
            });

            return {
                tabs: tabs,
                pageTitle: pageTitle,
                activeTab: activeTab,
                loading: loading,
                tableData: tableData,
                total: total,
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchPointCode: searchPointCode,
                searchSegment: searchSegment,
                pointOptions: pointOptions,
                segmentOptions: segmentOptions,
                pushCountOptions: pushCountOptions,
                searchPushCount: searchPushCount,
                searchMeasureTime: searchMeasureTime,
                goBack: goBack,
                onTabChange: onTabChange,
                handleSearch: handleSearch,
                handleReset: handleReset,
                handlePageChange: handlePageChange,
                handleSizeChange: handleSizeChange,
                handleImport: handleImport,
                handleCalc: handleCalc,
                handleExport: handleExport,
                handleEdit: handleEdit,
                handleDownloadMeasured: handleDownloadMeasured,
                handleDownloadTemplate: handleDownloadTemplate,
                importDialogVisible: importDialogVisible,
                importLoading: importLoading,
                importFile: importFile,
                importDialogTitle: importDialogTitle,
                importPushCount: importPushCount,
                importMeasureTime: importMeasureTime,
                onFileChange: onFileChange,
                onFileExceed: onFileExceed,
                confirmImport: confirmImport
            };
        }
    });

    try {
        var locale = typeof ElementPlusLocaleZhCn !== 'undefined' ? ElementPlusLocaleZhCn : undefined;
        app.use(ElementPlus, { locale: locale });
        if (typeof ElementPlusIconsVue !== 'undefined' && ElementPlusIconsVue.UploadFilled) {
            app.component('UploadFilled', ElementPlusIconsVue.UploadFilled);
        }
        app.mount('#app');
        console.log('[LinearControl] Vue app mounted successfully');
    } catch (e) {
        console.error('[LinearControl] Mount error:', e);
    }
});
