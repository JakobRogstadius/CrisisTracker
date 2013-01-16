/**
 * CSMap.controller
 * Used to manage map mapLayers and showing their related views
 */
Ext.define('CSMap.controller.GridMapController', {
    extend: 'Ext.app.Controller',	
	views: [
		'Map',
		'GridPanel'
	],
	
	stores: [ 'MapItems'],
	models: [ 'MapItem'],
	
    init: function() {
		// Access DOM Items and listen to their events
        this.control({
            'mappanel': {
                'beforerender': this.onMapPanelBeforeRender,
				'mapPan': this.onMapPan		
            }			
        }, this);
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
		var store = this.getMapItemsStore();
		store.load({		
			params:{
				boundingbox: currentBoundingBox
			},
			
			callback : function(records, options, success) {
				if(success) {
					var	mapPanel = Ext.ComponentQuery.query('mappanel')[0];					
					if(records) {
						mapPanel.injectPoints(records);		
					}		
				}				
			}				
		});	
	}
	
});
