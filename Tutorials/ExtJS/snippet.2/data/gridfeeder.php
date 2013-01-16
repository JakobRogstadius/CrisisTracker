<?php

// Default return value
$data = "error";
	
// Read $_GET
if(isset($_GET['option'])){
	$option = $_GET['option'];

	// Sleeper
	for($i =0; $i<2; $i++) {
		sleep(1);
	}



	if ($option == 1) {
	
		// Static Data
		$data = array(
		"circles"=>
		array(
			array(
				"id" => 0,
				"circle" => 1,
				"location" => "PT",
				"component" => 34,
				"load" => rand(1,20)
			),
			array(
				"id" => 1,
				"circle" => 1,
				"location" => "PT",
				"component" => 35,
				"load" => rand(1,20)
			),
			array(
				"id" => 2,
				"circle" => 1,
				"location" => "BR",
				"component" => 49,
				"load" => rand(1,20)
			),
			array(
				"id" => 3,
				"circle" => 1,
				"location" => "BR",
				"component" => 49,
				"load" => rand(1,20)
			)	
		)		
		);	
	
	}
	else {
	
		// Static Data
		$data = array(
		"circles"=>
		array(
			array(
				"id" => 0,
				"circle" => 0,
				"location" => "XZ",
				"component" => 34,
				"load" => rand(1,20)
			),
			array(
				"id" => 1,
				"circle" => 1,
				"location" => "BR",
				"component" => 35,
				"load" => rand(1,20)
			),
			array(
				"id" => 2,
				"circle" => 0,
				"location" => "FR",
				"component" => 49,
				"load" => rand(1,20)
			),
			array(
				"id" => 3,
				"circle" => 0,
				"location" => "BR",
				"component" => 99,
				"load" => rand(1,20)
			)	
		)		
		);		
	
	}

}


// Json Export
echo json_encode($data);	


?>