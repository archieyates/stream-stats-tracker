# Read the stats JSON
$stats = Get-Content "INSERT_PATH_TO_JSON_HERE" | ConvertFrom-Json

# Get the stats of a game
function Get-GameStats 
{
    param ($game )
    $stat = $stats.Playthroughs | Where-Object {$_.Lookup -eq $game}

    if($stat)
    {
    Write-Host "Game:"$stat.Game" | Status:"$stat.Status" | Sessions:"$stat.Sessions" | Deaths:"$stat.Deaths
    }
    else
    {
    Write-Host "Argument not recognised. Use '!stats list' to get the list of playthroughs or '!stats args' for all possible arguments"
    }
}

# Parse the arguments
if($args)
{
    if($args[0] -eq "args")
    {
        Write-Host "Arg List: 'DEATHS' (total deaths), 'SHEET' (Link to spreadsheet), 'RANDOM' (stats for a random game), 'LIST' (List of all playthroughs), '<game>' (use 'list' to get games list)"
    }
    elseif ($args[0] -eq "list")
    {
        # Get a list of all playthroughs
        $output = "";
        foreach($session in $stats.Playthroughs)
        {
            $output += $session.Lookup + ", " 
        }        
        Write-Host "Playthrough List: "$output
    }
    elseif ($args[0] -eq "deaths")
    {
        # Calculate and output the total deaths
        $count = 0;
        foreach($session in $stats.Playthroughs)
        {
            $count += $session.Deaths;
        }

        Write-Host "Total Deaths: "$count
    }
    elseif ($args[0] -eq "sheet")
    {
        Write-Host "Detailed Spreadsheet available here: "
    }
    elseif ($args[0] -eq "random")
    {
        # Get a random playthrough and the stats for it
        Get-GameStats((Get-Random -InputObject $stats.Playthroughs).Lookup)
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
