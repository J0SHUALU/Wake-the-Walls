#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace WakeTheWalls.Editor
{
    public static class XcodeSwiftLibraryFix
    {
        const string SwiftLibraries = "$(DEVELOPER_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/lib/swift/$(PLATFORM_NAME)";

        [PostProcessBuild(100)]
        public static void OnPostprocessBuild(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;

            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            foreach (string guid in new[] { project.GetUnityFrameworkTargetGuid(), project.GetUnityMainTargetGuid() })
            {
                project.AddBuildProperty(guid, "LIBRARY_SEARCH_PATHS", SwiftLibraries);
            }
            File.WriteAllText(projectPath, project.WriteToString());
        }
    }
}
#endif
