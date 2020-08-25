using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AnimalsCountSlider : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void SetMaxCount(float value)
    {
        if (slider != null)
        {
            slider.maxValue = (float) value * value / 2;
            GetComponent<SimpleSlider>().ChangeMaxValueText(slider.maxValue);
            slider.value = Mathf.Min(slider.value, slider.maxValue);
        }
    }
}
