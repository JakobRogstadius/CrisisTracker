var Globals = {

  apiPath: 'api/',

  linkify: function(text) {
    text = text.replace(/http\S+/g, '<a href="$&" target="_blank">$&</a>');
    return text.replace(/@[A-Za-z0-9_]+/g, '<a href="https://twitter.com/$&" target="_blank">$&</a>');
  },

  cleanTitle: function(text) {
    return text.replace(/#|http\S+/g, '');
  },

  getIsoTime : function(date) {
    return date.toISOString().substring(0,19).replace('T', ' ');
  },

  mySqlToIsoTime : function(dateStr) {
    return new Date(Date.parse(dateStr.substring(0,19).replace(' ', 'T') + "Z"));
  },

  months : ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],

  parseTime: d3.time.format.utc("%Y-%m-%d %H:%M:%S").parse,

  getShortTime : function(indate) {
    var date = new Date();
    if (typeof indate == 'string')
      date = Globals.mySqlToIsoTime(indate);
    else
      date = indate;

    var secDiff = Math.floor((new Date().getTime() - date.getTime()) / 1000);
    if (secDiff < 60)
      return secDiff + " seconds ago";
    else if (secDiff < 120)
      return "1 minute ago";
    else if (secDiff < 3600)
      return Math.floor(secDiff / 60) + " minutes ago";
    else if (secDiff < 7200)
      return "1 hour ago";
    else if (secDiff < 86400)
      return Math.floor(secDiff / 3600) + " hours ago";
    else {
      var h = date.getHours();
      var m = date.getMinutes();
      return this.months[date.getMonth()] + " " + date.getDate() + " "
      + (h<10 ? "0"+h : h) + ":" + (m<10 ? "0"+m : m);
    }
  },

  getShortTime2 : function(indate) {
    var date = new Date();
    if (typeof indate == 'string')
      date = Globals.mySqlToIsoTime(indate);
    else
      date = indate;

    var secDiff = Math.floor((new Date().getTime() - date.getTime()) / 1000);
    if (secDiff < 60)
      return secDiff + " seconds ago";
    else if (secDiff < 120)
      return "1 minute ago";
    else if (secDiff < 3600)
      return Math.floor(secDiff / 60) + " minutes ago";
    else if (secDiff < 3600*24 && date.getDate()==new Date().getDate()) {
      var h = date.getHours();
      var m = date.getMinutes();
      return (h<10 ? "0"+h : h) + ":" + (m<10 ? "0"+m : m)
        + ' today';
    }
    else if (secDiff < 3600*48 && (new Date().getDate() - date.getDate()) == 1) {
      var h = date.getHours();
      var m = date.getMinutes();
      return (h<10 ? "0"+h : h) + ":" + (m<10 ? "0"+m : m)
        + ' yesterday';
    }
    else {
      var h = date.getHours();
      var m = date.getMinutes();
      return (h<10 ? "0"+h : h) + ":" + (m<10 ? "0"+m : m)
        + ' ' + this.months[date.getMonth()] + " " + date.getDate();
    }
  },

  getUrlVars : function() {
    var vars = {};
    var parts = window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function(m,key,value) {
      if(vars[key]){
        if(vars[key] instanceof Array){
          vars[key].push(value);
        }else{
          vars[key] = [vars[key], value];
        }
      }else{
        vars[key] = value;
      }
    });
    return vars;
  },

  getMembers : function(obj) {
    var result = [];
    for (var id in obj) {
      try {
        result.push("[" + typeof(obj[id]) + "]" + id + ": " + obj[id].toString());
      } catch (err) {
        result.push(id + ": inaccessible");
      }
    }
    return result;
  },

  renderExpandedTweet : function(oembed) {
    console.log(oembed);
    var li = $('li.tweet.expanded');
    oembed.html = oembed.html.replace("//platform.twitter.com", "http://platform.twitter.com");
    li.html('');
    $(li).append(oembed.html);
  }
}

Array.prototype.contains = function(v) {
    for(var i = 0; i < this.length; i++) {
        if(this[i] === v) return true;
    }
    return false;
};

Array.prototype.unique = function() {
    var arr = [];
    for(var i = 0; i < this.length; i++) {
        if(!arr.contains(this[i])) {
            arr.push(this[i]);
        }
    }
    return arr;
}
