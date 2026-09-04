$(document).ready(function(){
	//侧边收缩
	    $('.left_main').css('left','34px');

		var expanded = true;

		$('.left_bar').click(function(){
			if(expanded){
				$('.left_main').animate({left:'-22vw'},500);
				$('.left_bar').css('background-position','-36px 0px');
				
			}else{
				$('.left_main').animate({left:'34px'},500);
				$('.left_bar').css('background-position','-0px 0px');
			}
						
			expanded = !expanded;
		});
			
		$('.right_main').css('right','-21vw');
			var expanded = true;
			$('.right_bar').click(function(){
				if(expanded){
					$('.right_main').animate({right:'34px'},500);
					$('.right_bar').css('background-position','-36px 0px');
				}else{

					$('.right_main').animate({right:'-21vw'},500);
					$('.right_bar').css('background-position','-0px 0px');

				}
			
			expanded = !expanded;	
		});
	
	//气象详情
	$('#qxxq_kg').click(function() {
		$('.qixiang_content').animate({bottom:'3vw'},800);
    });
	$('#qxxq_close').click(function() {
		$('.qixiang_content').animate({bottom:'-75vw'},800);
		//$('.qixiang_content').hide();
    });
	//聊天室
	$('#chat_kg').click(function() {
		$('.chatroom_content').animate({bottom:'3vw'},800);
    });
	$('#chatroom_close').click(function() {
		$('.chatroom_content').animate({bottom:'-75vw'},800);
		//$('.chatroom_content').hide();
    });
	
	//	打开历史记录弹框
	$('#open_history').click(function() {
		//$('.chat_history').animate({bottom:'-75vw'},800);
		$('.chat_history').show();
    });

//	关闭历史记录弹框
	$('#chat_history_close').click(function() {
		//$('.chat_history').animate({bottom:'-75vw'},800);
		$('.chat_history').hide();
    });

	
});