/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

// ------------------------------------------------------------------------------------
  
  /**
  * Vector Layer StyleMap Generator
  **/
  function generateStyleMap(type) {
    switch (type) {
    
      case "explore":
        // Style object to be used by a StyleMap object
        vector_style_explore = new OpenLayers.Style({ 
          'fillColor': '#EB0000',
          'fillOpacity': .3,
          'strokeColor': '#8C0000',
          'strokeWidth': 1,
          //'label': '${id}',   // attribute replacement 
          'pointRadius': 8
        });
        return vector_style_explore;
      break;
      
      case "tag_select":
        // Style object to be used by a StyleMap object
        vector_style_select = new OpenLayers.Style({ 
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': '${id}',   // attribute replacement 
          'fontColor': '#FFFFFF',   // dark grey
          'pointRadius': 10,      // Size of Marker
          'graphicYOffset': -25,  // shift graphic up 28 pixels
          'graphicTitle': '${id}',  // attribute replacement 
          'externalGraphic': 'resources/externalGraphic/pinpointyellow.png'
        });
        return  vector_style_select;
      break;
      
      case "tag_default":
        // Style object to be used by a StyleMap object
        vector_style_default = new OpenLayers.Style({ 
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': '${id}',   // attribute replacement 
          'fontColor': '#FFFFFF',   // dark grey
          'pointRadius': 10,      // Size of Marker
          'graphicYOffset': -25,  // shift graphic up 28 pixels
          'graphicTitle': '${id}',  // attribute replacement 
          'externalGraphic': 'resources/externalGraphic/pinpoint.png'
          //'backgroundGraphic':'resources/externalGraphic/pinpoint.png'
        });
        return vector_style_default;
      break;
      
      default:
        // Style object to be used by a StyleMap object
        vector_style_explore = new OpenLayers.Style({ 
          'fillColor': '#669933',
          'fillOpacity': .8,
          'strokeColor': '#aaee77',
          'strokeWidth': 3,
          'label': '${id}',   // attribute replacement 
          'pointRadius': 8
        });
        return vector_style_explore;
      break;    
    }
  }

    
   /**
     * Create Vector Protocol
     * 
     * @return vector_protocol {OpenLayers.Protocol}
     */ 
  function createVectorProtocol() {
    var vector_protocol = new OpenLayers.Protocol.HTTP({ 
      url: 'data_points.json',  // lot of points
      format: new OpenLayers.Format.GeoJSON({})
    }); 
    return vector_protocol;
  }
  
   /**
     * clustering strategy that will group points together if they fall within some distance of each other
     * 
   * @param inside_distance {int}
     * @return vector_strategies [{OpenLayers.Strategy.Cluster}]
     */   
  function createStrategyCluster(inside_distance){
    return vector_strategies = [new OpenLayers.Strategy.Cluster({distance:inside_distance, threshold:3})];
  }
  
   /**
     * Fixed Strategy
     * 
     * @return vector_strategies [{OpenLayers.Strategy.Fixed}]
     */   
  function createStrategyFixed() {
    return vector_strategies = [new OpenLayers.Strategy.Fixed()];
  }
  
   /**
     * Paging Strategy
     * 
   * @param value
     * @return vector_strategies [{OpenLayers.Strategy.Fixed}]
     */   
  function createStrategyPaging(value) {
    return vector_strategies = [new OpenLayers.Strategy.Paging({length: value})];
  }
  
   /**
     * Get Symbolizer "object literals" to use as Rules in StyleMap of the Vector Layer -- {OpenLayers.StyleMap}
     * 
   * @return: 'feature.id'
     */   
  function getSymbolizerLookup(type) {  
      var symbolizer_emergencytypes_lookup = {
        'fire': {
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': 'F${id}',    // attribute replacement 
          'fontColor': '#343434',   // dark grey
          'pointRadius': 10, 
          'strokeColor': '#232323',   // black
          'strokeDashstyle': 'dot',   // .....
          'strokeWidth': 2, // attribute replacement 
          'backgroundGraphic':'resources/externalGraphic/bg_green.png',
          'graphicTitle':'${timestamp_update} by ${user}',  // attribute replacement 
          'externalGraphic':'resources/externalGraphic/fire.png'
        },
        'warning': {
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': 'W${id}',    // attribute replacement 
          'fontColor': '#343434',   // dark grey
          'pointRadius': 10, 
          'strokeColor': '#232323',   // black
          'strokeDashstyle': 'dot',   // .....
          'strokeWidth': 2, // attribute replacement 
          'backgroundGraphic':'resources/externalGraphic/bg_green.png',
          'graphicTitle':'${timestamp_update} by ${user}',  // attribute replacement 
          'externalGraphic': 'resources/externalGraphic/warning.png'
        },
        'mudslide': {
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': 'M${id}',    // attribute replacement 
          'fontColor': '#343434',   // dark grey
          'pointRadius': 10, 
          'strokeColor': '#232323',   // black
          'strokeDashstyle': 'dot',   // .....
          'strokeWidth': 2, // attribute replacement 
          'backgroundGraphic':'resources/externalGraphic/bg_green.png',
          'graphicTitle':'${timestamp_update} by ${user}',  // attribute replacement 
          'externalGraphic':'resources/externalGraphic/mudslide.png'
        },
        'flood': {
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': 'A${id}',    // attribute replacement 
          'fontColor': '#343434',   // dark grey
          'pointRadius': 10, 
          'strokeColor': '#232323',   // black
          'strokeDashstyle': 'dot',   // .....
          'strokeWidth': 2, // attribute replacement 
          'backgroundGraphic':'resources/externalGraphic/bg_green.png',
          'graphicTitle':'${timestamp_update} by ${user}',  // attribute replacement 
          'externalGraphic':'resources/externalGraphic/flood.png'
        },
        'earthquake': {
          'cursor': 'pointer', 
          'fontFamily': 'sans-serif',
          'fontSize': '8px',
          'label': 'E${id}',    // attribute replacement 
          'fontColor': '#343434',   // dark grey
          'pointRadius': 10, 
          'strokeColor': '#232323',   // black
          'strokeDashstyle': 'dot',   // .....
          'strokeWidth': 2, // attribute replacement 
          'backgroundGraphic':'resources/externalGraphic/bg_green.png',
          'graphicTitle':'${timestamp_update} by ${user}',  // attribute replacement 
          'externalGraphic':'resources/externalGraphic/earthquake.png'
        }   
      }
    return  symbolizer_emergencytypes_lookup;
  } 