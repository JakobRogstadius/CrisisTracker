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
			text: 'page 1',
			action: 'page1'
		}, 
		{
			xtype: 'button',					
			text: 'page 2',
			action: 'page2'			
		}, 
		{
			xtype: 'button',					
			text: 'page 3',
			action: 'page3'			
		} 								
	],	

    initComponent: function() {
		console.log('MENU - CrisisTracker.view.Menu.JS -> Initialized');
		this.callParent();
	}
	
});