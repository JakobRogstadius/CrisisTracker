<?php
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
	<p>We need participants who would like to help us test how well CrisisTracker can support geographically distributed
	volunteers to successfully organize information from social media into a format that can improve situation
	awareness among on-the-ground responders and professional analysts. If you wish to help, all you need to do is spend at least 15 minutes
	daily for one week tagging stories, and improving the way reports are grouped together by merging and splitting
	stories (as illustrated in the video).</p>
	<p>The preliminary duration of the study is September 7-14 and we will be tracking Twitter activity related to an ongoing crisis.</p>
	<p>During the study, the collaboratively organized live reports will be evaluated with
	the help of experienced professional disaster responders. At the end of the study, we will also be collecting
	feedback from you and other volunteers.</p>
	<p><i>We are also looking for <a href="study2.php">disaster response professionals</a>.</i></p>

	<h2>Registration</h2>
	<p>Please register below if you may be willing to participate. Further instructions will be provided closer to the study.</p>
	<form name="registrationForm" action="study.php" method="post" onsubmit="return validateForm()">
	<p>Name<br />
	<input type="text" name="name" style="width: 300px"/></p>
	<p>Contact information (email or Twitter @name)<br />
	<input type="text" name="email" style="width: 300px"/></p>
	<input type="submit" value="Register" />
	</form>
	<p><i>You can at any time withdraw from participation and your contact information will never be used beyond the scope of this study.</i></p>
    </div>
</div>

<?php
include('footer.php');
?>