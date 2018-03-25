using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateVizAlphaWithSlider : MonoBehaviour {

    public Slider slider;

    void OnSlider()
    {
        PoseViz v = GetComponent<PoseViz>();
        v.colorAlpha = slider.value;
    }

}
