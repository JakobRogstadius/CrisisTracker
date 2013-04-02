Ext.define('CrisisTracker.view.readstories.TagWho', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.tagWho',
	title: 'WHO',
	layout: 'anchor',
	items: [{
		xtype: 'textfield',
		anchor: '100% 100%',
		id: 'whoTextfield',
		name: 'whoTextfield',
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
	}],

    initComponent: function(config) {
		this.initConfig(config);
		this.callParent([config]);	
	}	
});
