using UnityEngine;
using TMPro; // TextMeshProを使用
using System.Collections; // コルーチンのために必要
using System;
using UnityEngine.UI; // Funcデリゲートのために必要

public class ResultUI : MonoBehaviour
{
    // --- インスペクターで設定するUI要素 ---
    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI commentText;

    // --- 表示演出のタイミング設定 ---
    [Header("Display Timing")]
    [Tooltip("スコア表示までの遅延時間")]
    public float initialDelay = 0.5f;
    [Tooltip("レア度表示までの遅延時間 (スコア表示後)")]
    public float rarityDelay = 1.0f;
    [Tooltip("コメント表示までの遅延時間 (レア度表示後)")]
    public float commentDelay = 1.5f;


    // --- 外部サービス/ゲーム制御へのアクション ---
    // ボタンが押されたときに実行する処理を、外部（例: GameController）から設定できるようにする
    [Header("Buttons")]
    public Button retryButton;
    public Button rankingButton;
    public Button achievementButton;


    void Awake()
    {
        // ★★★ ボタンイベントの自動登録 ★★★
        if (newRecordObject != null)
        {
            newRecordObject.SetActive(false);
        }
        if (achievementObject != null)
        {
            achievementObject.SetActive(false);
        }

        // 1. リトライボタン
        if (retryButton != null)
        {
            // 既存のリスナーをクリアしてから登録する（二重登録を防ぐため）
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnClickRetry);
            Debug.Log("リトライボタンに OnClickRetry を登録しました。");
        }
        else
        {
            Debug.LogError("Retry Button is not assigned in the Inspector.");
        }

        // 2. ランキングボタン
        if (rankingButton != null)
        {
            rankingButton.onClick.RemoveAllListeners();
            rankingButton.onClick.AddListener(OnClickRanking);
            Debug.Log("ランキングボタンに OnClickRanking を登録しました。");
        }
        else
        {
            Debug.LogError("Ranking Button is not assigned in the Inspector.");
        }

        // 3. 実績ボタン
        if (achievementButton != null)
        {
            achievementButton.onClick.RemoveAllListeners();
            achievementButton.onClick.AddListener(OnClickAchievement);
            Debug.Log("実績ボタンに OnClickAchievement を登録しました。");
        }
        else
        {
            Debug.LogError("Achievement Button is not assigned in the Inspector.");
        }
    }

    [Header("Conditional UI")]
    [Tooltip("新記録の場合にActiveにするGameObject")]
    public GameObject newRecordObject;
    [Tooltip("実績解除の場合にActiveにするGameObject")]
    public GameObject achievementObject;
    public void Start()
    {
        // 演出コルーチンを開始
        StartCoroutine(AnimateResultDisplay(currentScore, currentRarity, currentComment, isBest, isAchievement));
    }


    // --- 内部状態とデータ ---
    private long currentScore;
    private string currentRarity;
    private string currentComment;
    private bool isBest;
    bool isAchievement;
    /// <summary>
    /// ルーレット結果のデータを受け取り、表示演出を開始する
    /// </summary>
    /// <param name="score">獲得したスコア</param>
    /// <param name="rarity">スコアのレア度を示す文字列 (例: "N", "SSR", "1/1000")</param>
    public void DisplayResult(long score, string rarity, string comment, bool isBest = false, bool isAchievement = false)
    {
        currentScore = score;
        currentRarity = rarity;
        currentComment = comment;
        this.isBest = isBest;
        this.isAchievement = isAchievement;
    }

    /// <summary>
    /// スコア、レア度、コメントを時間差で表示する演出コルーチン
    /// </summary>
    public IEnumerator AnimateResultDisplay(long score, string rarity, string comment, bool isBest = false, bool isAchievement = false)
    {
        isInteractionAllowed = false;
        // 1. 初期遅延
        yield return new WaitForSeconds(initialDelay);

        // 2. スコア表示
        scoreText.text = $"{score}";
        yield return new WaitForSeconds(rarityDelay);

        // 3. レア度表示
        rarityText.text = $"{rarity}";
        yield return new WaitForSeconds(commentDelay);



        if (isBest && newRecordObject != null)
        {
            newRecordObject.SetActive(true);
            Debug.Log("新記録オブジェクトをアクティブにしました。");
        }

        // 実績解除の場合は対応するGameObjectをアクティブにする
        if (isAchievement && achievementObject != null)
        {
            achievementObject.SetActive(true);
            Debug.Log("実績解除オブジェクトをアクティブにしました。");
        }
        

        // 4. コメント表示
        commentText.text = comment;
        isInteractionAllowed = true;
        Debug.Log("結果表示アニメーション完了。");
    }

    // --- ボタンが押されたときに実行されるメソッド ---

    private bool isInteractionAllowed = false;
    public void OnClickRetry()
    {
        if (isInteractionAllowed)
        {
            Destroy(gameObject);
            GameManager.Instance.Restart();
        }
    }

    public void OnClickRanking()
    {
        if (isInteractionAllowed)
        {
            GameManager.Instance.OpenRankingsUI();
        }
    }

    public void OnClickAchievement()
    {
        if (isInteractionAllowed)
            GameManager.Instance.OpenAchievementsUI();
    }

    public long test_score = 100;
    public string test_comment = "ベストスコアおめでとう！";

    public void Test()
    {
        DisplayResult(test_score, GameManager.Instance.CalcRarity(test_score).ToString(), test_comment);
    }
}