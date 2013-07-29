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
	var survey=document.forms["registrationForm"]["survey"].checked;
	var interview=document.forms["registrationForm"]["interview"].checked;
	var deployment=document.forms["registrationForm"]["deployment"].checked;
	if (!(survey || interview || deployment))
	{
	    alert("Please select at least one form of participation.");
	    return false;
	}
	
	var name=document.forms["registrationForm"]["name"].value;
	var email=document.forms["registrationForm"]["email"].value;
	var role=document.forms["registrationForm"]["role"].value;
	if (name==null || name == ''
	    || email==null || email == ''
	    || role==null || role == '')
	{
	    alert("Please fill in your name, role and email.");
	    return false;
	}
    }
</script>

<?php
include('header_end.php');

$registered = false;
if (isset($_POST['role'])
    && isset($_POST['name'])
    && isset($_POST['email'])
    && (
	isset($_POST['survey'])
	|| isset($_POST['interview'])
	|| isset($_POST['deployment'])
    )) {
    $role = stripMaliciousSql($_POST['role']);
    $name = stripMaliciousSql($_POST['name']);
    $email = stripMaliciousSql($_POST['email']);
    $survey = 0;
    if (isset($_POST['survey']))
        $survey = intval($_POST['survey']);
    $interview = 0;
    if (isset($_POST['interview']))
        $interview = intval($_POST['interview']);
    $deployment = 0;
    if (isset($_POST['deployment']))
        $deployment = intval($_POST['deployment']);
  
    include('api/open_db.php');
    mysql_query("insert into ParticipantRegistrationProfessional
	(RegisteredAt, Name, Role, Email, Survey, Interview, Deployment) values
	(utc_timestamp(), '$name', '$role', '$email', $survey, $interview, $deployment);", $db_conn);
    include('api/close_db.php');

    $registered = true;
}

if ($registered) {
    echo '<div class="fullwidth-column"><div class="gui-panel textpanel">';
    echo "<p style=\"color: red; font-weight: bold;\">Thank you for registering! We will contact you shortly with more information about the study.</p>"; 
    echo '</div></div>';
}
?>

<div class="left-column-wide">
    <div class="gui-panel textpanel">
	<h1>Collaborative social media analysis for disaster response</h1>
	<iframe src="http://player.vimeo.com/video/45366518" width="570" height="321" style="border: 0" webkitAllowFullScreen mozallowfullscreen allowFullScreen></iframe>
	<p>During large-scale complex crises such as the Haiti earthquake, the Indian Ocean tsunami and the Arab Spring, social media has emerged as a source of timely and detailed reports regarding important events. However, individual disaster responders, government officials or citizens who wish to access this vast knowledge base are met with a torrent of information that quickly results in information overload. Without a way to organize and navigate the reports, important details are easily overlooked and it is challenging to use the data to get an overview of the situation as a whole.</p>
	<p>We (researchers at Madeira University, University of Oulu and IBM Research) believe that volunteers around the world would be willing to assist hard-pressed decision makers with information management, if the tools were available. With this vision in mind, we have developed CrisisTracker.</p>
	<p>CrisisTracker automatically collects millions of daily reports from Twitter and uses text processing and crowdsourcing to organize them around the 4Ws (who, where, what, when).</p>
	<p><i>For further information about CrisisTracker or the study, please contact PhD candidate Jakob Rogstadius at <a href="mailto:jakob@m-iti.org">jakob@m-iti.org</a>.</i></p>
    </div>
</div>
	
<div class="right-column-narrow">
    <div class="gui-panel textpanel">
	<h1>Upcoming field trial</h1>
	<p>Are you a professional intelligence analyst or decision maker in a disaster
  response organization, or a journalist trying to piece together scattered
  reports emerging from complex crises? Do you want to help test if clustered
  and crowd-curated reports from social media can support your work?</p>
	<p>We are looking for professional participants for an upcoming field trial
  where CrisisTracker will be used to track the Syrian Civil War. The duration
  of the study is September 7-14, during which volunteers from the Stand-By Task
  force (and others) will be working in the system to organize data.</p>

	<h2>Registration</h2>
	<p>Please indicate below if and how you are willing to participate. You can at any time withdraw from participation and your contact information will never be used beyond the scope of this study.</p>
	<form name="registrationForm" action="study2.php" method="post" onsubmit="return validateForm()">
	<p><input type="checkbox" name="survey" value="1" id="chksurvey"/> I am interested in watching a video demonstration of data analysis in CrisisTracker, and answering a survey about the tool's potential role in my work.</p>
	<p><input type="checkbox" name="interview" value="1" id="chkinterview"/> I am interested in participating in a 30-60 minute online interview which will discuss benefits and risks of using CrisisTracker to increase situation awareness in my professional role and organization.</p>
	<p><input type="checkbox" name="deployment" value="1" id="chkdeployment"/> I would like to use CrisisTracker during the one-week evaluation period to familiarize myself with the platform and the dataset. I would then like to share my reflections in a 60 minute Skype interview.</p>
	<p>Name<br />
	<input type="text" name="name" style="width: 300px"/></p>
	<p>In what professional role and organization do you work with disaster information management, or data analysis?<br />
	<input type="text" name="role" style="width: 300px"/></p>
	<p>Email<br />
	<input type="text" name="email" style="width: 300px"/></p>
	<input type="submit" value="Register" />
	</form>
    </div>
</div>

<?php
include('footer.php');
?>