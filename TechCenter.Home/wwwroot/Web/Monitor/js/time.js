function fnW(str) {
    var num;
    str >= 10 ? num = str : num = "0" + str;
    return num;
}
//获取当前时间
var timer = setInterval(function () {
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
