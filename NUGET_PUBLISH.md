# Guide de publication sur NuGet.org

## Étape 1 : Créer un compte NuGet.org

1. Va sur [nuget.org](https://www.nuget.org/)
2. Clique sur "Sign in" (en haut à droite)
3. Connecte-toi avec un compte Microsoft, GitHub ou Google

## Étape 2 : Générer une clé API

1. Une fois connecté, clique sur ton nom d'utilisateur → **API Keys**
2. Clique sur **Create** ou **+ Create**
3. Configure la clé :
   - **Key Name** : `SCPSLTemplateGenerator` (ou ce que tu veux)
   - **Package Owner** : Sélectionne ton compte
   - **Scopes** : `Push new packages and package versions`
   - **Glob Pattern** : `*` (ou `SCPSLTemplateGenerator` pour être spécifique)
   - **Expiration** : 365 jours (ou plus selon ton besoin)
4. Clique sur **Create**
5. **IMPORTANT** : Copie la clé générée et sauvegarde-la en sécurité (elle ne sera affichée qu'une fois !)

## Étape 3 : Mettre à jour les URLs GitHub

Avant de publier, tu dois mettre à jour les URLs dans `SCPSLTemplateGenerator.csproj` :

```xml
<PackageProjectUrl>https://github.com/TON-USERNAME/scpsl-template-generator</PackageProjectUrl>
<RepositoryUrl>https://github.com/TON-USERNAME/scpsl-template-generator</RepositoryUrl>
```

Remplace `TON-USERNAME` par ton vrai nom d'utilisateur GitHub (ou supprime ces lignes si tu n'as pas de repo).

## Étape 4 : Publier sur NuGet.org

Dans PowerShell, exécute :

```powershell
# Rebuild pour s'assurer que tout est à jour
dotnet clean
dotnet build -c Release
dotnet pack -c Release

# Publier (remplace <TA-CLE-API> par ta vraie clé)
dotnet nuget push .\nupkg\SCPSLTemplateGenerator.1.0.0.nupkg `
    --api-key <TA-CLE-API> `
    --source https://api.nuget.org/v3/index.json
```

## Étape 5 : Vérification

1. La publication prend quelques minutes pour être indexée
2. Va sur https://www.nuget.org/packages/SCPSLTemplateGenerator
3. Tu verras ton package avec le README, les stats de téléchargement, etc.

## Installation par les utilisateurs

Une fois publié, n'importe qui peut l'installer avec :

```bash
dotnet tool install --global SCPSLTemplateGenerator
```

## Publier une nouvelle version

Pour publier une mise à jour :

1. Modifie la version dans `SCPSLTemplateGenerator.csproj` :
   ```xml
   <Version>1.0.1</Version>
   ```

2. Met à jour `<PackageReleaseNotes>` avec les changements

3. Rebuild et push :
   ```powershell
   dotnet pack -c Release
   dotnet nuget push .\nupkg\SCPSLTemplateGenerator.1.0.1.nupkg --api-key <TA-CLE> --source https://api.nuget.org/v3/index.json
   ```

## Conseils

- ✅ Teste toujours localement avant de publier
- ✅ Utilise [Semantic Versioning](https://semver.org/) : `MAJOR.MINOR.PATCH`
- ✅ N'oublie pas de mettre à jour `PackageReleaseNotes`
- ⚠️ **Une fois publié, tu ne peux PAS supprimer ou modifier une version** (seulement la "unlister")
- 🔒 Garde ta clé API secrète ! Ne la commite jamais sur GitHub

## Script automatique (optionnel)

Crée `publish.bat` :

```bat
@echo off
set /p VERSION="Enter version (e.g., 1.0.1): "
set /p API_KEY="Enter NuGet API Key: "

dotnet pack -c Release /p:Version=%VERSION%
dotnet nuget push .\nupkg\SCPSLTemplateGenerator.%VERSION%.nupkg --api-key %API_KEY% --source https://api.nuget.org/v3/index.json

echo Published version %VERSION%!
pause
```
