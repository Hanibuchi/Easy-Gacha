using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// タイトル画面のオブジェクトをアニメーションさせるコンポーネント。（非UIオブジェクト用）
/// アニメーション完了後、GameManagerに通知してゲーム開始を促します。
/// </summary>
public class TitleUI : MonoBehaviour
{
    // --- インスペクタから設定するパラメータ ---
    [Header("Title Object Animation Settings")]
    [Tooltip("オブジェクトの移動にかける時間（秒）")]
    [SerializeField] float duration = 1.5f;

    [Tooltip("タイトルオブジェクトの移動開始位置 (ワールド座標)")]
    [SerializeField] Vector3 startPosition = new Vector3(0, 5, 0);

    [Tooltip("タイトルオブジェクトの移動終了位置 (ワールド座標)")]
    [SerializeField] Vector3 endPosition = Vector3.zero;

    [Tooltip("移動の進行度を決めるアニメーションカーブ")]
    [SerializeField] AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Transform objectTransform;

    void Awake()
    {
        // Transformコンポーネントを取得
        objectTransform = transform;
    }

    void Start()
    {
        // 初期位置に設定
        objectTransform.position = startPosition;

        // アニメーションを開始
        StartCoroutine(AnimateTitleRoutine(OnAnimationComplete));
    }

    /// <summary>
    /// タイトルオブジェクトの移動アニメーションを実行するコルーチン。
    /// </summary>
    private IEnumerator AnimateTitleRoutine(Action onComplete)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 経過時間に基づいて0から1の間の進行度（t）を計算
            float t = elapsedTime / duration;

            // AnimationCurveの値を適用して、移動の補間値（curveValue）を決定
            float curveValue = movementCurve.Evaluate(t);

            // 開始位置と終了位置の間を補間値に基づいて移動
            objectTransform.position = Vector3.Lerp(startPosition, endPosition, curveValue);

            // 経過時間を更新
            elapsedTime += Time.deltaTime;

            yield return null; // 1フレーム待機
        }

        // 終了位置を確定
        objectTransform.position = endPosition;

        // アニメーション完了時の処理を実行
        onComplete?.Invoke();
    }

    public event Action OnAnimationComplete;
}