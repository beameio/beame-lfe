var MAX_CATEGORY_LABELS = 15;

var dashReportKinds = {    
    content: 'content'
    ,stores: 'stores'
};

function getPeriodKind() {
    return {
        periodSelectionKind: $('#periodSelectionKind').data("kendoDropDownList") != undefined ? $('#periodSelectionKind').data("kendoDropDownList").value() : window.DEFAULT_PERIOD_SELECTION
    };
}

function setReviewTotals(total) {
    $('#spn-review-total').html(total);
}

function onPeriodKindSelected(e) {
    var dataItem = this.dataItem(e.item.index());
    window.getNotifManagerInstance().notify(notifEvents.report.periodChanged, dataItem);
}

function rebindChart() {
    $('#SalesLineChart').data("kendoChart").dataSource.read();
}

function loadSalesStats(response) {
    try {
        $('.past7 > .total').html(response.total7);
        $('.past30 > .total').html(response.total30);
    } catch (e) {
    }
}

function setCurrentTotal(total) {

    try {
        var period = parseInt($('#periodSelectionKind').data("kendoDropDownList").value());
        var text = null;

        switch (period) {
            case PeriodKinds.week:
                text = 'past 7 days';
                break;
            case PeriodKinds.thisMonth:
                text = 'this month';
                break;
            case PeriodKinds.lastMonth:
                text = 'last 30 days';
                break;
            case PeriodKinds.last90:
                text = 'past 90 days';
                break;
            case PeriodKinds.last180:
                text = 'past 180 days';
                break;
            case PeriodKinds.all:
                text = 'all time';
                break;
        }

        if (text == null) return;

        $('.past7 > .total').html(total);
        $('.past7 > .sub').html(text);

    } catch (e) {
        if (window.console) console.log(e);
    }
}

function getUnit() {
    try {
        var period = parseInt($('#periodSelectionKind').data("kendoDropDownList").value());
        var unit = 'days';

        switch (period) {
            case PeriodKinds.week:
            case PeriodKinds.thisMonth:
            case PeriodKinds.lastMonth:
                return 'days';
            case PeriodKinds.last90:
                return 'weeks';
            case PeriodKinds.last180:
            case PeriodKinds.all:
                return 'months';
            default:
                return unit;
        }
    } catch (e) {
        return 'days';
    }
}

function onChartBound(e) {
    var chart = e.sender;

    var categoryAxis = chart.options.categoryAxis;
    categoryAxis.baseUnit = getUnit();

    var cats = categoryAxis.categories.length;

    if (cats <= 15 * 2) return;

    var unit = categoryAxis.baseUnit;
    var factor = 1;

    switch (unit) {
        case 'years':
            categoryAxis.labels.skip = 0;
            categoryAxis.labels.step = 1;
            return;
        case 'months':
            categoryAxis.labels.skip = 0;
            categoryAxis.labels.step = 1;
            return;
        case 'weeks':
            factor = 7;
            break;
    }

    var s = parseInt(Math.max(cats / factor / MAX_CATEGORY_LABELS, 1));

    categoryAxis.labels.skip = s;
    categoryAxis.labels.step = s;

    var typeInput = $("input:radio[name=chartType]");
    var type = typeInput.filter(":checked").val();
    setSeries(chart, type);
}

function setSeries(chart, type) {
    var series = chart.options.series;

    var color = '#ff9300';

    switch (type) {
        case 'line':
            color = '#ff9300';
            break;
        case 'column':
            color = '#1d6894';
            break;
    }

    for (var i = 0, length = series.length; i < length; i++) {
        series[i].type = type;
        series[i].color = color;
    }
}

function redrawChart(type) {
    var chart = $("#SalesLineChart").data("kendoChart");

    setSeries(chart, type);

    chart.redraw();
}