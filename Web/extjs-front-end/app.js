/**
 * Ext.Loader
 * Ext.Loader is the heart of the new dynamic dependency loading capability in Ext JS 4+
 */
Ext.Loader.setConfig({
    enabled: true,
    disableCaching: false,
	
	// mapping from namespaces to file paths
    paths: {
        //GeoExt: "GeoExt",
	    //Ext: "ext-4.1.1a/src"
    }	
});


/**
 * CrisisTracker.app
 */
Ext.application({
    name: 'CrisisTracker',	// Namespace
    appFolder: 'app',
	autoCreateViewport: false,
    controllers: [
        'ApplicationController'
    ],
	
    launch: function() {
		console.log('app.js -> Application Launched');
		
		//  Viewport -> viewable application area
        Ext.create('CrisisTracker.view.Viewport');
    }
});