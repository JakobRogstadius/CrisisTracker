<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('header_start.php');
include('storymanagementincludes.php');
?>
<link rel="stylesheet" href="resources/css/storylist.css" type="text/css" media="screen" />
<link rel="stylesheet" href="resources/css/explorestories.css" type="text/css" media="screen" />

<script type="text/javascript" src="resources/javascript/explorestories.js"></script>
<script type="text/javascript" src="resources/javascript/ses.userpointcreation.js"></script>
<script type="text/javascript" src="resources/javascript/ses.randomgenerators.js"></script>

<script type="text/javascript">
  // Stories Explorer
  var environment = new storiesExplorer();

    //Initialize category buttons
    $(document).ready(function(){

    //***************************************************************
    // Map
    environment.map = new OpenLayers.Map('map',{
      maxExtent: new OpenLayers.Bounds(  // Explicitly Set 'Boundaries of the World'  -- EPSP:900913
      -128 * 156543.0339,
      -128 * 156543.0339,
      128 * 156543.0339,
      128 * 156543.0339),
      maxResolution: 156543.0339,      // fitting the environment.map's extent into 256 pixels
      sphericalMercator: true,
      units: 'm',              // Spherical Mercator is a projection that uses meters 'm'
      projection: new OpenLayers.Projection('EPSG:900913'),  // Spherical Mercator Projection
      displayProjection: new OpenLayers.Projection('EPSG:4326'),  // User see this
      eventListeners: {
        // "movestart": handleMapEventStart,
        // "zoomstart": handleMapEventStart,
        "moveend": handleMapEvent,
        "zoomend": handleMapEvent
        // "changelayer": mapLayerChanged,
        // "changebaselayer": mapBaseLayerChanged
      }
    });

    // Google Maps Layer
    var google_streets= new OpenLayers.Layer.Google("Google",{type: google.maps.MapTypeId.ROADMAP, numZoomLevels: 16});
    var google_satellite = new OpenLayers.Layer.Google("Google Satellite",{type: google.maps.MapTypeId.SATELLITE, numZoomLevels: 20});

    // Vector Layer
    environment.vector_layer = new OpenLayers.Layer.Vector('Vector Layer', {
      projection: new OpenLayers.Projection('EPSG:4326')
    });

    // Vector Layer StyleMap
    var vector_layer_style_map = new OpenLayers.StyleMap({
      'default': generateStyleMap("explore")
    });

    // Apply StyleMap to vector_layer
    environment.vector_layer.styleMap = vector_layer_style_map;

    // Map Layers
    environment.map.addLayers([google_streets,google_satellite,environment.vector_layer]);
    environment.map.zoomToMaxExtent();

    // MAP CONTROLS
    // Layer Switcher
    environment.map.addControl(new OpenLayers.Control.LayerSwitcher());

    //***************************************************************
    // Text Tag (Entities, Categories, Keywords)
    $('#tags_entities').tagsInput({width:'350', height:'25px', defaultText:'Enter name', minChars : 0, maxChars : 45, placeholderColor : '#9C9C9C', removeWithBackspace:false,onAddTag:onAddTagEntities,onRemoveTag:onRemoveTagEntities,onChange: onChangeTagEntities});

    $('#tags_keywords').tagsInput({width:'350',height:'25px', defaultText:'Enter keyword', minChars : 0,  maxChars : 45,  placeholderColor : '#9C9C9C', removeWithBackspace:false, onAddTag:onAddTagKeywords,onRemoveTag:onRemoveTagKeywords,onChange: onChangeTagKeywords});

    //***************************************************************
    // Data Structures Initializations
    added_geotags_array = new Array();

    //***************************************************************
    // Scripts
    resetTextTagsList();
    addDateTextboxListeners();
    startDatePicker();
    userSearchMapLocation();
    hideLoadingAnimation(); // hide loading 'gif'
    activateMapBorderSelection();
    populateFilterSettings();

      });
</script>

<?php
include('header_end.php');
?>

<!-- TEXT-TAGS -->
<div class="left-column-narrow">

  <!-- Categories & Keywords -->
    <div class="gui-panel what-filter-panel">
        <h2>What</h2>
        <div id="categorybuttons"></div>

        <div id="keywords" class="what-content">
            <form>
                 <br />  <input name="tags_keywords" id="tags_keywords" value="ferrari" /> <br />
            </form>
        </div>
    </div>

  <!-- Entities -->
    <div class="gui-panel who-filter-panel">
        <h2>Who</h2>
        <div id="entities" class="what-content">
            <form>
                 <br />  <input name="tags_entities" id="tags_entities" value="fire dept" /> <br />
            </form>
        </div>
    </div>

  <!-- Time -->
    <div class="gui-panel when-filter-panel">
        <h2>When</h2>
        <div class="related-content">

      <!-- JQuery DatePicker-->
      <label for="from">From</label>
      <input type="text" id="from" name="from" class="date-selector"/>
      <label for="to">to</label>
      <input type="text" id="to" name="to" class="date-selector"/>

      <div class="date-picker-description"></div>

        </div>
    </div>
</div>



<!-- MAP -->
<div class="right-column-wide">
    <div class="gui-panel where-panel">
        <h2>Where</h2>
      <div id="map_holder" class="where-content">

      <!-- environment.map bounds query check box -->
        <input type = "checkbox" id = "mapbounds" value = "false"/>
       <label for = "mapbounds">Only show stories within map bounds</label>

      <!-- map div -->
      <div id="map" class="map_inactive"></div>

      <!-- map search box -->
            <div id="mapsearchfield">
        Find location:
        <br />
                <form>
          <input type="text" value="" id="searchbox"/><br />
                </form>
            </div>

        </div>
    </div>

  <!-- PAGE search button -->
  <div class="search-panel">
    <!-- Html5 Save Button -->
    <button type="button" onclick="handleSearchButtonClick()" class="button-submit">SEARCH NOW</button>

    <div id="loading">
      <p><img src="resources/images/loading.gif" alt="Loading..."/> Loading...</p>
    </div>
  </div>

</div>





<!-- LIST OF STORIES -->
<div class="fullwidth-column">
    <div class="gui-panel filtered-stories-panel">
        <div class="priority-filters">
          <p>Sort order <select id="sortOrderSelect" onchange="switchPriority(this.options[this.selectedIndex].value)">
            <optgroup label="All twitter users">
              <option value="active-all" selected="selected">Most shared in the past four hours</option>
              <option value="largest-all">Most shared all-time, by size</option>
              <option value="timeline-all">Most shared all-time, by time</option>
              <option value="newest-all">Newest</option>
            </optgroup>
            <optgroup label="Domain experts (top 5000 users)">
              <option value="active-top">Most shared in the past four hours</option>
              <option value="largest-top">Most shared all-time, by size</option>
              <option value="timeline-top">Most shared all-time, by time</option>
              <option value="newest-top">Newest</option>
            </optgroup>
          </select></p>
        </div>
        <div class="gui-subpanel story-list explorestories-list">
            <ol id="story-list-items">
                <li class="story-list-item header">
                    <span class="column size" title="A weighted value of the total number of users, tweets and retweets in a story">Size</span>
                    <span class="column time" title="The time (UTC) at which the first tweet in the story was posted">Time</span>
                    <span class="column title" title="Derived from the most representative tweet in the story">Title</span>
                    <span class="column" title="Tags contributed by volunteers">Tags</span>
                </li>
                <!-- Populated via JavaScript -->
            </ol>
        </div>
    </div>
</div>

<?php
include('footer.php');
?>