/**
 * The Grid Panel of the Application. Displays the data returned by the WebService API
 * @extends Ext.grid.Panel
 */
Ext.define('CSMap.view.GridPanel' ,{	
    extend: 'Ext.grid.Panel',
    alias: 'widget.gridPanel',
	title: 'Data',
	store: 'MapItems',

	border: false,
    columns: [

		// Id
        { 
			header: 'Id',  dataIndex: 'id', flex: 3
		},	
		
		// Story
        { 
			header: 'Story',  dataIndex: 'story'
		},
		
		// Tags
        { 
			header: 'Tags', dataIndex: 'tags'
		},
		
		// Lon
        { 
			header: 'Coordinates Lon', dataIndex: 'lon' 
		},		
		
		// Lat
        { 
			header: 'Coordinates Lat', dataIndex: 'lat' 
		}
    ]
	
	
	
});