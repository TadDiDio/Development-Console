# Developer Console

A **powerful and customizable developer console** for debugging, monitoring, and executing commands in your application.

---

## Table of Contents

1. [Features](#features)  
2. [Installation](#installation)  
3. [Usage](#usage)  
4. [Configuration](#configuration)  
5. [Examples](#examples)  

---

## Features

- **Command Execution**: Run custom and predefined commands.  
- **Customizable**: Add new commands and tweak the console configuration to your needs.  
- **Lightweight & Fast**: Minimal performance impact on your app.  
- **Smart Compile and Bootstrapping**: No need to do any set up once imported, simply press '/' to access the console. Unticking the 'Development Build' checkbox in build settings automatically disables the bootstrapper.
- **Zero Coupling**: The system uses reflection extensively to remove the need for placing console code in next to application code. Once you write your application code, you can invoke public or private methods and set public or private fields without touching or exposing anything in the application class using the command you write. The base class Command provides lots of helpful methods for doing this.

---

## Installation

Simply import the pacakge using Unity's package importer in the assets browser.

### Prerequisites
- TMPro

## Usage
After importing the project, **DO NOT** add the developer console prefab to any scene. A bootstrapper will automatically do this when you enter playmode. The bootstrapper will be enabled in development builds but automatically disabled in release builds. 
Pressing the '/' key enables or disables the console. After enabling it, enter the command "reg" to see a list of all available commands, then enter "help <command>" where command is any command listed by reg to see how to use it.

## Configuration
Customize your console by using the console config Scriptable Objects. Make sure all of these are stored in a Resources/Configs directory somewhere in the project. You can also define a startup file. This 
must be called on_console_start and located in a directory called Resources/System.

## Examples
For a complete example on adding new commands, see the ExampleCommand shipped with the console. To create a new command, right click in the assets browser and select Create->Developer Console->New Command. 
To create a new console configuration, right click in the assets browser and select Create->Developer Console->New Console Config. 