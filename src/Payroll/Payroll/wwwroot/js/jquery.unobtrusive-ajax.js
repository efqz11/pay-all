/* NUGET: BEGIN LICENSE TEXT
 *
 * Microsoft grants you the right to use these script files for the sole
 * purpose of either: (i) interacting through your browser with the Microsoft
 * website or online service, subject to the applicable licensing or use
 * terms; or (ii) using the files as included with a Microsoft product subject
 * to that product's license terms. Microsoft reserves all other rights to the
 * files not expressly granted by Microsoft, whether by implication, estoppel
 * or otherwise. Insofar as a script file is dual licensed under GPL,
 * Microsoft neither took the code under GPL nor distributes it thereunder but
 * under the terms set out in this paragraph. All notices and licenses
 * below are for informational purposes only.
 *
 * NUGET: END LICENSE TEXT */
/*!
** Unobtrusive Ajax support library for jQuery
** Copyright (C) Microsoft Corporation. All rights reserved.
*/

/*jslint white: true, browser: true, onevar: true, undef: true, nomen: true, eqeqeq: true, plusplus: true, bitwise: true, regexp: true, newcap: true, immed: true, strict: false */
/*global window: false, jQuery: false */

(function ($) {
    var data_click = "unobtrusiveAjaxClick",
        data_target = "unobtrusiveAjaxClickTarget",
        data_validation = "unobtrusiveValidation";

    function getFunction(code, argNames) {
        var fn = window, parts = (code || "").split(".");
        while (fn && parts.length) {
            fn = fn[parts.shift()];
        }
        if (typeof (fn) === "function") {
            return fn;
        }
        argNames.push(code);
        return Function.constructor.apply(null, argNames);
    }

    function isMethodProxySafe(method) {
        return method === "GET" || method === "POST" || method === "DELETE";
    }

    function asyncOnBeforeSend(xhr, method) {
        if (!isMethodProxySafe(method)) {
            xhr.setRequestHeader("X-HTTP-Method-Override", method);
        }
    }

    function asyncOnSuccess(element, data, contentType) {
        var mode;
        var selctor;

        if (contentType.indexOf("application/x-javascript") !== -1) {  // jQuery already executes JavaScript for us
            return;
        }

        mode = (element.getAttribute("data-ajax-mode") || "").toUpperCase();
        selctor = element.getAttribute("data-ajax-update") || "";
        if (selctor === '.modal__container') selctor += ":first";

        console.log('(selctor):', selctor);
        $(selctor).each(function (i, update) {
            var top;
            console.log('(update): ', update);

            //update = update + ":first";
            switch (mode) {
            case "BEFORE":
                top = update.firstChild;
                $("<div />").html(data).contents().each(function () {
                    update.insertBefore(this, top);
                });
                break;
            case "AFTER":
                $("<div />").html(data).contents().each(function () {
                    update.appendChild(this);
                });
                break;
            case "REPLACE-WITH":
                $(update).replaceWith(data);
                break;
                default:
                    //console.log('defult. $(update): ', $(update));
                $(update).html(data);
                break;
            }
        });
    }

    function getLoadingElement() {
        return '<div class="loader loader-white loader-sm btn-loading" data-page="2"><div class="ball-beat"><div></div><div></div><div></div></div></div>';
    }

    function asyncRequest(element, options) {
        var confirm, loading, method, duration, buttonText, isUrlRewrite;
        //console.log('inside asyncRequest()');
        //console.log(options);
        //console.log(options.source);
        buttonText = $(options.source).html();
        _changeSrc = $(options.source).hasClass('btn') || $(options.source).hasClass('modal__btn');

        confirm = element.getAttribute("data-ajax-confirm");
        //console.log('printing asyncRequest(element, options)');
        //console.log(element);
        //console.log();
        isUrlRewrite = element.hasAttribute('data-url-rewrite')
        if (confirm && !window.confirm(confirm)) {
            return;
        }

        loading = $(element.getAttribute("data-ajax-loading"));
        duration = parseInt(element.getAttribute("data-ajax-loading-duration"), 10) || 0;

        $.extend(options, {
            type: element.getAttribute("data-ajax-method") || undefined,
            url: element.getAttribute("data-ajax-url") || undefined,
            cache: !!element.getAttribute("data-ajax-cache"),
            beforeSend: function (xhr) {
                if (_changeSrc) $(options.source).html(getLoadingElement());
                $(options.source).attr('disabled', 'disabled');
                var result;
                asyncOnBeforeSend(xhr, method);
                result = getFunction(element.getAttribute("data-ajax-begin"), ["xhr"]).apply(element, arguments);
                if (result !== false) {
                    loading.show(duration);
                }
                return result;
            },
            complete: function () {
                loading.hide(duration);
                if (_changeSrc) $(options.source).html(buttonText);
                $(options.source).removeAttr('disabled');
                getFunction(element.getAttribute("data-ajax-complete"), ["xhr", "status"]).apply(element, arguments);

                // remove prevent submission issue
                $(element).data('submitted', false);
            },
            success: function (data, status, xhr) {
                asyncOnSuccess(element, data, xhr.getResponseHeader("Content-Type") || "text/html");
                getFunction(element.getAttribute("data-ajax-success"), ["data", "status", "xhr"]).apply(element, arguments);

                if (isUrlRewrite) {
                    var requestedUrl = this.url.replace(/[&?]X-Requested-With=XMLHttpRequest/i, "");

                    // if the url is the same, replace the state
                    if (window.location.href == requestedUrl) {
                        history.replaceState({ html: data }, document.title, requestedUrl);
                    }
                    else {
                        history.pushState({ html: data }, document.title, requestedUrl);
                    }
                }
            },
            error: function () {
                if (element.getAttribute("data-ajax-failure") === null)
                    getFunction("handleModalPostFailure", ["xhr", "status", "error"]).apply(element, arguments);
                else
                    getFunction(element.getAttribute("data-ajax-failure"), ["xhr", "status", "error"]).apply(element, arguments);

                // check if modal-1 is open and loading is shown
                // then replace with error and hide model
                if ($('#modal-1.is-open .loader:visible').length)
                    ehideModal('Unexpected error has occured');
            }
        });

        options.data.push({ name: "X-Requested-With", value: "XMLHttpRequest" });

        method = options.type.toUpperCase();
        if (!isMethodProxySafe(method)) {
            options.type = "POST";
            options.data.push({ name: "X-HTTP-Method-Override", value: method });
        }
        //console.log('optios', options);
        $.ajax(options);
    }

    function validate(form) {
        var validationInfo = $(form).data(data_validation);
        return !validationInfo || !validationInfo.validate || validationInfo.validate();
    }

    $(document).on("click", "a[data-ajax=true]", function (evt) {
        evt.preventDefault();
        //console.log(this);
        asyncRequest(this, {
            url: this.href,
            type: "GET",
            data: [],
            source: this
        });
    });

    $(document).on("click", "form[data-ajax=true] input[type=image]", function (evt) {
        var name = evt.target.name,
            target = $(evt.target),
            form = $(target.parents("form")[0]),
            offset = target.offset();

        form.data(data_click, [
            { name: name + ".x", value: Math.round(evt.pageX - offset.left) },
            { name: name + ".y", value: Math.round(evt.pageY - offset.top) }
        ]);

        setTimeout(function () {
            form.removeData(data_click);
        }, 0);
    });

    $(document).on("click", "form[data-ajax=true] :submit", function (evt) {
        var name = evt.currentTarget.name,
            target = $(evt.target),
            form = $(target.parents("form")[0]);

        form.data(data_click, name ? [{ name: name, value: evt.currentTarget.value }] : []);
        form.data(data_target, target);

        setTimeout(function () {
            form.removeData(data_click);
            form.removeData(data_target);
        }, 0);
    });

    $(document).on("submit", "form[data-ajax=true]", function (evt) {
        var clickInfo = $(this).data(data_click) || [],
            clickTarget = $(this).data(data_target),
            isCancel = clickTarget && clickTarget.hasClass("cancel");
        evt.preventDefault();
        if (!isCancel && !validate(this)) {
            return;
        }

        //console.log(clickInfo);
        //console.log(clickTarget);
        //console.log(evt);

        asyncRequest(this, {
            url: this.action,
            type: this.method || "GET",
            data: clickInfo.concat($(this).serializeArray()),
            source: clickTarget
        });
    });
}(jQuery
));