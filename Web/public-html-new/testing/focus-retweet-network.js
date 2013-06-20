var eventRetweetNetwork  = {

  reload : function(id) {
    d3.json(common.apiPath + "get_retweet_network.php?id=" + id, this.render);
  },
  
  render : function(error, data) {
    
    var width = $('#focus-tab-propagation > svg').width(),
        height = $('#focus-tab-propagation > svg').height();

    //var color = d3.scale.category20();

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
          .attr("cy", function(d) { return d.y; });
    });
  }
};
