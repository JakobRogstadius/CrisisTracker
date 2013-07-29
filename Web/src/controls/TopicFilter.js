function TopicFilter() {
    "use strict";

    var self = this;
    var container = null,
        data = null,
        filterChangedCallback = null,
        selectedIDs = d3.map(),
        getL1ID = function(d) { return d; },
        getL1Name = function(d) { return d; },
        getL1Tooltip = function(d) { return d; },
        getL2List = function(d) { return d; },
        getL2ID = function(d) { return d; },
        getL2Name = function(d) { return d; },
        getL2Tooltip = function(d) { return d; },
        getL2Value = function(d) { return d; };

//Priviliged methods
    this.initialize = function(containerSelection) {
        container = containerSelection;
        container
            .classed("topic-filter", true)
            .append("ul")
            .attr("class", "colormap");

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

    this.getL1ID = function(func) {
        if (!arguments.length) return getL1ID;
        getL1ID = func;
        return self;
    }

    this.getL1Name = function(func) {
        if (!arguments.length) return getL1Name;
        getL1Name = func;
        return self;
    }

    this.getL1Tooltip = function(func) {
        if (!arguments.length) return getL1Tooltip;
        getL1Tooltip = func;
        return self;
    }

    this.getL2List = function(func) {
        if (!arguments.length) return getL2List;
        getL2List = func;
        return self;
    }

    this.getL2ID = function(func) {
        if (!arguments.length) return getL2ID;
        getL2ID = func;
        return self;
    }

    this.getL2Name = function(func) {
        if (!arguments.length) return getL2Name;
        getL2Name = func;
        return self;
    }

    this.getL2Tooltip = function(func) {
        if (!arguments.length) return getL2Tooltip;
        getL2Tooltip = func;
        return self;
    }

    this.getL2Value = function(func) {
        if (!arguments.length) return getL2Value;
        getL2Value = func;
        return self;
    }

    this.onFilterChanged = function(func) {
        if (!arguments.length) return filterChangedCallback;
        filterChangedCallback = func;
        return self;
    }

//Private methods
    function layout() {
        if (container == null || data == null) {
            return;
        }
        container.select("ul").selectAll("li").remove();
        var itemsL1 = container.select("ul")
            .selectAll("li")
            .data(data, function(d) { return getL1ID(d); })
            .enter()
                .append("li")
                .attr("class", function(d) {
                    var colorIndex = Array.prototype.indexOf.call(this.parentElement.children, this);
                    return "item-L1 q" + colorIndex;
                })
                .classed("single-item", function(d) { return getL2List(d).length < 2; });

        itemsL1.append("h1")
            .html(function(d) { return getL1Name(d); })
            .attr("title", function(d) { return getL1Tooltip(d); });

        itemsL1.append("ul")
            .selectAll("li")
            .data(function(d) { return getL2List(d); }, function(d) { return getL2ID(d); })
            .enter()
                .append("li")
                .attr("class", "item-L2")
                .html(function(d) { return getL2Name(d) + "<span>(" + getL2Value(d) + ")</span>"; })
                .attr("title", function(d) { return getL2Tooltip(d); })
                .on("mousedown", onItemSelected)
    }

    function onItemSelected(d, i) {
        var item = d3.select(this);
        var isSelected = item.classed("selected");
        item.classed("selected", !isSelected);

        var affectedIDL1 = getL1ID(d3.select(item.node().parentNode).datum());
        var affectedIDL2 = getL2ID(d);
        if (selectedIDs.has(affectedIDL1)) {
            if (selectedIDs.get(affectedIDL1).has(affectedIDL2)) {
                selectedIDs.get(affectedIDL1).remove(affectedIDL2);
                if (selectedIDs.get(affectedIDL1).values().length == 0) {
                    selectedIDs.remove(affectedIDL1);
                }
            }
            else {
                selectedIDs.get(affectedIDL1).add(affectedIDL2);
            }
        }
        else {
            selectedIDs.set(affectedIDL1, new d3.set());
            selectedIDs.get(affectedIDL1).add(affectedIDL2);
        }

        if (filterChangedCallback != null) {
            filterChangedCallback(selectedIDs);
        }
    }
}