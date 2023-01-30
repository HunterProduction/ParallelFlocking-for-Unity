using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UISliderValue : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    private TextMeshProUGUI _textField;

    private void Awake()
    {
        _textField = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        _textField.text = _slider.value.ToString("0.0");
    }
}
