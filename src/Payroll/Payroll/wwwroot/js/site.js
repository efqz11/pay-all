
//// Using JavaScript & Local Storage
//// FOR DarkMode
//// Select the button
//const btn = document.querySelector(".btn-toggle");
//// Check for dark mode preference at the OS level
//const prefersDarkScheme = window.matchMedia("(prefers-color-scheme: dark)");


//// Get the user's theme preference from local storage, if it's available
//const currentTheme = localStorage.getItem("theme");
//// If the user's preference in localStorage is dark...
//if (currentTheme == "dark") {
//    // ...let's toggle the .dark-theme class on the body
//    document.body.classList.toggle("dark-mode");
//    // Otherwise, if the user's preference in localStorage is light...
//} else if (currentTheme == "light") {
//    // ...let's toggle the .light-theme class on the body
//    document.body.classList.toggle("light-mode");
//}


//// Listen for a click on the button 
//btn.addEventListener("click", function () {
//    // If the user's OS setting is dark and matches our .dark-mode class...
//    if (prefersDarkScheme.matches) {
//        // ...then toggle the light mode class
//        document.body.classList.toggle("light-mode");
//        // ...but use .dark-mode if the .light-mode class is already on the body,
//        var theme = document.body.classList.contains("light-mode") ? "light" : "dark";
//    } else {
//        // Otherwise, let's do the same thing, but for .dark-mode
//        document.body.classList.toggle("dark-mode");
//        var theme = document.body.classList.contains("dark-mode") ? "dark" : "light";
//    }
//    // Finally, let's save the current preference to localStorage to keep using it
//    localStorage.setItem("theme", theme);
//});



//// END Dark Mode


// https://stackoverflow.com/questions/2830542/prevent-double-submission-of-forms-in-jquery
// jQuery plugin to prevent double submission of forms
jQuery.fn.preventDoubleSubmission = function () {
    $(this).on('submit', function (e) {
        var $form = $(this);

        if ($form.data('submitted') === true) {
            // Previously submitted - don't submit again
            alert('Form already submitted. Please wait.');
            e.preventDefault();
            e.stopImmediatePropagation();
        } else {
            // Mark it so that the next submit can be ignored
            // ADDED requirement that form be valid
            if ($form.valid()) {
                $form.data('submitted', true);
            }
        }
    });

    // Keep chainability
    return this;
};


$('form:not(.js-allow-double-submission)').preventDoubleSubmission();

//const connection = new signalR.HubConnectionBuilder()
//    .withUrl("/SignalServer")
//    .configureLogging(signalR.LogLevel.Information)
//    .build();

$(function () {

    $(document).on('mouseover', '.list-group-item', function (e) {
        $(this).find('.list-group-toggler').show()
    });
    $(document).on('mouseleave', '.list-group-item', function (e) {
        $(this).find('.list-group-toggler').hide()
    });

    $(document).on('mouseover', '.table tr', function (e) {
        //$(this).find('.table-tr-toggler').show()
        $(this).find('.table-tr-toggler').css('opacity', 1);
    });
    $(document).on('mouseleave', '.table tr', function (e) {
        //$(this).find('.table-tr-toggler').hide()
        $(this).find('.table-tr-toggler').css('opacity', 0);
    });

    InitializePopover();
});


// reset form submitting on modal close button click
$(document).on('click', 'button[data-micromodal-close]', function (e) { e.preventDefault(); });

//connection.start().catch(err => console.error(err.toString())).then(function () {
//    connection.invoke('getConnectionId')
//        .then(function (connectionId) {

//            sessionStorage.setItem('conectionId', connectionId);
//                    // Send the connectionId to controller


//            console.log("connected");
//            console.log("connectionID: " + connectionId);


//            //connection.invoke("Send", connectionId);
//            //$("#signalRconnectionId").attr("value", connectionId);
//        });
//});


//function displayNotification() {
//    console.log('notification called from SERVER...');
//    sendNotification('info', 'You\'ve just got a notification');
//}


//connection.on("ReceiveMessage", (user, message) => {
//    console.log('notification called from SERVER...');

//    const encodedMsg = user + " says " + message;
//    sendNotification('info', encodedMsg);
//    const li = document.createElement("li");
//    li.textContent = encodedMsg;
//    document.getElementById("messagesList").appendChild(li);
//});


//let connection = new SignalR.HubConnection("SignalServer");
//getNotification();
//connection.on('displayNotification', () => {
//    getNotification();
//});

//connection.start();

//function getNotification() {
//    console.log('update notification here...');
//    $('#noti_cnt').text();
//}


// hide amcharts logo
function hidelogos() {
    console.log('logos hidden');
    $('g title:contains(amCharts library)').parent().hide();
}



function loadMultipleEmpddSearch(element) {
    $(element).select2({
        width: '200px',
        placeholder: "Search for employees",
        minimumInputLength: 2,
        allowClear: true
    });
}


function formatState(state) {
    console.log(state);
    if (!state.id) {
        return state.text;
    }
    var baseUrl = "admin/images/flags";
    var $state = $(
        '<span><img src="' + state.avatar + '" height="25" class="img-flag" /> ' + state.text + '</span>'
    );
    return $state;
};

function loadEmpddSearch(element) {

    var url = GetAppRootPath() + '/Search/Employees';
    $(element).select2({
        width: $(element).data('width') || '200px',
        placeholder: "Search for employees",
        minimumInputLength: 2,
        allowClear: true,
        ajax: {
            url: url,
            dataType: 'json',
            type: "GET",
            delay: 250,

            data: function (term) {
                return {
                    'term': term.term //search term
                };
            },
            error: function (data) {
                console.log("ERROR");
            },

            processResults: function (data) {
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.name,
                            id: item.id,
                            avatar: item.avatar
                        }
                    })
                };
            },
            cache: true

            //processResults: function (data) {
            //    return {
            //        results: data
            //    };
            //}
        },
        templateResult: formatState,

        escapeMarkup: function (markup) { return markup; },
        //templateResult: function (item) {
        //    return item.name;
        //    //'<div class="d-flex"><div class="image-container"><img src="' + item.avatar + '" height="35" class="mt-1 mr-2 rounded-circle" /></div>' + 
        //    //    '<div class="name-display pt-1"><span class="">' + item.name + '</span><br><small class="text-muted" style="position: relative;top: -4px;">' +
        //    //    item.summary + '</small></div></div>';

        //    //return '<div class="facility">' + item.text + '</div><span class="city">' + item.city + '</span></div>';
        //},

        //escapeMarkup: function (m) {
        //    return m;
        //},
        // passed data-initvalue='[{"id":"US","value":"United States of America"}]'
        initSelection: function (element, callback) {
            //    //console.log('element', element);
            //    //console.log('callback', callback);
            //    //console.log('$(element).val()', $(element).val());
            //    //if ($(element).val() !== null) return true;
            

            try {
                var elementText = $(element).attr('data-initvalue');
                console.log('data-initvalue', elementText);
                if (elementText === undefined)
                    elementText = '{ "id": 0, "text": "Please choose employee" }';
                else if (elementText === null || elementText === undefined || elementText === "")
                    if ($(element).attr('data-initvalue') && $(element).attr('data-initvalue').length <= 0 && $(element).val() === null) {
                        elementText = '{ "id": 0, "text": "Please choose employee" }';
                        console.log('placeholder replaced/');
                    }

                    //elementText = '{id=' + $(element).val() + ', text=' + $(element).find(':selected').text() + '}';
                console.log('elementText', elementText);
                callback(JSON.parse(elementText));
            } catch (e) {
                console.log('error initiazling emp dd');
                console.log(e);
                return true;
            }
        }
    });
}


function loadCountryddSearch(element) {

    var url = 'http://18.139.135.243/secure/api/country/';
    $(element).select2({
        width: $(element).data('width') || '200px',
        placeholder: "Search for country",
        minimumInputLength: 2,
        allowClear: true,
        ajax: {
            url: url,
            dataType: 'json',
            type: "GET",
            delay: 250,

            data: function (term) {
                return {
                    'q': term.term //search term
                };
            },
            error: function (data) {
                console.log("ERROR");
            },

            processResults: function (data) {
                return {
                    results: $.map(data, function (item) {
                        return {
                            text: item.Name,
                            id: item.ID,
                        }
                    })
                };
            },
            cache: true

            //processResults: function (data) {
            //    return {
            //        results: data
            //    };
            //}
        },
        templateResult:
            function(state) {
                console.log(state, 'ddd');
                if (!state.id) {
                    return state.text;
                }
                var $state = $(
                    '<span>' + state.text + '</span>'
                );
                return $state;
            },

        escapeMarkup: function (markup) { return markup; },
    });
}


let modalConfig = {
    awaitCloseAnimation: true, // set to false, to remove close animation
    onShow: function (modal) {
        console.log("micromodal open");
        //addModalContentHeight("short");
        /**************************
          For full screen scrolling modal, 
          uncomment line below & comment line above
         **************************/
        //addModalContentHeight('tall');
    },
    onClose: function (modal) {
        console.log("micromodal close");
    }
};

MicroModal.init(modalConfig);

//tippy('a[data-tippy-content], button[data-tippy-content], i[data-tippy-content]');

function convertToLoadingTable(table) {
    console.log('before callled ()');
    $(table).addClass('box-placeholder');
    $(table).find('thead tr th').html('<span class="category text link"></span>');
    $(table).find('tbody tr td, tfoot tr td').html('<span class="text line"></span>');
}

function ssNoti(msg) { sendNotification('success', msg); }
function rrNoti(msg) { sendNotification('error', msg); }
function sendNotification(type, msg) {
    if (type === 'info') {
        iziToast.info({
            //title: 'Hello',
            message: msg,
        });
    }
    if (type === 'success') {
        iziToast.success({
            //title: 'Hello',
            message: msg,
        });
    }
    if (type === 'warning') {
        iziToast.warning({
            //title: 'Hello',
            message: msg,
        });
    }
    if (type === 'error') {
        iziToast.error({
            //title: 'Hello',
            message: msg,
        });
    }
    //try {
    //    notify({
    //        type: type, //alert | success | error | warning | info
    //        title: type === "success" ? type : "error",
    //        position: {
    //            x: "right", //right | left | center
    //            y: "top" //top | bottom | center
    //        },
    //        // icon: '<img src="images/paper_plane.png" />',
    //        message: msg,
    //        autoHide: false,
    //        delay: 2500
    //    });
    //} catch (e) {
    //}
}

function handleModalPostFailure(jqXHR, textStatus, errorThrown) {
    console.log('handleModalPostFailure()', jqXHR);
    try {
        if (jqXHR.responseJSON) {
            sendNotification('error', jqXHR.responseJSON.message);
        } else {
            console.log(jqXHR);
            sendNotification('error', "There was an error while processing your request");
        }
    } catch (e) {
        sendNotification('error', jqXHR.responseText);
    }
    
    //$('#modal-1 .modal__container .modal__content').prepend("<div class='alert alert-danger'>Oh Snap! That was an error.</div>");
    //console.log(jqXHR);
    //console.log(errorThrown);
    //sendNotification('error', jqXHR.responseText);
}


function table_search(search, tr, indexSearch = '0') {
    //check if element don't exist in dom
    var regEx = /^[0-9]*$/;
    if (tr.length === 0 || !regEx.test(indexSearch)) {
        return;
    }
    /*hide tr don't contain search in input*/
    for (var i = 0; i < tr.length; i++) {
        var resule = 'false';
        for (var j = 0; j < indexSearch.length; j++) {
            if (tr.eq(i).children().length > indexSearch[j]) {
                if (tr.eq(i).children().eq(indexSearch[j]).text().toLowerCase().indexOf(search.toLowerCase()) !== -1) {
                    resule = 'success';
                    break;
                }
            }
        }
        if (resule === 'success') {
            tr.eq(i).show();
        } else {
            tr.eq(i).hide();
        }
    }
}

function listgroup_search(search, listgrop) {
    //check if element don't exist in dom
    var regEx = /^[0-9]*$/;
    if (listgrop.length === 0 || $(listgrop).find("> .list-group-item").length <= 0) {
        return;
    }

    var items = $(listgrop).find("> .list-group-item");

    for (var j = 0; j < items.length; j++) {
        var resule = 'false';
        if (items.eq(j).children().text().toLowerCase().indexOf(search.toLowerCase()) !== -1) {
            resule = 'success';
        }


        if (resule === 'success' && (listgrop).find("> .list-group-item :visible").length <= 5) {
            items.eq(j).show();
        } else {
            items.eq(j).hide();
        }
    }


    ///*hide tr don't contain search in input*/
    //for (var i = 0; i < items.length; i++) {
    //    var resule = 'false';
    //    for (var j = 0; j < indexSearch.length; j++) {
    //        if (tr.eq(i).children().length > indexSearch[j]) {
    //            if (tr.eq(i).children().eq(indexSearch[j]).text().toLowerCase().indexOf(search.toLowerCase()) !== -1) {
    //                resule = 'success';
    //                break;
    //            }
    //        }
    //    }
    //}
}

//http://davidwalsh.name/javascript-debounce-function
function debounce(func, wait, immediate) {
    var timeout;
    return function () {
        var context = this, args = arguments;
        var later = function () {
            timeout = null;
            if (!immediate) func.apply(context, args);
        };
        var callNow = immediate && !timeout;
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
        if (callNow) func.apply(context, args);
    };
};

function initializeDatePicker() {
    $(".has-timepicker").flatpickr({
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
    });

    $(".has-datepicker").flatpickr({
        dateFormat: "j-M-Y"
    });

    $(".has-daterangetpicker").flatpickr({
        dateFormat: "j-M-Y",
        mode: "range"
    });
}

//$('body').tooltip({
//    selector: '[rel=tooltip]'
//});


/// Temp fix to routing issue
function GetAppRootPath() {
    var url = window.location.href;
    if (url.indexOf('hr') !== -1)
        return '/hr';
    else return '';
}

function getLoaderHtml() {
    return '<div class="loader loader-blue btn-loading" data-page="2" style="line-height: 1px;display:block">' +
        '<div class="ball-beat"><div></div><div></div><div></div></div>' +
        '</div>';
}
function getLoaderHtmlWithLineHeight(lh) {
    return '<div class="loader loader-blue btn-loading" data-page="2" style="line-height:1px;display:block">' +
        '<div class="ball-beat" style="line-height:' + lh + '"><div></div><div></div><div></div></div>' +
        '</div>';
}
function getLoaderSmWhiteHtml() {
    return '<div class="loader loader-white loader-sm btn-loading" data-page="2"><div class="ball-beat"><div></div><div></div><div></div></div></div>';
}
function getLoaderWhiteHtml() {
    return '<div class="loader loader-white btn-loading" data-page="2" style="line-height: 1px;display:block">' +
        '<div class="ball-beat"><div></div><div></div><div></div></div>' +
        '</div>';
}


//$('.edit_0').click();
function showModal(modal, reset) {
    var loader = getLoaderHtml();
    reset = reset || false;
    if (reset === false)
        $('#modal-1 .modal__container .modal__header').empty();
    $('#modal-1 .modal__container .modal__footer').empty();
    $('#modal-1 .modal__container .modal__content').html(loader);
    MicroModal.show(modal || 'modal-1', modalConfig);
}

function initModal(modal) {
    modal = '#' + (modal || 'modal-1');
    var form = $(modal).find('.modal__container form');
    $.validator.unobtrusive.parse(form);
}

// hides opened modal and closes after 1.5 sec.
// used when creating from modal (already open)
function shideModal(message) {
    _hideModal(message, "s");
}
function ehideModal(message) {
    _hideModal(message, "e");
}
function _hideModal(message, type) {
    var isErr = type === "e";
    console.log(message);

    setTimeout(function () {
        MicroModal.close('modal-1');
    }, 1500);

    $('#modal-1 .modal__container .modal__footer').empty();
    $('#modal-1 .modal__container .modal__content').fadeOut(150);

    if (isErr)
        $('#modal-1 .modal__container .modal__content').html('<p class="text-center text-danger"><i class="fa fa-exclamation-triangle text-danger"></i> ' + message + '</p>').fadeIn(200);
    else
        $('#modal-1 .modal__container .modal__content').html('<p class="text-center">' + message + '</p>').fadeIn(200);
}

// hides opened modal and closes after 1.5 sec.
// used when creating from modal (already open)
function openLoadingModal(title, message) {
    $('#modal-1 .modal__container .modal__footer').empty();
    $('#modal-1 .modal__container .modal__content').fadeOut(150);
    $('#modal-1 .modal__container .modal__header h2.modal__title').html(title).fadeIn(200);

    $('#modal-1 .modal__container .modal__header').css("display", "block");
    $('#modal-1 .modal__container .modal__header .modal__close').hide();
    $('#modal-1 .modal__container .modal__title, #modal-1 .modal__container .modal__footer').addClass('text-center');


    var _msg = message || 'Please wait until action is completed. This may take some time';
    var mainDiv = '<div class="text-center d1">' +
        '<div class="loader loader-blue btn-loading mb-4" data-page="2" style="line-height: 1px;display:block">' +
        '<div class="ball-beat"><div></div><div></div><div></div></div>' +
        '</div>' +
        '<span class="d1-span">' + _msg+'</span>' +
        '</div>';
    $('#modal-1 .modal__container .modal__content').html(mainDiv).fadeIn(200);
    $('#modal-1 .modal__container .modal__footer').html('<small>This window will close automatically</small>').fadeIn(200);


    MicroModal.show('modal-1');
}


function hideFailedLoadingModal(jqXHR, textStatus, errorThrown) {
    var mainDiv = '<div class="text-center d1">' +
        '<div class="loader loader-blue btn-loading mb-4" data-page="2" style="line-height: 1px;display:none">' +
        '<div class="ball-beat"><div></div><div></div><div></div></div>' +
        '</div>' +
        '<h5 class="d1-span"><i class="fa fa-exclamation-triangle text-danger"></i> ' + jqXHR.responseJSON.message +' </span>' +
        '</div>';
    $('#modal-1 .modal__container .modal__content').html(mainDiv).fadeOut().fadeIn(200);
    $('#modal-1 .modal__container .modal__footer').html('<small>This window will close automatically</small>').fadeIn(200);

    setTimeout(function () {
        MicroModal.close('modal-1');
    }, 1500);

}

function hideFailedLoadingModalCustom(errorMessage) {
    var mainDiv = '<div class="text-center d1">' +
        '<div class="loader loader-blue btn-loading mb-4" data-page="2" style="line-height: 1px;display:none">' +
        '<div class="ball-beat"><div></div><div></div><div></div></div>' +
        '</div>' +
        '<h5 class="d1-span"><i class="fa fa-exclamation-triangle text-danger"></i> ' + errorMessage + ' </span>' +
        '</div>';
    $('#modal-1 .modal__container .modal__content').html(mainDiv).fadeOut().fadeIn(200);
    $('#modal-1 .modal__container .modal__footer').html('<small>This window will close automatically</small>').fadeIn(200);

    setTimeout(function () {
        MicroModal.close('modal-1');
    }, 1500);
}


function hideSuccessLoadingModal(jqXHR, textStatus, errorThrown) {
    var mainDiv = '<div class="text-center d1">' +
        '<div class="loader loader-blue btn-loading mb-4" data-page="2" style="line-height: 1px;display:none">' +
        '<div class="ball-beat"><div></div><div></div><div></div></div>' +
        '</div>' +
        '<h5 class="d1-span"><i class="fa fa-check-circle text-success"></i> Completed</span>' +
        '</div>';
    $('#modal-1 .modal__container .modal__content').html(mainDiv).fadeOut().fadeIn(200);
    $('#modal-1 .modal__container .modal__footer').html('<small>This window will close automatically</small>').fadeIn(200);

    setTimeout(function () {
        MicroModal.close('modal-1');
    }, 1500);
}


function initDatePicker() {

    $(".has-datepicker").flatpickr({
        dateFormat: "j-M-Y"
    });
}

function initTimePicker() {
    $(".has-timepicker").flatpickr({
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
        time_24hr: true
    });
}


function hideModal(modal) {
    MicroModal.close(modal || 'modal-1');
}

function logout(event) {
    event.preventDefault();
    $.post(GetAppRootPath() + "/account/logout", function (e) {

    }).done(function (e) { location.reload(); });
}

function InitializeTabs() {
    $('.tab-link').click(function () {

        var tabID = $(this).attr('data-tab');

        $(this).addClass('active').siblings().removeClass('active');

        $('#tab-' + tabID).addClass('active').siblings().removeClass('active');
    });
}

function InitializePopover() {
    try {

        $('[data-toggle="popover"]').popover({
            html: true,
            trigger: 'manual',
            animation: true,
            template: '<div class="popover"><div class="popover-content"><div class="popover-content-wrapper"><div class="popover-body"></div></div></div></div>',
            // container: $(this).attr('id'),
            // placement: 'top',
            //content: function () {
            //  $return = '<div class="hover-hovercard"></div>';
            // }
        }).on("mouseenter", function () {
            var _this = this;
            $(this).popover("show");
            $(".popover").on("mouseleave", function () {
                $(_this).popover('hide');
            });
        }).on("mouseleave", function () {
            var _this = this;
            setTimeout(function () {
                if (!$(".popover:hover").length) {
                    $(_this).popover("hide");
                }
            }, 300);
        });
    } catch (e) {
        console.log('popover loading failed!');
    }
}

function onCheckChange(el) {
    var checked = $(el).is(":checked");
    if (checked)
        $('#RepeatEndDate').val("");
    $(el).val(checked);
    //$(el).next().text(!checked ? "Repeat until" : "Never");
    //$('.d-IsRepeatEndDateNever').toggle();

    //$(el).prop("checked", checked);

    console.log($(el).next());
}
function onChange_SetParentCheck(el) {
    $(el).toggleClass("active");
    var checked = $(el).hasClass("active");
    //console.log('checked__ ' + checked);

    if (checked)
        $(el).prev().val('False');
    else
        $(el).prev().val('True');

    console.log($(el).prev());

}

function getDuration(start, end, countDays) {
    if (end.diff(start, 'days') <= 0)
        return start.format('ddd, MMM DD, YYYY');


    var str = start.format("MMM DD");
    if (start.year() !== end.year())
        str += start.format(', YYYY');

    str += " — ";
    if (start.month() === end.month() && start.year() === end.year())
        str += end.format('DD, YYYY');
    else
        str += end.format('MMM DD, YYYY');

    if (countDays)
        str += " (" + end.diff(start, 'days') + ")";

    return str;
}



/// Load more button (updates more data in to same update selector on click of the button)
// ---------------------------------------------------------------------------------------
//<form asp-action="GetCompanyNotifications" asp-controller="Notification" data-ajax="true" data-ajax-method="POST" data-ajax-update="#listing" data-ajax-success="" id="form-filter" data-ajax-failure="handleModalPostFailure">
//    <input type="hidden" name="page" id="page" value="1" />
//    <input type="hidden" name="id" value="@Model.Id" />
//</form>

//    <div id="listing">
//    </div>

//    <div class="text-center">
//        <button type="button" data-form="#form-filter" class="btn btn-outline-info btn-load-more" id="btn-load-more" data-page="1" style="display:block">Load More</button>
//    </div>
//    <div class="hidden-container hide"></div>

var canFetch = true;
$('#btn-load-more').click(function () {
    var btnLoad = $(this);
    var form = $($(this).data('form'));
    var url = $(form).attr('action');
    var update = $(btnLoad).data('update') || $(form).data('ajax-update');
    console.log(url, form, update);

    $('.hidden-container').empty();
    if ($(update).find('.list-group-noti').length <= 0) {
        $(update).html(getLoaderHtml());
    }


    console.log('scrolled to bottom of page, canFetch = ' + canFetch);
    var fd = $(form).serialize();

    $.post(url, fd, function (data) {
    }).done(function (data) {
        $('.hidden-container').html(data);
        var rows = $('.hidden-container').find('.list-group-noti .list-group-item');

        console.log('data (clean rows): ', rows);

        if ($.isEmptyObject(rows) || rows === undefined || rows === "" || $(rows).length <= 0) {
            canFetch = false;
            $(btnLoad).hide();
            console.log('EMPTY DATA (fetch aborted)');
        }
        else if ($(update).find('list-group-noti').length <= 0) {
            $(update).html(data);
        }
        else {
            $(update).find('.list-group-noti .list-group-item').append(rows);
            console.log('data updated');
        }

        if (canFetch === true && $(rows).length < 10) {
            console.log('received less than 10 records, hence we have reached the end');
            canFetch = false;
            $(btnLoad).hide();
        }

    }).always(function (data) {

        if ($.isEmptyObject(data) || data === undefined || data === "") {
            canFetch = false;
            $(btnLoad).hide();
        } else {
            var newPage = (parseInt(page) + 1);
            console.log('newpage: ' + newPage);
            $(form).find('#page').val(newPage);
        }
        $(btnLoad).hide();
        $('.hidden-container').empty();
    }).fail(function (e) {
        canFetch = false;
        $(btnLoad).hide();
    });
});



/// load more from table view
$('#btn-load-more-table').click(function () {
    var btnLoad = $(this);
    var form = $($(this).data('form'));
    var url = $(form).attr('action');
    var update = $(btnLoad).data('update') || $(form).data('ajax-update');
    console.log(url, form, update);

    $('.hidden-container').empty();
    if ($(update).find('table').length <= 0) {
        $(update).html(getLoaderHtml());
    }


    console.log('scrolled to bottom of page, canFetch = ' + canFetch);
    var fd = $(form).serialize();

    $.get(url, fd, function (data) {
    }).done(function (data) {
        $('.hidden-container').html(data);
        var rows = $('.hidden-container').find('table tbody tr');

        console.log('data (clean rows): ', rows);

        if ($.isEmptyObject(rows) || rows === undefined || rows === "" || $(rows).length <= 0) {
            canFetch = false;
            $(btnLoad).hide();
            console.log('EMPTY DATA (fetch aborted)');
        }
        else if ($(update).find('table').length <= 0) {
            $(update).html(data);
        }
        else {
            $(update).find('table tbody').append(rows);
            console.log('data updated');
        }

        if (canFetch === true && $(rows).length < 10) {
            console.log('received less than 10 records, hence we have reached the end');
            canFetch = false;
            $(btnLoad).hide();
        }

    }).always(function (data) {

        if ($.isEmptyObject(data) || data === undefined || data === "") {
            canFetch = false;
            $(btnLoad).hide();
        } else {
            var newPage = (parseInt(page) + 1);
            console.log('newpage: ' + newPage);
            $(form).find('#page').val(newPage);
        }
        $(btnLoad).hide();
        $('.hidden-container').empty();
    }).fail(function (e) {
        canFetch = false;
        $(btnLoad).hide();
    });
});



// draw chart from dictionary
function drawActionPie(data, chartDiv) {
    am4core.ready(function () {

        // Themes begin
        am4core.useTheme(am4themes_animated);
        // Themes end

        // Create chart instance
        var chart = am4core.create(chartDiv, am4charts.PieChart);

        // Add data
        chart.data = data;

        // Set inner radius
        chart.innerRadius = am4core.percent(50);

        // Add and configure Series
        var pieSeries = chart.series.push(new am4charts.PieSeries());
        pieSeries.dataFields.value = "value";
        pieSeries.dataFields.category = "key";
        pieSeries.slices.template.stroke = am4core.color("#fff");
        pieSeries.slices.template.strokeWidth = 2;
        pieSeries.slices.template.strokeOpacity = 1;

        // This creates initial animation
        pieSeries.hiddenState.properties.opacity = 1;
        pieSeries.hiddenState.properties.endAngle = -90;
        pieSeries.hiddenState.properties.startAngle = -90;

        hidelogos();
    }); // end am4core.ready()
}
