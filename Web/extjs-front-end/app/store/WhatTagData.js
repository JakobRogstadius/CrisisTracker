Ext.define('CrisisTracker.store.WhatTagData', {
	storeId:'whatTag',
    extend: 'Ext.data.Store',	
	autoLoad: false,
	fields: ['left', 'right','toggled'],
	data: [
		{ 'left': 'Demonstration','right': 'Violence', 'toggled': "false"  },
		
		{ 'left': 'Missing','right': 'Torture/Rape', 'toggled': "false"  },
		
		{ 'left': 'Killed','right': 'Heavy Weapons', 'toggled': "false"  },
		
		{ 'left': 'Affected Infrastructure','right': 'People Movement', 'toggled': "false"  },
		
		{ 'left': 'Political Event','right': 'Risk/Hazard/Threat', 'toggled': "false"  },
		
		{ 'left': 'Summary Report','right': 'Eyewitness Report', 'toggled': "false"  },
		
		{ 'left': 'Rumor False','right': 'High Impact Event', 'toggled': "false"  }
		
	]
});