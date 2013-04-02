Ext.define('CrisisTracker.view.readstories.TagWhen', {
    extend: 'Ext.form.Panel',
    alias: 'widget.tagWhen',
	width: 300,
	title: 'WHEN',
	layout: {
		type: 'vbox',
		align: 'stretch'
	},		
	
    items: [{
        xtype: 'datefield',
        fieldLabel: 'From',
        name: 'dateFrom',
		width: 250,
        format: 'm-d-Y',
		edited: false,
		listeners: {
			change: function(newValue, oldValue, eOpt) {	
				this.edited = true;
			}
		}},
		{
        xtype: 'datefield',
        fieldLabel: 'To',
        name: 'dateTo',
		width: 250,
        format: 'm-d-Y',
		edited: false,
		listeners: {
			change: function(newValue, oldValue, eOpt) {	
				this.edited = true;
			}
		}}
    ],

    initComponent: function() {
		this.callParent();	
			
	}	
});
