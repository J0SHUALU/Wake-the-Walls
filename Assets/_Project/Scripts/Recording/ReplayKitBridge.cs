#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace WakeTheWalls.Recording
{
    enum RecorderState
    {
        Idle,
        Starting,
        Recording,
        Saving,
        Saved,
        PhotosDenied,
        Failed
    }

    static class ReplayKitBridge
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern int WtwRecorderIsAvailable();

        [DllImport("__Internal")] static extern int WtwRecorderIsRecording();

        [DllImport("__Internal")] static extern int WtwRecorderState();

        [DllImport("__Internal")] static extern void WtwRecorderStart();

        [DllImport("__Internal")] static extern void WtwRecorderStop();

        [DllImport("__Internal")] static extern void WtwRecorderReset();

        public static bool Available => WtwRecorderIsAvailable() != 0;

        public static bool IsRecording => WtwRecorderIsRecording() != 0;

        public static RecorderState State => (RecorderState)WtwRecorderState();

        public static void Start() => WtwRecorderStart();

        public static void Stop() => WtwRecorderStop();

        public static void Reset() => WtwRecorderReset();
#else
        public static bool Available => false;

        public static bool IsRecording => false;

        public static RecorderState State => RecorderState.Idle;

        public static void Start() { }

        public static void Stop() { }

        public static void Reset() { }
#endif
    }
}
