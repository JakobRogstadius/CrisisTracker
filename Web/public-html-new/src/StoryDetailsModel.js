function StoryDetailsModel() {

  var currentStoryID = null;
  var minTime = null;
  var maxTime = null;
  var textFilter = null;

  var timelineFilter;

  var loadingState = {
    reports : false,
    media : false,
    propagation : false,
    entities : false,
    related : false
  }

  var names = {
    tabContainer : "#stories-focus",
    titleBar : "#stories-focus-title",
    textFilter : "input[id=stories-focus-textfilter]",
    reportsTab : "#stories-focus-tab-reports",
    mediaTab : "#stories-focus-tab-media",
    entitiesTab : "#stories-focus-tab-entities",
    propagationTab : "#stories-focus-tab-propagation",
    relatedTab : "#stories-focus-tab-related"
  }

  function initialize() {
    //Bind event listener to text filter change
    $(names.textFilter).change(function() {
      textFilter = $(names.textFilter).val();
      if (textFilter == "")
        textFilter = null;

      reloadTweetList();
    });

    //Bind event listeners to detect when user switches tabs
    $(names.tabContainer).on("tabsactivate", onTabChanged);
  }

  this.loadStory = function(id) {
    loadStoryPrivate(id);
  }

  function reset() {
    clearTweetList();
    clearRelatedStories();

    loadingState.reports = false;
    loadingState.media = false;
    loadingState.propagation = false;
    loadingState.entities = false;
    loadingState.related = false;

    minTime = null;
    maxTime = null;
    textFilter = null;

    $(names.titleBar).html(null);
    $(names.textFilter).val(null);
  }

  function loadStoryPrivate(id) {
    if (id == currentStoryID) {
      return;
    }

    reset();

    $(names.tabContainer).tabs( "option", "active", 0 );

    console.log("Loading content for story " + id);

    currentStoryID = id;
    minTime = null;
    maxTime = null;
    textFilter = null;

    timelineFilter = new TimelineFilter(names.reportsTab + " > div > svg",
                          Globals.apiPath + "get_story_volume_by_time.php?id="+id,
                          function(d) { return d.created_at; },
                          function(d) { return d.first_seen_at; },
                          function(d) { return d.tweet_count; },
                          function(minT, maxT) {
                            minTime = minT;
                            maxTime = maxT;
                            reloadTweetList(); });

    timelineFilter.reloadData();
    reloadTweetList();
    reloadTitle();
  };

  function reloadTitle() {
    d3.json(Globals.apiPath + "get_story_info.php?id=" + currentStoryID, function(error, data) {
      $("#stories-focus-title").html(Globals.cleanTitle(data.title));
    });
  }

  function reloadTweetList() {
    console.log("Updating tweet list");

    var minT = minTime == null ? "" : "&mintime=" + encodeURIComponent(Globals.getIsoTime(minTime));
    var maxT = maxTime == null ? "" : "&maxtime=" + encodeURIComponent(Globals.getIsoTime(maxTime));
    var textF = textFilter == null ? "" : "&text=" + encodeURIComponent(textFilter);
    var url = Globals.apiPath + "get_story_tweets.php?id=" + currentStoryID + minT + maxT + textF;
    console.log(url);
    d3.json(url, renderTweetList);
  };

  function clearTweetList() {
    d3.select(names.reportsTab + ' > ul').selectAll('li').remove();
  }

  function renderTweetList(error, tweets) {
    console.log("Rendering tweet list");

    clearTweetList();

    var rows = d3.select(names.reportsTab + ' > ul')
      .selectAll('li.tweet')
      .data(tweets, function(d) { return d.tweet_id; } );

    var enter = rows.enter().append('li')
      .classed('tweet', true);

    enter.append('img')
      .attr('src', function(d) { return d.profile_image_url; });

    enter.append('div')
      .classed('text', true)
      .html(function(d) { return Globals.linkify(d.text); });

    var info = enter.append('div')
      .classed('source', true)
      .html(function(d) {
        return Globals.getShortTime(d.created_at)
          + ' from <a href="http://twitter.com/' + d.user_screen_name + '/status/'
          + d.tweet_id + '" target="_blank">' + d.user_real_name + '</a>'
          + (d.user_count>1 ? (' and ' + (d.user_count-1) + ' others') : '');
        });

    //http://twitter.com/#!/akhbarlb/status/338116842017148928
  };

  function onTabChanged(event, ui) {
    if (ui.newPanel.selector == names.relatedTab) {
      reloadRelatedStories();
    }
  }

  function reloadRelatedStories() {
    var url = Globals.apiPath + "get_similar_stories.php?id=" + currentStoryID;
    d3.json(url, renderSimilarStoriesList);
  }

  function clearRelatedStories() {
    d3.select(names.relatedTab + " > ul").selectAll('li').remove();
  }

  function renderSimilarStoriesList(error, data) {
    clearRelatedStories();

    var rows = d3.select(names.relatedTab + ' > ul')
      .selectAll('li')
      .data(data, function(d) { return d.story_id; } );

    d3.select(names.relatedTab + ' > ul')
      .append('li')
      .classed('story', true)
      .classed('header', true)
      .html('<div class="time">Time</div><div class="sources">Reports</div><div class="title">Title</div>');

    var enter = rows.enter().append('li')
      .classed('story', true)
      .on('click', function(d) {
        loadStoryPrivate(d.story_id);
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

    enter.append('div')
      .classed('merge', true)
      .append('a')
        .html("Merge")
        .on('click', function(d) {
          var url = Globals.apiPath + "merge_stories.php?storyid1=" + d.story_id + "&storyid2=" + currentStoryID;
          console.log(url);
          d3.html(url, function(error, data) { console.log(data); });
          var mergeDiv = $(this).parent();
          mergeDiv.parent().addClass("merged");
          mergeDiv.empty();
          mergeDiv.html("<i>merging</i>");
          d3.event.stopPropagation();
          });
  }

  initialize();
}
