/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

function testCSS(prop) {
    return prop in document.documentElement.style;
}

if ($.cookie("browsercheck") != "1") {
  var isOpera = !!(window.opera && window.opera.version);  // Opera 8.0+
  var isFirefox = testCSS('MozBoxSizing');                 // FF 0.8+
  var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0;
      // At least Safari 3+: "[object HTMLElementConstructor]"
  var isChrome = !isSafari && testCSS('WebkitTransform');  // Chrome 1+
  var isIE = /*@cc_on!@*/false || testCSS('msTransform');  // At least IE6
  
  if (!(isChrome || isFirefox)) {
      alert("CrisisTracker is a research prototype and we have limited development resources. Therefore the website only supports recent versions of Chrome and Firefox.");
  }
  
  $.cookie("browsercheck", "1");
}
