using Newtonsoft.Json;

Console.WriteLine("Enter the full path to the directory where the repository was cloned to.");
var cloneDir = Console.ReadLine()?.Trim() ?? "";

Console.WriteLine("Enter the new name for the app. Keep in mind that, with spaces removed, this value will also be used as the new root namespace. So it must become a valid identifier when the spaces are removed.");
var newName = Console.ReadLine()?.Trim() ?? "";
var newRootNamespace = newName.Replace(" ", string.Empty);
var newAppId = newName.Replace(" ", "-").ToLower();

if (string.IsNullOrWhiteSpace(cloneDir))
{
    Console.WriteLine("Clone directory was not specified.");
    return;
}

if (!Directory.Exists(cloneDir))
{
    Console.WriteLine("The specified directory does not exist.");
    return;
}

Console.WriteLine("Checking clone directory...");

var apiPath = Path.Combine(cloneDir, "app-api");
var uiPath = Path.Combine(cloneDir, "app-ui");
var workflowFilename = Path.Combine(cloneDir, ".github", "workflows", "dotnet.yml");
var apiSolutionFilename = Path.Combine(apiPath, "ElectronAppApi.sln");
var libProjectFilename = Path.Combine(apiPath, "LibElectronAppApi", "LibElectronAppApi.csproj");
var testHarnessProjectFilename = Path.Combine(apiPath, "ElectronAppApiTestHarness", "ElectronAppApiTestHarness.csproj");
var hostProjectFilename = Path.Combine(apiPath, "ElectronAppApiHost", "ElectronAppApiHost.csproj");


var packageJsonFilename = Path.Combine(uiPath, "package.json");
var electronConfigFilename = Path.Combine(uiPath, "builder-config.json");
var electronConfigArm64Filename = Path.Combine(uiPath, "builder-config-arm64.json");
var indexHtmlFilename = Path.Combine(uiPath, "index.html");
var appJsFilename = Path.Combine(uiPath, "app.js");
var appInitTsFilename = Path.Combine(uiPath, "src", "AppInit.ts");

string[] strReplFiles =
[
    workflowFilename, packageJsonFilename, electronConfigFilename, electronConfigArm64Filename, indexHtmlFilename,
    appJsFilename, appInitTsFilename
];

string[] keyFiles =
[
    apiSolutionFilename, libProjectFilename, testHarnessProjectFilename, hostProjectFilename, packageJsonFilename,
    electronConfigFilename, electronConfigArm64Filename, indexHtmlFilename, appJsFilename, appInitTsFilename,
    workflowFilename
];

if (!keyFiles.All(File.Exists))
{
    Console.WriteLine("The specified directory is not a clone of the electron-react-dotnet-starter repository, or it has been modified.");
    return;
}

if (string.IsNullOrWhiteSpace(newName))
{
    Console.WriteLine("The app name is required.");
    return;
}

if (string.Equals(newName, "Electron App", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(newName, "ElectronApp", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(newName, "Electron Starter App", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(newName, "ElectronStarterApp", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("The app name must be different than what is in the repository.");
    return;
}

Console.WriteLine($"About to rebrand the code under '{cloneDir}' as '{newName}' with '{newRootNamespace}' as the root namespace, and '{newAppId}' as the appId. Type Y to proceed.");

if (Console.ReadKey().KeyChar.ToString().ToUpper() != "Y")
{
    Console.WriteLine();
    Console.WriteLine("Aborted.");
    return;
}

if (!ProcessApi()) return;
if (!ProcessUi()) return;

Console.WriteLine("Rebranding completed.");

bool ProcessUi()
{
    Console.WriteLine("Processing Workflow and UI string replacements...");
    foreach (var file in strReplFiles)
    {
        Console.WriteLine($"\tUpdate {file}");
        var fileText = File.ReadAllText(file);
        fileText = fileText.Replace("ElectronApp", newRootNamespace).Replace("Electron App", newName)
            .Replace("ElectronStarterApp", newRootNamespace).Replace("Electron Starter App", newName);
        File.WriteAllText(file, fileText);        
    }
    
    Console.WriteLine("Processing package.json and Electron configs...");
    
    Console.WriteLine($"\tUpdate {packageJsonFilename}");
    dynamic packageJson = JsonConvert.DeserializeObject(File.ReadAllText(packageJsonFilename));
    if (packageJson is not null)
    {
        packageJson.name = $"{newAppId}-ui";
        packageJson.version = "0.0.1";
        packageJson.author = string.Empty;
        packageJson.repository = string.Empty;
        packageJson.@private = true;
        File.WriteAllText(packageJsonFilename, JsonConvert.SerializeObject(packageJson, Formatting.Indented));
    }
    
    Console.WriteLine($"\tUpdate {electronConfigFilename}");
    dynamic builderConfig = JsonConvert.DeserializeObject(File.ReadAllText(electronConfigFilename));
    if (builderConfig is not null)
    {
        builderConfig.appId = newAppId;
        builderConfig.productName = newRootNamespace;
        builderConfig.copyright = $"Copyright© {DateTime.Now.Year}";
        builderConfig.buildVersion = "0.0.1";
        File.WriteAllText(electronConfigFilename, JsonConvert.SerializeObject(builderConfig, Formatting.Indented));
    }
    
    Console.WriteLine($"\tUpdate {electronConfigArm64Filename}");
    dynamic builderConfigArm64 = JsonConvert.DeserializeObject(File.ReadAllText(electronConfigArm64Filename));
    if (builderConfigArm64 is not null)
    {
        builderConfigArm64.appId = newAppId;
        builderConfigArm64.productName = newRootNamespace;
        builderConfigArm64.copyright = $"Copyright© {DateTime.Now.Year}";
        builderConfigArm64.buildVersion = "0.0.1";
        File.WriteAllText(electronConfigArm64Filename, JsonConvert.SerializeObject(builderConfigArm64, Formatting.Indented));
    }    

    return true;
}

bool ProcessApi()
{
    if (ProcessSolutionFile())
    {
        ProcessProject(libProjectFilename);
        ProcessProject(hostProjectFilename);
        ProcessProject(testHarnessProjectFilename);
        return true;
    }

    return false;
}

bool ProcessSolutionFile()
{
    if (File.Exists(apiSolutionFilename))
    {
        Console.WriteLine("Processing API solution file...");

        var solutionText = File.ReadAllText(apiSolutionFilename);
        if (!solutionText.TrimStart().StartsWith("Microsoft Visual Studio Solution File, Format Version 12.00"))
        {
            Console.WriteLine("Invalid solution file.");
            return false;
        }

        Console.WriteLine("\tAssigning new project GUIDs");
        solutionText =
            solutionText.Replace("F210BE73-3C9E-4D27-9F0D-5B4094ACDA40", Guid.NewGuid().ToString("D").ToUpper());
        solutionText =
            solutionText.Replace("7D0D9772-6F5A-4255-B694-67DBED5AA96B", Guid.NewGuid().ToString("D").ToUpper());
        solutionText =
            solutionText.Replace("A601712B-0E49-4E7E-9475-B8045AB42E11", Guid.NewGuid().ToString("D").ToUpper());

        Console.WriteLine($"\tReplace 'ElectronApp' with '{newRootNamespace}'");
        solutionText = solutionText.Replace("ElectronAppRebrander", "AppRebrander");
        solutionText = solutionText.Replace("ElectronApp", newRootNamespace);
        solutionText = solutionText.Replace("AppRebrander", "ElectronAppRebrander");

        Console.WriteLine("\tSave changes");
        File.WriteAllText(apiSolutionFilename, solutionText);

        var oldApiSolutionFilename = apiSolutionFilename;
        apiSolutionFilename = apiSolutionFilename.Replace("ElectronApp", newRootNamespace);
        Console.WriteLine($"\tRename '{oldApiSolutionFilename}' to '{apiSolutionFilename}'");
        File.Move(oldApiSolutionFilename, apiSolutionFilename);

        Console.WriteLine("\tAPI solution file done.");
    }
    
    return true;
}
    
void ProcessProject(string projectFilename)
{
    if (File.Exists(projectFilename))
    {
        Console.WriteLine($"Processing project '{projectFilename}'...");

        var projectText = File.ReadAllText(projectFilename);

        Console.WriteLine($"\tReplace 'ElectronApp' with '{newRootNamespace}'");
        projectText = projectText.Replace("ElectronApp", newRootNamespace);

        Console.WriteLine("\tSave changes");
        File.WriteAllText(projectFilename, projectText);

        var oldprojectFilename = projectFilename;
        projectFilename = projectFilename.Replace("ElectronApp", newRootNamespace);
        var oldProjectDir = Path.GetDirectoryName(oldprojectFilename);
        var projectDir = Path.GetDirectoryName(projectFilename);
        Console.WriteLine($"\tRename '{oldProjectDir}' to '{projectDir}'");
        Directory.Move(oldProjectDir, projectDir);

        oldprojectFilename = Path.Combine(projectDir, Path.GetFileName(oldprojectFilename));
        Console.WriteLine($"\tRename '{oldprojectFilename}' to '{projectFilename}'");
        File.Move(oldprojectFilename, projectFilename);

        Console.WriteLine("\tDone with this project file");
        
        Console.WriteLine("\tDelete the 'bin' dir if present");
        var binDir = Path.Combine(projectDir, "bin");
        if (Directory.Exists(binDir)) Directory.Delete(binDir, true);
        Console.WriteLine("\tDelete the 'obj' dir if present");
        var objDir = Path.Combine(projectDir, "obj");
        if (Directory.Exists(objDir)) Directory.Delete(objDir, true);
        
        ProcessProjectSourceFiles(projectFilename);
    }
}

void ProcessProjectSourceFiles(string projectFilename)
{
    if (Directory.Exists(Path.GetDirectoryName(projectFilename)))
    {
        Console.WriteLine($"Processing project C# source files for '{projectFilename}'...");
    
        var csFiles = Directory.GetFiles(Path.GetDirectoryName(projectFilename), "*.cs", SearchOption.AllDirectories)
            .Where(x => !x.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !x.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")).ToArray();
        foreach (var file in csFiles)
        {
            Console.WriteLine($"\tUpdate {file}");
            var fileText = File.ReadAllText(file);
            fileText = fileText.Replace("ElectronApp", newRootNamespace).Replace("Electron App", newName);
            File.WriteAllText(file, fileText);
        }
    }    
}



