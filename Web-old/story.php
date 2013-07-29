<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('api/was_story_merged.php');
include('api/get_story_content.php');
include_once('api/common_functions.php');

$sortOrder = 'time';
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
  if ($sortOrder != 'time' && $sortOrder != 'size' && $sortOrder != 'first20') {
    $sortOrder = 'time';
  }
}

$storyID = -1;
if (isset($_GET['storyid'])) {
  $storyID = intval($_GET['storyid']);
}
if ($storyID<0) {
  exit;
}

include('api/open_db.php');

//Redirect if this story was merged with another
$mergedWith = was_story_merged($storyID, $db_conn);
if (!is_null($mergedWith))
    header("Location: story.php?storyid=$mergedWith");

//Fetch story content
$story = get_story_content($storyID, $sortOrder, $db_conn);
$documentTitle = htmlspecialchars($story['customTitle'] != '' ? $story['customTitle'] : $story['title']);

include('header_start.php');
include('storymanagementincludes.php');
?>
  <!-- CSS -->
  <link rel="stylesheet" href="resources/css/story.css" type="text/css" media="screen" />

  <!-- User Point Creation -->
  <script type="text/javascript" src="resources/javascript/ses.userpointcreation.js"></script>
  <!-- Active Story Management (update story_tagger.active_story with database returns) -->
  <script type="text/javascript" src="resources/javascript/ses.activestorymanagement.js"></script>
  <!-- Features (create features, remove features, create features from geotags) -->
  <script type="text/javascript" src="resources/javascript/ses.features.js"></script>
  <!-- Popup timer -->
  <script type="text/javascript" src="resources/javascript/ses.popuptimer.js"></script>
  <script type="text/javascript">
  // Objects
  var environment = new storyTagger();

  // Global Variables
  var drawControls, selectControl;
  var select_feature_control;
  var drag_feature_control;

  // JQuery Document Ready Handler *autostart
  $(document).ready(function(){

    // Map
    environment.map = new OpenLayers.Map('map');
    /*environment.map = new OpenLayers.Map('map',{
        maxExtent: new OpenLayers.Bounds(  // Explicitly Set 'Boundaries of the World'  -- EPSP:900913
        -128 * 156543.0339,
        -128 * 156543.0339,
        128 * 156543.0339,
        128 * 156543.0339),
        maxResolution: 156543.0339,      // fitting the map's extent into 256 pixels
        sphericalMercator: true,
        units: 'm',              // Spherical Mercator is a projection that uses meters 'm'
        projection: new OpenLayers.Projection('EPSG:900913'),  // Spherical Mercator Projection
        displayProjection: new OpenLayers.Projection("EPSG:4326"),  // User see this
    });*/

    // Google Maps Layer
    var google_streets= new OpenLayers.Layer.Google("Google",{type: google.maps.MapTypeId.ROADMAP, numZoomLevels: 20});
    var google_satellite = new OpenLayers.Layer.Google("Google Satellite",{type: google.maps.MapTypeId.SATELLITE, numZoomLevels: 20});

    // Vector Layer
    environment.vector_layer = new OpenLayers.Layer.Vector('Vector Layer', {
      projection: new OpenLayers.Projection('EPSG:4326')
    });

    // Vector Layer StyleMap
    var vector_style_default_map = new OpenLayers.StyleMap({
      'default': generateStyleMap('tag_default'),
      'select': generateStyleMap('tag_select')
    });

    // Apply StyleMap to environment.vector_layer
    environment.vector_layer.styleMap = vector_style_default_map;

    // Map Layers
    environment.map.addLayers([google_streets,google_satellite,environment.vector_layer]);
    environment.map.zoomToMaxExtent();

    // MAP CONTROLS
    // Layer Switcher
    environment.map.addControl(new OpenLayers.Control.LayerSwitcher());
    // Select Feature
    select_feature_control = new OpenLayers.Control.SelectFeature(environment.vector_layer,
      {onSelect: handleOnClickFeatureSelect,   // on_select handler
      onUnselect: handleOnFeatureUnselect});  // on_unselect handler
    environment.map.addControl(select_feature_control);
    select_feature_control.activate();
    // Mouse Position
    mousePosition = new OpenLayers.Control.MousePosition();    // used for move_feature
    environment.map.addControl(mousePosition);
    // Drag Feature
    drag_feature_control = new OpenLayers.Control.DragFeature(environment.vector_layer,
      {onComplete : handleFeatureEndOfDragging,
      onStart : handleFeatureStartOfDragging });
    environment.map.addControl(drag_feature_control);


    // TextTag Boxes (Entities, Categories, Keywords)
    $('#tags_entities').tagsInput({width:'350', height:'50px', defaultText:'add a tag', minChars : 0, maxChars : 45, placeholderColor : '#9C9C9C', removeWithBackspace:false,onAddTag:onAddTagEntities,onRemoveTag:onRemoveTagEntities,onChange: onChangeTagEntities});

    $('#tags_keywords').tagsInput({width:'350',height:'50px', defaultText:'add a tag', minChars : 0,  maxChars : 45,  placeholderColor : '#9C9C9C', removeWithBackspace:false, onAddTag:onAddTagKeywords,onRemoveTag:onRemoveTagKeywords,onChange: onChangeTagKeywords});

    // Data Structures Initializations
    active_user = null;
    addedCategories = new Array();
    addedEntities = new Array();
    addedKeywords = new Array();
    removedCategories = new Array();
    removedEntities = new Array();
    removedKeywords = new Array();
    environment.texttag_flag = true;

    // Scripts
    registerMouseClickEvent();
    zoomMap(1);
    userSearchMapLocation();
    resetTextTagsList();
    // Load Story
    firstStoryLoad(<?php echo $storyID; ?>);  // story id

  }) //eof

  // ---------------------------------------------------------------------------------------
  /**
   * Initial Load of a Story with sequence of 'Local -> Webservice -> Local' callbacks to render a story to local datastructure
   *@param  storyId{int}
  **/
  function firstStoryLoad(storyId) {
    ajaxGetItems(storyId); // xmlParser HTTP Request to WebService
  }

  //Login alerts
  function mergeStoryCheck() {

  }

  function addClass(itemID, name) {
    document.getElementById(itemID).className += " " + name;
  }
  function removeClass(itemID, name) {
    //r = new RegExp("(?:^|\s)" + name + "(?!\S)", "g");
    r = new RegExp(name, "g");
    document.getElementById(itemID).className = document.getElementById(itemID).className.replace(r, '')
  }
  function showDefaultTitle() {
    removeClass("panel-default-title", "hidden");
    addClass("panel-translated-title", "hidden");
    addClass("button-default-title", "selected");
    removeClass("button-translated-title", "selected");
  }
  function showTranslatedTitle() {
    addClass("panel-default-title", "hidden");
    removeClass("panel-translated-title", "hidden");
    removeClass("button-default-title", "selected");
    addClass("button-translated-title", "selected");
  }
</script>

<?php
include('header_end.php');
$hasCT = $story['customTitle'] != '';
?>

<div class="left-column-wide">
    <div class="gui-panel story-panel">
        <div class="multipanel-container">
            <div id="button-default-title" class="panel-button <?php echo (!$hasCT ? 'selected' : ''); ?>" onclick="showDefaultTitle()">Default title</div>
            <div id="button-translated-title" class="panel-button <?php echo ($hasCT ? 'selected' : ''); ?>" onclick="showTranslatedTitle()">Translation/Summary</div>
            <div id="panel-default-title" class="panel <?php echo ($hasCT ? 'hidden' : ''); ?>"><h1><?php echo addLinksToText($story['title']); ?></h1></div>
            <div id="panel-translated-title" class="panel <?php echo (!$hasCT ? 'hidden' : ''); ?>">
              <textarea id="custom-title-box"><?php echo $story['customTitle']; ?></textarea>
              <input type="button" value="Save" onclick="saveCustomTitle()"/>
            </div>
        </div>
        <div class="story-info">
            <div class="usercount">
                Shared by<br/><span class="value"><?php echo $story['userCount']; ?></span>
            </div>
            <div class="tweetcount">
                Tweets<br/><span class="value"><?php echo $story['tweetCount']; ?></span>
            </div>
            <div class="retweetcount">
                Retweets<br/><span class="value"><?php echo $story['retweetCount']; ?></span>
            </div>
            <div class="starttime">
                First seen<br/><span class="value" title="<?php echo $story['startTime']; ?>"><?php echo $story['startTimeShort']; ?></span>
            </div>
            <div class="endtime">
                Last seen<br/><span class="value" title="<?php echo $story['endTime']; ?>"><?php echo $story['endTimeShort']; ?></span>
            </div>
        </div>
        <h2>First report</h2>
        <div class="gui-subpanel story-first-tweet">
<?php
    $firstTweet = $story['firstTweet'];
    echo '<div class="story-title">' . addLinksToText($firstTweet['text']) . '</div>';
    echo '<div class="story-footer">' . $firstTweet['createdAtShort'] . ' by ' . $firstTweet['realName'] . ' (<a href="http://twitter.com/#!/' . $firstTweet['screenName'] . '/status/' . $firstTweet['tweetID'] . '" target="_blank">@' . $firstTweet['screenName'] . '</a>)</div>';
?>
        </div>
        <div>
            <span style="float: right; margin-top: 4px;">
                <?php
                    $hideStoryTooltip = "Hide this story if it does not contribute to situation awareness. Hiding the story excludes it from searches in CrisisTracker.";
                    $hideStoryConfirm = "Stories that do not contribute to situation awareness should be hidden. Hide this story?";
                    $hideStoryCaption = "Hide story";
                    if ($story['isHidden']==1) {
                        $hideStoryTooltip = "This story has been hidden because it does not contribute to situation awareness. Show this story to include it in searches again.";
                        $hideStoryConfirm = "Stories that contribute to situation awareness should not be hidden. Show this story?";
                        $hideStoryCaption = "Show story";
                        $hideStoryStyle = ' actionlink-highlight';
                    }
                ?>
                <a
                  class="actionlink<?php echo $hideStoryStyle; ?>"
                  title="<?php echo $hideStoryTooltip; ?>"
                  href="#"
                  onclick="if(confirm('<?php echo $hideStoryConfirm; ?>')) window.location='hidestory.php?storyid=<?php echo $storyID; ?>&hidden=<?php echo (1-$story['isHidden']); ?>';"><?php echo $hideStoryCaption; ?></a>
            </span>
            <h2 style="clear: none;">Content summary</h2>
        </div>
        <p>Sort by:
            <?php if($sortOrder=='size') echo '<strong>Size</strong>'; else echo '<a href="?storyid='.$storyID.'&amp;sortorder=size">Size</a>'; ?> |
            <?php if($sortOrder=='time') echo '<strong>Time</strong>'; else echo '<a href="?storyid='.$storyID.'&amp;sortorder=time">Time</a>'; ?> |
            <?php if($sortOrder=='first20') echo '<strong>First 20</strong>'; else echo '<a href="?storyid='.$storyID.'&amp;sortorder=first20">First 20</a>'; ?>
        </p>
        <div class="gui-subpanel story-summary">
            <ol>
<?php
    foreach($story['topTweets'] as $tweet) {
        echo '<li><div class="story-list-item">';
        echo '<div class="info">';
        echo '<div class="sort-field">' . $tweet['count'] . '</div>';
        echo '<div class="sub-field">' . $tweet['firstCreatedAtShort'] . '</div>';
        echo '</div>';
        echo '<div class="content">';
        echo '<div class="story-title">' . addLinksToText($tweet['text']) . '</div>';
        echo '<div class="story-footer">First posted ' . $tweet['firstCreatedAt'] . ' by ' . $tweet['firstRealName'] . ' (<a href="http://twitter.com/#!/' . $tweet['firstScreenName'] . '/status/' . $tweet['firstTweetID'] . '" target="_blank">@' . $tweet['firstScreenName'] . '</a>)';
            echo '<span class="story-footer-right"><a class="actionlink" title="Remove tweets like this from the story" href="confirmstorysplit.php?storyid=' . $storyID . '&amp;tweetclusterid=' . ($tweet['tweetClusterID']) . '">Remove</a></span>';
        echo '</div>';
        echo '</div>';
        echo '</div></li>';
    }
?>
            </ol>
        </div>
    </div>
</div>
<div class="right-column-narrow">
    <?php if (!is_logged_in()) { ?>
    <div class="gui-panel" style="font-weight: bold; color: white; background-color: red;">
      Logging in will enable you to curate stories in CrisisTracker, e.g. by geo-tagging, and merging duplicates. Any changes you make before you log in will be discarded.
    </div>
    <?php } ?>

    <!-- MAP -->
    <div class="gui-panel where-panel">
      <h2>Where</h2>
    	<div id="map_holder" class="where-content">
            <div id="map"></div>
            <div id="mapsearchfield">
                <form>
                    <p>Search location:</p><input type="text" value="" id="searchbox" />
                </form>
            </div>
        </div>
    </div>

    <!-- CATEGORIES and KEYWORDS -->
    <div class="gui-panel what-panel">
        <h2>What</h2>
        <div id="categorybuttons"></div>

        <div id="keywords" class="what-content">
            <form>
                 <input name="tags_keywords" id="tags_keywords" value="" />
            </form>
        </div>
    </div>

    <!-- ENTITIES -->
    <div class="gui-panel who-panel">
        <h2>Who</h2>
        <div id="entities" class="what-content">
            <form>
                 <input name="tags_entities" id="tags_entities" value="" />
            </form>
        </div>
    </div>

    <!-- RELATED STORIES -->
    <div class="gui-panel related-panel">
        <h2>Related Stories</h2>
        <div class="related-content">
            <ol>
<?php
    if (array_key_exists('relatedStories', $story)) {
      foreach($story['relatedStories'] as $relStory) {
          echo '<li>';
          echo '<span class="title"><a href="story.php?storyid=' . $relStory["storyID"] . '" title="' . htmlspecialchars($relStory["title"]) . '">' . htmlspecialchars($relStory["title"]) . '</a></span>';
          echo '<div class="info">First seen: ' . $relStory["startTimeShort"] . ' | Popularity: ' . $relStory["popularity"];
            echo '<span class="story-footer-right"><a class="actionlink" title="Merge the current story with this story" href="confirmstorymerge?storyid1=' . $storyID . '&amp;storyid2=' . $relStory['storyID'] . '">Merge</a></span>';
          echo '</div></li>';
      }
    }
    else {
      echo '<p>Add meta-tags and reload the page to see related stories.</p>';
    }
?>
            </ol>
        </div>
    </div>

    <!-- DUPLICATE STORIES -->
    <div class="gui-panel related-panel">
        <h2>Possible Duplicate Stories</h2>
        <div class="related-content">
            <ol>
<?php
    foreach($story['duplicateStories'] as $dupStory) {
        echo '<li>';
        echo '<span class="title"><a href="story.php?storyid=' . $dupStory["storyID"] . '" title="' . htmlspecialchars($dupStory["title"]) . '">' . htmlspecialchars($dupStory["title"]) . '</a></span>';
        echo '<div class="info">First seen: ' . $dupStory["startTimeShort"] . ' | Popularity: ' . $dupStory["popularity"];
            echo '<span class="story-footer-right"><a class="actionlink" title="Merge the current story with this story" href="confirmstorymerge?storyid1=' . $storyID . '&amp;storyid2=' . $dupStory['storyID'] . '">Merge</a></span>';
        echo '</div></li>';
    }
?>
            </ol>
        </div>
    </div>

    <div class="gui-panel" style="background-color: transparent"><a href="api/get_story.php?storyid=<?php echo $storyID; ?>"><img src="img/xml_icon.png" alt="XML" /></a></div>
</div>

<?php
include('footer.php');

//Log page view
$ip = $_SERVER['REMOTE_ADDR'];
$userID = get_user_id();
if ($userID == NULL)
  $userID = 0;
mysql_query(
  "insert into StoryLog (IP, UserID, Timestamp, EventType, StoryID, StoryAgeInSeconds, TweetCount, RetweetCount, UserCount, TopUserCount, Trend)
  select
      INET_ATON('$ip'), $userID, utc_timestamp(), 20, StoryID,
      unix_timestamp(utc_timestamp())-unix_timestamp(StartTime),
      TweetCount, RetweetCount, UserCount, TopUserCount, Trend
  from Story where StoryID=$storyID;", $db_conn);

include('api/close_db.php');

?>