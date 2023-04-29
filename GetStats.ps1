# Read the stats JSON
$stats = Get-Content "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Stats.json" | ConvertFrom-Json

# Get the stats of a game
function Get-GameStats 
{
    param ($game )
    $stat = $stats.Playthroughs | Where-Object {$_.Lookup -eq $game} | Select-Object -First 1

    if($stat)
    {
        # Get the boss data
        $bossPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Bosses\$($stat.Lookup).json"
        $bosses = Get-Content $bossPath | ConvertFrom-Json
        $currentBoss = $bosses.Bosses | Where-Object {$_.Status -eq "Current"} | Select-Object -First 1
        # Count up some boss stats
        $bossCount = 0
        $bossDefeatedCount = 0
        $bossDeaths = 0
        foreach($boss in $bosses.Bosses)
        {
            $bossCount++;
            $bossDeaths += $boss.deaths
            if($boss.Status -eq "Defeated")
            {
                $bossDefeatedCount++;
            }
        }

        # Build an output string based on what data is available
        $output = "Game: $($stat.Game)"
        if($stat.Status)
        {
            $output += " | Status: $($stat.Status)"
        }
        if($stat.Playtime)
        {
            $output += " | Playtime: $($stat.Playtime)"
        }
        if($stat.Sessions)
        {
            $output += " | Sessions: $($stat.Sessions)"
        }
        if($stat.Deaths)
        {
            $output += " | Deaths: $($stat.Deaths)"
        }
        if($bossCount -gt 0)
        {
            $output += " | Bosses Fought: $($bossCount)"
            $output += " | Bosses Defeated: $($bossDefeatedCount)"
            $output += " | Boss Deaths: $($bossDeaths)"
        }
        if($currentBoss)
        {
            $output += " | Current Boss: $($currentBoss.Boss)"
            $output += " | Current Boss Deaths: $($currentBoss.Deaths)"
        }
        # if($stat.VOD)
        # {
        #     $output += " | VODs: $($stat.VOD)"
        # }

        Write-Host $output
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
