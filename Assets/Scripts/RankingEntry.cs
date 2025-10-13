using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// RankingManagerで定義した HighScore モデルが必要です。
// public class HighScore : BaseModel { ... }

/// <summary>
/// ランキングリストの1行分の情報を表示するUIコンポーネント。
/// </summary>
public class RankingEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI usernameText;
    string _hoverMessage;

    /// <summary>
    /// ランキングエントリに情報を設定し、UIに表示します。
    /// </summary>
    /// <param name="rank">順位</param>
    /// <param name="data">RankingEntryデータ</param>
    public void SetData(int rank, RankingManager.RankingEntry data, bool isMyScore = false)
    {
        rankText.text = $"{rank}";
        // スコアはlong型なので、適切な形式で表示します。
        scoreText.text = data.score.ToString("N0"); // 例: 1,234,567 の形式
        usernameText.text = data.username;
        _hoverMessage = $"{GameManager.Instance.GetProbabilityStr(data.score)}\n{data.created_at.ToString("yyyy/MM/dd H時")}\n{data.attempt_count.ToString("N0")}回目の挑戦";
        if (isMyScore)
        {
            rankText.fontStyle = FontStyles.Underline;
            scoreText.fontStyle = FontStyles.Underline;
            usernameText.fontStyle = FontStyles.Underline;
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

    HoverUI hoverUI;

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
            hoverUI.gameObject.SetActive(true);
        }
    }
}