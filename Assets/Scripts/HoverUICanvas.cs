using UnityEngine;

public class HoverUICanvas : MonoBehaviour
{
    public static GameObject Instance;
    void Awake()
    {
        Instance = gameObject;
    }
}
