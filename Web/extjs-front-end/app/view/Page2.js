Ext.define('CrisisTracker.view.Page2', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.page2', 
	items: [
		{
			xtype: 'panel',
			html: "PAGE2 PAGE 2PAGE2",
			heigth: 800,
			width: 400
		}							
	],	

    initComponent: function() {
		console.log('VIEW Page 2 -> Initialized');		
		this.callParent();
	}
	
});