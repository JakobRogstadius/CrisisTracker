function StoriesModel() {

  var filters = {
    minTime : null,
    maxTime : null,
    showOngoingStories : true,
    minLatitude : null,
    maxLatitude : null,
    minLongitude : null,
    maxLongitude : null,
    text : null
  }

  var renderState = {
    selectedStoryID : null,

    timeFilterInvalidated : true,
    topicFilterInvalidated : true,
    mapInvalidated : true,
    storyListInvalidated : true,

    refreshHandle : null
  }

  var storyDetailsModel = new StoryDetailsModel();

  var timelineFilter = new TimelineFilter("#stories-filters > div > svg",
                          Globals.apiPath + "get_total_volume_by_time.php?days=7",
                          function(d) { return d.date_hour; },
                          function(d) { return d.tweets; },
                          null,
                          function(min, max) { setTimeFilter(min, max, true); }
                          );

  $('input[id=stories-context-textfilter]').change(function() {
      setTextFilter($(this).val(), true);
    });

  $('input[id=stories-context-radio-ongoing]').change(function(){
      if ($(this).is(':checked')) {
          setShowOngoingStories(true, true);
      }
    });
  $('input[id=stories-context-radio-timefilter]').change(function(){
      if ($(this).is(':checked')) {
          setShowOngoingStories(false, true);
      }
    });

  /**
   * Private methods
   */

  function onFilterChanged() {
    onRefresh();
  }

  function onRefresh() {
    console.log("Refreshing stories tab")

    if (renderState.timeFilterInvalidated)
      reloadTimeFilterData();

    if (renderState.topicFilterInvalidated)
      reloadTopicFilterData();

    if (renderState.mapInvalidated)
      reloadMapData();

    if(renderState.storyListInvalidated)
      reloadStoriesData();

    window.clearTimeout(renderState.refreshHandle);
    renderState.refreshHandle = setTimeout(reloadAll, 300000);
  }

  function reloadAll() {
    renderState.timeFilterInvalidated = true;
    renderState.topicFilterInvalidated = true;
    renderState.mapInvalidated = true;
    renderState.storyListInvalidated = true;
    onFilterChanged();
  }

  /* Time filter */
  function reloadTimeFilterData() {
    console.log("Reloading timeline")

    timelineFilter.reloadData();
    renderState.timeFilterInvalidated = false;
  }

  /* Topic filter */
  function reloadTopicFilterData() {

  }
  function renderTopicFilters(error, data) {

    renderState.topicFilterInvalidated = false;
  }

  /* Map */
  function reloadMapData() {

  }
  function renderMap(error, data) {
    renderState.mapInvalidated = false;
  }

  /* Story list */
  function reloadStoriesData() {
    console.log("Reloading stories")

    var minT = filters.minTime == null ? "" : "&mintime=" + encodeURIComponent(Globals.getIsoTime(filters.minTime));
    var maxT = filters.maxTime == null ? "" : "&maxtime=" + encodeURIComponent(Globals.getIsoTime(filters.maxTime));
    var textF = filters.text == null ? "" : "&text=" + encodeURIComponent(filters.text);
    var geoF = "";
    var topicF = "";
    var ongoing = "&ongoing=" + (filters.showOngoingStories ? 1 : 0);
    var url = Globals.apiPath + "get_stories.php?" + minT + maxT + textF + geoF + topicF + ongoing;
    d3.json(url, renderStories);
  }
  function renderStories(error, data) {
    d3.select('#stories-list > ul').selectAll('li').remove();

    var rows = d3.select('#stories-list > ul')
      .selectAll('li')
      .data(data, function(d) { return d.story_id; } );

    d3.select('#stories-list > ul')
      .append('li')
      .classed('story', true)
      .classed('header', true)
      .html('<div class="time">Time</div><div class="sources">Reports</div><div class="title">Title</div>');

    var enter = rows.enter().append('li')
      .classed('story', true)
      .on('click', function(d) {
        onStorySelected(d.story_id);
        });

    enter.append('div')
      .classed('title', true)
      .append('a')
        .html(function(d) { return Globals.cleanTitle(d.title); })
        .on('click', function(d) {
          d3.event.preventDefault();
          });

    enter.append('div')
      .classed('time', true)
      .attr('title', function(d) { return d.start_time + " UTC"; })
      .html(function(d) { return Globals.getShortTime(d.start_time); });

    enter.append('div')
      .classed('sources', true)
      .html(function(d) { return d.weighted_size; });
/*
    enter.append('div')
      .html(function(d) { return '<div class="inlinesparkline" tweets="'+d.tweet_trend+'" retweets='+d.retweet_trend+'>&nbsp;</span>'; });

    $('#stories-list').find('.inlinesparkline').sparkline('html', {
      tagValuesAttribute: 'retweets',
      type: 'bar',
      barWidth: 3,
      barSpacing: 0,
      barColor: '#ddd',
      tooltipPrefix: 'Retweets: ' });
    $('#stories-list  ').find('.inlinesparkline').sparkline('html', {
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
    */
    renderState.storyListInvalidated = false;
  }

  function onStorySelected(id) {
    console.log("Selected story " + id);
    storyDetailsModel.loadStory(id);
  }

  /**
   * Public methods
   */

  function setTimeFilter(minT, maxT, fireEvents) {
    filters.minTime = minT;
    filters.maxTime = maxT;

    renderState.topicFilterInvalidated = true;
    renderState.mapInvalidated = true;
    renderState.storyListInvalidated = true;

    setShowOngoingStories(false, false);

    if (fireEvents)
      onFilterChanged();
  }

  function setTextFilter(text, fireEvents) {
    filters.text = text;
    if (filters.text == "")
      filters.text = null;

    renderState.topicFilterInvalidated = true;
    renderState.mapInvalidated = true;
    renderState.storyListInvalidated = true;

    if (fireEvents)
      onFilterChanged();
  }

  function setGeoFilter(minLat, minLon, maxLat, maxLon, fireEvents) {
    filters.minLatitude = minLat;
    filters.minLongitude = minLon;
    filters.maxLatitude = maxLat;
    filters.maxLongitude = maxLon;

    renderState.topicFilterInvalidated = true;
    renderState.mapInvalidated = true;
    renderState.storyListInvalidated = true;

    if (fireEvents)
      onFilterChanged();
  }

  function setShowOngoingStories(showOngoing, fireEvents) {
    filters.showOngoingStories = showOngoing;

    renderState.topicFilterInvalidated = true;
    renderState.mapInvalidated = true;
    renderState.storyListInvalidated = true;

    $('input[id=stories-context-radio-ongoing]').prop('checked', showOngoing);
    $('input[id=stories-context-radio-timefilter]').prop('checked', !showOngoing);

    if (fireEvents)
      onFilterChanged();
  }

  this.refresh = function() {
    renderState.timeFilterInvalidated = true;
    renderState.topicFilterInvalidated = true;
    renderState.mapInvalidated = true;
    renderState.storyListInvalidated = true;

    onRefresh();
  }
}
