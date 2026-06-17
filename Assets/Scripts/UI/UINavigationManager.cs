using UnityEngine;

public class UINavigationManager : MonoBehaviour
{
    [Header("Panel Content Roots — drag each panel's Content child here")]
    [SerializeField] private GameObject _shopContent;
    [SerializeField] private GameObject _missionsContent;
    [SerializeField] private GameObject _passContent;
    [SerializeField] private GameObject _wardrobeContent;
    [SerializeField] private GameObject _vipContent;

    public void OpenShop()
    {
        Debug.Log("[UINav] OpenShop called");
        if (_shopContent != null) _shopContent.SetActive(true);
        ShopManager.Instance?.OpenShop();
    }

    public void CloseShop()
    {
        if (_shopContent != null) _shopContent.SetActive(false);
    }

    public void OpenMissions()
    {
        Debug.Log("[UINav] OpenMissions called");
        if (_missionsContent != null) _missionsContent.SetActive(true);
        MissionUI.Instance?.Open();
    }

    public void CloseMissions()
    {
        if (_missionsContent != null) _missionsContent.SetActive(false);
    }

    public void OpenPass()
    {
        Debug.Log("[UINav] OpenPass called");
        if (_passContent != null) _passContent.SetActive(true);
        PassUI.Instance?.Open();
    }

    public void ClosePass()
    {
        if (_passContent != null) _passContent.SetActive(false);
    }

    public void OpenWardrobe()
    {
        Debug.Log("[UINav] OpenWardrobe called");
        if (_wardrobeContent != null) _wardrobeContent.SetActive(true);
        WardrobeUI.Instance?.Open();
    }

    public void CloseWardrobe()
    {
        if (_wardrobeContent != null) _wardrobeContent.SetActive(false);
    }

    public void OpenVIP()
    {
        Debug.Log("[UINav] OpenVIP called");
        if (_vipContent != null) _vipContent.SetActive(true);
        VIPPanelUI.Instance?.Open();
    }

    public void CloseVIP()
    {
        if (_vipContent != null) _vipContent.SetActive(false);
    }
}
