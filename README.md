# ActionGraph
A simple (work-in-progress) visual tool for crafting and structuring finite state machines in Unity in a modular way by creating states from a series of small, self-contained and re-usable actions.

## Installation

Simply clone or download this repository and put everything (the ActionGraph folder) in your Unity Assets folder. That's it!

## Usage

ActionGraph is still very much a work in progress and I would not currently reccomend using for anything other than a a learning resource at the moment. That being said, here's a guide to getting started:


### Creating a new Graph

* Add a **Graph** component to a GameObject in your scene 
* Click the 'Open Editor' button or go to Window -> ActionGraph Editor to bring up the editor window
* Right-click anywhere to add a node (a container for a series of actions)
	* The only difference between a Standard Node and a Transition Node is the latter will cause Graph.IsTransitioning to return true while it is running
* Click the 'Add New Action' button to pick from any of the Actions in your project
* This will bring up the ActionGraph Inspector (AGInspector) window which is similar to the standard Unity inspector but used to modify ActionGraph elements such as Actions and Conditions
* Left-click and drag from one of the small white circles surrounding a node to create a connection between two nodes
* Left-click the larger white circle in the middle of the connection to modify the Conditions that are required for that connection to execute 

### Writing custom Actions and Conditions

* Custom Actions should inherit from the Action class and can implement *OnStart*, *OnUpdate*, and *OnFinish* to execute their logic When the action has done its work it should call *FinishAction* (which is what will trigger *OnFinish* to be called)
* Custom Conditions should inherit from the Condition class and need only implement *Check* which returns true or false

### Extra Notes

* **WARNING:** ActionGraph doesn't always save changes automatically so be sure to use the 'Save' button in the top-left corner of the ActionGraph window regularly.
* You can set which node should execute first by right-clicking a node and selecting 'Make Start Node'
