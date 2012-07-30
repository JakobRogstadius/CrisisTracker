// ----------------------------------------------------------------------------------------
// LOGIN CONTROL COMPONENT
/**
* Get active_user Global Variable
*
* @return user {string}
*/	
function getActiveUser() {
	return active_user;
}	

/**
* Generate Popup for User Login (via Ajax & Jquery)
*
*/		
function loginComponentActivator() {
console.log("Login Component Ready");	
	$(document).ready(function() {
		$('.login_activator').click(function() {
		
			if (!checkIfThereIsActiveUser()) {	// if there is no active user
			
				//Getting the variable's value from a link 
				var loginBox = $(this).attr('href');

				//Fade in the Popup
				$(loginBox).fadeIn(300);
				
				//Set the center alignment padding + border see css style
				var popMargTop = ($(loginBox).height() + 24) / 2; 
				var popMargLeft = ($(loginBox).width() + 24) / 2; 
				
				$(loginBox).css({ 
					'margin-top' : -popMargTop,
					'margin-left' : -popMargLeft
				});
				
				// Add the mask to body
				$('body').append('<div id="mask"></div>');	// inserts the specified content as the last child of each element in the jQuery collection
				$('#mask').fadeIn(300);
				
				return false;
			}
			else {
				return true;// do nothing
			}
		})
	})
}


// -------------------------- jquery VALIDATOR --------------------------
// checking that the name field is REQUIRED & email field is not only REQUIRED, but must be an email FORMAT
//$(document).ready(function(){
//if (!checkIfThereIsActiveUser()) {	// if there is no active user

$("#myform").validate({	// jQuery’s form validation - called over #myform
	debug: false,
	rules: {			// validation rules	
		username: "required",
		email: {
			required: true,
			email: true
		}
	},
	messages: {			// custom display message
		username: "Please let us know who you are.",	
		email: "A valid email will help us get in touch with you."
	},				
	submitHandler: function(form) {		// submitHandler() gets processed if the validation is successful								
		$.post('login/authentication.php', $("#myform").serialize(), // AJAX post method, sending '#myform' data to authentication.php					
			// SESSION STORED / ACTIVE
			function(data) {
console.log("Receiving SESSION via AJAX call from POST method of #myform processed via authentication.php");
				active_user = data;
				// Update login HTML information
				var user_div = document.getElementById('user_message');		
				user_div.innerHTML = 'Logged in: ';
				sessionUpdateDisplay(data);			
				console.log("Session Updated");						
				//user_div.innerHTML = "<?php print_r($_SESSION) ?>";		
				$('#user_logged').html(data);	// get back any results as HTML data to be displayed.																	
			}
		);					
	}
});			

// When clicking on the button close or the mask layer the popup closed		
//  Attach an event handler =>   .live( events, handler(eventObject) )
$('p, a.close, #mask').live('click', function() { 	// bind any current and future references of "#login-box" to the click event.			
  $('#mask, .login-popup').fadeOut(300 , function() {
	$('#mask').remove();  
}); 
return false;
});

// REMOVE SUBMIT BUTTON CLICK		
$("p").live("closePopupEvent", function(e, myName, myValue) {
  $('#mask, .login-popup').fadeOut(300 , function() {
	$('#mask').remove();  
}); 
return false;
});

$("#submit").click(function () {
  $("p").trigger("closePopupEvent");
});	

	/**
     * Checks if there is any active user in this console
     *
     * @return boolean {Boolean} 
     */	
	function checkIfThereIsActiveUser() {
		
		if (active_user != null) {
			return true;
		}
		else 
			return false;

		
		//return true;
	}
	
  /***
   * Generate Popup for User Login (via Ajax & Jquery)
   *
  ***/
  function loginComponentControl() {
    //BYPASS !!!!!!!!!!!!!!!!!!!
    return true;
    //BYPASS !!!!!!!!!!!!!!!!!!!
    console.log("Login Component Started");
    // 1 Layer Validation
    if (!checkIfThereIsActiveUser()) {    
      $("#tip5").click();      
    }
    else {
      console.log("There is an Active User");      
    }
    
    // 2 Layer Validation
    if(checkIfThereIsActiveUser()) {
      return true;
    }
    else {
      return false;  // user closed pop-up box
    }
  }	