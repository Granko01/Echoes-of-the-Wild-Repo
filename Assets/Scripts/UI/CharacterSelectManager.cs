using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    public const string PrefKey = "SelectedCharacter";

    [SerializeField] private CharacterData[] _characters;

    [Header("UI References")]
    [SerializeField] private Image           _portrait;
    [SerializeField] private Image           _colorIndicator;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;

    private int _selectedIndex;

    private void OnEnable()
    {
        _selectedIndex = PlayerPrefs.GetInt(PrefKey, 0);
        if (_characters != null && _characters.Length > 0)
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _characters.Length - 1);
        Refresh();
    }

    public void SelectPrev()
    {
        if (_characters == null || _characters.Length == 0) return;
        _selectedIndex = (_selectedIndex - 1 + _characters.Length) % _characters.Length;
        Refresh();
    }

    public void SelectNext()
    {
        if (_characters == null || _characters.Length == 0) return;
        _selectedIndex = (_selectedIndex + 1) % _characters.Length;
        Refresh();
    }

    public void ConfirmSelection()
    {
        PlayerPrefs.SetInt(PrefKey, _selectedIndex);
        PlayerPrefs.Save();
    }

    private void Refresh()
    {
        if (_characters == null || _characters.Length == 0) return;
        CharacterData cd = _characters[_selectedIndex];
        if (cd == null) return;

        if (_nameText != null) _nameText.text = cd.characterName;
        if (_descText != null) _descText.text = cd.description;

        if (_portrait != null)
        {
            if (cd.portrait != null)
            {
                _portrait.sprite  = cd.portrait;
                _portrait.color   = Color.white;
            }
            else
            {
                _portrait.sprite  = null;
                _portrait.color   = cd.characterColor;
            }
        }

        if (_colorIndicator != null)
            _colorIndicator.color = cd.characterColor;
    }
}
