function Controller() {  
  var storiesModel = new StoriesModel();

  this.loadStoriesTab = function() {
    storiesModel.refresh();
  }
  
  this.loadTrendsTab = function() {
    
  }
  
  this.loadEntitiesTab = function() {
    
  }
}