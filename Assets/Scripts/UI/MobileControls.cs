using UnityEngine;

// Attach to the MobileControls root GameObject.
// Auto-hides on desktop; always visible on mobile / in Editor when _forceShow is true.
public class MobileControls : MonoBehaviour
{
    [SerializeField] private bool _forceShow = false;   // tick this in Editor to test

    private void Start()
    {
#if UNITY_IOS || UNITY_ANDROID
        gameObject.SetActive(true);
#else
        // SystemInfo.deviceType is Handheld when a phone opens a WebGL build in a browser
        bool isMobile = SystemInfo.deviceType == DeviceType.Handheld;
        gameObject.SetActive(_forceShow || isMobile);
#endif
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
