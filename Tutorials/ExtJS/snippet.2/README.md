# This Snippet is part of the CrisisTracker Project
This is the Snippet #2 - out of 3, which will give you the basic knowledge to allow you collaborate in our/yours project.

# Snippet 2- Asynchronous Data Handling
In this snipped we demonstrate a screen with 2 panels A and B where B is a Grid.
When the user clicks on a component of Panel A it fetches data via Ajax from a simple PHP API consisting of static PHP document. If the user clicks in A before a current call has completed it aborts the existing call(s) and starts a new one. The application then clears and repopulates the grid when data finishes loading


## ExtJS Documentation
This snipped uses ExtJS 4.1 Library
```http://docs.sencha.com/ext-js/4-1/#!/api```
[Browse Sencha ExtJS Documentation here](http://docs.sencha.com/ext-js/4-1/#!/api)


## Tutorial
We have used the ExtJS MVC Pattern to design this application.
1. Naming Conventions
	* Use only letters: 		
		Namespace & Class Names USE camelCase
		packages USE lower cased	
	* Do Not use: _,1212,-,

	* Examples of good practices:
		CompanySix.package.CoolPix
		CompanySix.package.cool.Window
		CompanySix.package.debug.Professional
	
2. Creating the Folder Structure
A basic ExtJS application consists on the following folder structure: 
	/app
		/controller
		/model
		/store
		/view
	/data
	/ext-4		*put library files here
	/resources 
	index.html	*create simple html5 file with <html/><head/><body/> as usual. Include *.js and *.css of ExtJs library
	app.js		*create an empty js file
	
3. Creating the Application Files
	3.1 APPLICATION
		Create app.js file by instantiting "Ext.application({..})" singleton
			Lets add a namespace name "mod" to the property "name" 
			On the "launch: function() {}"	of the singleton we add the initialization code for our application
				Lets create a ViewPort a specialized container representing the viewable application area 
				Our ViewPort will define a "VBOX" layout, which allows us to create vertical boxes in a top-down fashion with 2 Panel Items:
					Control Panel (with buttons to initiate fecth operations)
					Grid Panel
					
	3.2	CONTROLLER	
		Create GridController.js in "./controller/"	
			The controller holds references (as fields) for: views, stores, models.
			We need to add reference to all the views, stores and models which we want to be controlled by this controller. This is also an implicit form of specificying the *.js files which the application should load.
			The controller as an init() function which grants us access to the control function which allows us to add and manage listeners to events thrown by View components
		Add this controller reference to the app.js (Ext.Application class) 'controllers:[]'	property.	
				
									
	3.3	VIEW's
		3.3.1 GridPanel
			Let's create a folder to hold our GridPanel "./view/layout/" and create a file "GridPanel.js" inside it.
			The GridPanel extends the 'Ext.grid.Panel' class and by default the 'store' parameter cannot be null, so we need to create a store and reference it here. (refer to 3.4 STORES)
			
		3.3.2 ControlPanel
			Let's create a folder to hold our ControlPanel "./view/layout/" and create a file "ControlPanel.js" inside it.
			The controlPanel holds a button on it's items property.
			Add a reference of this Class Name to the 'views:[]' property of the corresponding Controller.
				
				
				
	3.4 MODELS
		Let's create a folder to hold our Model "./model/" and create a file "GridData.js" inside it.
		In it's 'field:[}' property it will hold each column data type of the grid.
		Add a reference of this Class Name to the 'models:[]' property of the corresponding Controller.	
		
		
	3.5 STORES		
		Let's add a Store 'GridAjaxProxy' which defines a proxy store to fetch data from our WebService API (in our case the static php file).
		We associate our defined 'MODEL' to this store in order to define the data template.
		We need to perform a little hack on the store, since we want the special behavior of aborting existing requests when a new request is placed and that's not normal behavior.
		In order to achieve this, we wrote code for the handler of the native store thrown event 'beforeload'  where we are able to abort the last request.		
		Add a reference of this Class Name to the 'stores:[]' property of the corresponding Controller.	
		
	3.6 DATA
		In order to place the webservice php file, we create an 'gridfeeder.php' inside the './data/' folder.
		
			
4. Controlling and Creating the Application Functionality
	As stated before, the application Controller allows us to listen to any event which is thrown by any VIEW component under the control of it's Controller.
	Inside the init() function of the controller we can access each VIEW component by making use of the ExtJS Component Query function.
	
	4.1 Listening to Buttons
	On the 'GridController.js' we listen to both control panel buttons events.
	We define a handler function to the buttons with a parameter "option" which holds a button instance, according to the clicked one
	Inside the handler function we check for the option config of the 'button' and determine wether is button 2 or 1
	
	4.2 Calling the Server via Ajax Call
	Inside the button event handler on 'GridController.js', it calls the load method of the grid store with parameters (which will be sent as $GET parameters).
	The parameter is the selected 'option', either 1 or 2, according to the pressed button which in the static php api file, will return different values in order to clearly see the behavior of this snippet functionality.
	As stated if you do sequential calls it will abort the old ones, leaving only the last one active.
	*Note: by default, when you do multiple calls it doesn't cancel the old ones, it would refresh the grid with the new data several times as soon as data is received from the multiple calls .
	

		
		

		
## Getting Started
*Note: This assumes you have ExtJS 4.1 or later installed properly and have a basic working knowledge of how to use JavaScript*

First you'll need to fork and clone this repo

```bash
git clone https://github.com/ccc/selfstarter.git
```

Let's get all our dependencies setup:
```bash
bundle install --without production
```

Now let's create the database:
```bash
rake db:migrate
```

Let's get it running:
```bash
rails s
```


### Tests

There aren't any tests yet. Tests are very welcome!