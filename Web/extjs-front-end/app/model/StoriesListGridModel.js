/**
 * Latest News Model *JSON
 */ 
Ext.define('CrisisTracker.model.StoriesListGridModel', {
    extend: 'Ext.data.Model',
    fields: [
        {name: 'story_id',  type: 'int'},
        {name: 'start_time',   type: 'string'},
        {name: 'start_time_str', type: 'string'},
        {name: 'popularity', type: 'int'},
		{name: 'max_growth', type: 'int'},
		{name: 'title', type: 'string'},
		{name: 'custom_title', type: 'string', defaultValue: 'Title Unavailable'},
		{name: 'category_count', type: 'int'},
		{name: 'entity_count', type: 'int'},
		{name: 'keyword_count', type: 'int'}		
    ]
});
