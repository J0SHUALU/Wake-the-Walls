using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace WakeTheWalls.Core
{
    public class BootLoader : MonoBehaviour
    {
        [Tooltip("Scene loaded once AR support is confirmed.")]
        [SerializeField] string mainSceneName = "Main";

        [Tooltip("Shown when the device cannot run AR. Hidden on start.")]
        [SerializeField] GameObject unsupportedMessage;

        [SerializeField] int targetFrameRate = 60;

        void Awake()
        {
            Application.targetFrameRate = targetFrameRate;

            if (unsupportedMessage != null)
                unsupportedMessage.SetActive(false);
        }

        IEnumerator Start()
        {
            yield return ARSession.CheckAvailability();

            if (ARSession.state == ARSessionState.Unsupported)
            {
                ShowUnsupported();
                yield break;
            }

            yield return SceneManager.LoadSceneAsync(mainSceneName);
        }

        void ShowUnsupported()
        {
            Debug.LogWarning("AR is not supported on this device.");

            if (unsupportedMessage != null)
                unsupportedMessage.SetActive(true);
        }
    }
}
