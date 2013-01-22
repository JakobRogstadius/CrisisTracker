Ext.define('CrisisTracker.view.latestnews.FiltersPanel', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.latestNewsFiltersPanel', 
	height: 30,
	bodyPadding: 5,
	buttonAlign: 'left'	,
	layout: {
		type: 'hbox',
		pack: 'start',
		align: 'stretch'
	},	
	items: [
		{
			xtype: 'text',
			text: 'Show',	
		},
		
		{		
			xtype: 'textfield',
			id: 'stories',
			width: 40,
			// Add specialkey listener
			initComponent: function() {
				this.callParent();
				this.on('specialkey', this.checkEnterKey, this);
			},	

			// Handle enter key presses, execute the search if the field has a value
			checkEnterKey: function(field, e) {
				var value = this.getValue();
				if (e.getKey() === e.ENTER && !Ext.isEmpty(value)) {
					location.href = Ext.String.format(this.searchUrl, value);
				}
			}
		},
		
		{
			xtype: 'text',
			text: 'stories from the past',			
		},		
		
		{
			xtype: 'textfield',
			id: 'hours',
			width: 40,
			
			// Add specialkey listener
			initComponent: function() {
				this.callParent();
				this.on('specialkey', this.checkEnterKey, this);
			},	

			// Handle enter key presses, execute the search if the field has a value
			checkEnterKey: function(field, e) {
				var value = this.getValue();
				if (e.getKey() === e.ENTER && !Ext.isEmpty(value)) {
					location.href = Ext.String.format(this.searchUrl, value);
				}
			}
		},
		
		{
			xtype: 'text',
			text: 'hours, containing',			
		},			
		
		{
			xtype: 'radiogroup',
			id: 'wordsFilter',
			width: 100,
			items: [
				{ boxLabel: 'ALL', name: 'wordsFilter', inputValue: '0', checked: true, flex: 1, boxLabelCls: 'x-form-cb-label' },
				{ boxLabel: 'ANY', name: 'wordsFilter', inputValue: '1', flex: 2, boxLabelCls: 'x-form-cb-label'},
			]

		},
		{
			xtype: 'text',
			text: 'of the words',			
		},				
		
		{
			xtype: 'textfield',
			id: 'words',
			fieldLabel: '',
			// Add specialkey listener
			initComponent: function() {
				this.callParent();
				this.on('specialkey', this.checkEnterKey, this);
			},	

			// Handle enter key presses, execute the search if the field has a value
			checkEnterKey: function(field, e) {
				var value = this.getValue();
				if (e.getKey() === e.ENTER && !Ext.isEmpty(value)) {
					location.href = Ext.String.format(this.searchUrl, value);
				}
			}
		},
		
		{
			xtype: 'button',
			action: 'update',
			text: 'Click to Update'		
		}		
	],
	
    initComponent: function() {
		console.log('VIEW.LatestNewsPageContainer.FiltersPanel -> Initialized');
		this.callParent();
		
	}
	
});