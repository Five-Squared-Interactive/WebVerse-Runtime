using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FiveSQD.WebVerse.Handlers.File;
using System.IO;

namespace FiveSQD.WebVerse.Handlers.File.Tests
{
    public class FileHandlerCacheTests
    {
        private string testDirectory;
        private FileHandler fileHandler;
        private GameObject fileHandlerObject;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            LogAssert.ignoreFailingMessages = true;
        }

        [SetUp]
        public void Setup()
        {
            testDirectory = Path.Combine(Application.temporaryCachePath, "FileHandlerCacheTests");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
            Directory.CreateDirectory(testDirectory);

            fileHandlerObject = new GameObject();
            fileHandler = fileHandlerObject.AddComponent<FileHandler>();
            fileHandler.Initialize(testDirectory);
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
            Object.DestroyImmediate(fileHandlerObject);
        }

        [Test]
        public void ClearCache_RemovesRecentFiles()
        {
            // Create a file that is "recent" (0 seconds old)
            string recentFile = Path.Combine(testDirectory, "recent.txt");
            System.IO.File.WriteAllText(recentFile, "recent content");

            // Create a file that is "old" (simulated by setting LastWriteTime to 2 hours ago)
            string oldFile = Path.Combine(testDirectory, "old.txt");
            System.IO.File.WriteAllText(oldFile, "old content");
            System.IO.File.SetLastWriteTime(oldFile, System.DateTime.Now.AddHours(-2));

            // Clear cache older than 1 hour (3600 seconds)
            // Wait, my logic was: delete files NEWER than X seconds.
            // If I request "Clear cache from last 1 hour", I want to delete files created/modified in the last 1 hour.
            // So recentFile (0s old) < 3600s -> DELETE.
            // oldFile (2h old) > 3600s -> KEEP.
            
            fileHandler.ClearCache(3600);

            Assert.IsFalse(System.IO.File.Exists(recentFile), "Recent file should be deleted.");
            Assert.IsTrue(System.IO.File.Exists(oldFile), "Old file should be kept.");
        }

        [Test]
        public void ClearCache_AllTime_RemovesAllFiles()
        {
            string file1 = Path.Combine(testDirectory, "file1.txt");
            System.IO.File.WriteAllText(file1, "content");
            string file2 = Path.Combine(testDirectory, "file2.txt");
            System.IO.File.WriteAllText(file2, "content");

            fileHandler.ClearCache(-1);

            Assert.IsFalse(System.IO.File.Exists(file1), "File1 should be deleted.");
            Assert.IsFalse(System.IO.File.Exists(file2), "File2 should be deleted.");
        }
    }
}
