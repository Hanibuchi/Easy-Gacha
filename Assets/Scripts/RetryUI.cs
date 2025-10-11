using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RetryUI : MonoBehaviour
{
    public Button retryButton;

    private void Start()
    {
        retryButton.onClick.AddListener(OnRetryButtonClicked);
    }

    void OnRetryButtonClicked()
    {
        GameManager.Instance.Restart();
        Destroy(gameObject);
    }
}
