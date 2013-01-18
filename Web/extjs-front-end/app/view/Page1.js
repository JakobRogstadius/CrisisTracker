Ext.define('CrisisTracker.view.Page1', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.page1', 
	items: [
		{
			xtype: 'panel',
			html: "PAGE1 PAGE 1PAGE1",
			heigth: 800,
			width: 400
		}							
	],
	
    initComponent: function() {
		console.log('VIEW Page 1 -> Initialized');
		this.callParent();
		
	},
	
	removeArray: function() {
		this.test = null; 
	}
	
});