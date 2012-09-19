/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

// ---------------------------------------------------------------------------------------
/***
	Query Text Box for "Map Location"	
***/
function userSearchMapLocation() {
	// Google Geocoder => Geocoding Service (converting string addresses into geographic coordinates )=> remote data source.
	var geocoder = new google.maps.Geocoder();  

	// TEXTBOX INPUT
	$(function() {
		// Bind the '.autocomplete' event => search input box,
		/* jQuery Autocomplete		
			'term' -  value currently in the text input
			responseCallBack - expects a single argument to contain the data to suggest to the user
		*/
		$("#searchbox").autocomplete({	
			autoFocus: true,
			source: function(requestCallBack, responseCallBack) { // @param: source: {Callback}
		
				// Safety Measure
				if (geocoder == null){
					geocoder = new google.maps.Geocoder();
				}
		
				/* First -> request to the geocoding service (input terms + callback function to process results)
					inputTerms = GeocodeRequest  {address: string,latLng: LatLng, bounds: LatLngBounds, bounds: LatLngBounds}				
				*/				
				geocoder.geocode({'address': requestCallBack.term }, function(results, status) {
					if (status == google.maps.GeocoderStatus.OK) {
						
						// ..using the returning location {string}
						var searchLoc = results[0].geometry.location;
						
						var lat = results[0].geometry.location.lat();
						var lng = results[0].geometry.location.lng();
						
						var latlng = new google.maps.LatLng(lat, lng);	// Google Maps 'LatLng' object => list of search results with reverse geocoding
						
						var bounds = results[0].geometry.bounds;
						
						// Second Request with Coordinates
						geocoder.geocode({'latLng': latlng}, function(results1, status1) {
							if (status1 == google.maps.GeocoderStatus.OK) {			
								if (results1[1]) {
									responseCallBack($.map(results1, function(loc) {
                    return {																						
											label  : loc.formatted_address,
											value  : loc.formatted_address,
											lonlat : loc.geometry.location
										}
									}));
								}
							}
						});
					}
				});
			},	//end of 'source:'
			
			focus: function(event, ui) {  },
			
			// user selects an 'option' from search results
			select: function(event,ui){
				var pos = ui.item.position;	
					//alert("position" + pos);
					console.log(ui.item);
				var lct = ui.item.locType;
					//alert("lct" + lct);
				var lonlat = ui.item.lonlat;
		
				if (lonlat){
					// lat_lo,lng_lo,lat_hi,lng_hi
					var lonlat2 = new OpenLayers.LonLat(lonlat.lng(), lonlat.lat());
					console.log('LONLAT AFTER: ' + lonlat2);
					centerMapToPosition(lonlat2,10);
				}
			}												
		});
		
		// 'ENTER' Bug Solving (jQuery)
		$("#searchbox").keypress(function(event){
		  if(event.keyCode == 13){				  
			  event.preventDefault();
		  }
		});
	});   	
}