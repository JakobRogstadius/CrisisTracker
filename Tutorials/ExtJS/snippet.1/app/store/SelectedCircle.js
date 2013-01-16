/**
 * Circle Store
 */
Ext.define('mod1.store.SelectedCircle', {
    extend: 'Ext.data.Store',
	model: 'mod1.model.SelectedCircle',
	autoLoad: true,
	
    proxy: {
     //use sessionstorage if need to save data for that specific session only
     type: 'localstorage',
        id  : 'myProxyKey'
    }
	
});