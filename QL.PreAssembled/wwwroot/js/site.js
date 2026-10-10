// // Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// // for details on configuring this project to bundle and minify static web assets.

// // Write your JavaScript code.
const site = (function () {
    const customIcons = {
        CustomBridge: {
            template: `<svg t="1789348290853" class="icon" viewBox="0 0 1024 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="5654">
            <path d="M768 288a32 32 0 0 1 32 32v96h96a32 32 0 0 1 32 32v384a32 32 0 0 1-32 32h-96a288 288 0 0 0-576 0H128a32 32 0 0 1-32-32V448a32 32 0 0 1 32-32h96v-96a32 32 0 1 1 64 0v96h192v-96a32 32 0 0 1 64 0v96h192v-96a32 32 0 0 1 32-32z m128-128a32 32 0 0 1 32 32v32a32 32 0 0 1-32 32H128a32 32 0 0 1-32-32V192a32 32 0 0 1 32-32h768z" fill="currentColor" p-id="5655"></path></svg>`
        }, 
        CustomProject: {
            template: `<svg version="1.1" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200" style="enable-background:new 0 0 200 200;" xml:space="preserve">
<path class="icon" d="M179.7,22.4h-51.5c-2.2,0-4.3-0.8-5.9-2.2L102.5,2.6c-1.6-1.4-3.7-2.2-5.9-2.2H20.3c-4.9,0-8.9,4-8.9,8.9v181.6
	c0,4.9,4,8.9,8.9,8.9h159.5c4.9,0,8.9-4,8.9-8.9V31.3C188.6,26.4,184.6,22.4,179.7,22.4L179.7,22.4z M46.6,28.1
	c0-3.1,2.5-5.5,5.5-5.5H87c3.1,0,5.5,2.5,5.5,5.5c0,3.1-2.5,5.5-5.5,5.5H52.1C49,33.6,46.6,31.2,46.6,28.1z M29.1,33.6
	c-3.1,0-5.5-2.5-5.5-5.5c0-3.1,2.5-5.5,5.5-5.5h4.7c3.1,0,5.3,2.5,5.3,5.5c0,3.1-2.3,5.5-5.3,5.5H29.1z M121.9,160.9
	c0,3.1-2.5,5.5-5.5,5.5H38.9c-3.1,0-5.5-2.5-5.5-5.5c0-3.1,2.5-5.5,5.5-5.5h77.5C119.5,155.3,121.9,157.8,121.9,160.9L121.9,160.9z
	 M121.9,127.7c0,3.1-2.5,5.5-5.5,5.5H38.9c-3.1,0-5.5-2.5-5.5-5.5c0-3.1,2.5-5.5,5.5-5.5h77.5C119.5,122.1,121.9,124.6,121.9,127.7
	L121.9,127.7z M166.2,88c0,5.7-4.6,10.2-10.2,10.2H43.6c-5.7,0-10.2-4.6-10.2-10.2c0-5.7,4.6-10.2,10.2-10.2H156
	C161.6,77.7,166.2,82.3,166.2,88L166.2,88z"/>
</svg>`
        }
    }
    return {
        Message: top.ElementPlus.ElMessage,
        customIcons: customIcons
    }
})();