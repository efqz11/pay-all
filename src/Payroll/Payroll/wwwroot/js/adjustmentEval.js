
function GetFormatValue(fieldValue) {
    var evalMethod = $(fieldValue).data('evalmethod');
    var update = $(fieldValue).data('updateinputclass');
    var idf = $(fieldValue).data('identifier');
    console.log('data-evalmethod: ' + evalMethod);
    console.log('data-updateinputclass: ' + update);
    console.log('data-identifier: ' + idf);



    var formula = evalMethod;
    var prefix = "";
    var formatValue = "";

    do {
        console.log('inside loop -> formula: -> ' + formula);
        if (!formula.startsWith("{")) {
            if (formula.indexOf("{") >= 0)
                prefix = formula.substring(0, formula.indexOf("{"))
            else
                prefix = formula;
            formatValue += prefix;
        }

        if (formula.indexOf("{") < 0)
            formula = formula.replace(prefix, "");
        else {
            var cutoff = (formula.indexOf("{") > 0) ? formula.indexOf("{") - 1 : 0;
            var firstInsider = formula
                .substr(formula.indexOf("{"), formula.indexOf("}") - cutoff).replace("}", "").replace("{", "");
            var idfNow = firstInsider.replace("field.", "");

            if (idfNow === idf)
                formatValue += $(fieldValue).val();
            else {
                formatValue += $(fieldValue).parent().parent().find('input[data-identifier="' + idfNow + '"]').val() || 0;

                // if (formatValue === undefined)

            }

            if (formula.indexOf(prefix + "{" + firstInsider + "}") < 0)
                throw new Error("Formula evaluation has failed. please regenerate table or change config");
            formula = formula.replace(prefix + "{" + firstInsider + "}", "");
        }
        prefix = "";
    }
    while (formula !== "");


    console.log('final - eval: ' + formatValue);
    console.log('final - evaluated: ' + eval(formatValue));
    try {
        eval(formatValue);
    } catch (e) {
        formatValue = 0;
    }
    return formatValue;
}

$(document).on('change', '.calculated', function (e) {
    $(this).parents('tr').addClass('dirty');
    var formatValue = GetFormatValue($(this));
    var update = $(this).data('updateinputclass');

    console.log($(this).parents('tr'));
    $(this).parent().parent().find('input[data-identifier="' + update + '"]').val(eval(formatValue));
});


$(document).on('change keyup', '.manual-entry', function (e) {
    $(this).parents('tr').addClass('dirty');

    var formatValue = GetFormatValue($(this));
    var update = $(this).data('updateinputclass');

    try {
        console.log($(this).parents('tr'));
        var target = $(this).parent().parent().find('input[data-identifier="' + update + '"]');
        if (target.length > 0) {
            target.val(eval(formatValue));

            console.log('target found ::');
            console.log('has class .calculated = ' + $(target).hasClass("calculated"));
            if ($(target).hasClass("calculated"))
                $(target).trigger('change');
        }
    } catch (e) {
        console.log('Unable to parse eval, failed with errors');
        console.log(e);
    }
});



