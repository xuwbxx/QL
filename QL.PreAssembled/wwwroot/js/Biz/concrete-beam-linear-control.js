document.addEventListener('DOMContentLoaded', function () {
    var Vue = window.Vue;
    var ElementPlus = window.ElementPlus;
    var ElMessage = ElementPlus.ElMessage;
    var ElMessageBox = ElementPlus.ElMessageBox;
    var groupID = window.__groupID || 0;
    var appI18n = window.appI18n; // 子页面全局 i18n 实例，同父页配置 legacy: false

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
            var activeTab = Vue.ref('projectInfo');
            var hasConfirmedDesignCoordinates = Vue.ref(false);
            var hasCalculatedDesignCoordinates = Vue.ref(false);
            // 该浇筑组是否已存在梁段实测填报（segment 表有数据）。
            // 一旦为 true，【项目信息】的 A/B/C/参考高程与【设计坐标】就不再允许更改——
            // 它们是全部既有浇筑坐标的基准，改了会让已归档的实测数据失去可比性。
            // 服务端对 SaveConfig / 设计坐标导入 / 预拱度导入同样会拒绝，此处只是前置禁用。
            var hasSurveyData = Vue.ref(false);
            var confirmedDesignIncludesCamber = Vue.ref(false);
            var designPreviewToken = Vue.ref('');

            // 页签 label 用 computed 绑定多语言，语言切换时自动刷新（与 SteelBeam 页签写法一致）。
            // 注意：改 computed 后，JS 内部访问要写 tabs.value；模板里会自动解包，仍写 tabs。
            var tabs = Vue.computed(function () {
                return [
                    { name: 'projectInfo', label: i18nUtils.$t('biz.concreteBeam.tabProjectInfo'), disabled: false },
                    { name: 'designCoord', label: i18nUtils.$t('biz.concreteBeam.tabDesignCoord'), disabled: true },
                    {
                        name: 'preCamber', label: i18nUtils.$t('biz.concreteBeam.tabPreCamber'),
                        get disabled() {
                            return !hasConfirmedDesignCoordinates.value ||
                                confirmedDesignIncludesCamber.value ||
                                !!designPreviewToken.value;
                        }
                    },
                    { name: 'setupSurvey', label: i18nUtils.$t('biz.concreteBeam.tabSetupSurvey'), disabled: false },
                    { name: 'pouringCoord', label: i18nUtils.$t('biz.concreteBeam.tabPouringCoord'), disabled: false },
                    { name: 'report', label: i18nUtils.$t('biz.concreteBeam.tabReport'), disabled: false }
                ];
            });

            // 项目信息表单
            var projectInfo = Vue.reactive({
                projectName: '',
                bridgeName: '',
                castingGroupName: '',
                managerName: '',
                offsetA: '',
                offsetB: '',
                offsetC: '',
                referenceElevation: '',
                createdBy: '',
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
            var designNormalize = Vue.ref(false);
            var designUseCalculation = Vue.ref(false);
            var designIncludeCamber = Vue.ref(false);
            // 设计坐标「核对与预览」：服务端只给数据，等宽版式由前端拼（还原原版界面）
            var designPreviewVisible = Vue.ref(false);
            var designPreviewLoading = Vue.ref(false);
            var designPreviewError = Vue.ref('');
            var designPreviewUpper = Vue.ref('');
            var designPreviewLower = Vue.ref('');

            // 预拱度表格
            var camberLoading = Vue.ref(false);
            var camberTableData = Vue.ref([]);
            var camberTotal = Vue.ref(0);
            var camberPageIndex = Vue.ref(1);
            var camberPageSize = Vue.ref(10);
            var camberPreviewToken = Vue.ref('');

            // ==================== 设站及测量（Setup Survey） ====================
            // 字段名与后端 DTO 一一对应，前端不再自定义别名（yElevation/seElevation 之类的映射全部取消）：
            //   segmentSurvey ← SetupNSurveyDTO（实测录入 + 标识 + 状态），提交时可整表单直传
            //   segmentSetup  ← ConcreteBeamPerfectCastDTO（只读立模参数/目标值）
            // 本页所有数值均为接口实时计算/回填，不设任何预设值。
            var segmentList = Vue.ref([]);
            var segmentIsFirst = Vue.ref(true);
            var upstreamSegmentName = Vue.ref('');
            var isPerfectCast = Vue.ref(false);
            // 勾选「精度达标」前的实测录入快照（取消勾选时凭它原样还原，见 onIsPerfectCastChange）
            var perfectCastSnapshot = null;

            // SetupNSurveyDTO 的数值录入字段（驼峰名）。首段与标准段字段集不同，故分两个清单；
            // 清单同时驱动"回填"与"提交"，字段增删只需改这里，不再有手工搬运。
            var SURVEY_FIELDS_FIRST = [
                'boltOffsetAN', 'boltOffsetCN', 'boltOffsetAS', 'boltOffsetCS',
                'surveyBulkheadEastElev', 'surveyBulkheadWestElev', 'surveyBulkheadCenterOffset',
                // 首段的活动端（匹配端）= 活动端模板 Floating Bulkhead
                'surveyMatchSouthEastElev', 'surveyMatchSouthWestElev', 'surveyMatchSouthCenterOffset',
                'surveyWetNorthEastElev', 'surveyWetNorthWestElev',
                'surveyWetSouthEastElev', 'surveyWetSouthWestElev',
                'surveyWetNorthCenterOffset', 'surveyWetSouthCenterOffset',
                // 首段专用：浇筑东西侧长度（对应算法 0 号段 E 的 WetCastEastLength / WetCastWestLength）
                'surveyWetEastLength', 'surveyWetWestLength'
            ];
            var SURVEY_FIELDS_STANDARD = [
                'boltOffsetAN', 'boltOffsetCN', 'boltOffsetAS', 'boltOffsetCS',
                'surveyBulkheadEastElev', 'surveyBulkheadWestElev', 'surveyBulkheadCenterOffset',
                'surveyMatchNorthEastElev', 'surveyMatchNorthWestElev',
                'surveyMatchSouthEastElev', 'surveyMatchSouthWestElev',
                'surveyMatchNorthCenterOffset', 'surveyMatchSouthCenterOffset',
                'surveyWetNorthEastElev', 'surveyWetNorthWestElev',
                'surveyWetSouthEastElev', 'surveyWetSouthWestElev',
                'surveyWetNorthCenterOffset', 'surveyWetSouthCenterOffset',
                // 标准段专用：浇筑中线长度（首段用东西侧长度，故不含此项）
                'surveyWetCenterLength'
            ];
            var SURVEY_FIELDS_ALL = SURVEY_FIELDS_FIRST.concat(SURVEY_FIELDS_STANDARD.filter(function (k) {
                return SURVEY_FIELDS_FIRST.indexOf(k) < 0;
            }));

            // ConcreteBeamPerfectCastDTO 的数值字段（与算法 CalcResult 同名，按归属端面分组）
            var SETUP_FIELDS = [
                // 活动端模板（首段 South 侧）；东西侧长度算法归在 WetCast 组
                'floatingBulkheadEastElev', 'floatingBulkheadWestElev', 'floatingBulkheadCenterOffset',
                'wetCastEastLength', 'wetCastWestLength', 'wetCastCenterLength',
                // 匹配端（上一梁段 as-cast 端面）：标准段只读区「匹配梁段设置参数」
                'matchNorthEastElev', 'matchNorthWestElev', 'matchSouthEastElev', 'matchSouthWestElev',
                'matchNorthCenterOffset', 'matchSouthCenterOffset', 'matchCenterLength', 'matchCastTwist',
                // 浇筑段（本段目标端面）：实测区「浇筑梁段」的理论值（首段为 0，按 IsFirst 走 Match*）
                'wetCastSouthEastElev', 'wetCastSouthWestElev', 'wetCastNorthEastElev', 'wetCastNorthWestElev',
                'wetCastSouthCenterOffset', 'wetCastNorthCenterOffset',
                // 固定端模板（North 侧）
                'fixedBulkheadEastElev', 'fixedBulkheadWestElev', 'fixedBulkheadCenterOffset'
            ];

            // 「精度达标」勾选时的实测区填充映射：key = segmentSurvey 字段（实测录入），value = segmentSetup 字段（理论值）。
            // 语义：勾选后，实测区展示"完美符合设计坐标时本该测到多少"，故直接用 St=0 的理论值覆盖。
            // 注意：aN/cN/aS/cS（测点偏位）没有理论值，不进映射，始终保持用户录入/回显值。
            var SURVEY_FROM_SETUP_FIRST = {
                // 活动端模板（实测）← 活动端模板目标值（Floating Bulkhead）
                surveyMatchSouthEastElev: 'floatingBulkheadEastElev',
                surveyMatchSouthWestElev: 'floatingBulkheadWestElev',
                surveyMatchSouthCenterOffset: 'floatingBulkheadCenterOffset',
                // 固定端模板（实测）← 固定端模板目标值（Fixed Bulkhead）
                surveyBulkheadEastElev: 'fixedBulkheadEastElev',
                surveyBulkheadWestElev: 'fixedBulkheadWestElev',
                surveyBulkheadCenterOffset: 'fixedBulkheadCenterOffset',
                // 浇筑梁段（实测）：首段无匹配段，算法 WetCast* 恒 0，故理论值取 Match*（＝本段设计端面）
                surveyWetNorthEastElev: 'matchNorthEastElev',
                surveyWetNorthWestElev: 'matchNorthWestElev',
                surveyWetSouthEastElev: 'matchSouthEastElev',
                surveyWetSouthWestElev: 'matchSouthWestElev',
                surveyWetNorthCenterOffset: 'matchNorthCenterOffset',
                surveyWetSouthCenterOffset: 'matchSouthCenterOffset',
                // 首段专用：浇筑东西侧长度
                surveyWetEastLength: 'wetCastEastLength',
                surveyWetWestLength: 'wetCastWestLength'
            };
            var SURVEY_FROM_SETUP_STANDARD = {
                // 匹配梁段（实测）← 匹配端设置参数（Match*）
                surveyMatchNorthEastElev: 'matchNorthEastElev',
                surveyMatchNorthWestElev: 'matchNorthWestElev',
                surveyMatchSouthEastElev: 'matchSouthEastElev',
                surveyMatchSouthWestElev: 'matchSouthWestElev',
                surveyMatchNorthCenterOffset: 'matchNorthCenterOffset',
                surveyMatchSouthCenterOffset: 'matchSouthCenterOffset',
                // 浇筑梁段（实测）← 浇筑段目标端面（WetCast*）
                surveyWetNorthEastElev: 'wetCastNorthEastElev',
                surveyWetNorthWestElev: 'wetCastNorthWestElev',
                surveyWetSouthEastElev: 'wetCastSouthEastElev',
                surveyWetSouthWestElev: 'wetCastSouthWestElev',
                surveyWetNorthCenterOffset: 'wetCastNorthCenterOffset',
                surveyWetSouthCenterOffset: 'wetCastSouthCenterOffset',
                surveyWetCenterLength: 'wetCastCenterLength',
                // 固定端模板（实测）← 固定端模板目标值（Fixed Bulkhead）
                surveyBulkheadEastElev: 'fixedBulkheadEastElev',
                surveyBulkheadWestElev: 'fixedBulkheadWestElev',
                surveyBulkheadCenterOffset: 'fixedBulkheadCenterOffset'
            };

            // 实测录入表单（字段名 = SetupNSurveyDTO 字段名）
            var segmentSurvey = Vue.reactive(newSurveyForm());
            // 只读区表单（字段名 = ConcreteBeamPerfectCastDTO 字段名）
            var segmentSetup = Vue.reactive(newSetupForm());

            // 按字段清单构造空表单：所有数值字段初始为空串，避免任何预设值混入
            function newSurveyForm() {
                var form = { castingGroupID: groupID };
                SURVEY_FIELDS_ALL.forEach(function (k) { form[k] = ''; });
                form.segmentID = 0;
                form.segmentName = '';
                form.segmentIndex = 0;
                form.node1 = '';
                form.node2 = '';
                form.isFirst = true;
                form.pouringDate = null;
                form.canEdit = false;
                form.lockedReason = '';
                // 算法产出（非录入）：匹配浇筑扭转
                form.surveyMatchCastTwist = '';
                return form;
            }
            function newSetupForm() {
                var form = { castingGroupID: groupID, girderID: 0, segmentName: '', segmentIndex: 0, isFirst: true };
                SETUP_FIELDS.forEach(function (k) { form[k] = ''; });
                return form;
            }

            // ========== 浇筑坐标页签 ==========
            // 数据全部由后端 GetPouringCoords 现取（读已落库的坐标表），此页签不再有硬编码假数据。
            // 梁段下拉与「设站及测量」同源：GetSegmentSurveyList。
            var pouringSegmentList = Vue.ref([]);
            // 视图模型：加载后即格式化为 4 位小数字符串，模板直接绑定
            var pouringView = Vue.reactive(newPouringView());

            function newPouringView() {
                return {
                    segmentName: '', segmentIndex: 0, isFirst: true, isFilled: false,
                    upstreamSegmentName: '', pouringDate: '',
                    // [{ faceName, nodeName, theoretical: [西/中/东 三行], asCast: [西/中/东 三行] }]
                    faces: [],
                    elevationPoints: [],   // 4 行：东北 / 西北 / 东南 / 西南
                    axisPoints: [],        // 2 行：北侧 / 南侧
                    theoreticalCenterLength: '', asCastCenterLength: '',
                    theoreticalTotalCenterLength: '', asCastTotalCenterLength: ''
                };
            }

            // hasData=false 时一律给空串：后端对"未产出"返回 0，直接 num4 会画成 0.0000，看起来像真值
            // 后端行标签/面名是中文常量，前端映射成 i18n key 后在模板里 $t 渲染（切语言即时生效）。
            var POURING_LABEL_KEYS = {
                '西侧控制点': 'westControlPoint',
                '中心线': 'centerline',
                '东侧控制点': 'eastControlPoint',
                '东北侧点位': 'boltNorthEast',
                '西北侧点位': 'boltNorthWest',
                '东南侧点位': 'boltSouthEast',
                '西南侧点位': 'boltSouthWest',
                '北侧点位': 'hairpinNorth',
                '南侧点位': 'hairpinSouth'
            };
            var POURING_FACE_KEYS = { '活动端': 'floatingBulkhead', '固定端': 'fixedBulkhead' };
            function toCoordRow(r, hasData) {
                var o = r || {};
                var labelKey = POURING_LABEL_KEYS[o.label] || '';
                if (!hasData) return { label: o.label || '', labelKey: labelKey, x: '', y: '', z: '' };
                return { label: o.label || '', labelKey: labelKey, x: num4(o.x), y: num4(o.y), z: num4(o.z) };
            }

            function applyPouringView(d) {
                var filled = !!d.isFilled;
                pouringView.segmentName = d.segmentName || '';
                pouringView.segmentIndex = d.segmentIndex || 0;
                pouringView.isFirst = !!d.isFirst;
                pouringView.isFilled = filled;
                pouringView.upstreamSegmentName = d.upstreamSegmentName || '';
                pouringView.pouringDate = d.pouringDate ? String(d.pouringDate).substring(0, 10) : '';
                // 面组：起始段两组（活动端 joint0 + 固定端 joint1），标准段一组（固定端 joint m+1）
                pouringView.faces = (d.faces || []).map(function (f) {
                    return {
                        faceName: f.faceName || '',
                        faceKey: POURING_FACE_KEYS[f.faceName] || '',
                        nodeName: f.nodeName || '',
                        hasAsCast: !!f.hasAsCast,
                        theoretical: (f.theoretical || []).map(function (r) { return toCoordRow(r, true); }),
                        asCast: (f.asCast || []).map(function (r) { return toCoordRow(r, !!f.hasAsCast); })
                    };
                });
                // 测点：未填报段无 measure 记录，整块显示空态
                pouringView.elevationPoints = (d.elevationPoints || []).map(function (r) { return toCoordRow(r, filled); });
                pouringView.axisPoints = (d.axisPoints || []).map(function (r) { return toCoordRow(r, filled); });
                // 中线长度：理论值恒有效；实测值需有浇筑坐标
                pouringView.theoreticalCenterLength = num4(d.theoreticalCenterLength);
                pouringView.theoreticalTotalCenterLength = num4(d.theoreticalTotalCenterLength);
                pouringView.asCastCenterLength = filled ? num4(d.asCastCenterLength) : '';
                pouringView.asCastTotalCenterLength = filled ? num4(d.asCastTotalCenterLength) : '';
            }

            function isConfigDirty() {
                return String(projectInfo.offsetA) !== String(savedConfig.offsetA) ||
                    String(projectInfo.offsetB) !== String(savedConfig.offsetB) ||
                    String(projectInfo.offsetC) !== String(savedConfig.offsetC) ||
                    String(projectInfo.referenceElevation) !== String(savedConfig.referenceElevation);
            }

            function hasUnsavedChanges() {
                return isConfigDirty() || !!designPreviewToken.value || !!camberPreviewToken.value;
            }

            async function confirmLeaveCurrentTab() {
                var dirty = (activeTab.value === 'projectInfo' && isConfigDirty()) ||
                    (activeTab.value === 'designCoord' && !!designPreviewToken.value) ||
                    (activeTab.value === 'preCamber' && !!camberPreviewToken.value);
                if (!dirty) return true;
                try {
                    await ElMessageBox.confirm(i18nUtils.$t('biz.concreteBeam.unsavedConfirm'), i18nUtils.$t('biz.concreteBeam.notice'), {
                        type: 'warning', confirmButtonText: '继续离开', cancelButtonText: '取消'
                    });
                } catch (e) { return false; }
                if (activeTab.value === 'projectInfo') restoreConfig();
                if (activeTab.value === 'designCoord') await cancelPreview('design', true);
                if (activeTab.value === 'preCamber') await cancelPreview('camber', true);
                return true;
            }

            async function onTabChange(tabName) {
                var tab = tabs.value.find(function (t) { return t.name === tabName; });
                if (tab && tab.disabled) return;
                if (tabName === activeTab.value) return;
                if (!await confirmLeaveCurrentTab()) return;
                activeTab.value = tabName;
                if (tabName === 'designCoord') loadDesignData();
                if (tabName === 'preCamber') loadCamberData();
                if (tabName === 'setupSurvey') loadSetupSurveySheet();
                if (tabName === 'pouringCoord') loadPouringCoordSheet();
                if (tabName === 'report') loadReportSheet();
            }

            function setDesignCoordinateDisabled(disabled) {
                var tab = tabs.value.find(function (item) { return item.name === 'designCoord'; });
                if (tab) tab.disabled = !!disabled;
                if (disabled && activeTab.value === 'designCoord') activeTab.value = 'projectInfo';
            }

            async function loadGroupInfo() {
                try {
                    var info = await HttpUtils.get('/Biz/ConcreteBeam/Config', { id: groupID });
                    projectInfo.projectName = info.projectName || '';
                    projectInfo.bridgeName = info.bridgeName || '';
                    projectInfo.castingGroupName = info.castingGroupName || '';
                    projectInfo.managerName = info.managerName || '';
                    projectInfo.offsetA = formatNullableNumber(info.offsetA);
                    projectInfo.offsetB = formatNullableNumber(info.offsetB);
                    projectInfo.offsetC = formatNullableNumber(info.offsetC);
                    projectInfo.referenceElevation = formatNullableNumber(info.referenceElevation);
                    projectInfo.createdBy = info.createdBy || '';
                    setDesignCoordinateDisabled(!info.isConfigured);
                    designIncludeCamber.value = !!info.elevationIncludesCamber;
                    confirmedDesignIncludesCamber.value = !!info.elevationIncludesCamber;
                    hasConfirmedDesignCoordinates.value = !!info.hasConfirmedDesignCoordinates;
                    hasCalculatedDesignCoordinates.value = !!info.hasCalculatedDesignCoordinates;
                    hasSurveyData.value = !!info.hasSurveyData;
                    if (tabs.value.find(function (item) { return item.name === 'preCamber'; }).disabled &&
                        activeTab.value === 'preCamber') activeTab.value = 'designCoord';
                    projectInfo.createdTime = formatDateTime(info.createdTime);
                    projectInfo.updatedTime = formatDateTime(info.updatedTime);
                    rememberConfig();
                    if (info.isConfigured) loadDesignData();
                } catch (e) {
                    setDesignCoordinateDisabled(true);
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
                    var previewing = !!designPreviewToken.value;
                    var res = await HttpUtils.post(previewing
                        ? '/Biz/ConcreteBeam/DesignImportPreviewList'
                        : '/Biz/ConcreteBeam/DesignCoordinateList', previewing ? {
                            castingGroupID: groupID, previewToken: designPreviewToken.value,
                            pageIndex: designPageIndex.value, pageSize: designPageSize.value
                        } : {
                            castingGroupID: groupID, pageIndex: designPageIndex.value, pageSize: designPageSize.value,
                            useCalculation: designUseCalculation.value,
                            displayIncludesCamber: designIncludeCamber.value
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
            async function onDesignNormalizeChange() {
                if (designNormalize.value && hasConfirmedDesignCoordinates.value &&
                    !confirmedDesignIncludesCamber.value && !hasCalculatedDesignCoordinates.value) {
                    designNormalize.value = false;
                    ElMessage.warning('还未上传预拱度信息，请先上传预拱度信息');
                    return;
                }
                if (designPreviewToken.value) return;
                designUseCalculation.value = designNormalize.value;
                designPageIndex.value = 1;
                await loadDesignData();
            }
            async function onDesignCamberChange() {
                if (designPreviewToken.value || !designUseCalculation.value) return;
                designPageIndex.value = 1;
                await loadDesignData();
            }
            function handleImportDesignCoord() { openImport('design'); }
            function downloadDesignTemplate() { window.location.href = '/Biz/ConcreteBeam/DesignCoordinateTemplate'; }
            function handleDesignPrint() { ElMessage.info(i18nUtils.$t('biz.concreteBeam.printTodo')); }

            // ===== 设计坐标「核对与预览」=====
            // 上半段：已计算坐标（含预拱度）；下半段：PrintReport 几何报告。
            // 服务端只回数据与列序，版式在前端用等宽字符拼，与原版界面一致。
            function monoPad(value, width)
            {
                var s = value == null ? '' : String(value);
                return s.length >= width ? s : new Array(width - s.length + 1).join(' ') + s;
            }
            function monoCenter(text, width)
            {
                var s = String(text);
                if (s.length >= width) return s;
                var left = Math.floor((width - s.length) / 2);
                return new Array(left + 1).join(' ') + s + new Array(width - s.length - left + 1).join(' ');
            }
            /** 数值统一 4 位小数；NaN / Upward / Check 等字符串原样右对齐。 */
            function monoNum(value, width)
            {
                if (value == null || value === '') return new Array(width + 1).join(' ');
                if (typeof value === 'number') return monoPad(value.toFixed(4), width);
                return monoPad(value, width);
            }

            function formatOrientationJoints(joints)
            {
                var w = 12, gap = ' ', groupW = w * 3 + 2;
                var lines = [];
                lines.push(monoPad('Joint#', 6) + '  '
                    + monoCenter('East Ctrl Point', groupW) + '  '
                    + monoCenter('CenterLine', groupW) + '  '
                    + monoCenter('West Ctrl Point', groupW));
                lines.push(new Array(6 + 2 + 1).join(' ')
                    + monoCenter('X', w) + gap + monoCenter('Y', w) + gap + monoCenter('Z', w) + '  '
                    + monoCenter('X', w) + gap + monoCenter('Y', w) + gap + monoCenter('Z', w) + '  '
                    + monoCenter('X', w) + gap + monoCenter('Y', w) + gap + monoCenter('Z', w));
                (joints || []).forEach(function (j, i) {
                    lines.push(monoPad(String(i + 1), 6) + '  '
                        + monoNum(j.xe, w) + gap + monoNum(j.ye, w) + gap + monoNum(j.ze, w) + '  '
                        + monoNum(j.xc, w) + gap + monoNum(j.yc, w) + gap + monoNum(j.zc, w) + '  '
                        + monoNum(j.xw, w) + gap + monoNum(j.yw, w) + gap + monoNum(j.zw, w));
                });
                return lines.join('\n');
            }

            /**
             * 括号小字：差值行用。value 非有限数（含服务端下发的 "NaN" 字符串）时整格留白。
             * 形如 "(0.0001 )"：括号内左对齐、右侧补空格，与原版界面一致。
             */
            function monoParen(value, width)
            {
                if (value == null || typeof value !== 'number' || !isFinite(value))
                    return new Array(width + 1).join(' ');
                var s = value.toFixed(4);
                var inner = width - 2;
                if (s.length >= inner) return '(' + s + ')';
                return '(' + s + new Array(inner - s.length + 1).join(' ') + ')';
            }
            /** 差值：计算值 - 基准值；计算值无效时返回 null（由 monoParen 留白）。 */
            function diffOrBlank(value, base)
            {
                return (typeof value === 'number' && isFinite(value)) ? value - base : null;
            }

            /**
             * 下半段版式。baseB = Project Properties 的 B：
             * B_East / B_West 的差值 = 计算值 - B；端面折角 East / West 的差值 = 计算值 - 90°。
             */
            function formatOrientationReport(report, baseB)
            {
                if (!report || !report.length) return '';
                var w = 11;                       // 数值列宽
                var pairW = w * 2 + 1;            // East/West 成对列宽
                var lines = [];
                lines.push(monoPad('Joint#', 6) + '  '
                    + monoPad('B_East', w) + '  ' + monoPad('B_West', w) + '  '
                    + monoCenter('Bulkhead Angle', pairW) + '  '
                    + monoCenter('Long. Offset', pairW) + '  '
                    + monoCenter('Cross-Slope', pairW) + '  '
                    + 'Z-Axis');
                var subPrefix = new Array(6 + 2 + w + 2 + w + 2 + 1).join(' ');
                var pairSub = monoCenter('East', w) + ' ' + monoCenter('West', w);
                lines.push(subPrefix + pairSub + '  ' + pairSub + '  ' + pairSub);
                var bNum = Number(baseB);
                var bBase = (baseB == null || baseB === '' || !isFinite(bNum)) ? null : bNum;
                for (var i = 1; i < report.length; i++)
                {
                    var r = report[i];
                    lines.push(monoPad(r[0] == null ? '' : String(r[0]), 6) + '  '
                        + monoNum(r[1], w) + '  ' + monoNum(r[2], w) + '  '
                        + monoNum(r[3], w) + ' ' + monoNum(r[4], w) + '  '
                        + monoNum(r[5], w) + ' ' + monoNum(r[6], w) + '  '
                        + monoNum(r[7], w) + ' ' + monoNum(r[8], w) + '  '
                        + (r[9] == null ? '' : String(r[9])));
                    // 紧随其后的括号小字行：B 差值（对 B）与折角差值（对 90°）
                    if (bBase != null)
                    {
                        lines.push(monoPad('', 6) + '  '
                            + monoParen(diffOrBlank(r[1], bBase), w) + '  '
                            + monoParen(diffOrBlank(r[2], bBase), w) + '  '
                            + monoParen(diffOrBlank(r[3], 90), w) + ' '
                            + monoParen(diffOrBlank(r[4], 90), w));
                    }
                }
                return lines.join('\n');
            }

            /**
             * 「坐标核对与预览」统一实现：General Coordinates 与 Camber 两个页签共用同一套。
             * 依据：① 手册里只有这一种 preview 形态；② design_coordinate_calculation 本身就是
             * source 叠加导入的 camber 之后的值，两个页签关心的其实是同一份"加了预拱度之后的坐标"。
             */

            /** 导入预览态（还没点确定）下 calculation 未更新，此时预览会给出旧数据，故拦下。 */
            function guardOrientationPreview()
            {
                if (designPreviewToken.value || camberPreviewToken.value)
                {
                    ElMessage.warning(i18nUtils.$t('biz.concreteBeam.orientationPreviewPending'));
                    return false;
                }
                return true;
            }

            async function fetchOrientationPreview()
            {
                var res = await HttpUtils.get('/Biz/ConcreteBeam/DesignOrientationPreview', { castingGroupID: groupID });
                return {
                    upper: formatOrientationJoints(res && res.joints),
                    lower: formatOrientationReport(res && res.report, projectInfo.offsetB)
                };
            }

            /** 把两段等宽文本写进新窗口出纸（预览窗与直接打印共用）。 */
            function printOrientationText(upper, lower)
            {
                var win = window.open('', '_blank');
                if (!win) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.printWindowBlocked')); return; }
                win.document.open();
                win.document.write('<!doctype html><html><head><meta charset="utf-8"><title></title><style>' +
                    'body{font-family:Consolas,"Courier New",monospace;font-size:12px;margin:16px;color:#303133}' +
                    'pre{margin:0 0 18px;line-height:1.35}@page{size:auto;margin:15mm}' +
                    '</style></head><body><pre>' + escapePrintHtml(upper) +
                    '</pre><pre>' + escapePrintHtml(lower) +
                    '</pre></body></html>');
                win.document.close();
                win.focus();
                win.print();
            }

            /** 预览：弹窗展示（两个页签的「预览」都走这里）。 */
            async function showOrientationPreview()
            {
                if (!guardOrientationPreview()) return;
                designPreviewVisible.value = true;
                designPreviewLoading.value = true;
                designPreviewError.value = '';
                designPreviewUpper.value = '';
                designPreviewLower.value = '';
                try
                {
                    var content = await fetchOrientationPreview();
                    designPreviewUpper.value = content.upper;
                    designPreviewLower.value = content.lower;
                }
                catch (e)
                {
                    designPreviewError.value = (e && e.message) || i18nUtils.$t('biz.concreteBeam.reportGenFailed');
                }
                finally { designPreviewLoading.value = false; }
            }

            /** 打印：跳过弹窗，直接取数出纸（两个页签的「打印」都走这里）。 */
            async function printOrientationPreview()
            {
                if (!guardOrientationPreview()) return;
                try
                {
                    var content = await fetchOrientationPreview();
                    printOrientationText(content.upper, content.lower);
                }
                catch (e)
                {
                    ElMessage.error((e && e.message) || i18nUtils.$t('biz.concreteBeam.reportGenFailed'));
                }
            }

            async function handleDesignPreview() { await showOrientationPreview(); }
            async function handleDesignPrint() { await printOrientationPreview(); }

            /** 弹窗内的 Print：已有内容就直接打，不重新取数。 */
            function handleDesignPreviewPrint()
            {
                if (!designPreviewUpper.value && !designPreviewLower.value) return;
                printOrientationText(designPreviewUpper.value, designPreviewLower.value);
            }
            async function handleDesignConfirm() {
                if (!ensureSurveyEditable(i18nUtils.$t('biz.concreteBeam.subjectDesign'))) return;
                if (designPreviewToken.value) {
                    await confirmPreview('design', false);
                    return;
                }
                designUseCalculation.value = designNormalize.value;
                designPageIndex.value = 1;
                await loadDesignData();
                ElMessage.success(i18nUtils.$t('biz.concreteBeam.designRefreshed'));
            }
            async function handleDesignCancel() {
                if (designPreviewToken.value) await cancelPreview('design', false);
                else {
                    designNormalize.value = designUseCalculation.value;
                    ElMessage.info(i18nUtils.$t('biz.concreteBeam.cancelled'));
                }
            }

            function loadCamberData() {
                camberLoading.value = true;
                var previewing = !!camberPreviewToken.value;
                HttpUtils.post(previewing ? '/Biz/ConcreteBeam/CamberImportPreviewList' : '/Biz/ConcreteBeam/CamberList', previewing ? {
                    castingGroupID: groupID, previewToken: camberPreviewToken.value,
                    pageIndex: camberPageIndex.value, pageSize: camberPageSize.value
                } : { castingGroupID: groupID, pageIndex: camberPageIndex.value, pageSize: camberPageSize.value })
                    .then(function (res) {
                        camberTableData.value = (res.list || []).map(function (row) {
                            ['ze','zc','zw'].forEach(function (key) { row[key] = formatNullableNumber(row[key]); }); return row;
                        }); camberTotal.value = res.total || 0;
                    }).catch(function (e) { console.error('loadCamberData error', e); })
                    .finally(function () { camberLoading.value = false; });
            }

            function handleCamberPageChange(val) { camberPageIndex.value = val; loadCamberData(); }
            function handleCamberSizeChange(val) { camberPageSize.value = val; camberPageIndex.value = 1; loadCamberData(); }
            function handleImportCamber() { openImport('camber'); }
            function downloadCamberTemplate() { window.location.href = '/Biz/ConcreteBeam/CamberTemplate'; }
            function escapePrintHtml(value) {
                return String(value == null || value === '' ? '-' : value)
                    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
                    .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
            }
            // 预拱度页签的「打印 / 预览」与 General Coordinates 完全一致：
            // 预拱度本身没有独立的预览形态，它已叠加进 design_coordinate_calculation，故共用坐标核对预览
            async function handleCamberPreview() { await showOrientationPreview(); }
            async function handleCamberPrint() { await printOrientationPreview(); }
            async function handleCamberConfirm() {
                if (!ensureSurveyEditable(i18nUtils.$t('biz.concreteBeam.subjectCamber'))) return;
                if (!camberPreviewToken.value) { ElMessage.info(i18nUtils.$t('biz.concreteBeam.importCamberFirst')); return; }
                await confirmPreview('camber', false);
            }
            async function handleCamberCancel() {
                if (camberPreviewToken.value) await cancelPreview('camber', false);
                else ElMessage.info(i18nUtils.$t('biz.concreteBeam.cancelled'));
            }


            /* 浇筑坐标页签 function 开始 */
            // 进入页签：载入全量梁段列表（与「设站及测量」同源），默认展示最后一个已填报的梁段。
            // 与设站及测量取向不同：本页是"看结果"，那边是"填数据"，故默认段取最近浇筑的一段。
            async function loadPouringCoordSheet() {
                try {
                    var res = await HttpUtils.get('/Biz/ConcreteBeam/GetSegmentSurveyList', { castingGroupID: groupID });
                    var list = (res && res.list) || [];
                    pouringSegmentList.value = list;
                    var cur = list.find(function (s) { return s.segmentName === pouringView.segmentName; });
                    if (!cur) {
                        var filledList = list.filter(function (s) { return s.isFilled; });
                        cur = filledList.length ? filledList[filledList.length - 1] : list[0];
                    }
                    if (!cur) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.noDisplayableSegment')); return; }
                    pouringView.segmentName = cur.segmentName;
                    await loadPouringCoords(cur.segmentName);
                } catch (e) { console.error('load pouring segment list error', e); }
            }

            // 切换梁段：重新取该段坐标
            async function onPouringSegmentChange() {
                if (!pouringView.segmentName) return;
                await loadPouringCoords(pouringView.segmentName);
            }

            // 取某梁段的端面坐标 + 定位点信息（后端只读已落库坐标表，不重跑算法）
            async function loadPouringCoords(segmentName) {
                try {
                    var d = await HttpUtils.get('/Biz/ConcreteBeam/GetPouringCoords', {
                        castingGroupID: groupID, segmentName: segmentName
                    });
                    applyPouringView(d || {});
                } catch (e) { console.error('load pouring coords error', e); }
            }

            function handlePouringPrint() { ElMessage.info(i18nUtils.$t('biz.concreteBeam.printTodo')); }
            function handlePouringPreview() { ElMessage.info('预览功能待开发'); }
            function handlePouringCancel() { ElMessage.info(i18nUtils.$t('biz.concreteBeam.cancelled')); }

            // ========== 报告页签 ==========
            // 报告区有两组单选（整体报告 / 单梁段报告），底部按钮是共用的，
            // 故用 scope 记录当前生效的是哪一组，取数/校验/导出都按它分派
            var reportScope = Vue.ref('overall');
            var reportOverallType = Vue.ref('理论坐标');
            var reportIncludeAll = Vue.ref(true);
            var reportSegmentList = Vue.ref([]);
            var reportSegmentName = Vue.ref('');
            var reportSegmentType = Vue.ref('已浇筑梁段');
            var reportLoading = Vue.ref(false);
            var reportExporting = Vue.ref(false);
            var reportUnavailableHint = Vue.ref('');

            // 预览对话框：服务端出 PDF，PDF.js 渲染到 canvas
            var reportDialogVisible = Vue.ref(false);
            var reportDialogTitle = Vue.ref(i18nUtils.$t('biz.concreteBeam.reportPreviewTitle'));
            var reportError = Vue.ref('');
            var reportPageNo = Vue.ref(1);
            var reportPageCount = Vue.ref(0);
            var reportZoom = Vue.ref(1.2);
            var reportCanvas = Vue.ref(null);
            // 当前已取回的报告：缓存后供翻页/缩放/打印/导出复用，切换梁段或报告类型时作废
            var reportPdfDoc = null;
            var reportPdfBlob = null;
            // 由外部页签（设站及测量「打印」）指定的报告地址；非空时优先于页签内的单选项
            var reportUrlOverride = Vue.ref('');
            // 外部指定地址时配套的文件名（不含扩展名），页签内走自己的命名规则
            var reportDownloadName = Vue.ref('');

            // PDF.js 的 worker 路径由视图注入（避免虚拟目录下路径拼错）
            if (window.pdfjsLib && window.__pdfWorkerSrc) {
                window.pdfjsLib.GlobalWorkerOptions.workerSrc = window.__pdfWorkerSrc;
            }

            /** 界面单选 -> 后端 reportType：梁段设置=setting，已浇筑梁段=baogao。 */
            function currentReportType() {
                return reportSegmentType.value === '已浇筑梁段' ? 'baogao' : 'setting';
            }

            /** 整体报告：界面单选 -> 后端 reportType。 */
            function currentOverallReportType() {
                if (reportOverallType.value === '项目信息') return 'project';
                if (reportOverallType.value === '浇筑和理论坐标') return 'casttheory';
                return 'theory';
            }

            /** 整体报告的单选/勾选变化：切到整体报告并作废已缓存的 PDF（includeAll 也影响取数）。 */
            function onReportOverallChange() {
                reportScope.value = 'overall';
                invalidateReportCache();
            }

            function reportKindSuffix() { return currentReportType(); }

            /**
             * 报告接口 URL。GET，可被 fetch（预览/导出/打印）直接复用。
             * 外部（如设站及测量的「打印」）可临时指定 reportUrlOverride，
             * 弹窗共用同一套 PDF 预览/翻页/缩放/导出/打印能力，不另建一套。
             */
            function buildReportUrl() {
                if (reportUrlOverride.value) return reportUrlOverride.value;
                if (reportScope.value === 'overall') {
                    return '/Biz/ConcreteBeam/OverallReportPdf'
                        + '?castingGroupID=' + encodeURIComponent(groupID)
                        + '&reportType=' + encodeURIComponent(currentOverallReportType())
                        + '&includeAll=' + (reportIncludeAll.value ? 'true' : 'false');
                }
                return '/Biz/ConcreteBeam/SegmentReportPdf'
                    + '?castingGroupID=' + encodeURIComponent(groupID)
                    + '&segmentName=' + encodeURIComponent(reportSegmentName.value || '')
                    + '&reportType=' + encodeURIComponent(currentReportType());
            }

            /** 当前选中的梁段对象（未找到时返回 null）。 */
            function currentReportSegment() {
                return reportSegmentList.value.find(function (s) { return s.segmentName === reportSegmentName.value; }) || null;
            }

            /** 丢弃已取回的报告（切换梁段/切换报告类型/取消时调用）。 */
            function invalidateReportCache() {
                if (reportPdfDoc && typeof reportPdfDoc.destroy === 'function') {
                    try { reportPdfDoc.destroy(); } catch (e) { /* 释放失败不影响使用 */ }
                }
                reportPdfDoc = null;
                reportPdfBlob = null;
                reportPageCount.value = 0;
                reportPageNo.value = 1;
                reportError.value = '';
            }

            /** 进入页签：载入梁段列表（与「设站及测量」同一数据源），默认选中最后一个已填报段。 */
            async function loadReportSheet() {
                try {
                    var res = await HttpUtils.get('/Biz/ConcreteBeam/GetSegmentSurveyList', { castingGroupID: groupID });
                    var list = (res && res.list) || [];
                    var options = list.map(function (s) {
                        return {
                            segmentName: s.segmentName,
                            label: s.segmentName
                                + '(' + i18nUtils.$t('biz.concreteBeam.' + (s.isFirst ? 'tagStarter' : 'tagStandard')) + ')'
                                + (s.isFilled ? ' · ' + i18nUtils.$t('biz.concreteBeam.tagFilled') : ' · ' + i18nUtils.$t('biz.concreteBeam.tagUnfilled')),
                            isFilled: !!s.isFilled
                        };
                    });
                    reportSegmentList.value = options;
                    if (!options.length) { reportSegmentName.value = ''; refreshReportHint(); return; }

                    var keep = options.some(function (s) { return s.segmentName === reportSegmentName.value; });
                    if (!keep) {
                        // 本页是"看结果"，与设站及测量的"下一个待填报"取向相反，故默认取最后一个已填报段
                        var filled = list.filter(function (s) { return s.isFilled; });
                        reportSegmentName.value = (filled.length ? filled[filled.length - 1] : list[list.length - 1]).segmentName;
                    }
                    refreshReportHint();
                } catch (e) { console.error('load report sheet error', e); }
            }

            /** 「已浇筑梁段」报告要求本段已填报，未填报时给出提示。 */
            function refreshReportHint() {
                var cur = currentReportSegment();
                reportUnavailableHint.value = (cur && !cur.isFilled && reportSegmentType.value === '已浇筑梁段')
                    ? i18nUtils.$t('biz.concreteBeam.reportNotFilledHint')
                    : '';
            }

            function onReportSegmentChange() {
                reportScope.value = 'segment';
                invalidateReportCache();
                refreshReportHint();
            }

            /** 报告类型单选变化：缓存作废 + 刷新提示。 */
            function onReportTypeChange() {
                reportScope.value = 'segment';
                invalidateReportCache();
                refreshReportHint();
            }

            /** 前置校验：没选梁段、或选了未填报段却要看「已浇筑梁段」时拦下。 */
            function ensureReportReady() {
                // 外部页签指定的地址已自带全部参数（段名与类型在打开弹窗时就定好了），无需再校验页签内的单选
                if (reportUrlOverride.value) return true;
                // 整体报告不针对某个梁段，只要浇筑组本身可访问即可生成
                if (reportScope.value === 'overall') return true;
                if (!reportSegmentName.value) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.selectSegmentFirst')); return false; }
                var cur = currentReportSegment();
                if (reportSegmentType.value === '已浇筑梁段' && cur && !cur.isFilled) {
                    ElMessage.warning(i18nUtils.$t('biz.concreteBeam.reportNotFilledBlock'));
                    return false;
                }
                return true;
            }

            /**
             * 取报告字节。用原生 fetch 而不是 HttpUtils，因为要拿 Blob；
             * 出错时后端返回 400 + JSON，此处按 Content-Type 区分后把 message 抛出去，
             * 否则 PDF.js 对着一段 HTML 只会报一句看不懂的解析失败。
             */
            async function fetchReportBlob() {
                var token = localStorage.getItem('authToken');
                var resp = await fetch(buildReportUrl(), {
                    headers: token ? { 'Authorization': 'Bearer ' + token } : {}
                });
                var contentType = resp.headers.get('content-type') || '';
                if (!resp.ok || contentType.indexOf('application/pdf') < 0) {
                    var msg = i18nUtils.$t('biz.concreteBeam.reportGenFailed') + ' (HTTP ' + resp.status + ')';
                    try {
                        var body = await resp.json();
                        if (body && body.message) msg = body.message;
                    } catch (e) { /* 非 JSON 响应，沿用默认文案 */ }
                    throw new Error(msg);
                }
                return await resp.blob();
            }

            async function handleReportPreview() {
                if (!ensureReportReady()) return;
                invalidateReportCache();
                // 页签内的预览走自己的单选项，清掉外部指定的地址
                reportUrlOverride.value = '';
                reportDownloadName.value = '';
                var overallLabel = reportOverallType.value === '项目信息' ? i18nUtils.$t('biz.concreteBeam.reportProject')
                    : (reportOverallType.value === '浇筑和理论坐标' ? i18nUtils.$t('biz.concreteBeam.reportCastTheory')
                        : i18nUtils.$t('biz.concreteBeam.reportTheory'));
                reportDialogTitle.value = reportScope.value === 'overall'
                    ? (i18nUtils.$t('biz.concreteBeam.overallReports') + ' · ' + overallLabel
                       + (reportOverallType.value === '理论坐标' && reportIncludeAll.value ? i18nUtils.$t('biz.concreteBeam.includeAllSuffix') : ''))
                    : (reportSegmentName.value + ' · '
                       + i18nUtils.$t('biz.concreteBeam.' + (reportSegmentType.value === '已浇筑梁段' ? 'reportSegmentAsCastReport' : 'reportSegmentSetupReport')));
                reportDialogVisible.value = true;
                await renderReportPage(1);
            }

            /** 首次渲染时取数并 loadDocument，之后翻页/缩放只走 getPage。 */
            async function renderReportPage(pageNo) {
                if (!window.pdfjsLib) {
                    reportError.value = i18nUtils.$t('biz.concreteBeam.pdfLibMissing');
                    return;
                }
                reportLoading.value = true;
                try {
                    if (!reportPdfDoc) {
                        if (!reportPdfBlob) reportPdfBlob = await fetchReportBlob();
                        // 每次都取新的 ArrayBuffer：PDF.js 会把它 transfer 走
                        var buffer = await reportPdfBlob.arrayBuffer();
                        reportPdfDoc = await window.pdfjsLib.getDocument({ data: buffer }).promise;
                        reportPageCount.value = reportPdfDoc.numPages;
                    }
                    reportPageNo.value = Math.min(Math.max(pageNo, 1), reportPageCount.value || 1);
                    var page = await reportPdfDoc.getPage(reportPageNo.value);
                    var viewport = page.getViewport({ scale: reportZoom.value });
                    var canvas = reportCanvas.value;
                    if (!canvas) return;
                    var ctx = canvas.getContext('2d');
                    canvas.width = Math.floor(viewport.width);
                    canvas.height = Math.floor(viewport.height);
                    canvas.style.width = Math.floor(viewport.width) + 'px';
                    await page.render({ canvasContext: ctx, viewport: viewport }).promise;
                } catch (e) {
                    reportError.value = (e && e.message) || i18nUtils.$t('biz.concreteBeam.reportGenFailed');
                    reportPdfDoc = null;
                    reportPdfBlob = null;
                } finally {
                    reportLoading.value = false;
                }
            }

            async function reportPrevPage() { if (reportPageNo.value > 1) await renderReportPage(reportPageNo.value - 1); }
            async function reportNextPage() { if (reportPageNo.value < reportPageCount.value) await renderReportPage(reportPageNo.value + 1); }
            async function reportZoomIn() { reportZoom.value = Math.min(3, Number((reportZoom.value + 0.2).toFixed(2))); await renderReportPage(reportPageNo.value); }
            async function reportZoomOut() { reportZoom.value = Math.max(0.5, Number((reportZoom.value - 0.2).toFixed(2))); await renderReportPage(reportPageNo.value); }

            /** 打印：隐藏 iframe 载入同一份 PDF 后调浏览器打印（与预览共用已取回的字节）。 */
            async function handleReportPrint() {
                if (!ensureReportReady()) return;
                reportLoading.value = true;
                try {
                    if (!reportPdfBlob) reportPdfBlob = await fetchReportBlob();
                    var url = URL.createObjectURL(reportPdfBlob);
                    var frame = document.createElement('iframe');
                    frame.style.position = 'fixed';
                    frame.style.right = '0';
                    frame.style.bottom = '0';
                    frame.style.width = '0';
                    frame.style.height = '0';
                    frame.style.border = '0';
                    frame.onload = function () {
                        try {
                            frame.contentWindow.focus();
                            frame.contentWindow.print();
                        } catch (e) {
                            ElMessage.warning(i18nUtils.$t('biz.concreteBeam.printBlocked'));
                        }
                        // 打印对话框是模态的，这里延后回收；PDF 文档较大，给它足够时间
                        setTimeout(function () {
                            if (frame.parentNode) frame.parentNode.removeChild(frame);
                            URL.revokeObjectURL(url);
                        }, 60000);
                    };
                    frame.src = url;
                    document.body.appendChild(frame);
                } catch (e) {
                    ElMessage.error((e && e.message) || i18nUtils.$t('biz.concreteBeam.reportGenFailed'));
                } finally {
                    reportLoading.value = false;
                }
            }

            /** 导出：把同一份字节另存为 .pdf。 */
            async function handleReportExport() {
                if (!ensureReportReady()) return;
                reportExporting.value = true;
                try {
                    if (!reportPdfBlob) reportPdfBlob = await fetchReportBlob();
                    var url = URL.createObjectURL(reportPdfBlob);
                    var link = document.createElement('a');
                    link.href = url;
                    link.download = reportUrlOverride.value
                        ? (reportDownloadName.value || 'report') + '.pdf'
                        : (reportScope.value === 'overall'
                            ? ('Overall-' + currentOverallReportType()
                               + (reportOverallType.value === '理论坐标' && reportIncludeAll.value ? '-all' : '') + '.pdf')
                            : (reportSegmentName.value + '-' + reportKindSuffix() + '.pdf'));
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    setTimeout(function () { URL.revokeObjectURL(url); }, 10000);
                } catch (e) {
                    ElMessage.error((e && e.message) || i18nUtils.$t('biz.concreteBeam.reportGenFailed'));
                } finally {
                    reportExporting.value = false;
                }
            }

            function handleReportCancel() {
                invalidateReportCache();
                reportDialogVisible.value = false;
                ElMessage.info(i18nUtils.$t('biz.concreteBeam.cancelled'));
            }

            async function handleConfirm() {
                if (!ensureSurveyEditable(i18nUtils.$t('biz.concreteBeam.subjectConfig'))) return;
                if (String(projectInfo.offsetA).trim() === '') { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.offsetAPlaceholder')); return; }
                if (String(projectInfo.offsetB).trim() === '') { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.offsetBPlaceholder')); return; }
                if (String(projectInfo.offsetC).trim() === '') { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.offsetCPlaceholder')); return; }
                if (String(projectInfo.referenceElevation).trim() === '') { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.referenceElevationPlaceholder')); return; }
                var fields = [projectInfo.offsetA, projectInfo.offsetB, projectInfo.offsetC, projectInfo.referenceElevation];
                if (fields.some(function (v) { return !isFourDecimalNumber(v); })) {
                    ElMessage.warning(i18nUtils.$t('biz.concreteBeam.offsetNumericRule')); return;
                }
                if (Number(projectInfo.offsetA) <= 0 || Number(projectInfo.offsetB) <= 0 || Number(projectInfo.offsetC) <= 0) {
                    ElMessage.warning(i18nUtils.$t('biz.concreteBeam.offsetPositiveRule')); return;
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
                    projectInfo.createdBy = info.createdBy || projectInfo.createdBy;
                    projectInfo.createdTime = formatDateTime(info.createdTime);
                    projectInfo.updatedTime = formatDateTime(info.updatedTime);
                    rememberConfig();
                    setDesignCoordinateDisabled(!info.isConfigured);
                    ElMessage.success(i18nUtils.$t('biz.concreteBeam.saveSuccess'));
                    if (info.calculationMessage) ElMessage.warning(info.calculationMessage);
                } catch (e) { console.error('save config error', e); }
            }

            function handleCancel() { restoreConfig(); }

            function handleReset() { restoreConfig(); }

            /**
             * 已开始实测填报后，上游输入（项目信息 / 设计坐标 / 预拱度）不允许再改。
             * 按钮已在视图层禁用，这里是防绕过（键盘操作、残留的异步回调等），
             * 服务端对同一个请求也会拒绝，故此处提示与服务端文案保持一致。
             */
            function ensureSurveyEditable(subject) {
                if (!hasSurveyData.value) return true;
                ElMessage.warning(i18nUtils.$t('biz.concreteBeam.surveyLockedAction', { subject: subject }));
                return false;
            }

            function openImport(type) {
                var locked = type === 'design' ? i18nUtils.$t('biz.concreteBeam.subjectDesign')
                    : (type === 'camber' ? i18nUtils.$t('biz.concreteBeam.subjectCamber') : null);
                if (locked && !ensureSurveyEditable(locked)) return;
                importType.value = type;
                importFile.value = null;
                if (uploadRef.value) uploadRef.value.clearFiles();
                importDialogVisible.value = true;
            }
            function onFileChange(file) { importFile.value = file; }
            function onFileRemove() { importFile.value = null; }
            function onFileExceed() { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.onlyOneFile')); }

            async function confirmImport() {
                if (!importFile.value || !importFile.value.raw) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.selectFile')); return; }
                var raw = importFile.value.raw;
                if (!/\.xlsx$/i.test(raw.name)) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.onlyXlsx')); return; }
                if (raw.size > 20 * 1024 * 1024) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.fileTooLarge')); return; }
                await importExcel(importType.value, raw);
            }

            async function importExcel(type, file) {
                var form = new FormData(); form.append('file', file);
                var url = type === 'segments' ? '/Biz/ConcreteBeam/ImportSegments' :
                    (type === 'design' ? '/Biz/ConcreteBeam/ImportDesignCoordinates' : '/Biz/ConcreteBeam/ImportCamber');
                importLoading.value = true;
                try {
                    var importUrl = url + '?castingGroupID=' + groupID;
                    if (type === 'design') importUrl += '&elevationIncludesCamber=' + designIncludeCamber.value;
                    var response = await axios.post(importUrl,
                        form, { responseType: 'blob', headers: { 'Content-Type': 'multipart/form-data' }, timeout: 120000 });
                    var contentType = response.headers['content-type'] || '';
                    if (contentType.indexOf('application/json') < 0) {
                        downloadBlob(response.data, getDownloadName(response.headers) || i18nUtils.$t('biz.concreteBeam.errorDetailFile'));
                        ElMessage.error(i18nUtils.$t('biz.concreteBeam.importFailedDetail'));
                        return;
                    }
                    var json = JSON.parse(await response.data.text());
                    if (json.code !== 200) { ElMessage.error(json.message || '导入失败'); return; }
                    var result = json.data || {};
                    if (type === 'design' || type === 'camber') {
                        var previousToken = type === 'design' ? designPreviewToken.value : camberPreviewToken.value;
                        if (previousToken) await removePreviewToken(type, previousToken);
                        if (type === 'design') designPreviewToken.value = result.previewToken || '';
                        else camberPreviewToken.value = result.previewToken || '';
                    }
                    importDialogVisible.value = false;
                    ElMessage.success(result.message);
                    if (type === 'segments') { segmentPageIndex.value = 1; loadSegmentData(); }
                    else if (type === 'design') {
                        designPageIndex.value = 1;
                        loadDesignData();
                    }
                    else { camberPageIndex.value = 1; loadCamberData(); }
                } catch (e) {
                    ElMessage.error(e && e.message ? e.message : '导入失败');
                } finally { importLoading.value = false; }
            }

            async function confirmPreview(type, confirmOverwrite) {
                var token = type === 'design' ? designPreviewToken.value : camberPreviewToken.value;
                if (!token) return;
                var url = type === 'design' ? '/Biz/ConcreteBeam/ConfirmDesignImport' : '/Biz/ConcreteBeam/ConfirmCamberImport';
                try {
                    var result = await HttpUtils.post(url, {
                        castingGroupID: groupID, previewToken: token, confirmOverwrite: !!confirmOverwrite
                    });
                    if (result.requiresOverwriteConfirmation) {
                        try {
                            await ElMessageBox.confirm(result.message, '确认覆盖', {
                                type: 'warning', confirmButtonText: '确认覆盖', cancelButtonText: '取消'
                            });
                            await confirmPreview(type, true);
                        } catch (e) { }
                        return;
                    }
                    if (type === 'design') {
                        designPreviewToken.value = '';
                        designUseCalculation.value = designNormalize.value;
                        designPageIndex.value = 1;
                        await loadGroupInfo();
                        await loadDesignData();
                        await loadSegmentData();
                        await loadCamberData();
                    } else {
                        camberPreviewToken.value = '';
                        hasCalculatedDesignCoordinates.value = true;
                        camberPageIndex.value = 1;
                        designUseCalculation.value = designNormalize.value;
                        await loadCamberData();
                        await loadDesignData();
                    }
                    if (result.message === '请先导入预拱度')
                        ElMessage.warning(i18nUtils.$t('biz.concreteBeam.designSavedUploadCamber'));
                    else ElMessage.success(result.message || '保存成功');
                } catch (e) { console.error('confirm import preview error', e); }
            }

            async function removePreviewToken(type, token) {
                if (!token) return;
                try {
                    await HttpUtils.post('/Biz/ConcreteBeam/CancelImportPreview', {
                        castingGroupID: groupID, previewToken: token, importType: type
                    });
                } catch (e) { console.error('cancel import preview error', e); }
            }

            async function cancelPreview(type, silent) {
                var token = type === 'design' ? designPreviewToken.value : camberPreviewToken.value;
                await removePreviewToken(type, token);
                if (type === 'design') {
                    designPreviewToken.value = '';
                    designPageIndex.value = 1;
                    await loadDesignData();
                } else {
                    camberPreviewToken.value = '';
                    camberPageIndex.value = 1;
                    await loadCamberData();
                }
                if (!silent) ElMessage.info(i18nUtils.$t('biz.concreteBeam.cancelled'));
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

            function handleBeforeUnload(event) {
                if (!hasUnsavedChanges()) return;
                event.preventDefault();
                event.returnValue = i18nUtils.$t('biz.concreteBeam.unsavedConfirm');
                return event.returnValue;
            }

            async function requestPlatformTabClose() {
                if (!hasUnsavedChanges()) return true;
                try {
                    await ElMessageBox.confirm(i18nUtils.$t('biz.concreteBeam.unsavedConfirm'), i18nUtils.$t('biz.concreteBeam.notice'), {
                        type: 'warning', confirmButtonText: '继续离开', cancelButtonText: '取消'
                    });
                } catch (e) { return false; }
                restoreConfig();
                if (designPreviewToken.value) {
                    await removePreviewToken('design', designPreviewToken.value);
                    designPreviewToken.value = '';
                }
                if (camberPreviewToken.value) {
                    await removePreviewToken('camber', camberPreviewToken.value);
                    camberPreviewToken.value = '';
                }
                return true;
            }

            /* Setup Survey function 开始 */
            // 数值格式化：后端 decimal 统一转 4 位小数字符串，空值保持空串
            function num4(v) { return v === null || v === undefined || v === '' ? '' : Number(v).toFixed(4); }

            // 进入页签：载入全量梁段列表（每段自带 IsFirst/IsFilled/CanEdit/LockedReason/Upstream），
            // 默认选中"下一个待填报"的段。用全量接口而非 GetSegmentToSurvey（后者只返回可操作段，
            // 会使下拉项与 cur.isFilled/cur.upstream 查询落空）。
            async function loadSetupSurveySheet() {
                try {
                    var res = await HttpUtils.get('/Biz/ConcreteBeam/GetSegmentSurveyList', { castingGroupID: groupID });
                    var list = (res && res.list) || [];
                    segmentList.value = list;
                    var next = res ? res.nextSegmentIndex : -1;
                    var def = null;
                    if (next !== null && next !== undefined && next >= 0) {
                        def = list.find(function (x) { return x.segmentIndex === next; });
                    }
                    if (!def) def = list[0];
                    segmentSurvey.segmentName = def ? def.segmentName : '';
                    if (!segmentSurvey.segmentName) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.noFillableSegment')); return; }
                    isPerfectCast.value = false;
                    perfectCastSnapshot = null;      // 换段即作废预览快照，避免还原到别的段
                    await loadSegmentSurvey(segmentSurvey.segmentName);
                } catch (e) { console.error('load survey segment list error', e); }
            }

            // 切换梁段：重新回填该段的实测数据与立模参数
            async function onSegmentChange() {
                if (!segmentSurvey.segmentName) return;
                isPerfectCast.value = false;
                perfectCastSnapshot = null;          // 换段即作废预览快照，避免还原到别的段
                await loadSegmentSurvey(segmentSurvey.segmentName);
            }

            // 回填单个梁段：实测录入值（GetSurvey）+ 只读目标值（PerfectCast，St=0 现算）
            async function loadSegmentSurvey(segmentName) {
                try {
                    var d = await HttpUtils.get('/Biz/ConcreteBeam/GetSurvey', {
                        castingGroupID: groupID, segmentNo: segmentName
                    });
                    // 先清空全部录入字段，避免切换梁段时残留上一段的值（首段/标准段字段集不同）
                    SURVEY_FIELDS_ALL.forEach(function (k) { segmentSurvey[k] = ''; });

                    // 标识与状态：字段名与 SetupNSurveyDTO 一致，直接回填
                    segmentSurvey.segmentID = d.segmentID || 0;
                    segmentSurvey.segmentName = d.segmentName || segmentName;
                    segmentSurvey.segmentIndex = d.segmentIndex || 0;
                    segmentSurvey.node1 = d.node1 || '';
                    segmentSurvey.node2 = d.node2 || '';
                    segmentSurvey.isFirst = !!d.isFirst;
                    segmentSurvey.canEdit = !!d.canEdit;
                    segmentSurvey.lockedReason = d.lockedReason || '';
                    segmentSurvey.pouringDate = d.pouringDate ? new Date(d.pouringDate) : null;
                    // 匹配浇筑扭转：算法产出（非录入），仅回显
                    segmentSurvey.surveyMatchCastTwist = num4(d.surveyMatchCastTwist);
                    segmentIsFirst.value = segmentSurvey.isFirst;
                    // 匹配浇筑（上游梁段名）：SetupNSurveyDTO 无此字段，由列表携带的 Upstream 提供，仅展示
                    var cur = segmentList.value.find(function (x) { return x.segmentName === segmentName; });
                    upstreamSegmentName.value = (cur && cur.upstream) ? cur.upstream : '';

                    // 是否已填报：GetSurvey 未填报时返回默认 DTO（SegmentID=0、数值全为 0），
                    // 不能靠"值是否为空"判断，否则 num4(0) 会把"没数据"画成"实测值 0.0000"。
                    var filled = (d.segmentID || 0) > 0 || !!(cur && cur.isFilled);

                    // 录入字段：按本段类型对应的字段清单批量回填（字段名与后端一致，无手工搬运）。
                    // 未填报段显式清空为空串，避免把后端的 0 当成真实实测值展示。
                    var fields = segmentIsFirst.value ? SURVEY_FIELDS_FIRST : SURVEY_FIELDS_STANDARD;
                    fields.forEach(function (k) { segmentSurvey[k] = filled ? num4(d[k]) : ''; });

                    // 只读区的立模参数始终由 St=0 现算，不读本段保存过的实测修正值
                    await loadSegmentSetup(segmentName);

                    // 实测区不做任何预填：理论值只在勾选「精度达标」时由 applyPerfectCastToSurvey() 展示，
                    // 未勾选时一律留空（未填报段）或回显库中实测值（已填报段），确保测量员录入的是真实测量结果。
                } catch (e) { console.error('load survey error', e); }
            }

            // 立模参数（只读区）：基于上一梁段实测 + 当前梁段设计的理论目标值（St=0）
            // 字段名与 ConcreteBeamPerfectCastDTO 一致，故整块按清单回填
            async function loadSegmentSetup(segmentName) {
                try {
                    var r = await HttpUtils.get('/Biz/ConcreteBeam/PerfectCast', {
                        castingGroupID: groupID, segmentNo: segmentName
                    });
                    segmentSetup.castingGroupID = r.castingGroupID || groupID;
                    segmentSetup.girderID = r.girderID || 0;
                    segmentSetup.segmentName = r.segmentName || segmentName;
                    segmentSetup.segmentIndex = r.segmentIndex || 0;
                    segmentSetup.isFirst = !!r.isFirst;
                    SETUP_FIELDS.forEach(function (k) { segmentSetup[k] = num4(r[k]); });
                    segmentIsFirst.value = segmentSetup.isFirst;
                    segmentSurvey.isFirst = segmentSetup.isFirst;
                } catch (e) { console.error('load perfect cast error', e); }
            }

            // 「精度达标」预览：用 St=0 理论值覆盖实测区，演示"完美符合设计坐标时本该测到多少"。
            // 按段类型选映射表（首段/标准段字段集不同）；aN/cN/aS/cS 无理论值，不进映射、保持原值。
            function applyPerfectCastToSurvey() {
                var map = segmentIsFirst.value ? SURVEY_FROM_SETUP_FIRST : SURVEY_FROM_SETUP_STANDARD;
                Object.keys(map).forEach(function (k) {
                    segmentSurvey[k] = segmentSetup[map[k]] || '';
                });
            }

            // 取消勾选时必须原样还原实测区，**不能**改用 loadSegmentSurvey 重载：
            // 未填报段本就无库记录，重载会先清空全部录入字段（见 loadSegmentSurvey 开头）
            // 再把它们统一置为空串，把测量员已手输的值一并抹掉。
            // 其中 aN/cN/aS/cS 尤须保留：它们不参与本段 PerfectCast、也不在理论值映射表内，
            // 与「精度达标」预览毫无关系，绝不能因为取消勾选而丢失。
            function snapshotSurveyValues() {
                var snap = {};
                SURVEY_FIELDS_ALL.forEach(function (k) { snap[k] = segmentSurvey[k]; });
                return snap;
            }
            function restoreSurveyValues(snap) {
                if (!snap) return;
                Object.keys(snap).forEach(function (k) { segmentSurvey[k] = snap[k]; });
            }

            // 勾选"精度达标"：只读区与实测区一并刷新为理论值，勾选期间所有录入框只读且禁止提交；
            // 取消勾选：还原勾选前的录入快照，恢复可编辑（不做任何清空）。
            async function onIsPerfectCastChange() {
                if (!segmentSurvey.segmentName) { isPerfectCast.value = false; return; }
                if (isPerfectCast.value) {
                    await loadSegmentSetup(segmentSurvey.segmentName);
                    perfectCastSnapshot = snapshotSurveyValues();
                    applyPerfectCastToSurvey();
                    ElMessage.info(i18nUtils.$t('biz.concreteBeam.perfectCastTheoryApplied'));
                } else {
                    restoreSurveyValues(perfectCastSnapshot);
                    perfectCastSnapshot = null;
                    ElMessage.info(i18nUtils.$t('biz.concreteBeam.perfectCastRestored'));
                }
            }

            // 组装 SaveSurvey 请求：字段名与后端 SetupNSurveyDTO 一致，按本段字段清单数字化后直传
            function buildSurveyRequest() {
                var fields = segmentIsFirst.value ? SURVEY_FIELDS_FIRST : SURVEY_FIELDS_STANDARD;
                var req = {
                    castingGroupID: groupID,
                    segmentID: segmentSurvey.segmentID || 0,
                    segmentName: segmentSurvey.segmentName,
                    pouringDate: segmentSurvey.pouringDate || null
                };
                fields.forEach(function (k) { req[k] = Number(segmentSurvey[k] || 0); });
                return req;
            }

            /**
             * 设站及测量的「打印」：打开单梁段报告 PDF 的预览弹窗（弹窗内可下载 / 打印）。
             * 手册口径：点击 Apply 记录实测后打印 Set-Up 值与 resulting As-Cast Coordinates。
             * 故已记录实测（isFilled）→ baogao（SETUP + SURVEY + AS-CAST 三章），
             * 未记录 → setting（只有 MATCH-CAST SETUP 一章）。
             */
            async function openSegmentReportPreview(segmentName, isFilled) {
                invalidateReportCache();
                reportUrlOverride.value = '/Biz/ConcreteBeam/SegmentReportPdf'
                    + '?castingGroupID=' + encodeURIComponent(groupID)
                    + '&segmentName=' + encodeURIComponent(segmentName || '')
                    + '&reportType=' + (isFilled ? 'baogao' : 'setting');
                reportDownloadName.value = (segmentName || 'segment') + '-' + (isFilled ? 'baogao' : 'setting');
                reportDialogTitle.value = (segmentName || '') + ' · '
                    + i18nUtils.$t('biz.concreteBeam.' + (isFilled ? 'reportSegmentAsCastReport' : 'reportSegmentSetupReport'));
                reportDialogVisible.value = true;
                await renderReportPage(1);
            }

            async function handleSegmentPrint() {
                var name = segmentSurvey.segmentName;
                if (!name) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.selectSegmentFirst')); return; }
                var cur = segmentList.value.find(function (x) { return x.segmentName === name; });
                var filled = (segmentSurvey.segmentID || 0) > 0 || !!(cur && cur.isFilled);
                if (!filled) ElMessage.info(i18nUtils.$t('biz.concreteBeam.segmentNotSurveyedHint'));
                await openSegmentReportPreview(name, filled);
            }

            async function handleSegmentConfirm() {
                if (!segmentSurvey.segmentName) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.selectSegmentFirst')); return; }
                if (!segmentSurvey.canEdit) { ElMessage.warning(segmentSurvey.lockedReason || i18nUtils.$t('biz.concreteBeam.segmentNotEditable')); return; }
                if (isPerfectCast.value) { ElMessage.warning(i18nUtils.$t('biz.concreteBeam.perfectCastLockSubmit')); return; }
                try {
                    var res = await HttpUtils.post('/Biz/ConcreteBeam/SaveSurvey', buildSurveyRequest());
                    var d = (res && res.survey) ? res.survey : null;
                    if (d) segmentSurvey.surveyMatchCastTwist = num4(d.surveyMatchCastTwist);
                    ElMessage.success((res && res.message) ? res.message : i18nUtils.$t('biz.concreteBeam.saveSuccess'));
                    // 保存后梁段可操作范围可能变化（下一段解锁），重新载入列表
                    await loadSetupSurveySheet();
                } catch (e) { console.error('save survey error', e); }
            }

            function handleSegmentCancel() {
                isPerfectCast.value = false;
                perfectCastSnapshot = null;          // 「取消」是显式丢弃，快照一并作废
                if (segmentSurvey.segmentName) loadSegmentSurvey(segmentSurvey.segmentName);
                else ElMessage.info(i18nUtils.$t('biz.concreteBeam.cancelled'));
            }
            /* Setup Survey function 结束 */

            window.requestPlatformTabClose = requestPlatformTabClose;

            Vue.onMounted(function () {
                window.addEventListener('beforeunload', handleBeforeUnload);
                loadGroupInfo();
                loadSegmentData();
                loadCamberData();
            });
            Vue.onBeforeUnmount(function () {
                window.removeEventListener('beforeunload', handleBeforeUnload);
            });

            return {
                loading: loading,
                activeTab: activeTab,
                tabs: tabs,
                hasSurveyData: hasSurveyData,
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
                designPreviewToken: designPreviewToken,
                handleDesignPageChange: handleDesignPageChange,
                handleDesignSizeChange: handleDesignSizeChange,
                onDesignNormalizeChange: onDesignNormalizeChange,
                onDesignCamberChange: onDesignCamberChange,
                handleImportDesignCoord: handleImportDesignCoord,
                downloadDesignTemplate: downloadDesignTemplate,
                handleDesignPrint: handleDesignPrint,
                handleDesignPreview: handleDesignPreview,
                designPreviewVisible: designPreviewVisible,
                designPreviewLoading: designPreviewLoading,
                designPreviewError: designPreviewError,
                designPreviewUpper: designPreviewUpper,
                designPreviewLower: designPreviewLower,
                handleDesignPreviewPrint: handleDesignPreviewPrint,
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
                downloadCamberTemplate: downloadCamberTemplate,
                handleCamberPrint: handleCamberPrint,
                handleCamberPreview: handleCamberPreview,
                handleCamberConfirm: handleCamberConfirm,
                handleCamberCancel: handleCamberCancel,
                // 设站及测量：两个表单对象（实测录入 / 只读立模参数），字段名与后端 DTO 一致
                segmentList: segmentList,
                segmentSurvey: segmentSurvey,
                segmentSetup: segmentSetup,
                segmentIsFirst: segmentIsFirst,
                upstreamSegmentName: upstreamSegmentName,
                isPerfectCast: isPerfectCast,
                onSegmentChange: onSegmentChange,
                onIsPerfectCastChange: onIsPerfectCastChange,
                handleSegmentPrint: handleSegmentPrint,
                handleSegmentConfirm: handleSegmentConfirm,
                handleSegmentCancel: handleSegmentCancel,
                pouringSegmentList: pouringSegmentList,
                pouringView: pouringView,
                onPouringSegmentChange: onPouringSegmentChange,
                handlePouringPrint: handlePouringPrint,
                handlePouringPreview: handlePouringPreview,
                handlePouringCancel: handlePouringCancel,
                reportOverallType: reportOverallType,
                reportIncludeAll: reportIncludeAll,
                onReportOverallChange: onReportOverallChange,
                reportSegmentList: reportSegmentList,
                reportSegmentName: reportSegmentName,
                reportSegmentType: reportSegmentType,
                reportLoading: reportLoading,
                reportExporting: reportExporting,
                reportUnavailableHint: reportUnavailableHint,
                reportDialogVisible: reportDialogVisible,
                reportDialogTitle: reportDialogTitle,
                reportError: reportError,
                reportPageNo: reportPageNo,
                reportPageCount: reportPageCount,
                reportZoom: reportZoom,
                reportCanvas: reportCanvas,
                onReportSegmentChange: onReportSegmentChange,
                onReportTypeChange: onReportTypeChange,
                reportPrevPage: reportPrevPage,
                reportNextPage: reportNextPage,
                reportZoomIn: reportZoomIn,
                reportZoomOut: reportZoomOut,
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
        // 注册 Vue I18n：模板里的 $t('biz.concreteBeam.*') 依赖这一步
        if (appI18n) {
            app.use(appI18n);
        }
        var locale = typeof ElementPlusLocaleZhCn !== 'undefined' ? ElementPlusLocaleZhCn : undefined;
        // ElementPlus 内置文案跟随当前语言（日期选择器、分页、上传组件等的默认文字）
        if (typeof i18nUtils !== 'undefined' && i18nUtils.getCurrentLanguage() === 'en'
            && typeof ElementPlusLocaleEn !== 'undefined') {
            locale = ElementPlusLocaleEn;
        }
        app.use(ElementPlus, { locale: locale });
        if (window.ElementPlusIconsVue) {
            app.component('UploadFilled', window.ElementPlusIconsVue.UploadFilled);
            app.component('Download', window.ElementPlusIconsVue.Download);
        }
        app.mount('#app');
    } catch (e) {
        console.error('Mount error:', e);
    }
});
