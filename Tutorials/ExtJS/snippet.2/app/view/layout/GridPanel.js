/**
 * view.GridPanel
 */ 
Ext.define('mod2.view.layout.GridPanel' ,{	
    extend: 'Ext.grid.Panel',
    alias: 'widget.gridPanel',
	title: 'Data',	
	width: 400,
	store: 'GridAjaxProxy',
	
    columns: [
	
		// Circle
        { 
			header: 'Circle',  dataIndex: 'circle'
		},
		
		// Location
        { 
			header: 'Location', dataIndex: 'location', draggable: true ,
            field: {
                xtype: 'textfield',
                allowBlank: false
            }				
		},
		
		// Component
        { 
			header: 'Component', dataIndex: 'component' 
		},
		
		// load
		{ 
			header: 'Load', 
			dataIndex: 'load',
			renderer: function (value, metaData, record, row, col, store, gridView) {
				if(value < 10) {
					metaData.style = 'color:#D10000;font-weight:bold;background:#F7E4E4;';
					return value;
				}
				else {
					metaData.style = 'color:#00D150;font-weight:bold;background:#E6F7E4;';
					return value;
					
					// OTHER WAY OF FORMATTING:
					//return Ext.String.format('<font size="3" color="red">{0}</font>',value);
				}
			}			
		}
    ]
	
	
	
});