/**
 * Application Model
 * A Model represents some object that your application manages
 */ 
Ext.define('mod2.model.GridData', {
    extend: 'Ext.data.Model',
    fields: [
        {name: 'circle',  type: 'int'},
        {name: 'location',   type: 'string'},
        {name: 'component', type: 'int'},
        {name: 'load', type: 'int', defaultValue: 100}
    ]
});
