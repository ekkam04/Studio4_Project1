using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItemSelectionUI : MonoBehaviour
{
    public Button[] itemButtons;
    private ItemObject[] items;
    private System.Action<ItemObject> onItemSelected;

    public void Initialize(ItemObject[] items, System.Action<ItemObject> onItemSelected)
    {
        this.items = items;
        this.onItemSelected = onItemSelected;

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (i < items.Length)
            {
                ItemObject item = items[i];
                itemButtons[i].GetComponentInChildren<Text>().text = item.name;
                itemButtons[i].onClick.AddListener(() => SelectItem(item));
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectItem(ItemObject selectedItem)
    {
        onItemSelected?.Invoke(selectedItem);
    }
}