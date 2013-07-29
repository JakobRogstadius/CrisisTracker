<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

include('header_start.php');
?>

<script type="text/javascript">
    function validateForm()
    {	
	var name=document.forms["registrationForm"]["name"].value;
	var email=document.forms["registrationForm"]["email"].value;
	if (name==null || name == ''
	    || email==null || email == '')
	{
	    alert("Please fill in your name and contact information.");
	    return false;
	}
    }
</script>

<?php
include('header_end.php');

$registered = false;
if (isset($_POST['name'])
    && isset($_POST['email'])
    ) {
    $name = stripMaliciousSql($_POST['name']);
    $email = stripMaliciousSql($_POST['email']);
  
    include('api/open_db.php');
    mysql_query("insert into ParticipantRegistrationVolunteer
	(RegisteredAt, Name, Contact) values
	(utc_timestamp(), '$name', '$email');", $db_conn);
    include('api/close_db.php');

    $registered = true;
}

if ($registered) {
    echo '<div class="fullwidth-column"><div class="gui-panel textpanel">';
    echo "<p style=\"color: red; font-weight: bold;\">Thank you for registering! We will contact you closer to the study with more information.</p>"; 
    echo '</div></div>';
}
?>


<div class="fullwidth-column">
    <div class="gui-panel textpanel">
      <h1>Full-scale evaluation deployment, September 7-14</h1>
      <p>During September 7-14, 2012, We are running a first-ever full-scale
      deployment of CrisisTracker to turn real-time Twitter reports from the
      ongoing civil war in Syria into an organized 4W (who, where, what, when)
      database. During this week over 80 volunteers from the Stand-By Task Force
      and elsewhere are collaborating to organize reports in the system. At the
      same time, experienced disaster response professionals help to assess the
      utility of the tool and the resulting dataset.</p>
      <p>We welcome you to use the system during this time to familiatize
      yourself with its functionality, or to participate in the evaluation.</p>
      <p><a href="evaluation-volunteer.php">Participate as a volunteer</a> |
      <a href="evaluation-expert.php">Participate as a domain expert</a></p>
      <p><i>For further information about CrisisTracker or the study, please
      contact PhD candidate Jakob Rogstadius at <a href="mailto:jakob@m-iti.org">jakob@m-iti.org</a>.</i></p>
    </div>
</div>

<?php
include('footer.php');
?>