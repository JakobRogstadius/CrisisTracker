	// ACTIVE STORY MANAGEMENT -----------------------------------------------------------------------------------------
  
  /**
   * Receives a Story generated via XML_PARSER (database), and draws it with GeoTags and TextTags into Local DataStructure
   *
   *@return  boolean
   */    
  function drawStory(story){
    console.log("Drawing Story");
    // console.log(story);
    
    // Geotags reProjection - Official 900913
    var geotags_array = reprojectStoryCoordinatesFrom4326To900913(story.geotags);  // geotags[]	
    story.geotags = geotags_array;
    console.log("Story Geotags Updated");
    console.log(story.geotags);
    
    // Write 'Story' to Actual Context Environment 
    environment.active_story = story;
    
    // Add Geotags (Create Features from Geotags)
    createFeaturesFromActiveStoryGeotags();  // adds to layer and returns        

    // Add Text Tags
    loadStoryTextTags(environment.active_story);
    
    // Add Categories    
	printCategoryButtons(getStoryCategoriesIDs()); 
	setMapExtentToAllGeotags(environment.map, environment.active_story.geotags);	
    return true;
  }
  

    
  /**
   * Save any user made change to the 'Environment Contextual story object' to the WebService DataBase
   */  
	function saveStoryTagChangesDB() {
		console.log("save Story Tag Changes to DB");
		// Load Story
		var storyID = environment.active_story.storyId;
		var userID = 1;  //debug purposes ONLY*

		// Added Geotags
		// console.log(environment.added_geotags_array);
		var added_array = reprojectCoordinatesFrom900913To4326(environment.added_geotags_array);
		// console.log(added_array);

		// Extract LAT/LON
		var addedLocationsLatitude = new Array();
		for (var i=0; i < added_array.length; i++) {
		  addedLocationsLatitude.push(added_array[i].lat);
		  console.log("latitude.." + addedLocationsLatitude[0]);
		}  
		var addedLocationsLongitude = new Array();
		for (var i=0; i < added_array.length; i++) {
		  addedLocationsLongitude.push(added_array[i].lon);
		  console.log("longitude.." + addedLocationsLongitude[0]);
		}
		
		// Removed Geotags
		var removedLocations = new Array();
		for (var i=0; i < environment.removed_geotags_array.length; i++) {
		  removedLocations.push(environment.removed_geotags_array[i].id);
		}
		// /* NEW 
		// var changedLocations = new Array(); 
		// for (var i=0; i < changed_geotags_array.length; i++) { 
		// changedLocations.push(changed_geotags_array[i].id);
		// } 

		// Save to database via AJAX $_GET (xml_parser.js)
		ajaxSaveStoryTagChangesItems(storyID,userID,crisis_tracker.added_categories_array,crisis_tracker.added_entities_array,crisis_tracker.added_keywords_array,crisis_tracker.removed_categories_array,crisis_tracker.removed_entities_array,crisis_tracker.removed_keywords_array,addedLocationsLongitude,addedLocationsLatitude,removedLocations);
	}
  
	/**
	* Removes the full environment.active_story geotags from Vector Layer, Save previous geotag edit Changes to Database and receives callback with update on local environment.active_story full data structure and renders  again geotags to Vector Layer
	*
	*@return  story
	*/      
	function redrawVectorLayer() {
		console.log("Now Redrawing Vector Layer..");	
		removeAllFeaturesFromLayer(); // Clear Vector Layer
		saveStoryTagChangesDB();  
	}
	
	/***
		CallBack function for Internal Update of environment.active_story.Geotags[] with new User Added Geotag
		@param: geotag{id,lon,lat}	
	***/	
	function callbackUpdateGeotags(id,lon,lat) {
		console.log("CALLING BACK UPDATE STORY GEOTAGS");		
		var story_lonlat_coordinates = new OpenLayers.LonLat(lon, lat);
		var coordinates_900913 = reprojectFrom4326To900913(story_lonlat_coordinates); //{LonLat}				
		//var coordinates_900913 = story_lonlat_coordinates;		
		var geotag = {"id":id, "lon":coordinates_900913.lon, "lat":coordinates_900913.lat};
		
		// Save to Active Story
		addUserGeotagtoActiveStory(geotag);
		console.log("Active Story Updated");
		
		// Clear & Fill Vector Layer
		removeAllFeaturesFromLayer();
		createFeaturesFromActiveStoryGeotags();		
		resetTagContainers("added_geotags_array");
		
		setMapExtentToAllGeotags(environment.map, environment.active_story.geotags);
	}
	
	/***
		CallBack function for Internal Update of recently added environment.active_story 'attributes' with database ID's
		@param: typeOfChange{string}, id{int}, name{string}	
	***/		
	function callbackUpdateIds(typeOfChange, id, name) {
		console.log("CALLING BACK UPDATE IDS");
		console.log(typeOfChange);
		console.log(id);
		
		switch(typeOfChange) {
		
		case "keywords":
			environment.active_story.keywords.push({"id":id,"name":name});
			console.log("keywords array updated in ActiveStory");
			console.log(id,name);
			resetTagContainers("added_texttags");
			break;
			
		case "entities":
			environment.active_story.entities.push({"id":id,"name":name});
			console.log("entities array updated in ActiveStory");
			console.log(id,name);
			resetTagContainers("added_texttags");
			break;
			
		case "categories":	
			console.log(id);
			getCategoryNameById(id);
			resetTagContainers("added_texttags");
			// saving to environment.active_story procedures done via callback due to JQuery DOM Access Latency			
			break;	
			
		// ?BROKEN		
		case "geotags":
			// set ID to last added geotag
			console.log("reading geotags");
			console.log(id);		
			break;
		}		
	}
	
	
	/***
		Database Return CallBack function for removing attributes (entities,categories, keywords) from environment.active_story
		@param: typeOfChange{string}
	***/		
	function callbackRemove (typeOfChange) {		
		switch(typeOfChange) {
		
			case "entities":
				var id = crisis_tracker.removed_entities_array[0];
				for(var i=0; i<environment.active_story.entities.length; i++){
					if (environment.active_story.entities[i].id == id) {
						environment.active_story.entities.splice(i,1); 
						console.log("active story updated with removal of entity");
					}
				}
				resetTagContainers("removed_texttags");
			break;
			
			case "categories":
				var id = crisis_tracker.removed_categories_array[0];
				for(var i=0; i<environment.active_story.categories.length; i++){
					if (environment.active_story.categories[i].id == id) {
						environment.active_story.categories.splice(i,1); 
						console.log("active story updated with removal of categories");
					}
				}
				resetTagContainers("removed_texttags");
			break;
			
			case "keywords":
				var id = crisis_tracker.removed_keywords_array[0];
				for(var i=0; i<environment.active_story.keywords.length; i++){
					if (environment.active_story.keywords[i].id == id) {
						environment.active_story.keywords.splice(i,1); 
						console.log("active story updated with removal of keywords");
					}
				}
				resetTagContainers("removed_texttags");
			break;
			
			// Removed Geotags
			case "locations":
				try{
					var id = environment.removed_geotags_array[0].id;	//last geotag
					console.log("REMOVING LOCATION FROM environment.active_story");
					console.log("ID is " + id);
					for(var i=0; i<environment.active_story.geotags.length; i++){
						if (environment.active_story.geotags[i].id == id) {
							environment.active_story.geotags.splice(i,1); 
							console.log("active story updated with removal of Geotags");
						}
					}								
				}
				catch(e) {
					//ErrorHandler.handleError(e);
					console.log("ERROR CATCHED -> removing locations");
				}
				resetTagContainers("removed_geotags_array");
			break;		
		}		
	}
	
	
	 /**
     * Updates Story after environment.active_feature suffered changes (id, lon, lat)
	 *
     *@return  {GeoTag}	 
     */
	function updateStoryGeotagFromActiveFeature() {				
		// Verify if an array contains some object			
			for(var i=0; i<environment.environment.active_story.geotags.length; i++) {					
			    if (environment.active_story.geotags[i].id == environment.active_feature.attributes.id) {
					// Update Positions	
					console.log("found an ID on environment.active_story");
				    environment.active_story.geotags[i].lon = environment.active_feature.geometry.x;
					environment.active_story.geotags[i].lat = environment.active_feature.geometry.y;
			   }
			}	
		console.log("Update environment.active_story geotag with new Coordinates");			
		return getGeoTagFromFeatureById(environment.active_feature,environment.active_story);
	}	
	
	/**
	Update Active Story with added category	  
	**/
	function callbackUpdateActiveStoryAddedCategory(id, name) {
		console.log("CATEGORIES array updated in ActiveStory");
		environment.active_story.categories.push({"id":id,"name":name});	
		console.log("pushed into categories" + id + name );
		console.log(environment.active_story);
	}
	
	/***	
		Clear all Local Data Structure temporary container arrays
	***/	
	function resetTagContainers(identifier) {
		console.log("-------------- RESETING TAG CONTAINERS!!!! --------------");
		switch(identifier) {		
			// Added TextTags
			case "added_texttags":
				crisis_tracker.added_categories_array.length = 0;
				crisis_tracker.added_entities_array.length = 0;
				crisis_tracker.added_keywords_array.length = 0;
			break;
			
			// Removed TextTags
			case "removed_texttags":
				crisis_tracker.removed_categories_array.length = 0;
				crisis_tracker.removed_entities_array.length = 0;
				crisis_tracker.removed_keywords_array.length = 0;	
			break;
			
			// Geotags
			case "removed_geotags_array":
				environment.removed_geotags_array.length = 0;
			break;
			case "added_geotags_array":
				environment.added_geotags_array.length = 0;
			break;	
			case "added_bounds":
				environment.added_bounds.length = 0;
			break;				
		}
	}		
		
	