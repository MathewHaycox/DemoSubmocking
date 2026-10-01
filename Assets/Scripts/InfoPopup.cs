using UnityEngine;
using TMPro;

public class InfoPopup : MonoBehaviour
{
    public GameObject panel;

    public TMP_Text objectNameText;
    public TMP_Text objectTypeText;
    public TMP_Text descriptionText;

    public void Show(ObjectInfo info)
    {
        panel.SetActive(true);

        objectNameText.text = "Name: " + info.objectName;
        objectTypeText.text = "Type: " + info.objectType;
        descriptionText.text = "Description: " + info.description;

        objectNameText.ForceMeshUpdate();
        objectTypeText.ForceMeshUpdate();
        descriptionText.ForceMeshUpdate();
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}