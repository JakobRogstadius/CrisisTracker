/**
 * Application Store
 */ 

Ext.define('mod2.store.GridAjaxProxy', {
	storeId:'ajaxProxy',
    extend: 'Ext.data.Store',	
	model: 'mod2.model.GridData',	
	autoLoad: true,
    proxy: {
        type: 'ajax',
		extraParams : 'option',		
		autoAbort: true,
		url: 'data/gridfeeder.php',
		reader: {
			type: 'json',	//or xml
			root: 'circles'	// root of JSON results
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
