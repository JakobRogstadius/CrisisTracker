"use strict";
var myStoriesContextPanel = null;
var myStoryFocusPanel = null;

function layoutPanels() {
  $('#stories-context-panel-tabs').tabs();
  $('#stories-focus-panel-content').tabs();

  //GENERAL PAGE LAYOUT
  $('body').layout({
    applyDefaultStyles: true,
    livePaneResizing: true,
    resizable: true,

    //WEST (filters)
    west__size: 200,
  });

  $('#stories-content-panel').layout({
    applyDefaultStyles: true,
    livePaneResizing: false,
    resizable: false,
    closable: false,
    spacing_open: 1,

    east__size: 0.5,
  });


  $('#stories-filters-panel').layout({
    applyDefaultStyles: true,
    livePaneResizing: false,
    resizable: false,

    north__applyDefaultStyles: false,
    north__size: 22,
    north__spacing_open: 0,
    north__resizable: false,
    north__closable: false,
  });

  //Stories context list panel (timeline and list)
  $('#stories-context-panel').layout({
    applyDefaultStyles: true,
    livePaneResizing: false,
    resizable: false,

    north__applyDefaultStyles: false,
    north__size: 22,
    north__spacing_open: 0,
    north__resizable: false,
    north__closable: false,
  });

  $('#stories-context-panel-content').layout({
    applyDefaultStyles: true,
    livePaneResizing: true,
    resizable: true,

    //NORTH (timeline)
    north__onresize_end: onContextPanelResize,
    north__size: 80,
    north__spacing_open: 1,
    north__resizable: false,
    north__closable: false,
  });

  $('#stories-focus-panel').layout({
    applyDefaultStyles: true,
    livePaneResizing: false,
    resizable: false,

    north__applyDefaultStyles: false,
    north__size: 22,
    north__spacing_open: 0,
    north__resizable: false,
    north__closable: false,
  });


  //Stories focus panel (title and content)
  $('#stories-focus-panel-content-wrapper').layout({
    applyDefaultStyles: true,
    livePaneResizing: true,
    resizable: true,

    //NORTH (title)
    north__applyDefaultStyles: false,
    north__size: 54,
    north__spacing_open: 0,
    north__resizable: false,
    north__closable: false,
  });

  //Stories context list panel (timeline and list)
  $('#stories-focus-tab-tweets').layout({
    applyDefaultStyles: true,
    livePaneResizing: true,
    resizable: true,

    //NORTH (timeline)
    north__onresize_end: onFocusPanelResize,
    north__size: 105,
    north__spacing_open: 1,
    north__resizable: false,
    north__closable: false
  });
}

function onContextPanelResize() {
  myStoriesContextPanel.resize();
  return true;
}

function onFocusPanelResize() {
  myStoryFocusPanel.resize();
  return true;
}

$(document).ready(function () {
  layoutPanels();

  myStoriesContextPanel = new StoriesContextPanel()
    .initialize(
      d3.select("#stories-context-panel-timeline"),
      d3.select("#stories-context-tab-list"),
      d3.select("#stories-context-tab-bubbles")
    )
    .reloadAll();

  myStoryFocusPanel = new StoryFocusPanel()
    .initialize(
      d3.select("#stories-focus-panel-content"),
      d3.select("#stories-focus-panel-title"),
      d3.select("#stories-focus-panel-filters"),
      d3.select("#stories-focus-panel-list"),
      d3.select("#stories-focus-tab-similar")
    );

  myStoriesContextPanel.onStorySelected(function(d,i) {
    console.log(d);
    myStoryFocusPanel.loadStory(d.story_id);
  });

  var tf = new TextFilter()
    .initialize(d3.select("#context-text-filter"))
    .onFilterChanged(function(text) { myStoriesContextPanel.textFilter(text); });

  var topicF = new TopicFilter()
      .getL1ID(function(d) { return d.attribute_id; })
      .getL1Name(function(d) { return d.attribute_name; })
      .getL1Tooltip(function(d) { return d.attribute_description; })
      .getL2List(function(d) { return d.labels; })
      .getL2ID(function(d) { return d.label_id; })
      .getL2Name(function(d) { return d.label_name; })
      .getL2Tooltip(function(d) { return d.label_description; })
      .getL2Value(function(d) { return d.story_count; })
      .onFilterChanged(function(ids) { myStoriesContextPanel.topicFilter(ids); })
      .initialize(d3.select("#context-topic-filter"));

  d3.json(Globals.apiPath + "get_aidr_topic_filters.php", function(d) { topicF.data(d); });

  //Load deep-linked story
  var getParams = Globals.getUrlVars();
  if ("story" in getParams) {
    myStoryFocusPanel.loadStory(parseInt(getParams["story"]));
  }
  else {
      var url = Globals.apiPath + "get_top_story.php";
      d3.json(url, function(d) {
        myStoriesContextPanel.selectStory(d);
        myStoryFocusPanel.loadStory(parseInt(d.story_id));
      });
  }
});
