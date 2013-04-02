/**
 * Model for a Map Item -  This class specificies the  fields for any "MAP ITEM" data object.
  * @extends Ext.data.Model
 */
Ext.define('CrisisTracker.model.MapItem', {	
    extend: 'Ext.data.Model',	
	
    fields: [		
        {
			name: 'story', type: 'string',
			convert: function(value, record) {
				return record.raw.properties.story;
            }
		},
		
        {
			name: 'title', type: 'string',
			convert: function(value, record) {
				return record.raw.properties.title;
            }
		},
		
        {
			name: 'tags', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.tags;
            }
		},

        {
			name: 'max_growth', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.max_growth;
            }
		},

        {
			name: 'popularity', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.popularity;
            }
		},

        {
			name: 'start_time_str', type: 'string',
			convert: function(value, record) {
				return record.raw.properties.start_time_str;
            }
		},

        {
			name: 'start_time', type: 'string',
			convert: function(value, record) {
				return record.raw.properties.start_time;
            }
		},

        {
			name: 'story_id', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.story_id;
            }
		},		

        {
			name: 'category_count', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.category_count;
            }
		},	

        {
			name: 'custom_title', type: 'string', defaultValue: 'Title Unavailable',
			convert: function(value, record) {
				return record.raw.properties.custom_title;
            }
		},			

        {
			name: 'entity_count', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.entity_count;
            }
		},
		
        {
			name: 'keyword_count', type: 'int',
			convert: function(value, record) {
				return record.raw.properties.keyword_count;
            }
		},			
		
		{
			name: 'id', type: 'int'
		},
		
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
