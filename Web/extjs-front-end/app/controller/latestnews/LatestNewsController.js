Ext.define('CrisisTracker.controller.latestnews.LatestNewsController', {
    extend: 'Ext.app.Controller',
	views: [
		'latestnews.PageContainer',
		'latestnews.FiltersPanel',
		'StoriesListGridPanel'
	],	
	
	models: ['StoriesListGridModel'],	
	stores: ['LatestNewsProxy'],
	
	config: { initialized: false },
	
    init: function(last_controller) {		
		console.log('CONTROLLER LATESTNEWS - CrisisTracker.controller.lastestnews.LastestNewsController »» Initialized');
		
		// 1. Remove Current Page (if any)		
		var appContainer = Ext.ComponentQuery.query('appContainer')[0];	
		
		var currentPage = appContainer.getComponent(1);
		if(currentPage) {
			appContainer.remove(currentPage);
		}			
	
		// 2. Draw Page on appContainer
		appContainer.add({xtype: 'latestNewsPageContainer'});
		
		// 3. Add items to latest News Page Container
		var pageContainer = Ext.ComponentQuery.query('latestNewsPageContainer')[0];	
		pageContainer.add({xtype: 'latestNewsFiltersPanel'});
		pageContainer.add({xtype: 'storiesListGridPanel', id: "latestnews", store: this.application.getStore('LatestNewsProxy')});	
				
		if(!this.config.initialized) {		
			this.config.initialized = true;	
			// 4. Listening and Handling Events from the VIEWS under responsability of this Controller		
			this.control({
				
				'storiesListGridPanel[id=latestnews]': {
					itemclick: this.onGridPanelItemClick,	
				},
				
				'latestNewsFiltersPanel > button[action=update]':  {
					click: this.onUpdateButtonClick		
				}
			});	
		}		
    },

	
	// Event handlers
	onGridPanelItemClick: function (grid, record, item, index, e, eOpts) {	
		console.log(record.data);
	},
	
	onUpdateButtonClick: function(btn, e, eOpts) {
		console.log('clicked on button');
		
		var textFields = Ext.ComponentQuery.query('latestNewsFiltersPanel textfield');
		console.log(textFields[1].getValue());
		
		var radioButton = Ext.ComponentQuery.query('latestNewsFiltersPanel radiogroup')[0];
		console.log(radioButton.getValue());
		
		// Load Server data via Store (proxy)	
		var gridStore = this.getStore('LatestNewsProxy');
		gridStore.load({
			params:{
				stories: textFields[0].getValue(),
				hours: textFields[1].getValue(),
				words: textFields[2].getValue(),
				containing: radioButton.getValue().wordsFilter
			},
			
			callback : function(records, options, success) {
				if(success) {
					console.log(records[0].data);
				}
			}				
		});
	}

	
	
});