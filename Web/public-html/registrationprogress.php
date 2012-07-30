<?php
include('header_start.php');
include('header_end.php');
?>

<div class="fullwidth-column">
    <div class="gui-panel textpanel">
	<h1>Recruitment progress, professional practitioners</h1>
	<div class="gui-subpanel">
	    <?php dumpTable("ParticipantRegistrationProfessional"); ?>
        </div>
	<h1>Recruitment progress, volunteers</h1>
	<div class="gui-subpanel">
	    <?php dumpTable("ParticipantRegistrationVolunteer"); ?>
        </div>
    </div>
</div>

<?php
include('footer.php');
?>