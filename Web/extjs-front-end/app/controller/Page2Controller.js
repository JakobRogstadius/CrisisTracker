Ext.define('CrisisTracker.controller.Page2Controller', {
    extend: 'Ext.app.Controller',
	views: [
		'Page2'
	],	
	
    init: function(last_controller) {
		console.log('CONTROLLER PAGE 2 - CrisisTracker.controller.Page2Controller »» Initialized');
		
		// 1. Remove Current Page (if any)		
		var viewport = Ext.ComponentQuery.query('viewport')[0];		
		var currentPage = viewport.getComponent(1);
		if(currentPage) {
			viewport.remove(currentPage);		
		}
		console.log(viewport);
		
		// 2. Draw Page2 on Viewport
		viewport.add({xtype: 'page2'});	
		console.log(this);	

		// @TODO
		// 3. Remove Control Permissions over DOM of last page controller
		// if(last_controller){
			// this.application.eventbus.uncontrol([last_controller.id]);	
		// }	
    },
	
	function2: function() {
	
	}
	

});