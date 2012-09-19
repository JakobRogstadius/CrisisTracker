/*******************************************************************************
* Copyright (c) 2012 CrisisTracker Contributors (see /doc/authors.txt).
* All rights reserved. This program and the accompanying materials
* are made available under the terms of the Eclipse Public License v1.0
* which accompanies this distribution, and is available at
* http://opensource.org/licenses/eclipse-1.0.php
*******************************************************************************/

	// ----------------------------------------------------------------------------------------
	// RANDOM DATA	
	 /**
     * Generates simple ID
     *
     * @return int
     */		
	function generateRandomID() {
		return (Math.floor(Math.random() * 100)); 		
	}	
		
	 /**
     * Generate Timestamp (seconds)
     *
     * @return int
     */		
	function generateTimeStamp() {
		var dateTime = "";  
		var dateT = new Date();
        dateTime = dateT.getYear()
        + '.' + (dateT.getMonth()+1)
        + '.' + (dateT.getDate())
        + '.' + (dateT.getHours())
        + (dateT.getMinutes())
        + (dateT.getSeconds());		
		return  "'"+dateTime+"'";	
	}
	
	 /**
     * Get Emergency Type
     * 
	 * @param identifier {int}
     * @return emergency_type {string}
     */	
	function getEmergencyType(identifier){
		var emergency_types = { 
			0: 'warning', 
			1: 'fire', 
			2: 'mudslide', 
			3: 'flood',
			4: 'earthquake'
		};
		return emergency_types[identifier];	
	}