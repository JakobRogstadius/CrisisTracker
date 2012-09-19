/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

	// 	FEATURES MANAGEMENT (VECTOR_LAYER.FEATURES)

	// VECTOR LAYER --------------------------------------------------
	/**
		Add a new Feature to Vector_Layer from environment.added_geotags_array
	**/	
	function addDisplayNewFeature() {
		console.log("addDisplayNewFeature");	
		var features_array = createFeaturesFromGeotagsArray(environment.added_geotags_array);
		environment.vector_layer.addFeatures(features_array);				
		startCounterValidation(4, 'add');	//start and show counter validation & await for user command
	}
	
	
	/**
		Add a new Feature to Vector_Layer from environment.added_geotags_array
	**/		
	function displayNewFeature(geotags_container) {
	
		// // Style
		// var markerStyle = {fillOpacity: 0.1, strokeWidth: 10};
		
		// // Create Feature
		// var feature = new OpenLayers.Feature.Vector(polygon, {title: "hello", clickable: 'off'}, markerStyle);	
	
		console.log("displayNewFeature");	
		var features_array = createFeaturesFromSimpleGeotagsArray(geotags_container);
		environment.vector_layer.addFeatures(features_array);
	}
	
	
	/**
     * Destroy a Feature from environment.vector_layer
     *
	 *@param:feature{OpenLayers.Vector.Feature}
     */			
	function destroyFeature(feature) {		
		environment.vector_layer.destroyFeatures(feature);
	}		
			

	/**
		Creates features from a Story Geotags (attributes extraction algorithm) and adds the feature to the environment.vector_layer	
	**/		
	function createFeaturesFromActiveStoryGeotags() {
		console.log("createFeaturesFromActiveStoryGeotags");	
		var temp_features_array = new Array();		
		console.log("ActiveStory to extract has "+ environment.active_story.geotags.length);		
		
		// Scan all GeoTags in Array
		for(var i=0; i<environment.active_story.geotags.length; i++){
		console.log("Creating point.." + environment.active_story.geotags[i].lon + environment.active_story.geotags[i].lat);	
			temp_features_array.push(new OpenLayers.Feature.Vector( 		
				new OpenLayers.Geometry.Point(environment.active_story.geotags[i].lon,environment.active_story.geotags[i].lat),
				{
					// Set Attributes 
					//'timestamp_creation': '2012.1.3.10042',
					//'timestamp_update': generateTimeStamp(),
					//'user':story.information.user			
					//'type':'fire'					
					'id':environment.active_story.geotags[i].id,
					'story_id': environment.active_story.storyId,													
					'user':getActiveUser()
				}
			))
		};
		environment.vector_layer.addFeatures(temp_features_array);	
		console.log("TEMP FEATURES ARRAY");
		console.log(temp_features_array);		
		return temp_features_array;
	}
	
	
	/**
     * Creates features from Geotags Array that belongs to a Story(attributes extraction algorithm) and adds the feature to the environment.vector_layer
	 *
     * @param geotags_array {OpenLayers.LonLat, int}
     * @return features_array {id, story_id, user}
     */		
	function createFeaturesFromGeotagsArray(geotags_array) {
		console.log("Creating Features from Geotag array..");			
		console.log("Received geotag array to extract has "+ geotags_array.length);		
		console.log(geotags_array);
		
		var temp_features_array = new Array();
		
		// Scan all GeoTags in Array
		for(var i=0; i<geotags_array.length; i++){		
		console.log("Creating new point.." + geotags_array[i].lon + geotags_array[i].lat);	
			temp_features_array.push(new OpenLayers.Feature.Vector( 		
				new OpenLayers.Geometry.Point(geotags_array[i].lon,geotags_array[i].lat),
				{
					// Set Attributes 
					//'timestamp_creation': '2012.1.3.10042',
					//'timestamp_update': generateTimeStamp(),
					//'type':environment.active_story.types[0],	
					'id':geotags_array[i].id,				
					'story_id': environment.active_story.storyId,
					'user':getActiveUser()
				}
			))
		};		
		return temp_features_array;
	}
	

	/**
     * Creates features from Geotags Array (attributes extraction algorithm) and adds the feature to the environment.vector_layer
	 *
     * @param geotags_array {OpenLayers.LonLat, int}
     * @return features_array {id, story_id, user}
     */		
	function createFeaturesFromSimpleGeotagsArray(geotags_array) {
		console.log("Creating Features from Geotag array..");			
		console.log("Received geotag array to extract has "+ geotags_array.length);		
		console.log(geotags_array);
		
		var temp_features_array = new Array();
		
		// Scan all GeoTags in Array
		for(var i=0; i<geotags_array.length; i++){		
		//console.log("Creating new point.." + geotags_array[i].lon + geotags_array[i].lat);	
			temp_features_array.push(new OpenLayers.Feature.Vector( 		
				new OpenLayers.Geometry.Point(geotags_array[i].lon,geotags_array[i].lat),
				{
					// Set Attributes 
					//'timestamp_creation': '2012.1.3.10042',
					//'timestamp_update': generateTimeStamp(),
					//'type':environment.active_story.types[0],	
					'id':geotags_array[i].id
				}
			))
		};		
		return temp_features_array;
	}
	
	
	/**
     * Gets a Geotag From a Feature via Id Lookup
     *
	 *@param: environment.active_feature{OpenLayers.Vector.Feature}, environment.active_story {Story}
	 *@return geotag
     */	
	function getGeoTagFromFeatureById() {
		// Verify if its in Active Story.geotags[]	
		for(var i=0; i<environment.active_story.geotags.length; i++) {		
		   if (environment.active_story.geotags[i].id == environment.active_feature.attributes.id) {
				console.log("Found Geotag on environment.active_story and returning.. " + environment.active_story.geotags[i].id);
			   return environment.active_story.geotags[i];
		   }
		}
		// Verify if its in environment.added_geotags_array[]
		for(var i=0; i<environment.added_geotags_array.length; i++) {		
		   if (environment.added_geotags_array[i].id == environment.active_feature.attributes.id) {
				console.log("Found Geotag on environment.added_geotags_array and returning.. " + environment.added_geotags_array[i].id);
			   return environment.added_geotags_array[i];
		   }
		}
	alert("Something wrong... couldn't get geotag from the Feature by Id");	
	return -1;	// not found
	}
	
	/**
     * Sets no Active Feature (null)
     *
     */
	function setNoActiveFeature() {
		environment.active_feature = null;
	}
	
	/**
	* Unselects All Features and updates 'story dashboard'
	* 	 
    */		
	function unselectAllFeatures() {
		select_feature_control.unselectAll({});
		environment.active_feature = null;
		//updateStoryDashBoardHtml();
	}
	
	/**
	* Removes Polygon Features
	* 	 
    */			
	function removeAllGeometryFromVectorLayer(geometry_type) {
		// OpenLayers.Geometry.Polygon, OpenLayers.Geometry.Point
		//console.log("removing geometry..");
		var total = environment.vector_layer.features.length;
		for(var i=environment.vector_layer.features.length-1; i>=0; i--) {
			if (environment.vector_layer.features[i].geometry.CLASS_NAME == geometry_type){
				removeFeatureIndexFromLayer(i);
				//console.log("removing geometry.." + i);	
			}
		}	
	}
	
	 /**
     * Remove Last added feature from vector layer (common function)
	 *
     *@param index{int}	 
     */			
	function removeLastFeatureFromVectorLayer() {		
		// console.log("Removing last feature from Vector Layer");
		removeFeatureIndexFromLayer(environment.vector_layer.features.length-1);
	}

	 /**
     * Remove a Feature from Vector Layer via Index
	 *
     *@param index{int}	 
     */		
	function removeFeatureIndexFromLayer(index) {
		// console.log("removing feature from Index " + index);
		var temp = environment.vector_layer.features;
		//environment.vector_layer.removeFeatures(temp[index]);
		environment.vector_layer.destroyFeatures(temp[index]);
	}

	 /**
     * Remove all Features from Vector Layer
     * 	
	 * @param supresser {String}
     */		
	function removeAllFeaturesFromLayer() {
		console.log("Removing all Features from Vector Layer..");	
		environment.vector_layer.removeAllFeatures({silent:true});	// supress events
	}
	
	 /**
     * Updates environment.active_feature Lon/Lat Geometry values and adds a new timestamp
     * 	 
     */	
	function updateActiveFeatureGeometry(lonlat) {
		environment.active_feature.geometry.x = lonlat.lon;	
		environment.active_feature.geometry.y = lonlat.lat;
		environment.active_feature.attributes.timestamp_update = generateTimeStamp();
		console.log("Active Feature Geometry Updated!");	
	}
	
	
	// FEATURE EDITING COMMANDS ------------------------------------------------------------------------------------------
	 /**
     * Called from Counter after Dragging a feature was sucessfull (after user counter validation). 
	 * Deletes the Old Geotag and Adds a new Geotag
     * 	
	 * @param lonlat {OpenLayers.LonLat}
     */			
	function dragSucessfull(lonlat) {
		// Send both together Deleted and New Geotag togheter
		
		// 1. DELETE
		console.log("Removing Old Geotag");
		console.log(environment.active_feature);
		environment.removed_geotags_array.push(getGeoTagFromFeatureById());		

		// 2. ADD NEW
		console.log("Adding New Geotag");
		console.log("New coordinates are: ")
		console.log(lonlat);		
		
		// Generate New Geotag
		// updateActiveFeatureGeometry(lonlat);	//updates coordinates of environment.active_feature			
		geotag = generateTemporaryGeotagFromLonlat(lonlat);
		environment.added_geotags_array.push(geotag);
		//environment.added_geotags_array.push(getGeoTagFromFeatureById(environment.active_feature, environment.active_story));				
		
		// Deactivate environment.active_feature Controls
		deactivateControl(drag_feature_control);			
		unselectAllFeatures();

		// Call Big Query with Removed & Add to WebService	
		saveStoryTagChangesDB();
		callbackRemove("locations"); // Forced
	}
	
	 /**
     * Called when Dragging a feature was unsucessfull (forced via user counter validation)
     * 	
     */		
	function dragUnSucessfull() {		
		console.log("DRAG UNSUCESSFULL");	
		resetTagContainers("added_geotags_array");	// safety measure to avoid features created by non intentional click
		deactivateControl(drag_feature_control);
		
		// move feature to its original position
		environment.active_feature.move(environment.initial_lonlat);
		unselectAllFeatures();
	}
	
	 /**
     * Called when Removing a feature was unsucessfull (forced via user counter validation)
     * 	
     */			
	function removeUnSucessfull() {
		console.log("REMOVE UNSUCESSFULL");
		resetTagContainers("removed_geotags_array");	// safety measure to avoid features created by non intentional click
		unselectAllFeatures();		
	}
	
	/**
     * Remove feature from Vector Layer, Geotag and update it on database and callback to remove on active story
     *
     */		
	function removeSucessfull() {
		// Remove it from environment.vector_layer
		destroyFeature(environment.active_feature);
		
		// Add it to environment.removed_geotags_array
		environment.removed_geotags_array.push(getGeoTagFromFeatureById());
		console.log("Now removing " + environment.removed_geotags_array.length + " features " + environment.removed_geotags_array[0]);
		
		// Save Changes to Database
		saveStoryTagChangesDB();
			
		// Set environment.active_feature to null
		setNoActiveFeature();
		
		// Update Dashboard
		//updateStoryDashBoardHtml();
	}
	
	/**
	*	Add Sucessfull (Countdown timer)	
	**/	
	function addSucessfull() {
		console.log("ADD SUCESSFULL");	
		saveStoryTagChangesDB();	
		console.log("Saving to Database");
	}	
	
	/**
    * Remove last feature from vector layer
	*
    *@param index{int}	 
    */		
	function addUnSucessfull() {
		console.log("ADD UNSUCESSFULL");	
		resetTagContainers("added_geotags_array");	// safety measure to avoid features created by non intentional click		
		removeLastFeatureFromVectorLayer();
	}	
	
		/**
     * checkIfFeatureIsInAddedArray
     * @return {boolean}
     */		
	function checkIfFeatureIsInAddedArray(feature) {
		for(var i=0; i<environment.added_geotags_array.length; i++) {		
		   if (environment.added_geotags_array[i].id == feature.attributes.id) {
			 console.log("Found Geotag and returning.. " + environment.added_geotags_array[i].id);
			   return true;
		   }
		}
		return false;
	}
		
	/**
	* Create a new polygon geometry based on this bounds.
	* draw an orange square over the environment.map vector layer to inform the user that the environment.map bounds are selected.
	**/
	function drawSquareFromBounds(bounds) {		
		
		// Create Polygon
		var polygon = bounds.toGeometry();	// OpenLayers.Geometry.Polygon
		console.log("Polygon has been created");
		console.log(polygon);
				
		var markerStyle = {fillOpacity: 0.05, strokeWidth: 0};
		
		// Create Feature
		var feature = new OpenLayers.Feature.Vector(polygon, {title: "selection", clickable: 'off'}, markerStyle);
		var features_array = new Array();
		features_array.push(feature);
		
		// console.log("features array");
		// console.log(features_array);
		
		// Add Feature to Vector Layer
		environment.vector_layer.addFeatures(features_array);
	}		