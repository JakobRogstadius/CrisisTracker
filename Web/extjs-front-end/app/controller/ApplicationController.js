Ext.define('CrisisTracker.controller.ApplicationController', {
    extend: 'Ext.app.Controller',
	views: [
		'AppMenu',
	],	
	stores: [],
	models: [],
	
    init: function() {
		console.log('CONTROLLER APPLICATION - CrisisTracker.controller.ApplicationController »» Initialized');
		
		// Access DOM Items and listen to their events
        this.control({
			'appMenu > button[action=page1]': {
				click: this.onApplicationPageSwitch			
			},
			
			'appMenu > button[action=page2]': {
				click: this.onApplicationPageSwitch		
			},	

			'appMenu > button[action=page3]': {
				click: this.onApplicationPageSwitch		
			}			
        });
		
    },
	
	/**
	*  EVENT HANDLER - Switch Page Button
	**/	
    onApplicationPageSwitch: function(page, options) {
		switch(page.action) {
		
			// PAGE 1
			case "page1":	
				// Runtime Load Controller. **	call the init method manually		
				var controller = this.application.getController('Page1Controller');				
				controller.init();	
				
			break;
			
			
			// PAGE 2
			case "page2":		
				// Runtime Load Controller. **	call the init method manually		
				var controller = this.application.getController('Page2Controller');				
				controller.init();				
			break;
			
			
			// PAGE 3
			case "page3":
				console.log('page 3');
				var currentPage = viewport.getComponent(1);
				
			break;
		
		
		}
	}
});