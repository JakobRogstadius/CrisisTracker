/**
 * View - shapes.Circles 
 */
Ext.define('mod1.view.shapes.Circles' ,{	
	alias: ['widget.circles'],
	maxHeight : 150,
	resizable: true,
	extend: 'Ext.draw.Component',	

	// Mixes in functionality from the following classes...
	mixins: {		
		observable: 'Ext.util.Observable'
	},
	
	
	// Class Properties
	config: {
		my_index: 0,
		selected_index: null
	},
	
	statics: {
		unique_index: 0
	},
		
		
	// Class Constructor
	constructor: function(config){		
		this.callParent();
		this.initConfig(config);

		this.statics().unique_index++;
		this.my_index = this.statics().unique_index;		
		
		this.addEvents('selectedIndexChanged');		
		this.child_circles = [];
	},		
	
	
	// Class Mehods
	/**
	 *	Selects a Circle in this VIEW object instance via it's my_index
	 *	@params my_index, propagate		
	*/
	setSelectedIndex: function(my_index, propagate) {

		// 1. Already Selected
		if (my_index == null || my_index == this.selected_index){
			return;	// force return
		}				
				
		// 2. New Selection
		this.child_circles[my_index].setAttributes({fill: '#ff0000'}, true);	// Set RED to new Circle
		
		if (this.selected_index!=null) {	// first selection prevention
			this.child_circles[this.selected_index].setAttributes({fill: '#3F89BB'}, true);		// Set BLUE	to last Selected Circle
		}				
		// Update Last Selected Circle
		this.selected_index = my_index;				
				
		// Event Progapation Control
		if (propagate) {
			// @params: circleIndex, componentIndex
			this.fireEvent("selectedIndexChanged", my_index, this.index);
		}
	},
	

	/**
	 *	Draws 2 Circles on this VIEW instance 'surface'
	 *	@params my_index, propagate		
	*/	
	drawCircles: function() {			
		this.child_circles.push(this.surface.add(		// Note: 'surface.add' returns a circle as a "Ext.draw.Sprite" object 
		{
			type: 'circle',
			fill: '#3F89BB',
			radius: 50,
			x: 100,		// Note: parent width = 400
			y: 50,      	
			circle_id: 0
		}).show(true));
		
		this.child_circles.push(this.surface.add(	
		{
			type: 'circle',
			fill: '#3F89BB',
			radius: 50,
			x: 300,		
			y: 50,          	
			circle_id: 1
		}).show(true));	
	},	
	
	// Event Handlers
	listeners: {
		click: function(e,t,o) {			
			var component_position = this.getPosition();
            var pos_x  = e.getPageX() - component_position[0];
            var pos_y  = e.getPageY() - component_position[1];
			
			for(var i=0; i<this.child_circles.length; i++) {
				var distance = Math.sqrt((pos_x - this.child_circles[i].x)*(pos_x - this.child_circles[i].x)
				  + (pos_y - this.child_circles[i].y)*(pos_y - this.child_circles[i].y));
				  
				if (distance <  this.child_circles[i].radius ) {				
					this.fireEvent("selectedIndexChanged", i, this.id );
					break;
				}
			}			
		},
				
		afterrender: function() {			
			// Now 'surface' is available to draw over it
			this.drawCircles();
		},

		// Custom Created Event
		selected_index_changed: function() {
			console.log('event handleddd ' + 'selected_index_changed by ' + this.id);
		}		
	}
});