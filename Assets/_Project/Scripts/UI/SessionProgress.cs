using System;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    public static class SessionProgress
    {
        static readonly HashSet<MuralData> woken = new HashSet<MuralData>();

        public static event Action Changed;

        public static ICollection<MuralData> Woken => woken;

        public static int Count => woken.Count;

        public static bool IsWoken(MuralData mural) => mural != null && woken.Contains(mural);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            woken.Clear();
            Changed = null;
            GameEvents.MuralCompleted -= Record;
            GameEvents.MuralCompleted += Record;
        }

        static void Record(MuralData mural)
        {
            if (mural != null && woken.Add(mural)) Changed?.Invoke();
        }
    }
}
