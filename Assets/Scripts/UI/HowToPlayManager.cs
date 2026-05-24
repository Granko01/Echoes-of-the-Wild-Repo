using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _slides;
    [SerializeField] private Button       _prevButton;
    [SerializeField] private Button       _nextButton;
    [SerializeField] private float        _slideDistance = 800f;
    [SerializeField] private float        _slideDuration = 0.35f;

    private int  _currentIndex = 0;
    private bool _isAnimating  = false;

    private void Awake()
    {
        if (_prevButton != null) _prevButton.onClick.AddListener(PreviousSlide);
        if (_nextButton != null) _nextButton.onClick.AddListener(NextSlide);
    }

    private void OnEnable()
    {
        _currentIndex = 0;
        ShowSlide(0);
    }

    public void PreviousSlide()
    {
        if (_isAnimating || _currentIndex <= 0) return;
        int from = _currentIndex;
        _currentIndex--;
        StartCoroutine(TransitionSlide(from, _currentIndex, fromRight: false));
    }

    public void NextSlide()
    {
        if (_isAnimating || _slides == null || _currentIndex >= _slides.Length - 1) return;
        int from = _currentIndex;
        _currentIndex++;
        StartCoroutine(TransitionSlide(from, _currentIndex, fromRight: true));
    }

    private void ShowSlide(int index)
    {
        if (_slides == null) return;
        for (int i = 0; i < _slides.Length; i++)
            if (_slides[i] != null) _slides[i].SetActive(i == index);
        UpdateNavButtons();
    }

    private IEnumerator TransitionSlide(int fromIndex, int toIndex, bool fromRight)
    {
        _isAnimating = true;

        if (_slides == null || fromIndex >= _slides.Length || toIndex >= _slides.Length)
        {
            _isAnimating = false;
            yield break;
        }

        GameObject outSlide = _slides[fromIndex];
        GameObject inSlide  = _slides[toIndex];

        if (outSlide == null || inSlide == null)
        {
            ShowSlide(toIndex);
            _isAnimating = false;
            yield break;
        }

        inSlide.SetActive(true);

        RectTransform outRt = outSlide.GetComponent<RectTransform>();
        RectTransform inRt  = inSlide.GetComponent<RectTransform>();

        float    sign     = fromRight ? 1f : -1f;
        Vector2  outStart = outRt.anchoredPosition;
        Vector2  inStart  = new Vector2(sign * _slideDistance, 0f);
        Vector2  outEnd   = new Vector2(-sign * _slideDistance, 0f);
        Vector2  inEnd    = Vector2.zero;

        inRt.anchoredPosition = inStart;

        float elapsed = 0f;
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / _slideDuration);
            if (outRt != null) outRt.anchoredPosition = Vector2.Lerp(outStart, outEnd, t);
            if (inRt  != null) inRt.anchoredPosition  = Vector2.Lerp(inStart,  inEnd,  t);
            yield return null;
        }

        if (outRt != null) outRt.anchoredPosition = outStart;
        outSlide.SetActive(false);
        if (inRt  != null) inRt.anchoredPosition  = inEnd;

        UpdateNavButtons();
        _isAnimating = false;
    }

    private void UpdateNavButtons()
    {
        if (_slides == null) return;
        if (_prevButton != null) _prevButton.interactable = _currentIndex > 0;
        if (_nextButton != null) _nextButton.interactable = _currentIndex < _slides.Length - 1;
    }
}
