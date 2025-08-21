[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Configuration
$organizationName = "krosoft-dev"
$projectName = "Krosoft.Extensions.Cqrs"
$commitMessage = "Extraction du projet $projectName depuis Krosoft.Extensions"
$targetBranch = "main"  
$SourceBranch = "main"
$sourceRepoUrl = "https://github.com/krosoft-dev/Krosoft.Extensions.git"
$sourceRepoGit = "git@krosoft-dev:krosoft-dev/Krosoft.Extensions.git"
$targetRepoUrl = "https://github.com/krosoft-dev/$projectName.git"
$currentGitUser = git config user.name
$currentGitEmail = git config user.email

$targetDir = "C:\Dev\Krosoft.Extensions\$projectName"
$pathsToKeep = @( 
    # Fichiers communs    
    "src\Directory.Build.props",
    "tests\Directory.Build.props",   
    ".gitattributes", 
    ".gitignore", 
    "CODEOWNERS", 
    "Directory.Build.props", 
    "Krosoft.Extensions.sln" , 
    "LICENSE", 
    "NuGet.Config", 
    "README.md", 
    "renovate.json", 

    # Fichiers scripts
    "tools\scripts\dotnet_test.ps1",
    "tools\scripts\dotnet_build.ps1",
    "tools\scripts\git_clean.ps1",
    "tools\scripts\git_pull.ps1",

    # Fichiers sources   
    "src\Krosoft.Extensions.Cqrs", 
    "src\Krosoft.Extensions.Cqrs.Behaviors", 
    "src\Krosoft.Extensions.Cqrs.Behaviors.Identity", 
    "src\Krosoft.Extensions.Cqrs.Behaviors.Validations",  
       
    # Fichiers tests        
    # "tests\Krosoft.Extensions.Cqrs.Tests", 

    # Fichiers devops
    "tools\devops\vars\vars.yml",
    "tools\devops\build-pipeline.yml",        
    "tools\devops\nuget-Krosoft.Extensions.Cqrs-pipeline.yml",
    "tools\devops\nuget-Krosoft.Extensions.Cqrs.Behaviors-pipeline.yml", 
    "tools\devops\nuget-Krosoft.Extensions.Cqrs.Behaviors.Identity-pipeline.yml", 
    "tools\devops\nuget-Krosoft.Extensions.Cqrs.Behaviors.Validations-pipeline.yml"
    
) 


function Update-GitRepository {
    param (
        [Parameter(Mandatory = $true)][string]$OrganizationName,
        [Parameter(Mandatory = $true)][string]$ProjectName,
        [Parameter(Mandatory = $true)][string]$BranchName,
        [Parameter(Mandatory = $true)][string]$CommitMessage,
        [Parameter(Mandatory = $true)][string]$UserName,
        [Parameter(Mandatory = $true)][string]$UserEmail
    )
    
    try {
        # Supprime l'ancien remote
        git remote remove origin
        
        # Ajoute le nouveau remote 
        git remote add origin "git@${OrganizationName}:${OrganizationName}/${ProjectName}.git"
        # Crée une nouvelle branche et bascule dessus
        git checkout -b $BranchName
        
        # Ajoute tous les fichiers modifiés
        git add .

        git config user.name $UserName
        git config user.email $UserEmail       
        
        # Crée un commit
        git commit -m $CommitMessage

        Write-Log "Git config user.name : $(git config user.name)" -Color Green
        Write-Log "Git config user.email : $(git config user.email)" -Color Green 
        Write-Log "Git commit : " -Color Green 
        git log -1 --pretty=oneline

    }
    catch {
        Write-Log "Erreur lors de la configuration git : $_" -Color Red
    }
}

function Get-PathParts {
    param ( [string]$path )
    
    $parts = @()
    $currentPath = ""
    
    $path.Split('\') | ForEach-Object {
        if ($currentPath -eq "") {
            $currentPath = $_
        }
        else {
            $currentPath = "$currentPath\$_"
        }
        $parts += $currentPath
    }
    
    return $parts
}

function Write-Log {
    param(
        [Parameter(Mandatory = $true)][string]$Message,
        [Parameter(Mandatory = $false)][ConsoleColor]$Color = 'White'
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $Message" -ForegroundColor $Color
}

function Rename-SolutionFile {
    param (
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewName
    )

    $oldSlnPath = Join-Path $targetDir "Krosoft.Extensions.sln"
    $newSlnPath = Join-Path $targetDir "$NewName.sln"

    if (Test-Path $oldSlnPath) {
        Rename-Item -Path $oldSlnPath -NewName $newSlnPath
        Write-Log "Solution renommée : $projectName.sln" -Color Green
    }
    else {
        Write-Log "Fichier solution non trouvé : $oldSlnPath" -Color Red
    }

}

function Update-SonarConfiguration {
    param (
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewName
    )
    
    try {
        $varsPath = Join-Path $Path "tools\devops\vars\vars.yml"
        
        if (Test-Path $varsPath) {
            # Lire le contenu du fichier
            $content = Get-Content $varsPath -Raw
            
            # Remplacer les valeurs Sonar
            $content = $content -replace 'sonarCloudProjectKey: "krosoft-dev_Krosoft.Extensions"', "sonarCloudProjectKey: `"krosoft-dev_$NewName`""
            $content = $content -replace 'sonarCloudProjectName: "Krosoft.Extensions"', "sonarCloudProjectName: `"$NewName`""
            
            # Sauvegarder les modifications
            Set-Content -Path $varsPath -Value $content
            Write-Log "Configuration Sonar mise a jour avec : $NewName" -Color Green
        }
        else {
            Write-Log "Fichier vars.yml non trouve" -Color Red
        }
    }
    catch {
        Write-Log "Erreur lors de la mise a jour de la configuration Sonar : $_" -Color Red
    }
}


function Update-BuildPipelineName {
    param (
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewName
    )
    
    try {
        $pipelinePath = Join-Path $Path "tools\devops\build-pipeline.yml"
        
        if (Test-Path $pipelinePath) {
            # Lire le contenu du fichier
            $content = Get-Content $pipelinePath -Raw
            
            # Remplacer le nom du pipeline
            $oldName = "name: Build 'Krosoft.Extensions'"
            $newName = "name: Build '$NewName'"
            
            $content = $content -replace $oldName, $newName
            
            # Sauvegarder les modifications
            Set-Content -Path $pipelinePath -Value $content
            Write-Log "Pipeline name mis a jour avec : $NewName" -Color Green
        }
        else {
            Write-Log "Fichier build-pipeline.yml non trouve" -Color Red
        }
    }
    catch {
        Write-Log "Erreur lors de la mise a jour du pipeline name : $_" -Color Red
    }
}

function Update-BuildPropsRepository {
    param (
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$OldRepositoryUrl,
        [Parameter(Mandatory = $true)][string]$NewRepositoryUrl
    )
    
    try {
        $buildPropsPath = Join-Path $Path "Directory.Build.props"
        
        if (Test-Path $buildPropsPath) {
            # Lire le contenu du fichier
            $content = Get-Content $buildPropsPath -Raw
            
            # Remplacer l'URL du repository
 
            $content = $content -replace $oldRepositoryUrl, $NewRepositoryUrl
            
            # Sauvegarder les modifications
            Set-Content -Path $buildPropsPath -Value $content
            Write-Log "Directory.Build.props mis a jour avec : $NewRepositoryUrl" -Color Green
        }
        else {
            Write-Log "Fichier Directory.Build.props non trouve" -Color Red
        }
    }
    catch {
        Write-Log "Erreur lors de la mise a jour de Directory.Build.props : $_" -Color Red
    }
}

function Update-ReadmeFile {
    param (
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$NewName
    )
    
    $readmePath = Join-Path $Path "README.md"
    if (Test-Path $readmePath) {
        $content = Get-Content $readmePath -Raw
        $content = $content -replace "# Krosoft.Extensions", "# $NewName"
        Set-Content -Path $readmePath -Value $content
        Write-Log "README.md mis a jour avec : $NewName" -Color Green
    }
    else {
        Write-Log "Fichier README.md non trouve" -Color Red
    }
}

function Copy-FilteredRepository {
    param (
        [Parameter(Mandatory = $true)][string]$SourceUrl,
        [Parameter(Mandatory = $true)][string]$TargetPath,
        [Parameter(Mandatory = $true)][string[]]$PathsToKeep,
        [Parameter(Mandatory = $false)][string]$SourceBranch
    )
    
    try {
        # Préparer les dossiers à conserver
        $folderToKeep = @()
        foreach ($path in $PathsToKeep) {
            $parts = Get-PathParts $path
            $folderToKeep += $parts
        }
        $folderToKeep = $folderToKeep | Select-Object -Unique

        # Cloner le dépôt original
        git clone -b $SourceBranch $SourceUrl $TargetPath
        Set-Location $TargetPath
        Write-Log "Repository clone dans : $TargetPath" -Color Green

        # Récupérer tous les dossiers et fichiers
        $allItems = Get-ChildItem -Path $TargetPath -Recurse

        # Parcourir chaque élément en commençant par les plus profonds
        $allItems | Sort-Object -Property FullName -Descending | ForEach-Object {
            # Ignorer le dossier .git
            if ($_.FullName -like "*.git*") {
                return
            }     

            $relativePath = $_.FullName.Substring($TargetPath.Length + 1)
     
            $keepItem = $false
            foreach ($pathToKeep in $PathsToKeep) {       
                # Vérifie si c'est exactement le dossier qu'on veut garder ou un de ses sous-éléments
                if ($relativePath -eq $pathToKeep -or 
                    $relativePath.StartsWith("$pathToKeep\") -or 
                    $folderToKeep -contains $relativePath) {
                    $keepItem = $true
                    Write-Log "Conservation : $relativePath" -Color Yellow
                    break
                }
            }

            # Supprimer si le chemin n'est pas dans la liste à conserver
            if (-not $keepItem) {
                if (Test-Path $_.FullName) {
                    Remove-Item -Path $_.FullName -Force -Recurse -ErrorAction SilentlyContinue
                }
            }
        }
        
        Write-Log "Filtrage des fichiers termine avec succes" -Color Green
    }
    catch {
        Write-Log "Erreur lors du clonage/filtrage : $_" -Color Red
        throw
    }
}


# Nettoyage des anciens répertoires si présents
if (Test-Path $targetDir) {
    Remove-Item -Recurse -Force $targetDir
    Write-Log "Suppression du répertoire de sortie existant..." -Color Yellow
}


Copy-FilteredRepository -SourceUrl $sourceRepoGit -TargetPath $targetDir -PathsToKeep $pathsToKeep -SourceBranch $SourceBranch

#Rename sln file with project name
Rename-SolutionFile -Path $targetDir -NewName $projectName

# Update README.md
Update-ReadmeFile -Path $targetDir -NewName $projectName

#Update Directory.Build.props
Update-BuildPropsRepository -Path $targetDir -OldRepositoryUrl $sourceRepoUrl -NewRepositoryUrl $targetRepoUrl  

Update-BuildPipelineName -Path $targetDir -NewName $projectName
Update-SonarConfiguration -Path $targetDir -NewName $projectName
 
# git config
Update-GitRepository -OrganizationName $organizationName -ProjectName $projectName -BranchName $targetBranch -CommitMessage $commitMessage -UserName $currentGitUser -UserEmail $currentGitEmail
  
Write-Log "Projet extrait : '$targetDir' sur la branche '$targetBranch' de $targetRepoUrl" -Color Green
Write-Host 
Write-Host "git push -u origin $targetBranch" -ForegroundColor Cyan