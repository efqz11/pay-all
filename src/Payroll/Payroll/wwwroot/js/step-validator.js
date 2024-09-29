
function validate(o) {
    if ($(o).is(':hidden'))
        return true;

    console.log('validating item ', o, $(o).val());
    if ($(o).val() != "" && $(o).val().length > 0) {
        try {
            return $(o).valid();
        } catch (e) {

        }
    }

    if ($(o).is('input'))
        return $(o).val() != "";
    if ($(o).is('select'))
        return $(o).val() != ""; //  && $(o).val() != "0";;

    return false;
}

function validateStepAndShowHideButtons() {
    var _val = false;
    var currStep = getCurrentStep(false);
    $('div[data-step="' + currStep + '"]').find('select:visible,input:visible').each(function (i, e) {
        _val = validate(e);
        if (!_val) {
            $('.btn-next').prop('disabled', true);
            return;
        }
    });

    if (_val)
        $('.btn-next').prop('disabled', false); // enables button
    else
        $('.btn-next').prop('disabled', true);   // disables button
}

// on pop, update last html
$(window).bind("popstate", function (e) {
    var state = e.originalEvent.state;
    $("#main-content").html(state.html);
});

function triggerFormValidate() {
    $('#form input:visible, #form select:visible').change();
    $('#form input:visible, #form select:visible').blue();
}

$(document).on('blur change', '#form input:visible, #form select:visible', function () { // fires on every keyup & blur
    validateStepAndShowHideButtons();
    //if (validate(this)) { // $('div[data-step] #form').validate().checkForm()) {                   // checks form for validity
    //    $('.btn-next').prop('disabled', false); // enables button
    //} else {
    //    $('.btn-next').prop('disabled', true);   // disables button
    //}
});

function getCurrentStep(hideCurrntStepDiv) {
    var currStep = 0;
    if ($('.progress-bar-tabs li.active').length > 0)
        currStep = parseInt($('.progress-bar-tabs li.active').last().data('step'));

    if (hideCurrntStepDiv) {
        $('.step[data-step="' + currStep + '"]').hide();
        // $('.progress-bar-tabs li[data-step="' + currStep + '"]').toggleClass('active incomplete'); *@
        }

    $('.btn-back').css('display', currStep <= 1 ? 'none' : 'initial');
    return currStep;
}

$(document).on('click', '.btn-back', function () {
    var currStep = getCurrentStep(true); // $('.progress-bar-tabs li.active').last().data('step');
    // $('.progress-bar-tabs li[data-step="' + currStep + '"]').toggleClass('active incomplete');
        // $('.step[data-step="' + currStep + '"]').hide();
        var prevStep = parseInt(currStep) - 1;
    if (prevStep > 0) {
        $('.progress-bar-tabs li[data-step="' + currStep + '"]').toggleClass('active incomplete');

        $('.progress-bar-tabs li[data-step="' + prevStep + '"]').attr('class', 'progress-bar-item active');
        $('.step[data-step="' + prevStep + '"]').show();
    }
    else
        $('.btn-back').css('display', 'none');
});

$(document).on('click', '.btn-next', function () {
    var currStep = getCurrentStep(false); // $('.progress-bar-tabs li.active').last().data('step');
    //$('.progress-bar-tabs li[data-step="' + currStep + '"]').toggleClass('active incomplete');
    // $('.step[data-step="' + currStep + '"]').hide();
    $(this).prop('disabled', true);
    var loadFn = $(this).attr('data-loadfunc');
    var nextStep = parseInt(currStep) + 1;
    if ($('.progress-bar-tabs li[data-step="' + nextStep + '"]').length > 0) {
        getCurrentStep(true); // hide current div
        $('.progress-bar-tabs li[data-step="' + nextStep + '"]').toggleClass('active incomplete');
        $('.step[data-step="' + nextStep + '"]').show();
    } else {
        // this is last step
        console.log('this is last step...');
        try {
            if (loadFn != null)
                eval(loadFn);
        } catch (e) {

        }
        $("#form").submit();
    }
});

$(document).on('click', '.btn-step-completed', function (e) {
    e.preventDefault();
    e.stopImmediatePropagation();
    $('.btn-continue-to-step').click();
});

function startStep() {
    $('.btn-next').click();
}

function skipStep(e) {
    e.preventDefault();
    e.stopImmediatePropagation();
    $('.btn-next').prop('disabled', false);
    $('.btn-next').click();
}

function gotoNextStep(step) {
    $('.btn-continue-to-step').prop('disabled', false);
    var s = parseInt(step) || 0;
    if (s > 0) {
        $('.nav-steps:eq(' + (s - 1) + ')').find('a').click();
        $('.btn-continue-to-step').prop('disabled', true);
    }
}

function toggleContinueBtn(step) {
    var currStep = $('.btn-continue-to-step').data('step');
    if (currStep == step)
        $('.btn-continue-to-step').prop('disabled', true);
    else
        $('.btn-continue-to-step').prop('disabled', false);

    $('.nav-steps.font-weight-bold').removeClass('font-weight-bold');
    $('.nav-steps:eq(' + (step - 1) + ')').addClass('font-weight-bold');
}

$('.btn-continue-to-step').click(function () {
    var s = parseInt($(this).attr('data-step') || 0);
    console.log('going to step ' + s);
    $('.nav-steps:eq(' + (s - 1) + ')').find('a').click();
});