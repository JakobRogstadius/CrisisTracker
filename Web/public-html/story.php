<?php
include('api/was_story_merged.php');
include('api/get_story_content.php');
include_once('api/common_functions.php');

$sortOrder = 'size';
if (isset($_GET['sortorder'])) {
  $sortOrder = stripMaliciousSql($_GET['sortorder']);
  if ($sortOrder != 'time' && $sortOrder != 'size' && $sortOrder != 'first50') {
    $sortOrder = 'size';
  }
}

$storyID = 0;
if (isset($_GET['storyid'])) {
  $storyID = intval($_GET['storyid']);
}

include('api/open_db.php');

//Redirect if this story was merged with another
$mergedWith = was_story_merged($storyID, $db_conn);
if (!is_null($mergedWith))
    header("Location: story.php?storyid=$mergedWith");

//Fetch story content
$story = get_story_content($storyID, $sortOrder, $db_conn);

include('api/close_db.php');

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
    //ajaxGetItems(2);	// xmlParser HTTP Request to WebService
	ajaxGetItems(storyId);
  }        
</script>

<?php include('header_end.php'); ?>

<div class="left-column-wide">
    <div class="gui-panel story-panel">
        <h1><?php echo addLinksToText($story['title']); ?></h1>
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
                  href="hidestory.php?storyid=<?php echo $storyID; ?>"
                  onclick="if(!confirm('<?php echo $hideStoryConfirm; ?>')) return false;"><?php echo $hideStoryCaption; ?></a>
            </span>        
            <h2 style="clear: none;">Content summary</h2>
        </div>
        <p>Sort by:
            <?php if($sortOrder=='size') echo '<strong>Size</strong>'; else echo '<a href="?storyid='.$storyID.'&amp;sortorder=size">Size</a>'; ?> |
            <?php if($sortOrder=='time') echo '<strong>Time</strong>'; else echo '<a href="?storyid='.$storyID.'&amp;sortorder=time">Time</a>'; ?> |
            <?php if($sortOrder=='first50') echo '<strong>First 50</strong>'; else echo '<a href="?storyid='.$storyID.'&amp;sortorder=first50">First 50</a>'; ?>
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
    foreach($story['relatedStories'] as $relStory) {
        echo '<li>';
        echo '<span class="title"><a href="story.php?storyid=' . $relStory["storyID"] . '" title="' . htmlspecialchars($relStory["title"]) . '">' . htmlspecialchars($relStory["title"]) . '</a></span>';
        echo '<div class="info">First seen: ' . $relStory["startTimeShort"] . ' | Popularity: ' . $relStory["popularity"];
            echo '<span class="story-footer-right"><a class="actionlink" title="Merge the current story with this story" href="confirmstorymerge?storyid1=' . $storyID . '&amp;storyid2=' . $relStory['storyID'] . '">Merge</a></span>';
        echo '</div></li>';
    }
?>
            </ol>
        </div>
    </div>
</div>

<?php
include('footer.php');
?>