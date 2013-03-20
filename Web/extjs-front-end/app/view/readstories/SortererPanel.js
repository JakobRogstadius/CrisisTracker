Ext.define('CrisisTracker.view.readstories.SortererPanel', {
    extend: 'Ext.form.ComboBox',
    alias: 'widget.sortererPanel', 
	height: 50,
	fieldLabel: 'Sort Order',
    store: 'SortererCombo',
    queryMode: 'local',
    displayField: 'name',
    valueField: 'abbr',
	displayTpl: Ext.create('Ext.XTemplate',
			'<tpl for=".">',
				'{abbr} - {name}',
			'</tpl>'
		),	
    initComponent: function() {
		this.callParent();
		
	}	
});