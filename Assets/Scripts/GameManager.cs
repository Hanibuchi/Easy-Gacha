using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Min(1)]
    [SerializeField] long mean = 50;
    System.Random rand;
    public static GameManager Instance;
    long bestScore;
    const string BEST_SCORE_KEY = "BestScore";
    void Awake()
    {
        rand = new System.Random();
        // シングルトンのインスタンス設定

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
    }

    ScoreUI scoreUI;
    public void GetScoreUI(ScoreUI scoreUI)
    {
        this.scoreUI = scoreUI;
    }

    // --- インスペクタから設定するパラメータ ---
    [Header("Game Parameters")]
    [Tooltip("演出の分岐に使われるしきい値1")]
    public long scoreThreshold = 200;
    public float cameraDuration = 1.0f; // ルーレット演出の継続時間（秒）
    public float clackerDuration = 3f; // クラッカー演出の継続時間（秒）
    // Scriptのメンバー変数として追加
    public ParticleSystem clackerParticle;
    public GameObject resultScreenPrefab;
    public GameObject restartUIPrefab;

    (bool, string) CheckAchievement(long score)
    {
        // scoreがintの範囲内であることを前提とし、intにキャストしてマネージャーに渡す
        int scoreInt = (int)score;

        // AchievementsManagerを呼び出して実績解除を試みる
        Achievement newlyUnlocked = AchievementsManager.Instance.TryUnlockAchievement(scoreInt);

        // 新しく解除された実績があるかチェック
        if (newlyUnlocked != null)
        {
            return (true, newlyUnlocked.comment);
        }

        // 解除された実績がない場合
        return (false, "");
    }

    public void StartRoulette()
    {
        StartCoroutine(StartRouletteRoutine());
    }

    private IEnumerator StartRouletteRoutine()
    {
        var score = GenerateDiscreteExponential();

        bool isBest = score > bestScore;
        string comment = "";

        // 4. ベストスコア更新とPlayerPrefs保存
        if (isBest)
        {
            bestScore = score;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, (int)bestScore);
            PlayerPrefs.Save();
            comment = "新記録おめでとう！";
            Debug.Log("🎉 New Best Score! 🎉");
        }
        (bool isAchievement, string achievementComment) = CheckAchievement(score); // 実績解除判定
        if (isAchievement)
            comment = achievementComment;

        // 1. ルーレット演出の開始
        // まずはUIでルーレット（数字が回っているような演出）を開始
        if (scoreUI == null)
        {
            Debug.LogError("ScoreUI is not assigned.");
            yield break;
        }
        scoreUI.StartRoulette(score, isBest);

        // 2. 一定時間の待機
        yield return new WaitForSeconds(cameraDuration);

        // 3. スコア判定とカメラ演出
        if (score >= scoreThreshold || isAchievement)
        {
            CameraManager.Instance.ZoomInCamera();
            // カメラ演出の時間だけ待つ（演出時間に応じて調整）
            yield return new WaitForSeconds(CameraManager.Instance.moveDuration);
        }

        // 5. クラッカー演出の判定と実行
        bool shouldClack = isAchievement || (isBest && score >= scoreThreshold);
        if (shouldClack)
        {
            // クラッカーを鳴らす（エフェクトを生成）
            if (clackerParticle != null)
            {
                clackerParticle.Play();
                Debug.Log("クラッカーが鳴った！🎊");
            }

            // クラッカー演出が終了するまで待機
            yield return new WaitForSeconds(clackerDuration);

            // 6. 結果画面の生成
            if (resultScreenPrefab != null)
            {
                Instantiate(resultScreenPrefab).GetComponent<ResultUI>().DisplayResult(score, CalcRarity(score).ToString(), comment, isBest, isAchievement);
                scoreUI.Init();
            }
        }
        else
        {
            Instantiate(restartUIPrefab);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public long GenerateDiscreteExponential()
    {
        // 1. 0.0 (排他的) から 1.0 (包括的) の間の均一な乱数Uを生成
        // Random.valueは0.0~1.0なので、0を避けるためMathf.Maxで下限を設定
        double tmp = Math.Max(rand.NextDouble(), double.Epsilon);

        // 2. 連続な指数分布乱数Xを生成
        // X = -mean * ln(U)
        double x = mean * -Math.Log(tmp);

        // 3. 切り上げて整数に変換（Ceiling）
        long result = (long)x + 1;

        // 生成される最小値は 1 となります。
        return result;
    }

    public long CalcRarity(long x)
    {
        return (long)Math.Exp((double)x / mean);
    }

    public GameObject achievementsUIPrefab;
    public GameObject rankingUIPrefab;
    public void OpenAchievementsUI()
    {
        Instantiate(achievementsUIPrefab);
    }
    public void OpenRankingsUI()
    {
        Instantiate(rankingUIPrefab);
    }
}
