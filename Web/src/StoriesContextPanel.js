function StoriesContextPanel() {
    "use strict";

//Internals
    var self = this,
        timelineContainer = null,
        storyListContainer = null,
        storyBubblesContainer = null,
        timePeriodButtonValues = ['7d', '1m', '4m', '1y'],
        timelineDays = 7,
        storyList = null,
        timeline = null,
        timePeriodButtons = null,
        storyBubbles = null,
        selectedStory = null,
        filterCollection = new StoryFilter(),
        storySelectedCallback = null,
        timePeriodButtonWidth = "30px",
        currentPanel = null;

//Config
    function getTimelineTime(d) { return d.date_hour; }
    function getTimelineVolume(d) { return d.tweets; }

    function getStoryID(d) { return d.start_time; }
    function getStoryTime(d) { return Globals.parseTime(d.start_time); }
    function getStoryTitle(d) { return Globals.cleanTitle(d.title); }
    function getStorySize(d) { return d.user_count; }

    function getBubbleStoryID(d) { return d.story_id; }
    function getBubbleTime(d) { return d.start_time; }
    function getBubbleSize(d) { return d.weighted_size; }
    function getBubbleKeyword(d) { return d.keyword; }
    function getBubbleTooltip(d) { return Globals.getShortTime(Globals.parseTime(d.start_time)) + "\n" + Globals.cleanTitle(d.title); }

    function renderStoryItem(d, i) {
        var li = d3.select(this);

        li.append('div')
            .attr('class', 'title')
            .append('a')
                .html(function(d) { return Globals.cleanTitle(getStoryTitle(d)); })
                .on('click', function(d) {
                    d3.event.preventDefault();
                });

        li.append('div')
            .attr('class', 'time')
            .attr('title', function(d) { return "The first tweet in this story was seen at " + getStoryTime(d); })
            .html(function(d) { return Globals.getShortTime(getStoryTime(d)); });

        li.append('div')
            .attr('class', 'sources')
            .attr('title', function(d) { return "This story was mentioned on Twitter by at least " + getStorySize(d) + " people"; })
            .html(function(d) { return getStorySize(d); });
    }

//Public methods
    this.initialize = function(timelineContainerSelection,
                storyListContainerSelection,
                storyBubblesContainerSelection) {

        timelineContainer = timelineContainerSelection
        storyListContainer = storyListContainerSelection;
        storyBubblesContainer = storyBubblesContainerSelection;

        //Timeline and time interval buttons
        timelineContainer
            .style("position", "relative")
            .style("overflow", "hidden");
        var timelineDiv = timelineContainer.append("div")
            .style("position", "absolute")
            .style("left", timePeriodButtonWidth)
            .style("right", "0")
            .style("top", "0")
            .style("bottom", "0");
        var buttonDiv = timelineContainer.append("div")
            .style("position", "absolute")
            .style("left", "0")
            .style("width", timePeriodButtonWidth)
            .style("top", "0")
            .style("bottom", "0");

        timeline = new TimelineFilter()
            .getT(getTimelineTime)
            .getY1(getTimelineVolume)
            .onFilter(timeFilterChanged)
            .initialize(timelineDiv);

        timePeriodButtons = new ButtonList()
            .data(timePeriodButtonValues)
            .selectedItem(timePeriodButtonValues[0])
            .onItemSelected(timePeriodButtonClicked)
            .initialize(buttonDiv);

        storyList = new ItemList()
            .getID(getStoryID)
            .itemRenderer(renderStoryItem)
            .onItemSelected(self.selectStory)
            .initialize(storyListContainer);

        storyBubblesContainerSelection
            .style("overflow", "hidden");

        storyBubbles = new DotPlot()
            .rMultiplier(0.6)
            .getID(getBubbleStoryID)
            .getX(getBubbleTime)
            .getY(getBubbleKeyword)
            .getR(getBubbleSize)
            .getTooltip(getBubbleTooltip)
            .onItemSelected(self.selectStory)
            .initialize(storyBubblesContainer);

        setInterval(function() {
          self.reloadTimeline();
          if(filterCollection.showOngoingStories)
            self.reloadStoriesList();
        },60000);

        return self;
    }

    this.timelineData = function(value) {
        if (!arguments.length) return timeline.data;
        timeline.data(value);
        return self;
    }

    this.storyListData = function(value) {
        if (!arguments.length) return storyList.data;
        storyList.data(value);
        return self;
    }

    this.storyBubblesData = function(value) {
        if (!arguments.length) return storyBubbles.data;
        storyBubbles.data(value);
        return self;
    }

    this.reloadAll = function() {
        self.reloadTimeline();
        self.reloadStoriesList();
        self.reloadBubbles();
        return self;
    }

    this.reloadTimeline = function() {
        var url = Globals.apiPath + "get_total_volume_by_time.php?days=" + timelineDays;
        d3.json(url, function(d) { self.timelineData(d); });

        return self;
    }

    this.resize = function() {
        storyBubbles.onSizeChanged();
        timeline.onSizeChanged();
        return self;
    }

    this.reloadStoriesList = function() {
        var topicF = "";
        if (filterCollection.topics != null && filterCollection.topics.keys().length > 0) {
            topicF = "&topics=";
            var attrIDs = filterCollection.topics.keys();
            for (var i=0; i<attrIDs.length; ++i) {
                if (j > 0) { topicF += ";"; }
                topicF += attrIDs[i] + ":";
                var labelIDs = filterCollection.topics.get(attrIDs[i]).values();
                for (var j=0; j<labelIDs.length; ++j) {
                    if (j > 0) { topicF += ","; }
                    topicF += labelIDs[j];
                }
            }
        }
        var minT = filterCollection.minTime == null ? "" : "&mintime=" + encodeURIComponent(Globals.getIsoTime(filterCollection.minTime));
        var maxT = filterCollection.maxTime == null ? "" : "&maxtime=" + encodeURIComponent(Globals.getIsoTime(filterCollection.maxTime));
        var textF = filterCollection.text == null ? "" : "&text=" + encodeURIComponent(filterCollection.text);
        var geoF = "";
        var ongoing = "&ongoing=" + (filterCollection.showOngoingStories ? 1 : 0);
        var url = Globals.apiPath + "get_stories.php?" + minT + maxT + textF + geoF + topicF + ongoing;
        console.log(url);
        d3.json(url, function(d) { self.storyListData(d); });

        return self;
    }

    this.reloadBubbles = function() {
        //var minT = filterCollection.minTime == null ? "" : "&mintime=" + encodeURIComponent(Globals.getIsoTime(filterCollection.minTime));
        //var maxT = filterCollection.maxTime == null ? "" : "&maxtime=" + encodeURIComponent(Globals.getIsoTime(filterCollection.maxTime));
        var textF = filterCollection.text == null ? "" : "&text=" + encodeURIComponent(filterCollection.text);
        var url = Globals.apiPath + "get_top_stories_by_keyword.php?" + textF;
        d3.json(url, function(d) { self.storyBubblesData(d); });

        return self;
    }

    this.textFilter = function(value) {
        if (!arguments.length) return filterCollection.text;
        filterCollection.text = value;
        self.reloadStoriesList();
        self.reloadBubbles();
        return self;
    }

    this.topicFilter = function(value) {
        if (!arguments.length) return filterCollection.topics;
        filterCollection.topics = value;
        self.reloadStoriesList();
        self.reloadBubbles();
        return self;
    }

    this.onStorySelected = function(func) {
        if (!arguments.length) return storySelectedCallback;
        storySelectedCallback = func;
        return self;
    }

    this.selectStory = function(d, i) {
        timeline.highlightTime(getStoryTime(d));
        storyList.selectedItem(d);
        if (storySelectedCallback != null) {
            storySelectedCallback(d, i);
        }
    }

//Private methods
    function timeFilterChanged(minT, maxT) {
        filterCollection.minTime = minT;
        filterCollection.maxTime = maxT;
        filterCollection.showOngoingStories = (minT == null && maxT == null);

        self.reloadStoriesList();
    }

    function timePeriodButtonClicked(d) {
        switch (d) {
            case '7d':
                timelineDays = 7;
                break;
            case '1m':
                timelineDays = 30;
                break;
            case '4m':
                timelineDays = 120;
                break;
            case '1y':
                timelineDays = 365;
                break;
            default: //'7d'
                timelineDays = 7;
                break;
        }

        self.reloadTimeline();
    }
}