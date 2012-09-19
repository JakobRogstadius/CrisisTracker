<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/
?>
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