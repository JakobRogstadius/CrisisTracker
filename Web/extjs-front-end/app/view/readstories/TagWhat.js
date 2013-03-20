Ext.define('CrisisTracker.view.readstories.TagWhat', {
	extend: 'Ext.panel.Panel',
	title: 'WHAT',
	alias: 'widget.tagWhat',
	layout: 'anchor',
	items: [
	
		{	
			xtype: 'gridpanel',
			anchor: '100% 80%',
			hideHeaders: true,
			rowLines: false,
			disableSelection: true,
			store: Ext.create('Ext.data.Store', {
				fields: ['name', 'toggled'],
				data: [
					{ 'name': 'Bomb' , 'toggled': "true" },
					{ 'name': 'Grenade', 'toggled': "false" },
					{ 'name': 'Missile', 'toggled': "false" },
					{ 'name': 'Suicide', 'toggled': "false" },
					{ 'name': 'Ship', 'toggled': "false" },
					{ 'name': 'Xplosion', 'toggled': "false" }			
				]
			}),
			columns: [
				{		
					flex: 1,
					renderer: function (v, m, r) {
						var id = Ext.id();
						var press = false;
						if (r.get('toggled') == "true") {
							press = true;
						};
						
						Ext.defer(function () {
							Ext.widget('button', {
								renderTo: id,
								text: r.get('name'),
								width: 80,
								enableToggle: true,
								pressed: press,
								//handler: function () { Ext.Msg.alert('Info', r.get('name')) }
							});
						}, 10);
						return Ext.String.format('<div id="{0}"></div>', id);
					}
				},
				{
					flex: 1,
					renderer: function (v, m, r) {
						var id = Ext.id();
						var press = false;
						if (r.get('toggled') == "true") {
							press = true;
						};
						
						Ext.defer(function () {
							Ext.widget('button', {
								renderTo: id,
								text: r.get('name'),
								width: 80,
								enableToggle: true,
								pressed: press,
								//handler: function () { Ext.Msg.alert('Info', r.get('name')) }
							});
						}, 10);
						return Ext.String.format('<div id="{0}"></div>', id);
					}
				}
			],		
		},
		
		{xtype: 'panel', anchor: '100% 20%'}
	
	],
    initComponent: function() {
		this.callParent();	
		this.center();		
	}

});
