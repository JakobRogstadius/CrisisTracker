function itemList() {
    var getID = function(d) { return d; },
        itemRenderer = function(d, i) { d3.select(this).html(String); };

    var container = null,
        data = null,
        selectionCallback = null;

    //Constructor-ish
    function my(containerSelection) {
        container = containerSelection;
        layout();
    }

    function onItemSelected(d, i) {
        if (selectionCallback != null) {
            my.setSelectedItem(d);
            selectionCallback(d, i);
        }
    }

    function layout() {
        if (container == null || data == null) {
            return;
        }

        container.selectAll("ul").remove();

        container.append('ul')
            .attr('class', 'item-list')
            .selectAll("li.list-item")
            .data(data)
            .enter()
            .append("li")
                .attr("class", "list-item")
                .on('mousedown', onItemSelected)
                .each(itemRenderer);
    }

    my.setSelectedItem = function(item) {
        if (container == null)
            return;

        findVal = getID(item);
        var oldItem = container.select("li.selected")
            .classed("selected", false);
        var newItem = container.selectAll("li").filter(function(d,i) { return getID(d)==findVal; })
            .classed("selected", true);
    }

    my.data = function(value) {
        data = value;
        layout();
        return my;
    }

    my.getID = function(func) {
        if (!arguments.length) return getID;
        getID = func;
        return my;
    }

    my.itemRenderer = function(func) {
        if (!arguments.length) return itemRenderer;
        itemRenderer = func;
        return my;
    }

    my.onItemSelected = function(func) {
        if (!arguments.length) return selectionCallback;
        selectionCallback = func;
        return my;
    }

    return my;
}