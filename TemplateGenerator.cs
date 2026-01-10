using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace SCPSLTemplateGenerator;

public class TemplateGenerator
{
    public async Task<GenerationResult> GeneratePluginAsync(PluginGenerationOptions options)
    {
        try
        {
            // Vérification de la variable d'environnement SL_REFERENCES
            var envCheckResult = await CheckAndSetupSlReferencesAsync();
            if (!envCheckResult.IsValid)
            {
                return GenerationResult.Failure(envCheckResult.ErrorMessage ?? "Unknown error");
            }

            // Validation des paramètres
            var validationResult = ValidateOptions(options);
            if (!validationResult.IsValid)
            {
                return GenerationResult.Failure(validationResult.ErrorMessage ?? "Validation failed");
            }

            // Préparation des variables
            var variables = PrepareVariables(options);
            
            // Création du répertoire de sortie
            var outputDir = Path.Combine(options.OutputDirectory!.FullName, SanitizeDirectoryName(options.Name));
            
            if (Directory.Exists(outputDir) && !options.Force)
            {
                return GenerationResult.Failure($"Directory '{outputDir}' already exists. Use --force to overwrite.");
            }

            Directory.CreateDirectory(outputDir);
            
            // Génération des fichiers
            await GenerateFilesAsync(variables, outputDir);

            return GenerationResult.Success(outputDir);
        }
        catch (Exception ex)
        {
            return GenerationResult.Failure($"Generation failed: {ex.Message}");
        }
    }

    private ValidationResult ValidateOptions(PluginGenerationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Name))
            return ValidationResult.Invalid("Plugin name cannot be empty");

        if (!IsValidCSharpIdentifier(options.Name))
            return ValidationResult.Invalid("Plugin name must be a valid C# identifier");

        if (string.IsNullOrWhiteSpace(options.Author))
            return ValidationResult.Invalid("Author cannot be empty");

        if (string.IsNullOrWhiteSpace(options.Description))
            return ValidationResult.Invalid("Description cannot be empty");

        if (!IsValidVersionString(options.Version))
            return ValidationResult.Invalid("Version must be in format '1, 0, 0' or similar");

        return ValidationResult.Valid();
    }

    private Dictionary<string, string> PrepareVariables(PluginGenerationOptions options)
    {
        var sanitizedName = SanitizeName(options.Name);
        var projectGuid = Guid.NewGuid().ToString().ToUpper();
        var dependencyReferences = GenerateDependencyReferences();
        
        return new Dictionary<string, string>
        {
            { "{{PluginName}}", sanitizedName },
            { "{{Namespace}}", sanitizedName },
            { "{{Author}}", options.Author },
            { "{{Description}}", options.Description },
            { "{{Version}}", options.Version },
            { "{{ProjectGuid}}", projectGuid },
            { "{{DependencyReferences}}", dependencyReferences }
        };
    }

    private string GenerateDependencyReferences()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
        var dependenciesDir = Path.Combine(assemblyDir, "dependencies");

        if (!Directory.Exists(dependenciesDir))
        {
            return "    <!-- No dependencies found -->";
        }

        var dllFiles = Directory.GetFiles(dependenciesDir, "*.dll", SearchOption.AllDirectories);
        
        if (dllFiles.Length == 0)
        {
            return "    <!-- No dependencies found -->";
        }

        var references = new StringBuilder();
        foreach (var dllPath in dllFiles)
        {
            var dllName = Path.GetFileNameWithoutExtension(dllPath);
            var fileName = Path.GetFileName(dllPath);
            
            references.AppendLine($"    <Reference Include=\"{dllName}\">");
            references.AppendLine($"      <HintPath>$(SL_REFERENCES)\\{fileName}</HintPath>");
            references.AppendLine("      <Private>false</Private>");
            references.Append("    </Reference>");
            
            if (dllPath != dllFiles.Last())
            {
                references.AppendLine();
            }
        }

        return references.ToString();
    }

    private async Task GenerateFilesAsync(Dictionary<string, string> variables, string outputDir)
    {
        var templatesPath = GetTemplatesPath();
        
        // Scanner tous les fichiers template automatiquement
        var allTemplates = GetAllTemplateFiles(templatesPath);
        
        foreach (var (templateRelativePath, outputRelativePath) in allTemplates)
        {
            var templatePath = Path.Combine(templatesPath, templateRelativePath);
            var outputPath = Path.Combine(outputDir, ReplaceVariables(outputRelativePath, variables));
            
            // Créer le répertoire parent si nécessaire
            var outputDirectory = Path.GetDirectoryName(outputPath)!;
            Directory.CreateDirectory(outputDirectory);
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }
            
            var templateContent = await File.ReadAllTextAsync(templatePath, Encoding.UTF8);
            var processedContent = ReplaceVariables(templateContent, variables);
            
            await File.WriteAllTextAsync(outputPath, processedContent, Encoding.UTF8);
            
            ConsoleHelper.WriteCreated($"[+] Created: {Path.GetRelativePath(outputDir, outputPath)}");
        }
    }

    private Task<ValidationResult> CheckAndSetupSlReferencesAsync()
    {
        var slReferences = Environment.GetEnvironmentVariable("SL_REFERENCES", EnvironmentVariableTarget.User);
        
        if (!string.IsNullOrEmpty(slReferences))
        {
            if (Directory.Exists(slReferences))
            {
                ConsoleHelper.WriteInfo($"[INFO] Using SL_REFERENCES: {slReferences}");
                return Task.FromResult(ValidationResult.Valid());
            }
            else
            {
                ConsoleHelper.WriteError($"[ERROR] SL_REFERENCES points to non-existent directory: {slReferences}");
                return Task.FromResult(ValidationResult.Invalid($"SL_REFERENCES directory does not exist: {slReferences}"));
            }
        }

        // La variable n'existe pas, proposer de la créer
        ConsoleHelper.WriteInfo("[INFO] SL_REFERENCES environment variable not found.");
        Console.WriteLine();
        Console.WriteLine("The SL_REFERENCES variable is required to store SCP:SL dependencies.");
        Console.Write("Would you like to create it now? (Y/n): ");
        
        var response = Console.ReadLine()?.Trim().ToLower();
        if (response != "" && response != "y" && response != "yes")
        {
            return Task.FromResult(ValidationResult.Invalid("SL_REFERENCES environment variable is required."));
        }

        // Demander le chemin
        Console.WriteLine();
        var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scpsl", "references");
        Console.WriteLine($"Enter the path for SL_REFERENCES (default: {defaultPath}):");
        Console.Write("Path: ");
        
        var userPath = Console.ReadLine()?.Trim();
        var targetPath = string.IsNullOrEmpty(userPath) ? defaultPath : userPath;

        // Créer le dossier s'il n'existe pas
        try
        {
            Directory.CreateDirectory(targetPath);
            
            // Copier les DLL du dossier dependencies de l'outil vers le nouveau dossier
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
            var dependenciesSourceDir = Path.Combine(assemblyDir, "dependencies");
            
            if (Directory.Exists(dependenciesSourceDir))
            {
                ConsoleHelper.WriteInfo("[INFO] Copying dependencies to SL_REFERENCES...");
                var files = Directory.GetFiles(dependenciesSourceDir, "*.dll", SearchOption.AllDirectories);
                
                foreach (var sourceFile in files)
                {
                    var fileName = Path.GetFileName(sourceFile);
                    var destFile = Path.Combine(targetPath, fileName);
                    File.Copy(sourceFile, destFile, overwrite: true);
                }
                
                ConsoleHelper.WriteSuccess($"[SUCCESS] Copied {files.Length} DLL(s) to {targetPath}");
            }

            // Définir la variable d'environnement
            Environment.SetEnvironmentVariable("SL_REFERENCES", targetPath, EnvironmentVariableTarget.User);
            ConsoleHelper.WriteSuccess($"[SUCCESS] SL_REFERENCES set to: {targetPath}");
            Console.WriteLine();
            ConsoleHelper.WriteInfo("[INFO] Please restart your terminal for the changes to take effect.");
            Console.WriteLine();
            
            return Task.FromResult(ValidationResult.Valid());
        }
        catch (Exception ex)
        {
            return Task.FromResult(ValidationResult.Invalid($"Failed to setup SL_REFERENCES: {ex.Message}"));
        }
    }

    private Dictionary<string, string> GetAllTemplateFiles(string templatesPath)
    {
        var templates = new Dictionary<string, string>();
        
        if (!Directory.Exists(templatesPath))
            return templates;
        
        // Scanner tous les fichiers dans le dossier templates
        var allFiles = Directory.GetFiles(templatesPath, "*.*", SearchOption.AllDirectories);
        
        foreach (var filePath in allFiles)
        {
            var relativePath = Path.GetRelativePath(templatesPath, filePath);
            var outputPath = relativePath;
            
            // Traiter les cas spéciaux
            if (relativePath == "Template.csproj.template")
            {
                outputPath = "{{PluginName}}.csproj";
            }
            else if (relativePath == "Template.sln.template")
            {
                outputPath = "{{PluginName}}.sln";
            }
            else if (relativePath.EndsWith(".cs.template"))
            {
                // Retirer le .template des fichiers .cs
                outputPath = relativePath.Replace(".cs.template", ".cs");
                
                // Renommer Command.cs en ExampleCommand.cs
                if (outputPath.EndsWith("Commands\\Command.cs") || outputPath.EndsWith("Commands/Command.cs"))
                {
                    outputPath = outputPath.Replace("Command.cs", "ExampleCommand.cs");
                }
            }
            // LICENSE et .gitignore restent tels quels
            
            templates[relativePath] = outputPath;
        }
        
        return templates;
    }

    private string GetTemplatesPath()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
        return Path.Combine(assemblyDir, "templates");
    }

    private string ReplaceVariables(string content, Dictionary<string, string> variables)
    {
        foreach (var (placeholder, value) in variables)
        {
            content = content.Replace(placeholder, value);
        }
        return content;
    }

    private bool IsValidCSharpIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Les identifiants C# doivent commencer par une lettre ou un underscore
        // et ne contenir que des lettres, chiffres et underscores
        return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }

    private bool IsValidVersionString(string version)
    {
        // Accepte des formats comme "1, 0, 0" ou "1,0,0" ou "1.0.0"
        return Regex.IsMatch(version, @"^\d+[,.\s]*\d+[,.\s]*\d+$");
    }

    private string SanitizeName(string name)
    {
        // Supprime les caractères non valides et capitalise
        var sanitized = Regex.Replace(name, @"[^a-zA-Z0-9_]", "");
        return string.IsNullOrEmpty(sanitized) ? "Plugin" : sanitized;
    }

    private string SanitizeDirectoryName(string name)
    {
        // Supprime les caractères non valides pour les noms de dossier
        var invalidChars = Path.GetInvalidFileNameChars();
        return invalidChars.Aggregate(name, (current, c) => current.Replace(c, '_'));
    }
}

public class PluginGenerationOptions
{
    public string Name { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public DirectoryInfo? OutputDirectory { get; init; }
    public bool Force { get; init; }
}

public class GenerationResult
{
    public bool IsSuccess { get; private set; }
    public string? OutputPath { get; private set; }
    public string? ErrorMessage { get; private set; }

    private GenerationResult(bool success, string? outputPath = null, string? errorMessage = null)
    {
        IsSuccess = success;
        OutputPath = outputPath;
        ErrorMessage = errorMessage;
    }

    public static GenerationResult Success(string outputPath) => new(true, outputPath);
    public static GenerationResult Failure(string errorMessage) => new(false, errorMessage: errorMessage);
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Valid() => new(true);
    public static ValidationResult Invalid(string errorMessage) => new(false, errorMessage);
}