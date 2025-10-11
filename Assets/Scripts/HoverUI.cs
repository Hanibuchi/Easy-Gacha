using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProを使用
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.VisualScripting; // PointerEventDataを使用

/// <summary>
/// マウスカーソルを追従し、内容を表示するUIパネル。
/// HoverCanvasシングルトンの子として生成され、画面外に出ないように位置を調整する。
/// </summary>
public class HoverUI : MonoBehaviour
{
    // ----------------------------------------------------
    // ◆ メンバ変数
    // ----------------------------------------------------

    [Header("UI Components")]
    [SerializeField] private TMP_Text contentText; // 表示内容を入れるTMP_Text

    private RectTransform rectTransform;
    private RectTransform parentCanvasRect;

    // ----------------------------------------------------
    // ◆ Unityライフサイクル
    // ----------------------------------------------------

    void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    void Start()
    {
        parentCanvasRect = HoverUICanvas.RectTransform;
        transform.SetParent(parentCanvasRect, false); // 親子設定 (ワールド座標を維持しない)
    }

    void Update()
    {
        // 常に角がマウスの位置に来るように、かつ画面外に出ないように表示方向を調整
        AdjustPositionToMouse();
    }

    // ----------------------------------------------------
    // ◆ パブリックメソッド
    // ----------------------------------------------------

    /// <summary>
    /// UIに表示する文字列を設定します。
    /// </summary>
    /// <param name="content">表示したい文字列</param>
    public void SetContent(string content)
    {
        if (contentText != null)
        {
            contentText.text = content;
        }
    }

    /// <summary>
    /// 自身を削除します。
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    // ----------------------------------------------------
    // ◆ プライベートメソッド (位置調整)
    // ----------------------------------------------------

    /// <summary>
    /// マウスの位置に基づき、UIの位置とアンカーを調整し、画面外に出ないようにします。
    /// PointerEventDataは使用せず、Input.mousePositionを使います。（PointerEnter/Exitで生成された場合はそのデータを使っても良いですが、Updateでの追従のため）
    /// </summary>
    private void AdjustPositionToMouse()
    {
        // マウスのスクリーン座標を取得
        Vector2 localPoint = Pointer.current.position.ReadValue();

        // 1. マウスが右半分にあるか？ (x > 0)
        bool isMouseRight = localPoint.x > Screen.width / 2f;

        // 2. マウスが上半分にあるか？ (y > 0)
        bool isMouseTop = localPoint.y > Screen.height / 2f;

        // ----------------------------------------------------------------------
        // UIの表示位置調整 (アンカーとPivotの調整)
        // ----------------------------------------------------------------------

        float horiPivot;
        // **横方向の調整:**
        if (isMouseRight)
        {
            // マウスが右半分にある => UIはマウスの左側（左下/左上）に表示したい
            // UIの右端をマウス位置に合わせるため、Anchor/PivotのXを **1.0 (右)** に設定
            horiPivot = 1f;
        }
        else
        {
            // マウスが左半分にある => UIはマウスの右側（右下/右上）に表示したい
            // UIの左端をマウス位置に合わせるため、Anchor/PivotのXを **0.0 (左)** に設定
            horiPivot = 0f;
        }

        float vertPivot;
        // **縦方向の調整:**
        if (isMouseTop)
        {
            // マウスが上半分にある => UIはマウスの下側（左下/右下）に表示したい
            // UIの上端をマウス位置に合わせるため、Anchor/PivotのYを **1.0 (上)** に設定
            vertPivot = 1f;
        }
        else
        {
            // マウスが下半分にある => UIはマウスの上側（左上/右上）に表示したい
            // UIの下端をマウス位置に合わせるため、Anchor/PivotのYを **0.0 (下)** に設定
            vertPivot = 0f;
        }
        rectTransform.pivot = new Vector2(horiPivot, vertPivot);

        // Pivot/Anchor を調整したことで、rectTransform.anchoredPosition = localPoint と設定すれば、
        // UIの角（Pivotで指定した角）がマウスの位置に正確に配置されます。

        // 追従位置を設定
        rectTransform.position = localPoint;
    }
}