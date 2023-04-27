# Paths to the deaths text files
$currentDeathsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Current Game.txt"
$bossDeathsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Current Boss.txt"
$totalDeathsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Total.txt"
# Get the JSON data
$statsPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Stats.json"
$stats = Get-Content $statsPath | ConvertFrom-Json
# Find our current game (only 1 of these should realistically be set)
$currentGame = $stats.Playthroughs | Where-Object {$_.Status -eq "Current"} | Select-Object -First 1
if($currentGame)
{
    # Get the JSON data for our current boss
    $bossPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Bosses\$($currentGame.Lookup).json"
    $bosses = Get-Content $bossPath | ConvertFrom-Json
    $currentBoss = $bosses.Bosses | Where-Object {$_.Status -eq "Current"} | Select-Object -First 1
}
else 
{
    Write-Host "No Current Playthrough set"
}

# Update the total deaths file
function Update-Total
{
    $deaths = 0;
    foreach($session in $stats.Playthroughs)
    {
        $deaths += $session.Deaths;
    }

    Set-Content $totalDeathsPath $deaths
    Write-Host "Total Deaths set to $($deaths)"
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
            Write-Host "Current Game Deaths set to $($currentGame.Deaths)"

            if($args[2] -eq "boss")
            {
                if($currentBoss)
                {
                    # Optionally update the boss
                    $currentBoss.Deaths = "$([int]$currentBoss.Deaths + 1)"
                    Set-Content $bossDeathsPath "$([int]$currentBoss.Deaths)"
                    $bosses | ConvertTo-Json -depth 32| set-content $bossPath
                    Write-Host "Current Boss Deaths set to $([int]$currentBoss.Deaths)"
                }
                else 
                {
                    Write-Host "No Current Boss"
                }
            }
        }
        elseif($args[1] -eq "subtract")
        {
            # Always update the current game
            $currentGame.Deaths = "$([int]$currentGame.Deaths - 1)"
            Set-Content $currentDeathsPath $currentGame.Deaths
            Write-Host "Current Game Deaths set to $($currentGame.Deaths)"

            if($args[2] -eq "boss")
            {   
                if($currentBoss)
                {
                    # Optionally update the boss
                    $currentBoss.Deaths = "$([int]$currentBoss.Deaths - 1)"
                    Set-Content $bossDeathsPath "$([int]$currentBoss.Deaths)"
                    $bosses | ConvertTo-Json -depth 32| set-content $bossPath
                    Write-Host "Current Boss Deaths set to $([int]$currentBoss.Deaths)"
                }
                else 
                {
                    Write-Host "No Current Boss"
                }
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
            Write-Host "Set $($currentGame.Lookup) to In-Progress"
        }
        # Set our new game to Current
        $newCurrentGame.Status = "Current"
        Write-Host "Set $($newCurrentGame.Lookup) to Current"

    }
    elseif($args[0] -eq "setcomplete")
    {        
        if($currentGame)
        {
            $currentGame.Status = "Complete"
            Write-Host "Set $($currentGame.Lookup) to Complete"
        }
    }
    elseif($args[0] -eq "sessions")
    {
        # Update the session count
        $currentGame.Sessions = "$([int]$currentGame.Sessions+1)"
    }
    elseif($args[0] -eq "newgame")
    {
        $lookup = $args[1]
        $game = $args[2]

        # Add a new playthrough
        if($lookup -and $game)
        {
            # Check the playthrough doesn't already exist
            $gameExists = $stats.Playthroughs | Where-Object {$_.Lookup -eq $lookup}
            if($gameExists)
            {
                Write-Host "Playthrough $($lookup) already exists!"
            }
            else
            {
                # Add the game with default settings
                $stats.Playthroughs += [pscustomobject] @{Lookup= $lookup;Game=$game;Deaths=0;Sessions=0;Status="Scheduled"}
                Write-Host "Playthrough $($lookup) added to Stats.JSON with name '$($game)'"

                # Create boss data file
                $newBossPath = "$([environment]::getfolderpath("mydocuments"))\Streaming\Deathcount\Bosses\$($lookup).json"
                $newBosses = [pscustomobject] @{Bosses=@()}
                $newBosses | ConvertTo-Json -depth 32| set-content $newBossPath
                Write-Host "$($newBossPath) created"
            }
        }
        else
        {
            Write-Host "No Lookup or Boss name specified"
        }
    }
    elseif($args[0] -eq "boss")
    {
        # Action related to the bosses file
        if($bosses)
        {
            if($args[1] -eq "new")
            {
                # Add a new boss to the playthrough
                $boss = $args[2]
                # Lookup can be generated from the boss name
                $lookup = $boss.replace('\W', '')

                # Allow an override
                if($args[3])
                {
                    $lookup = $args[3]
                }

                if($boss)
                {
                    # Check the boss isn't already added
                    $bossexists = $bosses.Bosses | Where-Object {$_.Lookup -eq $lookup}
                    if($bossexists)
                    {
                        Write-Host "Boss $($lookup) already exists!"
                    }
                    else
                    {
                        # Add the entry setting it to current
                        $bosses.Bosses += [pscustomobject] @{Lookup=$lookup;Boss=$boss;Status= "Current";Deaths=0}
                        # Update the boss JSON
                        $bosses | ConvertTo-Json -depth 32| set-content $bossPath
                        Write-Host "Added $($lookup) to $($bossPath) with name $($boss)"
                    }
                }
            }
            elseif($args[1] -eq "list")
            {
                # Get a list of all bosses
                $output = "";
                foreach($boss in $bosses.Bosses)
                {
                    $output += $boss.Lookup + ", " 
                }        
                Write-Host "Bosses: $($output)"
            }
            elseif($args[1] -eq "setcurrent")
            {
                # Get the game we want to set as current
                $lookup = $args[2]
                # Find first instance of it by lookup (should only be 1 anyway)
                $newcurrentBoss = $bosses.Bosses | Where-Object {$_.Lookup -eq $lookup} | Select-Object -First 1
                if($currentBoss)
                {
                    # Set our current boss to Undefeated
                    $currentBoss.Status = "Undefeated"
                    Write-Host "Set $($currentBoss.Lookup) to Undefeateds"
                }
                # Set our new game to Current
                $newcurrentBoss.Status = "Current"
                $bosses | ConvertTo-Json -depth 32| set-content $bossPath
                Write-Host "Set $($newcurrentBoss.Lookup) to Current"

                # Update the current deaths text file
                Set-Content $bossDeathsPath "$([int]$newcurrentBoss.Deaths)"
                Write-Host "Set $($bossDeathsPath) to $([int]$newcurrentBoss.Deaths)"
            }
            elseif($args[1] -eq "defeat")
            {
                if($currentBoss)
                {
                    # Set our current boss to Undefeated
                    $currentBoss.Status = "Defeated"
                    $bosses | ConvertTo-Json -depth 32| set-content $bossPath
                    Write-Host "Set $($currentBoss.Lookup) to Defeated"
                    
                    # Update the current deaths text file
                    Set-Content $bossDeathsPath "0"
                    Write-Host "Set $($bossDeathsPath) to 0"
                }
                else 
                {
                    Write-Host "No Current Boss"
                }
            }
        }
        else 
        {
            Write-Host "No Bosses file found"
        }
    }
    # Write any changes out to JSON
    $stats | ConvertTo-Json -depth 32| set-content $statsPath
}