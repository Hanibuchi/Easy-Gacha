using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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
    const string IS_PLAYED_BEFORE_KEY = "IsPlayedBefore";
    const string ATTEMPT_COUNT_KEY = "AttemptCount";
    long attemptCount;
    public long AttemptCount => attemptCount;
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
    public GameObject buttonsUI;
    public GameObject startUIPrefab;
    public GameObject titleUIPrefab;
    GameObject titleUI;


    void Start()
    {
        titleUI = Instantiate(titleUIPrefab);
        titleUI.GetComponent<TitleUI>().OnAnimationComplete += OnTitleAnimationComplete;

        bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
        if (PlayerPrefs.HasKey(IS_PLAYED_BEFORE_KEY))
        {
            OpenButtonsUI();
        }
        PlayerPrefs.SetInt(IS_PLAYED_BEFORE_KEY, 1);
        attemptCount = PlayerPrefs.GetInt(ATTEMPT_COUNT_KEY, 0);
        PlayerPrefs.Save();
    }

    void OnTitleAnimationComplete()
    {
        Instantiate(startUIPrefab);
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
        OpenButtonsUI(false);
        if (titleUI != null)
        {
            Destroy(titleUI);
            titleUI = null;
        }
    }

    private IEnumerator StartRouletteRoutine()
    {
        var score = GenerateDiscreteExponential();
        attemptCount++;
        PlayerPrefs.SetInt(ATTEMPT_COUNT_KEY, (int)attemptCount);
        PlayerPrefs.Save();

        bool isBest = score > bestScore;
        string comment = "";

        // 4. ベストスコア更新とPlayerPrefs保存
        if (isBest)
        {
            bestScore = score;
            PlayerPrefs.SetInt(BEST_SCORE_KEY, (int)bestScore);
            PlayerPrefs.Save();
            RankingManager.Instance.SubmitBestScore(score);
            comment = "新記録おめでとう！";
            Debug.Log("🎉 New Best Score! 🎉");
        }
        (bool isAchievement, string achievementComment) = CheckAchievement(score); // 実績解除判定
        if (isAchievement)
            comment = achievementComment;

        bool isCameraEffect = score >= scoreThreshold || isAchievement;

        // 1. ルーレット演出の開始
        // まずはUIでルーレット（数字が回っているような演出）を開始
        if (scoreUI == null)
        {
            Debug.LogError("ScoreUI is not assigned.");
            yield break;
        }
        scoreUI.StartRoulette(score, isBest, isCameraEffect, () =>
        {
            StartCoroutine(DisplayResult(score, comment, isBest, isAchievement));
        }, isAchievement);
    }

    IEnumerator DisplayResult(long score, string comment, bool isBest, bool isAchievement)
    {
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
                Instantiate(resultScreenPrefab).GetComponent<ResultUI>().DisplayResult(score, GetProbabilityStr(score), comment, isBest, isAchievement);
                scoreUI.Init();
            }
        }
        else
        {
            Instantiate(restartUIPrefab);
            OpenButtonsUI();
        }
    }

    public void Restart()
    {
        scoreUI.Init();
        CameraManager.Instance.ResetCameraPosition(StartRoulette);
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

    public double CalcProbability(long x)
    {
        return Math.Exp(-(double)x / mean);
    }
    public long CalcRarity(long x)
    {
        return (long)Math.Exp((double)x / mean);
    }
    public long CalcRarityPinPoint(long x)
    {
        return (long)((double)1 / (-Math.Exp(-(double)x / mean) + Math.Exp(-(double)(x - 1) / mean)));
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
    public void OpenButtonsUI(bool open = true)
    {
        buttonsUI.SetActive(open);
    }


    public GameObject HoverUIPrefab;

    public string GetProbabilityStr(long score)
    {
        long rarity = Instance.CalcRarity(score);
        if (rarity == 1)
            return $"{Math.Round(Instance.CalcProbability(score) * 100)}%";
        else
            return $"{rarity.ToString("N0")}回に1回";
    }
    public void Tweet(long score, bool isBest = false, long rank = 0, bool isAchievement = false, string comment = "")
    {
        string text = $"🏆 スコア: {score}\n";
        if (isBest)
            text += $"📉 これ以上の数字が出る確率: {GetProbabilityStr(score)}\n" +
            $"🌍 現在のランキング: {rank}位\n";
        if (isAchievement)
            text += $"🌟 実績解除！\n" + $"{comment}\n";
        text += $"\nクリックでガチャを引くだけ！\n" +
        $"あなたはこの確率を超えられるか？いますぐ運試し！\n";

        naichilab.UnityRoomTweet.Tweet("exp_gacha", text, "指数分布ガチャ");
    }
}
