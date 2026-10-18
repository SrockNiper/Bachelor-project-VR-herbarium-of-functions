using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Manages loading and saving of mathematical functions from external files.
    /// Handles file persistence and automatic updates via FileWatcher.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        [Header("File Settings")]
        public string fileName = "functions.txt";
        
        private string filePath;
        private string dirPath;
        
        /// <summary>
        /// List of functions loaded from the text file.
        /// </summary>
        public List<string> availableFunctions = new List<string>();
        
        /// <summary>
        /// Event fired when the function list is updated.
        /// </summary>
        public Action<List<string>> OnFunctionsLoaded;
        
        private FileWatcher fileWatcher;
        private bool needsRefresh = false;

        void Awake()
        {
            dirPath = Application.streamingAssetsPath;
            filePath = Path.Combine(dirPath, fileName);
            EnsureFileExistance();
        }

        void Start()
        {
            // Setup file monitoring for real-time updates
            fileWatcher = new FileWatcher();
            fileWatcher.StartWatching(dirPath, fileName);
            fileWatcher.OnFileChanged += () => needsRefresh = true;
        }

        void Update()
        {
            // Refresh functions on the main thread if a change was detected
            if (needsRefresh)
            {
                needsRefresh = false;
                LoadFunctions();
            }
        }

        /// <summary>
        /// Reads all functions from the target text file and notifies subscribers.
        /// </summary>
        public void LoadFunctions()
        {
            availableFunctions.Clear();
            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            availableFunctions.Add(line.Trim());
                        }
                    }
                }
                OnFunctionsLoaded?.Invoke(availableFunctions);
            }
            catch (IOException)
            {
                // Retry next frame if file is locked
                needsRefresh = true;
            }
        }

        /// <summary>
        /// Ensures the StreamingAssets folder and the functions file exist.
        /// Creates default functions if the file is missing.
        /// </summary>
        private void EnsureFileExistance()
        {
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            if (!File.Exists(filePath))
            {
                string[] defaultFuncs = {
                    "a*sin(b*x)+c*cos(d*y)",
                    "(a*x^2*b*y^2)/(c*x^3+d*y^2)",
                    "a*sin((b*x)*(c*y)+d)",
                    "a*x^2 + 6*(b*x)*(c*y) + d*y^2",
                    "c*abs((a*x)*(b*y))^2"
                };
                File.WriteAllLines(filePath, defaultFuncs);
            }
        }

        /// <summary>
        /// Creates a .bat shortcut in the root folder to easily open the functions file.
        /// </summary>
        public void CreateShortcut()
        {
            string dataPath = Application.dataPath;
            string rootFolder = Directory.GetParent(dataPath).FullName;
            string shortcutPath = Path.Combine(rootFolder, "1_Upravit_Rovnice.bat");

            if (!File.Exists(shortcutPath))
            {
                string dataFolderName = new DirectoryInfo(dataPath).Name;
                string batCommand = $"@echo off\nstart \"\" \"{dataFolderName}\\StreamingAssets\\{fileName}\"";
                File.WriteAllText(shortcutPath, batCommand);
            }
        }

        void OnDestroy()
        {
            fileWatcher?.Dispose();
        }
    }
}
