Ext.define('CrisisTracker.view.readstories.PageContainer', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.readStoriesPageContainer', 					
	items: 
	[{	
		xtype: 'text',
		text: 'Read Stories'
	}],	
    initComponent: function() {
		console.log('VIEW.readStoriesPageContainer -> Initialized');
		this.callParent();		
	}
	
});