function TextFilter() {
    "use strict";

//Internals
    var self = this,
        container = null,
        textarea = null,
        filterChangedCallback = null;

//Config
    var placeholder = "Keyword AND-search [e.g. cityname, bomb]";

//Public methods
    this.initialize = function(containerSelection) {

        container = containerSelection;

        textarea = container
            .append("textarea")
            .attr("placeholder", placeholder)
            .classed("fill", true)
            .classed("textfilter", true)
            .style("resize", "none")
            .on("change", function() { textChanged(this); })
            .on("keypress", validateKeyPress);

        return self;
    }

    this.onFilterChanged = function(func) {
        if (!arguments.length) return filterChangedCallback;
        filterChangedCallback = func;
        return self;
    }

    this.placeHolderText = function(value) {
        if (!arguments.length) return placeholder;
        placeholder = value;
        container.select("textarea").attr("placeholder", placeholder);
        return self;
    }

    this.clear = function() {
        textarea[0][0].value = "";
    }

//Private methods
    function textChanged(source) {
        if (filterChangedCallback != null) {
            var text = source.value == "" ? null : source.value;
            filterChangedCallback(text);
        }
    }

    function validateKeyPress() {
        if (d3.event.keyCode == 13) {
            if(d3.event.preventDefault)
                d3.event.preventDefault();
            else
                d3.event.returnValue = false;
            textChanged(textarea[0][0])
        }
    }
}