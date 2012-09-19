/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

/* Functions for printing and interacting with the category buttons */
/*
var categoryNames = ["Civillian&nbsp;involvement",
  "Military&nbsp;involvement",
  "Crime",
  "Violence",
  "Deaths",
  "Missing&nbsp;people",
  "Damaged&nbsp;infrastructure",
  "Natural&nbsp;hazard",
  "Political&nbsp;event",
  "Summary&nbsp;report",
  "Available&nbsp;resource",
  "Request/Need",
  "Warning/Risk/Danger",
  "High&nbsp;impact&nbsp;event"];
*/
var categoryNames = ["Demonstration",
  "Violence",
  "Detained/Missing",
  "Torture/Rape",
  "Killed",
  "Heavy&nbsp;weapons/Bombing",
  "Affected&nbsp;infrastructure",
  "People&nbsp;movement",
  "Political/int&#39;l event",
  "Risk/Hazard/Threat",
  "Summary&nbsp;report",
  "Eyewitness&nbsp;report",
  "Rumor/False",
  "High&nbsp;impact&nbsp;event"];

var categoryTooltips = ["Demonstration, rally, public gathering, etc.",
  "Assault, break-ins, riots, stone throwing, small-arms fire, clashes between protesters and law enforcement, etc.",
  "Arrests, abductions, kidnappings, missing people, etc.",
  "Torture, rape, sexual abuse, etc.",
  "Report of death by non-natural causes",
  "E.g. snipers, machineguns, landmines, explosive devices, grenades, tanks, fighter jets, helicopters",
  "Destroyed or restored infrastructure, e.g. roads, hospitals, schools, homes, pipelines",
  "People who for political, safety or other reasons are forced to abandon their homes, temporarily or permanently",
  "Public statements, agreements, meetings, or other events that take place on the international area",
  "Events which can lead to future humanitarian needs, including forest fires, toxic releases, escalations in conflict, political threats, etc.",
  "Report summarizing several events, e.g. a news article or analytical report",
  "First-hand report by direct witness, ideally supported by video, image, sound or other non-textual media",
  "Report or statement suspected or confirmed as false",
  "Event with significant negative or positive impact on the future direction of the crisis"];

/*
 * Prints the list of category buttons to the div with id=categorybuttons
 * input: array of selected category IDs
 */
function printCategoryButtons(activeIDs) {
  $('div#categorybuttons').html("");
  
  if (activeIDs==null) {
    activeIDs = new Array();
  }

  html = "";
  for (i=1; i<15; i++) {
    if (i==1 || i==8) {
      html += '<div class="category-button-column">';
    }
  
    inactive="";
    if (activeIDs.indexOf(i) < 0) {
      inactive=" inactive";
    }
      
    html += '<a class="category-button cc' + i + inactive + '" title="' + categoryTooltips[i-1] + '"><span class="value">' + i + '</span><span class="label">' + categoryNames[i-1] + '</span></a>';
    
    if (i==7 || i==14) {
      html += '</div>';
    }
  }
  $('div#categorybuttons').html(html);

  //Attach event listeners
  $('a.category-button').click(function(){
    $(this).toggleClass("inactive");
    id = parseInt($(this).find('.value').text());
    isSelected = !($(this).hasClass('inactive'));
    updateCategory(id, isSelected); 
  });
}

/*
 * Returns the category IDs of currently selected category buttons
 * output: array of selected category IDs
 */
function getSelectedCategories() {
  $selectedItems = new Array();
  $('.category-button').each(function(index) {
    if(!$(this).hasClass('inactive')) {
      $selectedItems.push(index+1);
    }
  });
  return $selectedItems;
}

/**
Extract name of a category according to provided ID   
**/
function getCategoryNameById(id) {
var tag;
// since cannot have access to category_names array (not global)
  $('.category-button').each(function(index) {
    if (id == index+1) {  
      tag = $(this).find('.label').text();
      callbackUpdateActiveStoryAddedCategory(id,tag);
    }
    return 0;
  });
}

/**
Triggers Add or delete functions tag depending on value of isSelected
**/
function updateCategory(id, isSelected) {
  if (!isSelected) {
    // add to removed_categories_array
    onRemoveTagCategories(id);
  }
  else {    
    // add to added_categories_array
    onAddTagCategories(id);   
  }
}