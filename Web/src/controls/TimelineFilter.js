function TimelineFilter() {
    "use strict";

//Internals
    var self = this;
    var container = null,
        svg = null,
        data = null,
        axisLayer = null,
        seriesLayer1 = null,
        seriesLayer2 = null,
        brushingLayer = null,
        highlightLayer = null,
        tScale = d3.time.scale(),
        yScale1 = d3.scale.linear(),
        yScale2 = d3.scale.linear(),
        area1 = null,
        area2 = null,
        brush = null,
        brushT = [null, null];

//Properties
    var width = 100,
        height = 100,
        margin = { top: 2, bottom: 13, left: 0, right: 0 },
        getT = function(d) { return d; },
        getY1 = function(d) { return d; },
        getY2 = null,
        tailCutoffRatio = 1,
        filterCallback = null,
        highlightTime = null;

//Priviliged methods
    this.initialize = function(containerSelection) {
        container = containerSelection;
        svg = container.append("svg")
            .attr("class", "timeline-filter");

        axisLayer = svg.append("g")
            .attr("class", "axis");
        seriesLayer2 = svg.append("g")
            .attr("class", "series2");
        seriesLayer1 = svg.append("g")
            .attr("class", "series1");
        brushingLayer = svg.append("g")
            .attr("class", "brushing");
        highlightLayer = svg.append("g")
            .attr("class", "highlight");

        layout();

        return self;
    }


    this.data = function(value) {
        if (!arguments.length) return data;
        data = value;
        updateElements();
        layout();
        return self;
    }

    this.tailCutoffRatio = function(value) {
        if (!arguments.length) return tailCutoffRatio;
        tailCutoffRatio = value;
        return self;
    }

    this.onSizeChanged = function() {
        layout();
        return self;
    }

    this.getT = function(func) {
        if (!arguments.length) return getT;
        getT = func;
        return self;
    }

    this.getY1 = function(func) {
        if (!arguments.length) return getY1;
        getY1 = func;
        return self;
    }

    this.getY2 = function(func) {
        if (!arguments.length) return getY2;
        getY2 = func;
        return self;
    }

    this.getID = function(func) {
        if (!arguments.length) return getID;
        getID = func;
        return self;
    }

    this.onFilter = function(func) {
        if (!arguments.length) return filterCallback;
        filterCallback = func;
        return self;
    }

    this.clearHighlights = function() {
        if (brush != null) {
          highlightTime = null;
          brush.clear();
          brushed();
        }
        return self;
    }

    this.highlightTime = function(value) {
        if (!arguments.length) return highlightTime;
        highlightTime = value;
        layout();
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
              d.timeline_filter_time = Globals.parseTime(getT(d));
            });
        data.sort(function(a,b) { return b.timeline_filter_time-a.timeline_filter_time });

        yScale1.domain([0, d3.max(data, getY1)]);
        area1 = d3.svg.area()
          .interpolate("linear")
          .x(function(d) { return tScale(d.timeline_filter_time); })
          .y0(getInnerHeight)
          .y1(function(d) { return yScale1(getY1(d)); })

        if (getY2 != null) {
            yScale2.domain([0, d3.max(data, getY2)]);
            area2 = d3.svg.area()
              .interpolate("linear")
              .x(function(d) { return tScale(d.timeline_filter_time); })
              .y0(getInnerHeight)
              .y1(function(d) { return yScale2(getY2(d)); })
        }

        //Trim the timeline range, to hide part of the long tail of retweets
        var sortedData = data.slice(0).sort(function(a,b) { return a.timeline_filter_time.getTime()-b.timeline_filter_time.getTime()})
        var cumSum = 0;
        var totSum = d3.sum(sortedData, getY1) + (getY2 == null ? 0 : d3.sum(sortedData, getY2));
        var cutOffTime = null;
        for (var i=0; i<sortedData.length; i++) {
            cumSum += getY1(sortedData[i]) + (getY2 == null ? 0 : getY2(sortedData[i]));
            if (cumSum >= tailCutoffRatio*totSum) {
                cutOffTime = sortedData[i].timeline_filter_time;
                break;
            }
        }
        var maxTime = sortedData[sortedData.length-1].timeline_filter_time;
        var minTime = sortedData[0].timeline_filter_time;
        if ((cutOffTime.getTime() - minTime.getTime()) > 0.9 * (maxTime.getTime()-minTime.getTime())) {
            cutOffTime = maxTime;
        }

        tScale.domain([sortedData[0].timeline_filter_time, cutOffTime]);

        brush = d3.svg.brush()
            .x(tScale)
            .on("brushend", brushed);

    }

    function layout() {
        if (container == null || data == null) {
            return;
        }

        if (area1 == null) {
            updateElements();
        }

        width = Math.max(margin.left + margin.right, $(container.node()).width());
        height = Math.max(margin.top + margin.bottom, $(container.node()).height());
        var innerWidth = getInnerWidth();
        var innerHeight = getInnerHeight();

        yScale1.range([innerHeight, 0]);
        yScale2.range([innerHeight, 0]);
        tScale.range([0, innerWidth]);

        brush.extent(brushT).x(tScale);
        brushingLayer
            .call(brush)
            .selectAll("rect")
                .attr("y", margin.top)
                .attr("height", innerHeight);

        layoutAreas();
        layoutTimeAxis();
        layoutHighlight();
    }

    function brushed() {
      brushT = brush.extent();
      if (Math.abs(brushT[0] - brushT[1]) == 0)
        brushT = [null, null];

      if (filterCallback != null) {
          filterCallback(brushT[0], brushT[1]);
      }
    }

    function getInnerHeight() { return height - margin.top - margin.bottom; }
    function getInnerWidth() { return width - margin.left - margin.right; }

    function layoutAreas() {
        seriesLayer1.select("path").remove();
        seriesLayer1
            .attr("transform", "translate(" + margin.left + "," + margin.top + ")")
            .append("path")
                .datum(data)
                .attr("d", area1);

        if (getY2 != null) {
            seriesLayer2.select("path").remove();
            seriesLayer2
                .attr("transform", "translate(" + margin.left + "," + margin.top + ")")
                .append("path")
                    .datum(data)
                    .attr("d", area2);
        }
    }

    function layoutHighlight() {
        if (highlightTime != null) {

            var highlightLine = d3.svg.line()
                .x(function(d) { return margin.left + Math.min(width-1, Math.max(1, tScale(d.x))); })
                .y(function(d) { return d.y; });

            var coords = [{ x: highlightTime, y: margin.top },
                           { x: highlightTime, y: height - margin.bottom }];

            highlightLayer.select("path").remove();
            highlightLayer.append("path")
                .datum(coords)
                .attr("d", highlightLine);
            highlightLayer.style("display", "inline");
        }
        else {
            highlightLayer
                .style("display", "none");
        }
    }

    function layoutTimeAxis() {
        var timeAxis = d3.svg.axis()
            .scale(tScale)
            .orient('bottom')
            .ticks(Math.round(width/60))
            .tickSize(-(innerHeight))
            .tickSubdivide(true);

        axisLayer
            .attr("transform", "translate(" + margin.left + "," + (height - margin.bottom) + ")")
            .call(timeAxis);
    }
}