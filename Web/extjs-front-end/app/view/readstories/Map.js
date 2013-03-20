/**
 * The GeoExt.panel.Map used in the application.  Useful to define map options  and stuff.
 * @extends GeoExt.panel.Map
 */
Ext.define('CrisisTracker.view.readstories.Map', {

    // 'Ext.panel.Panel' specific options (ExtJS):
    extend: 'GeoExt.panel.Map',
    alias : 'widget.mapPanel',
    requires: [
        'Ext.window.MessageBox',
        'GeoExt.Action',
    ],
    border: 'false',
    layout: 'fit',
    width: 400,
	
    // 'GeoExt.panel.Map' specific options (OpenLayers) :    
	units: 'm', 
	sphericalMercator: true,
	maxExtent: new OpenLayers.Bounds(  // Explicitly Set 'Boundaries of the World'  -- EPSP:900913
			-128 * 156543.0339,
			-128 * 156543.0339,
			128 * 156543.0339,
			128 * 156543.0339),
	maxResolution: 156543.0339,
	projection: new OpenLayers.Projection('EPSG:900913'), 
	displayProjection: new OpenLayers.Projection('EPSG:900913'),  // User see this  
	
    initComponent: function() {
		console.log('Map Created');
		// ExtJS Event s
		this.addEvents('mapPan');	

		// OpenLayers Map
        this.map = new OpenLayers.Map();		
		this.map.panel = this;		
		// OpenLayers Events
		this.map.events.register('moveend', this.map, this.mapMoveHandler); 
                
        this.callParent(arguments);
    },
	
	
	
	/**
	 * OpenLayers Map - 'mapMoveHandler' Event Handler
	 */	
	mapMoveHandler: function () {
		var currentBoundingBox = this.getExtent().toArray(); // left, bottom, right, top								
		// ExtJS Event Firing
		this.panel.fireEvent("mapPan", this, currentBoundingBox);
	},	
	
	
	
	/**
	 * OpenLayers Map - 'mapMoveHandler' Event Handler
	 */	
	injectPoints: function(records) {		
		console.log('injecting points on vector layer');	
		// 0. Remove all Features
		this.map.getLayersByName('vector')[0].removeAllFeatures();
		
		// 1. Create Features (points)
		var features = [];
		for (var i=0; i<records.length; i++) {			
			tempPointFeature = new OpenLayers.Feature.Vector(new OpenLayers.Geometry.Point(records[i].data.lon, records[i].data.lon),null,null);			
			features.push(tempPointFeature);
		}		
		
		// 2. Add features to vector layer		
		this.map.getLayersByName('vector')[0].addFeatures(features);
		this.map.getLayersByName('vector')[0].redraw();	
	},
			
});