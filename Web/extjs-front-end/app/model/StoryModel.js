Ext.define('CrisisTracker.model.storiesexplorer.StoryModel', {
    extend: 'Ext.data.Model',
    fields: ['id', 'name', 'artist', 'album', 'played_date', 'station'],

    proxy: {
        type: 'ajax',
        url: 'data/staticdemo.json',
        reader: {
            type: 'json',
            root: 'results'
        }
    }
})