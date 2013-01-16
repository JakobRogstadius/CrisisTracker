# This Snippet is part of the CrisisTracker Project
This is the Snippet #1 - out of 3, which will give you the basic knowledge to allow you collaborate in our/yours project.

# Snippet 1- ExtJS Basic Rendering and Event Handling
In order to show basic rendering and event handling using ExtJS we have a Canvas where a pair of circles can be drawn severall times. 
You can select a circle with a "mouse click" and the selected circle will become colored with a different color.
The Circles have a "setSelectedIndex" method which is called on click, and can also be called programatically on it's instance.
The circles can be instantiated several times.
Each instance of the circles can throw a "selectedindexchanged" event, which is listened by the CONTROLLER of the Canvas and later propagated to all Circles instances, so that controls have linked selection behavior.


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
					Canvas (to draw the circles)
					Control Panel (with buttons to add new circles)
					
	3.2	CONTROLLER	
		Create CirclesController.js in "./controller/"	
			The controller holds references (as fields) for: views, stores, models.
			We need to add reference to all the views, stores and models which we want to be controlled by this controller. This is also an implicit form of specificying the *.js files which the application should load.
			The controller as an init() function which grants us access to the control function which allows us to add and manage listeners to events thrown by View components
		Add this controller reference to the app.js (Ext.Application class) 'controllers:[]'	property.	
		
									
	3.3	VIEW's
		3.3.1 Circles
			Let's create a folder to hold our Circles "./view/shapes/" and create a file "Circles.js" inside it. We will be creating a custom Class ''mod1.view.shapes.Circles' which extends ExtJS native 'Ext.draw.Component' which will allow us to draw circles.			
			This class "Mixes-In" the functionality of the native ExtJS 'Ext.util.Observable' Class via the base class "mixins" property.
			We add some custom properties via the base class "config" property, as also statics via "statics" property
			Our Circles View Component has a property: 'child_circles' which holds 2 circles
			We add the method 'setSelectedIndex(index,propagate)'
			We add the method 'drawCircles'
			We also add some event listeners to the 'listeners' literal object of the class
			
		3.3.2 ControlPanel
			Let's create a folder to hold our ControlPanel "./view/layout/" and create a file "ControlPanel.js" inside it.
			The controlPanel holds a button on it's items property.
			Add a reference of this Class Name to the 'views:[]' property of the corresponding Controller.			
			
		3.3.3 Canvas
			Let's create a folder to hold our ControlPanel "./view/layout/" and create a file "Canvas.js" inside it.
			This Canvas Panel will hold the circles.
			Add a reference of this Class Name to the 'views:[]' property of the corresponding Controller.
		
		

			
4. Controlling and Creating the Application Functionality
	As stated before, the application Controller allows us to listen to any event which is thrown by any VIEW component under the control of it's Controller.
	Inside the init() function of the controller we can access each VIEW component by making use of the ExtJS Component Query function.
	
	4.1 Add Circles Button
		We will listen to the 'click' event which is thrown by our button on the control panel and defined a handler which will draw (create an instance of) circles on the canvas.
		We will get a reference to our canvas also via the powerful ExtJS Component Query.
	
	4.2 Click on Circles
		We have defined in our Circles class constructor a custom event type via 'this.addEvents('selectedIndexChanged');'.
		We have also defined in the 'listeners:{}' property of our Circles Class, a listener to the native "click" event over the component (which via some trivial code we get the id of the clicked circle). This listener fires our custom event, 'this.fireEvent("selectedIndexChanged", i, this.id );' with the parameters: "component id, circle id".
		
		Going back to our CirclesController, we will listen to that event ('selectedIndexChanged') thrown when a user clicks over any circle on the canvas. Analogous we will define a handler function to it. 
		
		Now we define a handler function 'onCircleChanges' which get's the parameters (circleId, componentId), goes througout all circle instances drawn on canvas and calls a method on all of them  'setSelectedIndex(index,propagate)' in which the individual components will be responsible for selecting the corresponding circle index.
		
		

		
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