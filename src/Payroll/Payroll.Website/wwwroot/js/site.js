// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// home page tabs

$(".w-tab-menu > a").click(function(){
    var newTab = $(this).data('w-tab');
    
    // hide old tab
    $(".w-tab-menu a.w--current").removeClass('link w--current');
    $(".w-tab-content .w-tab-pane.w--tab-active").removeClass('w--tab-active');
    // change

    $(this).addClass('w--current');
    $(".w-tab-content > .w-tab-pane[data-w-tab='"+newTab+ "']").addClass('w--tab-active');
});