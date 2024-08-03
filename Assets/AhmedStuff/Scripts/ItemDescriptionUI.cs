using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDescriptionUI : MonoBehaviour
{
    public GameObject descriptionPanel;
    public TextMeshProUGUI descriptionText;

    private void Start()
    {
        Image image = descriptionPanel.GetComponent<Image>();
        image.raycastTarget = false;
        descriptionText.raycastTarget = false;

    }

    public void ShowDescription(string description)
    {
      descriptionText.text = description;
      descriptionPanel.SetActive(true);
    }
  
    public void HideDescription()
    {
      descriptionPanel.SetActive(false);
    }

    public void UpdatePosition(Vector3 position)
    {
        descriptionPanel.transform.position = position;
    }
}
