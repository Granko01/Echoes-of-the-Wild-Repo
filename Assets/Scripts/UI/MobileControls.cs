using UnityEngine;

public class MobileControls : MonoBehaviour
{
    [SerializeField] private bool _forceShow = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool IsMobileBrowser();
#endif

    private void Start()
    {
#if UNITY_IOS || UNITY_ANDROID
        gameObject.SetActive(true);
#elif UNITY_WEBGL && !UNITY_EDITOR
        gameObject.SetActive(_forceShow || IsMobileBrowser());
#else
        gameObject.SetActive(_forceShow);
#endif
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
