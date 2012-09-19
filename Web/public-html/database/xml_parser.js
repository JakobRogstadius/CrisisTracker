/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

/* This file uses 'Sarissa Ajax Library' to dynamically return XML data
1. Sarissa It’s small (11.8kb!)
2. Sarissa is Cross browser compatible!
3. Sarissa Escapes the information received nicely
*/
	var data;
	var typeOfChange='';

	/**  
	* Initate the http request for data.
	**/
	function getUrl(url,fn) { // URL of data and FUNCTION that will get called to handle the data (after returned)
	  if (url && fn) {
		var xmlhttp = new XMLHttpRequest();
		xmlhttp.open("GET", url, true);
		xmlhttp.onreadystatechange = function() {
		  if (xmlhttp.readyState == 4) {
			fn(xmlhttp.responseXML,typeOfChange);
		  }
		};
		xmlhttp.send('');
	  } else {
		alert('url or function not specified!');
	  }
	}

	/**  Trim whitespace from strings
	*
	**/	
	String.prototype.trim=function(){ 
	  return this.replace(/^\s*|\s*$/g,'');
	}
	String.prototype.ltrim=function(){
	  return this.replace(/^\s*/g,'');
	}
	String.prototype.rtrim=function(){
	  return this.replace(/\s*$/g,'');
	}

	/**  Checks the XML data you've return for <error_code>x</error_code>
	*  and returns the error to the user if found.
	*  XML document contains a valid error_code tag with error number (0 = no error)
	**/ 
	function checkResultIsOk(xml) {
	  if (!xml) {
		//xml data was empty. Result was not ok.
		//alert("WARNING!!!");
		console.log("xml data was empty. Result was not ok.");
		return false;		
	  }
	  /*
	  if (xml.getElementsByTagName('error_code')[0].firstChild.data && 
		Math.abs(xml.getElementsByTagName('error_code')[0].firstChild.data.trim()) > 0) {
		//error has been supplied
		alert('Error ' + xml.getElementsByTagName('error_code')[0].firstChild.data + ': ' +
		  xml.getElementsByTagName('error')[0].firstChild.data.trim());
		return false;
	  }
	  */
	  else {
		//no errors!
		return true;
	  }
	}
	
	
	/**  
	* Called from clicking a link or button on the page to request data to the WEBSERVICE
	* Constroys the URL to be called and calls it & handle the response
	**/
	function ajaxGetItems(storyId) {
	  console.log("Getting Items");	
	  //send the request    
	  // var filetouse = 'http://ufn.virtues.fi/~jakob/twitter/get_story.php';	// GET variables in request
	  // var filetouse = 'database/get_story_simulator.php';
	  var url_sufix = 'storyid='+storyId+'&onlytags=0';
	  var filetouse = 'database/get_story_cache.php?'+url_sufix;
	  getUrl(filetouse,httpResponseHandlerStoryLoad);
	}
	
	
	/**  
	* Calls the WebService to "get_stories.php" with changes via GET (Url)
	**/		
	function ajaxSendQuery(categoryfilter,entityfilter,keywordfilter,sortorder,limit,minstarttime,maxstarttime,locationfilter,hidearabic){	
		console.log("Preparing to send via AJAX");
		var url_sufix='';
		
		// ************* Values *************
		// Sort Order
		if(typeof(sortorder) != 'undefined'){			
			url_sufix+='sortorder='+sortorder;
		}
		// limit
		if(typeof(limit) != 'undefined'){			
			url_sufix+='&limit='+limit;
		}
		// minstarttime
		if(typeof(minstarttime) != 'undefined' && minstarttime != null){			
			url_sufix+='&minstarttime='+minstarttime;
		}
		// minstarttime
		if(typeof(maxstarttime) != 'undefined' && maxstarttime != null){			
			url_sufix+='&maxstarttime='+maxstarttime;
		}
    if (hidearabic===true)
  		url_sufix+='&hidearabic='+hidearabic;
		
		// ************* Arrays *************
		// location Filter - DO NOT SEND AS ARRAY
		if(typeof(locationfilter) != 'undefined'){
			if(locationfilter.length > 0) {								
				url_sufix+='&locationfilter='+locationfilter;				
			}
		}
		
		// Category Filter
		if(typeof(categoryfilter) != 'undefined'){	
			for (var i in categoryfilter){							
				url_sufix+='&categoryfilter[]='+categoryfilter[i];				
			}
		}
		
		
		// Entity Filter
		if(typeof(entityfilter) != 'undefined'){	
			for (var i in entityfilter){							
				url_sufix+='&entityfilter[]='+encodeURI(entityfilter[i]);				
			}
		}	
		
		// Keyword Filter
		if(typeof(keywordfilter) != 'undefined'){		
			for (var i in keywordfilter){									
				url_sufix+='&keywordfilter[]='+encodeURI(keywordfilter[i]);				
			}
		}
		
		
		// ************* CROSS DOMAIN BYPASS URL *************
		var filetouse = 'database/get_stories_cache.php?'+url_sufix;
		console.log(filetouse);
    console.log('hide arabic is: ' + hidearabic);
		getUrl(filetouse,httpResponseHandlerExploreStories);
	}
	
		
	/**  
	* Calls the WebService to "save_story_tag_changes.php" with changes via GET (Url)
	**/	
	function ajaxSaveStoryTagChangesItems(storyID,addedCategories,addedEntities,addedKeywords,removedCategories,removedEntities,removedKeywords,addedLocationsLongitude,addedLocationsLatitude,removedLocations) {
	  console.log("ajaxSaveStoryTagChangesItems");	
			  
		var url_sufix='';
		
		// StoryID
		if(typeof(storyID) != 'undefined'){		
			url_sufix+='storyid='+storyID;
		}
		// addedCategories
		if(typeof(addedCategories) != 'undefined'){
			if(addedCategories.length>0) {
				url_sufix+='&addedcategories[]='+addedCategories;
				typeOfChange = "addedCategories";
			}
		}
		// addedEntities
		if(typeof(addedEntities) != 'undefined'){	
			if(addedEntities.length>0) {
				url_sufix+='&addedentities[]='+encodeURI(addedEntities);
				typeOfChange = "addedEntities";;				
			}
		}
		// addedKeywords
		if(typeof(addedKeywords) != 'undefined'){
			for (var i in addedKeywords){
				url_sufix+='&addedkeywords[]='+encodeURI(addedKeywords);
				typeOfChange = "addedKeywords";
			}
		}
		// removedCategories
		if(typeof(removedCategories) != 'undefined'){
			for (var i in removedCategories){
				url_sufix+='&removedcategories[]='+removedCategories;
				typeOfChange = "removedCategories";
			}
		}
		// removedEntities
		if(typeof(removedEntities) != 'undefined'){
			for (var i in removedEntities){
				url_sufix+='&removedentities[]='+encodeURI(removedEntities);
				typeOfChange = "removedEntities";
			}
		}
		// removedKeywords
		if(typeof(removedKeywords) != 'undefined'){	
			for (var i in removedKeywords){
				url_sufix+='&removedkeywords[]='+encodeURI(removedKeywords);
				typeOfChange = "removedKeywords";
			}
		}
		// removedLocations
		if(typeof(removedLocations) != 'undefined'){
			for (var i in removedLocations){
				url_sufix+='&removedlocations[]='+encodeURI(removedLocations);
				typeOfChange = "removedLocations";
			}
		}
		// addedLocationsLatitude
		if(typeof(addedLocationsLatitude) != 'undefined'){
			for (var i in addedLocationsLatitude){
				url_sufix+='&addedlocationslatitude[]='+addedLocationsLatitude;
				typeOfChange = "addedLocations";
			}
		}
		// addedLocationsLongitude
		if(typeof(addedLocationsLongitude) != 'undefined'){	
			for (var i in addedLocationsLongitude){
				url_sufix+='&addedlocationslongitude[]='+addedLocationsLongitude;
			}
		}

		// CROSS DOMAIN BYPASS URL -X- 
		var filetouse = 'database/save_story_tag_changes_cache.php?'+url_sufix;		
		console.log(filetouse);
		getUrl(filetouse,httpResponseHandlerStoryChanges);
	}
	
	
	
	/**
		Http Response Handler for Changes made to Tags (XML Reader)
		@param: xml, typeOfChange
	**/
	function httpResponseHandlerExploreStories(xml) {
		console.log("handling CHANGES result data");		
		if (checkResultIsOk(xml)) {			
			try{
				// Local variables
				var temp_story;
				var stories_container = new Array();	
			
				// Reading XML Content..
				var xmlResult = xml.getElementsByTagName('xmlResult');
				var storiesCount = xmlResult[0].getElementsByTagName('storiesCount')[0].firstChild.data.trim();
				
				// Reading <stories> content
				var stories = xml.getElementsByTagName('stories');		
				// console.log("Found: " + storiesCount);
				// console.log(stories);
				
				// Reading <categories> content
				var categories = xml.getElementsByTagName('categories');
				//console.log("++++++++++++++");
				//console.log(categories);			
				
				// Reading <locations> content
				var locations = xml.getElementsByTagName('locations');
				//console.log("++++++++++++++");
				//console.log(locations);
				
				// Reset corrector (for stories which don't have any categories marked)
				var categories_corrector = 0;
				var locations_corrector = 0;
							
				// Iterating thorough <stories> <i-0> <i-1> <i-2> ... </stories>			
				for (i=0;i<storiesCount;i++) {				
					storyId = parseInt(stories[0].getElementsByTagName('storyID')[i].firstChild.data.trim());
					title = stories[0].getElementsByTagName('title')[i].firstChild.data.trim();
					userCount = parseInt(stories[0].getElementsByTagName('userCount')[i].firstChild.data.trim());
					startTime = stories[0].getElementsByTagName('startTime')[i].firstChild.data.trim();
					locationCount = parseInt(stories[0].getElementsByTagName('locationCount')[i].firstChild.data.trim());
					entityCount = parseInt(stories[0].getElementsByTagName('entityCount')[i].firstChild.data.trim());
					categoryCount = parseInt(stories[0].getElementsByTagName('categoryCount')[i].firstChild.data.trim());
					
					// *************** Locations ***************
					if (locationCount == 0) {locations_corrector ++};
					var locations_container = new Array();
					for (j=0;j<locationCount;j++) {
						var lon = locations[i-locations_corrector].getElementsByTagName('longitude')[j].firstChild.data.trim();
						var lat = locations[i-locations_corrector].getElementsByTagName('latitude')[j].firstChild.data.trim();					
						locations_container.push({'lon': lon, 'lat': lat});		
					}
					// console.log("geotags loaded");				
					// console.log(locations_container);
					//callbackUpdateGeotags(id,lon,lat);

					
					// *************** Categories ***************
					if (categoryCount == 0) { categories_corrector++ };				
					var categories_container = new Array();								
					for (j=0;j<categoryCount;j++) {
						category_id = categories[i-categories_corrector].getElementsByTagName('id')[j].firstChild.data.trim();
						categories_container.push(category_id);
						// console.log("pushing.." + category_id);
					}			
					
					// *************** Temporary Story *object literal ***************
					temp_story = {
						'storyId' : storyId,
						'title' : title,
						'userCount': userCount,
						'startTime':startTime,
						'locationCount':locationCount,
						'locations': locations_container,
						'entityCount':entityCount,
						'categoryCount':categoryCount,
						'categories' :categories_container
					};
					
					// console.log("temp_story");
					// console.log(temp_story);
					// Add the temporary story to an array
					stories_container.push(temp_story);
				}
			}
			catch (e) {
				console.log("Error Catched" + e);
			}
			
			//console.log("returning stories..");
			//console.log(stories_container);
			callBackQueryStoriesLoaded(stories_container);			
		}
		//else
		//  alert ("something wrong on -> httpResponseHandlerStoryLoad!");				
	}	
	
	
	/**
		Http Response Handler for Changes made to Tags (XML Reader)
		@param: xml, typeOfChange
	**/
	function httpResponseHandlerStoryChanges(xml,typeOfChange) {
		
		console.log("handling CHANGES result data" + typeOfChange);	
		if (checkResultIsOk(xml)) {
			switch(typeOfChange) {			
				// addedKeywords
				case "addedKeywords":
					try {
						var items = xml.getElementsByTagName('keywords');
						var id = items[0].getElementsByTagName('id')[0].firstChild.data.trim();
						var name = items[0].getElementsByTagName('name')[0].firstChild.data.trim();
						callbackUpdateIds("keywords", id, name);
						break;
					}
					catch(e) {
						//ErrorHandler.handleError(e);
						console.log("ERROR CATCHED -> httpResponseHandlerStoryChanges");
					}					
					break;
					
				// addedEntities	
				case "addedEntities":
					// extract id XML
					try {
						var items = xml.getElementsByTagName('entities');
						var id = items[0].getElementsByTagName('id')[0].firstChild.data.trim();
						var name = items[0].getElementsByTagName('name')[0].firstChild.data.trim();
						callbackUpdateIds("entities", id, name);
						break;
					} 
					catch(e) {
						//ErrorHandler.handleError(e);
						console.log("ERROR CATCHED -> httpResponseHandlerStoryChanges");
					}
					break;
				
				// addedLocations	
				case "addedLocations":
					// extract id XML
					try {					
						var items = xml.getElementsByTagName('locations');
						var id = items[0].getElementsByTagName('id')[0].firstChild.data.trim();
						var lon = items[0].getElementsByTagName('longitude')[0].firstChild.data.trim();
						var lat = items[0].getElementsByTagName('latitude')[0].firstChild.data.trim();
						console.log("geotags ID loaded");
						callbackUpdateGeotags(id,lon,lat);
						break;
					}
					catch(e) {
						//ErrorHandler.handleError(e);
						console.log("ERROR CATCHED -> httpResponseHandlerStoryChanges");
					}
					break;
				
				case "addedCategories":
					// extract id XML						
					callbackUpdateIds("categories",addedCategories[0],"null");
					break;
					
				case "removedEntities":
					// extract id XML						
					callbackRemove("entities");
					break;
					
				case "removedCategories":
					// extract id XML						
					callbackRemove("categories");
					break;
					
				case "removedKeywords":
					// extract id XML						
					callbackRemove("keywords");
					break;		

				case "removedLocations":							
					callbackRemove("locations");					
					break;
					
				default:
					// extract id XML	
			}		
		}
		else {
			//alert ("something wrong on httpResponseHandlerStoryChanges!");	
		}		
	}
	
	
	/**
		Http Response Handler for Loading Story (XML Reader)
		@param: xml
	**/
	function httpResponseHandlerStoryLoad(xml) {
		console.log("handling result data -> http Response Handler Story Loading..");	  
		if (checkResultIsOk(xml)) {
			
			//try {
				// Story Information
				var storyId = Math.abs(xml.getElementsByTagName('storyID')[0].firstChild.data.trim());
				var title = xml.getElementsByTagName('title')[0].firstChild.data.trim();

				// Geotags
				var geotags = new Array();	      
				var item_count = Math.abs(xml.getElementsByTagName('locationCount')[0].firstChild.data.trim());	      	      	
				var items = xml.getElementsByTagName('locations');
				//console.log("has only " + item_count + " geotags");
				for (i=0;i<item_count;i++) {				
					geotags.push({			
					  "lon": items[0].getElementsByTagName('longitude')[i].firstChild.data.trim().toString(),
					  "id": parseInt(items[0].getElementsByTagName('id')[i].firstChild.data.trim()),				   
					  "lat": items[0].getElementsByTagName('latitude')[i].firstChild.data.trim().toString()});
				}  
				console.log("geotags loaded");

				// Categories
				var categories = new Array();			
				var item_count = Math.abs(xml.getElementsByTagName('categoryCount')[0].firstChild.data.trim());	      			
				var items = xml.getElementsByTagName('categories');
				for (i=0;i<item_count;i++) {
					categories.push({
					  "id": parseInt(items[0].getElementsByTagName('id')[i].firstChild.data.trim()),
					  "name": items[0].getElementsByTagName('name')[i].firstChild.data.trim()});			          
				}     

				// Entities
				var entities = new Array();
				var item_count = Math.abs(xml.getElementsByTagName('entityCount')[0].firstChild.data.trim());	      
				var items = xml.getElementsByTagName('entities');
				for (i=0;i<item_count;i++) {
					entities.push({
					  "id": parseInt(items[0].getElementsByTagName('id')[i].firstChild.data.trim()),
					  "name": items[0].getElementsByTagName('name')[i].firstChild.data.trim()});			          
				}

				// Keywords
				var keywords = new Array();
				var item_count = Math.abs(xml.getElementsByTagName('keywordCount')[0].firstChild.data.trim());	      
				var items = xml.getElementsByTagName('keywords');
				for (i=0;i<item_count;i++) {
					keywords.push({
					  "id": parseInt(items[0].getElementsByTagName('id')[i].firstChild.data.trim()),
					  "name": items[0].getElementsByTagName('name')[i].firstChild.data.trim()});			          
				}

				// Debug * Debug * Debug * 
				console.log("StoryId " + storyId);
				console.log("Story Title " + title);
				console.log("geotags");
				console.log(geotags);
				console.log("categories");
				console.log(categories);
				console.log("entities");
				console.log(entities);
				console.log("keywords");
				console.log(keywords);

				// Story Object Literal Construction
				var story = {
					'storyId' : storyId,
					'storyTitle' : title,
					'geotags' : geotags,
					'categories' : categories,
					'entities' : entities,
					'keywords' : keywords
				}

				console.log("returning story..");		
				console.log(story);			
				callBackStoryLoaded(story);	// Callback to INDEX.PHP
			//}
			//catch(error) {
			//	// todo
			//	console.log("Error catched at xml_parser.js: " + error);
			//}		
		}
		//else
		  //alert ("something wrong on -> httpResponseHandlerStoryLoad!");	      	      	      
	} // end function
	
	
	/**
     * Callback function called from XML_PARSER after a Story has been loaded from DATABASE Webservice
	 *
     *@param  {Story}	
     */	
	function callBackStoryLoaded(story) {
		console.log("Call Back XML Parser complete");		
		drawStory(story);
	}