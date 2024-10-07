using System.IO.Compression;
using System.Text;
using MikManager.CustomFileTypes;
using MikManager.Handlers;
using MikManager.Util;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace MikManager.CustomFileTypes
{
    public class MikModDescription
    {
        private readonly string name = "null";
        private readonly string version = "null";
        private readonly string developer = "null";
        private readonly string dependencyString = "null";
        private readonly string description = "null";
        private readonly string githubLink = "null";
        private readonly string thunderstoreLink = "null";
        private readonly string[] dependencies = [];

        private bool isInstalled;

        // Might be empty if isInstalled flag is false
        private readonly string[] installPaths = [];

        // Might be empty if isInstalled flag is true
        private readonly Dictionary<string, string> installPathsDict = [];
        private readonly string extractedModPath = "null";
        
        private static readonly string FILE_FORMAT = ".lcmd";
        private static readonly string loggerID = "MikModDescription";

        public MikModDescription(string path, bool isModFile = true)
        {
            if (isModFile)
            {
                string modFilePath = path;
                string modFileName = Path.GetFileName(modFilePath);
                Debug.LogInfo($"Loading {modFileName} data...", loggerID);

                try {
                    string dataString = Encoding.UTF8.GetString(File.ReadAllBytes(modFilePath));
                    string[] dataSegements = dataString.Split('\0');
                    
                    this.name = dataSegements[0].Replace("\n", "");
                    this.version = dataSegements[1].Replace("\n", "");
                    this.developer = dataSegements[2].Replace("\n", "");
                    this.dependencyString = dataSegements[3].Replace("\n", "");
                    this.description = dataSegements[4].Replace("\n", "");
                    this.githubLink = dataSegements[5].Replace("\n", "");
                    this.thunderstoreLink = dataSegements[6].Replace("\n", "");

                    string[] dependenciesTemp = dataSegements[7].Split('\n');
                    this.dependencies = new string[dependenciesTemp.Length - 1];
                    Array.Copy(dependenciesTemp, 1, dependencies, 0, dependenciesTemp.Length - 1);

                    string[] installPathsTemp = dataSegements[8].Split('\n');
                    this.installPaths = new string[installPathsTemp.Length - 1];
                    Array.Copy(installPathsTemp, 1, installPaths, 0, installPathsTemp.Length - 1);

                    this.isInstalled = true;
                } catch (Exception e) {
                    Debug.LogError($"Error loading mod from .modd file path: {modFilePath}, {e}", loggerID);
                    this.isInstalled = false;
                }
            } 
            else
            {
                string extractedModFolderPath = path;
                this.extractedModPath = extractedModFolderPath;

                string manifestJsonPath = extractedModFolderPath + "/manifest.json";
                string? depString = Path.GetFileName(extractedModFolderPath);
                if (depString == null)
                {
                    Debug.LogError($"Can not generate mod description from path: {extractedModFolderPath}", loggerID);
                    return; 
                }
                this.dependencyString = depString;
                this.developer = depString.Split("-")[0];

                this.isInstalled = false;

                try {
                    string jsonString = File.ReadAllText(manifestJsonPath);
                    JObject jsonObject = JObject.Parse(jsonString);
                    this.name = (string?)jsonObject["name"] ?? "";
                    this.version = (string?)jsonObject["version_number"] ?? "";
                    this.githubLink = (string?)jsonObject["website_url"] ?? "";
                    this.description = (string?)jsonObject["description"] ?? "";
                    JArray? dependencyList = (JArray?)jsonObject["dependencies"] ?? [];
                    JToken[] dependencyArray = dependencyList.ToArray();
                    this.dependencies = dependencyArray
                        .Select(v => (string?)v)
                        .Where(v => v != null)
                        .Cast<string>()  // Cast to non-nullable string
                        .ToArray();
                } catch (Exception e) {
                    Debug.LogError($"Failed to load mod description from extracted folder: {e}", loggerID);
                    return;
                }
            
                this.thunderstoreLink = $"https://thunderstore.io/c/lethal-company/p/{developer}/{this.name}/";

                bool successful = DetermineSourceAndInstallPath(extractedModFolderPath, out string sourcePath, out string installPath);
                if (!successful)
                {
                    Debug.LogError($"Cannot determine install path for {Path.GetFileName(extractedModFolderPath)}, manual intervention required", loggerID);
                    Debug.LogInfo($"Mod can be found at: {extractedModFolderPath}", loggerID);
                    return; 
                }

                this.installPathsDict = CreateInstallDict(sourcePath, installPath);
                if (this.installPathsDict.Count == 0) Debug.LogError($"Extracted mod folder contains no mods: {extractedModFolderPath}", loggerID);  
            }
        }
    
        public string Name => name;
        public string Version => version;
        public string DependencyString => dependencyString;
        public string Description => description;
        public string Developer => developer;
        public string GithubLink => githubLink;
        public string ThunderstoreLink => thunderstoreLink;
        public string[] Dependencies => (string[])dependencies.Clone();
        public string[] InstallPaths => isInstalled ? (string[])installPaths.Clone() : installPathsDict.Values.ToArray();
        public Dictionary<string, string> PathsDictionary => new Dictionary<string, string>(this.installPathsDict);
        public bool AreFilePathsLoaded() => installPathsDict.Count > 0;
        public string ModFileName => $"{dependencyString}{FILE_FORMAT}";
        public string ModFilePath => $"{MikPathGuardian.modFilesPath}/{ModFileName}";


        /// <summary>
        /// Installs mod files and calls Write() afterwords
        /// </summary>
        /// <returns></returns>
        public bool Install()
        {
            Debug.LogInfo($"Installing {this.dependencyString}...", loggerID);
            if (this.installPathsDict.Count == 0) Debug.LogWarning($"Called to install {this.name} when folder appears to be empty", loggerID);
            if (isInstalled) Debug.LogWarning($"Called to install {this.name} when isInstalled is already true", loggerID);
            if (isInstalled) return true;

            foreach (var kvp in installPathsDict)
            {
                string sourcePath = kvp.Key;
                string installPath = kvp.Value;
                string? directory = Path.GetDirectoryName(installPath);
                if (directory == null)
                {
                    Debug.LogError($"Error getting directory path from file: {sourcePath}", loggerID);
                    // TODO: Move successful Files back
                    return false;
                }
                
                Directory.CreateDirectory(directory);

                if (File.Exists(installPath))
                {
                    Debug.LogWarning($"Trying to install source file \"{sourcePath}\" but destination file \"{installPath}\" already exists", loggerID);
                    return false;
                }
                if (!File.Exists(sourcePath))
                 {
                    Debug.LogError($"Source file \"{sourcePath}\" is missing", loggerID);
                    return false;
                }

                File.Move(sourcePath, installPath);  
            }

            Debug.LogInfo($"Deleting {this.dependencyString} source folder...", loggerID);
            try {
                Directory.Delete(this.extractedModPath, true);
            } catch (Exception e) {
                Debug.LogError($"Failed to delete extracted mod folder: {this.extractedModPath}: {e}", loggerID);
            } 

            foreach (string depString in dependencies)
                DependencyManager.AddReference(depString);

            Write();
            this.isInstalled = true;
            return true;
        }



        /// <summary>
        /// Deletes installed files
        /// </summary>
        /// <returns></returns>
        public bool Delete()
        {
            bool successful = true;
            Debug.LogInfo($"Deleting {this.dependencyString} mod files...", loggerID);
            if (!isInstalled)
            {
                Debug.LogWarning($"Called to delete \"{name}\" when mod is not installed", loggerID);
                return true;
            } 
            if (DependencyManager.GetReferences(this.dependencyString) > 0)
            {
                Debug.LogInfo($"Called to delete \"{name}\" while mod still has references, aborting...", loggerID);
                return true;
            } 

            // Delete dll files
            foreach (string path in InstallPaths)
            {
                try {
                    File.Delete(path);
                } catch (Exception e) {
                    Debug.LogError($"Failed to delete file: {path}: {e}", loggerID);
                    successful = false;
                    continue;
                } 
            
                string? directory = Path.GetDirectoryName(path);
                if (directory == null) continue;
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    try {
                        Directory.Delete(directory, false);
                    } catch (Exception e) {
                        Debug.LogError($"Failed to delete directory: {directory}: {e}", loggerID);
                        successful = false;
                    } 
                }
            }

            // Delete .lcmd file
            try {
                File.Delete(ModFilePath);
            } catch (Exception e) {
                Debug.LogError($"Failed to delete file: {ModFilePath}: {e}", loggerID);
                successful = false;
            } 

            // NOTE: These are in two seperate foreach loops because i think there might be a problem
            // the dependency manager getting out of sync maybe

            // Remove any dependency references
            if (dependencies.Length > 0 && dependencies[0].Length > 0)
            {
                foreach (string dependency in dependencies)
                    DependencyManager.RemoveReference(dependency);
            }

            // Remove dependency mods if references are 0
            if (dependencies.Length > 0 && dependencies[0].Length > 0)
            {
                foreach (string dependency in dependencies)
                {
                    if (DependencyManager.GetReferences(dependency) == 0)
                    {
                        Debug.LogInfo($"Dependency \"{dependency}\" has 0 references, sending delete request...", loggerID);
                        MikModDescription? dependencyMod = LCMDWarehouse.GetModDescription(dependency);
                        if (dependencyMod == null)
                        {
                            Debug.LogError($"Unable to find {dependency} in warehouse for deletion as mod has 0 references", loggerID);
                            continue;
                        }
                        dependencyMod.Delete();
                    } 
                }
            }

            return successful;
        }



        // Writes the current contents to a file
        public void Write()
        {
            Debug.LogInfo($"Writing {ModFileName} file...", loggerID);
            MikPathGuardian.EnsureMikManagerDirsQuiet();
            if (!Directory.Exists(MikPathGuardian.modFilesPath))
            {
                Debug.LogError($"Unable to write file: {MikPathGuardian.modFilesPath} does not exist", loggerID);
                return;
            }
            FileStream stream = File.Open(this.ModFilePath, FileMode.Create);
            byte[] buffer = this.SerializeData();
            stream.Write(buffer, 0, buffer.Length);
            stream.Close();
        }

        private byte[] SerializeData() => Encoding.UTF8.GetBytes(ToStringCore());





        // TODO need to be able to write and read to a file
        public override string ToString()
        {
            return ToStringCore();
        }

        private string ToStringCore()
        {
            // For every line it will be [string]:[number]\n
            // Ill say the average [string] is 20 characters
            int characters = name.Length + version.Length + description.Length + dependencyString.Length 
            + developer.Length + githubLink.Length + thunderstoreLink.Length;
            foreach (var kvp in installPathsDict)
                characters += kvp.Value.Length;
            characters += 7 + (isInstalled ? installPaths.Length : installPathsDict.Count) + dependencies.Length - 1; // the amount of \n
            characters += 8; // the amount of \0

            StringBuilder build = new StringBuilder(characters);
            build.Append(name).Append('\0').Append('\n');
            build.Append(version).Append('\0').Append('\n');
            build.Append(developer).Append('\0').Append('\n');
            build.Append(dependencyString).Append('\0').Append('\n');
            build.Append(description).Append('\0').Append('\n');
            build.Append(githubLink).Append('\0').Append('\n');
            build.Append(thunderstoreLink).Append('\0').Append('\n');
            build.Append(String.Join('\n', dependencies)).Append('\0').Append('\n');
            build.Append(String.Join('\n', InstallPaths));

            return build.ToString();
        }





        private static bool DetermineSourceAndInstallPath(string extractedModPath, out string sourcePath, out string installPath)
        {
            if (Directory.Exists($"{extractedModPath}\\BepInExPack"))
            {
                sourcePath = extractedModPath + "\\BepInExPack";
                installPath = ModHandler.GetLCPath();
                return true;
            } 
            else if (Directory.Exists($"{extractedModPath}\\BepInEx"))
            {
                sourcePath = extractedModPath;
                installPath = ModHandler.GetLCPath();
                return true;
            }
            else if (Directory.Exists($"{extractedModPath}\\plugins"))
            {
                sourcePath = extractedModPath;
                installPath = ModHandler.GetLCPath() + "\\BepInEx";
                return true;
            }
            else if (Directory.Exists($"{extractedModPath}\\LethalCompanyInputUtils"))
            {
                sourcePath = extractedModPath + "\\LethalCompanyInputUtils";
                installPath = ModHandler.GetLCPath() + "\\BepInEx\\plugins";
                return true;
            }
            else if (Directory.Exists($"{extractedModPath}\\config") || Directory.Exists($"{extractedModPath}\\patchers"))
            {
                sourcePath = extractedModPath;
                installPath = ModHandler.GetLCPath() + "\\BepInEx";
                return true;
            }
            // Check if dll file is just in the extracted folder bare bones
            else if (Directory.GetFiles(extractedModPath).Any((filePath) => filePath.EndsWith(".dll")))
            {
                sourcePath = extractedModPath;
                installPath = ModHandler.GetLCPath() + "\\BepInEx\\plugins";
                return true;
            }
            
            // Couldn't figure out the layout, return false
            installPath = "";
            sourcePath = "";
            return false;
        }

        private static Dictionary<string, string> CreateInstallDict(string sourceDir, string destinationDir)
        {
            // Ensure the destination directory exists
            // Directory.CreateDirectory(destinationDir);
            Dictionary<string, string> installDict = new Dictionary<string, string>();
            // Move files
            foreach (string file in Directory.GetFiles(sourceDir))
            {  
                try 
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destinationDir, fileName);

                    // This is specifically for skipping unnecessary content that
                    // comes with the downloaded mod
                    bool skip = fileName == "manifest.json" || fileName == "CHANGELOG.md" || fileName == "README.md" || fileName == "icon.png";
                    if (skip) continue;

                    // if (!File.Exists(destFile))
                    //     File.Move(file, destFile);
                    installDict.TryAdd(file, destFile);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error determining file install path: {ex}", loggerID);
                }
            }

            // Move directories
            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(destinationDir, dirName);

                // Recursively move the contents of the directory
                // installDict.Concat(CreateInstallDict(directory, destDir));
                installDict = installDict.Union(CreateInstallDict(directory, destDir)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);  // Overwrites duplicates
                // Delete the source directory after its contents have been moved
                // Directory.Delete(directory, true);
            }

            return installDict;
        }  
    }
}