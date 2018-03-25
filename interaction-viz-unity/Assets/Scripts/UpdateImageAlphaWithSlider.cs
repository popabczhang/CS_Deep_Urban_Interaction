using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateImageAlphaWithSlider : MonoBehaviour {

	public Slider slider;

	void OnSlider()
    {
        Image img = GetComponent<Image>();
        Color c = img.material.color;
        img.material.color = new Color(c.r, c.g, c.b, slider.value);
        //Debug.Log("now!");
    }

}
