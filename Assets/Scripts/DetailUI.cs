using TMPro;
using UnityEngine;

public class DetailUI : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public void Init(string text)
    {
        textMeshProUGUI.text = text;
    }
}
