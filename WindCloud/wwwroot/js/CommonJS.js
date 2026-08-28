var GetFullDateTime = function () {
    var time = new Date();//获取系统当前时间
    var year = time.getFullYear();
    var month = time.getMonth() + 1;
    var date = time.getDate();//系统时间月份中的日
    var hour = time.getHours();
    var minutes = time.getMinutes();
    var seconds = time.getSeconds();
    if (month < 10) {
        month = "0" + month;
    }
    if (date < 10) {
        date = "0" + date;
    }
    if (hour < 10) {
        hour = "0" + hour;
    }
    if (minutes < 10) {
        minutes = "0" + minutes;
    }
    if (seconds < 10) {
        seconds = "0" + seconds;
    }
    return year + "/" + month + "/" + date + " " + hour + ":" + minutes;// + ":" + seconds;
}


var GetFullDateTimeStr = function (str) {
    var time = new Date(str)
    var year = time.getFullYear();
    var month = time.getMonth() + 1;
    var date = time.getDate();//系统时间月份中的日
    var hour = time.getHours();
    var minutes = time.getMinutes();
    var seconds = time.getSeconds();
    if (month < 10) {
        month = "0" + month;
    }
    if (date < 10) {
        date = "0" + date;
    }
    if (hour < 10) {
        hour = "0" + hour;
    }
    if (minutes < 10) {
        minutes = "0" + minutes;
    }
    if (seconds < 10) {
        seconds = "0" + seconds;
    }
    return year + "/" + month + "/" + date + " " + hour + ":" + minutes + ":" + seconds;
}

var GetFullDateTimeString = function (time) {

    var year = time.getFullYear();
    var month = time.getMonth() + 1;
    var day = time.getDate();//系统时间月份中的日
    var hour = time.getHours();
    var minutes = time.getMinutes();
    var seconds = time.getSeconds();
    if (month < 10) {
        month = "0" + month;
    }
    if (day < 10) {
        day = "0" + day;
    }
    if (hour < 10) {
        hour = "0" + hour;
    }
    if (minutes < 10) {
        minutes = "0" + minutes;
    }
    if (seconds < 10) {
        seconds = "0" + seconds;
    }
    return year.toString() + month.toString() + day.toString() + hour.toString() + minutes.toString() + seconds.toString();
}

var CreateTimeToken = function () {

    var time = new Date();
    var year = time.getFullYear();
    var month = time.getMonth() + 1;
    var date = time.getDate();//系统时间月份中的日
    var hour = time.getHours();
    var minutes = time.getMinutes();
    var seconds = time.getSeconds();
    if (month < 10) {
        month = "0" + month;
    }
    if (date < 10) {
        date = "0" + date;
    }
    if (hour < 10) {
        hour = "0" + hour;
    }
    if (minutes < 10) {
        minutes = "0" + minutes;
    }
    if (seconds < 10) {
        seconds = "0" + seconds;
    }
    return btoa(year + month + date + hour + minutes + seconds + time.getTime());

}

//日期格式化
var formatDateTime = function (date) {

    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    m = m < 10 ? '0' + m : m;
    var d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    var h = date.getHours();
    h = h < 10 ? ('0' + h) : h;
    return y + '-' + m + '-' + d + ' ' + h + ":00:00";
}

var formatDateTimeByMinute = function (date) {

    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    m = m < 10 ? '0' + m : m;
    var d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    var h = date.getHours();
    h = h < 10 ? ('0' + h) : h;
    var mm = date.getMinutes();
    mm = mm < 10 ? ('0' + mm) : mm;
    return y + '-' + m + '-' + d + ' ' + h + ":" + mm;
}

var formatDateTimeByDay = function (date) {

    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    m = m < 10 ? '0' + m : m;
    var d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    return y + '-' + m + '-' + d;
}

function getDaysAgo(day) {
    const today = new Date();
    today.setDate(today.getDate() + day);
    return today;
}

function getMinutesAgo(minute) {
    const today = new Date();
    today.setMinutes(today.getMinutes() + minute);
    return today;
    
}

var formatFullDateTime = function (date) {

    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    m = m < 10 ? '0' + m : m;
    var d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    var h = date.getHours();
    h = h < 10 ? ('0' + h) : h;
    var mm = date.getMinutes();
    mm = mm < 10 ? ('0' + mm) : mm;
    var ss = date.getSeconds();
    ss = ss < 10 ? ('0' + ss) : ss;
    return y + '-' + m + '-' + d + ' ' + h + ":" + mm + ":" + ss;
}


//写cookie
var setCookie = function (name, value) {
    var Days = 30;
    var exp = new Date();
    exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
    document.cookie = name + "=" + escape(value) + "; path=/;expires=" + exp.toGMTString();
}

//拿cookie
var getCookie = function (name) {
    var arr, reg = new RegExp("(^| )" + name + "=([^;]*)(;|$)");

    if (arr = document.cookie.match(reg))

        return unescape(arr[2]);
    else
        return null;
}

var OpenMaskLayout = function () {
    $("div.mask").css({ "display": "block", "height": window.outerHeight, "top": $(document).scrollTop() - 75 });
    $(document).bind('mousewheel', function (event, delta) { return false; });
}

var CloseMaskLayout = function () {

    $("div.mask").css({ "display": "none", "height": window.outerHeight });
    $(document).unbind('mousewheel');
}

var OpenMaskPopUp = function () {
    $("div.mask").css({ "display": "block", "height": window.outerHeight });
}

var CloseMaskPopUp = function () {
    $("div.mask").css({ "display": "none", "height": window.outerHeight });
}

var StringEnterFormat = function (str) {
    if (str == null || str == '' || str.length == 0)
        return '';
    var n = parseInt(str.length / 2);
    return str.substring(0, n) + "</br>" + str.substring(n);
}

var popupOpen = function () {
    $('.popup-max').css({ 'height': window.innerHeight - 70 + 'px', 'top': $(document).scrollTop() + 'px' });
    $(document).bind('mousewheel', function (event, delta) { return false; });
}

var VideoStyleSetting = function () {
    $("#preview_1").css({ 'height': '400px' });
}
//时间戳
function timestampToTime(timestamp) {
    var date = new Date(timestamp * 1000);//时间戳为10位需*1000，时间戳为13位的话不需乘1000



function formatDateTime(timeStamp) {
    var date = new Date();
    date.setTime(timeStamp * 1000);
    var y = date.getFullYear();
    var m = date.getMonth() + 1;
    m = m < 10 ? ('0' + m) : m;
    var d = date.getDate();
    d = d < 10 ? ('0' + d) : d;
    var h = date.getHours();
    h = h < 10 ? ('0' + h) : h;
    var minute = date.getMinutes();
    var second = date.getSeconds();
    minute = minute < 10 ? ('0' + minute) : minute;
    second = second < 10 ? ('0' + second) : second;
    return y + '-' + m + '-' + d + ' ' + h + ':' + minute + ':' + second;

}}


//随机数打开弹框
var GetRandomNum = function () {

    var Num = Math.random() * 100;
    if (Num < 30) {
        return 1;
    }
    else if (Num > 70) {
        return 2;
    }
    else {
        return 3;
    }


}


var GetJSTimeString = function (timeStr) {
    var re = /-?\d+/;
    var m = re.exec(timeStr);
    var date = new Date(Number(m[0])).toLocaleString();

    return date;
}


function fnW(str) {
    var num;
    str >= 10 ? num = str : num = "0" + str;
    return num;
}
//获取当前时间
//var timer = setInterval(function () {
//    var date = new Date();
//    var year = date.getFullYear(); //当前年份
//    var month = date.getMonth(); //当前月份
//    var data = date.getDate(); //天
//    var hours = date.getHours(); //小时
//    var minute = date.getMinutes(); //分
//    var second = date.getSeconds(); //秒
//    var day = date.getDay(); //获取当前星期几 
//    var ampm = hours < 12 ? '上午' : '下午';
//    $('#time').html(fnW(hours) + ":" + fnW(minute) + ":" + fnW(second));
//    $('#date').html('<span>' + year + '/' + (month + 1) + '/' + data + '</span><span>' + ampm + '</span><span>周' + day + '</span>')

//}, 1000)

//时间戳
var formatDateStamp = function (valueStr, spe = '-') {

    var value = 0;
    if (valueStr.length == 10) {
        value = Number(valueStr) * 1000 //10位数时间戳要乘1000 13位不用
    }
    else {
        value = Number(valueStr);
    }

    let data = new Date(value)
    let year = data.getFullYear()
    let month = data.getMonth() + 1
    let day = data.getDate()
    let h = data.getHours()
    let mm = data.getMinutes()
    let s = data.getSeconds()
    month = month > 9 ? month : '0' + month
    day = day > 9 ? day : '0' + day
    h = h > 9 ? h : '0' + h
    mm = mm > 9 ? mm : '0' + mm
    s = s > 9 ? s : '0' + s
    return `${year}${spe}${month}${spe}${day} ${h}:${mm}:${s}`
}

var ConvertValue = function (Value) {

    if (Value == null || Value == '')
        return "-"

    return Value;

}


//模态框
var ModalPopup = function (type, text) {

    if (type == 'information') {
        $('#ts_tk_Information_Content').html(text);
        $('#ts_tk_Information').modal('show');
    }
    else if (type == 'error') {
        $('#ts_tk_Error_Content').html(text);
        $('#ts_tk_Error').modal('show');
    }
    else if (type == 'success') {
        $('#ts_tk_Success_Content').html(text);
        $('#ts_tk_Success').modal('show');
    }
}
var ModalClose = function (type) {

    if (type == 'information') {
        $('#ts_tk_Information').modal('hide');
    }
    else if (type == 'error') {
        $('#ts_tk_Error').modal('hide');
    }
    else if (type == 'success') {
        $('#ts_tk_Success').modal('hide');
    }
}

var ModalOpenCBZ = function (text) {
    $('#ts_tk_Error_CBZ_Content').html(text);
    $('#ts_tk_Error_CBZ').modal('show');
}

var ModalCloseCBZ = function (text) {
    $('#ts_tk_Error_CBZ').modal('hide');
}