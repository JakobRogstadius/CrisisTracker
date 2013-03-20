Ext.define('CrisisTracker.view.tagstories.Href', {
	extend: 'Ext.Component',
	alias: 'widget.href',
	config: {id: '0'},
	initComponent: function() {
		this.callParent(arguments);
		this.addEvents('click');
		this.on('render', function() {
			this.getEl().on('mousedown', this.onClick, this);
		});
		this.on('destroy', function() {
			if (this.rendered) {
				this.getEl().un('mousedown', this.onClick, this);
			}
		});
	},
	onClick: function(e) {
		if (this.handler) {
			this.handler.call(this.scope || this, this, e);
		}
		this.fireEvent('click', this, e);
	},
	
	setHandler: function(handler, scope) {
		this.handler = handler;
		this.scope = scope;
		return this;
	},
	
	activeTag: function(){		
		this.update('<b>'+this.id+'</b>');
	},
	
	inactiveTag: function() {
		this.update(this.id);
	}
});