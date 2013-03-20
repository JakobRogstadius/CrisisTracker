Ext.define('CrisisTracker.view.readstories.TaggingPanel', {
	extend: 'Ext.panel.Panel',
	alias: 'widget.taggingPanel',
	// title: 'Tagging Panel',	
	layout: 'anchor',
	items: [
		{xtype: 'tagWhat', anchor: '100% 70%'},	// anchor: width% height%
		{xtype: 'tagWho',anchor: '100% 15%'},
		{xtype: 'tagWhen',anchor: '100% 15%'}
	],	
   
    initComponent: function() {
		this.callParent();		
	}	
});
