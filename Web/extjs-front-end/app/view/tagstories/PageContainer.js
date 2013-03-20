Ext.define('CrisisTracker.view.tagstories.PageContainer', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.tagStoriesPageContainer',
	items: [
		{
			xtype: 'text',
			text: 'Tag Stories'
		}							
	],
	
    initComponent: function() {
		console.log('VIEW.tagStoriesPageContainer -> Initialized');
		this.callParent();
		
	}
	
});