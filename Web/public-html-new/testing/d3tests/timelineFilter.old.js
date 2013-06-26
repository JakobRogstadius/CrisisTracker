function TimelineFilter(containerPath,
                    dataUrl,
                    timeValueFunction,
                    foregroundValueFunction,
                    backgroundValueFunction,
                    onBrushFunction,
                    vertical) {
  
  var parseTime = d3.time.format.utc("%Y-%m-%d %H:%M:%S").parse;
  var path = containerPath;
  var data = null;
  
  this.reloadData = function() {
    d3.json(dataUrl, function(error, newData) {
      data = newData;
      data.forEach(function(d) {
        d.filter_time = parseTime(timeValueFunction(d));
      });
      
      render();
    });
  }
  
  function render() {

    var margin = {top: 0, right: 10, bottom: 20, left: 10},
        width = $(containerPath).width() - margin.left - margin.right,
        height = $(containerPath).height() - margin.top - margin.bottom;

    var x = d3.time.scale().range([0, width]),
        y1 = d3.scale.linear().range([height, 0]),
        y2 = d3.scale.linear().range([height, 0]);

    var xAxis = d3.svg.axis().scale(x).orient("bottom");

    var brush = d3.svg.brush()
        .x(x)
        .on("brushend", brushed);

    var svg = d3.select(containerPath)
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom)
        .classed("timefilter", true);

    svg.append("defs").append("clipPath")
        .attr("id", "clip")
      .append("rect")
        .attr("width", width)
        .attr("height", height);

    svg.selectAll("g").remove();
    var context = svg.append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    x.domain(d3.extent(data.map(function(d) { return d.filter_time; })));

    if (backgroundValueFunction != null) {
      y2.domain([0, d3.max(data.map(function(d) { return backgroundValueFunction(d); }))]);
      context.append("path")
        .datum(data)
        .attr("class", "background-series")
        .attr("d", d3.svg.area()
            .interpolate("linear")
            .x(function(d) { return x(d.filter_time); })
            .y0(height)
            .y1(function(d) { return y2(backgroundValueFunction(d)); }));
    }
    
    y1.domain([0, d3.max(data.map(function(d) { return foregroundValueFunction(d); }))]);
    context.append("path")
        .datum(data)
        .attr("class", "foreground-series")
        .attr("d", d3.svg.area()
            .interpolate("linear")
            .x(function(d) { return x(d.filter_time); })
            .y0(height)
            .y1(function(d) { return y1(foregroundValueFunction(d)); }));
    
    context.append("g")
        .attr("class", "x axis")
        .attr("transform", "translate(0," + height + ")")
        .call(xAxis);

    context.append("g")
        .attr("class", "x brush")
        .call(brush)
      .selectAll("rect")
        .attr("y", -6)
        .attr("height", height + 7);

    function brushed() {
      if (brush.empty())
        onBrushFunction(null, null);
      else
        onBrushFunction(brush.extent()[0], brush.extent()[1]);
    }
  }
}
