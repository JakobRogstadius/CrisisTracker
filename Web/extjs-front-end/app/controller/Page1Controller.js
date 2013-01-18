Ext.define('CrisisTracker.controller.Page1Controller', {
    extend: 'Ext.app.Controller',
	views: [
		'Page1'
	],	
	
    init: function(last_controller) {
		console.log('CONTROLLER PAGE 1 - CrisisTracker.controller.Page1 »» Initialized');
		
		// 1. Remove Current Page (if any)		
		var viewport = Ext.ComponentQuery.query('viewport')[0];		
		var currentPage = viewport.getComponent(1);
		if(currentPage) {
			viewport.remove(currentPage);
		}			
	
		// 2. Draw Page on Viewport
		viewport.add({xtype: 'page1'});		

		// @TODO
		// 3. Remove Control Permissions over DOM of last page controller
		// if(last_controller){
			// this.application.eventbus.uncontrol([last_controller.id]);	
		// }		
    },
	
	function2: function() {
	
	}
	
	
});