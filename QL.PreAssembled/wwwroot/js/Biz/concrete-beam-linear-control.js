document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var groupID = window.__groupID || 0;

    var app = Vue.createApp({
        setup: function () {
            var loading = Vue.ref(false);
            var activeTab = Vue.ref('projectInfo');

            var tabs = Vue.reactive([
                { name: 'projectInfo', label: '项目信息', disabled: false },
                { name: 'designCoord', label: '设计坐标', disabled: false },
                { name: 'preCamber', label: '预拱度', disabled: false },
                { name: 'stationMeasure', label: '设站及测量', disabled: false },
                { name: 'pouringCoord', label: '浇筑坐标', disabled: false },
                { name: 'report', label: '报告', disabled: false }
            ]);

            // 项目信息表单
            var projectInfo = Vue.reactive({
                projectName: '',
                castingGroupName: '',
                managerName: '',
                offsetA: '',
                offsetB: '',
                offsetC: '',
                referenceElevation: '',
                createdTime: '',
                updatedTime: ''
            });

            // 梁段表格
            var segmentTableData = Vue.ref([]);
            var segmentTotal = Vue.ref(0);
            var segmentPageIndex = Vue.ref(1);
            var segmentPageSize = Vue.ref(10);

            // 设计坐标表格
            var designLoading = Vue.ref(false);
            var designTableData = Vue.ref([]);
            var designTotal = Vue.ref(0);
            var designPageIndex = Vue.ref(1);
            var designPageSize = Vue.ref(10);
            var designNormalize = Vue.ref(true);
            var designIncludeCamber = Vue.ref(false);

            // 预拱度表格
            var camberLoading = Vue.ref(false);
            var camberTableData = Vue.ref([]);
            var camberTotal = Vue.ref(0);
            var camberPageIndex = Vue.ref(1);
            var camberPageSize = Vue.ref(10);

            // 设站及测量
            var stationSegmentType = Vue.ref('起始段');
            var stationPouringDate = Vue.ref(new Date());
            var stationMatchPouring = Vue.ref('KF-P2-0U');
            var stationAccuracyOk = Vue.ref(true);

            // 起始段-上部设置参数(活动端模板,只读)
            var stationStartTemplate = Vue.reactive({
                eastElevation: '1.0000', eastLength: '2.6896',
                westElevation: '1.0000', westLength: '2.6896',
                centerOffset: '-0.0001'
            });
            // 起始段-实测活动端模板
            var stationStartActiveMeasure = Vue.reactive({
                eastElevation: '1.00002', westElevation: '0.99999', centerOffset: '0'
            });
            // 起始段-实测固定端模板
            var stationStartFixedMeasure = Vue.reactive({
                eastElevation: '1.00002', westElevation: '0.99999', centerOffset: '0'
            });
            // 起始段-实测浇筑梁段
            var stationStartPouringMeasure = Vue.reactive({
                neElevation: '1.0013', nwElevation: '1.0102',
                seElevation: '1.0032', swElevation: '1.0111',
                northOffset: '0.2', southOffset: '0.2',
                eastLength: '0.2', westLength: '0.2'
            });

            // 标准段-上部设置参数(匹配梁段设置参数,只读)
            var stationStandardParams = Vue.reactive({
                neElevation: '1.0000', nwElevation: '1.0000',
                seElevation: '-0.0001', swElevation: '-0.0001',
                northOffset: '2.6896', southOffset: '2.6896',
                segmentLength: '2.6896'
            });
            // 标准段-实测匹配梁段
            var stationStandardMatchMeasure = Vue.reactive({
                neElevation: '1.0000', nwElevation: '1.0000',
                seElevation: '-0.0001', swElevation: '-0.0001',
                northOffset: '2.6896', southOffset: '2.6896'
            });
            // 标准段-实测浇筑梁段
            var stationStandardPouringMeasure = Vue.reactive({
                neElevation: '1.0000', nwElevation: '1.0000',
                seElevation: '-0.0001', swElevation: '-0.0001',
                northOffset: '2.6896', southOffset: '2.6896',
                segmentLength: '2.6896'
            });
            // 标准段-实测固定端模板
            var stationStandardFixedMeasure = Vue.reactive({
                eastElevation: '1.00002', westElevation: '0.99999', centerOffset: '0'
            });
            // 标准段-匹配浇筑扭转
            var stationMatchTorsion = Vue.ref('1.00002');

            // 测点偏位(共用)
            var stationDeviation = Vue.reactive({
                northA: '0.2', northC: '0.2',
                southA: '0.2', southC: '0.2'
            });

            // ========== 浇筑坐标页签 ==========
            var pouringSegmentType = Vue.ref('标准段');
            var pouringPouringDate = Vue.ref(new Date());
            var pouringMatchPouring = Vue.ref('KF-P2-0U');
            // 端部节点理论坐标
            var pouringTheoreticalCoords = Vue.reactive({
                west: { x: '713033.4493', y: '666803.5407', z: '11.5339' },
                center: { x: '713034.5986', y: '666805.5869', z: '11.4126' },
                east: { x: '713035.7480', y: '666807.6330', z: '11.2912' }
            });
            // 端部节点浇筑坐标
            var pouringActualCoords = Vue.reactive({
                west: { x: '713033.4493', y: '666803.5407', z: '11.5339' },
                center: { x: '713034.5986', y: '666805.5869', z: '11.4126' },
                east: { x: '713035.7480', y: '666807.6330', z: '11.2912' }
            });
            // 高程测点
            var pouringElevationPoints = Vue.reactive({
                ne: { x: '713033.4493', y: '666803.5407', z: '11.5339' },
                nw: { x: '713034.5986', y: '666805.5869', z: '11.4126' },
                se: { x: '713035.7480', y: '666807.6330', z: '11.2912' },
                sw: { x: '713035.7480', y: '666807.6330', z: '11.2912' }
            });
            // 轴线测点
            var pouringAxisPoints = Vue.reactive({
                north: { x: '713033.4493', y: '666803.5407', z: '11.5339' },
                south: { x: '713034.5986', y: '666805.5869', z: '11.4126' }
            });
            // 中线长度
            var pouringCenterLine = Vue.reactive({
                theoreticalValue: '713033.4493',
                actualValue: '713034.5986',
                theoreticalTotal: '713033.4493',
                actualTotal: '713034.5986'
            });

            function onTabChange(tabName) {
                var tab = tabs.find(function (t) { return t.name === tabName; });
                if (tab && tab.disabled) return;
                activeTab.value = tabName;
            }

            async function loadGroupInfo() {
                try {
                    var res = await axios.get('/Biz/ConcreteBeam/CastingGroupInfo?id=' + groupID);
                    var data = res.data;
                    if (data && data.code === 200 && data.data) {
                        var info = data.data;
                        projectInfo.projectName = info.projectName || '';
                        projectInfo.castingGroupName = info.castingGroupName || '';
                        projectInfo.managerName = info.managerName || '';
                    } else {
                        console.warn('No group info data');
                    }
                } catch (e) {
                    console.error('loadGroupInfo error', e);
                }
            }

            function loadSegmentData() {
                loading.value = true;
                var list = [];
                for (var i = 0; i < 10; i++) {
                    list.push({
                        segmentName: 'KF-P2-1U',
                        downstream: '',
                        upstream: '',
                        node1: '1',
                        node2: '2',
                        pouringDate: '2019-02-19'
                    });
                }
                segmentTableData.value = list;
                segmentTotal.value = 50;
                loading.value = false;
            }

            function handleSegmentPageChange(val) {
                segmentPageIndex.value = val;
                loadSegmentData();
            }

            function handleSegmentSizeChange(val) {
                segmentPageSize.value = val;
                segmentPageIndex.value = 1;
                loadSegmentData();
            }

            function handleImportSegment() {
                ElMessage.info('导入梁段信息功能待开发');
            }

            function loadDesignData() {
                designLoading.value = true;
                var list = [];
                for (var i = 0; i < 10; i++) {
                    list.push({
                        nodeName: 'KF-P2-1U',
                        xe: '713038.093088', ye: '666806.315995', ze: '11.290768',
                        xc: '713038.093088', yc: '666806.315995', zc: '11.290768',
                        xw: '713038.093088', yw: '666806.315995', zw: '11.290768'
                    });
                }
                designTableData.value = list;
                designTotal.value = 50;
                designLoading.value = false;
            }

            function handleDesignPageChange(val) { designPageIndex.value = val; loadDesignData(); }
            function handleDesignSizeChange(val) { designPageSize.value = val; designPageIndex.value = 1; loadDesignData(); }
            function onDesignNormalizeChange() { loadDesignData(); }
            function onDesignCamberChange() {
                var preCamberTab = tabs.find(function (t) { return t.name === 'preCamber'; });
                if (preCamberTab) {
                    preCamberTab.disabled = designIncludeCamber.value;
                    if (preCamberTab.disabled && activeTab.value === 'preCamber') {
                        activeTab.value = 'designCoord';
                    }
                }
                loadDesignData();
            }
            function handleImportDesignCoord() { ElMessage.info('导入设计坐标功能待开发'); }
            function handleDesignPrint() { ElMessage.info('打印功能待开发'); }
            function handleDesignPreview() { ElMessage.info('预览功能待开发'); }
            function handleDesignConfirm() { ElMessage.info('确定功能待开发'); }
            function handleDesignCancel() { ElMessage.info('已取消'); }

            function loadCamberData() {
                camberLoading.value = true;
                var list = [];
                for (var i = 0; i < 10; i++) {
                    list.push({
                        nodeName: 'KF-P2-1U',
                        ze: '713038.093088',
                        zc: '713038.093088',
                        zw: '713038.093088'
                    });
                }
                camberTableData.value = list;
                camberTotal.value = 50;
                camberLoading.value = false;
            }

            function handleCamberPageChange(val) { camberPageIndex.value = val; loadCamberData(); }
            function handleCamberSizeChange(val) { camberPageSize.value = val; camberPageIndex.value = 1; loadCamberData(); }
            function handleImportCamber() { ElMessage.info('导入预拱度功能待开发'); }
            function handleCamberPrint() { ElMessage.info('打印功能待开发'); }
            function handleCamberPreview() { ElMessage.info('预览功能待开发'); }
            function handleCamberConfirm() { ElMessage.info('确定功能待开发'); }
            function handleCamberCancel() { ElMessage.info('已取消'); }

            function onStationSegmentChange() { ElMessage.info('切换梁段：' + stationSegmentType.value); }
            function handleStationPrint() { ElMessage.info('打印功能待开发'); }
            function handleStationConfirm() { ElMessage.info('确定功能待开发'); }
            function handleStationCancel() { ElMessage.info('已取消'); }

            function onPouringSegmentChange() { ElMessage.info('切换梁段：' + pouringSegmentType.value); }
            function handlePouringPrint() { ElMessage.info('打印功能待开发'); }
            function handlePouringPreview() { ElMessage.info('预览功能待开发'); }
            function handlePouringCancel() { ElMessage.info('已取消'); }

            // ========== 报告页签 ==========
            var reportOverallType = Vue.ref('理论坐标');
            var reportIncludeAll = Vue.ref(true);
            var reportSegmentName = Vue.ref('起始段');
            var reportSegmentType = Vue.ref('已浇筑梁段');

            function onReportSegmentChange() { ElMessage.info('切换梁段：' + reportSegmentName.value); }
            function handleReportPrint() { ElMessage.info('打印功能待开发'); }
            function handleReportPreview() { ElMessage.info('预览功能待开发'); }
            function handleReportExport() { ElMessage.info('导出功能待开发'); }
            function handleReportCancel() { ElMessage.info('已取消'); }

            function handleConfirm() {
                if (!projectInfo.offsetA) { ElMessage.warning('请输入A值'); return; }
                if (!projectInfo.offsetB) { ElMessage.warning('请输入B值'); return; }
                if (!projectInfo.offsetC) { ElMessage.warning('请输入C值'); return; }
                if (!projectInfo.referenceElevation) { ElMessage.warning('请输入参考高程'); return; }
                ElMessage.info('确定功能待开发');
            }

            function handleCancel() {
                projectInfo.offsetA = '';
                projectInfo.offsetB = '';
                projectInfo.offsetC = '';
                projectInfo.referenceElevation = '';
            }

            function handleReset() {
                projectInfo.offsetA = '';
                projectInfo.offsetB = '';
                projectInfo.offsetC = '';
                projectInfo.referenceElevation = '';
            }

            Vue.onMounted(function () {
                loadGroupInfo();
                loadSegmentData();
                loadCamberData();
            });

            return {
                loading: loading,
                activeTab: activeTab,
                tabs: tabs,
                projectInfo: projectInfo,
                segmentTableData: segmentTableData,
                segmentTotal: segmentTotal,
                segmentPageIndex: segmentPageIndex,
                segmentPageSize: segmentPageSize,
                onTabChange: onTabChange,
                handleSegmentPageChange: handleSegmentPageChange,
                handleSegmentSizeChange: handleSegmentSizeChange,
                handleImportSegment: handleImportSegment,
                designLoading: designLoading,
                designTableData: designTableData,
                designTotal: designTotal,
                designPageIndex: designPageIndex,
                designPageSize: designPageSize,
                designNormalize: designNormalize,
                designIncludeCamber: designIncludeCamber,
                handleDesignPageChange: handleDesignPageChange,
                handleDesignSizeChange: handleDesignSizeChange,
                onDesignNormalizeChange: onDesignNormalizeChange,
                onDesignCamberChange: onDesignCamberChange,
                handleImportDesignCoord: handleImportDesignCoord,
                handleDesignPrint: handleDesignPrint,
                handleDesignPreview: handleDesignPreview,
                handleDesignConfirm: handleDesignConfirm,
                handleDesignCancel: handleDesignCancel,
                camberLoading: camberLoading,
                camberTableData: camberTableData,
                camberTotal: camberTotal,
                camberPageIndex: camberPageIndex,
                camberPageSize: camberPageSize,
                handleCamberPageChange: handleCamberPageChange,
                handleCamberSizeChange: handleCamberSizeChange,
                handleImportCamber: handleImportCamber,
                handleCamberPrint: handleCamberPrint,
                handleCamberPreview: handleCamberPreview,
                handleCamberConfirm: handleCamberConfirm,
                handleCamberCancel: handleCamberCancel,
                stationSegmentType: stationSegmentType,
                stationPouringDate: stationPouringDate,
                stationMatchPouring: stationMatchPouring,
                stationAccuracyOk: stationAccuracyOk,
                stationStartTemplate: stationStartTemplate,
                stationStartActiveMeasure: stationStartActiveMeasure,
                stationStartFixedMeasure: stationStartFixedMeasure,
                stationStartPouringMeasure: stationStartPouringMeasure,
                stationStandardParams: stationStandardParams,
                stationStandardMatchMeasure: stationStandardMatchMeasure,
                stationStandardPouringMeasure: stationStandardPouringMeasure,
                stationStandardFixedMeasure: stationStandardFixedMeasure,
                stationMatchTorsion: stationMatchTorsion,
                stationDeviation: stationDeviation,
                onStationSegmentChange: onStationSegmentChange,
                handleStationPrint: handleStationPrint,
                handleStationConfirm: handleStationConfirm,
                handleStationCancel: handleStationCancel,
                pouringSegmentType: pouringSegmentType,
                pouringPouringDate: pouringPouringDate,
                pouringMatchPouring: pouringMatchPouring,
                pouringTheoreticalCoords: pouringTheoreticalCoords,
                pouringActualCoords: pouringActualCoords,
                pouringElevationPoints: pouringElevationPoints,
                pouringAxisPoints: pouringAxisPoints,
                pouringCenterLine: pouringCenterLine,
                onPouringSegmentChange: onPouringSegmentChange,
                handlePouringPrint: handlePouringPrint,
                handlePouringPreview: handlePouringPreview,
                handlePouringCancel: handlePouringCancel,
                reportOverallType: reportOverallType,
                reportIncludeAll: reportIncludeAll,
                reportSegmentName: reportSegmentName,
                reportSegmentType: reportSegmentType,
                onReportSegmentChange: onReportSegmentChange,
                handleReportPrint: handleReportPrint,
                handleReportPreview: handleReportPreview,
                handleReportExport: handleReportExport,
                handleReportCancel: handleReportCancel,
                handleConfirm: handleConfirm,
                handleCancel: handleCancel,
                handleReset: handleReset
            };
        }
    });

    try {
        var locale = typeof ElementPlusLocaleZhCn !== 'undefined' ? ElementPlusLocaleZhCn : undefined;
        app.use(ElementPlus, { locale: locale });
        app.mount('#app');
    } catch (e) {
        console.error('Mount error:', e);
    }
});
