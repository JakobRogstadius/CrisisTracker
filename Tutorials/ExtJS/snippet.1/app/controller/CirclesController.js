/**
 * Circles Controller
 *  Manages Events and Actions on the Circles Components
 */
Ext.define('mod1.controller.CirclesController', {
    extend: 'Ext.app.Controller',
	views: [
		'shapes.Circles',
		'layout.ControlPanel',	
		'layout.Canvas'	
	],
	
	stores: [
		'SelectedCircle'
	],
	
	models: [
		'SelectedCircle'
	],	

	// Controller Initiatization Function
    init: function() {
		
		// The control function Setups -> listeners to events on View classes & define handlers
		this.control({
			
			'controlPanel > button[action=add]': {
				click: this.onControlPanelAddCirclesClick			
			},
			'canvas > component': {
				selectedIndexChanged: this.onCircleChanges			
			},
		
		});
	},
	
	// Event Handlers	
	onControlPanelAddCirclesClick: function(button) {
		console.log('Circles have been added');		
		// Get the 'Canvas Panel' by Id via ComponentQuery
		var canvas = Ext.ComponentQuery.query('#canvas')[0];
		canvas.add(Ext.create('widget.circles'));
	},	
	
	onCircleChanges: function(circleIndex, componentIndex) {			
		// Mark all Selection Circles with Color
		var allCirclesInstances = Ext.ComponentQuery.query('circles');		
		for(var i=0; i<allCirclesInstances.length; i++) {
			if (i != componentIndex) {
				allCirclesInstances[i].setSelectedIndex(circleIndex, false);
			}
		}				
	}
	
});