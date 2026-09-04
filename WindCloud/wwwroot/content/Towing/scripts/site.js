function getNowTime() {
    return new Date().toLocaleString();
}
function getTimeDiff(time1, time2) {
    // 将时间字符串转换为 Date 对象
    const date1 = new Date(time1);
    const date2 = new Date(time2);

    // 获取时间戳（以毫秒为单位）
    const timestamp1 = date1.getTime();
    const timestamp2 = date2.getTime();

    // 计算时间差（以毫秒为单位），然后转换为秒
    const differenceInMilliseconds = (timestamp1 - timestamp2);
    const differenceInSeconds = differenceInMilliseconds / 1000;

    return differenceInSeconds;
}
function toCamelCaseJson(obj) {
    if (typeof obj !== 'object' || obj === null) {
        return obj;
    }

    if (Array.isArray(obj)) {
        return obj.map(toCamelCaseJson); // 如果是数组，则递归处理每个元素
    }

    return Object.fromEntries(
        Object.entries(obj).map(([key, value]) => [
            key.charAt(0).toLowerCase() + key.slice(1), // 将首字母转为小写
            toCamelCaseJson(value) // 递归处理值
        ])
    );
}
function getYearDateStr(d) {
    return new Date(d).toLocaleDateString().replace('/', '年').replace('/', '月') + '日';
}
function getDateStr(d) {
    d = new Date(d);
    return (d.getMonth() + 1) + '月' + d.getDate() + '日';
}
var mapHelper = {
    setCenter: function (latLon) {
        var center = L.latLng(latLon);
        _map.setView(center);
    },
    // 绘制动态路径
    getDYLines: function (routes, cfg) {
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
        });
        return antPolyline;
    },
    drawDYLine: function (routes, cfg) {
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
    },
    getPoint: function (point) {
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
                iconAnchor: point.iconAnchor || [16, 16],//图标锚点
                iconSize: point.iconSize || [32, 32], //图标大小
            });
        }
        // 添加标记,到地图
        var marker = ShipxyAPI.marker([point.lat, point.lon], {
            icon: marker_icon_1,
            title: point.title,
            rotationAngle: point.rotationAngle ? point.rotationAngle : undefined,
        });
        return marker;
    },
    drawPoint: function (point) {
        var marker = mapHelper.getPoint(point);
        marker.addTo(_map);
        return marker;
    }
}

var mask = {
    // 显示加载遮罩
    showLoading: function () {
        $('#maskMsg').html('请稍后...')
        $('#custom-loading-overlay').modal({ backdrop: 'static', keyboard: false })
    },

    // 隐藏加载遮罩
    hideLoading: function () {
        $('#custom-loading-overlay').modal('hide');
    },
    showMessage: function (msg, time) {
        $('#maskMsg').html(msg);
        $('#custom-loading-overlay').modal({ backdrop: true, keyboard: true, show: true });
        setTimeout(mask.hideLoading, time || 1500);
    }
}

function scrollBottom(ctrl) {
    setTimeout(function () {
        $(ctrl).scrollTop($(ctrl).prop("scrollHeight"));
    }, 500);
}

function enableDragScroll(cfg) {
    if (!cfg || !(cfg.container)) {
        return cfg;
    }
    if (!(cfg.isDragging)) {
        cfg.isDragging = false; // 是否正在拖拽
        cfg.startX = 0; // 初始鼠标位置
        cfg.startY = 0; // 初始鼠标位置
    }

    // 绑定鼠标按下事件
    $(cfg.container).on('click', function (e) {
        if (cfg.isDragging) {
            cfg.isDragging = false;
            $(cfg.container).css('cursor', 'pointer');
        } else {
            cfg.isDragging = true;
            $(cfg.container).css('cursor', 'all-scroll');
            cfg.startX = e.pageX; // 记录初始 X 坐标
            cfg.startY = e.pageY; // 记录初始 Y 坐标
        }
    });

    // 绑定鼠标移动事件
    $(document).on('mousemove', function (e) {
        if (!cfg.isDragging) return;

        const deltaX = e.pageX - cfg.startX; // 水平拖拽距离
        const deltaY = e.pageY - cfg.startY; // 垂直拖拽距离

        // 更新滚动位置
        $(cfg.container).scrollTop($(cfg.container).scrollTop() - deltaY);
        $(cfg.container).scrollLeft($(cfg.container).scrollLeft() - deltaX);

        // 更新初始位置
        cfg.startX = e.pageX;
        cfg.startY = e.pageY;
    });

    //// 绑定鼠标释放事件
    //$(document).on('mouseup', function () {
    //    cfg.isDragging = false;
    //});
}

function rotateImg(img, degree, toFix) {
    // console.log('rotateImg:', 'rotate(' + degree + 'deg)');
    if (degree || degree === 0) {
        if (toFix) degree = Number(degree).toFixed(toFix);
        $(img).css('transform', 'rotate(' + degree + 'deg)')
    }
}

function mergeCells(table, columnIndex) {
    table = $(table);
    var rows = table.find('tbody tr');
    var currentCell = null;
    var rowspan = 1;
    var flag = 0;

    rows.each(function (index) {
        var cell = $(this).find('td').eq(columnIndex);
        if (currentCell === null) {
            currentCell = cell;
        } else if (cell.text() === currentCell.text()) {
            rowspan++;
            cell.hide();
        } else {
            currentCell.attr('rowspan', rowspan);
            if (index > 0) {
                currentCell.parent().prev().addClass('border_bottom');
                flag = 1;
            }
            currentCell = cell;
            rowspan = 1;
        }
    });

    if (currentCell !== null) {
        currentCell.attr('rowspan', rowspan);
        if (flag == 1) {
            currentCell.parent().prev().addClass('border_bottom');
        }
    }
}

function getRes(r, module) {
    if ($.type(r) == "string") r = JSON.parse(r);
    if (r.ErrorMessage) console.log((module || 'load') + ' Error:', r.ErrorMessage);
    return r;
}


var popupImg = function (img) {
    console.log('popupImg:', img);
    // 创建遮罩层
    const $overlay = $('div.overlay');
    if ($overlay.length == 0) $overlay = $('<div class="overlay"></div>').appendTo('body');

    // 创建放大后的图片
    const $enlarged = $(img).clone()
        .removeClass('zoomable-img')
        .addClass('enlarged-img');
    console.log('$overlay', $overlay);
    console.log('$img:', $(img));
    // 显示遮罩层和放大图片
    $overlay.fadeIn().append($enlarged);

    // 点击遮罩层关闭
    $overlay.click(function () {
        $overlay.fadeOut();
        $('.enlarged-img').remove();
    });

    // 按ESC键关闭
    $(document).keyup(function (e) {
        if (e.key === "Escape") {
            $overlay.fadeOut();
            $('.enlarged-img').remove();
        }
    });
}