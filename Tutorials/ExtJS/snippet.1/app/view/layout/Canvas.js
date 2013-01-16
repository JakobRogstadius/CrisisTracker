/**
 * View - Canvas (area to draw circles)
 */
Ext.define('mod1.view.layout.Canvas' ,{	
    extend: 'Ext.panel.Panel',
    alias: 'widget.canvas',	
	store: 'SelectedCircle',
	
	// Canvas configuration properties
	config: {
		selectedIndex: 0
	},	
	
	// Properties
	id: 'canvas',
    title: 'Circles Canvas',	
	viewBox: false,
	autoShow: true,
	autoScroll: true,
	layout: 'fit',
	
	// Initialization Method
    initComponent: function() {
        this.callParent(arguments);
		this.selectedIndex = Ext.create('mod1.store.SelectedCircle',{});
    },
	
});