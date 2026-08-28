$(document).ready(function(){
   	    //人员详情	
	   	$('.login_ryxx').click( function(){
		if( $('.ryxx_box').css("display") == 'block'){
		    $('.ryxx_box').stop(true,true).slideUp();
			
		}else{
		    $('.ryxx_box').stop(true,true).slideDown();
			
		}
	  });
		//关闭申请软件资料填写窗口
	//$('.zltx_tk').click(function(){
	//	$('.tk_sqzl_tx').stop(true,true).fadeIn();
	//})
	
	$('.tk_sqzl_tx_close').click(function(){
		$('.tk_sqzl_tx').stop(true,true).fadeOut();
	})
	  	
	//$('.zltx_tk02').click(function () {
	//	$('.tk_sqzl_tx02').stop(true, true).fadeIn();
	//})

	//$('.tk_sqzl_tx02_close').click(function () {
	//	$('.tk_sqzl_tx02').stop(true, true).fadeOut();
	//})

	//$('.tk_sqzl_tx06_close').click(function () {
	//	$('.tk_sqzl_tx06').stop(true, true).fadeOut();
	//})

	//$('.login_help').click(function () {
	//	if ($('.help_box').css("display") == 'block') {
	//		$('.help_box').stop(true, true).slideUp();

	//	} else {
	//		$('.help_box').stop(true, true).slideDown();

	//	}
	//});
	
});

//项目基本信息展开收起
//$('.xmjbxx_boxmore_btn').click(function () {
//	if ($('.xmjbxx_boxmore').css("display") == 'block') {
//		$('.xmjbxx_boxmore').stop(true, true).slideUp();
//		$('.xmjbxx_boxmore_btn').css("background", "url('img/icon-fc-bottom.png') no-repeat center center");
//	} else {
//		$('.xmjbxx_boxmore').stop(true, true).slideDown();
//		$('.xmjbxx_boxmore_btn').css("background", "url('img/icon-fc-top.png') no-repeat center center");
//	}
//});



