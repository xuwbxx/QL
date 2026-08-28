document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var ElMessageBox = ElementPlus.ElMessageBox;
    var bridgeID = Number(window.__bridgeID || 0);

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
            var rows = Vue.ref([]);
            var total = Vue.ref(0);
            var pointOptions = Vue.ref([]);
            var segmentOptions = Vue.ref([]);
            var measureTimeOptions = Vue.ref([]);
            var measureTimeValue = Vue.ref('');
            var state = Vue.reactive({ hasTheoretical: false, hasMeasured: false, maxPushCount: null, importPushCounts: [], queryPushCounts: [] });
            var theoryQuery = Vue.reactive({ bridgeID: bridgeID, pointCode: null, segmentNo: null, pageIndex: 1, pageSize: 20 });
            var measuredQuery = Vue.reactive({ bridgeID: bridgeID, pushCount: null, measureTime: null, pageIndex: 1, pageSize: 20 });
            var editingId = Vue.ref(null);
            var editForm = Vue.reactive({ x: '', y: '', z: '' });
            var importDialogVisible = Vue.ref(false);
            var importLoading = Vue.ref(false);
            var importType = Vue.ref('theoretical');
            var importFile = Vue.ref(null);
            var importPushCount = Vue.ref(null);
            var importMeasureTime = Vue.ref(null);
            var uploadRef = Vue.ref(null);

            function goBack() { window.history.back(); }

            async function loadBridgeInfo() {
                try {
                    var data = await HttpUtils.get('/Biz/SteelBeam/BridgeInfo?id=' + bridgeID);
                    pageTitle.value = (data.projectName || '') + ' - ' + (data.bridgeName || '');
                } catch (e) { pageTitle.value = '线形管控'; }
            }

            async function loadState() {
                var data = await HttpUtils.get('/Biz/SteelBeam/ImportState?bridgeID=' + bridgeID);
                state.hasTheoretical = !!data.hasTheoretical;
                state.hasMeasured = !!data.hasMeasured;
                state.maxPushCount = data.maxPushCount === undefined ? null : data.maxPushCount;
                state.importPushCounts = data.importPushCounts || [];
                state.queryPushCounts = data.queryPushCounts || [];
            }

            async function loadTheoryOptions() {
                var data = await HttpUtils.get('/Biz/SteelBeam/TheoreticalOptions?bridgeID=' + bridgeID);
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
                if (pushCount === null || pushCount === undefined || pushCount === '') return;
                try { measureTimeOptions.value = await HttpUtils.get('/Biz/SteelBeam/MeasureTimeOptions?bridgeID=' + bridgeID + '&pushCount=' + pushCount) || []; }
                catch (e) { measureTimeOptions.value = []; }
            }

            function onTabChange(name) {
                activeTab.value = name; rows.value = []; total.value = 0; cancelEdit();
                if (name === 'theoretical') loadTheory();
                if (name === 'measured') resetMeasured();
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
                if (![editForm.x, editForm.y, editForm.z].every(validCoordinate)) { ElMessage.warning('坐标必须是有效数字，最多保留6位小数'); return; }
                var payload = { id: row.id, bridgeID: bridgeID, x: Number(editForm.x), y: Number(editForm.y), z: Number(editForm.z), version: row.version };
                var url = activeTab.value === 'theoretical' ? '/Biz/SteelBeam/UpdateTheoretical' : '/Biz/SteelBeam/UpdateMeasured';
                try {
                    await HttpUtils.post(url, payload); ElMessage.success('保存成功'); cancelEdit();
                    if (activeTab.value === 'theoretical') await loadTheory(); else await loadMeasured();
                } catch (e) { console.error(e); }
            }

            function validCoordinate(value) { return /^[-+]?(?:\d+\.?\d*|\.\d+)$/.test(String(value)) && (String(value).split('.')[1] || '').length <= 6; }

            function openImport(type) {
                if (type === 'measured' && !state.hasTheoretical) { ElMessage.warning('请先导入理论数据'); return; }
                importType.value = type; importFile.value = null; importMeasureTime.value = null;
                importPushCount.value = state.importPushCounts.length ? state.importPushCounts[0] : null;
                if (uploadRef.value) uploadRef.value.clearFiles();
                importDialogVisible.value = true;
            }
            function onFileChange(file) { importFile.value = file; }
            function onFileRemove() { importFile.value = null; }
            function onFileExceed() { ElMessage.warning('只能上传一个文件，请先移除已选文件'); }

            async function confirmImport() {
                if (!importFile.value || !importFile.value.raw) { ElMessage.warning('请选择要导入的文件'); return; }
                var raw = importFile.value.raw;
                if (!/\.xlsx$/i.test(raw.name)) { ElMessage.warning('仅支持.xlsx文件'); return; }
                if (raw.size > 20 * 1024 * 1024) { ElMessage.warning('文件大小不能超过20 MB'); return; }
                var confirmed = false;
                if (importType.value === 'theoretical' && state.hasTheoretical) {
                    try { await ElMessageBox.confirm('本次导入将覆盖当前全部理论数据，是否继续？', '提示', { type: 'warning' }); confirmed = true; }
                    catch (e) { return; }
                }
                if (importType.value === 'measured') {
                    if (importPushCount.value === null || !importMeasureTime.value) { ElMessage.warning('请选择顶推次数和测量时间'); return; }
                    var existingTimes = await HttpUtils.get('/Biz/SteelBeam/MeasureTimeOptions?bridgeID=' + bridgeID + '&pushCount=' + importPushCount.value) || [];
                    var selected = formatMeasureTime(importMeasureTime.value);
                    if (existingTimes.some(function (x) { return formatMeasureTime(x) === selected; })) {
                        try { await ElMessageBox.confirm('本次导入将覆盖原数据，是否继续？', '提示', { type: 'warning' }); confirmed = true; }
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
                        url = '/Biz/SteelBeam/ImportTheoretical?bridgeID=' + bridgeID + '&confirmOverwrite=' + confirmOverwrite;
                    } else {
                        url = '/Biz/SteelBeam/ImportMeasured?bridgeID=' + bridgeID + '&pushCount=' + importPushCount.value + '&measureTime=' + encodeURIComponent(importMeasureTime.value) + '&confirmOverwrite=' + confirmOverwrite;
                    }
                    var response = await axios.post(url, form, { responseType: 'blob', headers: { 'Content-Type': 'multipart/form-data' } });
                    var contentType = response.headers['content-type'] || '';
                    if (contentType.indexOf('application/json') >= 0) {
                        var json = JSON.parse(await response.data.text());
                        if (json.code !== 200) { ElMessage.error(json.message || '导入失败'); return; }
                        importDialogVisible.value = false; ElMessage.success((json.data && json.data.message) || '导入成功');
                        await refreshAfterImport();
                    } else {
                        downloadBlob(response.data, getDownloadName(response.headers) || '错误明细.xlsx');
                        ElMessage.error('导入失败，请查看错误明细文件');
                    }
                } catch (e) { ElMessage.error(extractError(e)); }
                finally { importLoading.value = false; }
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
                        var json = JSON.parse(await response.data.text()); ElMessage.warning(json.message || '当前筛选条件下无可下载数据'); return;
                    }
                    downloadBlob(response.data, getDownloadName(response.headers) || '钢梁实测数据.xlsx');
                } catch (e) { ElMessage.error(extractError(e)); }
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
            function extractError(error) { return error && error.message ? error.message : '操作失败'; }

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
                await loadBridgeInfo();
                try { await loadState(); await loadTheoryOptions(); await loadTheory(); }
                catch (e) { console.error(e); }
            });

            return {
                tabs: tabs, pageTitle: pageTitle, activeTab: activeTab, loading: loading, rows: rows, total: total,
                pointOptions: pointOptions, segmentOptions: segmentOptions, measureTimeOptions: measureTimeOptions, measureTimeValue: measureTimeValue,
                state: state, theoryQuery: theoryQuery, measuredQuery: measuredQuery, editingId: editingId, editForm: editForm,
                importDialogVisible: importDialogVisible, importLoading: importLoading, importType: importType,
                importPushCount: importPushCount, importMeasureTime: importMeasureTime, uploadRef: uploadRef,
                goBack: goBack, onTabChange: onTabChange, searchTheory: searchTheory, resetTheory: resetTheory, loadTheory: loadTheory,
                onTheorySize: onTheorySize, searchMeasured: searchMeasured, resetMeasured: resetMeasured, loadMeasured: loadMeasured,
                onMeasuredSize: onMeasuredSize, onPushChange: onPushChange, startEdit: startEdit, cancelEdit: cancelEdit, saveEdit: saveEdit,
                openImport: openImport, onFileChange: onFileChange, onFileRemove: onFileRemove, onFileExceed: onFileExceed,
                confirmImport: confirmImport, downloadTemplate: downloadTemplate, downloadMeasured: downloadMeasured,
                measuredSpanMethod: measuredSpanMethod, formatMeasureTime: formatMeasureTime, formatDecimal: formatDecimal, disableFutureDate: disableFutureDate
            };
        }
    });

    app.use(ElementPlus, { locale: window.ElementPlusLocaleZhCn });
    if (window.ElementPlusIconsVue && window.ElementPlusIconsVue.UploadFilled) app.component('UploadFilled', window.ElementPlusIconsVue.UploadFilled);
    app.mount('#app');
});
