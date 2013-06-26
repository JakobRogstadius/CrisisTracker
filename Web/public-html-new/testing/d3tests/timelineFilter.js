Array.prototype.contains = function(v) {
    for(var i = 0; i < this.length; i++) {
        if(this[i] === v) return true;
    }
    return false;
};

Array.prototype.unique = function() {
    var arr = [];
    for(var i = 0; i < this.length; i++) {
        if(!arr.contains(this[i])) {
            arr.push(this[i]);
        }
    }
    return arr;
}

function dotPlot() {

    //Properties
    var width = 100,
        height = 100,
        margin = { top: 0, bottom: 20, left: 0, right: 0 },
        getT = function(d) { return d; },
        getY = function(d) { return d; },
        selectionCallback = null;

    //Internals
    var container = null,
        svg = null,
        data = null,
        axisLayer = null,
        mainSeriesLayer = null,
        secondarySeriesLayer = null,
        brushingLayer = null,
        highlightLayer = null,
        tScale = d3.time.scale(),
        yScale = d3.scale.linear(),
        mainArea = null,
        secondaryArea = null;

    //Constructor-ish
    function my(containerSelection) {
        container = containerSelection;
        svg = container.append("svg")
            .attr("class", "timeline-filter");

        axisLayer = svg.append("g");
        mainSeriesLayer = svg.append("g");
        secondarySeriesLayer = svg.append("g");
        brushingLayer = svg.append("g");
        highlightLayer = svg.append("g");

        layout();
    }

    function updateElements() {
        if (svg == null || data == null)
            return;

        var uniqueYValues = data.map(getY).unique();
        yScale.domain(uniqueYValues);
        tScale.domain([0, d3.max(data, getT)]);
        rScale.domain([0, d3.max(data, function(d) { return Math.sqrt(getR(d)); })]);

        //Update circles
        nodes = itemLayer.selectAll("g.data-item")
            .data(data, getID);

        nodes.exit()
            .remove()
            .select("circle");

        nodes.enter().append("g")
            .attr("class", "data-item")
            .on("mousedown", onItemSelected)
            .on("mouseover", function(d, i) {
                d3.select(this)
                    .classed("mouseover", true);
            })
            .on("mouseout", function(d, i) {
                d3.select(this)
                    .classed("mouseover", false);
            })
            .append("circle");

        circles = nodes.selectAll("circle");

        nodes.append("title").text(getTooltip);

    }

    function onItemSelected(d, i) {
        if (selectionCallback != null) {
            my.setSelectedItem(d);
            selectionCallback(d, i);
        }
    }

    function layout() {
        if (container == null || data == null) {
            return;
        }

        if (nodes == null) {
            updateElements();
        }

        width = $(container.node()).width();
        height = $(container.node()).height();

        yScale.rangePoints([0, height], 1);
        xScale.range([margin, width-margin]);
        rScale.range([0, 40]);

        layoutCategoryAxis();
        layoutValueAxis();
        layoutItems();
    }

    function layoutItems() {
        nodes.attr("transform", function(d) {
            return "translate(" + xScale(getX(d)) + "," + yScale(getY(d)) + ")"; })

        circles
            .attr("r", function(d) { return rMultiplier*rScale(Math.sqrt(getR(d))); });
    }

    function layoutCategoryAxis() {
        var categoryAxes = categoryAxisLayer.selectAll('line')
            .data(yScale.range());

        categoryAxes.enter()
            .append("line")
            .attr("class", "category-axis");

        categoryAxes.exit()
            .remove();

        categoryAxes
            .attr("x1", 0)
            .attr("x2", width)
            .attr("y1", function(d) { return d; })
            .attr("y2", function(d) { return d; })
            .attr("class", "y axis");

        var axisLabels = categoryAxisLayer.selectAll("text.category-label")
            .data(yScale.domain(), String);
        axisLabels.enter().append("text")
            .attr("class", "category-label")
            .text(String);
        axisLabels.exit()
            .remove();
        axisLabels.attr("x", 3)
            .attr("y", function(d) { return yScale(d) - 2; });
    }

    function layoutValueAxis() {
        var ypos = 16;

        var valueAxis = d3.svg.axis()
            .scale(xScale)
            .orient('bottom')
            .ticks(Math.round(width/50))
            .tickSize(-(height-ypos))
            .tickSubdivide(true);

        valueAxisLayer
            .attr("class", "x axis")
            .attr("transform", "translate(0, " + (height-ypos) + ")")
            .call(valueAxis);
    }

    my.setSelectedItem = function(item) {
        if (nodes == null)
            return;

        findVal = getID(item);
        var oldItem = nodes.filter(".selected")
            .classed("selected", false);
        var newItem = nodes.filter(function(d,i) { return getID(d)==findVal; })
            .classed("selected", true);
    }

    my.data = function(value) {
        data = value;
        updateElements();
        layout();
        return my;
    }

    my.onSizeChanged = function() {
        layout();
        return my;
    }

    my.rMultiplier = function(func) {
        if (!arguments.length) return rMultiplier;
        rMultiplier = func;
        return my;
    }

    my.getX = function(func) {
        if (!arguments.length) return getX;
        getX = func;
        return my;
    }

    my.getY = function(func) {
        if (!arguments.length) return getY;
        getY = func;
        return my;
    }

    my.getR = function(func) {
        if (!arguments.length) return getR;
        getR = func;
        return my;
    }

    my.getID = function(func) {
        if (!arguments.length) return getID;
        getID = func;
        return my;
    }

    my.getTooltip = function(func) {
        if (!arguments.length) return getTooltip;
        getTooltip = func;
        return my;
    }

    my.onItemSelected = function(func) {
        if (!arguments.length) return selectionCallback;
        selectionCallback = func;
        return my;
    }

    my.doLayout = function() {
        layout();
        return my;
    }

    return my;
}