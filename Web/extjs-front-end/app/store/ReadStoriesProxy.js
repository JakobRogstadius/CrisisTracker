Ext.define('CrisisTracker.store.ReadStoriesProxy', {
	storeId:'ajaxProxyReadStories',
    extend: 'Ext.data.Store',	
	model: 'CrisisTracker.model.StoriesListGridModel',	
	autoLoad: false,
	selType: 'cellmodel',
	stateful: true,
    pageSize: 20,
    proxy: {
        type: 'ajax',
		extraParams : 'option',		
		autoAbort: true,
		url: 'data/dummy/tag_stories.json',
		reader: {
			type: 'json',	//or xml
			root: 'stories'	// root of JSON results
		}
    },
	
	listeners: {		
		beforeload: function(store, operation, eOpts) {
		
			var proxy = this.getProxy(),
			lastRequest = proxy.lastRequest;

			if (this.isLoading()) {
				console.log('aborting current one');
				Ext.Ajax.abort(lastRequest);
			}					
		}		
	}
	
});
