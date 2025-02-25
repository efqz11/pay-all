$(function () {
    var activeTab = $('#ActiveTab').val();
    if (activeTab === 'PayPeriod')
        $('#_loadDashboard').click();

});

$('.tab-link').click(function (e) {
    console.log($(this));
    var indx = parseInt($(this).data('tab'));
    
    if (indx === 0)
        $('#_loadDashboard').click();
    else if (indx === 1)
        $('#_loadPayPeriodDays').click();
    else if (indx === 2) 
        $('#_LoadAttendanceTab').click();
    else if (indx === 3) {
        $('#_loadAdjustmentTable').click();
    }
    else if (indx === 4)
        $('#_LoadMasterSheet').click();
    else if (indx === 5)
        $('#_LoadReports').click();
});

var canFetch = true;
$(function () {
    $(window).on("scroll", function () {
        var docHeight = $(document).height();
        var winScrolled = $(window).height() + $(window).scrollTop(); // Sum never quite reaches
        $('.btn-load-more').hide();
        if ((docHeight - winScrolled) < 30) {
            console.log('scrolled to bottom of page');

            if (canFetch === false) return;
            $('.btn-load-more').show();

            // makeAjaxRequest();
        }
    });
});

function makeAjaxRequest_UpdatePayrolPeriod() {
    var btnLoad = $('.btn-load-more');
    var id = $('#PayrolPeriodId').val();
    var page = $(btnLoad).data('page');
    var url = GetAppRootPath() + '/Payroll/GetPayrollPeriod/' + id + '&page=' + page;


    $(btnLoad).html(getLoaderWhiteHtml());
    $.post(url, function (data) {
        //console.log(data);

        var len = $(data).find('.table-schedule tbody tr').length;
        //console.log(len);

        console.log(rows);
        if (len > 0) {
            var rows = $(data).find('.table-schedule tbody tr');
            var hasTdColspan = $(rows).find('td[colspan]').length;
            console.log('hasTdColspan', hasTdColspan);

            if (hasTdColspan > 0) {
                // no schedules
                console.log('no data found :: empty');

                data = null;
                canFetch = false;
                $('.btn-loading').hide();
            }
            else {
                if (page === 1) {
                    var dataRemoveHeader = $(data).filter('#weekly-schedule-table-container');

                    $('#weekly-schedule-table-container').html(dataRemoveHeader.children());
                    console.log('data replaced with <>');
                }
                else {
                    $('.table-schedule tbody').append(rows);
                    console.log('data updated');

                    var newPage = (parseInt(page) + 1);
                    console.log('newpage: ' + newPage);
                    $('.btn-loading').data('page', newPage);
                }
            }
        }
        else {
            console.log('no data found!');
            canFetch = false;
            $('.btn-loading').hide();
        }
    }).done(function (e) {
        //}).always(function (data) {
        //    if ($.isEmptyObject(data) || data == undefined || data == "") {
        //        canFetch = false;
        //        $('.btn-loading').hide();
        //    } else {
        //        var newPage = (parseInt(page) + 1);
        //        console.log('newpage: ' + newPage);
        //        $('.btn-loading').data('page', newPage);
        //    }
        //    $('.btn-loading').hide();
    }).always(function (e) {
        $(btnLoad).html("Load More");
    }).fail(function (e) {
        canFetch = false;
        $('.btn-loading').hide();
    });
}

//function loadPayPeriodDays() {
//    console.log('updating employee id');
//    var empId = $('#empId').val() || 0;
//    var id = $('#PayrolPeriodId').val();

//    var page = $('.btn-loading').data('page');
//    var unscheduled = $('.switch-schedule').prop('checked');
//    var url = GetAppRootPath() + '/Payroll/GetPayrollPeriod/' + id + '?empId=' + empId + '&page=1&showUnScheduled=' + (unscheduled ? "True" : "False");

//    $('#_loadPayPeriodDays').attr('href', url);
//    $('#_loadPayPeriodDays').click();
//};



function makeAjaxRequest_UpdateAdjustmentFieldValues() {
    var btnLoad = $('.btn-load-more-pa');
    var page = $(btnLoad).data('page');
    var id = $('#PayrolPeriodId').val();
    var addId = $('#itemId').val() || 0;

    console.log(url);
    var url = GetAppRootPath() + '/Payroll/GetPayAdjustmentFieldValues/' + id + '?addId=' + addId + '&page=' + page;

    $(btnLoad).html(getLoaderWhiteHtml());

    $.post(url, function (data) {
        var newPage = (parseInt(page) + 1);
        var parsedResponse = $.parseHTML(data);
        console.log('printing  data');
        console.log(data);

        if (page > 1) {
            if (data === null || data === undefined) {
                canFetch = false;
                $('.btn-loading').hide();
                return;
            }
            else {
                $('.table-payAdj tbody').append(data);
                console.log('data updated leng >1');
                console.log('newpage: ' + newPage);
                $('.btn-loading').data('page', newPage);
                return;
            }
        }
        else {
            // replace whole
            $('#tasks-schedule').html(data);
            console.log('data replace');
        }

        //var dss = $(parsedResponse).html();
        //var len = $(dss).find('.table-payAdj tbody tr').length;
        //console.log('len: ' + len);

        //console.log(rows);
        //if (len > 0) {
        //    var rows = $(data).find('.table-payAdj tbody tr');
        //    var hasTdColspan = $(rows).find('td[colspan]').length;
        //    console.log('hasTdColspan', hasTdColspan)

        //    if (hasTdColspan > 0) {
        //        // no schedules
        //        console.log('no data found :: empty');

        //        data = null;
        //        canFetch = false;
        //        $('.btn-loading').hide();
        //    }
        //    else {
        //        if (page === 1) {
        //            $('#tasks-schedule').html(data);
        //            console.log('data replace');
        //        }
        //        else {
        //            $('.table-payAdj tbody').append(rows);
        //            console.log('data updated');

        //            console.log('newpage: ' + newPage);
        //            $('.btn-loading').data('page', newPage);
        //        }
        //    }
        //}
        //else {
        //    console.log('no data found!');
        //    canFetch = false;
        //    $('.btn-loading').hide();
        //}
    }).done(function (e) {
    }).fail(function (e) {
        canFetch = false;
        $('.btn-loading').hide();
    }).always(function (e) {
        $(btnLoad).html("Load More");
    });
}

function setAdjustmentValue() {
    var addId = $('#__AddJustmentID').val() || 0;
    //if (parseInt(addId) !== 0) 
    $('#itemId').val(addId);
}

function pdateWorkingEmployee() {
    var empId = $('#empId').val() || 0;
    var url = GetAppRootPath() + '/Payroll/UpdateWorkingEmployee/' + empId + '?text=' + $('#empId :selected').text();
    $.post(url);
    console.log('employee updated: ' + empId);
}


// <<
function loadEmployeeAttendance() {
    $('div#adjustment-table-fv').html(GetLoader());
    var id = $('#PayrolPeriodId').val();
    var date = $('#SelectedDate').val();
    var unscheduled = $('.switch-schedule').prop('checked');
    var url = GetAppRootPath() + '/Payroll/AttendanceView/' + id + '?date=' + date;

    console.log('url: ' + url);

    $('#_LoadAttendanceTab').attr('href', url);
    $('#_LoadAttendanceTab').click();
}



function loadPayAdjustmentFieldValues() {
    $('div#adjustment-table-fv').html(GetLoader());
    var addId = $('#itemId').val() || 0;

    //canFetch = empId === 0;

    var id = $('#PayrolPeriodId').val();
    var page = $('.btn-loading').data('page');
    var unscheduled = $('.switch-schedule').prop('checked');
    var url = GetAppRootPath() + '/Payroll/GetPayAdjustmentFieldValues/' + id + '?addId=' + addId + '&page=1&showUnScheduled=' + (unscheduled ? "True" : "False");

    console.log('value: ' + addId);
    //if (parseInt(addId) !== 0) {
    $('#__AddJustmentID').val(addId);
    //    console.log('value update ..');
    //}

    $('#tasks-schedule').html(GetLoader());
    $.get(url, function (d) {
        $('#tasks-schedule').html(d);
    }).done(function () {

    });
}


//function loadMasterSheet() {
//    console.log('updating employee id');
//    var empId = $('#empId').val() || 0;
//    var id = $('#PayrolPeriodId').val();

//    var page = $('.btn-loading-ms').data('page');
//    var unscheduled = $('.switch-schedule').prop('checked');
//    var url = GetAppRootPath() + '/Payroll/Finals/' + id + '?empId=' + empId + '&page=1&showUnScheduled=' + (unscheduled ? "True" : "False");

//    $('#btnReloadMasterTable').attr('href', url);
//    $('#btnReloadMasterTable').click();
//}



$(document).on('hover', '.table-master-data td',
    function () {
        var t = parseInt($(this).index()) + 1;
        $('td:nth-child(' + t + ')').addClass('dirty');
    },
    function () {
        var t = parseInt($(this).index()) + 1;
        $('td:nth-child(' + t + ')').removeClass('dirty');
    });

function makeAjaxRequest_UpdateMasterSheet() {
    var btnLoad = $('.btn-load-more-ms');
    var page = $(btnLoad).data('page');
    var id = $('#PayrolPeriodId').val();
    var addId = $('#itemId').val() || 0;

    console.log(url);
    var url = GetAppRootPath() + '/Payroll/Finals/' + id + '?page=' + page;

    $(btnLoad).html(getLoaderWhiteHtml());

    $.post(url, function (data) {
        var newPage = (parseInt(page) + 1);
        var parsedResponse = $.parseHTML(data);
        console.log('printing  data');
        console.log(data);

        var rows = $(data).find('#table-master-data tbody tr');
        //if (empId !== 0) {
        //    $('#table-master-data tbody').empty();
        //}
        $('#table-master-data tbody').append(rows);

        console.log('data updated');
        $('.table-master-data thead th').each(function (i) {
            if (i !== 0)
                calculateColumn(i);
        });
        
    }).done(function (e) {
    }).fail(function (e) {
        canFetch = false;
        $('.btn-loading').hide();
    }).always(function (e) {
        $(btnLoad).html("Load More");
    });
}

let _chartData = null;
function getAttendanceOverviewData() {
    $(function () {

        var id = $('#PayrolPeriodId').val();
        $.get(GetAppRootPath() + '/Payroll/GetPayrollAttendanceOverview/' + id, function (data) {
            console.log(data);
            _chartData = data.chartdata;

            $('#chartdiv').html('<h6 class="text-center"><i class="fa fa-hand-peace"></i> Click on any dimension to show details</h4>');
            //chart.data = d.chartdata;
            //drawScheduleEmployeeChart(data.chartdata);
        });
    });

    $('.attendanceChartType a.list-group-item-action').click(function (e) {
        if (_chartData === null) {
            console.log('chart data was not defined');
            return;
        }

        $(this).addClass('active');
        $('.attendanceChartType a.list-group-item-action').not(this).removeClass('active');

        var type = $(this).data('type');
        console.log('drawing chart .... ' + type);

        $('#chartdiv').html(getLoaderHtml());
        $('#chartdiv').prev().html($(this).html());

        switch (type) {
            case "workedHrs":
                drawWorkedHrsChart();
                break;
            case "late":
                drawLateChart();
                break;
            case "late-emp":
                drawLateEmployeesChart();
                break;
            default:
                break;
        }

        hidelogos();
    });
}

function drawWorkedHrsChart() {
    am4core.ready(function () {

        // Themes begin
        am4core.useTheme(am4themes_animated);
        // Themes end

        // Create chart instance
        var chart = am4core.create("chartdiv", am4charts.XYChart);

        // Add percent sign to all numbers
        chart.numberFormatter.numberFormat = "#.";

        // Add data
        chart.data = _chartData;


        var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        //dateAxis.groupData = true;
        //dateAxis.groupCount = 500;
        //dateAxis.minZoomCount = 5;
        dateAxis.renderer.grid.template.location = 0;
        dateAxis.renderer.axisFills.template.disabled = true;
        dateAxis.renderer.ticks.template.disabled = true;

        var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.title.text = "Hours";
        valueAxis.title.fontWeight = 800;

        // Create series
        var series = chart.series.push(new am4charts.ColumnSeries());
        series.dataFields.valueY = "actualWorkedHours";
        series.dataFields.dateX  = "dateString";
        series.clustered = false;
        series.tooltipText = "Actual Worked Hours\n[bold font-size: 18]{valueY} Hrs[/]";

        var series2 = chart.series.push(new am4charts.ColumnSeries());
        series2.dataFields.valueY = "totalScheduledHours";
        series2.dataFields.dateX  = "dateString";
        series2.clustered = false;
        series2.strokeWidth = 0;
        series2.columns.template.width = am4core.percent(40);
        series2.tooltipText = "Scheduled Hours\n[bold font-size: 18]{valueY} Hrs[/]";

        chart.cursor = new am4charts.XYCursor();
        chart.cursor.lineX.disabled = true;
        chart.cursor.lineY.disabled = true;
    }); 
}

function drawLateChart() {
    am4core.ready(function () {

        // Themes begin
        am4core.useTheme(am4themes_animated);
        // Themes end

        var chart = am4core.create("chartdiv", am4charts.XYChart);
        chart.paddingRight = 20;
        chart.numberFormatter.numberFormat = "#.";
        chart.data = _chartData;

        var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        //dateAxis.groupData = true;
        //dateAxis.groupCount = 500;
        //dateAxis.minZoomCount = 5;
        dateAxis.renderer.grid.template.location = 0;
        dateAxis.renderer.axisFills.template.disabled = true;
        dateAxis.renderer.ticks.template.disabled = true;

        var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.tooltip.disabled = true;
        valueAxis.title.text = "Minutes";
        valueAxis.renderer.minWidth = 35;
        valueAxis.renderer.axisFills.template.disabled = true;
        valueAxis.renderer.ticks.template.disabled = true;

        var series = chart.series.push(new am4charts.LineSeries());
        series.dataFields.dateX = "dateString";
        series.dataFields.valueY = "totalLateMins";
        series.strokeWidth = 2;
        series.stroke = am4core.color("#ff0000"); // red
        series.tooltipText = "Late \n[bold font-size: 20]{valueY} Mins[/]";
        series.fill = am4core.color("#ff0000"); // red

        // set stroke property field
        //series.propertyFields.stroke = "color";

        chart.cursor = new am4charts.XYCursor();
        chart.cursor.lineX.disabled = true;
        chart.cursor.lineY.disabled = true;

        //var scrollbarX = new am4core.Scrollbar();
        //chart.scrollbarX = scrollbarX;

        //dateAxis.start = 0.7;
        //dateAxis.keepSelection = true;


    }); // end am4core.ready()
}


function drawLateEmployeesChart() {
    am4core.ready(function () {

        // Themes begin
        am4core.useTheme(am4themes_animated);
        // Themes end

        var chart = am4core.create("chartdiv", am4charts.XYChart);
        chart.paddingRight = 20;
        chart.numberFormatter.numberFormat = "#.";
        chart.data = _chartData;

        var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        //dateAxis.groupData = true;
        //dateAxis.groupCount = 500;
        //dateAxis.minZoomCount = 5;
        dateAxis.renderer.grid.template.location = 0;
        dateAxis.renderer.axisFills.template.disabled = true;
        dateAxis.renderer.ticks.template.disabled = true;

        var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.tooltip.disabled = true;
        valueAxis.title.text = "No. of Employees";
        valueAxis.renderer.minWidth = 35;
        valueAxis.renderer.axisFills.template.disabled = true;
        valueAxis.renderer.ticks.template.disabled = true;

        var series = chart.series.push(new am4charts.LineSeries());
        series.dataFields.dateX = "dateString";
        series.dataFields.valueY = "lateEmployeeCount";
        series.strokeWidth = 2;
        series.stroke = am4core.color("#ff0000"); // red
        series.tooltipText = "Late \n[bold font-size: 20]{valueY} Employees[/]";
        series.fill = am4core.color("#ff0000"); // red

        var series1 = chart.series.push(new am4charts.LineSeries());
        series1.dataFields.dateX = "dateString";
        series1.dataFields.valueY = "totalAbsentEmployeeCount";
        series1.strokeWidth = 2;
        series1.stroke = am4core.color("#6c757d"); // grey
        series1.tooltipText = "Absent \n[bold font-size: 20]{valueY} Employees[/]";
        series1.fill = am4core.color("#6c757d"); // grey

        // set stroke property field
        //series.propertyFields.stroke = "color";

        chart.cursor = new am4charts.XYCursor();
        chart.cursor.lineX.disabled = true;
        chart.cursor.lineY.disabled = true;

        //var scrollbarX = new am4core.Scrollbar();
        //chart.scrollbarX = scrollbarX;

        //dateAxis.start = 0.7;
        //dateAxis.keepSelection = true;


    }); // end am4core.ready()
}

function drawScheduleEmployeeChart(data) {

    am4core.options.queue = true;
    am4core.options.minPolylineStep = 5;
    am4core.options.onlyShowOnViewport = true;

    // Create chart instance
    var chart = am4core.create("chartdiv", am4charts.XYChart);
    chart.data = data;

    // Create axes
    var dateAxis = chart.xAxes.push(new am4charts.DateAxis());

    // --- 
    dateAxis.groupData = true;
    dateAxis.groupCount = 500;
    dateAxis.minZoomCount = 5;
    //dateAxis.renderer.grid.template.location = 0;
    //dateAxis.renderer.minGridDistance = 30;

    var valueAxis1 = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis1.title.text = "Hours";
    valueAxis1.groupData = true;
    valueAxis1.groupCount = 500;

    var valueAxis2 = chart.yAxes.push(new am4charts.ValueAxis());
    valueAxis2.title.text = "Employees";
    valueAxis2.renderer.opposite = true;
    valueAxis2.renderer.grid.template.disabled = true;

    // Create series
    var series1 = chart.series.push(new am4charts.ColumnSeries());
    series1.dataFields.valueY = "totalScheduledHours";
    series1.dataFields.dateX = "dateString";
    series1.yAxis = valueAxis1;
    series1.name = "Scheduled Hours";
    series1.tooltipText = "{name}\n[font-size: 20]{valueY} hrs[/]";
    series1.fill = chart.colors.getIndex(0);
    series1.strokeWidth = 0;
    series1.clustered = false;
    series1.columns.template.width = am4core.percent(40);

    var series2 = chart.series.push(new am4charts.ColumnSeries());
    series2.dataFields.valueY = "actualWorkedHours";
    series2.dataFields.dateX = "dateString";
    series2.yAxis = valueAxis1;
    series2.name = "Actual Hours";
    series2.tooltipText = "{name}\n[font-size: 20]{valueY} hrs[/]";
    series2.fill = chart.colors.getIndex(0).lighten(0.5);
    series2.strokeWidth = 0;
    series2.clustered = false;
    series2.toBack();

    var series3 = chart.series.push(new am4charts.LineSeries());
    series3.dataFields.valueY = "totalAbsentEmployeeCount";
    series3.dataFields.dateX = "dateString";
    series3.name = "Absent Employees";
    series3.strokeWidth = 2;
    series3.tensionX = 0.7;
    series3.yAxis = valueAxis2;
    series3.tooltipText = "{name}\n[font-size: 20]{valueY} empls[/]";
    series3.tooltip.background.fill = am4core.color("red");

    var bullet3 = series3.bullets.push(new am4charts.CircleBullet());
    bullet3.circle.radius = 3;
    bullet3.circle.strokeWidth = 2;
    bullet3.circle.fill = am4core.color("#fff");

    var series4 = chart.series.push(new am4charts.LineSeries());
    series4.dataFields.valueY = "lateEmployeeCount";
    series4.dataFields.dateX = "dateString";
    series4.name = "Late Employees";
    series4.strokeWidth = 2;
    series4.tensionX = 0.7;
    series4.yAxis = valueAxis2;
    series4.tooltipText = "{name}\n[font-size: 20]{valueY} empls[/]";
    series4.stroke = am4core.color("#bf0808");
    series4.tooltip.getFillFromObject = false;
    series4.tooltip.background.fill = am4core.color("#bf0808");
    series4.strokeDasharray = "3,3";

    var bullet4 = series4.bullets.push(new am4charts.CircleBullet());
    bullet4.circle.radius = 3;
    bullet4.circle.strokeWidth = 2;
    bullet4.circle.fill = am4core.color("#fff");

    //var series5 = chart.series.push(new am4charts.LineSeries());
    //series5.dataFields.valueY = "totalScheduledEmployees";
    //series5.dataFields.dateX = "dateString";
    //series5.name = "Schdeuled Employees";
    //series5.strokeWidth = 2;
    //series5.tensionX = 0.7;
    //series5.yAxis = valueAxis2;
    //series5.tooltipText = "{name}\n[font-size: 20]{valueY} empls[/]";
    //series5.stroke = chart.colors.getIndex(5).lighten(0.5);
    //series5.strokeDasharray = "3,3";

    //var bullet5 = series5.bullets.push(new am4charts.CircleBullet());
    //bullet5.circle.radius = 3;
    //bullet5.circle.strokeWidth = 2;
    //bullet5.circle.fill = am4core.color("#fff");

    // Add cursor
    //chart.cursor = new am4charts.XYCursor();

    // Add legend
    chart.legend = new am4charts.Legend();
    chart.legend.position = "top";

    // Add scrollbar
    chart.scrollbarX = new am4charts.XYChartScrollbar();
    chart.scrollbarX.series.push(series1);
    chart.scrollbarX.series.push(series3);
    chart.scrollbarX.parent = chart.bottomAxesContainer;
    hidelogos();

}

function clearHome(e) {
    // clear all html not in active tab
    //var activeTab = $(e)('.tab-content.active');
    $(e).next().html(GetLoader());
}

function clearAttendanceDayView() {
    $('#attendanceDayView').html(GetLoader());
}

function GetLoader() {
    return '<div class="loader loader-blue btn-loading" id="loader2" data-page="2" style="line-height: 85px;display:block;white-space: pre"><div class="ball-beat"><div></div><div></div><div></div></div></div>';
}





function SaveForm(gotonext) {
    var hasDirtyRows = $('#tblCalculationTable tbody tr.dirty').length > 0;
    if (!hasDirtyRows && gotonext === undefined) return;
    if (!hasDirtyRows && gotonext === true) {
        //$('#ItemId option:selected').next().attr('selected', 'selected').trigger('change');
        sendNotification('success', 'Table was just saved without changes, going to next');
        $('.btn-go-next').click();
        return;
    }

    var btn = $('.btn-save');
    var url = GetAppRootPath() + '/Payroll/SavePayAdjustments';
    var postData = $('#tblCalculationTable tbody tr.dirty').find('input').serializeArray();

    var indx = -1;
    $.each(postData, function (i, e) {
        console.log("(e.name): " + e.name);
        if (e.name === "PayrolPayAdjustments[i].Id") {
            indx++;
        }
        e.name = e.name.replace('[i]', '[' + indx + ']');
    });
    convertToLoadingTable('.data-container-table');
    $.post(url, postData, function (d) {
    }).done(function (data) {
        if (gotonext) {
            $('.btn-go-next').click();
            //$('#payAdjustmentId option:selected').next().attr('selected', 'selected').trigger('change');
            sendNotification('success', 'Table was just saved, going to next');
        }
        else {
            sendNotification('success', 'Values were just saved');
            loadPayAdjustmentFieldValues();
            //$('#payAdjustmentId option:selected').trigger('change');
        }
    }).fail(function () {
        sendNotification('error', 'Error occured while saving, please check pay adjustment configuration');
    });
}

//function updateReCreateTableUrl() {
//    console.log('updateReCreateTableUrl()');
//    var id = $('#PayrolPeriodId').val();
//    var addId = $('#payAdjustmentId').val() || 0;
//    $('.btn-group-afjustment').hide();

//    if (addId !== 0)
//        $('.btn-group-afjustment').show();  
//    console.log(url);
//    var url = GetAppRootPath() + '/Payroll/UpdatePayAdjustmentFields/' + id + '?addId=' + addId;
//    $('.btn-recreate-table').attr('href', url);

//    url = GetAppRootPath() + '/Payroll/RemovePayAdjustmentFieldValues/' + id + '?addId=' + addId;
//    $('.btn-remove-payDss').attr('href', url);    
//}

//function createTableSuccess(payAdj) {
//    $('#modal-recreate-table').find('.d2 .d2-span').html('Table to calculate pay adjustment <b>' + payAdj + '</b> was just re-created');
//    $('#modal-recreate-table').find('.d1').fadeOut(100);
//    $('#modal-recreate-table').find('.d2').fadeIn(800);

//    setTimeout(function () { hideModal('modal-recreate-table'); }, 1500);
//}

//function createTableSuccessReRunPayroll() {
//    $('#modal-regenerate-payroll').find('.d2 .d2-span').html('Payroll run was completed successfully');
//    $('#modal-regenerate-payroll').find('.d1').fadeOut(100);
//    $('#modal-regenerate-payroll').find('.d2').fadeIn(800);

//    setTimeout(function () { hideModal('modal-regenerate-payroll'); }, 1500);
//}






/// FINALES ///
/// expoty
// export to csv
jQuery.fn.tableToCSV = function () { var t = function (t) { return t = t.replace(/"/g, '""'), '"' + t + '"' }; $(this).each(function () { var n = ($(this), $(this).find("caption").text()), e = [], i = []; $(this).find("tr").each(function () { var n = []; $(this).find("th").each(function () { var n = t($(this).text()); e.push(n) }), $(this).find("td").each(function () { var e = t($(this).text()); n.push(e) }), n = n.join(","), i.push(n) }), e = e.join(","), i = i.join("\n"); var o = e + i, a = "data:text/csv;charset=utf-8," + encodeURIComponent(o), c = document.createElement("a"); c.href = a; var h = (new Date).getTime(); "" === n ? c.download = h + ".csv" : c.download = n + "-" + h + ".csv", document.body.appendChild(c), c.click(), document.body.removeChild(c) }) };

$(function () {

    // Add Total row, each column total
    //var rowTotal = $('.table-master-data tbody').find('tr').length;
    //var totals = new Array(rowTotal);
    //var $dataRows = $('.table-master-data tbody').find('td');
    //var newRow = "<tr><td><b>Total</b></td>";
    $('.table-master-data thead th').each(function (i) {
        if (i !== 0)
            calculateColumn(i);
    });
});

function fnExportPrint(tableSelectorId = 'table-master-data') {
    var divToPrint = document.getElementById(tableSelectorId);

    var htmlToPrint = '' +
        '<style type="text/css">' +
        'table th, table td {' +
        'border:1px solid #000;' +
        'padding:0.5em;' +
        '}' +
        '</style>';

    htmlToPrint += divToPrint.outerHTML;
    newWin = window.open("");
    newWin.document.write(htmlToPrint);
    newWin.print();
    newWin.close();
}

function fnExportCsv(name, tableSelector = '#table-master-data') {
    //var html = document.querySelector(tableSelector).outerHTML;
    filename = "master-data-table-" + name + ".csv";
    var csv = [];
    var rows = document.querySelectorAll(tableSelector + " tr");
    //var rows = document.querySelectorAll("#table-master-data tr");

    for (var i = 0; i < rows.length; i++) {
        var row = [],
            cols = rows[i].querySelectorAll("td, th");

        for (var j = 0; j < cols.length; j++) row.push(cols[j].innerText.replace(',', ''));

        csv.push(row.join(","));
    }

    // Download CSV
    download_csv(csv.join("\n"), filename);
}

function download_csv(csv, filename) {

    var csvFile;
    var downloadLink;

    // CSV FILE
    csvFile = new Blob([csv], { type: "text/csv" });

    // Download link
    downloadLink = document.createElement("a");

    // File name
    downloadLink.download = filename;

    // We have to create a link to the file
    downloadLink.href = window.URL.createObjectURL(csvFile);

    // Make sure that the link is not displayed
    downloadLink.style.display = "none";

    // Add the link to your DOM
    document.body.appendChild(downloadLink);

    // Lanzamos
    downloadLink.click();
}



function fnExportExcel(name, tableSelectorId = "table-master-data") {
    let tableData = document.getElementById(tableSelectorId).outerHTML;
    let filename = "master-data-table-" + name + ".xls";
    //tableData = tableData.replace(/<img[^>]*>/gi,""); //enable thsi if u dont want images in your table
    tableData = tableData.replace(/<A[^>]*>|<\/A>/g, ""); //remove if u want links in your table
    tableData = tableData.replace(/<input[^>]*>|<\/input>/gi, ""); //remove input params

    tableData = tableData + '<br /><br />';
    // Code witten By sudhir K gupta.If you found this helpful then please like my FB page - <br />https://facebook.com/comedymood<br />My Blog - https://comedymood.com'

    //click a hidden link to which will prompt for download.
    let a = document.createElement('a')
    let dataType = 'data:application/vnd.ms-excel';
    a.href = `data:application/vnd.ms-excel, ${encodeURIComponent(tableData)}`
    a.download = filename;
    a.click()

    //var tab_text = "<table border='2px'><tr bgcolor='#87AFC6'>";
    //var textRange; var j = 0;
    //tab = document.getElementById('table-master-data'); // id of table

    //for (j = 0; j < tab.rows.length; j++) {
    //    tab_text = tab_text + tab.rows[j].innerHTML + "</tr>";
    //    //tab_text=tab_text+"</tr>";
    //}

    //tab_text = tab_text + "</table>";
    //tab_text = tab_text.replace(/<A[^>]*>|<\/A>/g, "");//remove if u want links in your table
    //tab_text = tab_text.replace(/<img[^>]*>/gi, ""); // remove if u want images in your table
    //tab_text = tab_text.replace(/<input[^>]*>|<\/input>/gi, ""); // reomves input params

    //var ua = window.navigator.userAgent;
    //var msie = ua.indexOf("MSIE ");

    //if (msie > 0 || !!navigator.userAgent.match(/Trident.*rv\:11\./))      // If Internet Explorer
    //{
    //    txtArea1.document.open("txt/html", "replace");
    //    txtArea1.document.write(tab_text);
    //    txtArea1.document.close();
    //    txtArea1.focus();
    //    sa = txtArea1.document.execCommand("SaveAs", true, "Say Thanks to Sumit.xls");
    //}
    //else                 //other browser not tested on IE 11
    //    sa = window.open('data:application/vnd.ms-excel,' + encodeURIComponent(tab_text));

    //return (sa);
}


function calculateColumn(index) {
    var total = 0;
    $('.table-master-data tbody tr').each(function () {
        var value = parseFloat($('td', this).eq(index).text().trim().replace(',', ''));
        console.log(parseFloat($('td', this).eq(index).text().trim().replace(',', '')));
        if (!isNaN(value)) {
            total += value;
        }
    });
    $('.table-master-data tfoot td').eq(index).html('<b>' + total.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",") + '</b>');
}

function recalculateTotals() {
    $('.table-master-data thead th').each(function (i) {
        if (i !== 0)
            calculateColumn(i);
    });
}

/*
http://stackoverflow.com/questions/28948383/how-to-implement-debounce-fn-into-jquery-keyup-event
*/

function GetActiveTab() {
    return $('.tab-content.active');
}
$('.txt-search').keyup(debounce(function () {
    console.log('starting... ');
    table_search(GetActiveTab().find('.txt-search').val(), $('.table-master-data tbody tr'), '012');
}, 500));
