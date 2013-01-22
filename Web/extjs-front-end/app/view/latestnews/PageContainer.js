Ext.define('CrisisTracker.view.latestnews.PageContainer', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.latestNewsPageContainer', 
	items: [
		{
			xtype: 'text',
			text: 'Latest News'
		}							
	],
	
    initComponent: function() {
		console.log('VIEW.LatestNewsPageContainer -> Initialized');
		this.callParent();
		
	}
	
});