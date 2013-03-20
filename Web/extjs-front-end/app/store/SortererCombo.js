// The data store containing the list of states
Ext.define('CrisisTracker.store.SortererCombo', {
	storeId:'ajaxProxyTagStories',
    extend: 'Ext.data.Store',	
	autoLoad: false,
    fields: ['abbr', 'name'],
    data : [
        {"abbr":"AL", "name":"Most Shared in the Past 4 Hours"},
        {"abbr":"AK", "name":"Most Shared in All Time by Size"},
        {"abbr":"AZ", "name":"Newest"}
    ]
});