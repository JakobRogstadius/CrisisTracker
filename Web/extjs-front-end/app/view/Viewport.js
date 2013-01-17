// Adjust the application's overall layout here

Ext.define('CrisisTracker.view.Viewport', {
    extend: 'Ext.container.Viewport',
	requires: [
			'CrisisTracker.view.latestnews.NewsList',
			'CrisisTracker.view.storiesexplorer.Tagger',
	],
	layout: 'fit',
	defaults: {bodyStyle:'padding:5px'},		
	
	initComponent: function() {
		console.log('VIEWPORT - Viewport.js -> Viewport Created');
        this.items = {
            xtype: 'panel',
            dockedItems: [{
                dock: 'top',
                xtype: 'toolbar',
                height: 80,
                items: [{
                    xtype: 'panel',
                    width: 150
                }, {
                    xtype: 'panel',
                    height: 70,
                    flex: 1
                }, {
                    xtype: 'component',
                    html: 'Panda<br>Internet Radio'
                }]
            }],
			
            layout: {
                type: 'hbox',
                align: 'stretch'
            },
            items: [{
                width: 250,
                xtype: 'panel',
                layout: {
                    type: 'vbox',
                    align: 'stretch'
                },
                items: [{
                    xtype: 'panel',
                    flex: 1
                }, {
                    html: 'Ad',
                    height: 250,
                    xtype: 'panel'
                }]
            }, {
                xtype: 'container',
                flex: 1,
                layout: {
                    type: 'vbox',
                    align: 'stretch'
                },
                items: [{
                    xtype: 'panel',
                    height: 250
                }, {
                    xtype: 'panel',
                    flex: 1
                }]
            }]
        };

        this.callParent();
    }
});	