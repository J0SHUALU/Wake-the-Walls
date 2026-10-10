using System;
using System.Collections.Generic;
using UnityEngine;
using WakeTheWalls.Core;
using WakeTheWalls.Murals;

namespace WakeTheWalls.UI
{
    /// <summary>
    /// Remembers which murals have been woken (completed) during this session, for the wall stamps.
    /// It starts empty every time the app starts and listens to <see cref="GameEvents.MuralCompleted"/>.
    /// </summary>
    public static class SessionProgress
    {
        static readonly HashSet<MuralData> woken = new HashSet<MuralData>();

        /// <summary>Raised when a mural is woken for the first time this session.</summary>
        public static event Action Changed;

        /// <summary>Murals completed this session.</summary>
        public static ICollection<MuralData> Woken => woken;

        /// <summary>Number of murals completed this session.</summary>
        public static int Count => woken.Count;

        /// <summary>True if <paramref name="mural"/> has been completed this session.</summary>
        public static bool IsWoken(MuralData mural) => mural != null && woken.Contains(mural);

        // Subscribes before any scene loads, so the stamp is recorded before the completion screen reads it.
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
