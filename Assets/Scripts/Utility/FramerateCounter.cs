using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class FramerateCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    public float pollingTime = 1f;
    public bool showDecimal;
    
    private float _time;
    private int _frameCount;

    private decimal _frameRate;
    public float FrameRate {
        get => (float)(showDecimal ? Math.Round(_frameRate, 2) : Math.Round(_frameRate));
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.unscaledDeltaTime;
        _frameCount++;
        if (_time >= pollingTime)
        {
            _frameRate = (decimal)(_frameCount / _time);
            fpsText.text = showDecimal ? 
                FrameRate.ToString("0.00") + "fps" : 
                FrameRate.ToString("0") + "fps";
            
            _time -= pollingTime;
            _frameCount = 0;
        }
    }
}
