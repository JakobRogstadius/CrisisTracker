/**
 * Store for acessing the data in WebService
  * @extends Ext.data.Store
 */
Ext.define('CrisisTracker.store.MapItems', {
	storeId:'mapItems',    
	model: 'CrisisTracker.model.MapItem',		
	extend: 'Ext.data.Store',	
	autoLoad: false,
	
	proxy: {
        type: 'ajax',
		extraParams : 'boundingbox',		
		autoAbort: true,
		url: 'data/dummy/read_stories.php',
		reader: {
			type: 'json',	//or xml
			root: 'features'
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
	
	
	// TODO: Define a Proxy via 'GeoExt' library in order to work with GeoJSON OpenLayers format.
	
	// proxy:  Ext.create('GeoExt.data.proxy.Protocol',{		
		// protocol: new OpenLayers.Protocol.HTTP({
			// url: "data/features.php",
			// format: new OpenLayers.Format.GeoJSON(),
			// read: function(o) {
				// console.log(o.params);
				// console.log(o.filter);
				// console.log(o.proxy);
				// console.log(o.params);
				// o.callback.call(o.scope, response);
			// }			
		// })
	// }),
	// autoLoad: true,	
  

	
});
