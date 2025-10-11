using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;

public class AchievementEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    // === インスペクタで設定するUI要素 ===
    [Header("UI References")]
    [SerializeField] private GameObject unlockedIcon; // 解除済み時に表示するチェックマークなどのGameObject
    [SerializeField] private TextMeshProUGUI displayNameText; // 実績名を表示するText
    [SerializeField] GameObject detailUI;
    HoverUI hoverUI;

    // === 内部データ ===
    private string _hoverMessage = "実績の詳細情報がここに表示されます。";

    // PopUpを生成・設定する関数 (AchievementsManagerから情報を渡すために使用)
    public void Initialize(Achievement achievementData)
    {
        // データを内部に保持し、UIに反映
        _hoverMessage = $"{achievementData.displayName}\n<size=80%>{achievementData.comment}</size>";

        // 達成度合いをUIに反映
        bool isUnlocked = achievementData.isUnlocked;

        unlockedIcon.SetActive(isUnlocked);

        // 未達成の場合は名前をグレーアウトするなど、視覚的なフィードバックを追加できます
        if (isUnlocked)
        {
            displayNameText.text = achievementData.displayName;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OpenHoverUI();
    }

    // === IPointerEnterHandler (ホバー開始時) ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        // (1) ホバーメッセージを生成・表示
        if (Pointer.current.press.isPressed)
        {
            OpenHoverUI();
        }
    }

    // === IPointerExitHandler (ホバー終了時) ===
    public void OnPointerExit(PointerEventData eventData)
    {
        OpenHoverUI(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OpenHoverUI(false);
    }

    void OpenHoverUI(bool open = true)
    {
        if (hoverUI != null)
        {
            hoverUI.DestroySelf();
            hoverUI = null;
        }
        if (open)
        {
            hoverUI = Instantiate(GameManager.Instance.HoverUIPrefab).GetComponent<HoverUI>();
            hoverUI.SetMessage(_hoverMessage);
        }
    }
}