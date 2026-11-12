#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace WakeTheWalls.Editor
{
    public static class PhotosUsageDescription
    {
        const string AddKey = "NSPhotoLibraryAddUsageDescription";
        const string AddText = "Wake the Walls saves the videos you record to your Photos.";

        [PostProcessBuild(101)]
        public static void OnPostprocessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;

            string plistPath = Path.Combine(buildPath, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            if (!plist.root.values.ContainsKey(AddKey)) plist.root.SetString(AddKey, AddText);
            plist.WriteToFile(plistPath);
        }
    }
}
#endif
