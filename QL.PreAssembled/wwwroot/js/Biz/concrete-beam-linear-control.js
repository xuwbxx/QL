document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var ElMessageBox = ElementPlus.ElMessageBox;
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
            var savedConfig = { offsetA: '', offsetB: '', offsetC: '', referenceElevation: '' };
            var importDialogVisible = Vue.ref(false);
            var importLoading = Vue.ref(false);
            var importType = Vue.ref('segments');
            var importFile = Vue.ref(null);
            var uploadRef = Vue.ref(null);

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
                    var info = await HttpUtils.get('/Biz/ConcreteBeam/Config', { id: groupID });
                    projectInfo.projectName = info.projectName || '';
                    projectInfo.castingGroupName = info.castingGroupName || '';
                    projectInfo.managerName = info.managerName || '';
                    projectInfo.offsetA = formatNullableNumber(info.offsetA);
                    projectInfo.offsetB = formatNullableNumber(info.offsetB);
                    projectInfo.offsetC = formatNullableNumber(info.offsetC);
                    projectInfo.referenceElevation = formatNullableNumber(info.referenceElevation);
                    projectInfo.createdTime = formatDateTime(info.createdTime);
                    projectInfo.updatedTime = formatDateTime(info.updatedTime);
                    rememberConfig();
                } catch (e) {
                    console.error('loadGroupInfo error', e);
                }
            }

            async function loadSegmentData() {
                loading.value = true;
                try {
                    var res = await HttpUtils.post('/Biz/ConcreteBeam/SegmentList', {
                        castingGroupID: groupID, pageIndex: segmentPageIndex.value, pageSize: segmentPageSize.value
                    });
                    segmentTableData.value = (res.list || []).map(function (row) {
                        row.pouringDate = row.pouringDate ? row.pouringDate.substring(0, 10) : '';
                        return row;
                    });
                    segmentTotal.value = res.total || 0;
                } catch (e) {
                    console.error('loadSegmentData error', e);
                } finally {
                    loading.value = false;
                }
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

            function handleImportSegment() { openImport('segments'); }
            function downloadSegmentTemplate() { window.location.href = '/Biz/ConcreteBeam/SegmentTemplate'; }

            async function loadDesignData() {
                designLoading.value = true;
                try {
                    var res = await HttpUtils.post('/Biz/ConcreteBeam/DesignCoordinateList', {
                        castingGroupID: groupID, pageIndex: designPageIndex.value, pageSize: designPageSize.value
                    });
                    designTableData.value = (res.list || []).map(function (row) {
                        ['xe','ye','ze','xc','yc','zc','xw','yw','zw'].forEach(function (key) { row[key] = formatNullableNumber(row[key]); });
                        return row;
                    });
                    designTotal.value = res.total || 0;
                } catch (e) {
                    console.error('loadDesignData error', e);
                } finally {
                    designLoading.value = false;
                }
            }

            function handleDesignPageChange(val) { designPageIndex.value = val; loadDesignData(); }
            function handleDesignSizeChange(val) { designPageSize.value = val; designPageIndex.value = 1; loadDesignData(); }
            function onDesignNormalizeChange() { loadDesignData(); }
            function onDesignCamberChange() { loadDesignData(); }
            function handleImportDesignCoord() { openImport('design'); }
            function downloadDesignTemplate() { window.location.href = '/Biz/ConcreteBeam/DesignCoordinateTemplate'; }
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

            async function handleConfirm() {
                if (!projectInfo.offsetA) { ElMessage.warning('请输入A值'); return; }
                if (!projectInfo.offsetB) { ElMessage.warning('请输入B值'); return; }
                if (!projectInfo.offsetC) { ElMessage.warning('请输入C值'); return; }
                if (!projectInfo.referenceElevation) { ElMessage.warning('请输入参考高程'); return; }
                var fields = [projectInfo.offsetA, projectInfo.offsetB, projectInfo.offsetC, projectInfo.referenceElevation];
                if (fields.some(function (v) { return !isFourDecimalNumber(v); })) {
                    ElMessage.warning('A、B、C和参考高程必须是最多四位小数的有效数字'); return;
                }
                try {
                    var info = await HttpUtils.post('/Biz/ConcreteBeam/SaveConfig', {
                        castingGroupID: groupID,
                        offsetA: Number(projectInfo.offsetA), offsetB: Number(projectInfo.offsetB),
                        offsetC: Number(projectInfo.offsetC), referenceElevation: Number(projectInfo.referenceElevation)
                    });
                    projectInfo.offsetA = formatNullableNumber(info.offsetA);
                    projectInfo.offsetB = formatNullableNumber(info.offsetB);
                    projectInfo.offsetC = formatNullableNumber(info.offsetC);
                    projectInfo.referenceElevation = formatNullableNumber(info.referenceElevation);
                    projectInfo.createdTime = formatDateTime(info.createdTime);
                    projectInfo.updatedTime = formatDateTime(info.updatedTime);
                    rememberConfig();
                    ElMessage.success('保存成功');
                } catch (e) { console.error('save config error', e); }
            }

            function handleCancel() { restoreConfig(); }

            function handleReset() { restoreConfig(); }

            function openImport(type) {
                importType.value = type;
                importFile.value = null;
                if (uploadRef.value) uploadRef.value.clearFiles();
                importDialogVisible.value = true;
            }
            function onFileChange(file) { importFile.value = file; }
            function onFileRemove() { importFile.value = null; }
            function onFileExceed() { ElMessage.warning('只能上传一个文件，请先移除已选文件'); }

            async function confirmImport() {
                if (!importFile.value || !importFile.value.raw) { ElMessage.warning('请选择要导入的文件'); return; }
                var raw = importFile.value.raw;
                if (!/\.xlsx$/i.test(raw.name)) { ElMessage.warning('仅支持 .xlsx 文件'); return; }
                if (raw.size > 20 * 1024 * 1024) { ElMessage.warning('文件大小不能超过20 MB'); return; }
                await importExcel(importType.value, raw, false);
            }

            async function importExcel(type, file, confirmOverwrite) {
                var form = new FormData(); form.append('file', file);
                var url = type === 'segments' ? '/Biz/ConcreteBeam/ImportSegments' : '/Biz/ConcreteBeam/ImportDesignCoordinates';
                importLoading.value = true;
                try {
                    var response = await axios.post(url + '?castingGroupID=' + groupID + '&confirmOverwrite=' + confirmOverwrite,
                        form, { responseType: 'blob', headers: { 'Content-Type': 'multipart/form-data' }, timeout: 120000 });
                    var contentType = response.headers['content-type'] || '';
                    if (contentType.indexOf('application/json') < 0) {
                        downloadBlob(response.data, getDownloadName(response.headers) || '混凝土梁_导入错误明细.xlsx');
                        ElMessage.error('导入失败，请查看错误明细文件');
                        return;
                    }
                    var json = JSON.parse(await response.data.text());
                    if (json.code !== 200) { ElMessage.error(json.message || '导入失败'); return; }
                    var result = json.data || {};
                    if (result.requiresOverwriteConfirmation) {
                        try {
                            await ElMessageBox.confirm(result.message, '确认覆盖', { type: 'warning', confirmButtonText: '确认覆盖', cancelButtonText: '取消' });
                            return await importExcel(type, file, true);
                        } catch (e) { return; }
                    }
                    importDialogVisible.value = false;
                    ElMessage.success(result.message);
                    if (type === 'segments') { segmentPageIndex.value = 1; loadSegmentData(); }
                    else { designPageIndex.value = 1; loadDesignData(); }
                } catch (e) {
                    ElMessage.error(e && e.message ? e.message : '导入失败');
                } finally { importLoading.value = false; }
            }

            function downloadBlob(blob, name) {
                var url = URL.createObjectURL(blob); var link = document.createElement('a');
                link.href = url; link.download = name; document.body.appendChild(link); link.click(); link.remove(); URL.revokeObjectURL(url);
            }
            function getDownloadName(headers) {
                var value = headers['content-disposition'] || '';
                var utf = value.match(/filename\*=UTF-8''([^;]+)/i); if (utf) return decodeURIComponent(utf[1]);
                var plain = value.match(/filename="?([^";]+)"?/i); return plain ? plain[1] : '';
            }

            function rememberConfig() {
                savedConfig = { offsetA: projectInfo.offsetA, offsetB: projectInfo.offsetB,
                    offsetC: projectInfo.offsetC, referenceElevation: projectInfo.referenceElevation };
            }
            function restoreConfig() {
                projectInfo.offsetA = savedConfig.offsetA; projectInfo.offsetB = savedConfig.offsetB;
                projectInfo.offsetC = savedConfig.offsetC; projectInfo.referenceElevation = savedConfig.referenceElevation;
            }
            function formatNullableNumber(value) { return value === null || value === undefined || value === '' ? '' : Number(value).toFixed(4); }
            function formatDateTime(value) { return value ? String(value).replace('T', ' ').substring(0, 19) : ''; }
            function isFourDecimalNumber(value) { return /^[-+]?\d+(\.\d{1,4})?$/.test(String(value).trim()); }

            Vue.onMounted(function () {
                loadGroupInfo();
                loadSegmentData();
                loadDesignData();
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
                downloadSegmentTemplate: downloadSegmentTemplate,
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
                downloadDesignTemplate: downloadDesignTemplate,
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
                handleReset: handleReset,
                importDialogVisible: importDialogVisible,
                importLoading: importLoading,
                importType: importType,
                uploadRef: uploadRef,
                onFileChange: onFileChange,
                onFileRemove: onFileRemove,
                onFileExceed: onFileExceed,
                confirmImport: confirmImport
            };
        }
    });

    try {
        var locale = typeof ElementPlusLocaleZhCn !== 'undefined' ? ElementPlusLocaleZhCn : undefined;
        app.use(ElementPlus, { locale: locale });
        if (window.ElementPlusIconsVue && window.ElementPlusIconsVue.UploadFilled) app.component('UploadFilled', window.ElementPlusIconsVue.UploadFilled);
        app.mount('#app');
    } catch (e) {
        console.error('Mount error:', e);
    }
});
