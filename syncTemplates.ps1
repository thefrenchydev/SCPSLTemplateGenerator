# Script pour synchroniser automatiquement tous les fichiers du dossier src vers les templates
Write-Host "[INFO] Syncing src files to templates..." -ForegroundColor Cyan

$srcDir = "src"
$templateDir = "templates"

# Dossiers à exclure de la synchronisation
$excludeDirs = @('bin', 'obj', '.vs', '.git')

# Fonction pour remplacer les valeurs hardcodées par des variables
function Convert-ToTemplate {
    param(
        [string]$Content,
        [string]$FileName
    )
    
    # Remplacements généraux pour tous les fichiers
    $Content = $Content -replace 'namespace Template;', 'namespace {{Namespace}};'
    $Content = $Content -replace 'namespace Template\.', 'namespace {{Namespace}}.'
    $Content = $Content -replace '"Template"', '"{{PluginName}}"'
    $Content = $Content -replace '"TheFrenchyDev"', '"{{Author}}"'
    $Content = $Content -replace '"This is a template plugin"', '"{{Description}}"'
    $Content = $Content -replace 'new\(1, 1, 0\)', 'new({{Version}})'
    $Content = $Content -replace 'EventHandler\.GetEvents\("Template\.Events"\)', 'EventHandler.GetEvents("{{Namespace}}.Events")'
    $Content = $Content -replace '<RootNamespace>Template</RootNamespace>', '<RootNamespace>{{Namespace}}</RootNamespace>'
    $Content = $Content -replace '<AssemblyName>Template</AssemblyName>', '<AssemblyName>{{PluginName}}</AssemblyName>'
    
    # Pour les fichiers .csproj, remplacer les références par le placeholder
    if ($FileName -like "*.csproj") {
        $pattern = '(?s)  <!-- Références automatiques depuis SL_REFERENCES -->.*?  </ItemGroup>'
        $Content = $Content -replace $pattern, "  <!-- Références automatiques depuis SL_REFERENCES -->`r`n  <ItemGroup>`r`n{{DependencyReferences}}`r`n  </ItemGroup>"
    }
    
    # Pour les fichiers .sln, gérer les GUIDs et noms de projets
    if ($FileName -like "*.sln") {
        # Remplacer les chemins src\ par racine (doit être fait AVANT le remplacement de Template.csproj)
        $Content = $Content -replace 'src\\Template\.csproj', '{{PluginName}}.csproj'
        $Content = $Content -replace 'src/Template\.csproj', '{{PluginName}}.csproj'
        $Content = $Content -replace 'Template\.csproj', '{{PluginName}}.csproj'
        $Content = $Content -replace '"Template"', '"{{PluginName}}"'
        # Remplacer les GUIDs par le placeholder
        $Content = $Content -replace '\{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}\}', '{{ProjectGuid}}'
    }
    
    return $Content
}

# Nettoyer le dossier templates (sauf LICENSE s'il existe dans templates)
Write-Host "  [*] Cleaning templates directory..." -ForegroundColor Gray
if (Test-Path $templateDir) {
    Get-ChildItem -Path $templateDir -Recurse | Where-Object { 
        $_.Name -ne "LICENSE" 
    } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}

# Créer le dossier templates s'il n'existe pas
if (-not (Test-Path $templateDir)) {
    New-Item -Path $templateDir -ItemType Directory | Out-Null
}

# Compteur de fichiers synchronisés
$syncCount = 0

# Scanner récursivement tous les fichiers du src
Get-ChildItem -Path $srcDir -Recurse -File | ForEach-Object {
    $file = $_
    $fullSrcPath = (Resolve-Path $srcDir).Path
    $relativePath = $file.FullName.Substring($fullSrcPath.Length + 1)
    
    # Vérifier si le fichier est dans un dossier exclu
    $isExcluded = $false
    foreach ($excludeDir in $excludeDirs) {
        if ($relativePath -like "$excludeDir\*" -or $relativePath -like "*\$excludeDir\*") {
            $isExcluded = $true
            break
        }
    }
    
    if ($isExcluded) {
        return  # Continuer à la prochaine itération
    }
    
    # Définir le chemin de destination
    $destPath = Join-Path $templateDir $relativePath
    
    # Renommer Template.csproj en Template.csproj.template dans la destination
    if ($file.Name -eq "Template.csproj") {
        $destPath = $destPath -replace '\.csproj$', '.csproj.template'
    }
    # Renommer Template.sln en Template.sln.template
    elseif ($file.Name -eq "Template.sln") {
        $destPath = $destPath -replace '\.sln$', '.sln.template'
    }
    # Ajouter .template aux fichiers .cs
    elseif ($file.Extension -eq ".cs") {
        $destPath += ".template"
    }
    # Renommer .gitignore en gitignore.template
    elseif ($file.Name -eq ".gitignore") {
        $destPath = Join-Path (Split-Path $destPath -Parent) "gitignore.template"
    }
    # Garder LICENSE tel quel (pas de .template)
    elseif ($file.Name -eq "LICENSE") {
        # Pas de modification du nom
    }
    
    # Créer le dossier de destination s'il n'existe pas
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -Path $destDir -ItemType Directory -Force | Out-Null
    }
    
    # Lire le contenu du fichier
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    
    if ($null -ne $content) {
        # Appliquer les transformations
        $templateContent = Convert-ToTemplate -Content $content -FileName $file.Name
        
        # Écrire le fichier template
        Set-Content -Path $destPath -Value $templateContent -NoNewline
        
        Write-Host "  [+] Synced: $relativePath" -ForegroundColor Gray
        $syncCount++
    }
    else {
        # Pour les fichiers binaires ou sans contenu, copier directement
        Copy-Item -Path $file.FullName -Destination $destPath -Force
        Write-Host "  [+] Copied: $relativePath" -ForegroundColor Gray
        $syncCount++
    }
}

Write-Host "[SUCCESS] $syncCount file(s) synchronized successfully!" -ForegroundColor Green
