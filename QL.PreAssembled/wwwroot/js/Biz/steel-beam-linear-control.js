document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var ElMessageBox = ElementPlus.ElMessageBox;
    var appI18n = window.appI18n; // 子页面全局i18n实例，同父页配置 legacy: false
    var girderID = Number(window.__girderID || 0);

    // 中英文切换（与父页面语言同步，含 ElementPlus 内置文案）
    initIframeLangSync({
        ElementPlus: ElementPlus,
        appI18n: appI18n,
        localeEn: ElementPlusLocaleEn,
        localeZh: ElementPlusLocaleZhCn
    });

    var app = Vue.createApp({
        setup: function () {
            // 页签label用computed绑定多语言，语言切换时自动刷新
            const tabs = Vue.computed(function () {
                return [
                    { name: 'theoretical', label: i18nUtils.$t('biz.steelBeam.tabTheoretical') },
                    { name: 'measured', label: i18nUtils.$t('biz.steelBeam.tabMeasured') },
                    { name: 'monitoring', label: i18nUtils.$t('biz.steelBeam.tabMonitoring') },
                    { name: 'analysis', label: i18nUtils.$t('biz.steelBeam.tabAnalysis') }
                ];
            });
            const pageTitle = Vue.ref(i18nUtils.$t('common.loading'));
            const activeTab = Vue.ref('theoretical');
            const loading = Vue.ref(false);
            const rows = Vue.ref([]);
            const total = Vue.ref(0);
            const pointOptions = Vue.ref([]);
            const segmentOptions = Vue.ref([]);
            // [Shen, Lin] 监控计算 - 开始
            // 梁段选项
            const segmentMonitorOptions = Vue.ref([]);
            const calcTheoryReq = Vue.reactive({ segmentNos: null });
            const calcTheoryDownloadFlag = Vue.ref(0);
            // [Shen, Lin] 监控计算 - 结束
            // [Shen, Lin] 误差分析 - 开始
            const posNameList = Vue.ref([]);
            const chartRefs = Vue.ref([])
            const chartInstances = new Map() // key:positionName, value:echart实例
            // [Shen, Lin] 误差分析 - 结束
            const measureTimeOptions = Vue.ref([]);
            const measureTimeValue = Vue.ref('');
            const calcResult = Vue.ref([]);
            const state = Vue.reactive({ hasTheoretical: false, hasMeasured: false, maxPushCount: null, importPushCounts: [], queryPushCounts: [] });
            const theoryQuery = Vue.reactive({ girderID: girderID, pointCode: null, segmentNo: null, pageIndex: 1, pageSize: 20 });
            const measuredQuery = Vue.reactive({ girderID: girderID, pushCount: null, measureTime: null, pageIndex: 1, pageSize: 20 });
            const editingId = Vue.ref(null);
            const editForm = Vue.reactive({ x: '', y: '', z: '' });
            const importDialogVisible = Vue.ref(false);
            const importLoading = Vue.ref(false);
            const importType = Vue.ref('theoretical');
            const importFile = Vue.ref(null);
            const importPushCount = Vue.ref(null);
            const importMeasureTime = Vue.ref(null);
            const uploadRef = Vue.ref(null);
            const rebuildLoading = Vue.ref(false);

            async function loadGirderInfo() {
                try {
                    var data = await HttpUtils.get('/Biz/SteelBeam/GirderInfo?girderID=' + girderID);
                    pageTitle.value = (data.projectName || '') + ' - ' + (data.bridgeName || '');
                } catch (e) { pageTitle.value = i18nUtils.$t('biz.steelBeam.linearControl'); }
            }

            async function loadState() {
                var data = await HttpUtils.get('/Biz/SteelBeam/ImportState?girderID=' + girderID);
                state.hasTheoretical = !!data.hasTheoretical;
                state.hasMeasured = !!data.hasMeasured;
                state.maxPushCount = data.maxPushCount === undefined ? null : data.maxPushCount;
                state.importPushCounts = data.importPushCounts || [];
                state.queryPushCounts = data.queryPushCounts || [];
            }

            async function loadTheoryOptions() {
                var data = await HttpUtils.get('/Biz/SteelBeam/TheoreticalOptions?girderID=' + girderID);
                pointOptions.value = data.points || [];
                segmentOptions.value = data.segments || [];
            }

            async function loadTheory() {
                loading.value = true; cancelEdit();
                try {
                    var data = await HttpUtils.post('/Biz/SteelBeam/TheoreticalList', theoryQuery);
                    rows.value = data.list || []; total.value = data.total || 0;
                } catch (e) { rows.value = []; total.value = 0; }
                finally { loading.value = false; }
            }

            async function loadMeasured() {
                loading.value = true; cancelEdit();
                try {
                    measuredQuery.measureTime = measureTimeValue.value || null;
                    var data = await HttpUtils.post('/Biz/SteelBeam/MeasuredList', measuredQuery);
                    rows.value = data.list || []; total.value = data.total || 0;
                } catch (e) { rows.value = []; total.value = 0; }
                finally { loading.value = false; }
            }

            async function loadMeasureTimes(pushCount) {
                measureTimeOptions.value = [];
                measureTimeValue.value = '';
                if (pushCount === null || pushCount === undefined || pushCount === '') return;
                try { measureTimeOptions.value = await HttpUtils.get('/Biz/SteelBeam/MeasureTimeOptions?girderID=' + girderID + '&pushCount=' + pushCount) || []; }
                catch (e) { measureTimeOptions.value = []; }
                // 默认选中第一个测量时间（服务端已按时间倒序，即最新一次）
                measureTimeValue.value = measureTimeOptions.value.length > 0 ? measureTimeOptions.value[0] : '';
            }

            function onTabChange(name) {
                activeTab.value = name; rows.value = []; total.value = 0; cancelEdit();
                if (name === 'theoretical') loadTheory();
                if (name === 'measured') resetMeasured();
                if (name === 'monitoring') loadCalc();
                if (name === 'analysis') loadAllCharts();
                console.log('onTabChange', name);
            }

            function searchTheory() { theoryQuery.pageIndex = 1; loadTheory(); }
            function resetTheory() { theoryQuery.pointCode = null; theoryQuery.segmentNo = null; theoryQuery.pageIndex = 1; theoryQuery.pageSize = 20; loadTheory(); }
            function onTheorySize() { theoryQuery.pageIndex = 1; loadTheory(); }
            function searchMeasured() { measuredQuery.pageIndex = 1; loadMeasured(); }
            function onMeasuredSize() { measuredQuery.pageIndex = 1; loadMeasured(); }

            async function resetMeasured() {
                measuredQuery.pushCount = state.hasMeasured ? state.maxPushCount : null;
                measuredQuery.pageIndex = 1; measuredQuery.pageSize = 20;
                measureTimeValue.value = '';
                await loadMeasureTimes(measuredQuery.pushCount);
                await loadMeasured();
            }

            async function onPushChange(value) {
                measureTimeValue.value = '';
                await loadMeasureTimes(value);
            }

            function startEdit(row) {
                editingId.value = row.id;
                editForm.x = String(activeTab.value === 'theoretical' ? row.designX : row.measuredX);
                editForm.y = String(activeTab.value === 'theoretical' ? row.designY : row.measuredY);
                editForm.z = String(activeTab.value === 'theoretical' ? row.designZ : row.measuredZ);
            }
            function cancelEdit() { editingId.value = null; editForm.x = ''; editForm.y = ''; editForm.z = ''; }

            async function saveEdit(row) {
                if (![editForm.x, editForm.y, editForm.z].every(validCoordinate)) { ElMessage.warning(i18nUtils.$t('biz.steelBeam.coordInvalid')); return; }
                var payload = { id: row.id, girderID: girderID, x: Number(editForm.x), y: Number(editForm.y), z: Number(editForm.z), version: row.version };
                var url = activeTab.value === 'theoretical' ? '/Biz/SteelBeam/UpdateTheoretical' : '/Biz/SteelBeam/UpdateMeasured';
                try {
                    await HttpUtils.post(url, payload); ElMessage.success(i18nUtils.$t('message.saveSuccess')); cancelEdit();
                    if (activeTab.value === 'theoretical') await loadTheory(); else await loadMeasured();
                } catch (e) { console.error(e); }
            }

            function validCoordinate(value) { return /^[-+]?(?:\d+\.?\d*|\.\d+)$/.test(String(value)) && (String(value).split('.')[1] || '').length <= 6; }

            function openImport(type) {
                if (type === 'measured' && !state.hasTheoretical) { ElMessage.warning(i18nUtils.$t('biz.steelBeam.importTheoryFirst')); return; }
                importType.value = type; importFile.value = null; importMeasureTime.value = null;
                importPushCount.value = state.importPushCounts.length ? state.importPushCounts[0] : null;
                if (uploadRef.value) uploadRef.value.clearFiles();
                importDialogVisible.value = true;
            }
            function onFileChange(file) { importFile.value = file; }
            function onFileRemove() { importFile.value = null; }
            function onFileExceed() { ElMessage.warning(i18nUtils.$t('biz.steelBeam.onlyOneFile')); }

            async function confirmImport() {
                if (!importFile.value || !importFile.value.raw) { ElMessage.warning(i18nUtils.$t('biz.steelBeam.selectFile')); return; }
                var raw = importFile.value.raw;
                if (!/\.xlsx$/i.test(raw.name)) { ElMessage.warning(i18nUtils.$t('biz.steelBeam.onlyXlsx')); return; }
                if (raw.size > 20 * 1024 * 1024) { ElMessage.warning(i18nUtils.$t('biz.steelBeam.fileTooLarge')); return; }
                var confirmed = false;
                if (importType.value === 'theoretical' && state.hasTheoretical) {
                    try { await ElMessageBox.confirm(i18nUtils.$t('biz.steelBeam.confirmOverwriteTheory'), i18nUtils.$t('biz.steelBeam.notice'), { type: 'warning' }); confirmed = true; }
                    catch (e) { return; }
                }
                if (importType.value === 'measured') {
                    if (importPushCount.value === null || !importMeasureTime.value) { ElMessage.warning(i18nUtils.$t('biz.steelBeam.selectPushAndTime')); return; }
                    var existingTimes = await HttpUtils.get('/Biz/SteelBeam/MeasureTimeOptions?girderID=' + girderID + '&pushCount=' + importPushCount.value) || [];
                    var selected = formatMeasureTime(importMeasureTime.value);
                    if (existingTimes.some(function (x) { return formatMeasureTime(x) === selected; })) {
                        try { await ElMessageBox.confirm(i18nUtils.$t('biz.steelBeam.confirmOverwriteMeasured'), i18nUtils.$t('biz.steelBeam.notice'), { type: 'warning' }); confirmed = true; }
                        catch (e) { return; }
                    }
                }
                await submitImport(confirmed);
            }

            async function submitImport(confirmOverwrite) {
                importLoading.value = true;
                try {
                    var form = new FormData(); form.append('file', importFile.value.raw);
                    var url;
                    if (importType.value === 'theoretical') {
                        url = '/Biz/SteelBeam/ImportTheoretical?girderID=' + girderID + '&confirmOverwrite=' + confirmOverwrite;
                    } else {
                        url = '/Biz/SteelBeam/ImportMeasured?girderID=' + girderID + '&pushCount=' + importPushCount.value + '&measureTime=' + encodeURIComponent(importMeasureTime.value) + '&confirmOverwrite=' + confirmOverwrite;
                    }
                    var response = await axios.post(url, form, { responseType: 'blob', headers: { 'Content-Type': 'multipart/form-data' } });
                    var contentType = response.headers['content-type'] || '';
                    if (contentType.indexOf('application/json') >= 0) {
                        var json = JSON.parse(await response.data.text());
                        if (json.code !== 200) { ElMessage.error(json.message || i18nUtils.$t('biz.steelBeam.importFailed')); return; }
                        importDialogVisible.value = false; ElMessage.success((json.data && json.data.message) || i18nUtils.$t('biz.steelBeam.importSuccess'));
                        await refreshAfterImport();
                    } else {
                        downloadBlob(response.data, getDownloadName(response.headers) || i18nUtils.$t('biz.steelBeam.errorDetailFile'));
                        ElMessage.error(i18nUtils.$t('biz.steelBeam.importFailedDetail'));
                    }
                } catch (e) { ElMessage.error(extractError(e)); }
                finally { importLoading.value = false; }
            }

            // 重算推算坐标：已推出胎架梁段在各顶推批次下的坐标
            // 导入实测会自动推算本批次；这里用于回填历史批次或人工改完实测数据后整体刷新
            async function rebuildDerived() {
                try {
                    await ElMessageBox.confirm(i18nUtils.$t('biz.steelBeam.rebuildDerivedConfirm'), i18nUtils.$t('biz.steelBeam.notice'), { type: 'warning' });
                } catch (e) { return; }
                rebuildLoading.value = true;
                try {
                    var message = await HttpUtils.post('/Biz/SteelBeam/RebuildDerived?girderId=' + girderID, {});
                    ElMessage.success(message || i18nUtils.$t('biz.steelBeam.rebuildDerivedSuccess'));
                    await refreshAfterImport();
                } catch (e) { ElMessage.error(extractError(e)); }
                finally { rebuildLoading.value = false; }
            }

            async function refreshAfterImport() {
                await loadState(); await loadTheoryOptions();
                if (activeTab.value === 'theoretical') await loadTheory(); else await resetMeasured();
            }

            function downloadTemplate(type) {
                window.location.href = type === 'theoretical' ? '/Biz/SteelBeam/TheoreticalTemplate' : '/Biz/SteelBeam/MeasuredTemplate';
            }

            async function downloadMeasured() {
                try {
                    var payload = Object.assign({}, measuredQuery, { measureTime: measureTimeValue.value || null, pageIndex: 1, pageSize: 20 });
                    var response = await axios.post('/Biz/SteelBeam/DownloadMeasured', payload, { responseType: 'blob' });
                    var contentType = response.headers['content-type'] || '';
                    if (contentType.indexOf('application/json') >= 0) {
                        var json = JSON.parse(await response.data.text()); ElMessage.warning(json.message || i18nUtils.$t('biz.steelBeam.noDownloadData')); return;
                    }
                    downloadBlob(response.data, getDownloadName(response.headers) || i18nUtils.$t('biz.steelBeam.measuredFileName'));
                } catch (e) { ElMessage.error(extractError(e)); }
            }

            // 监控计算 - 开始
            async function loadCalc() {
                console.log('loadCalc');
                loadCalcSegmentNo();
            }

            async function loadCalcSegmentNo() {
                // GetCalcSegmentNo
                console.log('loadCalcSegmentNo');
                segmentMonitorOptions.value = [];
                try { segmentMonitorOptions.value = await HttpUtils.get('/Biz/SteelBeam/GetCalcSegmentNo?girderID=' + girderID) || []; }
                catch (e) {
                    console.log('loadCalcSegmentNo:e', e);
                    segmentMonitorOptions.value = [];
                }
            }
            async function calTheory() {
                getCalcResult();
            }
            async function calTheoryReset() {

            }
            async function calcTheoryDownload() {
                try {
                    await HttpUtils.downloadPost('/Biz/SteelBeam/DownloadCalcResult', calcResult.value);
                }
                catch (e) {
                    console.log('loadCalcSegmentNo:e', e);
                    segmentMonitorOptions.value = [];
                }
            }
            async function calcTheoryReqChange() {
                calcTheoryDownloadFlag.value = 0;
                console.log('calcTheoryReqChange:', calcTheoryDownloadFlag.value)
            }
            async function getCalcResult() {
                calcResult.value = [];
                try {
                    const arr = calcTheoryReq.segmentNos;
                    const params = new URLSearchParams();
                    arr.forEach(x => params.append("segmentNos", x));
                    // 得到 "segmentNos=A&segmentNos=B"
                    const queryStr = params.toString();
                    var result = await HttpUtils.get('/Biz/SteelBeam/GetCalcResultBySegmentNos?girderID=' + girderID
                        + "&" + queryStr
                    );
                    console.log('getCalcResult:result:', result);
                    calcResult.value = result;
                    calcTheoryDownloadFlag.value = 1;
                } catch (e) {
                    console.log('getCalcResult:result:', e);
                    calcResult.value = [];
                }
            }
            // 监控计算 - 结束

            // 误差分析 - 开始
            async function loadPositionNames() {
                try {
                    const result = await HttpUtils.post('/Biz/SteelBeam/GetPositionNames?girderId=' + girderID);
                    console.log('GetPositionNames:', result);
                    return result;
                } catch (e) {
                    console.log('GetPositionNames', e);
                }
            }
            async function loadChartByPosition(positionName, sort) {
                try {
                    const result = await HttpUtils.post('/Biz/SteelBeam/GetSteelBeamOffset?girderId=' + girderID
                        + "&positionName=" + positionName + "&sort=" + (sort || 'asc')
                    );
                    console.log('loadChartByPosition:', result);
                    return result.rows;
                } catch (e) {
                    console.log('loadChartByPosition', e);
                }
            }
            async function loadAllCharts() {
                posNameList.value = await loadPositionNames()
                await Vue.nextTick() // 等待dom渲染完成
                for (const pos of posNameList.value) {
                    try {
                        const data = await loadChartByPosition(pos)
                        renderChart(pos, data)
                    } catch (e) { console.log('loadAllCharts', pos, e) }
                }
            }
            // 渲染单个图表
            function renderChart(posName, data) {
                const dom = document.querySelector(`.chart-box[data-pos="${posName}"]`)
                if (!dom) return
                let chart = chartInstances.get(posName)
                if (!chart) {
                    chart = echarts.init(dom)
                    chartInstances.set(posName, chart)
                }

                // 构造series数据，同时保留索引对应原始data
                const seriesData = data.map((item) => {
                    return [item.pointDistance, item.zError]
                })

                const option = {
                    tooltip: {
                        trigger: 'axis',
                        formatter: function (params) {
                            // params[0] 是当前hover的项，dataIndex拿到下标，取原始data中的segmentNo
                            const idx = params[0].dataIndex
                            const row = data[idx]
                            return `
${row.pointDistance}<br/>
${params[0].marker} ${params[0].seriesName} &nbsp; ${row.zError}<br/>
SegmentNo：${row.segmentNo}
                `
                        }
                    },
                    grid: {
                        left: 40,
                        right: 20,
                        top: 40,
                        bottom: 40
                    },
                    xAxis: {
                        name: 'PointDistance',
                        type: 'value'
                    },
                    yAxis: {
                        name: 'Z误差',
                        type: 'value'
                    },
                    series: [
                        {
                            name: posName,
                            type: 'line',
                            data: seriesData,
                            smooth: true
                        }
                    ]
                }
                chart.setOption(option, true)
            }

            // 误差分析 - 结束
            function downloadBlob(blob, name) {
                var url = URL.createObjectURL(blob); var link = document.createElement('a');
                link.href = url; link.download = name; document.body.appendChild(link); link.click(); link.remove(); URL.revokeObjectURL(url);
            }
            function getDownloadName(headers) {
                var value = headers['content-disposition'] || '';
                var utf = value.match(/filename\*=UTF-8''([^;]+)/i); if (utf) return decodeURIComponent(utf[1]);
                var plain = value.match(/filename="?([^";]+)"?/i); return plain ? plain[1] : '';
            }
            function extractError(error) { return error && error.message ? error.message : i18nUtils.$t('common.failed'); }

            function measuredSpanMethod(args) {
                if (args.columnIndex !== 0) return [1, 1];
                return args.row.pointRowSpan > 0 ? [args.row.pointRowSpan, 1] : [0, 0];
            }
            function formatMeasureTime(value) {
                if (!value) return '';
                var text = String(value).replace('T', ' ');
                return text.length >= 13 ? text.substring(0, 13) + ':00' : text;
            }
            function formatDecimal(value) { return value === null || value === undefined ? '-' : value; }
            function disableFutureDate(date) { var today = new Date(); today.setHours(23, 59, 59, 999); return date.getTime() > today.getTime(); }

            Vue.onMounted(async function () {
                await loadGirderInfo();
                try { await loadState(); await loadTheoryOptions(); await loadTheory(); }
                catch (e) { console.error(e); }
            });

            return {
                tabs: tabs, pageTitle: pageTitle, activeTab: activeTab, loading: loading, rows: rows, total: total,
                pointOptions: pointOptions, segmentMonitorOptions: segmentMonitorOptions, segmentOptions: segmentOptions, measureTimeOptions: measureTimeOptions, measureTimeValue: measureTimeValue,
                state: state, theoryQuery: theoryQuery, measuredQuery: measuredQuery, editingId: editingId, editForm: editForm,
                importDialogVisible: importDialogVisible, importLoading: importLoading, importType: importType,
                importPushCount: importPushCount, importMeasureTime: importMeasureTime, uploadRef: uploadRef, rebuildLoading: rebuildLoading,
                rebuildDerived: rebuildDerived,
                onTabChange: onTabChange, searchTheory: searchTheory, resetTheory: resetTheory, loadTheory: loadTheory,
                onTheorySize: onTheorySize, searchMeasured: searchMeasured, resetMeasured: resetMeasured, loadMeasured: loadMeasured,
                onMeasuredSize: onMeasuredSize, onPushChange: onPushChange, startEdit: startEdit, cancelEdit: cancelEdit, saveEdit: saveEdit,
                openImport: openImport, onFileChange: onFileChange, onFileRemove: onFileRemove, onFileExceed: onFileExceed,
                confirmImport: confirmImport, downloadTemplate: downloadTemplate, downloadMeasured: downloadMeasured,
                measuredSpanMethod: measuredSpanMethod, formatMeasureTime: formatMeasureTime, formatDecimal: formatDecimal, disableFutureDate: disableFutureDate,
                calcTheoryReq: calcTheoryReq, calcTheory: calTheory, calTheoryReset: calTheoryReset,
                loadCalcSegmentNo: loadCalcSegmentNo, loadCalc: loadCalc,
                calcResult: calcResult, calcTheoryDownloadFlag: calcTheoryDownloadFlag, calcTheoryReqChange: calcTheoryReqChange,
                calcTheoryDownload: calcTheoryDownload,
                posNameList: posNameList
            };
        }
    });

    // 注册 Vue I18n
    app.use(appI18n);

    app.use(ElementPlus, { locale: window.ElementPlusLocaleZhCn });
    // 注册页面模板用到的图标（原只注册了UploadFilled，Search/RefreshLeft/RefreshRight未注册不显示）
    if (window.ElementPlusIconsVue) {
        ['Search', 'RefreshLeft', 'RefreshRight', 'Refresh', 'UploadFilled', 'Download'].forEach(function (name) {
            if (window.ElementPlusIconsVue[name]) app.component(name, window.ElementPlusIconsVue[name]);
        });
    }
    app.mount('#app');
});
