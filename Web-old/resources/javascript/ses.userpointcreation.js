/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

  // USER POINT CREATION    
  /**
     * Regist event for Click(Event) on the Map
     * 
     */     
  function registerMouseClickEvent() {  
    console.log("Reading mouse clicks.. Now, Click on a Position to Map");  
    environment.map.events.register('click', environment.map, addUserPoint);  // Register an event on the 'environment.map.events' object.
  }
  
  /**
     * Temporarily disable a mouse click event
     * 
     */     
  function disableMouseClickEvent() {
    environment.map.events.unregister('click', environment.map, addUserPoint);
  }
  
  /**
     * Adds a user Point to the Story - controls fix while drag_feature_control is enabled
     */   
  function addUserPoint(){
    console.log("Add User Point");
    
    // Check Authentication
    if(!environment.popup_flag){           
      lonlat = getMouseCoordinates();
      console.log("user coordinates" + lonlat.lon + " , " + lonlat.lat);                
      environment.map.setCenter(lonlat);  //lonlat, zoom, dragging, forceZoomChange
      console.log("centering environment.map..");     
              
      // Generate a StoryGeoTag Array
      geotag = generateTemporaryGeotagFromLonlat(lonlat); //@ return [geotags{id, lon, lat}
      console.log("We've got a new LonLat");
      console.log(geotag);
      
      // Add the New GeoTag to 'environment.added_geotags_array'
      environment.added_geotags_array.push(geotag);   
      console.log("environment.added_geotags_array updated with new geotag");               
            
      // Display temporary geotag on environment.map and wait for user command / timercountdown
      addDisplayNewFeature(); 
    }
    
    // Switch Hack
    if (environment.popup_flag == true) {
      environment.popup_flag = false;
    }   
  }

  /**
     * Get MouseCoordinates
     *
   * @return: {OpenLayers.LonLat} translated into lon/lat by the base layer EPSG:900913  
     */   
  function getMouseCoordinates() {  
      var lonlat = environment.map.getLonLatFromPixel(mousePosition.lastXy);  // LonLat     
      return lonlat;
  } 
  
    /**
     * Update environment.active_story with a user inserted geotag
     *
     * @param geotag {id, lon, lat, last_update}
     */   
  function addUserGeotagtoActiveStory(geotag) {
    environment.active_story.geotags.push(geotag);
  }
  
  // NOT USED YET * NOT USED YET  * NOT USED YET  * NOT USED YET  * NOT USED YET 
   // /**
     // * Activate handlers: Before Feature Insertion and After Feature Insertion
     // * 
     // */  
  // function activateBeforeAfterFeatureHandlers() {
    // handleBeforeFeatureInsertion();
    // handleAfterFeatureInsertion();
  // }
  
   // /**
     // * Handle Before a Feature Insertion into Vector Layer
     // * 
   // * @return: 'feature.id'
     // */    
  // function handleBeforeFeatureInsertion() {  
    // var id;
    // vector_layer.preFeatureInsert = function(feature){ 
      // console.log('preFeatureInsert – ID: ' + feature.id) 
      // id = feature.id;
    // };
    // return id;
  // }
  
   // /**
     // * Handle After a Feature Insertion into Vector Layer
     // * 
   // * @return: 'feature.id'
     // */  
  // function handleAfterFeatureInsertion() {
    // var id;
    // //var geometry;  geometry.x, geometry.y
    // vector_layer.onFeatureInsert = function(feature){ 
    // console.log('onFeatureInsert - ID:'  + feature.id) // feature.geometry
      // id = feature.id;
    // };       
    // return id;
  // }
  
  /**
     * Get active_user Global Variable
     *
     * @return user {string}
     */ 
  function getActiveUser() {
    return session_global;
  } 
  
   /**
     * Adds a temporary ID to a LonLat point and returns a geotag
   *
     * @used by: ses.activestorymanagent.js
     * @param lonlat {OpenLayers.LonLat}
     * @return geotag
     */ 
  function generateTemporaryGeotagFromLonlat(lonlat,id) {
    var geotag;
    if(id){ 
      geotag = ({'id' : id, 'lon':lonlat.lon, 'lat':lonlat.lat});
    }
    else {
      geotag = ({'id' : 0, 'lon':lonlat.lon, 'lat':lonlat.lat});
    }
    return geotag;
  }