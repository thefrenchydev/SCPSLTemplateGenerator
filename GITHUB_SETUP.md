# Guide GitHub pour SCPSLTemplateGenerator

## Créer le repository GitHub

### 1. Créer le repo sur GitHub.com

1. Va sur [github.com/new](https://github.com/new)
2. Configure :
   - **Repository name** : `SCPSLTemplateGenerator`
   - **Description** : `A .NET CLI tool for generating SCP:SL LabAPI plugin templates`
   - **Visibility** : Public
   - ⚠️ **NE COCHE PAS** "Add a README file" (tu en as déjà un)
   - ⚠️ **NE COCHE PAS** "Add .gitignore" (tu en as déjà un)
   - **License** : MIT
3. Clique sur **Create repository**

### 2. Initialiser Git localement

Dans PowerShell (dans le dossier SCPSLTemplateGenerator) :

```powershell
# Initialiser Git
git init

# Ajouter tous les fichiers
git add .

# Premier commit
git commit -m "Initial commit: SCPSL Template Generator v1.0.0"

# Ajouter le remote
git remote add origin https://github.com/thefrenchydev/SCPSLTemplateGenerator.git

# Définir la branche principale
git branch -M main

# Push vers GitHub
git push -u origin main
```

### 3. Créer une Release avec le .nupkg

#### Option A : Via l'interface GitHub

1. Va sur ton repo → **Releases** → **Draft a new release**
2. Configure :
   - **Choose a tag** : `v1.0.0` (créer le tag)
   - **Release title** : `v1.0.0 - Initial Release`
   - **Description** :
     ```markdown
     ## 🎉 Initial Release
     
     A .NET CLI tool for quickly generating SCP:SL LabAPI plugin templates.
     
     ### Features
     - ✨ Quick plugin generation with one command
     - 🔧 Automatic dependency management via LABAPI_REFERENCES
     - 📦 Pre-configured with LabAPI and FasterAPI
     - 📁 Complete project structure with examples
     
     ### Installation
     ```bash
     dotnet tool install --global --add-source https://github.com/thefrenchydev/SCPSLTemplateGenerator/releases/download/v1.0.0 SCPSLTemplateGenerator
     ```
     
     Or download the .nupkg file below and install locally:
     ```bash
     dotnet tool install --global --add-source ./path/to/nupkg SCPSLTemplateGenerator
     ```
     
     ### Usage
     ```bash
     scpsl-template new MyPlugin --author "YourName" --description "My awesome plugin"
     ```
     ```
3. **Attach files** : Glisse-dépose `nupkg\SCPSLTemplateGenerator.1.0.0.nupkg`
4. Clique sur **Publish release**

#### Option B : Via GitHub CLI (gh)

```powershell
# Installer GitHub CLI si nécessaire : winget install GitHub.cli

# Login
gh auth login

# Créer la release avec le .nupkg
gh release create v1.0.0 `
    .\nupkg\SCPSLTemplateGenerator.1.0.0.nupkg `
    --title "v1.0.0 - Initial Release" `
    --notes "Initial release with automatic dependency management and FasterAPI integration."
```

### 4. Installation par les utilisateurs

Une fois la release publiée, les utilisateurs peuvent :

**Option 1 : Télécharger et installer localement**
```bash
# Télécharger le .nupkg depuis https://github.com/thefrenchydev/SCPSLTemplateGenerator/releases
dotnet tool install --global --add-source ./chemin/vers/nupkg SCPSLTemplateGenerator
```

**Option 2 : Installer directement depuis GitHub (avec URL complète)**
```bash
dotnet tool install --global --add-source https://github.com/thefrenchydev/SCPSLTemplateGenerator/releases/download/v1.0.0 SCPSLTemplateGenerator
```

## Mettre à jour le .csproj avec la vraie URL

Une fois le repo créé, mets à jour [SCPSLTemplateGenerator.csproj](SCPSLTemplateGenerator.csproj) :

```xml
<PackageProjectUrl>https://github.com/thefrenchydev/SCPSLTemplateGenerator</PackageProjectUrl>
<RepositoryUrl>https://github.com/thefrenchydev/SCPSLTemplateGenerator</RepositoryUrl>
```

Puis rebuild et republish :

```powershell
git add .
git commit -m "Update GitHub URLs"
git push
```

## Publier une nouvelle version

1. Met à jour `<Version>` dans le .csproj
2. Commit et push les changements
3. Rebuild : `dotnet pack -c Release`
4. Crée une nouvelle release sur GitHub avec le nouveau .nupkg

## Alternative : GitHub Packages (NuGet feed privé)

Si tu veux un vrai feed NuGet hébergé sur GitHub :

```powershell
# Générer un Personal Access Token sur github.com (Settings → Developer settings → PAT)
# Avec scope 'write:packages'

dotnet nuget add source https://nuget.pkg.github.com/thefrenchydev/index.json `
    --name github `
    --username thefrenchydev `
    --password TON-GITHUB-PAT `
    --store-password-in-clear-text

dotnet nuget push .\nupkg\SCPSLTemplateGenerator.1.0.0.nupkg `
    --source github `
    --api-key TON-GITHUB-PAT
```

Les utilisateurs devront alors configurer le feed :
```bash
dotnet nuget add source https://nuget.pkg.github.com/thefrenchydev/index.json --name github-thefrenchydev
dotnet tool install --global SCPSLTemplateGenerator --add-source github-thefrenchydev
```

## Recommandation finale

**Pour la simplicité** : Utilise GitHub Releases (Option A ci-dessus)  
**Pour la distribution** : Retente NuGet.org après avoir fixé l'erreur (probablement besoin de valider l'email ou accepter les terms)

Tu peux faire les DEUX : publier sur GitHub Releases ET sur NuGet.org !
