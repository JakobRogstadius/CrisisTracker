function DotPlot() {
    "use strict";

//Internals
    var self = this;
    var container = null,
        svg = null,
        data = null,
        valueAxisLayer = null,
        categoryAxisLayer = null,
        itemLayer = null,
        xScale = d3.time.scale(),
        yScale = d3.scale.ordinal(),
        rScale = d3.scale.linear(),
        circles = null,
        nodes = null,
        selectedItem = null;

//Properties
    var width = 100,
        height = 100,
        margin = 10,
        rMultiplier = 1,
        parseTime = d3.time.format.utc("%Y-%m-%d %H:%M:%S").parse,
        getX = function(d) { return d; },
        getY = function(d) { return d; },
        getR = function(d) { return d; },
        getID = function(d) { return d; },
        getTooltip = function(d) { return d; },
        selectionCallback = null;

//Priviliged methods
    this.initialize = function(containerSelection) {
        container = containerSelection;
        svg = container.append("svg")
            .attr("class", "dot-plot");

        valueAxisLayer = svg.append("g");
        categoryAxisLayer = svg.append("g");
        itemLayer = svg.append("g");

        layout();

        return self;
    }

    this.selectedItem = function(item) {
        if (!arguments.length) return selectedItem;

        selectedItem = item;

        if (nodes == null)
            return self;

        var findVal = getID(item);
        var oldItem = nodes.filter(".selected")
            .classed("selected", false);
        var newItem = nodes.filter(function(d,i) { return getID(d)==findVal; })
            .classed("selected", true);

        return self;
    }

    this.data = function(value) {
        if (!arguments.length) return data;
        data = value.splice(0).sort(function(a,b) { return getR(b)-getR(a); });
        updateElements();
        layout();
        return self;
    }

    this.onSizeChanged = function() {
        layout();
        return self;
    }

    this.rMultiplier = function(func) {
        if (!arguments.length) return rMultiplier;
        rMultiplier = func;
        return self;
    }

    this.getX = function(func) {
        if (!arguments.length) return getX;
        getX = func;
        return self;
    }

    this.getY = function(func) {
        if (!arguments.length) return getY;
        getY = func;
        return self;
    }

    this.getR = function(func) {
        if (!arguments.length) return getR;
        getR = func;
        return self;
    }

    this.getID = function(func) {
        if (!arguments.length) return getID;
        getID = func;
        return self;
    }

    this.getTooltip = function(func) {
        if (!arguments.length) return getTooltip;
        getTooltip = func;
        return self;
    }

    this.onItemSelected = function(func) {
        if (!arguments.length) return selectionCallback;
        selectionCallback = func;
        return self;
    }

    this.doLayout = function() {
        layout();
        return self;
    }

//Private methods
    function updateElements() {
        if (svg == null || data == null)
            return;

        data.forEach(function(d) {
              d.dotplot_filter_time = parseTime(getX(d));
            });

        var uniqueYValues = data.map(getY).unique();
        yScale.domain(uniqueYValues);
        xScale.domain(d3.extent(data.map(function(d) { return d.dotplot_filter_time; })));
        rScale.domain([0, d3.max(data, function(d) { return Math.sqrt(getR(d)); })]);

        //Update circles
        itemLayer.selectAll("g.data-item").remove();

        nodes = itemLayer.selectAll("g.data-item")
            .data(data);

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
        rScale.range([2, 40]);

        layoutCategoryAxis();
        layoutValueAxis();
        layoutItems();
    }

    function layoutItems() {
        nodes.attr("transform", function(d) {
            return "translate(" + xScale(d.dotplot_filter_time) + "," + yScale(getY(d)) + ")"; })

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

    function onItemSelected(d, i) {
        if (selectedItem == d)
            return;

        if (selectionCallback != null) {
            self.selectedItem(d);
            selectionCallback(d, i);
        }
    }
}