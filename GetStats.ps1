# Read the stats JSON
$stats = Get-Content "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Stats.json" | ConvertFrom-Json

# Get the stats of a game
function Get-GameStats 
{
    param ($game )
    $stat = $stats.Playthroughs | Where-Object {$_.Lookup -eq $game} | Select-Object -First 1

    if($stat)
    {
        if($stat.Playtime)
        {
            Write-Host "Game: $($stat.Game) | Status: $($stat.Status) | Playtime: $($stat.Playtime) | Sessions: $($stat.Sessions) | Deaths: $($stat.Deaths) | VODs: $($stat.VOD)"
        }
        else
        {
            Write-Host "Game: $($stat.Game) | Status: $($stat.Status) | Sessions: $($stat.Sessions) | Deaths: $($stat.Deaths) | VODs: $($stat.VOD)"
        }
    }
    else
    {
    Write-Host "Playthrough not found. Use '!stats list' to get the list of playthroughs or '!stats args' for all possible arguments"
    }
}

# Parse the arguments
if($args)
{
    if($args[0] -eq "args")
    {
        # Write out the commands that can be parsed
        Write-Host "Arg List: 'TOTAL' (summed stats), 'SHEET' (Link to spreadsheet), 'RANDOM' (stats for a random game), 'CURRENT' (Current Playthrough's stat),'SCHEDULED' (Next planned playthrough),'LIST' (List of all playthroughs), '<game>' (use 'list' to get games list)"
    }
    elseif ($args[0] -eq "list")
    {
        # Get a list of all playthroughs
        $output = "";
        foreach($session in $stats.Playthroughs)
        {
            $output += $session.Lookup + ", " 
        }        
        Write-Host "Playthrough List: $($output)"
    }
    elseif ($args[0] -eq "total")
    {
        # Calculate and output the total deaths and sessions
        $deaths = 0;
        $sessions = 0;
        foreach($session in $stats.Playthroughs)
        {
            $deaths += $session.Deaths;
            $sessions += $session.Sessions;
        }

        Write-Host "Total Deaths:$($deaths) | Total Sessions:$($sessions)"
    }
    elseif ($args[0] -eq "sheet")
    {
        # External spreadsheet of the death stats
        Write-Host "Detailed Spreadsheet available here: https://docs.google.com/spreadsheets/d/14ZpB5ZOm6qGx6onCp0KD-13XRtigpmJOHf8WwDqlMcI/edit?usp=sharing"
    }
    elseif ($args[0] -eq "random")
    {
        # Get a random playthrough and the stats for it
        Get-GameStats((Get-Random -InputObject $stats.Playthroughs).Lookup)
    }
    elseif ($args[0] -eq "current")
    {
        # Find the playthrough currently being played and get its stats
        $stat = $stats.Playthroughs | Where-Object {$_.Status -eq "Current"} | Select-Object -First 1
        if($stat)
        {
            Get-GameStats($stat.Lookup)
        }
        else
        {
            Write-Host "No game currently being tracked"
        }
    }
    elseif ($args[0] -eq "scheduled")
    {
        # Find the playthrough currently being played and get its stats
        $stat = $stats.Playthroughs | Where-Object {$_.Status -eq "Scheduled"} | Select-Object -First 1
        if($stat)
        {
            Get-GameStats($stat.Lookup)
        }
        else
        {
            Write-Host "No game currently scheduled"
        }
    }
    else
    {
        # Assume that the argument is a game
        Get-GameStats($args[0]);
    } 
}
else
{
    Write-Host "No Argument. Use '!stats list' to get the list of playthroughs or '!stats args' for all possible arguments"
}
