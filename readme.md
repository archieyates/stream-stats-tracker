# Stream Stat Tracker

Stream Stat Tracker provides a console application for tracking various data during game playthroughs.
The core functionality revolves around tracking deaths and boss encounters although some additional data such as session counts, and VOD links are supported.

Stream Stat Tracker requires .NET 6.0 or later in order to run which you can download [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

## Running The App
When running SST for the first time it will automatically generate several files that are used as part of its tracking process inside a new folder called **Stats**.

- Playthroughs.json
- Bosses Folder
- Deaths Folder
	- Boss.txt
	- Game.txt
	- Total.txt

These are the files/folders that are used by the program to manage the stats.

## Files
### Playthroughs.json
Playthoughs.json is where the core data relating to your playthroughs is located.
Each playthrough stored here can contain any of the following properties.
- _Lookup_: The lookup name for the playthrough used to identify it (this needs to be unique)
- _Game_: The name of the game itself
- _Deaths_: How many deaths have happened in this playthrough
- _Sessions_: How many sessions have been spent on this game
- _Status_: Is this playthrough currently in-progress, current, complete etc.
- _VOD_: Link to the Youtube playlist of the VODs
- _Playtime_: How long the playthrough took

It is worth noting that currently _VOD_ and _Playtime_ are not automatically supported with the app but will be in the future.
If you want to add this data you need to just manually edit the json file.

### Bosses Folder
The Bosses folder is where the json files for each Playthrough's bosses are saved.
When a new Playthrough is created a boss file will be automatically generated using the _Lookup_ of the playthrough as the file name.
Each boss contains the following properties
- _Lookup_: The lookup name for the boss used to identify it (this needs to be unique)
- _Boss_: The name of the boss
- _Deaths_: How many deaths have happened to this boss
- _Status_: Is this Boss "Defeated", "Undefeated" or "Current"

## Deaths Folder
Within the Deaths folder there are 3 text files: **Boss.txt**, **Game.txt**, and **Total.txt**.
These text files can be read from by a program like OBS in order to display the death count.

**Boss.txt** tracks the deaths of the current boss. 
If there isn't a current boss then this will just be 0.

**Game.txt** tracks the deaths of the current game/playthrough.
If there isn't a current game then this will just be 0.

**Total.txt** tracks the total deaths across all playthroughs.
This is automatically summed anytime the deaths change.

## Layers
Stream Stat Tracker uses text commands entered into the console in order to update its stats.
You can consider the app has having several "layers" of commands where one command will take you to the next set of commands.
The layers can be broken down as follows:

- _Top_: The first layer used to work out what general command you want
- _Game_: Sublayer that handles anything related to the game/playthrough
- _Boss_: Sublayer that handles anything related to the bosses of the current game/playthrough
- _Death_: Sublayer that handles anything related to the death counting

Each of these layers can accept a variety of different commands.
Using the command _help_ in the Top layer will print out the full list of commands as well as which layers they belong to.

## Top
The Top layer is the first layer in the application.
After performing an operation the app will return to this layer.
The full list of commands (some support multiple entries) are:
- _[game, playthrough]_: Perform actions on the playthrough data (using Game Layer)
- _[boss, bosses]_: Perform actions on the boss data for the current playthrough (using Boss Layer)
- _[death, deaths]_: Perform actions on the death counts (using Death Layer)
- _[++]_: Increment the death count shortcut
- _[--]_: Decrement the death count shortcut
- _[help, commands]_: List help

## Game
The Game layer or Playthrough (these words are kind of interchangable but the code uses Playthrough) updates the main data for the playthrough.
The full list of commands are:
- _[new]_: Create a new playthrough
- _[list]_: List all the playthroughs
- _[current]_: Set the current playthrough
- _[complete]_: Complete the current playthrough
- _[sessions, session]_: Update the session count for current playthrough
- _[delete]_: Delete a specified playthrough
- _[esc]_: Return back to main

## Boss
The Boss layer updates data for the bosses of the current playthrough.
If there is no current playthrough then these commands will not run.
The full list of commands are:
- _[new]_: Create a new boss (sets to current)
- _[list]_: List all the bosses for this playthrough
- _[current]_: Set the current boss
- _[complete]_: Mark current boss as defeated
- _[delete]_: Delete a specified boss
- _[esc]_: Return back to main

## Death
The Death layer updates the death counts for the current playthrough and boss.
If there is no current playthrough then these commands will not run.
- _[add, ++]_: Increment the death count
- _[subtract, --]_: Decrement the death count
- _[esc]_: Return back to main
