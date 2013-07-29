function ItemList() {
	"use strict";

	var self = this;

	var getID = function(d) { return d; },
		itemRenderer = function(d, i) { d3.select(this).html(String); },
		itemRendererExpanded = null,
        noItemsMessage = "No items available";

	var container = null,
		data = null,
		selectionCallback = null,
		selectedItem = null;

//Priviliged methods
	this.initialize = function(containerSelection) {
		container = containerSelection;
		container.style("overflow", "auto")
			.append("ul")
			.attr("class", "item-list");
		layout();

		return self;
	}

	this.selectedItem = function(item) {
		if (!arguments.length) return selectedItem;

		selectedItem = item;

		if (container == null || data == null)
			return self;

		var findVal = getID(item);
		var oldItem = container.select("li.selected")
			.classed("selected", false);
		var newItem = container.selectAll("li").filter(function(d,i) { return getID(d)==findVal; })
			.classed("selected", true);

        if (itemRendererExpanded != null) {
            oldItem.each(itemRenderer);
            newItem.each(itemRendererExpanded);
        }

		return self;
	}

	this.data = function(value) {
		if (!arguments.length) return data;
		data = value;
		layout();
		return self;
	}

	this.getID = function(func) {
		if (!arguments.length) return getID;
		getID = func;
		return self;
	}

	this.itemRenderer = function(func) {
		if (!arguments.length) return itemRenderer;
		itemRenderer = func;
		return self;
	}

	this.itemRendererExpanded = function(func) {
		if (!arguments.length) return itemRendererExpanded;
		itemRendererExpanded = func;
		return self;
	}

	this.onItemSelected = function(func) {
		if (!arguments.length) return selectionCallback;
		selectionCallback = func;
		return self;
	}

//Private methods
	function layout() {
		if (container == null || data == null) {
			return;
		}
		container.selectAll("ul").remove();

		var newItem = container.selectAll("li").filter(function(d,i) { return getID(d)==findVal; })
			.classed("selected", true);

		var selectedID = selectedItem == null ? null : getID(selectedItem);
		if (data.length > 0) {
            container.append('ul')
                .attr('class', 'item-list')
                .selectAll("li.list-item")
                .data(data, getID)
                .enter()
                .append("li")
                    .attr("class", "list-item")
                    .classed('list-item', true)
                    .classed('selected', function(d,i) { return getID(d)==selectedID; })
                    .on('mousedown', onItemSelected)
                    .each(itemRenderer);
        }
        else {
            container.append('ul')
                .attr("class", "item-list")
                .append('li')
                .attr("class", "message")
                .html(noItemsMessage);
        }
    }

	function onItemSelected(d, i) {
		if (selectedItem == d)
			return;

		if (selectionCallback != null) {
			self.selectedItem(d);
			selectionCallback(d, i);
		}
	}
}