using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    void OnStartButtonClicked()
    {
        GameManager.Instance.StartRoulette();
        Destroy(gameObject);
    }
}
