using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq; // OrderByDescendingを使うために必要
using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

public class RankingManager : MonoBehaviour
{
    // --- 1. シングルトンパターン ---
    public static RankingManager Instance { get; private set; }

    // --- 2. Supabase設定 ---
    [Header("Supabase Settings")]
    [SerializeField] private string supabaseUrl = "YOUR_SUPABASE_URL";
    [SerializeField] private string supabaseAnonKey = "YOUR_SUPABASE_ANON_KEY";

    private Supabase.Client supabase;

    private void Awake()
    {
        // シングルトンのインスタンス設定
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでも破棄されないようにする（必要に応じて）
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Supabaseクライアントの初期化
        try
        {
            supabase = new Supabase.Client(supabaseUrl, supabaseAnonKey);
            Debug.Log("Supabase Client Initialized.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Supabase Initialization Error: {e.Message}");
        }
    }

    // --- 3. ランキング情報取得関数 ---
    /// <summary>
    /// Supabaseからランキング情報を取得します。
    /// </summary>
    /// <param name="limit">取得する件数</param>
    /// <returns>HighScoreオブジェクトのリスト（降順）</returns>
    public async Task<List<HighScore>> GetRankingAsync(int limit = 10)
    {
        if (supabase == null)
        {
            Debug.LogError("Supabase is not initialized.");
            return new List<HighScore>();
        }

        try
        {
            // high_scoresテーブルからスコアが高い順に指定件数を取得
            var response = await supabase
                .From<HighScore>()
                .Order(x => x.Score, Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(limit)
                .Get();

            // 取得したデータをList<HighScore>として返す
            return response.Models;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to fetch ranking: {e.Message}");
            return new List<HighScore>();
        }
    }
    public async Task<HighScore> GetMyScoreAsync(string clientToken)
    {
        if (supabase == null)
        {
            Debug.Log("Supabase is null");
            return new();
        }

        try
        {
            var response = await supabase
            .From<HighScore>()
            .Filter("client_token", Supabase.Postgrest.Constants.Operator.Equals, clientToken)
            .Limit(3)
            .Get();

            return response.Models.FirstOrDefault();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to fetch ranking: {e.Message}");
            return new();
        }
    }

    // --- 4. スコア登録/更新関数 ---
    /// <summary>
    /// スコアをSupabaseに登録または更新します。
    /// client_tokenとusernameが一致するデータがあれば更新し、なければ新規登録します。
    /// </summary>
    /// <param name="score">登録するスコア</param>
    /// <param name="username">プレイヤー名</param>
    /// <param name="clientToken">プレイヤー識別用トークン</param>
    /// <returns>成功した場合はtrue</returns>
    public async Task<bool> SubmitScoreAsync(long score, string username, string clientToken)
    {
        if (supabase == null)
        {
            Debug.LogError("Supabase is not initialized.");
            return false;
        }

        // client_tokenを識別子として既存のデータを検索
        var existingScores = await supabase
            .From<HighScore>()
            .Filter("client_token", Supabase.Postgrest.Constants.Operator.Equals, clientToken)
            .Get();

        // 既存のデータが見つかったか確認
        var existingEntry = existingScores.Models.FirstOrDefault();

        if (existingEntry != null)
        {
            // 既存データが見つかった場合、**スコアがより高ければ**更新する
            if (score > existingEntry.Score)
            {
                existingEntry.Score = score;
                existingEntry.Username = username; // 名前も更新できるようにする
                existingEntry.CreatedAt = DateTime.UtcNow;
                // Update
                var response = await existingEntry.Update<HighScore>();
                Debug.Log($"Score updated for client_token: {clientToken}. New Score: {score}");
                return response != null;
            }
            else
            {
                Debug.Log($"Existing score is higher or equal. No update needed. Current Score: {existingEntry.Score}");
                return true; // 更新はしなかったが、処理としては成功
            }
        }
        else
        {
            // 既存データが見つからなかった場合、新規登録
            var newScore = new HighScore
            {
                ClientToken = clientToken,
                Username = username,
                Score = score,
                CreatedAt = DateTime.UtcNow,
            };

            // Insert
            var response = await supabase
                .From<HighScore>()
                .Insert(newScore);

            Debug.Log($"New score submitted for client_token: {clientToken}. Score: {score}");
            return response.Models.Count > 0;
        }
    }

    // public long score;
    // public string username;
    // public string chientToken;
    // public async void Test()
    // {
    //     if (chientToken == "")
    //         chientToken = Guid.NewGuid().ToString();
    //     var result = await SubmitScoreAsync(score, username, chientToken);
    //     Debug.Log($"result: {result}");
    // }

    // public async void Test2()
    // {
    //     var result = await GetRankingAsync(10);
    //     for (int i = 0; i < result.Count; i++)
    //     {
    //         Debug.Log($"rank: {i}, score: {result[i].Score}, name: {result[i].Username}, chientToken: {result[i].ClientToken}");
    //     }
    // }
    // public async void Test3()
    // {
    //     var result = await GetMyScoreAsync(chientToken);
    //     Debug.Log($"score: {result.Score}, name: {result.Username}, chientToken: {result.ClientToken}");
    // }
}


// Supabaseのテーブル構造と対応させる
[Table("ScoreRanking")] // Supabaseのテーブル名
public class HighScore : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; }

    [Column("client_token")]
    public string ClientToken { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("score")]
    public long Score { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}