
Array.prototype.sum = function (prop) {
    var total = 0
    for (var i = 0, _len = this.length; i < _len; i++) {
        total += this[i][prop]
    }
    return total
}


function DrawPerformanceValues(data, id)
{
    console.log('drawing ppEmpId ' + id + ' with data: ', data);

    am4core.ready(function() {


        // Themes begin
        am4core.useTheme(am4themes_animated);
        // Themes end

        // Color set
        var colors = new am4core.ColorSet();

        createLine("Late", data, am4core.color("#dc3545"), "chartdiv_late_" + id, "dateString", "totalLateMins");

        createColumn("Worked Hours", data, am4core.color("#17a2b8"), "chartdiv_workhours_" + id, "dateString", "actualWorkedHoursPerSchedule");

        createColumn("Overtime Hours", data, am4core.color("#ffc107"), "chartdiv_overtime_" + id, "dateString", "overtimeWorkedHoursPerSchedule");

        createLine("AbsentDays", data, am4core.color("#343a40"), "chartdiv_absent_" + id, "dateString", "totalAbsentCount");


        var pieData = new Array();
        pieData.push({
            category: "Scheduled Hours",
                    value: data.sum("totalScheduledHours")
                });
        pieData.push({
            category: "Actual Hours",
                    value: data.sum("actualWorkedHours")
                });

        console.log('pieData', pieData);
        createPie(pieData, "chartdiv_workhours_pie_" + id, am4core.color("#17a2b8"));


        //// Create chart instance
        //var container = am4core.create("chartdiv_late", am4core.Container);
        //container.layout = "grid";
        //container.fixedWidthGrid = false;
        //container.width = am4core.percent(100);
        //container.height = am4core.percent(100);



    }); // end am4core.ready()




    // Functions that create various sparklines
    function createLine(title, data, color, div, x, y)
    {

        //var chart = container.createChild(am4charts.XYChart);
        var chart = am4core.create(div, am4charts.XYChart);
        //chart.width = am4core.percent(45);
        //chart.height = 70;

        chart.data = data;

        //chart.titles.template.fontSize = 10;
        //chart.titles.template.textAlign = "left";
        //chart.titles.template.isMeasured = false;
        //chart.titles.create().text = title;

        // Set input format for the dates
        chart.dateFormatter.inputDateFormat = "yyyy-MM-dd";

        chart.padding(0, 0, 0, 0);

        //chart.padding(20, 5, 2, 5);

        var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        dateAxis.renderer.grid.template.disabled = true;
        dateAxis.renderer.labels.template.disabled = true;
        //dateAxis.startLocation = 0.5;
        //dateAxis.endLocation = 0.7;
        dateAxis.cursorTooltipEnabled = false;

        chart.dataDateFormat = "yyyy-MM-dd";
        //dateAxis.groupData = true;
        dateAxis.baseInterval = {
            timeUnit: "day"
                }

        var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.min = 0;
        valueAxis.renderer.grid.template.disabled = true;
        valueAxis.renderer.baseGrid.disabled = true;
        valueAxis.renderer.labels.template.disabled = true;
        valueAxis.cursorTooltipEnabled = false;

        chart.cursor = new am4charts.XYCursor();
        chart.cursor.lineY.disabled = true;
        chart.cursor.behavior = "none";

        // parser
        //chart.dateFormatter.inputDateFormat = "yyyy-MM-dd HH:mm:ss"

        var series = chart.series.push(new am4charts.LineSeries());
        series.tooltipText = "{" + x + "}: [bold]{" + y + "}"; // "{dateString}: [bold]{actualWorkedHours}";
        series.tooltip.getFillFromObject = false;
        series.tooltip.background.fill = color;
        series.dataFields.dateX = x; // "dateString";
        series.dataFields.valueY = y; //"actualWorkedHours";
        series.tensionX = 0.8;
        series.strokeWidth = 2;
        series.stroke = color;

        // render data points as bullets
        var bullet = series.bullets.push(new am4charts.CircleBullet());
        bullet.circle.opacity = 0;
        bullet.circle.fill = color;
        bullet.circle.propertyFields.opacity = "opacity";
        bullet.circle.radius = 3;

        //console.log('chart', chart.data);

        return chart;
    }

    function createColumn(title, data, color, div, x, y)
    {

        var chart = am4core.create(div, am4charts.XYChart);
        //var chart = container.createChild(am4charts.XYChart);
        //chart.width = am4core.percent(45);
        //chart.height = 70;

        chart.data = data;

        //chart.titles.template.fontSize = 10;
        //chart.titles.template.textAlign = "left";
        //chart.titles.template.isMeasured = false;
        //chart.titles.create().text = title;

        chart.dateFormatter.inputDateFormat = "yyyy-MM-dd";
        chart.numberFormatter.numberFormat = "#.##";

        chart.padding(0, 0, 0, 0);

        var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
        dateAxis.renderer.grid.template.disabled = true;
        dateAxis.renderer.labels.template.disabled = true;
        dateAxis.cursorTooltipEnabled = false;


        // --- (testing)

        chart.dataDateFormat = "yyyy-MM-dd";

        dateAxis.baseInterval = {
            timeUnit: "day",
                    count: 1
                }
        dateAxis.min = new Date(2020, 2, 10);
        dateAxis.max = new Date(2020, 3, 10);

        var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
        valueAxis.min = 0;
        valueAxis.renderer.grid.template.disabled = true;
        valueAxis.renderer.baseGrid.disabled = true;
        valueAxis.renderer.labels.template.disabled = true;
        valueAxis.cursorTooltipEnabled = false;

        chart.cursor = new am4charts.XYCursor();
        chart.cursor.lineY.disabled = true;

        var series = chart.series.push(new am4charts.ColumnSeries());
        series.tooltipText = "{" + x + "}: [bold]{" + y + "}";
        series.dataFields.dateX = x;
        series.dataFields.valueY = y;
        series.strokeWidth = 0;
        series.fillOpacity = 0.5;
        series.fixedWidthGrid = true;
        series.columns.template.propertyFields.fillOpacity = "opacity";
        series.columns.template.fill = color;

        return chart;
    }

    function createPie(data, div, color)
    {

        var chart = am4core.create(div, am4charts.PieChart);
        //chart.maxHeight = 50;
        //chart.width = am4core.percent(10);
        //chart.scale = 0.5;
        chart.padding(0, 0, 0, 0);
        chart.margin(1, 1, 1, 1);
        //chart.radius = am4core.percent.percent(50);

        chart.data = data;
        //chart.innerRadius = am4core.percent(40);
        //chart.paddingTop = 0;
        //chart.marginTop = 0;
        //chart.valign = 'top';
        //chart.contentValign = 'top'
        //chart.autoMargins = false;
        chart.radius = 9;

        // Add and configure Series
        var pieSeries = chart.series.push(new am4charts.PieSeries());
        pieSeries.dataFields.value = "value";
        pieSeries.dataFields.category = "category";
        pieSeries.labels.template.disabled = true;
        pieSeries.ticks.template.disabled = true;
        pieSeries.slices.template.fill = color;
        pieSeries.slices.template.adapter.add("fill", function(fill, target) {
            return fill.lighten(0.1 * target.dataItem.index);
        });
    pieSeries.slices.template.stroke = am4core.color("#fff");

    // chart.chartContainer.minHeight = 40;
    // chart.chartContainer.minWidth = 40;

    return chart;
}

        }