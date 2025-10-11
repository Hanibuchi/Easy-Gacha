using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // TextMeshProを使うために必要

// Unityのコンポーネントとして機能させるため、MonoBehaviourを継承
public class ScoreUI : MonoBehaviour
{
    // --- インスペクタから設定するUI要素 ---
    [Header("UI Components")]
    [Tooltip("スコアの数字を表示するテキスト")]
    public TextMeshPro ScoreText;
    [Tooltip("最初に表示されるコメント用テキスト")]
    public TextMeshPro CommentText;
    [Tooltip("時間差で表示されるコメント用テキスト")]
    public TextMeshPro RarityText;
    public GameObject BestScoreText;



    [Header("Audio Components")]
    public AudioSource audioSource;
    [Tooltip("ドラムロール開始時の単発音")]
    public AudioClip drumRollStartClip;
    [Tooltip("ドラムロール中のループ音")]
    public AudioClip drumRollLoopClip;
    [Tooltip("結果が表示されるときの音")]
    public AudioClip resultRevealClip;

    public int randScoreTextSize = 48;
    public int scoreTextSize = 64;

    // --- 内部状態変数 ---
    private long _finalScore; // 最終的に表示するスコア
    private bool _isRouletteRunning = false;
    public float dorumRollDuration = 2.0f; // ドラムロールの基本の長さ（秒）
    public float minDrumRollDuration = 1.0f; // 最短ドラムロール時間

    public void Init()
    {
        // UIを初期状態に戻す
        CommentText.text = "";
        RarityText.text = "";
        ScoreText.text = "";
        BestScoreText.SetActive(false);
    }

    // --- エントリポイント ---
    /// <summary>
    /// ルーレット演出を開始し、最終スコアを決定します。
    /// </summary>
    /// <param name="score">最終的に表示するスコア</param>
    public void StartRoulette(long score, bool isBest = false, Action callback = null)
    {
        if (_isRouletteRunning) return; // 既に実行中なら無視

        _finalScore = score;
        _isRouletteRunning = true;

        Init();

        // スコアの大きさに基づき、ドラムロールの長さを決定
        // スコアが大きいほど、期待感を持たせるために長くする（最大 dorumRollDuration）
        float duration = Mathf.Lerp(minDrumRollDuration, dorumRollDuration, (float)score / 50.0f);

        // ドラムロールのコルーチンを開始
        StartCoroutine(DrumRollCoroutine(duration, isBest, callback));
    }

    public float timeBetweenResultAndComment = 0.5f;

    // --- ドラムロール演出 ---
    private System.Collections.IEnumerator DrumRollCoroutine(float totalDuration, bool isBest = false, Action callback = null)
    {
        // 1. **開始音の再生**
        if (drumRollStartClip != null)
        {
            audioSource.PlayOneShot(drumRollStartClip);
            // 開始音の長さだけ待つ
            yield return new WaitForSeconds(drumRollStartClip.length);
        }

        // 2. **ループ音の再生**
        if (drumRollLoopClip != null)
        {
            // 残りの演出時間を計算 (開始音の再生時間を差し引く)
            float remainingDuration = totalDuration;
            if (drumRollStartClip != null)
            {
                remainingDuration = totalDuration - drumRollStartClip.length;
            }
            // 負の値にならないように調整
            if (remainingDuration < 0) remainingDuration = 0;

            // ループ音を再生開始し、ループ設定をONにする
            audioSource.clip = drumRollLoopClip;
            audioSource.loop = true;
            audioSource.Play();
            ScoreText.fontSize = randScoreTextSize;

            // ドラムロールのメイン処理
            float startTime = Time.time;
            float elapsed = 0f;

            while (elapsed < remainingDuration)
            {
                elapsed = Time.time - startTime;

                long displayScore = GameManager.Instance.GenerateDiscreteExponential();
                ScoreText.text = displayScore.ToString();

                // ドラムロールの速さを徐々に落とす
                float waitTime = Mathf.Lerp(0.05f, 0.15f, elapsed / remainingDuration);
                yield return new WaitForSeconds(waitTime);
            }

            // ループ音の再生を停止し、ループ設定を解除
            audioSource.Stop();
            audioSource.loop = false;
        }

        // 3. **結果表示音の再生**
        ScoreText.fontSize = scoreTextSize;
        ScoreText.text = _finalScore.ToString();

        if (resultRevealClip != null)
        {
            audioSource.PlayOneShot(resultRevealClip);
        }

        yield return new WaitForSeconds(timeBetweenResultAndComment); // 時差
        CommentText.text = "この数字が出るのは大体...";

        yield return new WaitForSeconds(timeBetweenResultAndComment); // 時差
        RarityText.text = $"{GameManager.Instance.CalcRarity(_finalScore)}回に1回";

        if (isBest)
        {
            yield return new WaitForSeconds(timeBetweenResultAndComment); // 時差
            BestScoreText.SetActive(true);
        }

        _isRouletteRunning = false;
        callback?.Invoke();
    }

    void Start()
    {
        GameManager.Instance.GetScoreUI(this);
    }
}