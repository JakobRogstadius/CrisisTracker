	// CHANGES REVERSION MODULE ------------------------------------------------------------------------------	 
	 /**
     * Changes Reverter Controller
     * 	
	 * @param: command {String}
     */		
	function revertChanges(command) {
		if (command == 'drag') {
			dragUnSucessfull();
		}	
		else if (command == 'remove') {
			removeUnSucessfull();
		}
		else if (command == 'add') {
			addUnSucessfull();
		}
	}	
	
	 /**
     * Starts the Counter at 5
     * 
     */			
	function startCounterValidation(startValue, action) {
		command = action;
		reset = false;
		CounterStart = 0; //startValue;	// start value
		//openPopup(50,50);	// ww hh
		// Add HTML Remove Button Click Listener
		//document.getElementById('cancel_btn').addEventListener('click', handleCancelButton, false);		
		setTimeout('decrementCounter()', 0);//1000);	
	}
	 
	 /**
     * you want done every second.
     * 	
     */			 
	function toDoWhileCounting() {		
		timer_div = document.getElementById('timeleft');
		timer_div.innerHTML = CounterStart + " seconds remaining!";	
		//console.log(CounterStart);
		//popup.document.timer.timeleft.value = CounterStart;		
		//console.log("counting down..");
		// other things lol
	}

	 /**
     * you want done every second.
     * 	
     */		
	function decrementCounter() {
		if (!reset) {
			CounterStart--;
			//toDoWhileCounting();
			if(CounterStart <= 0) {
				console.log('Changes Submitted!!');
				if (command == 'drag') {
					dragSucessfull(environment.final_lonlat);
				}				
				else if (command == 'remove') {
					removeSucessfull();
				}
				else if (command == 'add') {
					addSucessfull();
				}
				else  {
					closeCancelPopup();
				}								
				//closeCancelPopup();				
			}
			else { setTimeout('decrementCounter()',1000); }	// 1 secound delay
		}
	}
	
	 /**
     * Close Cancel PopUp
     * 	
     */				
	function closeCancelPopup() {
		reset = true;
		environment.map.removePopup(popup_cancel);		
	}
	 
	 /**
     * Open the Popup window
     * 	
	 * @param: width, heigth {integer, integer}
     */	
	function openPopup(w,h){
		// Generate HTML for popup
		var html = '<html><head>';
			html += '<link rel="stylesheet" href="resources/css/timeoutbox_style.css" type="text/css">';
			html += '</head>'; 
			html += '<body>';			
			html += '<div id=timeout>';
				html += '<form name="timer">';
					html += '<input type="button" href="#" class="mistake" id="cancel_btn" name="cancel_btn" value="CANCEL NOW!"/>';
				html += '<div id="timeleft" />';
			html += '</form> </div>';		
		html += '</body></html>';
				
		// Popup.FramedCloud @params: {id, lonlat, contentSize, contentHTML, anchor, closeBox, closeBoxCallback}		
        popup_cancel = new OpenLayers.Popup.Anchored("featurepop", 
           environment.map.getCenter(),
            null,	// contentSize
			html,          
            null, false);	// Close callback function: handleFeaturePopupClose
		popup_cancel.panMapIfOutOfView = false;
		popup_cancel.setSize(new OpenLayers.Size(150,50));	//ww,hh
		//popup_cancel.moveTo(new OpenLayers.Pixel(0,0));
		
		popup_cancel.updateRelativePosition();
        environment.map.addPopup(popup_cancel);
	}