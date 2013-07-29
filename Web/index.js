"use strict";
var myStoriesContextPanel = null;
var myStoryFocusPanel = null;

$(document).ready(function () {

  //GENERAL PAGE LAYOUT
  $('body').layout({
    applyDefaultStyles: true,
    livePaneResizing: true,
    resizable: true,

    //NORTH (page header)
    north__resizable: false,
    north__size: 60,

    //SOUTH (page footer)
    south__resizable: false,
    south__spacing_open: 0,
    south__size: 20,
  });

  //Load deep-linked story
  var getParams = Globals.getUrlVars();
  if ("story" in getParams) {
    console.log(getParams["story"]);
    d3.select("#page-content").attr('src', 'explore.html?story=' + getParams["story"]);
  }

});

function loadPage(path) {
    d3.select('#page-content').attr('src', path);
    var menu = d3.select('#page-header').select('.menu');
    menu.select('.selected').classed('selected', false);
    menu.select('[href="' + path + '"]').classed('selected', true);

    return false;
}

