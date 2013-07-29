/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

	/***	
	Load Story Tags
	import a list of tags from ActiveStory -- fill in each "TAG BOX"
		@param story
		@return true
	***/	
	function loadStoryTextTags(story) {
		console.log("Loading Story Text Tags");
		// reset tag list
		$('#tags_categories').importTags('');
		$('#tags_entities').importTags('');
		$('#tags_keywords').importTags('');
		
		// Loopers
		// Categories
		console.log("Searching on categories.." + environment.active_story.categories.length);
		for(var i=0; i<environment.active_story.categories.length; i++){ 
			$('#tags_categories').addTag(environment.active_story.categories[i].name);
		}
		
		// Entities
		for(var i=0; i<environment.active_story.entities.length; i++){ 
			$('#tags_entities').addTag(environment.active_story.entities[i].name);
		}
		
		// Keywords
		for(var i=0; i<environment.active_story.keywords.length; i++){ 
			$('#tags_keywords').addTag(environment.active_story.keywords[i].name);
		}		
		
		environment.texttag_flag = false;
		return true;
	}
	
	/**
	 * Generates an array of ID's to Jakobs new CSS categories list selection
	 *
	 *@returns  array{int}
	 */		
	function getStoryCategoriesIDs() {
		var temp_array = new Array();
		for(var i=0; i<environment.active_story.categories.length; i++){ 
			temp_array.push(environment.active_story.categories[i].id);
		}
		return temp_array;
	}
	
	
	/**
	 * Clears the Text Tags Lists
	 *
	 */		
	function resetTextTagsList(){
		// reset tag list
		$('#tags_categories').importTags('');
		$('#tags_entities').importTags('');
		$('#tags_keywords').importTags('');
	}
	
	
	// ENTITIES --------------------------------------------	
	/**
	 * Called when user 'Enters/Types' a new Entity
	 *
	 *@param  tag{String}
	 */		
	function onAddTagEntities(tag) {
		// console.log("on Add TagEntities");
		
		// Stories Explorer
		if (environment instanceof storiesExplorer) {	
			crisis_tracker.added_entities_array.push(tag);
			onQueryUpdated();
		}
		
		// Story Tagger
		else if(!environment.texttag_flag && environment instanceof storyTagger) {
			// 1st load			
			crisis_tracker.added_entities_array.push(tag);			
			saveStoryTagChangesDB();		
		}
	}
	
	
	/**
	 * Called when user 'Removes' a new Entity
	 *
	 *@param  tag{String}
	 */		
	function onRemoveTagEntities(tag) {
		// console.log("on Remove TagEntities");
		
		// Stories Explorer
		if (environment instanceof storiesExplorer) {	
			var idx = crisis_tracker.added_entities_array.indexOf(tag); // Find the index
			if(idx!=-1) { crisis_tracker.added_entities_array.splice(idx, 1) }; // Remove it if really found!			
			onQueryUpdated();		
		}
		
		// Story Tagger
		else if(!environment.texttag_flag && environment instanceof storyTagger) {
			for(var i=0; i<environment.active_story.entities.length; i++){
				if (environment.active_story.entities[i].name == tag) {
					crisis_tracker.removed_entities_array.push((environment.active_story.entities[i].id));
				}
			}
			console.log(crisis_tracker.removed_entities_array);					
			saveStoryTagChangesDB();		
		}
	}	
	
	/**
	 * Called when user 'Changes' a new Entity
	 *
	 *@param  tag{String}
	 */		
	function onChangeTagEntities(input,tag) {
		// TODO
	}
	
	
	
	// CATEGORIES --------------------------------------------
	/**
	 * Called when user 'Enters/Types' a new Entity
	 *
	 *@param  tag{String}
	 */			
	function onAddTagCategories(tag) {
		// console.log("on Add TagCategories");
		
		// Stories Explorer
		if (environment instanceof storiesExplorer) {
			if (crisis_tracker.added_categories_array.indexOf(id) < 0) {
				crisis_tracker.added_categories_array.push(id);
			}
			onQueryUpdated();		
		}
		
		// Story Tagger
		else if (environment instanceof storyTagger) {	
			crisis_tracker.added_categories_array.push(id);
			saveStoryTagChangesDB();		
		}
	}

	/**
	 * Called when user 'Removes' a new Category, saves the change on-the-fly to DB Webservice
	 *
	 *@param  tag{String}
	 */			
	function onRemoveTagCategories(tag) {
	
		// Stories Explorer
		if (environment instanceof storiesExplorer) {
			var idx = $.inArray(tag, crisis_tracker.added_categories_array); // Find the index
			if(idx!=-1) { crisis_tracker.added_categories_array.splice(idx, 1) }; // Remove it if really found!			
			console.log("Exploring -> onRemoveTagCategories");
			onQueryUpdated();		
		}
		
		// Story Tagger
		else if(!environment.texttag_flag && environment instanceof storyTagger) {
			console.log("on Remove TagCategories");				
			crisis_tracker.removed_categories_array.push(id);
			console.log(crisis_tracker.removed_categories_array);
			// Save to Database
			saveStoryTagChangesDB();			
		}		
	}
	
	
	/**
	 * Updates environment.active_story with new "Category" added by user
	 *
	 *@param  tag{String}
	 */	
	function callBackUpdateStoryCategory(id,tag) {			
			environment.active_story.categories.push({"id":id,"name":tag});
			console.log("categories array updated in ActiveStory");	
	}

	/**
	 * Called when user 'Changes' a new Category
	 *
	 *@param  tag{String}
	 */		
	function onChangeTagCategories(input,tag) {
		colorizeCategoryTags(tag);
		// TODO
	}	

	
	
	// KEYWORDS --------------------------------------------
	function onAddTagKeywords(tag) {
		// console.log("on Add TagKeywords");
		
		// Stories Explorer
		if (environment instanceof storiesExplorer) {			
			crisis_tracker.added_keywords_array.push(tag);
			onQueryUpdated();		
		}
		
		// Story Tagger
		else {		
			if(!environment.texttag_flag && environment instanceof storyTagger) {	// 1st load				
				crisis_tracker.added_keywords_array.push(tag);
				saveStoryTagChangesDB();		
			}					
		}
	}
	
	function onRemoveTagKeywords(tag) {
		// console.log("on Remove TagKeywords");
		
		// Stories Explorer
		if (environment instanceof storiesExplorer) {	
			var idx = crisis_tracker.added_keywords_array.indexOf(tag); // Find the index
			if(idx!=-1) { crisis_tracker.added_keywords_array.splice(idx, 1) }; // Remove it if really found!
			onQueryUpdated();		
		}
		
		// Story Tagger
		else if(!environment.texttag_flag && environment instanceof storyTagger) {
			for(var i=0; i<environment.active_story.keywords.length; i++){
				if (environment.active_story.keywords[i].name == tag) {
					crisis_tracker.removed_keywords_array.push((environment.active_story.keywords[i].id));
				}
			}
			// console.log(crisis_tracker.removed_keywords_array);
			saveStoryTagChangesDB();
		}		
	}
	
	
	function onChangeTagKeywords(input,tag) {
		// TODO	
	}
	
	
	
	

	
	
	// // TO DEPRECATE *Jakob prefers checkboxes CSS
	// function getIntFromCategoriesList(tag) {
		// $.getJSON('database/categories.json', 
			// function(data) {
			  // var items = [];
			  
			  // $.each(data, function(i, item) {
			    // //console.log("comparing.." + item.label + " with " + tag);
				// if(item.label.toString() == tag) {
					// console.log("RETURNING.. " + item.id);
					// callBackFindTagCategoryName(item.id);					
				// }				
			  // });	
			// return 0;
		// });	
	// }
	
	// // TO DEPRECATE *Jakob prefers checkboxes CSS
	// function getTagFromCategoriesList(id) {
		// $.getJSON('database/categories.json', 
			// function(data) {
			  // var items = [];
			  
			  // $.each(data, function(i, item) {
			    // //console.log("comparing.." + item.label + " with " + tag);
				// if(item.id == id) {
					// console.log("RETURNING.. " + item.label);
					// callBackUpdateStoryCategory(id, item.label);					
				// }				
			  // });	
			// return 0;
		// });	
	// }	
	
	
	/***	
		Colorize Category Text Tags
	***/	
	function colorizeCategoryTags(tag) {
		var languages = ['fire','explosion','tsunami'];
		$('.tag', tag).each(function()
		{
			if($(this).text().search(new RegExp('\\b(' + languages.join('|') + ')\\b')) >= 0)
				$(this).css('background-color', 'orange');
		});			
	}