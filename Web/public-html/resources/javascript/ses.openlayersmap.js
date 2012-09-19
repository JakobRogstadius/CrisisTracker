/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

	 /**
     * Center Map to Bounds
     *
	 * @param: OpenLayers.Control
	 * @return: {Boolean}
     */		
	function centerMapToPosition(lonlat) {
		/*
		var bottom_left_corner = new OpenLayers.LonLat(bounds.lat_lo, bounds.lng_lo);
		var top_right_corner = new OpenLayers.LonLat(bounds.lat_hi, bounds.lng_hi);		
		bottom_left_corner = reprojectFrom4326To900913(bottom_left_corner);
		top_right_corner = reprojectFrom4326To900913(top_right_corner);		
		console.log("updated bottom left " + bottom_left_corner);
		console.log("updated top right " + top_right_corner);			
		// centers environment.map to bounds
		// represents a rectangle in geographical coordinates
		//((30.0302939, -16.96976540000003), (32.7355055, -15.855869699999971))
		console.log("full: " + bottom_left_corner.lon, bottom_left_corner.lat, top_right_corner.lon, top_right_corner.lat);		
		environment.map.zoomToExtent(new OpenLayers.Bounds(bottom_left_corner.lon, bottom_left_corner.lat, top_right_corner.lon, top_right_corner.lat), false);
		*/
		var temp = reprojectFrom4326To900913(lonlat);
		console.log(temp);
		console.log("Centering Map Coordinates to.. " + temp);
		environment.map.setCenter(temp,12); 	//lonlat, zoom, dragging, forceZoomChange
	}
	
	/**
     * Zooms environment.map to provided value
	 *@param_ zoom_level {int}
     * 	 
     */		
	function zoomMap(zoom_level) {
		environment.map.zoomTo(zoom_level);	
	}
	
	
	// MAP CONTROLS
	 /**
     * Deactivates any control
     *
	 * @param: OpenLayers.Control
	 * @return: {Boolean}
     */			
	function deactivateControl(control) {
		return control.deactivate();	// {Boolean}
	}
	
	
	 /**
     * Captures Map Active Bounds
     *
	 * @return: {String[]}
     **/		
	function getMapActiveBounds() {
		// Load Current Bounds (ESPG: 900913)
		var current_bounds = new OpenLayers.Bounds();
		current_bounds = environment.map.calculateBounds();
		drawSquareFromBounds(current_bounds);
		
		// Coordinates Projection Transformation (ESPG: 4326)	 			
		var proj_4326 = new OpenLayers.Projection('EPSG:4326');
		var proj_900913 = new OpenLayers.Projection('EPSG:900913');
		current_bounds.transform(proj_900913,proj_4326);		
		
		// Validate if its within Earth Bounds
		var earthBounds_4326 = new OpenLayers.Bounds(-180, -90, 180, 90);	
		// bottom, left,right, top		
		var value = earthBounds_4326.containsBounds(current_bounds, false, true);		
		if (!value) {
			current_bounds = earthBounds_4326;	//assume full earth	bounds
		}
		
		// Return
		var returnbounds = current_bounds.toArray();	
		return returnbounds;
	}
	
	function setMapActiveBounds(bounds) {		
		console.log("Setting map bounds");
		console.log(bounds);
		environment.added_bounds = bounds.toArray();
		
		//Zoom the map
		var p1 = reprojectFrom4326To900913(new OpenLayers.LonLat(bounds.left, bounds.bottom));
		var p2 = reprojectFrom4326To900913(new OpenLayers.LonLat(bounds.right, bounds.top));
		var bounds2 = new OpenLayers.Bounds(p1.lon, p1.lat, p2.lon, p2.lat);
		console.log(bounds2);
		environment.map.zoomToExtent(bounds2, true);
	}
	
	//mapcontrol: environment.map
	//geotags: environment.active_story.geotags
	function setMapExtentToAllGeotags(mapcontrol, geotags) {
		console.log("setMapExtentToAllGeotags");
		console.log(geotags.length);
		// No Geotags
		if (geotags.length == 0) {
			//Do nothing
		}
		// Cover All Geotags
		else {
			var default_bounds = new OpenLayers.Bounds();
			
			for(var i=0; i<geotags.length; i++) {	
				var temp_geotag = createLonLatFromGeotag(geotags[i]);

				// Extend the bounds to include the lonlat
				default_bounds.extend(temp_geotag);
				if (!default_bounds.containsLonLat(geotags[i]))
					alert('Point not in bounds');
			}
			
			//If there is only one geotag, use zoom level 7
			if (default_bounds.top==default_bounds.bottom && default_bounds.left == default_bounds.right) {
				mapcontrol.zoomToExtent(default_bounds, false);
				zoomMap(Math.min(mapcontrol.zoom,7));
			}
			//If there are several geotags, expand the box and assume that their spread indicates a good zoom level
			else {
				var size = default_bounds.getSize();
				default_bounds.top += 0.3*size.h;
				default_bounds.bottom -= 0.3*size.h;
				default_bounds.left -= 0.3*size.w;
				default_bounds.right += 0.3*size.w;
				mapcontrol.zoomToExtent(default_bounds, true);
			}
		}
	}
	
	
	function createLonLatFromGeotag(geotag) {	
		return new OpenLayers.LonLat(geotag.lon, geotag.lat);	
	}	