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
            const cardData = Vue.ref({});        // posName -> 该测位所有点位行（asc）
            const cardSort = Vue.reactive({});    // posName -> 'asc' | 'desc'（卡内独立升/降序，控制 X 轴方向）
            const chartInstances = new Map() // key:positionName, value:echart实例
            const analysisMode = Vue.ref('dist'); // 'dist' 测位分布 | 'merge' 测位合并
            const zoomVisible = Vue.ref(false);
            const zoomTarget = Vue.reactive({ mode: 'dist', pos: '' }); // 放大弹窗目标
            let mergeChartInstance = null; // 测位合并大图实例
            let zoomChartInstance = null;  // 放大弹窗图实例
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
                if (name === 'analysis') loadAllCards();
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
                measuredQuery.pushCount = value;
                measureTimeValue.value = '';
                await loadMeasureTimes(value);
                // 顶推次数变更后自动刷新（loadMeasureTimes 已把测量时间默认置为最新一次）
                measuredQuery.pageIndex = 1;
                await loadMeasured();
            }

            function onMeasureTimeChange() {
                measuredQuery.pageIndex = 1;
                loadMeasured();
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

            async function deleteMeasured(row) {
                try { await ElMessageBox.confirm(i18nUtils.$t('common.confirmDelete'), i18nUtils.$t('biz.steelBeam.notice'), { type: 'warning' }); }
                catch (e) { return; }
                try {
                    await HttpUtils.post('/Biz/SteelBeam/DeleteMeasured', { id: row.id, girderID: girderID, version: row.version });
                    ElMessage.success(i18nUtils.$t('common.deleteSuccess'));
                    await loadMeasured();
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
            // 取单个测位全部点位（按服务端 asc 返回，前端再按卡片开关排序绘制）
            async function loadPositionData(pos) {
                try {
                    const data = await HttpUtils.post('/Biz/SteelBeam/GetSteelBeamOffset?girderId=' + girderID
                        + "&positionName=" + pos + "&sort=asc"
                    );
                    console.log('loadPositionData:', pos, data);
                    const rows = (data && data.rows) ? data.rows : [];
                    cardData.value = Object.assign({}, cardData.value, { [pos]: rows });
                    if (!(pos in cardSort)) cardSort[pos] = 'asc';
                    renderCardChart(pos);
                    return rows;
                } catch (e) {
                    console.log('loadPositionData', pos, e);
                    return [];
                }
            }
            // 测位编号自然排序：数字段按数值比较（忽略前导0），字母段按字母表比较；
            // 字母+数字先比字母再比数字（A2 < A11、A1 < B1），纯数字 2 < 11。
            function naturalCompare(a, b) {
                const segA = String(a == null ? '' : a).match(/\d+|\D+/g) || [];
                const segB = String(b == null ? '' : b).match(/\d+|\D+/g) || [];
                const n = Math.min(segA.length, segB.length);
                for (let i = 0; i < n; i++) {
                    const x = segA[i], y = segB[i];
                    const xNum = /^\d+$/.test(x), yNum = /^\d+$/.test(y);
                    if (xNum && yNum) {
                        const xi = parseInt(x, 10), yi = parseInt(y, 10);
                        if (xi !== yi) return xi - yi;      // 数值比较，前导0不影响
                    } else if (!xNum && !yNum) {
                        const cmp = x.localeCompare(y, 'en', { sensitivity: 'base' });
                        if (cmp !== 0) return cmp;          // 字母表顺序
                    } else {
                        return xNum ? -1 : 1;               // 数字段排在字母段前
                    }
                }
                return segA.length - segB.length;           // 前缀相同者短者在前
            }
            // 加载所有测位：按测位编号自然排序后每卡一张 echarts 折线图
            async function loadAllCards() {
                cardData.value = {};
                for (const k in cardSort) delete cardSort[k];
                chartInstances.clear();
                posNameList.value = ((await loadPositionNames()) || []).slice().sort(naturalCompare);
                await Vue.nextTick(); // 等待卡片 DOM 渲染
                for (const pos of posNameList.value) {
                    try { await loadPositionData(pos); }
                    catch (e) { console.log('loadAllCards', pos, e); }
                }
                if (analysisMode.value === 'merge') { await Vue.nextTick(); renderMergeChart(); }
            }
            // 按当前卡片的升/降序，对点位按 X 轴（点位距离 PointDistance）排序
            function sortedCardRows(pos) {
                const rows = cardData.value[pos] || [];
                const dir = cardSort[pos] || 'asc';
                const sorted = rows.slice().sort((a, b) => {
                    const av = a.pointDistance == null ? 0 : Number(a.pointDistance);
                    const bv = b.pointDistance == null ? 0 : Number(b.pointDistance);
                    return dir === 'desc' ? bv - av : av - bv;
                });
                return sorted;
            }
            // 该测位最大正误差（ZError 中 > 0 的最大值）
            function cardMaxPositive(pos) {
                const rows = cardData.value[pos] || [];
                let max = null;
                rows.forEach(r => {
                    const v = r.zError == null ? null : Number(r.zError);
                    if (v != null && v > 0 && (max === null || v > max)) max = v;
                });
                return max;
            }
            // 该测位最大负误差（ZError 中 < 0 的最小值）
            function cardMaxNegative(pos) {
                const rows = cardData.value[pos] || [];
                let min = null;
                rows.forEach(r => {
                    const v = r.zError == null ? null : Number(r.zError);
                    if (v != null && v < 0 && (min === null || v < min)) min = v;
                });
                return min;
            }
            // 误差值格式化：带正负号，4 位小数；空值显示 '-'
            function formatError(value) {
                if (value === null || value === undefined) return '-';
                const v = Number(value);
                if (isNaN(v)) return '-';
                const s = v.toFixed(4);
                return (v > 0 ? '+' : '') + s;
            }
            // 单个测位的 echarts option（big=true 时带「下载图片」工具）
            function buildSingleOption(pos, big) {
                const rows = sortedCardRows(pos);
                const desc = cardSort[pos] === 'desc';
                // 降序：X 值重算为 nMax - n（nMax = 本测位最大点位距离），实现点位顺序反置且坐标原点不变
                const nMax = rows.reduce((m, r) => Math.max(m, Number(r.pointDistance) || 0), 0);
                const xOf = (r) => {
                    const x = Number(r.pointDistance) || 0;
                    return desc ? nMax - x : x;
                };
                const seriesData = rows.map(r => [xOf(r), r.zError == null ? null : Number(r.zError)]);
                const segmentNoStr = i18nUtils.$t('biz.steelBeam.segmentNo');
                const pointCodeStr = i18nUtils.$t('biz.steelBeam.pointCode');
                const option = {
                    tooltip: {
                        trigger: 'axis',
                        // 悬浮框限制在图表容器内，避免贴近右缘时溢出页面
                        confine: true,
                        formatter: function (params) {
                            const idx = params[0].dataIndex;
                            const row = rows[idx];
                            return `${pointCodeStr}: ${row.pointCode}<br/>` +
                                `${segmentNoStr}：${row.segmentNo}<br/>` +
                                `${i18nUtils.$t('biz.steelBeam.analysisYAxis')}: ${row.zError}<br/>` +
                                `${i18nUtils.$t('biz.steelBeam.analysisXAxis')}: ${row.pointDistance}`
                                ;
                        }
                    },
                    grid: { left: 50, right: 20, top: 40, bottom: 40 },
                    // X 轴保持 0→nMax（原点在左）；顺序反置由数据的 nMax-n 变换实现，不用 inverse（那会连刻度一起翻）
                    xAxis: { name: i18nUtils.$t('biz.steelBeam.analysisXAxis'), type: 'value' },
                    yAxis: { name: i18nUtils.$t('biz.steelBeam.analysisYAxis'), type: 'value' },
                    // 单测位小图统一使用第 1 种颜色；connectNulls=true 使孤点与前一个点相连（折线连续）
                    series: [{ name: pos, type: 'line', data: seriesData, smooth: false, connectNulls: true, color: POSITION_COLORS[0] }]
                };
                if (big) option.toolbox = buildSaveToolbox();
                return option;
            }
            // 测位配色：固定 30 种常用分类色（不随机，刷新前后一致）。
            // 前 9 个沿用 ECharts 内置主题色（token.color.theme），第 10 个补红色凑满前 10；
            // 其余为常用高区分度分类色。合并图按测位顺序取色，单测位小图统一取第 1 色。
            const POSITION_COLORS = [
                // —— 原 ECharts 默认主题色（9 个）——
                '#5070dd', '#b6d634', '#505372', '#ff994d', '#0ca8df',
                '#ffd10a', '#fb628b', '#785db0', '#3fbe95',
                // —— 第 10 个（补红）——
                '#d94f4f',
                // —— 其余 20 个常用分类色 ——
                '#8c564b', '#e377c2', '#7f7f7f', '#bcbd22', '#17becf',
                '#2ca02c', '#1f77b4', '#ff7f0e', '#9467bd', '#d62728',
                '#aec7e8', '#ffbb78', '#98df8a', '#ff9896', '#c5b0d5',
                '#c49c94', '#f7b6d2', '#c7c7c7', '#dbdb8d', '#9edae5'
            ];
            // 合并图：按测位顺序取色（超过 30 个循环复用）
            const positionColor = function (pos) {
                const idx = (posNameList.value || []).indexOf(pos);
                return POSITION_COLORS[(idx < 0 ? 0 : idx) % POSITION_COLORS.length];
            };
            // 全部测位合并为一个折线图（多 series，图例=测位名）
            function buildMergeOption(big) {
                const series = posNameList.value.map(function (pos) {
                    const rows = cardData.value[pos] || [];
                    // connectNulls=true：补全点（null）间隙直接连线，孤点与前一个点相连，折线保持连续
                    return { name: pos, type: 'line', smooth: false, connectNulls: true, color: positionColor(pos), data: rows.map(r => [Number(r.pointDistance) || 0, r.zError == null ? null : Number(r.zError)]) };
                });
                const option = {
                    // 30 色固定调色板（图例与折线一致，避免默认 10 色循环导致颜色重复）
                    color: POSITION_COLORS,
                    tooltip: {
                        trigger: 'axis',
                        // 悬浮框限制在图表容器内；按误差值降序并过滤无数据项，避免列表过高/溢出页面
                        confine: true,
                        order: 'valueDesc',
                        formatter: function (params) {
                            if (!params || !params.length) return '';
                            const lines = params
                                .filter(function (p) { return p.value && p.value[1] != null; })
                                .map(function (p) {
                                    return p.marker + p.seriesName + ': ' + Number(p.value[1]).toFixed(4);
                                });
                            return lines.length ? lines.join('<br/>') : '-';
                        }
                    },
                    legend: { type: 'scroll', top: 6 },
                    grid: { left: 50, right: 20, top: 60, bottom: 50 },
                    xAxis: { name: i18nUtils.$t('biz.steelBeam.analysisXAxis'), type: 'value' },
                    yAxis: { name: i18nUtils.$t('biz.steelBeam.analysisYAxis'), type: 'value' },
                    series: series
                };
                if (big) option.toolbox = buildSaveToolbox();
                return option;
            }
            // echarts「下载图片」工具（右上角）
            function buildSaveToolbox() {
                return {
                    right: 18,
                    feature: {
                        saveAsImage: {
                            title: i18nUtils.$t('biz.steelBeam.analysisSaveImage'),
                            name: 'steel-beam-error',
                            pixelRatio: 2
                        }
                    }
                };
            }
            // 渲染单卡片内的 echarts 折线图（X=点位距离，Y=Z误差）
            function renderCardChart(pos) {
                const dom = document.querySelector(`.err-card-chart[data-pos="${pos}"]`);
                if (!dom) return;
                let chart = chartInstances.get(pos);
                if (!chart) {
                    chart = echarts.init(dom);
                    chartInstances.set(pos, chart);
                }
                chart.setOption(buildSingleOption(pos, false), true);
            }
            // 渲染「测位合并」大图
            function renderMergeChart() {
                const dom = document.querySelector('.err-merge-chart');
                if (!dom) return;
                if (!mergeChartInstance) mergeChartInstance = echarts.init(dom);
                mergeChartInstance.setOption(buildMergeOption(true), true);
                mergeChartInstance.resize();
            }
            // 卡内升/降序按钮：切换 X 轴方向后仅重绘该卡片图表（不回请求后端）
            function onCardSortToggle(pos) {
                cardSort[pos] = (cardSort[pos] === 'desc') ? 'asc' : 'desc';
                renderCardChart(pos);
            }
            // 切换「测位分布 / 测位合并」
            async function onAnalysisModeChange() {
                await Vue.nextTick();
                if (analysisMode.value === 'merge') { renderMergeChart(); }
                else { chartInstances.forEach(c => c.resize()); }
            }
            // 打开放大弹窗（单测位 / 全部合并）
            function openZoom(pos) { zoomTarget.mode = 'dist'; zoomTarget.pos = pos; zoomVisible.value = true; }
            function openZoomMerge() { zoomTarget.mode = 'merge'; zoomTarget.pos = ''; zoomVisible.value = true; }
            // 放大弹窗内的图表渲染（由 el-dialog @@opened 触发）
            function renderZoomChart() {
                const dom = document.querySelector('.err-zoom-chart');
                if (!dom) return;
                if (!zoomChartInstance) zoomChartInstance = echarts.init(dom);
                zoomChartInstance.setOption(zoomTarget.mode === 'merge' ? buildMergeOption(true) : buildSingleOption(zoomTarget.pos, true), true);
                zoomChartInstance.resize();
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
                window.addEventListener('resize', () => {
                    chartInstances.forEach(c => c.resize());
                    if (mergeChartInstance) mergeChartInstance.resize();
                    if (zoomChartInstance) zoomChartInstance.resize();
                });
                // 语言切换后重绘误差分析图表：echarts 轴名由 JS 运行时取 $t（非模板响应式），不会自动刷新
                window.addEventListener('languageChanged', function () {
                    if (!posNameList.value || posNameList.value.length === 0) return;
                    posNameList.value.forEach(function (pos) { renderCardChart(pos); });
                    if (analysisMode.value === 'merge') { renderMergeChart(); }
                    if (zoomVisible.value) { renderZoomChart(); }
                });
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
                onMeasuredSize: onMeasuredSize, onPushChange: onPushChange, onMeasureTimeChange: onMeasureTimeChange, startEdit: startEdit, cancelEdit: cancelEdit, saveEdit: saveEdit, deleteMeasured: deleteMeasured,
                openImport: openImport, onFileChange: onFileChange, onFileRemove: onFileRemove, onFileExceed: onFileExceed,
                confirmImport: confirmImport, downloadTemplate: downloadTemplate, downloadMeasured: downloadMeasured,
                measuredSpanMethod: measuredSpanMethod, formatMeasureTime: formatMeasureTime, formatDecimal: formatDecimal, disableFutureDate: disableFutureDate,
                calcTheoryReq: calcTheoryReq, calcTheory: calTheory, calTheoryReset: calTheoryReset,
                loadCalcSegmentNo: loadCalcSegmentNo, loadCalc: loadCalc,
                calcResult: calcResult, calcTheoryDownloadFlag: calcTheoryDownloadFlag, calcTheoryReqChange: calcTheoryReqChange,
                calcTheoryDownload: calcTheoryDownload,
                posNameList: posNameList,
                analysisMode: analysisMode, onAnalysisModeChange: onAnalysisModeChange, zoomVisible: zoomVisible, zoomTarget: zoomTarget, openZoom: openZoom, openZoomMerge: openZoomMerge, renderZoomChart: renderZoomChart,
                cardData: cardData, cardSort: cardSort,
                sortedCardRows: sortedCardRows, cardMaxPositive: cardMaxPositive, cardMaxNegative: cardMaxNegative,
                formatError: formatError, onCardSortToggle: onCardSortToggle
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
