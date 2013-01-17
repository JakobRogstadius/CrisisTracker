# This Snippet is part of the CrisisTracker Project
This is the Snippet #3 - out of 3, which will give you the basic knowledge to allow you collaborate in our/yours project.

# Snippet 3 - Web Map (ExtJS with OpenLayers)
In this snippet we have a dummy 'API' consisiting of a PHP document which takes an input a geographic bounding box (minlat,minlon,maxlat,maxlon) and returns 10 random positions within the bounding box.
We have a layout with 2 panels, a grid and a map.
The map panel holds an OpenLayers map component, with the data layer simply presented as points in specific coordinates.
As the user pans and zooms the map, the API is called with the new map bounding box to get new data. As data finishes loading, it clears the data layer and repopulates it with the new points.
This snippet will give you a decent understanding of event handling in OpenLayers and it's marriage with ExtJS.

## OpenLayers
OpenLayers is a small webmapping Client with minimal GUI
It allows you to add an embed simple interactive map with just a few lines of JS
It's quite flexible while being fast and allows extensions and theming
Good documentation and widespread usage around the world (solid enough)
It Supports SVG/VML/CANVAS and it's CrossBrowser

## GeoExt2
GeoExt enables building desktop-like GIS applications through the web. It is a JavaScript framework that combines the GIS functionality of OpenLayers with the user interface of the ExtJS library provided by Sencha.

## ExtJS Documentation
This snipped uses ExtJS 4.1 Library
A Very Sophisticated JS Library for WebApplications
```http://docs.sencha.com/ext-js/4-1/#!/api```
[Browse Sencha ExtJS Documentation here](http://docs.sencha.com/ext-js/4-1/#!/api)


---------------------------------------------------------------------------------------------------
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

# 1 - FILES AND FOLDERS
1. Create and Place GeoEXT, OpenLayers and ExtJS Libaries Locally
2. Create index.html and load the library *.js and *.css files in header 
3. Create app.js (aplication file of your ExtJS app which will be responsible for creating our "Ext.Application" instance

# 1.1 - APP.JS
In order to create our application, since will be making use of GeoExt library, we will call the setConfig method over the static class Ext.Loader to fetcb Ext and GeoExt source files. (mapping from namespaces to file paths)
Ext.Loader.setConfig({...});
* Ext.Loader is the heart of the new dynamic dependency loading capability in Ext JS 4+ *
Then we finally create our application instance.
Ext.application({...});		
We define our controllers here, this case the 'Map' controller
After that we define launch() function which will create a viewport with a BORDER layout containing several panels, namely 'mappanel' and 'gridPanel'.
These 'mappanel' and 'gridPanel' are the alias/shortcuts to the View classes of this components.

#1.2 - GRIDMAPCONTROLLER.JS (CONTROLLER)
In our default MVC folder structure, we create a folder 'app' and inside it the 'controller' folder
Create the file 'Map.js' which will be our Map Controller
This will be the name of the class 'CSMap.controller.GridMapController' (note: CSMap = our namespace defined in app.js)
Our controller will be used to manage map layer and the other view components
*Don't foger to a controller reference to the app.js file.
Inside this controller we listen for evens thrown by any ExtJS panel contained in our application components as the MapPanel in this example.
Special attention for the event name 'beforerender' thrown by any Panel component, but in this example it's important to intercept this event of the mapPanel, since we will add some code for the map in a handler before the mapPanel is rendered.
Also 'mapPan' event thrown by mapPanel is listened and handled here, where we will force the store to load with a parameter (current map bounding box).

#1.2.1 mapPanel - 'beforerender' EVENT HANDLER
Before the map panel is rendered on screen:
We create the layer objects for the map and i's associated styles.
We add some basic controls to the map.

#1.2.2 mapPanel - 'onMapPan' EVENT HANDLER
When user zoom or pans the map, it calls the API with the bounding box values.
This call is made from the 'MapItems' Store by calling it's load() method.
On the callback hanlder function, we send the records (coordinates data) to the map panel in order to render it.
*Note: mapPanel is accessed here via the 'Ext.ComponentQuery'.


#1.3 - MAP.JS (VIEW)
Inside the view folder create the file Map.js
We will define a Class 'CSMap.view.Map' for the map view, which extends 'GeoExt.panel.Map'
*Note: The GeoExt.panel.Map is Useful to define map options and stuff.
In this class we define ExtJS panel specific options plus OpenLayers map specific options.
We override the initComponent() function where we register the custom event 'mapPan' and create the OpenLayers map instance itself (assigned to a 'map' variable)
Here we also register the default event 'moveend' on OpenLayers map and define a handler to it.
Also in this class, we define a function to 'injectPoints' on the map's vector layer. this function receives an array of records as parameter. The records are supposed to be received in JSON format by the ExtJS application and then passed in raw (or partial raw) format that is, needs to have the 'record.data' field.

#1.3.1 view.map - 'mapMoveHandler' EVENT HANDLER
This event handler is thrown by OpenLayers map instance when the map is panned or zoomed		
We then fire an ExtJS event in order to be catched and handled by the application controller.


#1.4 - GRIDPANEL.JS (VIEW)
This is a simple view wich extends the default 'Ext.grid.Panel' by introducing a specific format of columns on the grid and referencing a store.

#1.5 - MAPITEM.JS (MODEL) 
This is a simple ExtJS model class which extends the default 'Ext.data.Model'. This class specificies the  fields for any "MAP ITEM" data object.

#1.6 - MAPITEMS.JS (STORE)
This is a simple ExtJS store class which extends the default 'Ext.data.Store'. This class specificies an access type of the application to the data on the server.
It will make possible to all view.components working with data.
It has our MapItem.JS model as it's predefined model.
This Store defines an Ajax PROXY with parameters (sent via $_gET) to a url 'data/features.php' which emulates a real webservice by returning random points with coordinates and meta data.
The defined proxy has a reader which is configured to read 'JSON' return datatype only.
If no valid JSON is returned, it will assume there is 0 records on the webservice response.
This store is linked to the application GridPanel and updates it automatically when it's "load" method is called.
		
		
4. Controlling and Creating the Application Functionality		
When you pan the map, an event is thrown by OpenLayers Map which fires an ExtJS event on the map panel (with parameters: boundingbox of the map)which is by instance handled in the application controller.	The controller then calls the load() method over the application store, with the parameters. Then we define a callback of the load method, in order to pass the returned JSON records (containing meta data and point coordinates) to the map panel in order to extract those coordinates and create OpenLayers Points and add them to the vector layer.
This behavior is repeated when there is any change on the map bounding box (that is, zooming or panning)
		
		
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