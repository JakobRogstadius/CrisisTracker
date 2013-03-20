Ext.define('CrisisTracker.controller.tagstories.TagStoriesController', {
    extend: 'Ext.app.Controller',
	views: [
		'tagstories.PageContainer',
		'tagstories.OrdererPanel',
		'tagstories.Href',
		'StoriesListGridPanel'		
	],
	
	models: ['StoriesListGridModel'],
	stores: ['TagStoriesProxy'],
	
	config: { initialized: false },
		
    init: function(last_controller) {
		console.log('CONTROLLER TAGSTORIES - CrisisTracker.controller.tagstories.TagStoriesController »» 1Initialized');
		
		// 1. Remove Current Page (if any)		
		var appContainer = Ext.ComponentQuery.query('appContainer')[0];			
		var currentPage = appContainer.getComponent(1);
		if(currentPage) {
			appContainer.remove(currentPage);
		}

		// 2. Draw Page on appContainer
		appContainer.add({xtype: 'tagStoriesPageContainer'});
		
		// 3. Add items to latest News Page Container
		var pageContainer = Ext.ComponentQuery.query('tagStoriesPageContainer')[0];	
		pageContainer.add({xtype: 'ordererPanel'});
		pageContainer.add({xtype: 'storiesListGridPanel', id: "tagstories", store: this.application.getStore('TagStoriesProxy')});
		
		if(!this.config.initialized) {		
			this.config.initialized = true;	
			// 4. Listening and Handling Events from the VIEWS under responsability of this Controller		
			this.control({				
				'storiesListGridPanel[id=tagstories]': {
					itemclick: this.onGridPanelItemClick,			
				},
				
				'href':  {
					click: this.onHrefClick,					
				}			
			});			
		}
	},
	
	// Event handlers
	onGridPanelItemClick: function (grid, record, item, index, e, eOpts) {		
		console.log(record.data);
	},	
	
	onHrefClick: function (data) {			
		// Enable Clicked href
		data.activeTag();
		// Disable all others
		var hrefsArray = Ext.ComponentQuery.query('href');
		for(var i=0; i<hrefsArray.length; i++){
			if(hrefsArray[i].id != data.id) {
				hrefsArray[i].inactiveTag();
			}
		}
		
		var gridStore = Ext.ComponentQuery.query('storiesListGridPanel')[0].getStore();
			
		// Actions
		switch(data.id){			
			
			case ("Newest"):
				gridStore.proxy.extraParams = { sortorder:'newest'};
				gridStore.load();				
			break;	
			case ("Hidden"):
				gridStore.proxy.extraParams = { sortorder:'hidden'};
				gridStore.load();						
			break;				
			case ("Trending"):
				gridStore.proxy.extraParams = { sortorder:'trending'};
				gridStore.load();					
			break;
			default:
			
			break;		
		}
		

	},
});	
