/**
 * Model for a Map Item -  This class specificies the  fields for any "MAP ITEM" data object.
  * @extends Ext.data.Model
 */
Ext.define('CSMap.model.MapItem', {	
    extend: 'Ext.data.Model',	
	
    fields: [
		
        {name: 'story', type: 'string',
			convert: function(value, record) {
				return record.raw.properties.story;
            }
		},
		
        {name: 'tags', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.tags;
            }
		},
		
		{name: 'id', type: 'int'},
		
        {
            name: 'lon',
            convert: function(value, record) {					
                    return record.raw.geometry.coordinates[1];
            }
        },
        {
            name: 'lat',
            convert: function(value, record) {				
                    return record.raw.geometry.coordinates[0];
            }
        }
		
    ]
});
