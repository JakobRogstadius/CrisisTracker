function ButtonList() {
    "use strict";

    var self = this;
    var container = null,
        data = null,
        selectionCallback = null,
        selectedItem = null,
        horizontal = false;

//Priviliged methods
    this.initialize = function(containerSelection) {
        container = containerSelection;
        container
            .classed("button-list", true)
            .append("table");

        layout();

        return self;
    }

    this.selectedItem = function(item) {
        if (!arguments.length) return selectedItem;

        selectedItem = item;

        if (container == null || data == null)
            return self;

        var oldItem = container.selectAll("td.selected")
            .classed("selected", false);
        var newItem = container.selectAll("td").filter(function(d,i) { return d==item; })
            .classed("selected", true);
        return self;
    }

    this.data = function(value) {
        if (!arguments.length) return data;
        data = value;
        layout();
        return self;
    }

    this.direction = function(value) {
        if (!arguments.length)
            return horizontal ? 'horizontal' : 'vertical';
        horizontal = (value == "horizontal");
        layout();
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
        container.select("table").selectAll("tr").remove();
        var buttons = container.select("table")
            .selectAll("tr")
            .data(data)
            .enter()
                .append("tr")
                .append("td")
                .classed("selected", function(d) { return d == selectedItem; })
                .html(String)
                .on('mousedown', onItemSelected);
    }

    function onItemSelected(d, i) {
        if (d == selectedItem)
            return;

        if (selectionCallback != null) {
            self.selectedItem(d);
            selectionCallback(d, i);
        }
    }
}