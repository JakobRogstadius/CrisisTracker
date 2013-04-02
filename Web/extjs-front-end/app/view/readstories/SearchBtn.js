Ext.define('CrisisTracker.view.readstories.SearchBtn', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.searchBtnPanel',
    type:  "vbox",
    align: "center",
	items: [{
		xtype: 'button',
		id: 'searchBtn',
		name: 'searchBtn',
		text: 'Apply Filters',
		scale: 'medium',
	}],

    initComponent: function() {
		this.callParent();		
	}	
});
