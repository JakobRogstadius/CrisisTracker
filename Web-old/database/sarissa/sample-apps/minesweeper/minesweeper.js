/**
 * ====================================================================
 * About
 * ====================================================================
 * All XSLT Minesweeper
 * @version @sarissa.version@
 * @author: Copyright Sean Whalen
 * ====================================================================
 * Licence
 * ====================================================================
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2 or
 * the GNU Lesser General Public License version 2.1 as published by
 * the Free Software Foundation (your choice of the two).
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License or GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * or GNU Lesser General Public License along with this program; if not,
 * write to the Free Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 * or visit http://www.gnu.org
 *
 */
 function init() {
     var elem;
     if (document.all) {
          elem = document.all.gameArea;

     } else if (document.getElementById) {
          elem = document.getElementById("gameArea");	 
	  window.captureEvents(Event.MOUSEUP); 
     };
     if (elem) {
        elem.onmouseup = listenUp;
	elem.onmouseover = enterButton;
	elem.onmouseout = exitButton;
     };
};


function getClickedElement(e) {
    var clicker;
    e = (e ? e  : ((window.event) ? window.event : ""));
    if (e.target) { 
        clicker =  e.target;
    }
    else {
        clicker = e.srcElement;
    };
    return clicker;
};


function exitButton (e) {
    var clicker = getClickedElement(e); 
    clicker.style.borderColor = borderCache;
};


function enterButton (e) {
    var clicker = getClickedElement(e);
    if (clicker.style.borderColor != "#FFFF00" ) { 
        borderCache = clicker.style.borderColor ;
    };
    clicker.style.borderColor = "#FFFF00";
};


function extrabutton() {
    alert("full map:  " +  new XMLSerializer().serializeToString( fragment));
};
 

function revealSquares(clicker){
    var xmlDoc = fragment;
    var ele = xmlDoc.createElement('click'); 
    ele.setAttribute('h', jDictionary[clicker.id].x);
    ele.setAttribute('v', jDictionary[clicker.id].y);
    xmlDoc.documentElement.appendChild (ele);
    var xmlRevealed  = xsltProcessor.transformToDocument( xmlDoc); 
    var eleID = "";
    var nbc = "";
    var nbcColor = "";
    var elSquare  = xmlRevealed.getElementsByTagName("square");
    var SqBtn =  jDictionary["1/1"].btn;
    var sqBtnStyle;
    for(var i=0; i< elSquare.length; i++) {
        squareAttrs = elSquare[i].attributes;
        eleID = squareAttrs.getNamedItem("h").nodeValue + "/" + squareAttrs.getNamedItem("v").nodeValue;
        nbc = squareAttrs.getNamedItem("nbc").nodeValue;
        SqBtn = jDictionary[eleID].btn;	
        sqBtnStyle = SqBtn.style;
        sqBtnStyle.border="inset";
        if (nbc !=0) {
            SqBtn.value =  nbc;
            sqBtnStyle.color = squareAttrs.getNamedItem("nbcColor").nodeValue ;
            sqBtnStyle.fontWeight = "900";
        };
        sqBtnStyle.backgroundColor = "#CDC9C9";
        sqBtnStyle.borderColor = "#DCDCDC";
    };
};


function listenUp (e ) {
    var iButton = 0 ;
    e = (e ? e  : ((window.event) ? window.event : ""));
    if (e.target) { 
        clicker =  e.target;
        iButton = e.which;
    }
    else {
        clicker = e.srcElement; 
        iButton  = e.button;
    };
    if (clicker.id == "ddbtn" ) {
        return;
    };
    if (clicker.type == "button") {
        if  (iButton  == 1  ) {	
            Outer_onMouseUp(clicker );
        }
        else {
            if  (jDictionary[clicker.id].isRevealed ==  0 ) {
                // this doenst work bc the dictionary is stale at this point.
                jDictionary[clicker.id].btn.style.backgroundColor="#F08080";
                jDictionary[clicker.id].isMarked = -1;
            };
        };
    };
};


function Outer_onMouseUp(clicker ) {
    var ibutton;
    var clicked =jDictionary[clicker.id];
    var btnStyle = clicked.btn.style;
    if (clicked.isRevealed  !=0 ) {
        return; 
    };
    if  (clicked.isBomb != -1 ) {		
        btnStyle.backgroundColor="#11CC22";
        btnStyle.border="inset";
        revealSquares(clicker);
    }
    else {
        btnStyle.backgroundColor="#BB2233";
    };
};


function userVals() {
    var hDefault = 15;
    var vDefault = 15;
    var bombDefault = 30;
    hMax = hDefault;
    vMax = vDefault;
    bombCount = bombDefault ;
    var bContinue = false;
    bContinue = !isNaN( parseInt(document.getElementById("userH").value));
    bContinue  = bContinue  & !isNaN( parseInt(document.getElementById("userV").value));
    bContinue  = bContinue  & !isNaN( parseInt(document.getElementById("userB").value));
    if (!bContinue) {
        alert ("One of your entries is not a number.");
    }
    else{
        hMax =  parseInt(document.getElementById("userH").value);
        vMax = parseInt(document.getElementById("userV").value );
        bombCount = parseInt(document.getElementById("userB").value );
    };
    bContinue   = hMax <= 50  & vMax <= 50 & bombCount  <= (hMax  * vMax );
    if ( bContinue ) {
        alertsON = 1;
        loadBoard();
    }
    else {
        alert ("One of your entries is too big.  H and V are limited to 50.  Bombs cannot exceed H*V.");
    };
};


function loadBoard() {
    jDictionary = new Object ();
    var xmlDocument =  Sarissa.getDomDocument();
    var xmlString = "<SweeperMap></SweeperMap>";
    xmlDocument = (new DOMParser()).parseFromString(xmlString, "text/xml");
    var ele = xmlDocument.createElement('range'); 
    ele.setAttribute('hMax', hMax);
    ele.setAttribute('vMax', vMax);
    xmlDocument.documentElement.appendChild (ele);
    var bombList = new Array();
    var ran_h;
    var ran_v;
    var found;
    for ( i = 0;  i < bombCount ; ) {
        ran_h =Math.round(Math.random() * (hMax -1) );
        ran_v =Math.round(Math.random() * (vMax -1) );  
        found = false;
        try {
            found = bombList[ran_h + "/" + ran_v];
            if (found !=true ) {
                bombList[ran_h + "/" + ran_v]= true;
                ele = xmlDocument.createElement('bomb'); 
                ele.setAttribute('h', ran_h);
                ele.setAttribute('v', ran_v);
                xmlDocument.documentElement.appendChild (ele);
                i++;
            };
        }
        catch(e) {
            alert ("assoc access error");
        };
    };
    var xslMapExpander =   Sarissa.getDomDocument();
    xslMapExpander.async = false; 
    xslMapExpander.load("MapExpander.xsl");
    var xsltProc  = new XSLTProcessor();
    xsltProc.importStylesheet(xslMapExpander);
    fragment = xsltProc.transformToDocument(xmlDocument);
    // second style sheet.
    var xslMapBuilder =   Sarissa.getDomDocument();
    xslMapBuilder.async = false; 
    xslMapBuilder.load("MapBuilderBombList.xsl");  // ?contentType=text/html
    xsltProc  = new XSLTProcessor();
    xsltProc.importStylesheet(xslMapBuilder);
    var frag2 = xsltProc.transformToDocument( fragment);
    // TODO use Sarissa to get the error
    if (Sarissa.getParseErrorText(frag2) != Sarissa.PARSED_OK) {
        alert ("err = " +   Sarissa.getParseErrorText(frag2));
    };
    document.getElementById("gameArea").innerHTML = ""; 
    //document.getElementById("gameArea").appendChild( xsltProcIE.transformToDocument(fragment)) ; 
    document.getElementById("gameArea").innerHTML =  new XMLSerializer().serializeToString(frag2);
    loadDictionary (fragment);
};

 
function cacheRevealer()  {
    //cache the 3rd style sheet:
    var xslRevealer =   Sarissa.getDomDocument();
    xslRevealer.async = false; 
    xslRevealer.load("RevealBombs.xsl");  // ?contentType=text/html
    var xsltProcessor = new XSLTProcessor();
    xsltProcessor.importStylesheet(xslRevealer);
    return xsltProcessor;
};


function jDictionary (sweeperSquare) {	 
 this.sweeperSquare = sweeperSquare;
};


function sweeperSquare (btn, x, y, nbc, isBomb) {
    this.btn = btn;
    this.x = x;
    this.y = y;
    this.nbc = nbc;
    this.isBomb = isBomb;
    this.isRevealed = 0;
    this.isMarked = 0;

};


function addEle(id, x, y, nbc, isBOmb) {
    jDictionary[id] = new sweeperSquare(document.getElementById(id), x, y, nbc, isBOmb);
};


function loadDictionary (allSquares) {
    squares = allSquares.documentElement.childNodes;
    var ele;
    for (i= 0; i < squares.length; i++) {
        ele = squares.item(i);
        addEle (ele.getAttribute("h") + "/" + ele.getAttribute("v"),
                ele.getAttribute("h") ,
                ele.getAttribute("v"),
                ele.getAttribute("nbc"),
                ele.getAttribute("isBomb")); 
    };
};

var hMax = 15 ;
var vMax = 15 ; 
var bombCount = 30;
var borderCache =0; 
var xml2;  
var fragment ;
var xslRevealingsheet;
var xsltProcessor ;
var alertsON= 0; 
 
 
function calledOnload(){
    init () ;
    loadBoard();
    xsltProcessor = cacheRevealer();
};
window.onload = calledOnload;
