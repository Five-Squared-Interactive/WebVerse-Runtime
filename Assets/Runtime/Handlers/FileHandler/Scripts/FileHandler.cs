// Copyright (c) 2019-2026 Five Squared Interactive. All rights reserved.

using System.IO;
using FiveSQD.WebVerse.Utilities;
using UnityEngine;

namespace FiveSQD.WebVerse.Handlers.File
{
    /// <summary>
    /// WebVerse's File Handler.
    /// </summary>
    public class FileHandler : BaseHandler
    {
        /// <summary>
        /// Directory to use for WebVerse files.
        /// </summary>
        public string fileDirectory { get; private set; }

        /// <summary>
        /// Get a file URI representation of a URI.
        /// </summary>
        /// <param name="uri">URI to get file URI representation of.</param>
        /// <returns>File URI representation of URI.</returns>
        public static string ToFileURI(string uri)
        {
            return uri.Replace(":", "~");

        }

        /// <summary>
        /// Get a URI representation of a File URI.
        /// </summary>
        /// <param name="fileURI">File URI to get URI representation of.</param>
        /// <returns>URI representation of file URI.</returns>
        public static string FromFileURI(string fileURI)
        {
            return fileURI.Replace("http~", "http:").Replace("https~", "https:");
        }

        /// <summary>
        /// Initialize the File Handler. This call is invalid. Initialize
        /// must be called with a provided file directory.
        /// </summary>
        public override void Initialize()
        {
            Logging.LogError("[FileHandler->Initialize] Initialize must be called with a file directory.");
        }

        /// <summary>
        /// Initialize the File Handler.
        /// </summary>
        /// <param name="fileDirectory">Directory to use for WebVerse files.</param>
        public void Initialize(string fileDirectory)
        {
            base.Initialize();

            this.fileDirectory = fileDirectory;

            Logging.Log("[FileHandler->Initialize] Using file directory: " + Path.GetFullPath(fileDirectory));

            if (Directory.Exists(this.fileDirectory))
            {
                Logging.Log("[FileHandler->Initialize] File directory exists.");
            }
            else
            {
                Logging.Log("[FileHandler->Initialize] File directory does not exist. Creating...");
                Directory.CreateDirectory(this.fileDirectory);
            }
        }

        /// <summary>
        /// Terminate the File Handler.
        /// </summary>
        public override void Terminate()
        {
            base.Terminate();
        }

        /// <summary>
        /// Check if a file exists in the file directory.
        /// </summary>
        /// <param name="file">Path (relative to file directory) to check for.</param>
        /// <returns>Whether or not the file exists in the file directory.</returns>
        public bool FileExistsInFileDirectory(string file)
        {
            return System.IO.File.Exists(Path.Combine(fileDirectory, file));
        }

        /// <summary>
        /// Create a file in the file directory.
        /// </summary>
        /// <param name="fileName">Path (relative to file directory) to use.</param>
        /// <param name="data">Data to write.</param>
        public void CreateFileInFileDirectory(string fileName, byte[] data)
        {
            if (FileExistsInFileDirectory(fileName))
            {
                Logging.LogWarning("[FileHandler->CreateFileInFileDirectory] File already exists: " + fileName);
                return;
            }

            CreateDirectoryStructure(Path.Combine(fileDirectory, fileName));
            System.IO.File.WriteAllBytes(Path.Combine(fileDirectory, fileName), data);
        }

        /// <summary>
        /// Create a file in the file directory.
        /// </summary>
        /// <param name="fileName">Path (relative to file directory) to use.</param>
        /// <param name="image">Image to write.</param>
        public void CreateFileInFileDirectory(string fileName, Texture2D image)
        {
            if (FileExistsInFileDirectory(fileName))
            {
                Logging.LogWarning("[FileHandler->CreateFileInFileDirectory] File already exists: " + fileName);
                return;
            }

            CreateDirectoryStructure(Path.Combine(fileDirectory, fileName));

            System.IO.File.WriteAllBytes(Path.Combine(fileDirectory, fileName), image.EncodeToPNG());
        }

        /// <summary>
        /// Delete a file in the file directory.
        /// </summary>
        /// <param name="fileName">Path (relative to file directory) to use.</param>
        public void DeleteFileInFileDirectory(string fileName)
        {
            if (!FileExistsInFileDirectory(fileName))
            {
                Logging.LogWarning("[FileHandler->DeleteFileInFileDirectory] No file: " + fileName);
                return;
            }

            System.IO.File.Delete(Path.Combine(fileDirectory, fileName));
        }

        /// <summary>
        /// Get a file in the file directory.
        /// </summary>
        /// <param name="fileName">Path (relative to file directory) to use.</param>
        /// <returns>The data that was read from the file, or null.</returns>
        public byte[] GetFileInFileDirectory(string fileName)
        {
            if (!FileExistsInFileDirectory(fileName))
            {
                Logging.LogWarning("[FileHandler->GetFileInFileDirectory] No file: " + fileName);
                return null;
            }

            return System.IO.File.ReadAllBytes(Path.Combine(fileDirectory, fileName));
        }

        /// <summary>
        /// Create the directory structure (necessary subdirectories) to support a file.
        /// </summary>
        /// <param name="fileName">Path (relative to file directory) to use.</param>
        private void CreateDirectoryStructure(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        }

        /// <summary>
        /// Clear the cache.
        /// </summary>
        /// <param name="seconds">Seconds to look back (files deeper than this will be deleted). -1 for all.</param>
        public void ClearCache(float seconds)
        {
            if (!Directory.Exists(fileDirectory))
            {
                Logging.LogWarning("[FileHandler->ClearCache] File directory does not exist.");
                return;
            }

            string[] files = Directory.GetFiles(fileDirectory, "*", System.IO.SearchOption.AllDirectories);
            int deletedCount = 0;
            foreach (string file in files)
            {
                if (seconds < 0)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                        deletedCount++;
                    }
                    catch (System.Exception e)
                    {
                        Logging.LogError("[FileHandler->ClearCache] Error deleting file: " + file + ". " + e.Message);
                    }
                }
                else
                {
                    System.DateTime lastWriteTime = System.IO.File.GetLastWriteTime(file);
                    if ((System.DateTime.Now - lastWriteTime).TotalSeconds < seconds)
                    {
                        try
                        {
                            System.IO.File.Delete(file);
                            deletedCount++;
                        }
                        catch (System.Exception e)
                        {
                            Logging.LogError("[FileHandler->ClearCache] Error deleting file: " + file + ". " + e.Message);
                        }
                    }
                }
            }
            Logging.Log("[FileHandler->ClearCache] Cleared " + deletedCount + " files from " + fileDirectory + ".");

            // Clean up empty subdirectories (deepest first).
            CleanupEmptyDirectories(fileDirectory);
        }

        /// <summary>
        /// Recursively delete empty subdirectories.
        /// </summary>
        /// <param name="directory">Directory to clean up.</param>
        private void CleanupEmptyDirectories(string directory)
        {
            foreach (string subDir in Directory.GetDirectories(directory))
            {
                CleanupEmptyDirectories(subDir);
                try
                {
                    if (Directory.GetFiles(subDir).Length == 0 && Directory.GetDirectories(subDir).Length == 0)
                    {
                        Directory.Delete(subDir);
                    }
                }
                catch (System.Exception e)
                {
                    Logging.LogError("[FileHandler->CleanupEmptyDirectories] Error deleting directory: " + subDir + ". " + e.Message);
                }
            }
        }
    }
}