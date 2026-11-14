using System;
using TMPro;
using UnityEngine;

public class Amulet : MonoBehaviour
{
    private TextMeshProUGUI _amuletText;

    public static event Action<Amulet> OnPlayerEntered;
    public static event Action<Amulet> OnPlayerExited;

    private static readonly int _totalAmulets = 3; // Total number of amulets in the game
    private void Start()
    {
        _amuletText = GameObject.FindGameObjectWithTag("CollectiblesText").GetComponent<TextMeshProUGUI>();
        if (_amuletText != null)
        {
            _amuletText.text = $"{_totalAmulets} missing amulets";
        }
    }

    private void OnEnable()
    {
        PlayerController.OnAmuletCollected += Collect;
    }

    private void OnDisable()
    {
        PlayerController.OnAmuletCollected -= Collect;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        OnPlayerEntered?.Invoke(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExited?.Invoke(this);
        }
    }
    
    private void Collect(Amulet collectedAmuled, int amuletAmout)
    {
        if (collectedAmuled == this)
        {
            UpdateAmuletText(amuletAmout);
            OnPlayerExited?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private void UpdateAmuletText(int amuletCount)
    {
        if (_amuletText != null)
        {
            _amuletText.text = $"{_totalAmulets - amuletCount} missing amulets";
        }
    }
}
