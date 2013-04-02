Ext.define('CrisisTracker.view.readstories.TagWhat', {
	extend: 'Ext.panel.Panel',
	title: 'WHAT',
	alias: 'widget.tagWhat',
	layout: 'anchor',
	items: [	
		{	
			xtype: 'panel',
			id:'tagButtonsPanel',
			anchor: '100% 85%',		
			layout: 'hbox',
			align: 'stretch',
			items: 
			[{
				xtype: 'panel',
				layout: 'vbox',
				align : 'stretch',
				id:'tagButtonsPanelLeft',
				flex: 1
			},{
				xtype: 'panel',
				layout: 'vbox',
				align : 'stretch',				
				id:'tagButtonsPanelRight',
				flex: 1					
			}]
		},
		
		{
			xtype: 'textfield',
			id: 'whatTextfield',
			anchor: '100% 15%',
			height: 10,
			name: 'whatTextfield',
			value: 'Enter keyword',
			edited: false,
			// Remove text once
			listeners: {
				focus: function() {	
					if(this.value == 'Enter keyword') {
						this.setValue("");
						this.edited = true;
					}
				}
			}		
		},	
	],
    initComponent: function() {
		this.callParent();	
		this.center();
		
		var left = this.query('#tagButtonsPanelLeft')[0];
		var right = this.query('#tagButtonsPanelRight')[0];			
		var store = Ext.getStore('WhatTagData'); // class name
	
		store.load(function(records) {		
			Ext.each(records, function(record) {
				//Add a button for each record
				left.add({
					xtype: 'button',
					scale: 'medium',
					width: 150,
					enableToggle: true,
					pressed: false,						
					text: record.data.left
				});
				right.add({
					xtype: 'button',
					scale: 'medium',
					width: 150,
					enableToggle: true,
					pressed: false,						
					text: record.data.right
				});				
			});
		});	
	}

});
