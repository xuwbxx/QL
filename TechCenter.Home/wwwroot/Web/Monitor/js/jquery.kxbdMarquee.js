(function ($) {
    $.fn.kxbdMarquee = function (options) {
        var opts = $.extend({}, $.fn.kxbdMarquee.defaults, options);

        return this.each(function () {
            var $marquee = $(this);
            var $element = $marquee.children().first();
            var $kids = $element.children();

            // 方向判断
            var isHorizontal = opts.direction === "left" || opts.direction === "right";
            var scrollProp = isHorizontal ? 'scrollLeft' : 'scrollTop';
            var sizeProp = isHorizontal ? 'outerWidth' : 'outerHeight';
            var cssProp = isHorizontal ? 'width' : 'height';

            var timer = null;
            var totalSize = 0; // 原始内容总长度
            var containerSize = isHorizontal ? $marquee.width() : $marquee.height();

            // 计算内容总宽度/高度
            $kids.each(function () {
                totalSize += $(this)[sizeProp](true);
            });

            // 如果内容比容器小，不滚动
            if (totalSize <= containerSize) {
                return;
            }

            // 克隆一份内容，实现无缝
            $element.append($kids.clone());
            // 设置容器总长度 = 原始长度 × 2
            $element.css(cssProp, totalSize * 2 + 'px');

            // 开始滚动
            function startScroll() {
                if (timer) clearInterval(timer);
                timer = setInterval(function () {
                    var pos = $marquee[0][scrollProp];

                    if (opts.direction === 'left' || opts.direction === 'up') {
                        // 左/上滚动：超过原始长度 → 归零（无缝）
                        if (pos >= totalSize) {
                            $marquee[0][scrollProp] = 0;
                        } else {
                            $marquee[0][scrollProp] += opts.scrollAmount;
                        }
                    } else {
                        // 右/下滚动：小于0 → 跳回原始长度（无缝）
                        if (pos <= 0) {
                            $marquee[0][scrollProp] = totalSize;
                        } else {
                            $marquee[0][scrollProp] -= opts.scrollAmount;
                        }
                    }
                }, opts.scrollDelay);
            }

            function stopScroll() {
                clearInterval(timer);
            }

            startScroll();
            $marquee.hover(stopScroll, startScroll);
        });
    };

    $.fn.kxbdMarquee.defaults = {
        direction: "left",
        scrollAmount: 1,
        scrollDelay: 10
    };
})(jQuery);