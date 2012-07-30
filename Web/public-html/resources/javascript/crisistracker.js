// 1. All of the global variables and functions into a new object 
/**
* hold an instance of a 'story.php' file
**/
function storyTagger(){

	// ********************************************
	// Variables 
	var active_story;
	var active_feature;	
	var final_lonlat;
	var initial_lonlat;
	var map;
	var vector_layer;
	
	// Arrays
	var added_geotags_array;
	var removed_geotags_array;
	
	// Flags
	var texttag_flag;
	var popup_flag;
	
	// ********************************************
	// DataStructures Initializations
	this.added_geotags_array = new Array();
	this.removed_geotags_array = new Array();
}


/**
* hold an instance of a 'explorestories.php' file
**/
function storiesExplorer() {
	
	// ********************************************
	// Variables 
	var map;
	var vector_layer;
	var priority;
	var added_bounds;
	var addedMinDate;
	var addedMaxDate;		
}


/**
* (Common) - hold an instance of the common data structure used by all of the components of the "system"
**/
function crisisTracker() {
	// ********************************************
	// Variables
	var added_categories_array;
	var added_keywords_array;	
	var added_entities_array;
	
	var removed_categories_array;
	var removed_entities_array;
	var removed_keywords_array;	
	
	// Flags
	var action_flag = "";		
	
	// ********************************************
	// DataStructures Initializations	
	this.added_categories_array = new Array();
	this.added_entities_array = new Array();
	this.added_keywords_array = new Array();
	
	this.removed_entities_array = new Array();
	this.removed_categories_array = new Array();		
	this.removed_keywords_array = new Array();	
}