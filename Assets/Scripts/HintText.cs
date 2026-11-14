using TMPro;
using UnityEngine;

public class HintText : MonoBehaviour
{
    private TextMeshProUGUI _hintText;

    private void Awake()
    {
        _hintText = GameObject.FindGameObjectWithTag("HintText").GetComponent<TextMeshProUGUI>();
        _hintText.enabled = false;
    }

    private void OnEnable()
    {
        Amulet.OnPlayerEntered += ShowHint;
        Amulet.OnPlayerExited += HideHint;
    }

    private void OnDisable()
    {
        Amulet.OnPlayerEntered -= ShowHint;
        Amulet.OnPlayerExited -= HideHint;
    }

    private void ShowHint(Amulet amulet)
    {
        _hintText.enabled = true;
    }

    private void HideHint(Amulet amulet)
    {
        _hintText.enabled = false;
    }
}
