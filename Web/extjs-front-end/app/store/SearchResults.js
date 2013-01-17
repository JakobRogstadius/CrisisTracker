Ext.define('CrisisTracker.store.storiesexplorer.SearchResults', {
    extend: 'Ext.data.Store',
    // requires: 'CrisisTracker.model.StoryModel',
    // model: 'CrisisTracker.model.StoryModel',

    // Overriding the model's default proxy
    proxy: {
        type: 'ajax',
        url: 'data/searchresults.json',
        reader: {
            type: 'json',
            root: 'results'
        }
    }
	
});