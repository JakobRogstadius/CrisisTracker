/**
 * view.ControlPanel (buttons)
 */ 
Ext.define('mod2.view.layout.ControlPanel' ,{	
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
	
	// Panel Items
	items: [
		{
			xtype: 'button',
			text: 'Fecth Data Option 1',
			scale: 'medium',
			action: 'fetch1',
			tooltip: 'Click here to Fetch',
		},
		{
			xtype: 'button',
			text: 'Fecth Data Option 2',
			scale: 'medium',
			action: 'fetch2',
			tooltip: 'Click here to Fetch',
		}		
	]
});