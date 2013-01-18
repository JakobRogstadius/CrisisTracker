var is_ssl = ("https:" == document.location.protocol);
var asset_host = is_ssl ? "https://d3rdqalhjaisuu.cloudfront.net/" : "http://d3rdqalhjaisuu.cloudfront.net/";
document.write(unescape("%3Cscript src='" + asset_host + "javascripts/feedback-v2.js' type='text/javascript'%3E%3C/script%3E"));

var feedback_widget_options = {};
feedback_widget_options.display = "overlay";  
feedback_widget_options.company = "crisistracker";
feedback_widget_options.placement = "right";
feedback_widget_options.color = "#222";
feedback_widget_options.style = "idea";

var feedback_widget = new GSFN.feedback_widget(feedback_widget_options);
