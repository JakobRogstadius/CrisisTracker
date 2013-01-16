/**
 * Ext.Loader
 * Ext.Loader is the heart of the new dynamic dependency loading capability in Ext JS 4+
 */
Ext.Loader.setConfig({
    enabled: true,
    disableCaching: false,
	// mapping from namespaces to file paths
    paths: {
        GeoExt: "GeoExt",
	    //Ext: "ext-4.1.1a/src"
    }
});

/**
 * CSMap.app
 * A MVC application demo that uses GeoExt and Ext components to display
 * geospatial data.
 */
Ext.application({
    name: 'CSMap',	// Namespace
    appFolder: 'app',
    controllers: [
        'GridMapController'
    ],
	
    launch: function() {
		//  Viewport -> viewable application area
        Ext.create('Ext.container.Viewport', {
			title: 'page',
			layout: 'fit',
			defaults: {bodyStyle:'padding:5px'},		
			renderTo: Ext.getBody(),						
			items: [{
				xtype: 'panel',
				border: false,
				layout: 'border',
				items: [{
						title: 'LIST',
						region: 'south',
						xtype: 'gridPanel',
						height: 150
					},{
						title: 'TAGGING REGION',
						region:'west',
						xtype: 'panel',							
						width: 300,
						id: 'west-region-container',
						layout: 'fit'
					},{
						title: 'MAP REGION',
						xtype: 'mappanel',
						region: 'center'			
					},{
						title: 'MENU REGION',
						region: 'north', 
						xtype: 'panel',
						layout: 'fit',
						height: 50,					
				}]
			}]	
        });
    }
});