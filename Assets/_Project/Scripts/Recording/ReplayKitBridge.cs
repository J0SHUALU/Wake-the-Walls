#pragma warning disable CS0618
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.Apple.ReplayKit;

namespace WakeTheWalls.Recording
{
    static class ReplayKitBridge
    {
        public static bool Available => ReplayKit.APIAvailable;

        public static bool IsRecording => ReplayKit.isRecording;

        public static bool HasRecording => ReplayKit.recordingAvailable;

        public static string LastError => ReplayKit.lastError;

        public static void Start() => ReplayKit.StartRecording(false, false);

        public static void Stop() => ReplayKit.StopRecording();

        public static void Preview() => ReplayKit.Preview();
    }
}
#else
namespace WakeTheWalls.Recording
{
    static class ReplayKitBridge
    {
        public static bool Available => false;

        public static bool IsRecording => false;

        public static bool HasRecording => false;

        public static string LastError => string.Empty;

        public static void Start() { }

        public static void Stop() { }

        public static void Preview() { }
    }
}
#endif
