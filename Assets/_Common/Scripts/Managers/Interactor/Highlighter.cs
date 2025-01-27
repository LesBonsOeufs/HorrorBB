using DG.Tweening;
using System;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
    [SerializeField] private Transform scaledTransform;
    [SerializeField] private GameObject activatedObject;
    [SerializeField] private float signDuration = 0.5f;
    [SerializeField] private float deltaScale = 0.2f;

    public event Action<bool> OnStateChanged;

    public bool IsOn
    {
        get => _isOn;

        set
        {
            if (!enabled || _isOn == value) return;

            _isOn = value;
            activatedObject.SetActive(_isOn);
            Vector3 lDeltaScale = Vector3.one * deltaScale * (_isOn ? 1f : -1f);
            scaledTransform.DOBlendableScaleBy(lDeltaScale, signDuration);
            OnStateChanged?.Invoke(_isOn);
        }
    }
    private bool _isOn = false;

    private void Awake()
    {
        activatedObject.SetActive(false);
    }
}