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
		'readstories.Map'		
	],	
	
	models: ['StoriesListGridModel'],	
	stores: ['ReadStoriesProxy',
			'SortererCombo'],
	
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
			
				'sortererPanel': {
					beforeshow: this.borderLayoutRendered,
					select: this.comboBoxItemSelect
				},
				
				'mapPanel': {
					beforerender: this.onMapPanelBeforeRender,
					mapPan: this.onMapPan		
				}
			});	
		}			
    },

	
	borderLayoutRendered: function(args) {
		console.log('borderLayoutRendered');
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
			
			//@ OPTIONAL: The following code defines an automatic HTTP feeder for the Map Vector Layer, which fetches new data when bounding box changes according to user defined ratio.
            // protocol: new OpenLayers.Protocol.HTTP({
                // url: "data/features.php",
                // format: new OpenLayers.Format.GeoJSON()
            // }),
			// strategies: [new OpenLayers.Strategy.BBOX({ratio: 0.5})],
        });
		
		//@ TODO
		// Bind an ExtJS store to update automatically the Map vector layer
        // myContext.getMapItemsStore().bind(vectorLayer, {'initDir': 'STORE_TO_LAYER'});
		
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
		// var store = this.getMapItemsStore();
		// store.load({		
			// params:{
				// boundingbox: currentBoundingBox
			// },
			
			// callback : function(records, options, success) {
				// if(success) {
					// var	mapPanel = Ext.ComponentQuery.query('mappanel')[0];					
					// if(records) {
						// mapPanel.injectPoints(records);		
					// }		
				// }				
			// }				
		// });	
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
