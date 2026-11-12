using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEditor.Android;
using UnityEngine;

namespace WakeTheWalls.Editor
{
    public class ArCorePermissionsNamespaceFix : IPostGenerateGradleAndroidProject
    {
        const string Library = "libs/unityandroidpermissions.aar";
        const string OldPackage = "package=\"com.google.ar.core\"";
        const string NewPackage = "package=\"com.unity3d.plugin.permissions\"";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string aar = Path.Combine(path, Library);
            if (!File.Exists(aar)) return;

            var entries = new List<(string name, byte[] data)>();
            bool changed = false;
            using (var source = new ZipArchive(File.OpenRead(aar), ZipArchiveMode.Read))
            {
                foreach (var entry in source.Entries)
                {
                    using var buffer = new MemoryStream();
                    using (var stream = entry.Open()) stream.CopyTo(buffer);
                    byte[] data = buffer.ToArray();
                    if (entry.FullName.EndsWith("AndroidManifest.xml"))
                    {
                        string text = Encoding.UTF8.GetString(data);
                        if (text.Contains(OldPackage))
                        {
                            data = Encoding.UTF8.GetBytes(text.Replace(OldPackage, NewPackage));
                            changed = true;
                        }
                    }
                    entries.Add((entry.FullName, data));
                }
            }
            if (!changed) return;

            using (var target = new ZipArchive(File.Create(aar), ZipArchiveMode.Create))
            {
                foreach (var (name, data) in entries)
                {
                    using var stream = target.CreateEntry(name).Open();
                    stream.Write(data, 0, data.Length);
                }
            }
            Debug.Log("Renamed the ARCore permissions library namespace for the Android build.");
        }
    }
}
