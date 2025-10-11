using UnityEngine;

public class HoverUICanvas : MonoBehaviour
{
    public static RectTransform RectTransform;
    void Awake()
    {
        RectTransform = (RectTransform)transform;
    }
}
