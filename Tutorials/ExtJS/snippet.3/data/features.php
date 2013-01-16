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
		
		if ($i < 3) {			
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
					),
					"id" => $i,
			);
		}
		else {
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
						"story" => "Story ".$i,
					),
					"id" => $i,
			);
		}			
		array_push($data['features'], $temp);		
	}	
	echo json_encode($data);	
}

else {

	$features = '{"type": "FeatureCollection", "features": [{"geometry": {"type": "Point", "coordinates": [2.18218, -35.5949]}, "type": "Feature", "properties": {"tags": 1770, "story": "Col dArclusaz"}, "id": 472}, {"geometry": {"type": "Point", "coordinates": [6.27827, 45.6769]}, "type": "Feature", "properties": {"tags": 1831, "story": "Pointe de C\u00f4te Favre"}, "id": 458}, {"geometry": {"type": "Point", "coordinates": [6.47122, 46.0062]}, "type": "Feature", "properties": {"tags": 2364, "story": "Pointe du Midi"}, "id": 487}, {"geometry": {"type": "Point", "coordinates": [26.82156068193, 46.3129835428]}, "type": "Feature", "properties": {"tags": 1856, "story": "Col dOutanne"}, "id": 5700}, {"geometry": {"type": "Point", "coordinates": [16.84989241629, 46.136626095]}, "type": "Feature", "properties": {"tags": 2375, "story": "Col de Comba Mornay"}, "id": 5644}, {"geometry": {"type": "Point", "coordinates": [26.62, -46.187778]}, "type": "Feature", "properties": {"tags": 2000, "story": "La Pointe"}, "id": 5710}, {"geometry": {"type": "Point", "coordinates": [5.9134, 44.93331]}, "type": "Feature", "properties": {"tags": 2607, "story": "Le Coiro"}, "id": 496}, {"geometry": {"type": "Point", "coordinates": [6.03219, 45.7204]}, "type": "Feature", "properties": {"tags": 1451, "story": "D\u00f4me de la Cochette"}, "id": 322}, {"geometry": {"type": "Point", "coordinates": [6.23048, 45.6324]}, "type": "Feature", "properties": {"tags": 2197, "story": "Mont P\u00e9cloz"}, "id": 515}, {"geometry": {"type": "Point", "coordinates": [16.1962, 45.6927]}, "type": "Feature", "properties": {"tags": 2181, "story": "Tr\u00e9lod"}, "id": 601}]}';
	
	echo ($features);

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