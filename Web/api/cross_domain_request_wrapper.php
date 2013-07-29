<?php
/*******************************************************************************
 * Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://opensource.org/licenses/eclipse-1.0.php
 *******************************************************************************/

header('Access-Control-Allow-Origin: *');

$url = $_GET['url'];
$parsedUrl = parse_url($url);
$scheme = $parsedUrl['scheme'];
if ($scheme == 'http' || $scheme == 'https') {
    echo file_get_contents($url);
}
?>