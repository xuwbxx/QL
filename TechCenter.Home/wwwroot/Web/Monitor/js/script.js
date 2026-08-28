// JavaScript Document
$(document).ready(function(){
	
    //点开工程 弹出所属工程详情
	$('#thgc').click(function() {
		$('#marquee1').hide();
		$('#thgc_son').show();
		
    });
	$('#exit').click(function() {
		$('#thgc_son').hide();
		$('#marquee1').show();
		
    });
	
    // 图片弹出
    const $overlay = $('<div class="overlay"></div>').appendTo('body');

    // 为所有可放大的图片绑定点击事件
    $('.zoomable-img').click(function (e) {
        e.stopPropagation(); // 阻止事件冒泡

        // 如果已经放大，则关闭
        if ($('.enlarged-img').length) {
            $overlay.fadeOut();
            $('.enlarged-img').remove();
            return;
        }

        // 创建放大后的图片
        const $enlarged = $(this).clone()
            .removeClass('zoomable-img')
            .addClass('enlarged-img');

        // 显示遮罩层和放大图片
        $overlay.fadeIn().append($enlarged);
    });

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

});


