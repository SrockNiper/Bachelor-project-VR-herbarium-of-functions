using UnityEngine;
using System.IO;
using System;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Wrapper around System.IO.FileSystemWatcher to provide simple event-based 
    /// file monitoring for Unity.
    /// </summary>
    public class FileWatcher : IDisposable
    {
        private FileSystemWatcher watcher;
        
        /// <summary>
        /// Event fired when the watched file is modified.
        /// </summary>
        public Action OnFileChanged;

        /// <summary>
        /// Starts watching for changes in the specified directory and file.
        /// </summary>
        /// <param name="path">Absolute path to the directory.</param>
        /// <param name="filter">File name filter (e.g. "functions.txt").</param>
        public void StartWatching(string path, string filter)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            watcher = new FileSystemWatcher(path, filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            
            // Note: This callback runs on a background thread
            watcher.Changed += (s, e) => OnFileChanged?.Invoke();
            
            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Disables events and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
        }
    }
}
