Ext.define('CrisisTracker.store.ReadStoriesProxy', {
	storeId:'ajaxProxyReadStories',
    extend: 'Ext.data.Store',	
	model: 'CrisisTracker.model.MapItem',	
	autoLoad: false,
	selType: 'cellmodel',
	stateful: true,
    //pageSize: 20,
    proxy: {
        type: 'ajax',
		extraParams : {
			'boundingbox' : null,
			'whatcategories' : null,
			'what'  : null,
			'who'  : null,
			'from'  : null,
			'to'  : null,
		},
		autoAbort: true,
		url: 'data/dummy/read_stories.php',
		reader: {
			type: 'json',	//or xml
			root: 'features'	// root of JSON results
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
