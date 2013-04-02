Ext.define('CrisisTracker.controller.readstories.ReadStoriesController', {
    extend: 'Ext.app.Controller',
	views: [	
		'readstories.PageContainer',
		'readstories.BorderPanelLayout',
		'readstories.ListGridPanel',
		'readstories.SortererPanel',
		'readstories.TaggingPanel',
		'readstories.TagWhat',		
		'readstories.TagWho',		
		'readstories.TagWhen',		
		'readstories.Map',		
		'readstories.SearchBtn',
	],	
	
	models: ['StoriesListGridModel',
			'MapItem'],	
			
	stores: ['ReadStoriesProxy',
			'SortererCombo',
			'MapItems',
			'WhatTagData'],
	
	config: { initialized: false },
	
    init: function(last_controller) {		
		
		// 1. Remove Current Page (if any)		
		var appContainer = Ext.ComponentQuery.query('appContainer')[0];			
		var currentPage = appContainer.getComponent(1);
		if(currentPage) {
			appContainer.remove(currentPage);
		}
	
		// 2. Draw Page on appContainer
		appContainer.add({xtype: 'readStoriesPageContainer'});
		
		// 3. Add items to latest News Page Container
		var pageContainer = Ext.ComponentQuery.query('readStoriesPageContainer')[0];		
		pageContainer.add({xtype: 'readStoriesBorderPanelLayout'});
		
		// 3.1 Add map to the border layout
		var borderLayout = Ext.ComponentQuery.query('readStoriesBorderPanelLayout')[0];
		borderLayout.add({xtype:'mapPanel', title: 'WHERE',region: 'center', listeners: {
			beforerender: this.onMapPanelBeforeRender}});	// map_rendering listener must be set here (strange bug)
		
		if(!this.config.initialized) {		
			console.log("first run");
			this.config.initialized = true;	
			// 4. Listening and Handling Events from the VIEWS under responsability of this Controller
			this.control({								
				'searchBtnPanel > button': {
					click: this.onSearchBtnClick
				},
				
				'sortererPanel': {					
					select: this.comboBoxItemSelect
				},
				
				'mapPanel': {
					'beforerender': this.onMapPanelBeforeRender,
					'mapPan': this.onMapPan		
				}
				
			});	
		}			
    },

	
	onSearchBtnClick: function(args) {		
		// Data Containers		
		var buttons = new Array();
		var whoKeywords = new Array();
		var whatKeywords = new Array();
		var whenFrom;
		var whenTo;
		var boundingBox;

		// Load all the pressed buttons		
		var	whatBtnTagsLeftPanel = Ext.ComponentQuery.query('#tagButtonsPanelLeft')[0];	
		var	whatBtnTagsRightPanel = Ext.ComponentQuery.query('#tagButtonsPanelRight')[0];		
		var buttonsLeft = whatBtnTagsLeftPanel.query('button');
		var buttonsRight = whatBtnTagsRightPanel.query('button');
		
		Ext.each(buttonsLeft, function(button) {
			if(button.pressed) {
				buttons.push(button.text);
			}
		});
		Ext.each(buttonsRight, function(button) {
			if(button.pressed) {
				buttons.push(button.text);
			}
		});		
		
		// Load all Keywords
		var what = Ext.ComponentQuery.query('tagWhat > textfield')[0];
		if (what.edited) {
			whatKeywords = what.value.split(",");
		}
		var	who = Ext.ComponentQuery.query('tagWho > textfield')[0];
		if (who.edited) {
			whoKeywords = who.value.split(",");
		}
		
		// Load Dates
		var	whenFromRaw = Ext.ComponentQuery.query('tagWhen > datefield')[0];
		if (whenFromRaw.edited) {
			whenFrom = whenFromRaw.rawValue;
		}		
		var	whenToRaw = Ext.ComponentQuery.query('tagWhen > datefield')[1];
		if (whenToRaw.edited) {
			whenTo = whenToRaw.rawValue;
		}
		
		// Load BoundingBox
		var mapPanel = Ext.ComponentQuery.query('mapPanel')[0];
		var bbox = mapPanel.getCurrentBoundingBox();

		// Force Load (w/ search parameters) on Grid/Map Store	
		var store = Ext.getStore('ReadStoriesProxy'); // class name	
		store.load({
			params: {
				boundingbox: bbox,
				whatcategories: buttons,
				what: whatKeywords,
				who: whoKeywords,
				from: whenFrom,
				to: whenTo			
			},
			callback : function(records, options, success) {
				if(success) {
					var	mapPanel = Ext.ComponentQuery.query('mapPanel')[0];	
					console.log(mapPanel);					
					if(records) {
						mapPanel.injectPoints(records);		
					}		
				}				
			}			
		});
		
	},
		
	/**
	*  EVENT HANDLER - Add Layers to the Map before Map is rendered in ExtJS
	**/	
    onMapPanelBeforeRender: function(mapPanel, options) {

		// Map Layers
        var mapLayers = new Array();
		var google_satellite = new OpenLayers.Layer.Google("Google Satellite",{type: google.maps.MapTypeId.SATELLITE, numZoomLevels: 16});mapLayers.push(google_satellite);

        // Point Colors
        var pointColors = {
            getColor: function(feature) {
                if (feature.attributes.tags < 2000) {
                    return 'green';
                }
                if (feature.attributes.tags < 2300) {
                    return 'red';
                }
                return 'yellow';	//default
            }
        };
		
		// Simple Vector Layer Style Template
        var template = {
            cursor: "pointer",
            fillOpacity: 0.8,
            fillColor: "${getColor}",
            pointRadius: 5,
            strokeWidth: 1,
            strokeOpacity: 1,
            strokeColor: "${getColor}"
        };
		
        var style = new OpenLayers.Style(template, {context: pointColors});	//@params: style, options
		
		
		// Vector Layer
        var vectorLayer = new OpenLayers.Layer.Vector("vector", {
            styleMap: new OpenLayers.StyleMap({
                'default': style
            })
        });					
        mapLayers.push(vectorLayer);
        mapPanel.map.addLayers(mapLayers);

		
        // Controls
        mapPanel.map.addControls([
			new OpenLayers.Control.MousePosition(),
			new OpenLayers.Control.LayerSwitcher({'ascending':false})
		]);		
		mapPanel.map.zoomToMaxExtent();		
    },	
	
	
	/**
	*  EVENT HANDLER - Call API with the bounding box values	
	**/	
	onMapPan :function(map,currentBoundingBox) {	
		console.log('onMapPan');
		var store = this.getReadStoriesProxyStore();
		store.load({		
			params:{
				boundingbox: currentBoundingBox
			},
			
			callback : function(records, options, success) {
				if(success) {
					var	mapPanel = Ext.ComponentQuery.query('mapPanel')[0];	
					console.log(mapPanel);					
					if(records) {
						mapPanel.injectPoints(records);		
					}		
				}				
			}				
		});	
	},
	
	
	// Event handlers
	comboBoxItemSelect: function (combo, records, eOpts) {	
		console.log(records);
	},
	

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
