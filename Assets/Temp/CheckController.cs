using System;
using UnityEngine;
using UnityEngine.UI;

public class CheckController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Checkmark button")]
    Toggle m_checkmarkToggle;
    [SerializeField]
    [Tooltip("Checkmark image")]
    Image m_checkmarkImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(m_checkmarkToggle != null)
        {
            m_checkmarkToggle.onValueChanged.AddListener(ToggleCheck);
        }
    }

    void ToggleCheck(bool isOn)
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_checkmarkToggle.isOn = !m_checkmarkToggle.isOn;
        }
    }
}
