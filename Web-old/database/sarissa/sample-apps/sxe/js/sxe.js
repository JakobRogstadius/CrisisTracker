var log;
MessageDisplay = function(elemId) {
    this.area = document.getElementById(elemId);
    this.display("Logging initialized");
};
MessageDisplay.prototype.display = function(sMsg){
    var msg = document.createElement("div");
    msg.appendChild(document.createTextNode(sMsg));
    this.area.appendChild(msg);
};

var procsNeeded = 2; 
var procsLoaded = 0;
// used to transform from XML to Grid View HTML
var xml2grid = new XSLTProcessor();
// used to transform the Grid View HTML to XML
var grid2xml = new XSLTProcessor();

// initialize everything, called on window load
function init() {
    log = new MessageDisplay("messages");
    log.display("Loading stylesheets...");
    prepareProcessor("xslt/xml2grid.xml", xml2grid);
    prepareProcessor("xslt/grid2xml.xml", grid2xml);
};
window.onload = init;

// prepares the given processor using 
// the XML from the given URL
function prepareProcessor(url, proc) {
    var xmlhttp = new XMLHttpRequest();
    xmlhttp.onreadystatechange = function() {
        if(xmlhttp.readyState == 4 ) {
            proc.importStylesheet(xmlhttp.responseXML);
            log.display("Loaded: "+url);
            if((++procsLoaded) == procsNeeded){
                log.display("All stylesheets loaded");
            };
        };
    };
    xmlhttp.open("GET", url, true);
    xmlhttp.send(null);
};

// loads the XML from the given URL
// and renders it as a Grid
function loadXmlFromUri(url) {
    document.getElementById("sourceView").style.display = "none";
	Sarissa.updateContentFromURI(url, document.getElementById("gridView"), xml2grid);
    document.getElementById("gridView").style.display = "block";
};

function showSourceView() {
    document.getElementById("gridView").style.display = "none";
	var source = document.getElementById("gridView").innerHTML;
	//alert(source);
	source = source
	    .replace(/<table/g, "<TABLE")
	    .replace(/<tbody/g, "<TBODY")
	    .replace(/<tr/g, "<TR")
	    .replace(/<td/g, "<TD")
	    .replace(/<textarea/g, "<TEXTAREA")
	    .replace(/<div/g, "<DIV")
	    .replace(/table>/g, "TABLE>")
	    .replace(/tbody>/g, "TBODY>")
	    .replace(/tr>/g, "TR>")
	    .replace(/td>/g, "TD>")
	    .replace(/textarea>/g, "TEXTAREA>")
	    .replace(/div>/g, "DIV>");
	//alert(source);
	
	document.getElementById("sourceView").value = source;
	var doc = (new DOMParser()).parseFromString(source, "text/xml");
	//alert("doc: \n"+new XMLSerializer().serializeToString(doc));
	//alert(doc.documentElement);
	var resultDoc = grid2xml.transformToDocument(doc);
	if(resultDoc.documentElement){
    	document.getElementById("sourceView").value = Sarissa.getText(resultDoc.documentElement, true);
	};
	//alert(source);
	//Sarissa.updateContentFromNode(doc, document.getElementById("sourceView"), grid2xml);
    document.getElementById("sourceView").style.display = "block";
};



function Sxe() {

};
 
Sxe.elemControlMouseOver = function(oCaller) {
    oCaller.style.border = "1px outset menu";
    oCaller.style.backgroundColor = "menu";
};
Sxe.elemControlMouseOut = function(oCaller) {
    oCaller.style.border = "1px solid transparent";
    oCaller.style.backgroundColor = "transparent";
};
// Used by buttons in the editable area
// Hides and shows the childnodes

Sxe.toggleVisibility = function (oCaller, oTarget) {
   if(oTarget.style.display != "none") {
       oTarget.style.display = "none";
   } else {
       oTarget.style.display = "block";
   };
   //nextFocus.focus();
};
function setContextNode(){};
