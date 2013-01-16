/**
 * View - Control Panel (buttons)
 */
Ext.define('mod1.view.layout.ControlPanel' ,{	
    extend: 'Ext.panel.Panel',
    alias: 'widget.controlPanel',	

	// Properties
	id: 'controlPanel',
    title: 'Control Panel',	
	viewBox: false,
	autoShow: true,
	autoScroll: true,
	layout: 'auto',	
	manageOverflow :1,	
	
	// Configuration properties
	config: {

	},	
	
	// Panel Items
	items: [
		{
			xtype: 'button',
			text: 'Add Circles',
			scale: 'small',
			action: 'add',
			tooltip: 'Click here to Add Circles',
		}
	]
});