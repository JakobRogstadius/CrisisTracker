/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

	/**
     * 	Handle Feature Unselect (click out or other) & Remove PopUp Box on Feature unselect
	 * 	Updates Story Dash HTML
	 *	@param: {OpenLayers.Feature}
     * 
     */	
    function handleOnFeatureUnselect(feature) {	
		console.log("handling Feature Unselect");
		if(feature.popup) {
			environment.map.removePopup(feature.popup);
			feature.popup.destroy();
			feature.popup = null;
			setNoActiveFeature();
			//updateStoryDashBoardHtml();
			//popup_closed = true;	// bug to fix on close popup box
		}
    }		
		
	/**
     * Create HTML PopUp Box on Feature Select and updates active_featured
	 * Adds listeners to the buttoms inside the HTML Pop-up Boxes and Sets environment.active_feature
     * @param: feature 
     */		
    function handleOnClickFeatureSelect(feature) {
		environment.popup_flag = true;
		console.log("Generating popup html..");	
		// Generate HTML for popup
		var html = "<div id='div_button_addremove' style='font-size:.8em'>"
			+"<input type='button' href='#login-box' class='login_activator' id='remove_btn' name='remove_btn' value='Remove GeoTag'/>" 
			+"<input type='button' href='#login-box' class='login_activator' id='move_btn' name='move_btn' value='Move GeoTag'/><br/>" 			
			+"GEOTAG: " + feature.attributes.id + " , STORY: " + feature.attributes.story_id
			+"</div>";	
		
		// Control if environment.active_feature is not in added_array & Set selected as the environment.active_feature
		if (!checkIfFeatureIsInAddedArray(feature)) {	//@param: feature {attributes: id}
			environment.active_feature = feature;
			
			// Popup.FramedCloud @params: {,id, lonlat, contentSize, contentHTML, anchor, closeBox, closeBoxCallback}
			popup = new OpenLayers.Popup.FramedCloud("featurepop", 
				feature.geometry.getBounds().getCenterLonLat(),	// ok
				null,	// contentSize
				html,          
				null, true, handleFeaturePopupClose);	// Close callback function: handleFeaturePopupClose
			popup.panMapIfOutOfView = true;
			feature.popup = popup;
			environment.map.addPopup(popup);
			
			// Add HTML Remove/Move Buttons Click Listeners
			document.getElementById('remove_btn').addEventListener('click', handleRemoveButton, false);
			document.getElementById('move_btn').addEventListener('click', handleMoveButton, false);
					
			// Store Active Geotag Reference
			environment.active_feature = feature;		
			console.log("environment.active_feature updated..");		
			//updateStoryDashBoardHtml();
			//loginComponentControl();	// VALIDATE LOGIN
			
		}
		else {
			console.log("Active Feature was not enable due to some error");			
		}
    }
	
	
	/**
     * Close PopUp (event) - unselect the environment.active_feature 
     * 
     */	
    function handleFeaturePopupClose(event) {			
        select_feature_control.unselect(environment.active_feature);				
		console.log("closing popup.." + environment.active_feature);			
    }

	
	 /**
     * Gets triggered when dragging on a feature starts. 
	 * Controls if its the active feature that's being dragged, cancels dragging if not
     * 	 
     */	
	function handleFeatureStartOfDragging() {	
		// Check if is environment.active_feature
		if (drag_feature_control.feature == environment.active_feature) {		
		console.log("Keep dragging the geotag to your desired position and then release it");
		}
		else {
			console.log("Wrong Feature, please move the ACTIVE FEATURE instead");
			drag_feature_control.cancel();
		}
		environment.initial_lonlat = new OpenLayers.LonLat(environment.active_feature.geometry.x, environment.active_feature.geometry.y);
	}
	
		
	 /**
     * Gets triggered when dragging on a feature ends. Updates new position of environment.active_feature
	 * 1. Gets position of end of drag (feature), 
	 * 2. Converts pixels to LonLat, 
	 * 3. Updates environment.active_feature (selected)
	 * 4. Adds to the changed_geotags_array 
	 * 5. Deactivates drag_feature_control,
	 * 6. Unselects all features
     * 
     */	
	function handleFeatureEndOfDragging() {	
		if (environment.active_feature == null)		{
			// do nothing
			console.log("No Active Feature");
		}
		else {
			// Get end of drag position
			var last_position = drag_feature_control.lastPixel;
			// Alert
			console.log("Drag Complete");						
			console.log(last_position);
			
			// Get Final LonLat Coordinates	
			//environment.final_lonlat = environment.map.getLonLatFromPixel(last_position);	// GLOBAL
			//environment.final_lonlat = getMouseCoordinates();
			environment.final_lonlat = new OpenLayers.LonLat(environment.active_feature.geometry.x, environment.active_feature.geometry.y);
			console.log(environment.active_feature);
			console.log("Final LonLat");
			console.log(environment.final_lonlat);
			// Validate with User
			startCounterValidation(3, 'drag');		
		}
	}

	
	// HTML BUTTONS HANDLERS **********************************************************************************************		
	/***
    * Handles the Cancel Button Click to revert the changes  
	* 	
    */		
	function handleCancelButton() {
		console.log("Now cancelling action..");
		CounterStart = 0; 	// reset counter				
		closeCancelPopup();
		revertChanges(command);
		console.log("Reverting changes..");	
	}

	
	/**
     * Activates Moving Point Control when user wants to drag/move point
     * 	 
     */	
	function handleMoveButton(){
		console.log("Now Moving a Feature");	
		// 1. Close Popup
		environment.map.removePopup(environment.active_feature.popup);			
		// 2. Activate Moving Control	
		console.log("Activate Drag Control: " + drag_feature_control.activate());
	}		
	
	/**
     * Handles Clicks on 'Remove' Button by Removing Popup and Validating via counter
     *
     */	
	function handleRemoveButton(){
		console.log("removing geotag..");	
		// Close Popup
		environment.map.removePopup(environment.active_feature.popup);		
		startCounterValidation(3, 'remove');
	}
	
	
	/**
     * Handles CheckBoxClick
     *
     */
	function handleMapBoundsCheckBoxClick(element) {
		if (element.checked) {
			environment.added_bounds = getMapActiveBounds();	// globalVar set
			saveQueryParamsToDB();
		}
		else {			
			// removeAllFeaturesFromLayer();
			// addedBounds.length=0;
			//removePolygonsFromVectorLayer();
			removeAllGeometryFromVectorLayer("OpenLayers.Geometry.Polygon");
			resetTagContainers("added_bounds");
			saveQueryParamsToDB();
		}
	}

	/**
     * Handles Map Event (Move, Pan)
     *  check if the container of the environment.map bounds should be updated. If the checkbox is checked, and the environment.map as moved or zoomed a new environment.map bounds * calculation is done and the corresponding container is updated.
     */		
	function handleMapEvent(event) {
        thisCheckbox = document.getElementById('mapbounds');
        if (thisCheckbox.checked) {
            removeAllGeometryFromVectorLayer("OpenLayers.Geometry.Polygon");
            environment.added_bounds = getMapActiveBounds();	// draw bounds
            saveQueryParamsToDB();
            //thisCheckbox.checked = false;
        }
	}
	
	
	/**
     * Handles DateSelection via JQuery <div id=from> <div id =to>
     *
     */	
	function handleDateSelection(date, instance) {
    console.log("before: " + environment.addedMinDate);

		switch(instance) {
			case "from":
				environment.addedMinDate = date;
			  break;
			case "to":
				environment.addedMaxDate = date;
			  break;		
		}

    console.log("after: " + environment.addedMinDate);
  }
	
	/**
	  * Show Loading Animation
	  */		
	function showLoadingAnimation() {
		$('#loading').show();	
	}
	
	
	/**
    * Hide Loading Animation
    */		
	function hideLoadingAnimation() {
		$('#loading').hide();
	}
	
	
	/**
    * ExploreStories.php
    */			
	function handleSearchButtonClick() {
		saveQueryParamsToDB();
	}
	
	/**
     * Priority Click
     */		
	function handlePriorityClick(command) {
		console.log(command);
		switchPriority(command);
	}
	