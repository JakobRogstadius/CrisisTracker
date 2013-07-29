/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

	 /**
     * Transform coordinates from 4326 to 900913 EPSG
     *
     * @param lonlat {OpenLayers.LonLat} *in EPSG:4326
     * @return projection {OpenLayers.Projection}
     */	
	function reprojectFrom4326To900913(lonlat4326) {
		// console.log("Transforming Coordinates..");	
		// console.log("Coordinates before 4326 LON: "+ lonlat4326.lon + " , LAT " + lonlat4326.lat);
	
		// Round Decimal to 6
		//lonlat4326.lon=Math.round(lonlat4326.lon*1000000)/1000000;
		//lonlat4326.lat=Math.round(lonlat4326.lat*1000000)/1000000;
	
		// Projections
		var proj_4326 = new OpenLayers.Projection('EPSG:4326');
		var proj_900913 = new OpenLayers.Projection('EPSG:900913');
		lonlat4326.transform(proj_4326, proj_900913);
		//console.log("Coordinates after 900913 LON: "+ lonlat4326.lon + " , LAT: " + lonlat4326.lat);
		return lonlat4326; //{OpenLayers.LonLat}
	}
	
	
	 /**
     * Transform coordinates from 900913 to 4326 EPSG
     *
     * @param lonlat {OpenLayers.LonLat} *in EPSG:4326
     * @return projection {OpenLayers.Projection}
     */	
	function reprojectFrom900913To4326(lonlat900913) {
		// console.log("Transforming Coordinates..");	
		// console.log("Coordinates before 90013 LON: "+ lonlat900913.lon + " , LAT: " + lonlat900913.lat);
		
		// Round Decimal to 6
		//lonlat900913.lon=Math.round(lonlat900913.lon*1000000)/1000000;
		//lonlat900913.lat=Math.round(lonlat900913.lat*1000000)/1000000;
		
		// Projections
		var proj_4326 = new OpenLayers.Projection('EPSG:4326');
		var proj_900913 = new OpenLayers.Projection('EPSG:900913');
		lonlat900913.transform(proj_900913,proj_4326);			
		//console.log("Coordinates after 4326 LON: "+ lonlat900913.lon + " , LAT: " + lonlat900913.lat);
		return lonlat900913; //{OpenLayers.LonLat}
	}

    /**
     * Transform the coordinates from a Loaded Story from DataBase - > to Spherical Mercador
     *
     * @param geotags_array {id,lon,lat}
     * @return geotags_array {id,lon,lat}	// CORRECTED COORDINATES EPSG:900913
     */	
	function reprojectStoryCoordinatesFrom4326To900913(geotags_array) {		
		console.log("transforming story geotags.. " + geotags_array.length);
		for(var i=0; i<geotags_array.length; i++){		
			// Load EPSG4326 LonLat coordinates
			var story_lonlat_coordinates = new OpenLayers.LonLat(geotags_array[i].lon, geotags_array[i].lat);
			// Transform from EPSG4326 (lon,lat) to EPSG900913 (x,y)
			var coordinates_900913 = reprojectFrom4326To900913(story_lonlat_coordinates); //{LonLat}
			// Update with new EPSG900913 coordinates
			geotags_array[i].lon = coordinates_900913.lon;			
			geotags_array[i].lat = coordinates_900913.lat;
		}						
		return geotags_array;
	}
	
	
	 /**
     * Transform coordinates from 900913 EPSG to 4326
     *
     * @param geotags_array {OpenLayers.LonLat} *in EPSG:900913
     * @return geotags_array {OpenLayers.LonLat} *in EPSG:4326
     */		
	function reprojectCoordinatesFrom900913To4326(geotags_array) {		
		// Transform GeoTags Coordinates to EPSG:900913
		console.log("transforming geotags.. " + geotags_array.length);
		for(var i=0; i<geotags_array.length; i++){		
			// Load EPSG4326 LonLat coordinates
			var story_lonlat_coordinates = new OpenLayers.LonLat(geotags_array[i].lon, geotags_array[i].lat);
			// Transform from PSG900913 (x,y) to EPSG4326 (lon,lat)
			var coordinates_4326 = reprojectFrom900913To4326(story_lonlat_coordinates); //{LonLat}
			// Update with new EPSG4326 coordinates
			geotags_array[i].lon = coordinates_4326.lon;
			//geotags_array[i].lat = coordinates_900913.lat;
			geotags_array[i].lat = coordinates_4326.lat;
		}						
		return geotags_array;
	}