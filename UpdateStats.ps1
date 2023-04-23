# Paths to the deaths text files
$currentDeathsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Current Game.txt"
$bossDeathsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Current Boss.txt"
$totalDeathsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Total.txt"
# Get the JSON data
$statsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Stats.json"
$stats = Get-Content $statsPath | ConvertFrom-Json
# Find our current game (only 1 of these should realistically be set)
$currentGame = $stats.Playthroughs | Where-Object {$_.Status -eq "Current"} | Select-Object -First 1

# Update the total deaths file
function Update-Total
{
    $deaths = 0;
    foreach($session in $stats.Playthroughs)
    {
        $deaths += $session.Deaths;
    }

    Set-Content $totalDeathsPath $deaths
}

if($args)
{
    if($args[0] -eq "death")
    {
        # Update the death count
        if($args[1] -eq "add")
        {
            # Always update the current game
            $currentGame.Deaths = "$([int]$currentGame.Deaths + 1)"
            Set-Content $currentDeathsPath $currentGame.Deaths

            if($args[2] -eq "boss")
            {
                # Optionally update the boss
                $currentBossDeaths = [int](Get-Content $bossDeathsPath)
                Set-Content $bossDeathsPath "$([int]$currentBossDeaths+1)"
            }
        }
        elseif($args[1] -eq "subtract")
        {
            # Always update the current game
            $currentGame.Deaths = "$([int]$currentGame.Deaths - 1)"
            Set-Content $currentDeathsPath $currentGame.Deaths

            if($args[2] -eq "boss")
            {   
                # Optionally update the boss
                $currentBossDeaths = [int](Get-Content $bossDeathsPath)
                Set-Content $bossDeathsPath "$([int]$currentBossDeaths-1)"
            }
        }

        # When we update the count we need to adjust the total
        Update-Total
    }
    elseif($args[0] -eq "setcurrent")
    {
        # Get the game we want to set as current
        $lookup = $args[1]
        # Find first instance of it by lookup (should only be 1 anyway)
        $newCurrentGame = $stats.Playthroughs | Where-Object {$_.Lookup -eq $lookup} | Select-Object -First 1
        if($currentGame)
        {
            # Set our current game to In-Progress
            $currentGame.Status = "In-Progress"
        }
        # Set our new game to Current
        $newCurrentGame.Status = "Current"
    }
    elseif($args[0] -eq "sessions")
    {
        # Update the session count
        $currentGame.Sessions = "$([int]$currentGame.Sessions+1)"
    }

    # Write any changes out to JSON
    $stats | ConvertTo-Json -depth 32| set-content $statsPath
}