# Script pour mettre à jour automatiquement les références DLL dans Template.csproj
param(
    [string]$DependenciesDir = "dependencies",
    [string]$TemplateProjectPath = "src\Template.csproj"
)

Write-Host "[INFO] Updating dependencies in Template.csproj..." -ForegroundColor Cyan

# Vérifier que le dossier dependencies existe
if (-not (Test-Path $DependenciesDir)) {
    Write-Host "[ERROR] Dependencies folder not found: $DependenciesDir" -ForegroundColor Red
    exit 1
}

# Vérifier que le fichier Template.csproj existe
if (-not (Test-Path $TemplateProjectPath)) {
    Write-Host "[ERROR] Template.csproj not found: $TemplateProjectPath" -ForegroundColor Red
    exit 1
}

# Scanner tous les fichiers DLL dans le dossier dependencies
$dllFiles = Get-ChildItem -Path $DependenciesDir -Filter "*.dll" | Sort-Object Name

if ($dllFiles.Count -eq 0) {
    Write-Host "[WARNING] No DLL files found in $DependenciesDir" -ForegroundColor Yellow
    exit 0
}

Write-Host "[INFO] Found $($dllFiles.Count) DLL file(s)" -ForegroundColor Cyan

# Générer le XML pour les références
$referencesXml = @()
$referencesXml += "  <!-- Références automatiques depuis LABAPI_REFERENCES -->"
$referencesXml += "  <ItemGroup>"

foreach ($dll in $dllFiles) {
    $name = [System.IO.Path]::GetFileNameWithoutExtension($dll.Name)
    $referencesXml += "    <Reference Include=`"$name`">"
    $referencesXml += "      <HintPath>`$(LABAPI_REFERENCES)\$($dll.Name)</HintPath>"
    $referencesXml += "      <Private>false</Private>"
    $referencesXml += "    </Reference>"
}

$referencesXml += "  </ItemGroup>"

# Lire le contenu actuel du fichier
$content = Get-Content $TemplateProjectPath -Raw

# Trouver et remplacer la section des références
$pattern = '(?s)<!-- Références automatiques depuis LABAPI_REFERENCES -->.*?</ItemGroup>'
$newReferences = ($referencesXml -join "`r`n")

if ($content -match $pattern) {
    $content = $content -replace $pattern, $newReferences
    Write-Host "[INFO] Updated existing references section" -ForegroundColor Green
} else {
    # Si la section n'existe pas, l'ajouter après le PackageReference
    $pattern = '(?s)(</ItemGroup>)(\s*)(<!--|\s*<PropertyGroup)'
    if ($content -match $pattern) {
        $content = $content -replace $pattern, "`$1`r`n`r`n$newReferences`r`n`r`n`$2`$3"
        Write-Host "[INFO] Added new references section" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Could not find appropriate location to insert references" -ForegroundColor Red
        exit 1
    }
}

# Écrire le contenu mis à jour
Set-Content -Path $TemplateProjectPath -Value $content -NoNewline

Write-Host "[SUCCESS] Updated $($dllFiles.Count) reference(s) in Template.csproj" -ForegroundColor Green
