function RetweetNetworkPlot() {
  "use strict";

//Internals
    var self = this;
    var container = null,
        svg = null,
        data = null,
        edgeLayer = null,
        itemLayer = null,
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
        getR = function(d) { return d; },
        getID = function(d) { return d; },
        getTooltip = function(d) { return d; },
        selectionCallback = null;

//Priviliged methods
    this.initialize = function(containerSelection) {
        container = containerSelection;
        svg = container.append("svg")
            .attr("class", "retweet-network-plot");

        edgeLayer = svg.append("g");
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

        rScale.range([2, 30]);




        var force = d3.layout.force()
            .size([width, height])
            .nodes(data.tweets)
            .links(data.links)
            .start();
        var forcelinks = force.links();
        mylinks.forEach(function(d) { forcelinks.push(d); });

        var svg = d3.select('#focus-tab-propagation> svg');

        var link = svg.selectAll(".link")
            .data(tweets)
          .enter().append("line")
            .attr("class", "link")
            .style("stroke-width", 1);

        var node = svg.selectAll(".node")
            .data(graph.nodes)
          .enter().append("circle")
            .attr("class", "node")
            .attr("r", 5)
            .style("fill", '#ccc')
            .call(force.drag);

        node.append("title")
            .text(function(d) { return d.user_real_name; });

        force.on("tick", function() {
          link.attr("x1", function(d) { return d.source.x; })
              .attr("y1", function(d) { return d.source.y; })
              .attr("x2", function(d) { return d.target.x; })
              .attr("y2", function(d) { return d.target.y; });

          node.attr("cx", function(d) { return d.x; })
              .attr("cy", function(d) { return d.y; })
              .attr("r", function(d) { return rMultiplier*rScale(Math.sqrt(getR(d))); });
        });
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


var eventRetweetNetwork  = {

  reload : function(id) {
    d3.json(common.apiPath + "get_retweet_network.php?id=" + id, this.render);
  },

  render : function(error, data) {

    var width = $('#focus-tab-propagation > svg').width(),
        height = $('#focus-tab-propagation > svg').height();

    //var color = d3.scale.category20();


  }
};
