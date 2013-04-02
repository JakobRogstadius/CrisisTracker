Ext.define('CrisisTracker.view.AppMenu', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.appMenu', 
	layout: {
		type: 'hbox',
		align: 'stretch'
	},
	items: [
		{
			xtype: 'button',			
			text: 'Latest News',
			action: 'page1'
		}, 
		{
			xtype: 'button',					
			text: 'Tag Stories',
			action: 'page2'			
		}, 
		{
			xtype: 'button',					
			text: 'Read Stories',
			action: 'page3'			
		}	
	],	

    initComponent: function() {
		console.log('MENU - CrisisTracker.view.Menu.JS -> Initialized');
		this.callParent();
	}
	
});