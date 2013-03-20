Ext.define('CrisisTracker.view.tagstories.OrdererPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.ordererPanel', 
	height: 30,
	bodyPadding: 5,
	buttonAlign: 'left'	,
	layout: {
		type: 'hbox',
		pack: 'start',
		align: 'stretch'
	},	
	items: [
		{
			xtype: 'href',	
			id: 'Newest',	
			autoEl: {
				tag: 'a',
				href: '#',
				html: 'Newest'
			},
		},		
		{
			xtype: 'text',
			text: '|'
		},			
		{
			xtype: 'href',	
			id: 'Hidden',	
			autoEl: {
				tag: 'a',
				href: '#',
				html: 'Hidden'
			},
		},		
		{
			xtype: 'text',
			text: '|'
		},
		{
			xtype: 'href',	
			id: 'Trending',	
			autoEl: {
				tag: 'a',
				href: '#',
				html: 'Trending'
			},
		},		
		{
			xtype: 'text',
			text: '|'
		},		
	],
	
    initComponent: function() {
		console.log('VIEW.Orderer Panel -> Initialized');
		this.callParent();	
	}
});