using System.Text;
using MikManager.Handlers;
using MikManager.Util;

namespace MikManager.CustomFileTypes
{
    public static class DependencyManager
    {
        private static readonly string fileName = ".dependencies";
        private static readonly string dirPath = $"{ModHandler.GetLCPath()}\\{MikPathGuardian.parentDir}";
        private static readonly string filePath = $"{dirPath}\\{fileName}";
        private static readonly string loggerID = "DependencyManager";
        private static Dictionary<string, byte> depRefs = new Dictionary<string, byte>();

        // public static void AddReference(IEnumerable<string> depStrings) 
        // {
        //     Debug.LogInfo($"Adding \"{depStrings}\" references...", loggerID);
        //     foreach (string dep in depStrings)
        //         AddReferenceCore(dep, 1, depRefs);
        // }

        public static void AddReference(string depString) 
        {
            Debug.LogInfo($"Adding \"{depString}\" reference...", loggerID);
            AddReferenceCore(depString, 1, depRefs);
            Write();
        }

        private static void AddReferenceCore(string depString, byte references, Dictionary<string, byte> dict) 
        {
            dict.TryAdd(depString, 0);
            dict[depString] += references;
        }



        // public static void RemoveReference(IEnumerable<string> depStrings) 
        // {
        //     Debug.LogInfo($"Removing \"{depString}\" references...", loggerID);
        //     RemoveReferenceCore(depString);
        // }

        public static void RemoveReference(string depString) 
        {
            Debug.LogInfo($"Removing \"{depString}\" reference...", loggerID);
            RemoveReferenceCore(depString, 1, depRefs);
            Write();
        }

        private static void RemoveReferenceCore(string depString, byte references, Dictionary<string, byte> dict) 
        {
            if (!dict.ContainsKey(depString)) 
            {
                Debug.LogWarning($"Attempted to remove \"{depString}\" reference when it does not exist", loggerID);
                return;
            };
            dict[depString] -= references;
            if (dict[depString] < 1) dict.Remove(depString);
        }


        
        public static byte GetReferences(string depString) => depRefs.TryGetValue(depString, out byte value) ? value : (byte)0;

        public static void Clear() 
        {
            Debug.LogInfo("Clearing dependencies...", loggerID);
            Write();
            depRefs.Clear();
        }


        // Writes the current contents to a file
        public static void Write()
        {
            Debug.LogInfo("Writing file...", loggerID);
            MikPathGuardian.EnsureMikManagerDirsQuiet();
            if (!Directory.Exists(dirPath))
            {
                Debug.LogError($"Unable to write file: {dirPath} does not exist", loggerID);
                return;
            }
            FileStream stream = File.Open(filePath, FileMode.Create);
            byte[] buffer = SerializeData(depRefs);
            stream.Write(buffer, 0, buffer.Length);
            stream.Close();
        }

        // Attempts to read the file and overwrites the current data with the file data
        public static void Read()
        {
            Debug.LogInfo("Reading from file...", loggerID);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Dependencies file does not exist: {filePath}", loggerID);
                return;
            }
            byte[] buffer = File.ReadAllBytes(filePath);
            depRefs = DeserializeData(buffer);
        }

        private static byte[] SerializeData(Dictionary<string, byte> dict) => Encoding.UTF8.GetBytes(ToStringCore(dict));

        private static Dictionary<string, byte> DeserializeData(byte[] buffer)
        {
            Dictionary<string, byte> dict = new Dictionary<string, byte>();
            string dataString = Encoding.UTF8.GetString(buffer);
            if (dataString.Length < 2) return dict;
            // One entry looks something like "bizzlemip-BiggerLobby-2.7.0:5"
            string[] entries = dataString.Split('\n');
            foreach (string entry in entries)
            {
                string depString = entry.Split(':')[0];
                byte refs;
                try { refs = byte.Parse(entry.Split(':')[1]); }
                catch (Exception e)
                { 
                    Debug.LogError($"Unable to deserialize entry \"{entry}\": {e.Message}", loggerID); 
                    continue;
                }
                AddReferenceCore(depString, refs, dict);
            }
            return dict;
        }

        public new static string ToString() => ToStringCore(depRefs);

        private static string ToStringCore(Dictionary<string, byte> dict)
        {
            // For every line it will be [string]:[number]\n
            // Ill say the average [string] is 20 characters
            int charactersGuess = dict.Count * 3 + dict.Count * 20;
            StringBuilder build = new StringBuilder(charactersGuess);
            foreach (KeyValuePair<string, byte> kvp in dict)
                build.Append($"{kvp.Key}:{kvp.Value}\n");
            string str = build.ToString();
            return (str.Length > 0) ? str.Remove(str.Length - 1) : str; // Remove the last \n
        }
    }
}