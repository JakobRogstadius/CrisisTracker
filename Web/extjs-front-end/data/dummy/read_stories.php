<?php

$id_arr[] = null;

// Read $_GET
if(isset($_GET['boundingbox'])){
	$boundingBox = $_GET['boundingbox'];	

	// Sleeper
	for($i =0; $i<2; $i++) {
		sleep(1);
	}	
	
	// Get Array Values
	foreach (explode("&", $_SERVER['QUERY_STRING']) as $tmp_arr_param) {
		$split_param = explode("=", $tmp_arr_param);
		if ($split_param[0] == "boundingbox") {
			$id_arr[] = urldecode($split_param[1]);
		}
	}
	
	// Return Data
	$data = array (
		"type" => "FeatureCollection",
		"features" => 
		array()			
	);	
		

	for ($i = 1; $i <= 5; $i++) {
		$coordinates = rand_coord();
					
		$temp = array(
			"geometry" => 
			array (
				"type"	=> "Point",
				"coordinates" => $coordinates,
				),
				"type" => "Feature",
				"properties" => 
				array (	
					"tags" => 1300,
					"story" => "Story".$i,
					"story_id" => $i,
					"start_time" => 10001203,
					"start_time_str" => "10:0".$i,
					"popularity" => 200+$i,
					"max_growth" => 100+$i,
					"title" => "Example Title".$i,
					"custom_title" => "Example Title".$i,
					"category_count" => $i*2,
					"entity_count" => $i*3,
					"keyword_count" => $i*4					
				),
				"id" => $i,
		);
			
		array_push($data['features'], $temp);		
	}	
	echo json_encode($data);	
}

else {
	echo "no bounding box provided";
}

// Random Coordinates Generator
function rand_coord()  { 
        //$lon = rand(1,128 * 156543.0339); 
		$lon = rand(1,$GLOBALS['id_arr'][1]);	//left, right
        $lat = rand(1,$GLOBALS['id_arr'][2]);	// bottom, top
		$coordinates = array();
		array_push($coordinates, $lon);
		array_push($coordinates, $lat);
	
        return $coordinates; 
}



?>