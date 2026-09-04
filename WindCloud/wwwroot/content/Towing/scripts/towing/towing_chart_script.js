// JavaScript Document
$(document).ready(function () {

    //点开工程 弹出所属工程详情
    $('#thgc').click(function () {
        $('#marquee1').hide();
        $('#thgc_son').show();

    });
    $('#exit').click(function () {
        $('#thgc_son').hide();
        $('#marquee1').show();

    });

    initMap();
    initControls();
    initLoading();
});

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

    ShipxyAPI.TrackService(_map);
}

var towingChartControls = {
};
function fnW(str) {
    var num;
    str >= 10 ? num = str : num = "0" + str;
    return num;
}
var timer
function initControls() {
    //获取当前时间
    timer = setInterval(function () {
        var date = new Date();
        var year = date.getFullYear(); //当前年份
        var month = date.getMonth(); //当前月份
        var data = date.getDate(); //天
        var hours = date.getHours(); //小时
        var minute = date.getMinutes(); //分
        var second = date.getSeconds(); //秒
        var day = date.getDay(); //获取当前星期几 
        var daysOfWeek = ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];
        var chineseDay = daysOfWeek[day];

        $('#time').html(fnW(hours) + ":" + fnW(minute) + ":" + fnW(second));
        $('#year').html(year + '-' + (month + 1) + '-' + data)
        $('#date').html(chineseDay)

    }, 1000)
}

function initLoading() {
    loadShipInfo();

    loadMRU();

    loadEGYRO();

    loadWindInfo();
}

var timer = {
    loadShipInfo: 5 * 60 * 1000, // 刷新船舶信息：5分钟
    shipInfo: null, //
    setShipInfo: function () {
        if (timer.shipInfo) {
            clearTimeout(timer.shipInfo);
        }
        timer.shipInfo = setTimeout(loadShipInfo, timer.loadShipInfo);
    },
    loadMRU: 5 * 60 * 1000, // 刷新MRU信息：5分钟
    mru: null, //
    loadEGYRO: 5 * 60 * 1000, // 刷新电磁罗经：5分钟
    egyro: null, //

}

var shipInfo = {
    info: null,
    lastTime: '',
    point: null,
}
function loadShipInfo() {
    $.post('/Towing/Chart/GetShipInfo', function (r) {
        r = JSON.parse(r);
        shipInfo.info = r.Data;
        if (shipInfo.point) _map.removeLayer(shipInfo.point);
        shipInfo.point = mapHelper.getPoint({
            lat: r.Data.Latitude, lon: r.Data.Longitude,
            iconAnchor: [32, 32], iconSize: [64, 64], icon: '/TowingContent/img/icon_船拖物品.png',
            title: getYearDateStr(r.Data.UpdateTime),
            rotationAngle: r.Data.LatestDirectionAngle,
            updateTime: r.Data.UpdateTime
        });
        _map.addLayer(shipInfo.point);
        mapHelper.setCenter(shipInfo.point.getLatLng());

        timer.setShipInfo();
    });
}
var mru = {
    list: [],
    lastTime: ''
}
var chartDangXDom;// = document.getElementById('main');
var chartDangYDom;
var chartDangZDom;
var chartYaoXDom;
var chartYaoYDom;
var chartYaoZDom;
var chartDangX; // echarts.init(chartDom);
var chartDangY;
var chartDangZ;
var chartYaoX;
var chartYaoY;
var chartYaoZ;
//let yAxisData = ['08-03', '08-04', '08-05', '08-06', '08-07', '08-08', '08-09', '08-10', '08-11', '08-12', '08-13', '08-14']
//let data = [100, 90, 200, 250, 240, 500, 120, 390, 480, 270, 290, 200]
//let lineColor = "rgba(204, 231, 255, 1)"
var optBase = function () {
    return {
        backgroundColor: '',
        lineColor: '',
        legend: [],
        legendData: [],
        xsize: 1, // 每隔多少数据才显示legend
        grid: {
            top: '20%',
            bottom: '10%',
            left: '15%',
            right: '5%',
        },
        itemStyle: {
            color: 'rgba(0, 243, 255, 0.6)'
        },
        areaLowColor: 'rgba(0, 243, 255, 0.6)',
        areaHightColor: 'rgba(0, 243, 255, 0.1)',
        data: []
    }
}
function getOption(option) {
    return {
        backgroundColor: option.backgroundColor || 'rgba(200, 200, 200, 0)',
        legend: {
            show: false,
            icon: 'rect',
            itemWidth: 10,
            itemHeight: 10,
            x: '46%',
            top: '5%',
            textStyle: {
                color: option.lineColor,
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
                    color: option.lineColor || "rgba(204, 231, 255, 1)", //轴线和单位颜色
                }
            },

            data: option.legendData || [] // yAxisData
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
                    color: option.lineColor || "rgba(204, 231, 255, 1)", //轴线和单位颜色
                }
            },
            splitLine: {
                show: true,
                lineStyle: {
                    color: option.lineColor || "rgba(204, 231, 255, 1)", //轴线和单位颜色,
                    type: [2, 3],
                    dashOffset: 2
                }
            }
        },
        series: [{
            name: option.legend && option.legend[0] ? option.legend[0] : 'legend',
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
                color: 'rgba(0, 243, 255, 0.6)',
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
            data: (option.data || []).map(function (item) {
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
        }]
    };
}
function drawChart(chart, opt, id) {
    if (!(chart)) {
        var chartdom = document.getElementById(id);
        chart = echarts.init(chartdom);
    }
    chart.setOption(opt);
    return chart;
}
function loadMRU() {
    $.post('/Towing/Chart/GetMRUEChartDataByNum?size=', function (r) {
        r = JSON.parse(r);
        r = r.Data;
        var opt, idx, legend;
        legend = '横荡';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}m';
        opt = getOption(opt);
        //console.log('chart opt:', opt);
        opt.xsize = 3;
        chartDangX = drawChart(chartDangX, opt, 'dangXChart');

        legend = '纵荡';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}m';
        opt = getOption(opt);
        //console.log('chart opt:', opt);
        opt.xsize = 3;
        chartDangY = drawChart(chartDangY, opt, 'dangYChart');

        legend = '垂荡';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}m';
        opt = getOption(opt);
        //console.log('chart opt:', opt);
        opt.xsize = 3;
        chartDangZ = drawChart(chartDangZ, opt, 'dangZChart');

        legend = '横摇';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}°';
        opt = getOption(opt);

        //console.log('chart opt:', opt);
        opt.xsize = 3;
        chartYaoX = drawChart(chartYaoX, opt, 'yaoXChart');

        legend = '纵摇';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}°';
        opt = getOption(opt);
        //console.log('chart opt:', opt);
        opt.xsize = 3;
        chartYaoY = drawChart(chartYaoY, opt, 'yaoYChart');

        legend = '艏摇';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}°';
        opt = getOption(opt);
        //console.log('chart opt:', opt);
        // opt.xsize = 3;
        chartYaoZ = drawChart(chartYaoZ, opt, 'yaoZChart');


    });
}

var egyro = {
    chartData: null,
    list: [],
    lastTime: ''
}

var chartEGYRODom;
var chartEGYRO;

var directionMap = {};
// prettier-ignore
var directionList = ['W', 'WSW', 'SW', 'SSW',
    'S', 'SSE', 'SE', 'ESE',
    'E', 'ENE', 'NE', 'NNE',
    'N', 'NNW', 'NW', 'WNW'];
directionList = ["N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"];
directionList.forEach(function (name, index) {
    directionMap[name] = Math.PI / 8 * index;
});
var dims = {
    time: 0,
    degree: 1,
    R: 2,
    waveHeight: 3,
    weatherIcon: 2,
    minTemp: 3,
    maxTemp: 4
};
var arrowSize = 18;

var renderArrow = function (param, api) {
    var point = api.coord([
        api.value(dims.time),
        api.value(dims.degree)
    ]);
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
        rotation: directionMap[api.value(dims.R)],
        position: point,
        style: api.style({
            fill: '#00ff99',
            stroke: '#555',
            lineWidth: 1
        })
    };
};

var egyroOption = function (data) {
    return {
        title: null,
        tooltip: {
            trigger: 'axis',
            formatter: function (params) {
                return [
                    '时间:' + params[0].value[dims.time],
                    '方向(°)：' + params[0].value[dims.degree],
                    '朝向：' + params[0].value[dims.R]
                ].join('<br>');
            }
        },
        grid: {
            top: '10%',
            bottom: '15%',
            left: '60'
        },
        xAxis: {
            type: 'category',
            maxInterval: 3600 * 1000 * 24,
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
function loadEGYRO() {
    $.post('/Towing/Chart/GetEGYROEChartDataByNumV2?size=', function (r) {
        r = JSON.parse(r);
        r = r.List;
        egyro.chartData = r;

        var opt, idx, legend;

        var data = [];
        r.forEach(function (itm, idx) {
            data.push([itm.UpdateTimeStr, itm.AzAngle, itm.AzAngleDirectionStr]);
        });

        // console.log('egyro data:', data);

        opt = egyroOption(data);
        chartEGYRO = drawChart(chartEGYRO, opt, 'egyroChart');

    });
}


var windInfo = {
}
var chartWindInfo;
function loadWindInfo() {
    $.post('/Towing/Chart//GetWindEChartDataByNum?size=300', function (r) {
        r = JSON.parse(r);
        r = r.Data;
        var legend, opt, idx;
        legend = '风速';
        opt = optBase();
        opt.legend.push(legend);
        idx = r.Legend.indexOf(legend);
        opt.legendData = r.LegendDataSet[idx];
        opt.data = r.DataSet[idx];
        opt.yFormatter = '{value}(m/s)';
        opt = getOption(opt);
        console.log('chart opt:', opt);
        opt.xsize = 3;
        chartWindInfo = drawChart(chartWindInfo, opt, 'windInfoChart');

    });
}