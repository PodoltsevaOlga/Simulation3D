using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SimpleSlider : MonoBehaviour
{
    private Slider slider;
    private Text minValueText;
    private Text maxValueText;
    private Text currentValueText;

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        minValueText = slider.transform.Find("Min").GetComponent<Text>();
        maxValueText = slider.transform.Find("Max").GetComponent<Text>();
        currentValueText = slider.transform.Find("Current").GetComponent<Text>();
        minValueText.text = slider.minValue.ToString();
        maxValueText.text = slider.maxValue.ToString();
        currentValueText.text = slider.value.ToString();
    }
    
    public void ChangeCurrentValueText(float value)
    {
        currentValueText.text = ((int)value).ToString();
    }

    public void ChangeMaxValueText(float value)
    {
        maxValueText.text = ((int)value).ToString();
    }
}
