/**
* Tweak - Allow uncontrol of controllers
 */
Ext.override(Ext.app.EventBus, {
    uncontrol: function(controllerArray) {
		console.log('unloading controller');
		console.log(controllerArray);
        var me  = this,
            bus = me.bus,
            deleteThis, idx;

        Ext.iterate(bus, function(ev, controllers) {
            Ext.iterate(controllers, function(query, controller) {
                deleteThis = false;

                Ext.iterate(controller, function(controlName) {
                    idx = controllerArray.indexOf(controlName);

                    if (idx >= 0) {
                        deleteThis = true;
                    }
                });

                if (deleteThis) {
                    delete controllers[query];
					console.log('deleted controller');
                }
            });
        });
    }
});
 

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
	    //Ext: "ext-4/src"
    }	
});



/**
 * CrisisTracker.app
 */
Ext.application({
    name: 'CrisisTracker',	// Namespace
    appFolder: 'app',
	// autoDestroy: true,
    controllers: [
        'ApplicationController'
    ],	
	
	views: [
		'AppMenu',
		'AppContainer'
	],
	
	requires: [
		'CrisisTracker.controller.latestnews.LatestNewsController',
		'CrisisTracker.controller.Page2Controller'
	],
	
    launch: function() {
		console.log('app.js -> Application Launched');
        var pageContainer = Ext.create('widget.appContainer', {
			renderTo: Ext.getDom('content'),	// manually specify DOM element to place the container
        });
    }
});

