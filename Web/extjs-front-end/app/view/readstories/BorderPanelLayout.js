Ext.define('CrisisTracker.view.readstories.BorderPanelLayout', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.readStoriesBorderPanelLayout', 
	width: 1000,
	height: 650,
	border: false,
	layout: 'border',
	
	items: [{			
			region:'west',
			xtype: 'taggingPanel',							
			width: 300,
			id: 'west-tags',
		},{
		    id: 'read-stories-list',
			region: 'south',
			xtype: 'panel',
			items: [{xtype: 'sortererPanel'},{xtype: 'readStoriesListGridPanel'}]
		}
		// Center Region Added Programatically in *ReadStoriesController
	],
	
    initComponent: function() {
		console.log('Border Panel Layout -> Initialized');
		this.callParent();		
	}	
});