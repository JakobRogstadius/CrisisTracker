var ongoingEvents  = {

  reload : function() {
    d3.json(common.apiPath + "get_current_events.php", this.render);
  },

  render : function(error, events) {

    var rows = d3.select('#context-tab-ongoing-events > ul')
      .selectAll('li')
      .data(events, function(d) { return d.story_id; } );

    rows.exit()
      .remove();

    var enter = rows.enter().append('li')
      .classed('story', true)
      .on('click', function(d) {
        eventReports.loadStory(d.story_id);
        });
    
    enter.append('div')
      .classed('title', true)
      .append('a')
        .html(function(d) { return d.title; })
        .on('click', function(d) {
          d3.event.preventDefault();
          });

    var info = enter.append('div')
      .classed('info', true)
      .html(function(d) {
        return '<span class="inlinesparkline" tweets="'+d.tweet_trend+'" retweets='+d.retweet_trend+'>&nbsp;</span>'
        + '<span class="sources">' + d.weighted_size + ' reports</span><span class="time">' + d.start_time_short + '</span>';
        });

    $('#context-tab-ongoing-events').find('.inlinesparkline').sparkline('html', {
      tagValuesAttribute: 'retweets',
      type: 'bar',
      barWidth: 3,
      barSpacing: 0,
      barColor: '#ddd',
      tooltipPrefix: 'Retweets: ' });
    $('#context-tab-ongoing-events').find('.inlinesparkline').sparkline('html', {
      tagValuesAttribute: 'tweets',
      composite: true,
      type: 'line',
      width: '60',
      lineColor: '#222',
      fillColor: false,
      spotColor: false,
      minSpotColor: false,
      maxSpotColor: false,
      tooltipPrefix: 'Tweets: ' });
  }
}
