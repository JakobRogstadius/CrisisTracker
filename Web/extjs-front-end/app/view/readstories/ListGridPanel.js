Ext.define('CrisisTracker.view.readstories.ListGridPanel', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.readStoriesListGridPanel',
	store: 'ReadStoriesProxy',
    dockedItems: [{
        xtype: 'pagingtoolbar',
        store: 'ReadStoriesProxy',
        dock: 'bottom',
        displayInfo: true
    }],	
    columns: [
	
        { 
			header: 'Popularity',  dataIndex: 'popularity', flex: 1
		},	

        { 
			header: 'Time', dataIndex: 'start_time_str', flex: 2				
		},
		
        { 
			header: 'Title', dataIndex: 'title', flex: 10 
		},
		
		{ 
			header: 'Tags', 	flex: 2,
			
			// Renderer Function - You can render anything you want on this grid column 'Tags' cells
			renderer: function (value, metaData, record, row, col, store, gridView) {
			
				if (record.data.popularity < 200) {
					return Ext.String.format('<img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="red">{0}</font> <img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="red">{1}</font><img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="red">{2}</font><img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="red">{3}</font>',record.data.popularity,record.data.category_count,record.data.entity_count,record.data.keyword_count);
				}
				else {
					return Ext.String.format('<img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="green">{0}</font> <img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="green">{1}</font><img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="green">{2}</font><img src="./resources/images/tag_entity.png" alt="Popularity" height="13" width="13"></img><font size="2" color="green">{3}</font>',record.data.popularity,record.data.category_count,record.data.entity_count,record.data.keyword_count);
				}

			}			
		}
    ],
	
	initComponent : function() {
		this.callParent(arguments);		
	},
	
	// Event Handler
	listeners: {
		// beforerender: function() {
			// console.log(this);		
			// this.getStore().load({
				// params: {
					// // specify params for the first page load if using paging
					// start: 0,
					// limit: 10,
					// // other params
					// //foo:   'bar'
				// }
			// });		
		// }
	}

});