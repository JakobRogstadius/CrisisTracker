/**
 * Application Controller - 'Grid Controller'
 * Manages Events and Actions on the Grid Component
 */ 
Ext.define('mod2.controller.GridController', {
    extend: 'Ext.app.Controller',
	views: [
		'layout.ControlPanel',
		'layout.GridPanel'
	],
	
	stores: [
		'GridAjaxProxy'
	],
	
	models: [
		'GridData'
	],	

	// Initiatization Function
    init: function() {
		
		// The control function Setups -> listeners to events on View classes & define handlers
		this.control({
			
			'controlPanel > button[action=fetch1]': {
				click: this.onControlPanelLoadAjaxStorage			
			},
			
			'controlPanel > button[action=fetch2]': {
				click: this.onControlPanelLoadAjaxStorage		
			},			
		
		});
	},
		
	// Event Handlers	
	onControlPanelLoadAjaxStorage: function(option) {
		var store = this.getStore('GridAjaxProxy');		
		var action = option.action;
		
		if(action == 'fetch1') {					
			store.load({
				params:{
					option:1
				},
				callback : function(records, options, success) {
					if(success) {
						console.log(records[0].data);
					}
                }				
			});
		}
		
		else {
			store.load({
				params:{
					option:2
				},
				callback : function(records, options, success) {
					if(success) {
						console.log(records[0].data);
					}
                }				
			});
		}	
	}
	
});