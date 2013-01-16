/**
 * mod1.app
 * A MVC application demo that draws Circles and defines event handlers for them
 */
Ext.application({		
    requires: ['Ext.container.Viewport'],
    name: 'mod1',	//Namespace * gloval variable -> registers the namespace to Ext.Loader
    appFolder: 'app',
	
	controllers: [
		'CirclesController'
	],

	// simple launch function
    launch: function() {
		// Tooltips
		Ext.tip.QuickTipManager.init();
	
		//  Viewport, a specialized container representing the viewable application area
        var viewport = Ext.create('Ext.container.Viewport', {
			layout: {
				type: 'vbox',
				align: 'stretch'
			},
			height: 800,			
			title: 'page',
			defaults: {bodyStyle:'padding:5px'},	
			renderTo: Ext.getBody(),			
			items: [
				{
					xtype: 'canvas',
					title: 'Canvas - Container',
					flex: 3,	// 75%
				},
				{
					xtype: 'controlPanel',
					title: 'Control Panel',
					flex: 1,	// 25%
				}
			]				
		
        });
		
    }
	
})