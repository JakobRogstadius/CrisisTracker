Ext.define('CrisisTracker.view.AppContainer', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.appContainer', 
	defaults: {bodyStyle:'padding:5px'},	
	layout: {
		type: 'auto',
	},			
	items: [
		{
			xtype: 'appMenu'
		}			
	],
    initComponent: function() {
		console.log('APP CONTAINER VIEW -> Initialized');		
		this.callParent();
	}
	
});