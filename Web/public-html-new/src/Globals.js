var Globals = {
  apiPath : 'http://ufn.virtues.fi/~jakob/newct/api/',

  linkify : function(text) {
    text = text.replace(/http\S+/g, '<a href="$&" target="_blank">$&</a>');
    return text.replace(/@[A-Za-z0-9_]+/g, '<a href="https://twitter.com/$&" target="_blank">$&</a>');
  },

  cleanTitle : function(text) {
    return text.replace(/#|http\S+/g, '');
  },

  getIsoTime : function(date) {
    return date.toISOString().substring(0,19).replace('T', ' ');
  },

  months : ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],

  getShortTime : function(date) {
    if (typeof date == 'string')
      date = new Date(Date.parse(date + " UTC"));

    var secDiff = Math.floor((new Date().getTime() - date.getTime()) / 1000);
    if (secDiff < 60)
      return secDiff + " seconds ago";
    else if (secDiff < 3600)
      return Math.floor(secDiff / 60) + " minutes ago";
    else if (secDiff < 86400)
      return Math.floor(secDiff / 3600) + " hours ago";
    else {
      var h = date.getHours();
      var s = date.getSeconds();
      return this.months[date.getMonth()] + " " + date.getDate() + " "
      + (h<10 ? "0"+h : h) + ":" + (s<10 ? "0"+s : s);
    }
  },
}
