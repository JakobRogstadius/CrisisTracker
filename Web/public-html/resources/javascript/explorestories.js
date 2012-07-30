/**
* Save any change made to the Query Parameters
**/

//Returns an object of arrays of filter settings
function loadFilterSettingsFromCookie() {
	var settings = {};
	
	var categoryfilter = $.cookie("categoryfilter");
	if (categoryfilter !== null && categoryfilter !== '') {
		settings["categoryfilter"] = $.map(categoryfilter.split(","), function (x) { return parseInt(x); });
	}

	var keywordfilter = $.cookie("keywordfilter");
	if (keywordfilter !== null && keywordfilter !== '') {
		settings["keywordfilter"] = keywordfilter.split(",");
	}

	var entityfilter = $.cookie("entityfilter");
	if (entityfilter !== null && entityfilter !== '') {
		settings["entityfilter"] = entityfilter.split(",");
	}

	var minstarttime = $.cookie("minstarttime");
	if (minstarttime !== null && minstarttime !== '') {
		settings["minstarttime"] = minstarttime;
	}

	var maxstarttime = $.cookie("maxstarttime");
	if (maxstarttime !== null && maxstarttime !== '') {
		settings["maxstarttime"] = maxstarttime;
	}

	var locationfilter = $.cookie("locationfilter");
	if (locationfilter !== null && locationfilter !== '') {
		settings["locationfilter"] = $.map(locationfilter.split(","), function (x) { return parseFloat(x); });
	}

	var sortorder = $.cookie("sortorder");
	if (sortorder !== null && sortorder !== '') {
		settings["sortorder"] = sortorder;
	}

	return settings;
}

//Takes arrays of filter settings
function saveFilterSettingsToCookie(categoryfilter, keywordfilter, entityfilter, minstarttime, maxstarttime, locationfilter, sortorder)
{
	var inputs = {
		"categoryfilter" : categoryfilter,
		"keywordfilter" : keywordfilter,
		"entityfilter" : entityfilter,
		"minstarttime" : minstarttime,
		"maxstarttime" : maxstarttime,
		"locationfilter" : locationfilter,
		"sortorder" : sortorder
	};
	for (var key in inputs) {
		if (inputs[key] == null || inputs[key] == '' || inputs[key] == [])
			$.cookie(key, null);
		else if (inputs[key] instanceof Array)
			$.cookie(key, inputs[key].join(","));
		else
			$.cookie(key, inputs[key]);
	}
}

var populated_filters = false;
function saveQueryParamsToDB() {
	if (!populated_filters)
		return;

	removeAllGeometryFromVectorLayer("OpenLayers.Geometry.Point");
	showLoadingAnimation();	
	clearHtmlStoriesList();
	// removeAllFeaturesFromLayer();
	// clearDatePickers();
	console.log("saveQueryParamsToDB.... now calling ajaxSendQuery()");	
	// -------- Parameters -----------	
	// Category
	var categoryfilter = new Array();
	categoryfilter = crisis_tracker.added_categories_array;
	
	// Entity
	var entityfilter = new Array();
	entityfilter = crisis_tracker.added_entities_array;
	
	// Keyword
	var keywordfilter = new Array();
	keywordfilter = crisis_tracker.added_keywords_array;
	
	// locationfilter: minLong, minLat, maxLong, maxLat
	// var locationfilter = "-180,-90,180,90";	
	var locationfilter = environment.added_bounds;
	
	// minstarttime: YYYY-MM-DD hh:mm:ss
	var minTimeStr = null, minTimeStrShort = null;
	if (environment.addedMinDate) {
		m = environment.addedMinDate.getMonth() + 1;
		d = environment.addedMinDate.getDate();
		minTimeStrShort = environment.addedMinDate.getFullYear() + "-" + (m < 10 ? "0" + m : m) + "-" + (d < 10 ? "0" + d : d);
		minTimeStr = minTimeStrShort + "%2000:00:00";
	}

	var maxTimeStr = null, maxTimeStrShort = null;
	if (environment.addedMaxDate) {
		m = environment.addedMaxDate.getMonth() + 1;
		d = environment.addedMaxDate.getDate();
    maxTimeStrShort = environment.addedMaxDate.getFullYear() + "-" + (m < 10 ? "0" + m : m) + "-" + (d < 10 ? "0" + d : d);
		maxTimeStr = maxTimeStrShort + "%2000:00:00";
	}
		
	// Stories sort order (e.g. largest, trending)
	var sortorder = environment.priority;
	
	// limit: int
	var limit = 50;	
	
	console.log("LOCATIONFILTER IS " + locationfilter);
	saveFilterSettingsToCookie(
	    categoryfilter,
	    keywordfilter,
	    entityfilter,
	    minTimeStrShort,
	    maxTimeStrShort,
	    locationfilter,
	    sortorder
	);
	
	// --------- AJAX Call ------------
	ajaxSendQuery(categoryfilter,entityfilter,keywordfilter,sortorder,limit,minTimeStr,maxTimeStr,locationfilter);
	// ajaxSendQuery(categoryfilter,entityfilter,keywordfilter,sortorder,limit,minstarttime,maxstarttime);	//../database/xlm_parser.js	
}

function populateFilterSettings() {
	console.log("Populating filter settings");
	var settings = loadFilterSettingsFromCookie();
	
	//Categories
	if (settings.hasOwnProperty("categoryfilter")){
		printCategoryButtons(settings["categoryfilter"]); //Doesn't set global filter variable
		for(i=0;i<settings["categoryfilter"].length;i++)
			crisis_tracker.added_categories_array.push(settings["categoryfilter"][i]);
	}
	else
		printCategoryButtons();
	
	//Entities
	if (settings.hasOwnProperty("entityfilter")) {
		for(var i=0; i<settings["entityfilter"].length; i++){ 
			$('#tags_entities').addTag(settings["entityfilter"][i]);
		}
	}

	//Keywords
	if (settings.hasOwnProperty("keywordfilter")) {
		for(var i=0; i<settings["keywordfilter"].length; i++){ 
			$('#tags_keywords').addTag(settings["keywordfilter"][i]);
		}
	}
	
	//Time range
	if (settings.hasOwnProperty("minstarttime")) {
		$("#from").val(settings["minstarttime"]);
		var dateParts = settings["minstarttime"].split("-");
		date = new Date(dateParts[0], (dateParts[1] - 1), dateParts[2]);
		handleDateSelection(date, "from");
		
	}
	if (settings.hasOwnProperty("maxstarttime")) {
		$("#to").val(settings["maxstarttime"]);
		var dateParts = settings["maxstarttime"].split("-");
		date = new Date(dateParts[0], (dateParts[1] - 1), dateParts[2]);
		handleDateSelection(date, "to");
	}
	
	//Location
	if (settings.hasOwnProperty("locationfilter")) {
		var minlon = settings["locationfilter"][0];
		var minlat = settings["locationfilter"][1];
		var maxlon = settings["locationfilter"][2];
		var maxlat = settings["locationfilter"][3];
		
		$("#mapbounds").attr("checked", true);
		$('.map_inactive').toggleClass("map_active");
		
		setMapActiveBounds(new OpenLayers.Bounds(minlon, minlat, maxlon, maxlat));
	}

	populated_filters = true;

  //Sortorder
	if (settings.hasOwnProperty("sortorder")) {
    switchPriority(settings["sortorder"]);
	}
	else {
    switchPriority("active");
	}
}

/**
**	On-the-fly updated
**/
function onQueryUpdated(){
	//console.log("Query Updated");
	saveQueryParamsToDB()
}

/**
**	Reset Containers
**/
function onLoadExploreStoriesResetContainers() {
	resetTagContainers("added_texttags");
	resetTagContainers("removed_texttags");
}


/**
**	Called after the stories_container was constructed on xml_parser.js from the WebService AJAX Call
**/
function callBackQueryStoriesLoaded(stories_container) {
	console.log("callBackQueryStoriesLoaded");
	var geotags_container = new Array();
	var lonlat;
	// console.log(stories_container);
	
	// Stories Reading
	for(var i=0; i<stories_container.length; i++) {
		
		// params
		var storyId = stories_container[i].storyId;
		var title = stories_container[i].title;
		var userCount = stories_container[i].userCount;
		// console.log("userCount");
		// console.log(userCount);
		var startTime = stories_container[i].startTime;

		var entityCount = stories_container[i].entityCount;
		
		var locationCount = stories_container[i].locationCount;
		var locations = stories_container[i].locations;
				
		var categoryCount = stories_container[i].categoryCount;
		var categories = stories_container[i].categories;
				
		// filling html data
		htmlStoriesListFiller(storyId, title, userCount, locationCount, entityCount, startTime, categoryCount, categories);

		// GeoTags
		for (var j=0; j<locationCount; j++) {
			lonlat = new OpenLayers.LonLat(locations[j].lon, locations[j].lat);	// Create temporary LonLat
			lonlat = reprojectFrom4326To900913(lonlat);	// Transform coordinates from 4326 to 900913 EPSG
			geotags_container.push(generateTemporaryGeotagFromLonlat(lonlat,storyId));	// {lon,lat,id}
		}
	}
	// drawGeoTags(geotags_container);
	displayNewFeature(geotags_container);
	hideLoadingAnimation();
	if (!document.getElementById('mapbounds').checked) {
		setMapExtentToAllGeotags(environment.map, geotags_container);
	}
}



/**
**	Fill in the HTML code into the List with the content from the Stories
**/
function htmlStoriesListFiller(storyId, title, userCount, locationCount, entityCount, startTime, categoryCount, categories) {	
	var colors_html='';
	var loc_tags_hasvalue = '';
	var ent_tags_hasvalue = '';
	
	for(var j=0; j<categoryCount; j++) {
		var color_code = 'cc'+categories[j];
		colors_html += '<span class="td '+color_code+'">&nbsp;</span>';
	}
	
	if (locationCount != 0) {
		loc_tags_hasvalue = "hasvalue";
	}
	
	if (entityCount != 0) {
		ent_tags_hasvalue = "hasvalue";
	}
	
	var html = '<li class="story-list-item"><a class="wrapper" href="story.php?storyid='+storyId+'">'
		+ '<span class="column size">'+userCount+'</span>'
		+ '<span class="column time">'+startTime+'</span>'
		+ '<span class="column title">'+title+'</span>'
		+ '<span class="column loc-tags '+loc_tags_hasvalue+'">'+locationCount+'</span>'
		+ '<span class="column ent-tags '+ent_tags_hasvalue+'">'+entityCount+'</span>'
		+ '<span class="column colorbars"><span class="table"><span class="tr">'+colors_html+'</span></span></span></a></li>';

	$('#story-list-items').append(html);
}



/**
**	Clear the Current Stories List
**/
function clearHtmlStoriesList() {
	// Using jQuery '.empty()' *http://api.jquery.com
	$('#story-list-items').empty();

	var html = '<li class="story-list-item header">'
		+ '<span class="column size" title="A weighted value of the total number of users, tweets and retweets in a story">Size</span>'
		+ '<span class="column time" title="The time (UTC) at which the first tweet in the story was posted">Time</span>'
		+ '<span class="column title" title="Derived from the most representative tweet in the story">Title</span>'
		+ '<span class="column" title="Tags contributed by volunteers">Tags</span>'
		+ '</li>';

	$('#story-list-items').append(html);
}

function addDateTextboxListeners() {
	$("#from").change(function() {
		var date = null;
		if ($("#from").val() != "") { 
			var dateParts = $("#from").val().split("-");
			date = new Date(dateParts[0], (dateParts[1] - 1), dateParts[2]);
		}
		handleDateSelection(date, "from");
	});
	$("#to").change(function() {
		var date = null;
		if ($("#to").val() != "") { 
			var dateParts = $("#to").val().split("-");
			date = new Date(dateParts[0], (dateParts[1] - 1), dateParts[2]);
		}
		handleDateSelection(date, "to");
	});
}

/**
**	JQuery Date Picker
**/
function startDatePicker() {
	var dates = $( "#from, #to" ).datepicker({
		defaultDate: "+1w",
		changeMonth: true,
		dateFormat: "yy-mm-dd",
		numberOfMonths: 3,
		
		// Event Handlers
		onSelect: function( selectedDate ) {
			var option = this.id == "from" ? "minDate" : "maxDate",				
				instance = $( this ).data( "datepicker" ),
				date = $.datepicker.parseDate(
					instance.settings.dateFormat ||
					$.datepicker._defaults.dateFormat,
					selectedDate, instance.settings );				
			dates.not( this ).datepicker( "option", option, date );
			handleDateSelection(date, this.id);
		},
		
		onClose : function( selectedDate ) {
			instance = $( this ).data( "datepicker" );
			//handleDateClose(selectedDate, instance);
		}
	});
	
	$("#from").datepicker({dateFormat: 'yyyy-mm-dd'});
	$("#to").datepicker({dateFormat: 'yyyy-mm-dd'});
}

/**
**	Clear Input Text Boxes of 'JQuery Date Picker'
**/
function clearDatePickers() {
	 $("#from").val('');
	 $("#to").val('');
}

/**
**	Switch Priority
**/
function switchPriority(command) {
	var active = "'active'";
	//var newest = "'newest'";
	var largest = "'largest'";
	var trending = "'trending'";
	var timeline = "'timeline'";
	var html = "<p>Sort order: ";

	if (command == 'active') {
		environment.priority = 'active';
		html += '<strong title="Sort stories by the number of users, tweets and retweets they received in the past four hours">Most active</strong>';
	}
	else
		html += '<a title="Sort stories by the number of users, tweets and retweets they received in the past four hours" href="javascript:void(0)" onClick="handlePriorityClick('+active+')">Most active</a>';
	if (command == 'trending') {
		environment.priority = 'trending';
		html += '| <strong title="Sort stories by their relative rate of growth" >Trending</strong>';
	}
	else
		html += '| <a title="Sort stories by their relative rate of growth" href="javascript:void(0)" onClick="handlePriorityClick('+trending+')">Trending</a>';
	if (command == 'largest') {
		environment.priority = 'large';
		html += '| <strong title="Sort stories by their total size">Largest</strong>';
	}
	else
		html += '| <a title="Sort stories by their total size" href="javascript:void(0)" onClick="handlePriorityClick('+largest+')">Largest</a>';
	if (command == 'timeline') {
		environment.priority = 'timeline';
		html += '| <strong title="Show the largest stories sorted by time">Timeline</strong>';
	}
	else
		html += '| <a title="Show the largest stories sorted by time" href="javascript:void(0)" onClick="handlePriorityClick('+timeline+')">Timeline</a>';
	html += "</p>";
	
	saveQueryParamsToDB();
	
	// Using jQuery '.append()'
	$('.priority-filters').empty();
	$('.priority-filters').append(html);
}


function activateMapBorderSelection() {
	//Attach event listeners
	$('#mapbounds').click(function(){
		$('.map_inactive').toggleClass("map_active");
		//isSelected = !($(this).hasClass('inactive'));
		handleMapBoundsCheckBoxClick(this);
	});
}