Ext.define('CrisisTracker.view.readstories.TaggingPanel', {
	extend: 'Ext.panel.Panel',
	alias: 'widget.taggingPanel',
	// title: 'Tagging Panel',	
	layout: 'anchor',
	items: [
		{xtype: 'tagWhat', anchor: '100% 60%'},	// anchor: width% height%
		{xtype: 'tagWho',anchor: '100% 15%'},
		{xtype: 'tagWhen',anchor: '100% 17.5%'},
		{xtype: 'searchBtnPanel',anchor: '100% 7.5%'},
	],	
   
    initComponent: function() {
		this.callParent();		
	}	
});
