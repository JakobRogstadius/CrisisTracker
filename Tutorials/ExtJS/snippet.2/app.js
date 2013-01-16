/**
 * mod2.app
 * A MVC application demo that supports assynchronous data handling
 */
Ext.application({		
    requires: ['Ext.container.Viewport'],
    name: 'mod2',
    appFolder: 'app',
	controllers: [
		'GridController'
	],

    launch: function() {	
		//  Viewport -> viewable application area
        Ext.create('Ext.container.Viewport', {
			layout: {
				type: 'vbox',
				align: 'stretch'
			},
			height: 800,			
			width: 400,
			title: 'page',
			defaults: {bodyStyle:'padding:5px'},	
			renderTo: Ext.getBody(),			
			items: [
				{
					xtype: 'gridPanel',
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

