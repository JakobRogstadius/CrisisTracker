function StoryFocusPanel() {
    "use strict";

//Internals
    var self = this;
    var titleContainer = null,
        tabsContainer = null,
        timelineContainer = null,
        textFilterContainer = null,
        tweetListContainer = null,
        similarStoriesContainer = null,
        storyID = null,
        tweetList = null,
        similarStoriesList = null,
        timeline = null,
        textFilter = null,
        labelFilter = null,
        labelFilterData = null,
        title = null,
        filterCollection = new StoryFilter();

//Config
    var textFilterHeight = "24px";
    function getStoryID(d) { return d.story_id; }
    function getStoryTitle(d) { return d.title; }
    function getStorySize(d) { return d.user_count; }
    function getStoryTime(d) { return d.start_time; }

    function getTimelineTime(d) { return d.created_at; }
    function getTimelineVolume1(d) { return d.first_seen_at; }
    function getTimelineVolume2(d) { return d.tweet_count; }
    function getTweetID(d) { return d.tweet_id; }
    function getTweetText(d) { return d.text; }
    function getUserCount(d) { return d.user_count; }
    function getCreatedAt(d) { return Globals.parseTime(d.created_at); }
    function getProfileImageUrl(d) { return d.profile_image_url; }
    function getUserScreenName(d) { return d.user_screen_name; }
    function getUserRealName(d) { return d.user_real_name; }

    function renderTweetItem(d, i) {
        var li = d3.select(this);

        li.classed('tweet', true);
        li.classed("expanded", false);
        li.attr("title", "Click to expand");
        li.html('');

        li.append('img')
            .attr('src', function(d) { return getProfileImageUrl(d); });

        li.append('div')
            .classed('text', true)
            .html(function(d) { return Globals.linkify(getTweetText(d)); });

        li.append('div')
            .classed('source', true)
            .html(function(d) {
                return '<span title="' + getCreatedAt(d) + '">' + Globals.getShortTime(getCreatedAt(d)) + '</span>'
                    + ' from <a href="http://twitter.com/' + getUserScreenName(d) + '/status/'
                    + getTweetID(d) + '" target="_blank">' + getUserRealName(d) + '</a>'
                    + (getUserCount(d)==2 ? (' and ' + (getUserCount(d)-1) + ' other') : '')
                    + (getUserCount(d)>2 ? (' and ' + (getUserCount(d)-1) + ' others') : '');
              });
    }

    function renderExpandedTweetItemAsync(d, i) {
        var li = d3.select(this);
        li.classed("expanded", true);

        var oembedUrl = "https://api.twitter.com/1/statuses/oembed.json?id=" + getTweetID(d) + "&align=center&maxwidth=550&callback=Globals.renderExpandedTweet";
        var url = Globals.apiPath + "cross_domain_request_wrapper.php?url=" + encodeURIComponent(oembedUrl);
        //d3.json(url, function(d) { renderExpandedTweetItem(li, d); });

        var div = $('#tweetExpansionCallbackDiv');
        div.html('');
        div.append('<script async src="' + oembedUrl + '"></script>');
        console.log(div.html());
    }

    function renderSimilarStoryItem(d, i) {
        var li = d3.select(this);

        li.classed('story', true)
          .on('click', function(d) {
            self.loadStory(d.story_id);
            });

        li.append('div')
            .attr('class', 'title')
            .append('a')
                .html(function(d) { return Globals.cleanTitle(getStoryTitle(d)); })
                .on('click', function(d) {
                    d3.event.preventDefault();
                });

        li.append('div')
            .attr('class', 'time')
            .attr('title', function(d) { return getStoryTime(d); })
            .html(function(d) { return Globals.getShortTime(Globals.parseTime(getStoryTime(d))); });

        li.append('div')
            .attr('class', 'sources')
            .html(function(d) { return getStorySize(d); });

        li.append('div')
            .classed('merge', true)
            .append('a')
              .html("Merge")
              .on('click', function(d) {
                var url = Globals.apiPath + "merge_stories.php?storyid1=" + getStoryID(d) + "&storyid2=" + storyID;
                console.log(url);
                d3.html(url, function(error, data) { console.log(data); });
                var mergeDiv = $(this).parent();
                mergeDiv.parent().addClass("merged");
                mergeDiv.empty();
                mergeDiv.html("<i>merging</i>");
                d3.event.stopPropagation();
                });
    }

//Public methods
    this.initialize = function(
                tabsContainerSelection,
                titleContainerSelection,
                filterContainerSelection,
                tweetListContainerSelection,
                similarStoriesContainerSelection) {

        tabsContainer = tabsContainerSelection;

        titleContainer = titleContainerSelection;
        var filterContainer = filterContainerSelection
            .style("position", "relative")
            .style("overflow", "hidden");
        tweetListContainer = tweetListContainerSelection;
        similarStoriesContainer = similarStoriesContainerSelection;

        //Timeline
        timelineContainer = filterContainer.append("div")
            .style("position", "absolute")
            .style("left", "0")
            .style("right", "0")
            .style("top", "0")
            .style("bottom", textFilterHeight);

        timeline = new TimelineFilter()
            .getT(getTimelineTime)
            .getY1(getTimelineVolume1)
            .getY2(getTimelineVolume2)
            .onFilter(timeFilterChanged)
            .tailCutoffRatio(0.95)
            .initialize(timelineContainer);

        //Text filter
        textFilterContainer = filterContainer.append("div")
            .style("position", "absolute")
            .style("left", "0")
            .style("width", "50%")
            .style("height", textFilterHeight)
            .style("bottom", "0");

        textFilter = new TextFilter()
            .initialize(textFilterContainer)
            .onFilterChanged(function(text) {
                filterCollection.text = text;
                self.reloadTweets();
            });

        //Topic filter
        labelFilter = filterContainer.append("select")
            .style("position", "absolute")
            .style("width", "50%")
            .style("right", "0")
            .style("height", textFilterHeight)
            .style("bottom", "0")
            .on('change', function() {
                filterCollection.topics = this.options[this.selectedIndex].__data__;
                self.reloadTweets();
            });

        //Tweet list
        tweetList = new ItemList()
            .getID(getTweetID)
            .itemRenderer(renderTweetItem)
            .itemRendererExpanded(renderExpandedTweetItemAsync)
            .onItemSelected(tweetSelected)
            .initialize(tweetListContainer);

        //Similar stories list
        similarStoriesList = new ItemList()
            .getID(getStoryID)
            .itemRenderer(renderSimilarStoryItem)
            .initialize(similarStoriesContainer);

        return self;
    }

    this.title = function(value) {
        if (!arguments.length) return title;
        title = value;
        titleContainer.select("h1").html('<a target="_top" href="index.html?story=' + storyID + '">Story link</a>' + Globals.cleanTitle(title));
        return self;
    }

    this.timelineData = function(value) {
        if (!arguments.length) return timeline.data;
        timeline.data(value);
        return self;
    }

    this.labelFilterData = function(value) {
        if (!arguments.length) return labelFilterData;
        labelFilterData = value;

        labelFilter.selectAll('option').remove();
        labelFilter.selectAll('optgroup').remove();
        labelFilter.append('option')
            .html('Select label filter');
        labelFilter.selectAll('optgroup')
            .data(labelFilterData)
            .enter()
                .append('optgroup')
                .attr('label', function(d) { return d.attribute_name; })
                .selectAll('option')
                .data(function(d) { return d.labels; })
                    .enter()
                        .append('option')
                        .attr('value', function(d) { return d.label_id; })
                        .html(function(d) { return d.label_name + " (" + d.tag_count + ")"; });

        return self;
    }

    this.tweetListData = function(value) {
        if (!arguments.length) return tweetList.data;
        tweetList.data(value);
        return self;
    }

    this.similarStoriesListData = function(value) {
        if (!arguments.length) return similarStoriesList.data;
        similarStoriesList.data(value);
        return self;
    }

    this.resize = function() {
        timeline.onSizeChanged();
        return self;
    }

    this.loadStory = function(value) {
        storyID = value;
        var url = Globals.apiPath + "is_story_merged.php?id=" + storyID;
        d3.json(url, function(d) { actualLoadStory(d); });
        return self;
    }

    this.reloadStoryInfo = function() {
        var url = Globals.apiPath + "get_story_info.php?id=" + storyID;
        d3.json(url, function(d) { self.title(getStoryTitle(d)); });
        return self;
    }

    this.reloadTimeline = function() {
        var url = Globals.apiPath + "get_story_volume_by_time.php?id=" + storyID;
        d3.json(url, function(d) { self.timelineData(d); });
        return self;
    }

    this.reloadLabelFilter = function() {
        var url = Globals.apiPath + "get_story_aidr_labels.php?id=" + storyID;
        d3.json(url, function(d) { self.labelFilterData(d); });
        return self;
    }

    this.reloadTweets = function() {
        var sid = "id=" + storyID;
        var minT = filterCollection.minTime == null ? "" : "&mintime=" + encodeURIComponent(Globals.getIsoTime(filterCollection.minTime));
        var maxT = filterCollection.maxTime == null ? "" : "&maxtime=" + encodeURIComponent(Globals.getIsoTime(filterCollection.maxTime));
        var textF = filterCollection.text == null ? "" : "&text=" + encodeURIComponent(filterCollection.text);
        var geoF = "";
        var topicF = filterCollection.topics == null ? "" : "&label_id=" + filterCollection.topics.label_id;
        var url = Globals.apiPath + "get_story_tweets.php?" + sid + minT + maxT + textF + geoF + topicF;
        console.log(url);
        d3.json(url, function(d) { self.tweetListData(d); });

        return self;
    }

    this.reloadRelatedStories = function() {
      var url = Globals.apiPath + "get_similar_stories.php?id=" + storyID;
      d3.json(url, function(d) { self.similarStoriesListData(d); });

      return self;
    }

//Private methods
     function actualLoadStory(result) {
        if (result.merged_with != null) {
            storyID = parseInt(result.merged_with);
        }
        else if (result.story_id == null) {
            return;
        }

        resetFilters();
        self.reloadStoryInfo();
        self.reloadTimeline();
        self.reloadTweets();
        self.reloadRelatedStories();
        self.reloadLabelFilter();

        $(tabsContainer.node()).tabs("option", "active", 0);
    }

    function resetFilters() {
        filterCollection = new StoryFilter();
        timeline.clearHighlights();
        textFilter.clear();
    }

    function tweetSelected(d,i) {
        timeline.highlightTime(getCreatedAt(d));
    }

    function timeFilterChanged(minT, maxT) {
        filterCollection.minTime = minT;
        filterCollection.maxTime = maxT;

        self.reloadTweets();
    }
}