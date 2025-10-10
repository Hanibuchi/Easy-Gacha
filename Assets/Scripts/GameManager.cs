using System;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Min(1)]
    [SerializeField] long mean = 50;
    System.Random rand;
    public static GameManager Instance;
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

    }

    ScoreUI scoreUI;
    public void GetScoreUI(ScoreUI scoreUI)
    {
        this.scoreUI = scoreUI;
    }

    public void StartRoulette()
    {
        var score = GenerateDiscreteExponential();
        if (scoreUI == null)
        {
            Debug.LogError("ScoreUI is not assigned.");
            return;
        }
        scoreUI.StartRoulette(score, null);
    }

    
    // --- インスペクタから設定するパラメータ ---
    [Header("Game Parameters")]
    [Tooltip("演出の分岐に使われるしきい値1")]
    public long scoreThreshold = 50;

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
}
