/* Functions for printing and interacting with the category buttons */

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
      
    html += '<a class="category-button cc' + i + inactive + '"><span class="value">' + i + '</span><span class="label">' + categoryNames[i-1] + '</span></a>';
    
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