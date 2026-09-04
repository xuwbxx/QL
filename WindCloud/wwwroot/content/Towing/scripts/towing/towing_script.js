$(document).ready(function () {

    // 设置登录人：
    $('.login_user a').html(loginUser.RealName);
    initControl();
    initMap();
    initLoading();

    
});
// 控件集
var towingMainControl = {
    left_main: null, // 左侧内容
    left_bar: null,  // 左侧bar
    right_main: null,  // 右侧内容
    right_bar: null, // 右侧bar
    qxxq_kg: null, // 气象信息开关
    qxxq_close: null, // 气象信息关闭
    qixiang_content: null, // 气象内容
    chat_kg: null, // 聊天室kg
    chatroom_content: null, // 聊天室内容
    open_history: null, // 聊天记录打开按钮
    chat_history: null, // 聊天记录 
    chat_history_close: null, // 聊天记录关闭按钮
    ziyoudu_ul: null, // 六自由度ul
    table_CGQ: null, // 传感器table
    voyageStatus: { // 左侧航行状态
        leftTime: null, // 剩余航行时间
        spentTime: null, // 已航行时间
        yawDegree: null, // 航向（度）
        speedNum: null, // 航速
    },
    monitorStatus: { // 右侧传感器状态
        monitorOfflineNum: null, // 离线个数
        monitorOnlineNum: null, // 在线个数
        //remainingEPower: null, // 剩余电量
        eleStatus: null, //电池状态
        engineStatus1: null, //发电机1状态
        engineStatus2: null, //发电机2状态
    },
    weatherInfo: { // 顶部气象信息
        speed: null, // 风速
        direction: null, // 风向
    },
    planRoute: {
        chk: null, // 计划路径checkbox
    },
    progressPath: {
        chk: null, // 航行路径checkbox
    },
    forecastRoute: {
        chk: null, // 预报数据
    }
}
// 初始化控件
function initControl() {
    if (!(towingMainControl.left_main)) {
        towingMainControl = {
            left_main: $('.left_main'),
            left_bar: $('.left_bar'),
            right_main: $('.right_main'),
            right_bar: $('.right_bar'),
            qxxq_kg: $('#qxxq_kg'),
            qxxq_close: $('#qxxq_close'),
            qixiang_content: $('.qixiang_content'),
            chat_kg: $('#chat_kg'),
            chatroom_content: $('.chatroom_content'),
            open_history: $('#open_history'),
            chat_history: $('.chat_history'),
            chat_history_close: $('#chat_history_close'),
            ziyoudu_ul: $('.ziyoudu_ul'),
            table_CGQ: $('.table_CGQ'),
            voyageStatus: {
                remainingTime: $('#remainingTime'), // 剩余航行时间
                remainingDistance: $('#remainingDistance'), // 剩余航行时间
                spentTime: $('#spentTime'), // 已航行时间
                spentDistance: $('#spentDistance'), // 已航行距离
                headingDegree: $('#headingDegree'), // 航向（度）
                speedNum: $('#speedNum'), // 航速
            },
            monitorStatus: {
                monitorOfflineNum: $('#monitorOfflineNum'), // 离线个数
                monitorOnlineNum: $('#monitorOnlineNum'), // 在线个数
                //remainingEPowerICON: $('#remainingEPowerICON'), // 剩余电量的图标
                //remainingEPower: $('#remainingEPower'), // 剩余电量
                //remainingEPowerICON2: $('#remainingEPowerICON2'), // 剩余电量的图标
                //remainingEPower2: $('#remainingEPower2'), // 剩余电量
                eleStatus: $('#EleStatus'), // 电池状态
                engineStatus1: $('#EngineStatus1'), //发电机1状态
                engineStatus2: $('#EngineStatus2'), //发电机2状态
            },
            weatherInfo: {
                speed: $('#weatherInfo_speed'), // 风速
                direction: $('#weatherInfo_direction'), // 风向
                directionName: $('#weatherInfo_directionName'), // 风向
            },
            planRoute: {
                chk: $('#chkJihua'),
            },
            progressPath: {
                chk: $('#chkHangxing'),
            },
            forecastRoute: {
                chk: $('#chkYuBao'),
            }
        }
    }

    // 计划路径checkbox开关
    towingMainControl.planRoute.chk.on('change', function (e) {
        // console.log('planRoute:chk:', e.target.checked);
        // planRoute.show = e.target.checked;
        showPlanPath();
    });
    // 实际航线checkbox开关
    towingMainControl.progressPath.chk.on('change', function (e) {
        // console.log('progressPath:chk:', e.target.checked);
        // progressPath.show = e.target.checked;
        showProgressPath();
    });
    //预报数据
    towingMainControl.forecastRoute.chk.on('change', function (e) {
        // console.log('planRoute:chk:', e.target.checked);
        // planRoute.show = e.target.checked;
        //debugger;
        showForecastRoute();
    });

    // 聊天室
    // 发送按钮绑定
    $('#btnChatSend').on('click', function (e) {
        sendMsg();
    });

    // 是否紧急消息开关
    $('.chat_gongneng .btn_jj').on('click', function (e) {
        $('.chat_gongneng .btn_jj').css('opacity', 1);
        $('.chat_gongneng .btn_pt').css('opacity', 0.6);
        $('#chatMsgContent').css('color', '#d32e2e');
        $('#chatMsgContent').focus();
        chatMsg.isUrgent = true;
    });
    $('.chat_gongneng .btn_pt').on('click', function (e) {
        $('.chat_gongneng .btn_jj').css('opacity', 0.6);
        $('.chat_gongneng .btn_pt').css('opacity', 1);
        $('#chatMsgContent').css('color', '');
        $('#chatMsgContent').focus();
        chatMsg.isUrgent = false;
    });

    // 其他聊天室相关控件初始化
    initMsgControls();

    // 台风
    $("#chkTaifeng").on("change", function (e) {
        // e.stopPropagation();
        if (e.target.checked) {
            OpenTyphoonList();
        } else {
            CloseTyphoonList();
        }
        // setActive($(this));
    });



    // 船舶周边切换开关
    $("#chkZhoubian").on("change", function (e) {
        // e.stopPropagation();
        shipSurShips.show = e.target.checked;
        drawSurShips();
    });
    // 锚地切换开关
    $("#chkMaodi").on("change", function (e) {
        // e.stopPropagation();
        anchorageInfo.show = e.target.checked;
        loadAnchorages();
    });

    // 地图类型切换
    $('.map_box .bottom_menu').on('click', function (e) {
        var currType = $('.map_box .bottom_bar').html();
        var newType = $(e.target).html();
        var mapType = '';
        switch (newType) {
            case '海图':
                mapType = 'MT_SEA';
                break;
            case '卫星图':
                mapType = 'MT_SATELLITE';
                break;
            case '路线图':
                mapType = 'MT_GOOGLE';
                break;
        }
        // 切换地图
        _map.basemapsControl.changeMap(mapType);
        $('.map_box .bottom_bar').html(newType);
        $(e.target).html(currType);
        $('.map_box .bottom_bar').click();
    });

    $('.qxxq_ul a').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr('href'); // 获取目标选项卡的 ID
        loadDetailTab(target);
        // 可在此处执行其他操作，比如更新页面内容
    });

    $('#btnQuery').on('click', function (e) {
        loadDetailTab();
    });

    $('#btnSaveProjCfg').on('click', function (e) {
        saveProjSetting();
    })

    $('#btnSavePlanRoutes').on('click', function (e) {
        savePlanRouteSetting();
    })

    /* 小姜设计稿js开始 */

    //侧边收缩
    $('.left_main').css('left', '34px');

    var expandedLeft = true;

    $('.left_bar').click(function () {
        if (expandedLeft) {
            $('.left_main').animate({ left: '-25vw' }, 500);
            $('.left_bar').css('background-position', '-36px 0px');

        } else {
            $('.left_main').animate({ left: '34px' }, 500);
            $('.left_bar').css('background-position', '-0px 0px');
        }

        expandedLeft = !expandedLeft;
    });

    $('.right_main').css('right', '-24vw');
    var expandedRight = true;
    $('.right_bar').click(function () {
        if (expandedRight) {
            $('.right_main').animate({ right: '34px' }, 500);
            $('.right_bar').css('background-position', '-36px 0px');
        } else {

            $('.right_main').animate({ right: '-25vw' }, 500);
            $('.right_bar').css('background-position', '-0px 0px');

        }

        expandedRight = !expandedRight;
    });

    //底部按钮 
    $('.bottom_bar').click(function () {
        if ($('.bottom_menu').css("display") == 'block') {
            $('.bottom_menu').stop(true, true).slideUp();

            $('.bottom_bar').css("backgroundSize", "100% 100%");
        } else {
            $('.bottom_menu').stop(true, true).slideDown();

            $('.bottom_bar').css("backgroundSize", "100% 100%");
        }
    });

    //信息详情
    $('#qxxq_kg').click(function () {
        $('.xinxi_content').animate({ bottom: '2vw' }, 800);
        //$('.xinxi_content').show();
    });
    $('#qxxq_close').click(function () {
        $('.xinxi_content').animate({ bottom: '-75vw' }, 800);
        //$('.xinxi_content').hide();
    });
    //聊天室
    $('.bg_zhtx').click(function () { openChatMsgDlg() });
    $('#chatroom_close').click(function () { closeChatMsgDlg() });

    //	打开历史记录弹框
    $('#open_history').click(function () {
        //$('.chat_history').animate({bottom:'-75vw'},800);
        $('.chat_history').show();
    });

    //	关闭历史记录弹框
    $('#chat_history_close').click(function () {
        //$('.chat_history').animate({bottom:'-75vw'},800);
        $('.chat_history').hide();
    });


    /* 小姜设计稿js结束 */
}

var OpenTyphoonList = function () {

    _map.weatherService.showTyphoonInfoList(function () {
        console.log("showTyphoonInfoList is close!")
    }, true, null, { showWarningLine: true, autoLocation: true, showTimeTipMinZoom: 6 });

    displayProjectArea(true);

}

var CloseTyphoonList = function () {

    _map.weatherService.hideTyphoonInfoList();
    displayProjectArea(false);
}

function setTaifengChecked(checked) {
    // 设置复选框状态
    $("#chkTaifeng").prop("checked", checked);
    // 触发change事件
    $("#chkTaifeng").trigger("change");


    //setTaifengChecked(true);
    //setTaifengChecked(false);

}

var PopupConfigur = function () {

    if (loginUser == null || loginUser.RoleID == null || loginUser.RoleID != 1)
    {
        alert('您没有权限进入配置页面');
        return;
    }

    $('#tk_configur').modal('show');
    loadProjSetting();
}

// 地图参数
var _map;
var map_options = {
    ak: $("input#PXYKey").val(),
    mapTypes: ['MT_SEA', 'MT_GOOGLE', 'MT_SATELLITE'],// 显示地图类型，'MT_SEA'，MT_GOOGLE,MT_SATELLITE,
    // 默认展示地图类型
    defaultMapType: 'MT_SATELLITE',
    // 初始中心点坐标
    centerPoint: [29.1, 120.11],
    // centerPoint: [19.724857, 109.188154],
    // 初始缩放级别
    zoom: 6,
    // 最小缩放级别
    minZoom: 2,
    // 最大缩放级别
    maxZoom: 14,
    // 公司版权信息( 支持html )，默认Elane Inc.
    attribution: { isShow: false, emptyString: '&copy;2025 <a class="shipxy_small"></a>&nbsp;<a>Elane Inc.</a>' },
    // 测距控件
    measureCtrl: {
        // 是否开启测距控件，默认：true
        isShow: true,
        // 是否显示测距按钮，默认：true
        showMeasurementsMeasureControl: true,
        // 是否显示删除按钮，默认：true
        showMeasurementsClearControl: true,
        // 是否显示切换单位按钮，默认：true
        showUnitControl: false,
        position: 'bottomleft'
    },
    //鼠标移动悬浮经纬度控件
    mousePostionCtrl: { isShow: true, position: 'bottomleft' },
    //缩放工具控件的显示隐藏
    zoomControlElane: { isShow: false, position: 'topright' },
    // 缩放级别显示控件
    zoomviewControl: { isShow: false, position: 'topleft' },
    //地图切换控件的位置
    basemapsControl: { isShow: false, position: 'topleft' },
    // 比例尺，控件
    scaleCtrl: { isShow: false, position: "bottomleft" },
};	// 创建地图示例
// 初始化地图
function initMap() {
    //var mapper = ShipxyAPI.Map("map", map_options);
    //_map = new mapper();
    _map = new ShipxyAPI.Map("map", map_options);

    ShipxyAPI.WeatherService(_map);

    //缩小改变
    _map.on('zoomend', function (e) {
        var ZoomValue = _map.getZoom();
    });
    _map.basemapsControl.changeMap(map_options.defaultMapType)
    ShipxyAPI.TrackService(_map);

    _map.on('click', onMapClick);

}

var Setting = false;

//监听鼠标点击事件，弹出经纬度
function onMapClick(e) {
    //L.popup().setLatLng(e.latlng)
    //    .setContent('经纬度' + e.latlng.toString())
    //    .openOn(_map);
    //console.log(e.latlng)
    if (Setting) {
        //console.log(e.latlng.lat);
        var ThisPosition = {};
        ThisPosition.Lat = e.latlng.lat;
        ThisPosition.Lon = e.latlng.lng;

        //SettingWarningPositions.push(Position);

        console.log(ThisPosition.Lat);
        console.log(ThisPosition.Lon);

        //AddPositionMarker(ThisPosition);
    }

};

//添加工具标记点
var PositionMarkers = [];
var SettingWarningPositions = [];
var Setting = false;
var AddPositionMarker = function (Position) {

    if (Position == null)
        return;

    var IconUrl = "/Content/Towing/img/cb_red.png";

    var icon = L.icon({
        iconSize: [6, 6],
        iconUrl: IconUrl,
        iconAnchor: [3, 3]
    });

    var marker = L.marker([Position.Lat, Position.Lon],
        {
            icon: icon,
        }).addTo(_map);
    marker.bindTooltip('E:' + Position.Lon + ' N:' + Position.Lat, {
        //className: 'leaflet-labelship',
        //permanent: IsFanZoom,
        offset: [130, -20]
    });
    PositionMarkers.push(marker);
}

// 轮询时间配置
var timer = {
    fillLeftPanel: flashTime.fillLeftPanel * 1000, // 左侧面板：5分钟
    leftPanel: null,
    setLeft: function () {
        if (timer.leftPanel) {
            clearTimeout(timer.leftPanel);
        }
        timer.leftPanel = setTimeout(fillLeftPanel, timer.fillLeftPanel);
    },
    fillRightPanel: flashTime.fillRightPanel * 1000, // 右侧面板：5分钟
    offlineSeconds: flashTime.offlineSeconds, // 离线时间：5分钟（此处为秒，不是setTimeout用的毫秒）
    rightPanel: null,
    setRight: function () {
        if (timer.rightPanel) {
            clearTimeout(timer.rightPanel);
        }
        timer.rightPanel = setTimeout(fillRightPanel, timer.fillRightPanel);
    },
    fillChatPanel: flashTime.fillChatPanel * 1000, // 聊天记录：1分钟
    chatPanel: null,
    setChat: function () {
        if (timer.chatPanel) {
            clearTimeout(timer.chatPanel);
        }
        timer.chatPanel = setTimeout(fillChatPanel, timer.fillChatPanel);
    },
    fillWeatherForecost: flashTime.fillWeatherForecost * 1000, // 气象：4*60分钟 （4小时)
    weatherForecost: null,
    setWeatherForecost: function () {
        if (timer.weatherForecost) {
            clearTimeout(timer.weatherForecost);
        }
        timer.weatherForecost = setTimeout(fillWeatherForecost, timer.fillWeatherForecost);
    },
    fillProgressPath: flashTime.fillProgressPath * 1000, // 实时路径：5分钟
    progressPath: null,
    setProgressPath: function () {
        if (timer.progressPath) {
            clearTimeout(timer.progressPath);
        }
        timer.progressPath = setTimeout(loadProgressPath, timer.fillProgressPath);
    },
    fillWeatherInfo: flashTime.fillWeatherInfo * 1000, // 天气信息：5分钟
    weatherInfo: null,
    setWeatherInfo: function () {
        if (timer.weatherInfo) {
            clearTimeout(timer.weatherInfo);
        }
        timer.weatherInfo = setTimeout(loadWeatherInfo, timer.fillWeatherInfo);
    },

    loadMsgList: flashTime.loadMsgList * 1000, // 聊天记录：5分钟
    loadMsg: null,
    setLoadMsg: function () {
        if (timer.loadMsg) {
            clearTimeout(timer.loadMsg);
        }
        timer.loadMsg = setTimeout(loadMsg, timer.loadMsgList);
    },
    loadShipSurShips: flashTime.loadShipSurShips * 1000, // 船舶周围情况：5分钟
    loadSurShips: null,
    setLoadShipSurShips: function () {
        if (timer.loadSurShips) {
            clearTimeout(timer.loadSurShips);
        }
        timer.loadSurShips = setTimeout(loadShipSurShips, timer.loadShipSurShips);
    },

    snapPics: 1 * 60 * 1000, // 抓取照片：1分钟
    snapPic: null,
    setGetSnapPic: function () {
        //    if (timer.snapPic) {
        //        clearTimeout(timer.snapPic);
        //    }
        //    timer.snapPic = setTimeout(snapPics, timer.getSnapPics);
    },

    fillTyphoonForecast: flashTime.fillTyphoon * 1000, // 台风预报：60分钟
    TyphoonForecast: null,
    setTyphoonForecast: function () {
        if (timer.TyphoonForecast) {
            clearTimeout(timer.TyphoonForecast);
        }
        timer.TyphoonForecast = setTimeout(loadTyphoonForecast, timer.fillTyphoonForecast);
    },

}
// 初始化轮询
function initLoading() {
    loadPlanPath(); // 加载计划路径
    loadProgressPath(); // 加载实际路径
    //loadForecastRoute();

    fillLeftPanel(); // 加载左侧窗体信息
    fillRightPanel(); // 加载右侧窗体信息
    // 清理聊天框
    $('#chatMsgList').html('');
    loadMsg(); // 加载聊天记录
    fillWeatherForecost(); // 加载天气预报，虽然在最下方，但每天加载一次，不受其他标签页影响
    //loadWeatherInfo(); // 合并到loadLeftPanel();
    loadDetailInfo(); // 下方加载信息详情，气象详情无需相应查询，其余查询，将最多返回时间范围内最接近开始时间的300条数据
    loadShipSurShips(); // 加载目标周围船舶
    loadProjSetting(); // 加载项目信息
    //snapPics(); // 抓取照片

    $('#DataStartTime').val(formatDateTimeByDay(getDaysAgo(-3)));
    $('#DataEndTime').val(formatDateTimeByDay(new Date()));

    loadProjectArea();
    loadTyphoonForecast();//台风预测
}

// 填充左侧窗体信息
var leftPanel = {
}
var mru = {
    //maxDangX: 7, // 最大横荡
    //maxDangY: 7, // 最大纵荡
    //maxDangZ: 7, // 最大垂荡
    //maxYaoX: 7, // 最大横摇 
    //maxYaoY: 7, // 最大总要
    //maxYaoZ: 7, // 最大艏摇
    data: null,
    lastTime: null,
}
var navigationalStatus = {
    data: null,
}
var windInfo = {
    data: null,
}
var satellteInfo = {
    data: null,
}
function getNum(v, s) {
    if (v == 0) v = '-';
    if ($.type(v) == 'number') {
        v = v.toFixed(s || 3);
    }

    return v;
}
var tmpMRU = `<li class="clearfix">
        <div class="pull-left ziyoudu_div_one"><img src="/Content/Towing/img/icon_cd.png"><span>垂荡</span></div>
        <div class="pull-left ziyoudu_div_two">
          <div><span class="cd_maxvalue">{{getNum(this.DangZMax, 3)}}</span><span>m</span></div>
          <div><span class="cd_absvalue">{{getNum(this.DangZAbs, 3)}}</span><span>m</span></div>
        </div>
      </li>
      <li class="clearfix">
        <div class="pull-left ziyoudu_div_one"><img src="/Content/Towing/img/icon_hd.png"><span>横荡</span></div>
        <div class="pull-left ziyoudu_div_two">
          <div><span class="hd_maxvalue">{{getNum(this.DangXMax, 3)}}</span><span>m</span></div>
          <div><span class="hd_absvalue">{{getNum(this.DangXAbs, 3)}}</span><span>m</span></div>
        </div>
      </li>
      <li class="clearfix">
        <div class="pull-left ziyoudu_div_one"><img src="/Content/Towing/img/icon_hy.png"><span>横摇</span></div>
        <div class="pull-left ziyoudu_div_two">
          <div><span class=" {{this.YaoXMaxLvl+' '}} hy_maxvalue">{{getNum(this.YaoXMax, 3)}}</span><span>°</span></div>
          <div><span class=" {{this.YaoXAbsLvl+' '}} hy_absvalue">{{getNum(this.YaoXAbs, 3)}}</span><span>°</span></div>
        </div>
      </li>
      <li class="clearfix">
        <div class="pull-left ziyoudu_div_one"><img src="/Content/Towing/img/icon_zd.png"><span>纵荡</span></div>
        <div class="pull-left ziyoudu_div_two">
          <div><span class="zd_maxvalue">{{getNum(this.DangYMax, 3)}}</span><span>m</span></div>
          <div><span class="zd_absvalue">{{getNum(this.DangYAbs, 3)}}</span><span>m</span></div>
        </div>
      </li>
      <li class="clearfix">
        <div class="pull-left ziyoudu_div_one"><img src="/Content/Towing/img/icon_zy.png"><span>纵摇</span></div>
        <div class="pull-left ziyoudu_div_two">
          <div><span class="{{this.YaoYMaxLvl+' '}} zy_maxvalue">{{getNum(this.YaoYMax, 3)}}</span><span>°</span></div>
          <div><span class="{{this.YaoYAbsLvl+' '}} zy_absvalue">{{getNum(this.YaoYAbs, 3)}}</span><span>°</span></div>
        </div>
      </li>
      <li class="clearfix">
        <div class="pull-left ziyoudu_div_one"><img src="/Content/Towing/img/icon_sy.png"><span>艏摇</span></div>
        <div class="pull-left ziyoudu_div_two">
          <div><span class="{{this.YaoZMaxLvl+' '}} sy_maxvalue">{{getNum(this.YaoZMax, 3)}}</span><span>°</span></div>
          <div><span class="{{this.YaoZAbsLvl+' '}} sy_absvalue">{{getNum(this.YaoZAbs, 3)}}</span><span>°</span></div>
        </div>
      </li>
`
function fillLeftPanel() {
    $.post('/Towing/Main/GetLeftPanel', { lastTime: mru.lastTime || '' }, function (r) {
        // console.log('leftPanel:', r);
        leftPanel = JSON.parse(r);

        mru.lastTime = leftPanel.MRU.Data.UpdateTime;

        if (leftPanel) {
            // 六自由度(MRU)
            if (leftPanel.MRU) {
                mru.data = leftPanel.MRU;
                if (mru.data && mru.data.Data) mru.data = leftPanel.MRU.Data;
                if (mru && mru.data) {
                    //mru.lastTime = mru.data.UpdateTime;
                    towingMainControl.ziyoudu_ul.html('');
                    towingMainControl.ziyoudu_ul.append(TemplateEngine(tmpMRU, mru.data));
                }
            }
            // 航行信息，航速（SailSpeed）、航向（EGYRO）
            if (leftPanel.NavigationalStatus) {
                navigationalStatus.data = leftPanel.NavigationalStatus;
                if (navigationalStatus.data && navigationalStatus.data.Data) navigationalStatus.data = navigationalStatus.data.Data;
                if (navigationalStatus && navigationalStatus.data) {
                    towingMainControl.voyageStatus.remainingTime.html(navigationalStatus.data.RemainingTimeHoursStr); // 剩余航行时间
                    towingMainControl.voyageStatus.remainingDistance.html(navigationalStatus.data.RemainingDistance); // 剩余航行距离
                    towingMainControl.voyageStatus.spentTime.html(navigationalStatus.data.SpentTimeHoursStr); // 已航行时间
                    towingMainControl.voyageStatus.spentDistance.html(navigationalStatus.data.SpentDistance); // 已航行距离
                    towingMainControl.voyageStatus.headingDegree.html(navigationalStatus.data.HeadingDegreeStr); // 航向（度）
                    towingMainControl.voyageStatus.speedNum.html(navigationalStatus.data.SpeedStr); // 航速
                    rotateImg($('#imgHeadingDegree'), navigationalStatus.data.HeadingDegree, 2);
                }
            }
            //风速风向
            if (leftPanel.WindInfo) {
                windInfo.data = leftPanel.WindInfo;
                if (windInfo.data && windInfo.data.Data) windInfo.data = windInfo.data.Data;
                towingMainControl.weatherInfo.speed.html(windInfo.data.WindSpeedStr);
                towingMainControl.weatherInfo.speed.removeClass('red alerm normal');
                towingMainControl.weatherInfo.speed.addClass(windInfo.data.WindSpeedLvl);
                towingMainControl.weatherInfo.direction.html(windInfo.data.WindDirectionStr);
                rotateImg($('#imgWindDirection'), Number(windInfo.data.WindDirection) + 180, 2);

                towingMainControl.weatherInfo.directionName.html(getWindDirection(Number(windInfo.data.WindDirection)) + '风');

            }
            // 经纬度
            if (leftPanel.SatellteInfo) {
                satellteInfo.data = leftPanel.SatellteInfo;
                if (satellteInfo.data && satellteInfo.data.Data) satellteInfo.data = satellteInfo.data.Data;
                $('#shipLon').html(satellteInfo.data.LonStr);
                $('#shipLat').html(satellteInfo.data.LatStr);
            }
        }

        timer.setLeft();
    });

    //function loadWeatherInfo() {
    //    $.post('/Towing/Main/GetWindInfo', function (r) {
    //        r = JSON.parse(r);
    //        if (r && r.Data) {
    //            towingMainControl.weatherInfo.speed.html(r.Data.WindSpeed);
    //            towingMainControl.weatherInfo.direction.html(r.Data.WindDirection);
    //        }

    //        timer.setWeatherInfo();
    //    });
    //}
}

// 填充右侧窗体信息
var tmpDeviceStatus = `<tr>
        <td class="leixing_box">{{this.DeviceType}}</td>
        <td>{{this.DeviceName}}</td>
        <td>{{this.updateTimeStr}}</td>
        <td><span class="{{this.statusCSS}}">{{this.statusStr}}</span></td>
    </tr>`;
var rightPanel = {
};
var deviceStatus = {
    info: null,
    list: null,
    online: 0,
    offline: 0,
};
function fillRightPanel() {

    // 传感器列表信息
    $.post('/Towing/Main/GetRightPanel', function (r) {
        // console.log('rightPanel:', r);

        //debugger;

        rightPanel = JSON.parse(r);
        deviceStatus = rightPanel.DeviceStatusList;
        if (deviceStatus && deviceStatus.List) {
            towingMainControl.table_CGQ.find('tbody').html('');
            deviceStatus.online = 0;
            deviceStatus.offline = 0;
            //deviceStatus.remainingEPower = deviceStatus.Data.RemainEle || -1;
            //deviceStatus.remainingEPower2 = deviceStatus.Data.RemainEle2 || -1;

            //1电压2发电机
            //deviceStatus.eleStatus = deviceStatus.Data.EleStatus || -1;
            //deviceStatus.engineStatus1 = deviceStatus.Data.Engine1Status || -1;
            //deviceStatus.engineStatus2 = deviceStatus.Data.Engine2Status || -1;

            var lastType = '';
            $.each(deviceStatus.List, function (idx, itm) {
                if (idx == 0) lastType = itm.DeviceType;
                // console.log('each.deviceStatus.list:', idx, itm);
                itm.updateTimeStr = itm.LatestUpdateTime.toLocaleString().replace('T', ' ');

                //0701临时修改
                itm.statusCSS = 'zx';
                itm.statusStr = '在线';
                deviceStatus.online += 1;

                //if (getTimeDiff(getNowTime(), itm.LatestUpdateTime) > timer.offlineSeconds) {
                //    itm.statusCSS = 'lx';
                //    itm.statusStr = '离线';
                //    deviceStatus.offline += 1;
                //} else {
                //    itm.statusCSS = 'zx';
                //    itm.statusStr = '在线';
                //    deviceStatus.online += 1;
                //}
                towingMainControl.table_CGQ.find('tbody').append(TemplateEngine(tmpDeviceStatus, itm));
            });
            mergeCells($('.table_CGQ'), 0);
            //  $.each($('.table_CGQ').find('td[rowspan]').parent(), function (idx, itm) { if (idx > 0) $(itm).prev().addClass('border_bottom'); })')
            // 传感器状态信息
            var ms = towingMainControl.monitorStatus;
            ms.monitorOfflineNum.html(deviceStatus.offline);
            ms.monitorOnlineNum.html(deviceStatus.online);


            //电池和发电机
            ms.eleStatus.removeClass('xcd').removeClass('zc').removeClass('tzgz');
            ms.eleStatus.html(deviceStatus.eleStatus || '-');
            ms.engineStatus1.removeClass('xcd').removeClass('zc').removeClass('tzgz');
            ms.engineStatus1.html(deviceStatus.engineStatus1 || '-');
            ms.engineStatus2.removeClass('xcd').removeClass('zc').removeClass('tzgz');
            ms.engineStatus2.html(deviceStatus.engineStatus2 || '-');

            if (deviceStatus.Data.EleStatus == 1) {
                //正常
                ms.eleStatus.addClass('zc');
                ms.eleStatus.html(deviceStatus.Data.EleStatusStr);
            }
            else if (deviceStatus.Data.EleStatus == 2)
            {
                //需充电
                ms.eleStatus.addClass('xcd');
                ms.eleStatus.html(deviceStatus.Data.EleStatusStr);
            }
            else if (deviceStatus.Data.EleStatus == 3) {
                //停止工作
                ms.eleStatus.addClass('tzgz');
                ms.eleStatus.html(deviceStatus.Data.EleStatusStr);
            }

            if (deviceStatus.Data.Engine1Status == 1) {
                //工作
                ms.engineStatus1.addClass('zc');
                ms.engineStatus1.html(deviceStatus.Data.Engine1StatusStr);
            }
            else if (deviceStatus.Data.Engine1Status == 2) {
                //停止
                ms.engineStatus1.addClass('dj');
                ms.engineStatus1.html(deviceStatus.Data.Engine1StatusStr);
            }

            if (deviceStatus.Data.Engine2Status == 1) {
                //工作
                ms.engineStatus2.addClass('zc');
                ms.engineStatus2.html(deviceStatus.Data.Engine2StatusStr);
            }
            else if (deviceStatus.Data.Engine2Status == 2) {
                //停止
                ms.engineStatus2.addClass('dj');
                ms.engineStatus2.html(deviceStatus.Data.Engine2StatusStr);
            }

            //ms.remainingEPower.removeClass("alert");
            //ms.remainingEPower.css('color', '');

            //var iconSrc = '';
            //ms.remainingEPower.html(deviceStatus.remainingEPower || '-');
            //if (deviceStatus.remainingEPower == -1) {
            //    ms.remainingEPower.html('-');
            //    iconSrc = '/Content/Towing/img/icon_SYdl01.png';
            //    ms.remainingEPower.css('color', 'red');
            //} else if (deviceStatus.remainingEPower > 80) {
            //    iconSrc = '/Content/Towing/img/icon_SYdl03.png';
            //} else if (deviceStatus.remainingEPower > 20) {
            //    iconSrc = '/Content/Towing/img/icon_SYdl02.png';
            //} else {
            //    iconSrc = '/Content/Towing/img/icon_SYdl01.png';
            //    ms.remainingEPower.css('color', 'red');
            //}
            //ms.remainingEPowerICON.attr('src', iconSrc);

            //ms.remainingEPower2.removeClass("alert");
            //ms.remainingEPower2.css('color', '');
            //var iconSrc = '';
            //ms.remainingEPower2.html(deviceStatus.remainingEPower2 || '-');
            //if (deviceStatus.remainingEPower2 == -1) {
            //    ms.remainingEPower2.html('-');
            //    iconSrc = '/Content/Towing/img/icon_SYdl01.png';
            //    ms.remainingEPower2.css('color', 'red');
            //} else if (deviceStatus.remainingEPower2 > 80) {
            //    iconSrc = '/Content/Towing/img/icon_SYdl03.png';
            //} else if (deviceStatus.remainingEPower2 > 20) {
            //    iconSrc = '/Content/Towing/img/icon_SYdl02.png';
            //} else {
            //    iconSrc = '/Content/Towing/img/icon_SYdl01.png';
            //    ms.remainingEPower2.css('color', 'red');
            //}
            //ms.remainingEPowerICON2.attr('src', iconSrc);

        }
        timer.setRight();
    });
}

// 加载计划路径
var planRoute = {
    show: false,
    append: false,
    lines: null,
    points: [],
    forecast: null,
    cfg: {
        title: '计划航线',
        color: "#00e4ff",
        pColor: "#aae9fa"
    },
    list: [],
    dock: {}, // 起始港口
    anch: {}, // 目标锚地
}
function showPlanPath() {
    planRoute.load = loadPlanPath;
    planRoute = showCommon(planRoute);
}
function loadPlanPath() {
    $.post('/Towing/Main/GetPlanPath', function (r) {
        // planRoute.list = r.List;
        drawPlanPath(r);
        // append Plan Route Setting
        appendPlanRouteSetting(r);
    });
}
function drawPlanPath(r) {
    //debugger;
    if ($.type(r) == "string") r = toCamelCaseJson(JSON.parse(r));
    planRoute.list = r.list;
    var list = planRoute.list;
    if (list && list.length > 0) {
        // 绘制路径
        planRoute.lines = getJTLines(planRoute.list, planRoute.cfg);
        planRoute.points = [];
        $.each(planRoute.list, function (idx, itm) {
            itm.icon = '/Content/Towing/img/icon_hxtz.png';
            var routeTime = itm.routeTime == null ? '' : itm.routeTime.substr(0, 19).replace('T', ' ');
            itm.title = '[经:' + itm.lon + '°,纬:' + itm.lat + '°]\n' + routeTime; // getYearDateStr(itm.routeTime);//.toLocaleString();
            // console.log('itm:', itm);
            if (idx > 0 && idx < (planRoute.list.length - 1)) {
                planRoute.points.push(getPoint(itm));
            }
        })
        // 若show为true，则展示
        if (planRoute.show) {
            // 若已有lines，则先移除（重绘的情况可能会已有lines）
            if (planRoute.lines) {
                _map.removeLayer(planRoute.lines);
            }
            _map.addLayer(planRoute.lines);
            planRoute.append = true;
        }
        // 设置起点（码头）
        planRoute.dock = list[0];
        planRoute.dock.icon = '/Content/Towing/img/icon_guiji_qd.png';
        // 设置终点（工作锚地）
        if (list.length > 1) {
            planRoute.anch = list[list.length - 1];
        } else {
            planRoute.anch = planRoute.dock;
        }
        planRoute.anch.icon = '/Content/Towing/img/icon_guiji_zd.png';
        planRoute.dock.zIndex = 99999;
        planRoute.anch.zIndex = 99999;
        planRoute.dock.iconAnchor = [12, 12];
        planRoute.anch.iconAnchor = [20, 20];
        planRoute.dock.iconSize = [24, 24];
        planRoute.anch.iconSize = [40, 40];
        drawPoint(planRoute.dock);
        drawPoint(planRoute.anch);
        // mapHelper.setCenter(planRoute.anch);
    }

}

//预报数据
// 加载
var forecastRoute = {
    show: false,
    append: false,
    lines: null,
    points: [],
    introduces:[],
    cfg: {
        title: '预报数据',
        color: "#ffa200",
        pColor: "#aae9fa"
    },
    list: [],
}
function showForecastRoute() {

    

    if (!forecastRoute.show) {
        loadForecastRoute();
    }
    else
    {
        if (forecastRoute.lines != null)
        {
            _map.removeLayer(forecastRoute.lines);
        }
        if (forecastRoute.points != null && forecastRoute.points.length > 0)
        {
            $.each(forecastRoute.points, function (idx, itm) {
                _map.removeLayer(itm);
            });
        }
        if (forecastRoute.forecast != null)
        {
            forecastRoute.forecast.remove();
        }
        
    }
    


    forecastRoute.show = !forecastRoute.show;

}

function loadForecastRoute() {
    $.post('/Towing/Main/GetForecastRoute', function (r) {
        // planRoute.list = r.List;
        drawForecastRoute(r);
    });
}

function drawForecastRoute(r) {
    if ($.type(r) == "string") r = toCamelCaseJson(JSON.parse(r));
    forecastRoute.list = r.list;
    var list = forecastRoute.list;
    if (list && list.length > 0) {
        // 绘制路径
        forecastRoute.lines = getJTLines(forecastRoute.list, forecastRoute.cfg);
        forecastRoute.points = [];
        $.each(forecastRoute.list, function (idx, itm) {

            if (itm.totalAlarm) {
                itm.icon = '/Content/Towing/img/icon_hxtz_hs.png';
                itm.size = 10;
            }
            else
            {
                itm.icon = '/Content/Towing/img/icon_hxtz_cs.png';
            }
            
            itm.title = '[' + itm.timeStr + '时]\n\r' + '横摇:' + itm.yaoX + '° 纵摇:' + itm.yaoY + '° 垂荡:' + itm.dangZ + '米'; // getYearDateStr(itm.routeTime);//.toLocaleString();
            // console.log('itm:', itm);

            forecastRoute.points.push(getForecastPoint(itm, itm.size));

            if (itm.totalAlarm)
            {
                forecastRoute.introduces.push(getForecastIntroducePoint(itm));
            }

            
        })

        //划线
        if (forecastRoute.lines) {
            _map.removeLayer(forecastRoute.lines);
        }
        _map.addLayer(forecastRoute.lines);

        //画点
        $.each(forecastRoute.points, function (idx, itm) {
            _map.addLayer(itm);
        });

        //最后一个点要加上数据列表
        if (r.list.length > 0)
        {
            drawIntroduceMarker(r.list[r.list.length - 1], forecastRoute.list);
        }

    }

}

var getForecastPoint = function (point, size) {

    if (size == null) {
        size = 6;
    }


    // 自定义图标
    var marker_icon_1;
    if (point.html) {
        marker_icon_1 = L.divIcon({
            html: point.html,
            className: point.className || 'div_icon'
        });
    } else {
        marker_icon_1 = L.icon({
            iconUrl: point.icon, // "https://api.shipxy.com/h5s/api/3.0/images/death.png",
            iconAnchor: point.iconAnchor || [size, size],//图标锚点
            iconSize: point.iconSize || [size * 2, size * 2], //图标大小
        });
    }

    //坐标要转换（wgs84转gcj02）
    const wgs84Coord = { lng: point.lon, lat: point.lat }; // 示例坐标
    const gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

    // 添加标记,到地图
    var marker = ShipxyAPI.marker([gcj02Coord.lat, gcj02Coord.lng], {
        icon: marker_icon_1,
        title: '', //point.title,
        rotationAngle: point.rotationAngle ? point.rotationAngle : undefined,
    });

    marker.bindTooltip(point.title, {
        //className: 'leaflet-labelship',
        //permanent: IsFanZoom,
        //offset: [-20, -35]
    });

    marker.on('click', function (e) {

        OpenForecastAlarmPoint(point.dataID);

    });

    return marker;



}

//预报报警点详情
var getForecastIntroducePoint = function (point) {


    var introduceHtml = '<div class="mapyjxq_box" id="mapyjxq' + point.dataID + '" style="display:none;"><div class="mapyjxq_son"><div class="mapyjxq_box_img"><img src="/Content/Towing/img/map_img_zx.png"></div><div class="mapyjxq_box_info">'
        + '<div class="mapyjxq_box_info_tit"><span>预警详情</span><img src="/Content/Towing/img/icon_close.png" onclick="CloseForecastIntroducePoint(' + point.dataID + ')"></div><ul class="list02 clearfix">'
        + '<li><div>' + point.timeStr + ' 时</div><div><span>横摇<i>' + point.yaoX + '</i>°</span><span>纵摇<i>' + point.yaoY + '</i>°</span><span>垂荡<i>' + point.dangZ + '</i>m</span></div></li></ul><ul class="yjsx_box">';

    //var introduceHtml = '<div class="mapyjxq_box" id="mapyjxq' + point.dataID + '" style="display:none;"><div class="mapyjxq_son"><div class="mapyjxq_box_img"><img src="/Content/Towing/img/map_img_zx.png"></div><div class="mapyjxq_box_info">'
    //    + '<div class="mapyjxq_box_info_tit"><span>预警详情</span><img src="/Content/Towing/img/icon_close.png" onclick="CloseForecastIntroducePoint(' + point.dataID + ')"></div><div class="jcxq_box"><span>[' + point.timeStr + ']</span><span>横摇：' + point.yaoX + '°</span><span>纵摇：' + point.yaoY + '°</span><span>垂荡：' + point.dangZ + 'm</span></div><ul class="yjsx_box">';

    if (point.windAlarm)
    {
        introduceHtml += '<li>' + point.windAlarmContent + '</li>';
    }
    if (point.waveAlarm) {
        introduceHtml += '<li>' + point.waveAlarmContent + '</li>';
    }
    if (point.shakeAlarm) {
        introduceHtml += '<li>' + point.shakeAlarmContent + '</li>';
    }

    introduceHtml += '</ul></div></div></div>';

    var icon = L.divIcon({
        iconSize: [20, 20],
        className: "box-farm",
        html: introduceHtml,
        iconAnchor: [0, 0],//偏移
    });

    //坐标要转换（wgs84转gcj02）
    const wgs84Coord = { lng: point.lon, lat: point.lat }; // 示例坐标
    const gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

    var marker = L.marker([gcj02Coord.lat, gcj02Coord.lng],
        {
            icon: icon,
            zIndexOffset:1000
        }).addTo(_map);  //先不要显示

    //marker.data = dom;
    //marker.bindTooltip(farmName);
    marker.on('click', function (e) {

        //$('.sjlb_content').show();

    });

}



//最后一个点
var drawIntroduceMarker = function (point, list) {


    var introduceHtml = '<div class="sjlb_content"><div class="sjlb_content_close"><img src="/Content/Towing/img/icon_close.png" onclick="CloseForecastList();"></div><div class="sjlb_box">'
        + '<table><thead><tr><th width="30%" >时间</th><th width="20%">横摇</th><th width="20%">纵摇</th><th width="20%">垂荡</th><th width="10%">状态</th></tr></thead><tbody>'
    $(list).each(function (index, dom) {

        var imgHtml = '';
        if (dom.totalAlarm)
        {
            imgHtml = '<img src="/Content/Towing/img/icon_bj.png" onclick="OpenForecastAlarmPoint(' + dom.dataID + ');">';
        }

        introduceHtml += '<tr><td style="color:#ff9703;">' + dom.timeStr + '时</td><td><span>' + dom.yaoX + '</span>°</td><td><span>' + dom.yaoY + '</span>°</td><td><span>' + dom.dangZ + '</span>m</td><td>' + imgHtml + '</td></tr>';

    })
    introduceHtml += '</tbody></table></div></div> ';


    var icon = L.divIcon({
        iconSize: [20, 20],
        className: "box-farm",
        html: introduceHtml,
        iconAnchor: [-10, 30],//偏移
    });

    //坐标要转换（wgs84转gcj02）
    const wgs84Coord = { lng: point.lon, lat: point.lat }; // 示例坐标
    const gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

    var marker = L.marker([gcj02Coord.lat, gcj02Coord.lng],
        {
            icon: icon,
        }).addTo(_map);  //先不要显示

    //marker.data = dom;
    //marker.bindTooltip(farmName);
    marker.on('click', function (e) {

        //$('.sjlb_content').show();

    });

    forecastRoute.forecast = marker;

}

var CloseForecastList = function () {

    $('.sjlb_content').hide();

}

var OpenForecastAlarmPoint = function (dataID) {

    if (dataID == null || dataID == '' || dataID == 0)
    {
        return;
    }

    $('#mapyjxq' + dataID).show();

}

var CloseForecastIntroducePoint = function (dataID) {

    if (dataID == null || dataID == '' || dataID == 0) {
        return;
    }

    $('#mapyjxq' + dataID).hide();

}


// 加载实时路径
var progressPath = {
    show: false,
    append: false,
    lines: null,
    points: [],
    cfg: {
        title: '实际航线',
        color: "rgba(9,246,255,1)",
        pColor: "#fde900",
    },
    list: [],
    lastPoint: null,
    lastMarker: null,
    lastTime: '',
    size: 0, // 初次加载点个数
    refreshSize: 2, // 后续更新点个数
}
var tmpDivIcon = `<img class="icon_ship" src="{{this.icon}}">`;
function showProgressPath() {
    progressPath.load = loadProgressPath;
    progressPath = showCommon(progressPath);
}
function showCommon(cfg) {
    cfg.show = !(cfg.show);
    if (cfg.show) {
        if (cfg.lines) {
            // 已经展示过的不重复展示
            if (cfg.append) {
            } else {
                _map.addLayer(cfg.lines);
                cfg.append = true;
                $.each(cfg.points, function (idx, itm) {
                    _map.addLayer(itm);
                });
            }
        } else {
            cfg.load();// loadPlanPath();
        }
    } else {
        if (cfg.lines) {
            // 已经移除的不再重复移除
            if (cfg.append) {
                _map.removeLayer(cfg.lines);
                cfg.append = false;
                $.each(cfg.points, function (idx, itm) {
                    _map.removeLayer(itm);
                });
            } else {
            }
        }
    }
    return cfg;
}

var IsSetCenter = false;
function loadProgressPath() {
    debugger;
    var cfg = progressPath;
    $.post('/Towing/Main/GetSatellteHistory?startTime=' + cfg.lastTime + '&size=' + cfg.size, function (r) {
        // console.log('GetSatellteHistory:', r);

        //debugger;

        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadProgressPath Error:', r.ErrorMessage);
        if (r && r.List) {
            cfg.list = cfg.list || [];
            var lastTime = cfg.lastTime;
            $.each(r.List, function (idx, itm) {
                if (cfg.lastTime == '' || getTimeDiff(itm.UpdateTime, cfg.lastTime) > 0) {
                    var p = {
                        lat: itm.Latitude, lon: itm.Longitude,
                        azAngle: itm.LatestDirectionAngle, updateTime: itm.UpdateTime.toLocaleString(),
                        iconAnchor: [6, 6], icon: '/Content/Towing/img/icon_hxtz.png', iconSize: [12, 12]
                    };
                    p.title = '[经:' + p.lon + '°,纬:' + p.lat + '°]\n' + p.updateTime.replace('-', '年')
                        .replace('-', '月')
                        .replace('T', '日 ');// getYearDateStr(p.updateTime);

                    cfg.list.push(p);
                    // 起点（码头）和终点（船舶）不用另外画点
                    if (cfg.list.length != 1 && idx < (r.List.length - 1)) {
                        cfg.points.push(getPoint(p));
                    }
                    cfg.lastPoint = p;
                }
                // 初始化为20个点，后续更新都为2个点
                cfg.size = cfg.refreshSize;
            });
            // 仅拉取到新坐标，才更新点
            if (cfg.lastTime == '' || new Date(cfg.lastPoint.updateTime) > new Date(cfg.lastTime)) {
                if (cfg.lines) {
                    // 清理已有的线段
                    _map.removeLayer(cfg.lines);
                }
                cfg.lines = getDYLines(cfg.list, cfg.cfg);
                if (cfg.show) {
                    _map.addLayer(cfg.lines);
                }
                // 去掉最后一个点（是船舶点）单独绘制
                // cfg.lastPoint.icon = '/Content/Towing/img/icon_船拖物品.png';
                cfg.lastPoint.icon = '/Content/Towing/img/icon_border.gif';
                cfg.lastPoint.iconAnchor = [24, 24];
                cfg.lastPoint.iconSize = [48, 48];
                cfg.lastPoint.rotationAngle = cfg.lastPoint.azAngle;
                // cfg.lastPoint.html = TemplateEngine(tmpDivIcon, cfg.lastPoint);
                cfg.lastPoint.className = 'divIcon_ship';
                // 重绘终点（当前船舶所在）
                if (cfg.lastMarker) removeMarker(cfg.lastMarker);
                cfg.lastMarker = drawPoint(cfg.lastPoint);
                // 根据航向旋转角度
                var rCss = 'rotate(' + cfg.lastPoint.azAngle + 'deg)';
                // console.log('rCss', rCss);
                $('.icon_ship').css('transform', rCss);
                if (!IsSetCenter)
                {
                    mapHelper.setCenter(cfg.lastPoint); // 当前被拖物位置设为地图居中点
                    IsSetCenter = true;
                }
                
                cfg.lastTime = cfg.lastPoint.updateTime;
            }
        }

        // 初始化为20个点，后续更新都为2个点
        cfg.size = 2;
        // progressPath.lastTime = '';

        timer.setProgressPath();
    });
}

var getCircle = function () {

    //坐标 半径  （半径单位是米）

    var polygon1 = new L.Circle([34, 108], 120000, {
        color: 'red', //颜色
        fillColor: '#f03',
        fillOpacity: 0.4, //透明度
    });

}


//绘制静态线
var getJTLines = function (routes, cfg) {

    var gcj02Routes = [];

    $(routes).each(function (index, dom) {

        var wgs84Coord = { lng: dom.lon, lat: dom.lat }; // 示例坐标
        var gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

        var point = {};
        point.lon = gcj02Coord.lng;
        point.lat = gcj02Coord.lat;

        gcj02Routes.push(point);

    })

    var Polyline = new L.Polyline(gcj02Routes, {
        weight: 4,
        opacity: 0.8,
        color: cfg.color, //"#00e4ff"
    });
    //Polyline.bindTooltip(cfg.title, {
    //    //className: 'leaflet-labelship',
    //    //permanent: IsFanZoom,
    //    //offset: [-20, -35]
    //});
    return Polyline;

}

// 绘制动态路径
var getDYLines = function (routes, cfg) {

    var gcj02Routes = [];

    $(routes).each(function (index, dom) {

        var wgs84Coord = { lng: dom.lon, lat: dom.lat }; // 示例坐标
        var gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

        var point = {};
        point.lon = gcj02Coord.lng;
        point.lat = gcj02Coord.lat;

        gcj02Routes.push(point);

    })

    var antPolyline = new L.Polyline.AntPath(gcj02Routes, {
        paused: false,
        delay: 2000,
        dashArray: [10, 20],
        weight: 2.5,
        opacity: 0.8,
        color: cfg.color, // "#fe7200",
        pulseColor: cfg.pColor, // "#fde900"
    });
    antPolyline.bindTooltip(cfg.title, {
        //className: 'leaflet-labelship',
        //permanent: IsFanZoom,
        //offset: [-20, -35]
    });
    return antPolyline;
}
var drawDYLine = function (routes, cfg) {
    // routes: [{lat,lon}]
    // cfg: {color:'',pColor:'',title:''}
    var antPolyline = new L.Polyline.AntPath(routes, {
        paused: false,
        delay: 2000,
        dashArray: [10, 20],
        weight: 2.5,
        opacity: 0.8,
        color: cfg.color, // "#fe7200",
        pulseColor: cfg.pColor, // "#fde900"
    });
    antPolyline.bindTooltip(cfg.title, {
        //className: 'leaflet-labelship',
        //permanent: IsFanZoom,
        //offset: [-20, -35]
    }).addTo(_map);
    return antPolyline;
}
var getPoint = function (point) {


    // 自定义图标
    var marker_icon_1;
    if (point.html) {
        marker_icon_1 = L.divIcon({
            html: point.html,
            className: point.className || 'div_icon'
        });
    } else {
        marker_icon_1 = L.icon({
            iconUrl: point.icon, // "https://api.shipxy.com/h5s/api/3.0/images/death.png",
            iconAnchor: point.iconAnchor || [6, 6],//图标锚点
            iconSize: point.iconSize || [12, 12], //图标大小
        });
    }

    //坐标要转换（wgs84转gcj02）
    const wgs84Coord = { lng: point.lon, lat: point.lat }; // 示例坐标
    const gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

    // 添加标记,到地图
    var marker = ShipxyAPI.marker([gcj02Coord.lat, gcj02Coord.lng], {
        icon: marker_icon_1,
        title: '', //point.title,
        rotationAngle: point.rotationAngle ? point.rotationAngle : undefined,
    });

    marker.bindTooltip(point.title, {
        //className: 'leaflet-labelship',
        //permanent: IsFanZoom,
        //offset: [-20, -35]
    });

    return marker;

    

}
var drawPoint = function (point) {
    var marker = getPoint(point);
    if (point.zIndex) {
        marker.setZIndexOffset(point.zIndex);
    }
    marker.addTo(_map);
    return marker;
}

var removeMarker = function (marker) {
    if (marker && marker.remove) marker.remove();
}
var weatherForecost;
var weatherDateIdx = -1;
// fill tab data
var tabHeaderTmp = '<li id="#header{{this.tabID}}" class="{{this.cssClass}}"><a data-toggle="tab" href="/#{{this.tabID}}">{{this.dateStr}}</a></li>';
var tabBodyHeaderTmp = `<th width="20%">{{this.timeStr}}</th>`;
var tabBodyValueTmp = `<td>{{this.valueStr}}</td>`;
var tabBodyTmp = `<div id="{{this.tabID}}" class="tab-pane fade in {{this.cssClass}}">
          <table class="qxxq_table">
            <thead>
              <tr>
                <th width="20%">小时：</th>
              </tr>
            </thead>
            <tbody>
              <tr class="qxxq_table_wd">
                <td>温度(°C)：</td>
              </tr>
              <tr class="qxxq_table_fs">
                <td>风速(m/s)：</td>
              </tr>
              <tr class="qxxq_table_fx">
                <td>风向(°C)：</td>
              </tr>
              <tr class="qxxq_table_yx">
                <td>涌向(°C)：</td>
              </tr>
              <tr class="qxxq_table_lg">
                <td>浪高(m)：</td>
              </tr>
              <tr class="qxxq_table_sd">
                <td>湿度(RH)：</td>
              </tr>
              <tr class="qxxq_table_njd">
                <td>能见度(km)：</td>
              </tr>
            </tbody>
          </table>
        </div>
`;
var dataTmp = `<td>
    <ul class="table_ul">
    <li>2</li>
    <li>8</li>
    <li>14</li>
    <li>20</li>
    </ul>
</td>`;
function fillWeatherForecost(lon, lat) {
    $.post('/Towing/Main/GetWeatherForecase?lon=' + (lon || '') + '&lat=' + (lat || ''), function (r) {
        weatherForecost = r;
        if ($.type(weatherForecost) == 'string') {
            weatherForecost = JSON.parse(weatherForecost);
        }
        if (weatherForecost.ErrorMessage) console.log('fillWeatherForecost Error:', weatherForecost.ErrorMessage);
        var dateIdx = 0;
        var dateTabId = ['one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight'];
        var tabHeader = [];
        var tabBody = [];

        if (weatherForecost) {
            r = weatherForecost;
        }
        // console.log('r:', r);
        var lastDate = '';
        var lastTime = '';
        var tabObj = null;
        var tabHeader = null;
        var tabBody = null;
        $('.qxxq_table thead tr').html('<th width="9%"></th>');
        $('.qxxq_table tbody').html(`
<tr class="qxxq_table_sj">
<td>小时(h)：</td>
</tr>
<tr class="qxxq_table_wd">
<td>温度(°C)：</td>
</tr>
<tr class="qxxq_table_fs">
<td>风速(m/s)：</td>
</tr>
<tr class="qxxq_table_fx">
<td>风向(°)：</td>
</tr>
<tr class="qxxq_table_yx">
<td>涌向(°)：</td>
</tr>
<tr class="qxxq_table_lg">
<td>浪高(m)：</td>
</tr>
<tr class="qxxq_table_sd">
<td>湿度(RH)：</td>
</tr>
<tr class="qxxq_table_njd">
<td>能见度(km)：</td>
</tr>
`);
        lastDate = '';
        lastTime = '';
        weatherDateIdx = -1;

        var thead = $('.qxxq_table thead');
        var tbody = $('.qxxq_table tbody');

        var ulBody_sj, ulBody_wd, ulBody_fs, ulBody_fx, ulBody_yx, ulBody_lg, ulBody_sd, ulBody_njd;
        $.each(r.List, function (idx, itm) {
            //console.log('idx,itm', idx, itm, dateTabId[idx]);
            // 日期变更
            // 新日期，新的表头，新的ul填tbody>tr>td
            // 
            itm.DateStr = getDateStr(itm.Date)
            itm.HourStr = Number((itm.ShortTime || '-').split(':')[0]);
            if (itm.WindDirStr != '未知') {
                itm.WindDirImg = '<img src="/Content/Towing/img/icon_fangxiang.png"';
                if (itm.WindDir) itm.WindDirImg += ' style="transform:rotate(' + itm.WindDir + 'deg)"';
                itm.WindDirImg += "/>";
            }
            if (itm.SwellDirStr != '未知') {
                itm.SwellDirImg = '<img src="/Content/Towing/img/icon_fangxiang.png"';
                if (itm.SwellDir) itm.SwellDirImg += ' style="transform:rotate(' + itm.SwellDir + 'deg)"';
                itm.SwellDirImg += "/>";
            }
            if (idx == 0) {
                $('.qxxq_table thead tr').append('<th width="13%">' + itm.DateStr + '</th>');
                // 初始化
                ulBody_sj = '<td><ul class="table_ul"><li>' + itm.HourStr + '</li>';
                ulBody_wd = '<td><ul class="table_ul"><li>' + (itm.Temperature || '-') + '</li>';
                ulBody_fs = '<td><ul class="table_ul"><li>' + (itm.WindSpeed || '-') + '</li>';
                ulBody_fx = '<td><ul class="table_ul"><li>' + (itm.WindDirImg || '-') + '</li>';
                ulBody_yx = '<td><ul class="table_ul"><li>' + (itm.SwellDirImg || '-') + '</li>';
                ulBody_lg = '<td><ul class="table_ul"><li>' + (itm.SwellHeight || '-') + '</li>';
                ulBody_sd = '<td><ul class="table_ul"><li>' + (itm.Humidity || '-') + '</li>';
                ulBody_njd = '<td><ul class="table_ul"><li>' + (itm.Visibility || '-') + '</li>';

                lastDate = itm.Date;
            } else if (lastDate == itm.Date) {
                // 填写信息
                ulBody_sj += '<li>' + itm.HourStr + '</li>';
                ulBody_wd += '<li>' + (itm.Temperature || '-') + '</li>';
                ulBody_fs += '<li>' + (itm.WindSpeed || '-') + '</li>';
                ulBody_fx += '<li>' + (itm.WindDirImg || '-') + '</li>';
                ulBody_yx += '<li>' + (itm.SwellDirImg || '-') + '</li>';
                ulBody_lg += '<li>' + (itm.SwellHeight || '-') + '</li>';
                ulBody_sd += '<li>' + (itm.Humidity || '-') + '</li>';
                ulBody_njd += '<li>' + (itm.Visibility || '-') + '</li>';
            } else {
                ulBody_sj += '</ul></td>';
                ulBody_wd += '</ul></td>';
                ulBody_fs += '</ul></td>';
                ulBody_fx += '</ul></td>';
                ulBody_yx += '</ul></td>';
                ulBody_lg += '</ul></td>';
                ulBody_sd += '</ul></td>';
                ulBody_njd += '</ul></td>';

                tbody.find('.qxxq_table_sj').append(ulBody_sj);
                tbody.find('.qxxq_table_wd').append(ulBody_wd);
                tbody.find('.qxxq_table_fs').append(ulBody_fs);
                tbody.find('.qxxq_table_fx').append(ulBody_fx);
                tbody.find('.qxxq_table_yx').append(ulBody_yx);
                tbody.find('.qxxq_table_lg').append(ulBody_lg);
                tbody.find('.qxxq_table_sd').append(ulBody_sd);
                tbody.find('.qxxq_table_njd').append(ulBody_njd);

                $('.qxxq_table thead tr').append('<th width="13%">' + itm.DateStr + '</th>');

                ulBody_sj = '<td><ul class="table_ul"><li>' + itm.HourStr + '</li>';
                ulBody_wd = '<td><ul class="table_ul"><li>' + (itm.Temperature || '-') + '</li>';
                ulBody_fs = '<td><ul class="table_ul"><li>' + (itm.WindSpeed || '-') + '</li>';
                ulBody_fx = '<td><ul class="table_ul"><li>' + (itm.WindDirImg || '-') + '</li>';
                ulBody_yx = '<td><ul class="table_ul"><li>' + (itm.SwellDirImg || '-') + '</li>';
                ulBody_lg = '<td><ul class="table_ul"><li>' + (itm.SwellHeight || '-') + '</li>';
                ulBody_sd = '<td><ul class="table_ul"><li>' + (itm.Humidity || '-') + '</li>';
                ulBody_njd = '<td><ul class="table_ul"><li>' + (itm.Visibility || '-') + '</li>';

                lastDate = itm.Date;
            }
            if (lastTime == itm.ShortTime) {
            } else {
                // 新增tbody td
                lastTime = itm.ShortTime;
            }
        });
        tbody.find('.qxxq_table_sj').append(ulBody_sj);
        tbody.find('.qxxq_table_wd').append(ulBody_wd);
        tbody.find('.qxxq_table_fs').append(ulBody_fs);
        tbody.find('.qxxq_table_fx').append(ulBody_fx);
        tbody.find('.qxxq_table_yx').append(ulBody_yx);
        tbody.find('.qxxq_table_lg').append(ulBody_lg);
        tbody.find('.qxxq_table_sd').append(ulBody_sd);
        tbody.find('.qxxq_table_njd').append(ulBody_njd);

        // 启动下一次调用
        timer.setWeatherForecost();
    })
}

// 加载聊天记录
var chatMsg = {
    chatOpen: false,
    lastTime: '', // getNowTime();
    msgTmp: '<li>{{this.Sender}}({{this.CreateTime}}): <span class="{{this.css}}">{{this.MsgContent}}</span></li>',
    isUrgent: false,
    lastMsgId: [], // 保留最近10个，用来解决连续发送消息太快的话，会重复加载某条记录的问题（因为EntityFramework做DateTime比较的时候，无法精确到毫秒）
}
function loadMsg() {
    // 拉取
    $.post('/Towing/Main/MessageListMsg', {
        LastTime: chatMsg.lastTime
    }, function (r) {
        if ($.type(r) == 'string') {
            r = JSON.parse(r);
        }
        if (r.ErrorMessage) console.log('loadMsg Error:', r.ErrorMessage);
        chatMsg.loadMsgResult = r;
        console.log('loadMsg:', r);

        if (r.Data) {
            chatMsg.lastTime = r.Data.LastTime
        }

        appendMsgList(r.List);

        timer.setLoadMsg();
    });
}

function openChatMsgDlg() {
    $('.chatroom_content').animate({ top: '4vw' }, 800);
    $('#chatMsgContent').focus();
    chatMsg.chatOpen = true;
}
function closeChatMsgDlg() {
    $('.chatroom_content').animate({ top: '-75vw' }, 800);
    chatMsg.chatOpen = false;
}
function switchChatMsgDlg() {
    if (chatMsg.chatOpen) {
        closeChatMsgDlg();
    } else {
        openChatMsgDlg();
    }
}
function appendMsg(msg) {
    console.log('appendMsg:', msg);
    if (chatMsg.lastMsgId.indexOf(msg.ID) == -1) {
        msg.CreateTime = new Date(msg.CreateTime).toLocaleString().replaceAll('/', '-');
        if (msg.MsgLevel == 1) {
            msg.css = "text_jinji";
            if (chatMsg.chatOpen == false) {
                openChatMsgDlg();
            }
        }
        $('#chatMsgList').append(TemplateEngine(chatMsg.msgTmp, msg));

        chatMsg.lastMsgId.push(msg.ID);
        if (chatMsg.lastMsgId.length > 10) {
            chatMsg.lastMsgId.shift();
        }
    }
}

function appendMsgList(msgList) {
    if (msgList) {
        $.each(msgList, function (idx, itm) {
            appendMsg(itm);
        })
        scrollBottom('#chatMsgList');
    }
}
function sendMsg() {
    var msg = {
        MsgType: 'text',
        MsgLevel: chatMsg.isUrgent ? 1 : 0,
        MsgContent: $('#chatMsgContent').val(),
        Sender: loginUser.RealName,
        LastTime: chatMsg.lastTime
    };
    // 空白不发
    if ($.trim(msg.MsgContent).length == 0) return;
    $.post('/Towing/Main/MessageSend', msg, function (r) {
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('sendMsg Error:', r.ErrorMessage);
        if (r.Data) {
            chatMsg.lastTime = r.Data.LastTime;
            $('#chatMsgContent').val('');
        }
        appendMsgList(r.List);
    });
}
function openThisImg(img) {
    //console.log('openThisImg', img);
    //$('#showMsgImg').fadeIn();
    //$('#msgImgLarge').prop('src', $(img).prop('src'));
    popupImg(img);
}
var imageExtensions = /\.(jpg|jpeg|png|gif|bmp)$/i;
function initMsgControls() {
    // 回车发送消息
    $('#chatMsgContent').on('keydown', function (e) {
        // 检测是否按下回车键（键码为 13）
        if (e.which === 13) {
            e.preventDefault(); // 阻止默认行为（如换行）
            sendMsg(e.target);
        }
    });
    // 选择图片
    $('#open_picture').on('click', function (e) {
        $('#fileInput').click();
    })
    // 上传图片
    $('#fileInput').on('change', function (event) {
        var files = event.target.files; // 获取选中的文件列表
        // console.log('files:', files);
        if (files && files[0] && files[0].name) {
            if (imageExtensions.test(files[0].name)) {
            } else {
                mask.showMessage('请上传jpg、jpeg、png、gif或bmp格式的图片。');
                return;
            }
        } else {
            mask.showMessage('请选择图片');
            return;
        }
        $('#fileSender').val(loginUser.RealName);
        $('#fileMsgLevel').val((chatMsg.isUrgent ? 1 : 0).toString());
        $('#fileMsgContent').val($('#chatMsgContent').val());
        $('#fileMsgLastTime').val(chatMsg.lastTime);
        var formData = new FormData($('#uploadForm')[0]); // 获取表单数据
        console.log('formData:', formData);
        $.ajax({
            url: '/Towing/Main/MessageUpload', // 替换为你的服务器端处理地址
            type: 'POST',
            data: formData,
            processData: false, // 防止 jQuery 转换数据
            contentType: false, // 防止 jQuery 设置错误的 Content-Type
            success: function (r) {
                r = JSON.parse(r);

                if (r.Data) {
                    chatMsg.lastTime = r.Data.LastTime;
                    $('#chatMsgContent').val('');
                }
                appendMsgList(r.List);
            },
            error: function (xhr, status, error) {
                $('#status').text('文件上传失败：' + error);
            }
        });
    });
    // 关闭图片查看
    $('#showMsgImg_close').on('click', function (e) {
        $('#showMsgImg').fadeOut();
    });

    // 监听输入框的 keydown 事件
    $('#chat_history_searchInput').on('keydown', function (e) {
        // 检测是否按下回车键（键码为 13）
        if (e.which === 13) {
            e.preventDefault(); // 阻止默认行为（如换行）
            queryMsg(e.target);
        }
    });
    enableDragScroll({ container: '.showMsgBody' });
}

var chatHistory = {
    hisTemp: `<li>
        <div>{{this.Sender}}({{this.CreateTime}})</div>
        <div class="{{this.css}}">{{this.MsgContent}}</div>
      </li>`
}
function queryMsg(qInput) {
    console.log('queryMsg:', $(qInput).val());
    $.post('/Towing/Main/MessageListHistory?query=' + $(qInput).val(), function (r) {
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('queryMsg Error:', r.ErrorMessage);
        console.log('queryMsg:', r);
        appendChatHistory(r.List);
    })
}

function appendChatHistory(list) {
    $('#chatHistoryList').html('');
    list.reverse(); // 查询结果正序，显示按倒序
    $.each(list, function (idx, msg) {
        appendChatHistoryRow(msg);
    })
}
function appendChatHistoryRow(msg) {
    msg.CreateTime = new Date(msg.CreateTime).toLocaleString().replaceAll('/', '-');
    if (msg.MsgLevel == 1) {
        msg.css = "text_jinji";
    }
    $('#chatHistoryList').append(TemplateEngine(chatHistory.hisTemp, msg));
}


// 船舶周围情况
var shipSurShips = {
    show: false,
    append: false,
    list: [],
    points: [],
    lastTime: ''
}


function loadShipSurShips() {
    $.post('/Towing/Main/GetShipSurShips', function (r) {
        debugger;
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadShipSurShips Error:', r.ErrorMessage);
        var list = r.List;
        if (list) {
            shipSurShips.list = list;
            if (shipSurShips.show) {
                drawSurShips();
            }
        }
        timer.setLoadShipSurShips();
    })
}
function drawSurShips() {
    if (shipSurShips.points) {
        $.each(shipSurShips.points, function (idx, itm) {
            _map.removeLayer(itm);
        });
    }
    if (shipSurShips.show) {
        if (shipSurShips.list) {
            $.each(shipSurShips.list, function (idx, itm) {
                var marker = drawPoint({ title: itm.ShipName + '(' + itm.MMSI + ')', lat: itm.LatValue, lon: itm.LonValue, icon: '/Content/Towing/img/cb_green.png', iconAnchor: [16, 16], iconSize: [8, 8] });
                shipSurShips.points.push(marker);
            });
        }
    } else {

    }
}

// 锚地展示
var anchorageInfo = {
    show: false,
    append: false,
    list: [],
    areas: [],
    points: [],
    color: 'rgba(0, 230, 130, 1)', // 锚地颜色
}
function loadAnchorages() {
    if (anchorageInfo.list && anchorageInfo.list.length == 0) {
        $.post('/Towing/Main/GetAnchorageInfo', function (r) {
            r = JSON.parse(r);
            debugger;
            if (r.ErrorMessage) console.log('loadAnchorages Error:', r.ErrorMessage);
            anchorageInfo.list = r.List;
            drawAnchorages();
        });
    } else {
        drawAnchorages();
    }
}

function drawAnchorages() {
    if (anchorageInfo.areas.length == 0 && anchorageInfo.points.length == 0) {
        $.each(anchorageInfo.list, function (idx, itm) {

            if (itm.Type == 'Position') {
                anchorageInfo.areas.push(getAnchArea(itm.LatLons, itm.Name));
            }
            if (itm.Type == 'Circle') {
                anchorageInfo.points.push(getAnchPoint(itm.Lat, itm.Lon, itm.Radius, itm.Name));
            }
        })
    }
    if (anchorageInfo.areas) {
        $.each(anchorageInfo.areas, function (idx, area) {
            _map.removeLayer(area);
        })
        $.each(anchorageInfo.points, function (idx, point) {
            _map.removeLayer(point);
        })
    }
    if (anchorageInfo.show) {
        $.each(anchorageInfo.areas, function (idx, area) {
            area.addTo(_map);
        })
        $.each(anchorageInfo.points, function (idx, point) {
            point.addTo(_map);
        })
    }
}
function getAnchArea(latlngs, title) {
    // 多边形：坐标点，注意 第一个 与 最后一个点是无需一样，会自动闭合
    //var latlngs = [
    //    [31.537628173828125, 121.190185546875],
    //    [30.878448486328125, 121.19430541992188],
    //    [30.839996337890625, 121.92901611328125],
    //    [31.348114013671875, 121.8878173828125]
    //];
    var polygon = L.polygon(latlngs, { color: anchorageInfo.color }).bindTooltip(title);//.addTo(map);
    console.log('polygon:', polygon);
    // 跳到多边形的范围
    // map.fitBounds(polygon.getBounds());
    return polygon;
}
function getAnchPoint(lat, lon, radius, title) {
    // 圆：半径 radius 的单位为米
    var circle = L.circle([lat, lon], { radius: radius, color: anchorageInfo.color }).bindTooltip(title);//.addTo(map);
    // 跳到圆的范围
    // map.fitBounds(circle.getBounds());
    return circle;
}

function loadDetailInfo() {
    // 页面加载后，只加载气象预报，设置一天更新一次，且气象信息不受顶部开始、结束日期查询影响
    // 日期控件查询的时候，只查询当前tab页信息，切换tab页要再点一下，
    //  都没有填的时候，查询最近的300条
    //  只填开始日期的时候，查询距离开始日期最近的300条
    //  只填结束日期的时候，查询距离结束日期最近的300条
    //  开始、结束日期都填的时候，在开始、结束日期范围内，查询距离开始日期最近的300条
}
var currTab = '';
function loadDetailTab(tabID) {
    console.log(tabID);
    currTab = tabID || currTab;
    switch (currTab) {
        case '#two':
            loadZMotion();
            break;
        case '#three':
            loadXMotion();
            break;
        case '#four':
            loadWindSpeedTrace()
            break;
        case '#five':
            loadWindDirectionTrace();
            break;
        case '#six':
            loadShipSpeed();
            break;
        case '#seven':
            loadShipDirection();
            break;
        case '#eight':
            loadPhotos();
            break;
        case '#nine':
            loadTowingForecastFile();
            break;
        case '#ten':
            loadTowingReportFile();
            break;
    }
}
var chartZMotion, chartXMotion, chartWindSpeed, chartWindDir, chartShipSpeed, chartShipDir;
function getParam() {
    var param = {
        startTime: $('#queryStartTime').val(),
        endTime: $('#queryEndTime').val()
    }
    if (param.startTime != '') param.startTime += ':00';
    if (param.endTime != '') param.endTime += ':00';

    return param;
}

// 加载垂向运动
function loadZMotion() {
    $.post('/Towing/Main/GetChartZMotion', getParam(), function (r) {
        // console.log('r', r);
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadAnchorages Error:', r.ErrorMessage);
        if (r.IsValid == false) {
            if (chartZMotion) chartZMotion.clear();
            mask.showMessage('未能查询到数据');
        }
        if (r && r.Data) r = r.Data;
        var opt, idx, legend;
        opt = chartOpt.optBase();
        opt.legend = r.Legend;
        opt.legendData = r.LegendDataSet;
        // console.log('chart opt:', opt);
        opt.xsize = 10;
        opt.yFormatter = '{value} m|°';
        opt = chartOpt.getOption(opt);
        opt.data = r.DataSet;

        for (var i = 0; i < r.Legend.length; i++) {
            var legend = r.Legend[i];

            opt.itemStyle = {
                color: opt.color[i]
            }
            idx = r.Legend.indexOf(legend);
            opt.series.push(
                chartOpt.getSerie(opt, idx)
            )
        }
        // console.log('echart option:', opt);
        chartZMotion = chartOpt.drawChart(chartZMotion, opt, 'chartZMotion');

    })
}
// 加载横向运动
function loadXMotion() {
    $.post('/Towing/Main/GetChartXMotion', getParam(), function (r) {
        // console.log('r', r);
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadAnchorages Error:', r.ErrorMessage);
        if (r.IsValid == false) {
            if (chartXMotion) chartXMotion.clear();
            mask.showMessage('未能查询到数据');
        }
        if (r && r.Data) r = r.Data;
        var opt, idx, legend;
        opt = chartOpt.optBase();
        opt.legend = r.Legend;
        opt.legendData = r.LegendDataSet;
        // console.log('chart opt:', opt);
        opt.xsize = 10;
        opt.yFormatter = '{value} m|°';
        opt = chartOpt.getOption(opt);
        opt.data = r.DataSet;

        for (var i = 0; i < r.Legend.length; i++) {
            var legend = r.Legend[i];

            opt.itemStyle = {
                color: opt.color[i]
            }
            idx = r.Legend.indexOf(legend);
            opt.series.push(
                chartOpt.getSerie(opt, idx)
            )
        }
        // console.log('echart option:', opt);
        chartXMotion = chartOpt.drawChart(chartXMotion, opt, 'chartXMotion');

    })
}
// 加载风速历史
function loadWindSpeedTrace() {
    $.post('/Towing/Main/GetChartWindSpeed', getParam(), function (r) {
        // console.log('r', r);
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadAnchorages Error:', r.ErrorMessage);
        if (r.IsValid == false) {
            if (chartWindSpeed) chartWindSpeed.clear();
            mask.showMessage('未能查询到数据');
        }
        if (r && r.Data) r = r.Data;
        var opt, idx, legend;
        opt = chartOpt.optBase();
        opt.legend = r.Legend;
        opt.legendData = r.LegendDataSet;
        // console.log('chart opt:', opt);
        opt.xsize = 10;
        opt.yFormatter = '{value} m/s';
        opt = chartOpt.getOption(opt);
        opt.data = r.DataSet;

        for (var i = 0; i < r.Legend.length; i++) {
            var legend = r.Legend[i];

            opt.itemStyle = {
                color: opt.color[i]
            }
            idx = r.Legend.indexOf(legend);
            opt.series.push(
                chartOpt.getSerie(opt, idx)
            )
        }
        // console.log('echart option:', opt);
        chartWindSpeed = chartOpt.drawChart(chartWindSpeed, opt, 'chartWindSpeed');

    })
}


//定义一个风向转换
var DirectionDiff = 0;

// 加载风向历史
function loadWindDirectionTrace() {
    $.post('/Towing/Main/GetChartWindDir', getParam(), function (r) {
        // console.log('r', r);
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadAnchorages Error:', r.ErrorMessage);
        if (r.IsValid == false) {
            if (chartWindDir) chartWindDir.clear();
            mask.showMessage('未能查询到数据');
        }
        if (r && r.Data) r = r.Data;
        var opt, idx, legend;
        opt = chartOpt.optBase();
        opt.legend = r.Legend;
        opt.legendData = r.LegendDataSet;
        opt.xsize = 10;
        opt.yFormatter = '{value} °';
        opt = chartOpt.getOption(opt);
        opt.data = r.DataSet;


        DirectionDiff = 180;

        //风向的箭头要+180
        //if (r.DataSet != null && r.DataSet.length > 0 && r.DataSet[0] != null)
        //{
        //    var arr = r.DataSet[0];
        //    for (var i = 0; i < arr.length; i++) {
        //        // 通过索引直接修改原数组中的元素
        //        var TempValue = normalizeAngle(arr[i] + 180);
        //        arr[i] = TempValue;
        //    }
        //}
        

        //for (var i = 0; i < r.Legend.length; i++) {
        //    var legend = r.Legend[i];

        //    opt.itemStyle = {
        //        color: opt.color[i]
        //    }
        //    idx = r.Legend.indexOf(legend);
        //    opt.series.push(
        //        chartOpt.getSerie(opt, idx)
        //    )
        //}
        var data = [];
        r.DataSet[0].forEach(function (itm, idx) {
            data.push([opt.xAxis.data[idx], itm]);
        });
        opt = directOption(data, { legend: ['风向'] });
        console.log('chart opt:', opt);
        // console.log('echart option:', opt);
        chartWindDir = chartOpt.drawChart(chartWindDir, opt, 'chartWindDir');

    })
}
// 加载航速历史
function loadShipSpeed() {
    // 待实现，当前没有船舶速度历史记录
    $.post('/Towing/Main/GetChartShipSpeed', getParam(), function (r) {
        // console.log('r', r);
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadShipSpeed Error:', r.ErrorMessage);
        if (r.IsValid == false) {
            if (chartShipSpeed) chartShipSpeed.clear();
            mask.showMessage('未能查询到数据');
        }
        if (r && r.Data) r = r.Data;
        var opt, idx, legend;
        opt = chartOpt.optBase();
        opt.legend = r.Legend;
        opt.legendData = r.LegendDataSet;
        // console.log('chart opt:', opt);
        opt.xsize = 10;
        opt.yFormatter = '{value} 节';
        opt = chartOpt.getOption(opt);
        opt.data = r.DataSet;

        for (var i = 0; i < r.Legend.length; i++) {
            var legend = r.Legend[i];

            opt.itemStyle = {
                color: opt.color[i]
            }
            idx = r.Legend.indexOf(legend);
            opt.series.push(
                chartOpt.getSerie(opt, idx)
            )
        }
        // console.log('echart option:', opt);
        chartShipSpeed = chartOpt.drawChart(chartShipSpeed, opt, 'chartShipSpeed');

    })

}
// 加载航向历史
function loadShipDirection() {
    $.post('/Towing/Main/GetChartShipDir', getParam(), function (r) {
        // console.log('r', r);
        r = JSON.parse(r);
        if (r.ErrorMessage) console.log('loadShipDirection Error:', r.ErrorMessage);
        if (r.IsValid == false) {
            if (chartShipDir) chartShipDir.clear();
            mask.showMessage('未能查询到数据');
        }
        if (r && r.Data) r = r.Data;
        var opt, idx, legend;
        opt = chartOpt.optBase();
        opt.legend = r.Legend;
        opt.legendData = r.LegendDataSet;
        opt.xsize = 10;
        opt.yFormatter = '{value} °';
        opt = chartOpt.getOption(opt);
        opt.data = r.DataSet;

        DirectionDiff = 0;

        //for (var i = 0; i < r.Legend.length; i++) {
        //    var legend = r.Legend[i];

        //    opt.itemStyle = {
        //        color: opt.color[i]
        //    }
        //    idx = r.Legend.indexOf(legend);
        //    opt.series.push(
        //        chartOpt.getSerie(opt, idx)
        //    )
        //}
        var data = [];
        r.DataSet[0].forEach(function (itm, idx) {
            data.push([opt.xAxis.data[idx], itm]);
        });

        opt = directOption(data, { legend: ['航向'] });
        console.log('chart opt:', opt);
        chartShipDir = chartOpt.drawChart(chartShipDir, opt, 'chartShipDir');

    })
}
// 加载现场照片
var photos = {
    pageNo: 1,
    pageSize: 50,
    ul: null,
    list: []
}

function snapPics() {
    $.post('/Towing/Main/GetSnapPic', function (r) {
        timer.setGetSnapPic();
    });
}

function loadPhotos(step) {
    if (!(photos.ul)) {
        photos.ul = $('#eight .xczp_box ul');
    }
    step = step || 0;
    var param = getParam();
    if (step == 0) {
        param.pageNo = 1;
    } else {
        param.pageNo = phtots.pageNo + step;
    }
    param.pageSize = photos.pageSize || 50;
    $.post('/Towing/Main/GetHikLocalPics', param, function (r) {
        r = getRes(r, 'loadPhotos.step:' + step);
        console.log('loadPhotos', r);
        if (r && r.msg == 'success' && r.data && r.data.list) {
            photos.list = r.data.list;
            var photoStr = '';
            $.each(photos.list, function (idx, itm) {
                photoStr += '<li style="text-align: center;"><img alt="' + itm.SnapTime + '" src="' + itm.SnapPicUrl + '" class="zoomable-img">'
                    + '<span>' + (new Date(itm.SnapTime)).toLocaleString() + '</span></li>';
            })

            photos.ul.html(photoStr);
            bindImgenLarge();

            //if (photoStr) {
            //    photos.ul.html(photoStr);
            //    bindImgenLarge();
            //}
        } else {
            //mask.showMessage('未找到照片');
        }
    });
}

//function loadTowingForecastFile() {

//    var param = getParam();
    
//    $.post('/Towing/Main/GetTowingForecastFile', param, function (r) {

//        r = JSON.parse(r);

//        if (r && r.msg == 'success' && r.data && r.data.list) {
//            photos.list = r.data.list;
//            var photoStr = '';
//            $.each(photos.list, function (idx, itm) {
//                photoStr += '<li style="text-align: center;"><img alt="' + itm.SnapTime + '" src="' + itm.SnapPicUrl + '" class="zoomable-img">'
//                    + '<span>' + (new Date(itm.SnapTime)).toLocaleString() + '</span></li>';
//            })

//            photos.ul.html(photoStr);
//            bindImgenLarge();

//        } else {
//            //mask.showMessage('未找到照片');
//        }
//    });
//}




var arrowSize = 28;
var dims = {
    time: 0,
    degree: 1
}

var degreesToRadians = function (degrees) {
    return (0 - degrees + 90 + DirectionDiff) * (Math.PI / 180);
}


var renderArrow = function (param, api) {
    var point = api.coord([
        api.value(dims.time),
        api.value(dims.degree)
    ]);
    var p = { t: api.value(dims.time), d: api.value(dims.degree) };
    // console.log('renderArrow:', param, point, api.value(dims.time), api.value(dims.degree));
    return {
        type: 'path',
        shape: {
            pathData: 'M31 16l-15-15v9h-26v12h26v9z',
            x: -arrowSize / 2,
            y: -arrowSize / 2,
            width: arrowSize,
            height: arrowSize
        },
        rotation: degreesToRadians(api.value(dims.degree)),
        position: point,
        style: api.style({
            fill: '#00ff99',
            stroke: '#555',
            lineWidth: 1
        })
    };
};

var directOption = function (data, option) {
    option = option || {};
    return {
        title: null,
        tooltip: {
            trigger: 'axis',
            formatter: function (params) {
                return [
                    '时间:' + params[0].value[dims.time],
                    '方向(°)：' + params[0].value[dims.degree] + '(' + getWindDirection(params[0].value[dims.degree]) + ')'
                ].join('<br>');
            }
        },
        legend: {
            show: true,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            x: '46%',
            top: '5%',
            textStyle: {
                fontSize: 30,
                color: option.textColor || '#fff',
            },
            data: option.legend || ['']
        },
        grid: {
            top: '20%',
            bottom: '10%',
            left: '5%',
            right: '2%',
        },
        xAxis: {
            type: 'category',
            maxInterval: 360,
            splitLine: {
                lineStyle: {
                    color: '#ddd'
                }
            },
            axisLabel: {

                textStyle: {
                    color: "rgba(204, 231, 255, 1)", //轴线和单位颜色
                }
            }
        },
        yAxis: [
            {
                axisLine: {
                    lineStyle: {
                        color: '#666'
                    }
                },
                axisLabel: {
                    formatter: '{value}°',
                    textStyle: {
                        color: "rgba(204, 231, 255, 1)", //轴线和单位颜色
                    }
                }
            }
        ],
        series: [
            {
                type: 'custom',
                renderItem: renderArrow,
                encode: {
                    x: dims.time,
                    y: dims.degree
                },
                data: data,
                z: 10
            },
            {
                name: option.legend[0] || '',
                type: 'line',
                symbol: 'none',
                areaStyle: {
                    color: {
                        type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
                        colorStops: [{
                            offset: 0, color: 'rgba(0, 243, 255, 0.6)' // 0% 处的颜色
                        }, {
                            offset: 1, color: 'rgba(0, 243, 255, 0.1)' // 100% 处的颜色
                        }],
                        global: false // 缺省为 false
                    },
                },
                encode: {
                    x: dims.time,
                    y: dims.degree
                },
                lineStyle: {
                    color: '#aaa',
                    type: 'dotted'
                },
                data: data,
                z: 1
            }
        ]
    };
}
var chartOpt = {};
chartOpt.optBase = function () {
    return {
        backgroundColor: '',
        lineColor: '',
        legend: [],
        legendData: [],
        xsize: 1, // 每隔多少数据才显示legend
        grid: {
            top: '20%',
            bottom: '10%',
            left: '5%',
            right: '2%',
        },
        itemStyle: {
            color: 'rgba(0, 243, 255, 0.6)'
        },
        areaLowColor: 'rgba(0, 243, 255, 0.6)',
        areaHightColor: 'rgba(0, 243, 255, 0.1)',
        data: []
    }
}
chartOpt.getOption = function (option) {
    return {
        color: ['#80FFA5', '#00DDFF', '#37A2FF', '#FF0087', '#FFBF00'],
        backgroundColor: option.backgroundColor || 'rgba(200, 200, 200, 0)',
        legend: {
            show: true,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            x: '46%',
            top: '5%',
            textStyle: {
                fontSize: 30,
                color: option.textColor || '#fff',
            },
            data: option.legend || ['legend']
        },
        grid: option.grid || {
            top: '20%',
            bottom: '10%',
        },
        xAxis: {
            type: 'category',
            boundaryGap: true,
            axisTick: {
                show: false
            },
            //axisLabel: {
            //    formatter: function (idx, value) {
            //        if (idx % option.xsize == 0) {
            //            return value;
            //        } else {
            //            return '';
            //        }
            //    }
            //},
            axisLine: {
                lineStyle: {
                    color: option.lineXColor || "rgba(204, 231, 255, 1)", //轴线和单位颜色
                }
            },

            data: option.legendData[0] || [] // yAxisData
        },
        tooltip: {
            trigger: 'axis'
        },
        yAxis: {
            type: 'value',

            axisLabel: {
                formatter: option.yFormatter || '{value}m',
                show: true,
                textStyle: {
                    color: option.lineYColor || "rgba(204, 231, 255, 1)", //轴线和单位颜色
                }
            },
            splitLine: {
                show: true,
                lineStyle: {
                    color: option.lineYColor || "rgba(204, 231, 255, 1)", //轴线和单位颜色,
                    type: [2, 3],
                    dashOffset: 2
                }
            }
        },
        series: []
    };
}
chartOpt.getSerie = function (option, idx) {
    idx = idx || 0;
    console.log('geSerie:', option, idx, option.legend[idx])
    return {
        name: option.legend.data && option.legend.data[idx] ? option.legend.data[idx] : 'legend',
        type: 'line',

        smooth: true,
        symbol: 'circle',
        symbolSize: 8,
        itemStyle: option.itemStyle || {
            color: 'rgba(0, 243, 255, 0.6)'
        },
        areaStyle: {
            color: {
                type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
                colorStops: [{
                    offset: 0, color: option.areaLowColor || 'rgba(0, 243, 255, 0.6)' // 0% 处的颜色
                }, {
                    offset: 1, color: option.areaHightColor || 'rgba(0, 243, 255, 0.1)' // 100% 处的颜色
                }],
                global: false // 缺省为 false
            },
        },
        lineStyle: {
            color: option.lineColor || 'rgba(0, 243, 255, 0.6)',
            width: 2,
            type: 'solid'
        },
        //markPoint: {
        //    label: {
        //        normal: {
        //            textStyle: {
        //                color: '#fff'
        //            }
        //        }
        //    },
        //    data: [{
        //        type: 'max',
        //        name: '最大值',

        //    }, {
        //        type: 'min',
        //        name: '最小值'
        //    }]
        //},
        // 最高点标点
        data: (option.data[idx] || []).map(function (item) {
            //var maxValue = Math.max.apply(null, option.data || []);

            //if (item === maxValue) {
            //    return {
            //        value: item,
            //        symbol: 'circle',
            //        symbolSize: 8,
            //        itemStyle: {
            //            color: '#fff',
            //            shadowColor: 'rgba(255,255,255,0.6)',
            //            shadowBlur: 10
            //        }
            //    };
            //}

            return item;
        })
    };
}
chartOpt.drawChart = function (chart, opt, id) {
    if (!(chart)) {
        var chartdom = document.getElementById(id);
        chart = echarts.init(chartdom);
    }
    chart.setOption(opt);
    chart.resize();
    return chart;
}

// 配置保存
var cfgSetting = {
    projectID: null,
    planRouteTmp: `<div class="ydhx_box_son clearfix">
<div>{{this.idx}}</div>
<div><label>经度：</label><input type="text" data-field="lon" class="form-control" placeholder="请输入经度" value="{{this.lon}}"> </div>
<div><label>纬度：</label><input type="text" data-field="lat" class="form-control" placeholder="请输入纬度" value="{{this.lat}}"></div>
<div><label>预计时间：</label><input type="text" data-field="time" placeholder="请输入时间" value="{{this.routeTime}}" class="laydate-icon timeUstyle stateUTime form-control" onclick="laydate({istime: true,format:'YYYY-MM-DD hh:mm'})"></div>
<div><a class="box_del"><i class="fa fa-minus"></i></a></div>
</div>`
}
function loadProjSetting() {
    $.post('/Towing/Main/GetProjInfo', function (r) {
        r = getRes(r, 'loadProjSetting');
        bindProjSetting(r);
    })
}
function bindProjSetting(r) {
    if (r && r.Data) r = r.Data;
    /*
EndLat: 21.4436833
EndLon: 111.2502333
MMSI: "412374560"
ProjectID: 1
ProjectName: "三峡阳江16兆瓦漂浮式风电项目"
ProjectStartTime: null
ShipName: "三航拖3003"
StartLat: 21.5537666
StartLon: 109.5650166
    */
    cfgSetting.projectID = r.ProjectID;
    $('#cfgProjID').val(r.ProjectID);
    $('#cfgProjName').val(r.ProjectName);
    $('#cfgMMSI').val(r.MMSI);
    $('#cfgShipName').val(r.ShipName);
    $('#cfgProjStartTime').val((r.ProjectStartTime || '').replace('T', ' '));
    $('#cfgStartLon').val(r.StartLon);
    $('#cfgStartLat').val(r.StartLat);
    $('#cfgEndLon').val(r.EndLon);
    $('#cfgEndLat').val(r.EndLat);
    $('.title_fc_img').html(r.ProjectName);
}
function saveProjSetting() {

    if (loginUser.RoleID != 1)
    {
        alert('你不是管理员，不能修改。');
        return;
    }

    // 保存后，要刷新当前页面和配置有关的部分
    var projSetting = {
        ProjectID: $('#cfgProjID').val(),
        ProjectName: $('#cfgProjName').val(),
        MMSI: $('#cfgMMSI').val(),
        ShipName: $('#cfgShipName').val(),
        ProjectStartTime: $('#cfgProjStartTime').val(),
        StartLon: $('#cfgStartLon').val(),
        StartLat: $('#cfgStartLat').val(),
        EndLon: $('#cfgEndLon').val(),
        EndLat: $('#cfgEndLat').val()
    }
    $.post('/Towing/Main/SaveProjInfo', projSetting, function (r) {

        if (r == '')
        {
            alert('没有权限修改');
            return;
        }

        r = getRes(r, 'SaveProjInfo');
        if (r && r.Data) {
            if (projSetting.ProjectName) {
                $('.title_fc_img').html(projSetting.ProjectName);
            }
        }
    })
}
var planRouteSetting = {
    projectID: null,
    idx: 1,
    tmp: `<div class="ydhx_box_son clearfix">
<div class="ydhx_box_son_idx">{{this.idx}}</div>
<div><label>经度：</label><input type="text" data-field="lon" class="form-control" value="{{this.Lon || ''}}" placeholder="请输入经度"></div>
<div><label>纬度：</label><input type="text" data-field="lat" class="form-control" value="{{this.Lat || ''}}" placeholder="请输入纬度"></div>
<div><label>预计时间：</label><input type="text" data-field="time" value="{{this.RouteTime || ''}}" placeholder="请输入时间" class="laydate-icon timeUstyle stateUTime form-control" onclick="laydate({istime: true,format:'YYYY-MM-DD hh:mm'})"></div>
<div><a class="box_del" onclick="removeRoutePoint(this)"><i class="fa fa-minus"></i></a></div>
</div>`
}
function addRoutePoint(point) {
    point = point || {};
    point.idx = planRouteSetting.idx;
    point.RouteTime = (point.RouteTime || '').replace('T', ' ');
    if (point.RouteTime.indexOf('9999') == -1) { // 去掉
        $('.mart2').before(TemplateEngine(planRouteSetting.tmp, point));
        planRouteSetting.idx += 1;
    }
}
function removeRoutePoint(btn) {
    $(btn).parent().parent().remove();
    $.each($('.ydhx_box_son_idx'), function (idx, itm) {
        // console.log('idx, itm:', idx, itm);
        $(itm).html(idx + 1);
    });
    planRouteSetting.idx -= 1;
}
function appendPlanRouteSetting(r) {
    r = getRes(r);
    if (r && r.List) {
        $('.ydhx_box_son').remove();
        $.each(r.List, function (idx, itm) {
            // 因原list添加了项目起点、终点，因此配置时去掉此两点
            if (idx > 0 && idx < (r.List.length - 1)) {
                planRouteSetting.projectID = itm.ProjectID;
                addRoutePoint(itm);
            }
        })
    }
}

function savePlanRouteSetting() {

    if (loginUser.RoleID != 1)
    {
        alert('你不是管理员。不能修改。')
        return;
    }

    // 保存后，要刷新当前页面的计划路径
    var list = [];
    $.each($('.ydhx_box_son'), function (idx, pitem) {
        pitem = $(pitem);
        var point = {
            ProjectID: planRouteSetting.projectID,
            Lon: pitem.find('[data-field=lon]').val(),
            Lat: pitem.find('[data-field=lat]').val(),
            RouteTime: pitem.find('[data-field=time]').val(),
        }
        list.push(point);
    });
    console.log('list:', list);
    $.post('/Towing/Main/SaveProjPlanRoute/', { list: list }, function (r) {

        if (r == '')
        {
            alert('没有权限修改');
            return;
        }

        if ($.type(r) == "string") r = JSON.parse(r);
        if (r.ErrorMessage) console.log('saveProjSetting Error:', r.ErrorMessage);
        drawPlanPath(r); // 刷新计划路径
    })
}

function DataExport() {

    var starttime = $('#DataStartTime').val();
    var endtime = $('#DataEndTime').val();

    window.location.href = "DataExport?startTime=" + starttime + "&endTime=" + endtime;

}

function PicExport() {

    var starttime = $('#DataStartTime').val();
    var endtime = $('#DataEndTime').val();

    window.location.href = "PicExport?startTime=" + starttime + "&endTime=" + endtime;

}

function DataUpload() {

    var formData = new FormData();

    var Files = $('#file_TowingReportFile')[0].files;

    if (Files == null || Files.length == 0)
    {
        alert('没有选择任何文件');
        return;
    }

    var FileCount = Files.length;
    formData.append("FileCount", FileCount);
    var Count = 1;
    $(Files).each(function (index, dom) {

        formData.append("File" + Count, dom);

        Count++;
    });
        
    $.ajax({
        url: 'UploadTowingReport',
        dataType: 'json',
        type: 'POST',
        async: false,
        data: formData,
        processData: false, // 使数据不做处理
        contentType: false, // 不要设置Content-Type请求头
        success: function (data) {

            if (data) {
                if (data.Success) {

                    alert('上传成功');

                }
                else {
                    alert(data.Message);
                }

            }

        },
        error: function (response) {
            console.log(response);
        },
        complete: function (data) {
        }
    });
}


function loadTowingReportFile() {

    var param = getParam();
    $.post('/Towing/Main/GetTowingReportFile', param, function (r) {

        if (r.Success)
        {

            var FileHtml = '';
            
            $(r.Data).each(function (index, dom) {

                var FileIcon = 'icon_qtwd.png';
                if (dom.FileExtension == 'doc' || dom.FileExtension == 'docx')
                {
                    FileIcon = 'icon_word.png';
                }
                else if (dom.FileExtension == 'xls' || dom.FileExtension == 'xlsx' || dom.FileExtension == 'csv')
                {
                    FileIcon = 'icon_excel.png';
                }
                else if (dom.FileExtension == 'pdf')
                {
                    FileIcon = 'icon_pdf.png';
                }
                else if (dom.FileExtension == 'png' || dom.FileExtension == 'jpg' || dom.FileExtension == 'img' || dom.FileExtension == 'jpeg')
                {
                    FileIcon = 'icon_pic.png';
                } 

                FileHtml += '<div class="wd_box clearfix col-lg-2 col-sm-3"><div class="left_wdicon" ><img src="/Content/Towing/img/' + FileIcon + '"></div><div class="right_edxiangqing"><div><a href="' + dom.FilePath + '" download="' + dom.FileName + '">' + dom.FileName.substr(0,30) + '</a></div><div>' + dom.CreateTime + '</div></div></div> ';

            })

            $('#div_TowingFile').html(FileHtml);

        }


        //alert(r);

        //if(r.Data)

        //r = getRes(r, 'loadPhotos.step:' + step);
        //console.log('loadPhotos', r);
        //if (r && r.msg == 'success' && r.data && r.data.list) {
        //    photos.list = r.data.list;
        //    var photoStr = '';
        //    $.each(photos.list, function (idx, itm) {
        //        photoStr += '<li style="text-align: center;"><img alt="' + itm.SnapTime + '" src="' + itm.SnapPicUrl + '" class="zoomable-img">'
        //            + '<span>' + (new Date(itm.SnapTime)).toLocaleString() + '</span></li>';
        //    })

        //    photos.ul.html(photoStr);
        //    bindImgenLarge();

        //    //if (photoStr) {
        //    //    photos.ul.html(photoStr);
        //    //    bindImgenLarge();
        //    //}
        //} else {
        //    //mask.showMessage('未找到照片');
        //}
    });

}

function loadTowingForecastFile() {

    var param = getParam();
    $.post('/Towing/Main/GetTowingForecastFile', param, function (r) {

        if (r.Success) {

            var FileHtml = '';

            $(r.Data).each(function (index, dom) {

                var FileIcon = 'icon_qtwd.png';
                if (dom.FileExtension == 'doc' || dom.FileExtension == 'docx') {
                    FileIcon = 'icon_word.png';
                }
                else if (dom.FileExtension == 'xls' || dom.FileExtension == 'xlsx' || dom.FileExtension == 'csv') {
                    FileIcon = 'icon_excel.png';
                }
                else if (dom.FileExtension == 'pdf') {
                    FileIcon = 'icon_pdf.png';
                }
                else if (dom.FileExtension == 'png' || dom.FileExtension == 'jpg' || dom.FileExtension == 'img' || dom.FileExtension == 'jpeg') {
                    FileIcon = 'icon_pic.png';
                }

                FileHtml += '<div class="wd_box clearfix col-lg-2 col-sm-3"><div class="left_wdicon" ><img src="/Content/Towing/img/' + FileIcon + '"></div><div class="right_edxiangqing"><div><a href="' + dom.FilePath + '" download="' + dom.FileName + '">' + dom.FileName.substr(0, 30) + '</a></div><div>' + dom.CreateTime + '</div></div></div> ';

            })

            $('#div_TowingForecastFile').html(FileHtml);

        }


    });

}

var projectArea = {
    IsShow: false,
    name: '项目区域',
    points: [],
    cfg: {
        title: '项目区域',
        color: "#00e4ff",
        pColor: "#aae9fa"
    },
    area: null,
    color: 'rgba(233, 93, 123, 1)', // 颜色
}
var loadProjectArea = function () {

    var param = getParam();
    $.post('/Towing/Main/LoadProjectArea', param, function (r) {

        if (r.Success) {

            $(r.Data).each(function (index, dom) {

                var position = [];
                position.push(dom.Lat);
                position.push(dom.Lon);
                projectArea.points.push(position);

            })

            projectArea.area = drawProjectArea(projectArea.points, projectArea.name);

            
        }
    });
}

function drawProjectArea(latlngs, title) {
    // 多边形：坐标点，注意 第一个 与 最后一个点是无需一样，会自动闭合
    //var latlngs = [
    //    [31.537628173828125, 121.190185546875],
    //    [30.878448486328125, 121.19430541992188],
    //    [30.839996337890625, 121.92901611328125],
    //    [31.348114013671875, 121.8878173828125]
    //];
    var polygon = L.polygon(latlngs, { color: projectArea.color }).bindTooltip(title);//.addTo(map);
    console.log('polygon:', polygon);
    // 跳到多边形的范围
    // map.fitBounds(polygon.getBounds());
    return polygon;
}

var displayProjectArea = function (IsDisplay) {

    if (IsDisplay) {
        projectArea.area.addTo(_map);
    }
    else
    {
        _map.removeLayer(projectArea.area);
    }
}

//var displayProjectArea = function() {

//    if (projectArea.IsShow) {
//        _map.removeLayer(projectArea.area);
//    }
//    else
//    {
//        projectArea.area.addTo(_map);
//    }
//    projectArea.IsShow = !projectArea.IsShow;
//}

var loadTyphoonForecast = function () {

    var param = getParam();
    $.post('/Towing/Main/LoadTyphoonForecast', param, function (r) {

        if (r.Success) {

            var TyphoonHtml = '';

            if (r.Data.length == 0) {
                //CloseTyphoonPopup();
                TyphoonHtml = '暂无';
            }
            else {

                //OpenTyphoonPopup();

                

                $(r.Data).each(function (index, dom) {

                    TyphoonHtml += '<div class="tfyj_tit_sm" data-XuHao="' + dom.xuHao + '"><img src="/Content/Towing/img/icon_tfmc.png"><span style="color:red;">拖航过程中可能遭遇台风</span>  ' + dom.chnName + '</div>';
                    TyphoonHtml += '<ul class="list01 clearfix">';
                    $(dom.Typhoons).each(function (index2, dom2) {

                        TyphoonHtml += '<li><div>' + dom2.forecastTimeStr + '</div><div><span><i>' + dom2.grade + '</i>级</span><span><i>' + dom2.speed + '</i>米/秒</span><span>经过<i>(' + dom2.lon + ',' + dom2.lat + ')</i></span></div></li> ';

                    })
                    TyphoonHtml += '</ul>';
                })

                
            }
            $('#div_htmlTyphoonAlarm').html(TyphoonHtml);
            

        }

        timer.setTyphoonForecast();
    });

}

//查看风机轨迹详情
var OpenTyphoonInfo = function () {

    if (!$("#chkTaifeng").prop("checked")) {
        displayProjectArea(true);
        OpenTyphoonList();

        setTaifengChecked(true);
    }
    else {
        displayProjectArea(false);
        CloseTyphoonList();

        setTaifengChecked(false);
    }

    

}


var OpenTyphoonPopup = function () {

    $('.tfyj_content').show();

}
var CloseTyphoonPopup = function () {

    $('.tfyj_content').hide();

}




var AddPointnMarker = function (Position) {

    //Position.Lat = 21.6041091;
    //Position.Lon = 109.5733875;

    var IconUrl = "/Content/Towing/img/cb_red.png";

    var icon = L.icon({
        iconSize: [6, 6],
        iconUrl: IconUrl,
        iconAnchor: [3, 3]
    });

    //坐标要转换（wgs84转gcj02）
    const wgs84Coord = { lng: Position.lon, lat: Position.lat }; // 示例坐标
    const gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);

    var marker = L.marker([gcj02Coord.lat, gcj02Coord.lng],
        {
            icon: icon,
        }).addTo(_map);
}


function wgs84ToGcj02(lng, lat) {
    // 常量定义
    const PI = Math.PI;
    const a = 6378245.0; // 地球半径
    const ee = 0.00669342162296594323; // 扁率

    // 判断坐标是否在国内，不在在这里处理，直接进行转换

    let dLat = transformLat(lng - 105.0, lat - 35.0);
    let dLng = transformLng(lng - 105.0, lat - 35.0);
    const radLat = lat / 180.0 * PI;
    let magic = Math.sin(radLat);
    magic = 1 - ee * magic * magic;
    const sqrtMagic = Math.sqrt(magic);
    dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * PI);
    dLng = (dLng * 180.0) / (a / sqrtMagic * Math.cos(radLat) * PI);
    const mgLat = lat + dLat;
    const mgLng = lng + dLng;

    return { lng: mgLng, lat: mgLat };
}

/**
 * 纬度转换辅助函数
 */
function transformLat(x, y) {
    let ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.sqrt(Math.abs(x));
    ret += (20.0 * Math.sin(6.0 * x * Math.PI) + 20.0 * Math.sin(2.0 * x * Math.PI)) * 2.0 / 3.0;
    ret += (20.0 * Math.sin(y * Math.PI) + 40.0 * Math.sin(y / 3.0 * Math.PI)) * 2.0 / 3.0;
    ret += (160.0 * Math.sin(y / 12.0 * Math.PI) + 320 * Math.sin(y * Math.PI / 30.0)) * 2.0 / 3.0;
    return ret;
}

/**
 * 经度转换辅助函数
 */
function transformLng(x, y) {
    let ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.sqrt(Math.abs(x));
    ret += (20.0 * Math.sin(6.0 * x * Math.PI) + 20.0 * Math.sin(2.0 * x * Math.PI)) * 2.0 / 3.0;
    ret += (20.0 * Math.sin(x * Math.PI) + 40.0 * Math.sin(x / 3.0 * Math.PI)) * 2.0 / 3.0;
    ret += (150.0 * Math.sin(x / 12.0 * Math.PI) + 300.0 * Math.sin(x / 30.0 * Math.PI)) * 2.0 / 3.0;
    return ret;
}

// const wgs84Coord = { lng: 116.403874, lat: 39.914885 }; // 示例坐标
// const gcj02Coord = wgs84ToGcj02(wgs84Coord.lng, wgs84Coord.lat);
// console.log("转换后的坐标:", gcj02Coord);

function normalizeAngle(angle) {
    // 先将角度转换为其等效的在-360到360之间的值
    const remainder = angle % 360;

    // 如果结果是360的倍数，返回0
    if (remainder === 0) {
        return 0;
    }

    // 如果结果为负数，加上360使其变为正数
    return remainder > 0 ? remainder : remainder + 360;
}

function getWindDirection(degrees) {
    // 确保角度在0-360度范围内
    degrees = (degrees % 360 + 360) % 360;

    // 风向定义及对应的角度范围
    if (degrees >= 337.5 || degrees < 22.5) {
        return '北';
    } else if (degrees >= 22.5 && degrees < 67.5) {
        return '东北';
    } else if (degrees >= 67.5 && degrees < 112.5) {
        return '东';
    } else if (degrees >= 112.5 && degrees < 157.5) {
        return '东南';
    } else if (degrees >= 157.5 && degrees < 202.5) {
        return '南';
    } else if (degrees >= 202.5 && degrees < 247.5) {
        return '西南';
    } else if (degrees >= 247.5 && degrees < 292.5) {
        return '西';
    } else if (degrees >= 292.5 && degrees < 337.5) {
        return '西北';
    }
}