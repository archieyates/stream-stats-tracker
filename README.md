# stream-death-counter
Some simple scripts that can be used in conjunction with Mix It Up to output detailed death statistics for a game

## Files
- **Stats.JSON**: An example file of my own playthroughs which is referenced by the Powershell scripts
- **GetStats.ps1**: Powershell script run by chat commands (facilitated through **Mix It Up**)
- **UpdateStats.ps1**: Powershell script for updating stats run via my **Stream Deck**

## Stats.JSON
**Stats.JSON** contains all the core stat data for the playthroughs done on stream. New playthroughs currently need to be added manually but there are several properties that can be updated via other script commands.

The current properties are:
- _Lookup_: The lookup name for the playthrough used to identify it
- _Game_: The name of the game itself
- _Deaths_: How many deaths have happened in this playthrough (automated)
- _Sessions_: How many sessions have been spent on this game (automated)
- _Status_: Is this playthrough currently in-progress, current, complete etc. (partially automated)
- _VOD_: Link to the Youtube playlist of the VODs
- _Playtime_: How long the playthrough took

## GetStats.ps1
The main function of **GetStats.ps1** is to allow Twitch viewers to type commands and arguments which can then retrive specific data about game playthroughs. This started off as just a Death Counter but has expanded to include a more general set of stats for these playthroughs.

All commands are triggered by users with _!stats [arg]_ where _[arg]_ can be any of the following:
- _Total_: Prints summed totals for deaths and sessions of all playthroughs
- _Sheet_: Prints a link to an online spreadsheet of the death data
- _Random_: Provides the stats of a random playthough in the list
- _Current_: Provides the stats of the current playthrough in the list
- _Scheduled_: Provides the sats of the next scheduled playthrough in the list (should it exist)
- _List_: Prints a list of playthroughs that can be used for the _[game]_ arg
- _[game]_: Uses the arg as the _Lookup_ name for **Stats.JSON**

## UpdateStats.ps1
**UpdateStats.ps1** Is an automation tool I use with my Stream Deck to handle updating death counters. Rather than using just simple Text file updating I run this script which will update the **Stats.JSON** file before populating that info to text files that are read by my OBS overlay. As I track Current Game, Current Boss, and Total counts I've found it easier and faster to use a Powershell script as opposed to a multi-action inside Stream Deck.

**UpdateStats.ps1** can be called with a variable number of arguments that are parsed differently. The commands are:

- _death_: Update the Death Counter in some form
  - _add_: Add a death to the current game and total
    - _boss_: Also add a death to the current boss count
  - _subtract_: Subtract a death from the current game and total
    - _boss_: Also subtract a death from the current boss count
- _setcurrent_: Updates which game is the current game (marking previous current as "In-Progress"
  - [_game_]: The name of the playthrough (using the _Lookup_ property)
- _sessions_: Increase the _Session_ count for the current playthrough

## Future
There is more I want to add to this including:
- Adding new playthroughs via Powershell
- Marking a game as Complete and setting the playtime via Powershell
- Adding per-boss tracking with dynamically addable bosses to **Stats.JSON**
