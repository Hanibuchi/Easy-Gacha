using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq; // OrderByDescendingを使うために必要
using System;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using unityroom.Api;
using UnityEngine.Networking;
using TMPro;

public class RankingManager : MonoBehaviour
{
    // --- 1. シングルトンパターン ---
    public static RankingManager Instance { get; private set; }

    // --- 2. Supabase設定 ---
    [Header("Supabase Settings")]
    [SerializeField] private string supabaseUrl = "YOUR_SUPABASE_URL";
    [SerializeField] private string supabaseAnonKey = "YOUR_SUPABASE_ANON_KEY";

    private Supabase.Client supabase;
    public string Username => username;
    string username;
    const string USERNAME_KEY = "username";
    [SerializeField] string defaultUsername = "通りすがりの挑戦者";

    public string ClientToken => _clientToken;
    string _clientToken;
    const string CLIENT_TOKEN_KEY = "clientToken";

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

        username = PlayerPrefs.GetString(USERNAME_KEY, defaultUsername);
        PlayerPrefs.SetString(USERNAME_KEY, username);

        _clientToken = PlayerPrefs.GetString(CLIENT_TOKEN_KEY, Guid.NewGuid().ToString());
        PlayerPrefs.SetString(CLIENT_TOKEN_KEY, _clientToken);
        PlayerPrefs.Save();
    }

    public async void SubmitBestScore(long score)
    {
        var rankingEntry = await GetUserScoreAsync2(_clientToken);
        if (rankingEntry != null)
        {
            rankingEntry.score = score;
            rankingEntry.attempt_count = GameManager.Instance.AttemptCount;
            rankingEntry.created_at = DateTime.UtcNow;
            var result = await UpdateScoreAsync(rankingEntry);
            if (!result)
                Debug.LogWarning("Failed to submit score.");
        }
        else
        {
            var result = await SubmitScoreAsync2(score, username, _clientToken, GameManager.Instance.AttemptCount);
            if (!result)
                Debug.LogWarning("Failed to submit score.");
        }
    }

    public async Task ChangeUserName(string newUsername)
    {
        username = newUsername;
        PlayerPrefs.SetString(USERNAME_KEY, username);
        PlayerPrefs.Save();

        var mydata = await GetUserScoreAsync2(_clientToken);
        if (mydata != null)
        {
            mydata.username = newUsername;
            var result = await UpdateScoreAsync(mydata);
            if (!result)
                Debug.LogWarning("Failed to submit score.");
        }
    }

    [Serializable]
    class RankingEntry_
    {
        public string id;

        public string client_token;

        public string username;

        public long score;

        public string created_at;
        public long attempt_count;
        public RankingEntry ToRankingEntry()
        {
            return new RankingEntry
            {
                id = id,
                client_token = client_token,
                username = username,
                score = score,
                created_at = DateTime.Parse(created_at),
                attempt_count = attempt_count
            };
        }
    }
    [Serializable]
    public class RankingEntry
    {
        public string id;

        public string client_token;

        public string username;

        public long score;

        public DateTime created_at;
        public long attempt_count;
    }
    [Serializable]
    class RankingsList_
    {
        public List<RankingEntry_> entries;
    }

    public async Task<List<RankingEntry>> GetRankingAsync(int limit = 10)
    {
        string requestUrl = $"{scoreTableUrl}?select=*&order=score.desc&limit={limit}";
        using UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            Debug.Log($"サーバーからの応答: {jsonResponse}");

            string wrappedJson = $"{{\"entries\":{jsonResponse}}}";
            RankingsList_ list = JsonUtility.FromJson<RankingsList_>(wrappedJson);

            if (list == null || list.entries == null)
            {
                Debug.LogWarning("JSONのパースに失敗したか、ランキングデータが空です。");
                return new();
            }

            return list.entries.Select(entry => entry.ToRankingEntry()).ToList();
        }
        else
        {
            Debug.LogError($"エラー: {request.error}\n詳細: {request.downloadHandler.text}");
            return new();
        }
    }


    public async Task<HighScore> GetUserScoreAsync(string clientToken)
    {
        if (supabase == null)
        {
            Debug.Log("Supabase is null");
            return null;
        }

        try
        {
            var response = await supabase
            .From<HighScore>()
            .Filter("client_token", Supabase.Postgrest.Constants.Operator.Equals, clientToken)
            .Limit(3)
            .Get();

            if (response.Models.Count < 0)
                return null;
            return response.Models.FirstOrDefault();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to fetch ranking: {e.Message}");
            return null;
        }
    }

    public Task<RankingEntry> GetMyScoreAsync()
    {
        return GetUserScoreAsync2(_clientToken);
    }

    async Task<RankingEntry> GetUserScoreAsync2(string clientToken)
    {
        string requestUrl = $"{scoreTableUrl}?select=*&client_token=eq.{clientToken}&limit=1";

        using UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var jsonResponse = request.downloadHandler.text;

            if (string.IsNullOrEmpty(jsonResponse) || jsonResponse == "[]")
            {
                Debug.Log($"指定されたClientTokenのデータは見つかりませんでした: {clientToken}");
                return null;
            }

            string wrappedJson = $"{{\"entries\":{jsonResponse}}}";
            RankingsList_ list = JsonUtility.FromJson<RankingsList_>(wrappedJson);

            if (list != null && list.entries != null && list.entries.Count > 0)
            {
                return list.entries[0].ToRankingEntry();
            }
            else
            {
                Debug.LogWarning("データが見つかりましたが、JSONのパースに失敗しました。");
                return null;
            }
        }
        else
        {
            Debug.LogError($"ユーザースコアの取得エラー: {request.error}\n詳細: {request.downloadHandler.text}");
            return null;
        }
    }

    [System.Serializable]
    private class CountResponse
    {
        public long count;
    }

    [System.Serializable]
    private class CountListWrapper
    {
        public List<CountResponse> entries;
    }
    public async Task<int> GetMyRankAsync()
    {
        var bestScore = GameManager.Instance.BestScore;
        string requestUrl = $"{scoreTableUrl}?score=gt.{bestScore}&select=count";

        using UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;

            string wrappedJson = $"{{\"entries\":{jsonResponse}}}";
            CountListWrapper parsedList = JsonUtility.FromJson<CountListWrapper>(wrappedJson);
            if (parsedList != null && parsedList.entries != null && parsedList.entries.Count > 0)
            {
                long higherCount = parsedList.entries[0].count;
                Debug.Log($"自分よりスコアが高い人は {higherCount} 人いました。");

                return (int)higherCount + 1;
            }
            else
            {
                Debug.LogError("カウントのパースに失敗しました。");
                return -1;
            }
        }
        else
        {
            Debug.LogError($"順位計算エラー: {request.error}\n詳細: {request.downloadHandler.text}");
            return -1;
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
        UnityroomApiClient.Instance.SendScore(1, score, ScoreboardWriteMode.HighScoreDesc);
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
            if (score >= existingEntry.Score)
            {
                existingEntry.Score = score;
                existingEntry.Username = username; // 名前も更新できるようにする
                existingEntry.CreatedAt = DateTime.UtcNow;
                existingEntry.AttemptCount = GameManager.Instance.AttemptCount;
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
                AttemptCount = GameManager.Instance.AttemptCount,
            };

            // Insert
            var response = await supabase
                .From<HighScore>()
                .Insert(newScore);

            Debug.Log($"New score submitted for client_token: {clientToken}. Score: {score}");
            return response.Models.Count > 0;
        }
    }

    string scoreTableName = "ScoreRanking";
    string scoreTableUrl => $"{supabaseUrl}/rest/v1/{scoreTableName}";


    [Serializable]
    private class SubmitPayload
    {
        public string client_token;
        public string username;
        public long score;
        public long attempt_count;
    }
    async Task<bool> SubmitScoreAsync2(long score, string username, string clientToken, long attemptCount)
    {
        SubmitPayload payload = new()
        {
            client_token = clientToken,
            username = username,
            score = score,
            attempt_count = attemptCount,
        };
        string json = JsonUtility.ToJson(payload);


        using UnityWebRequest request = new(scoreTableUrl, "POST");
        byte[] bodyRow = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRow);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Prefer", "return=minimal");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return true;
        }
        else
        {
            Debug.LogError($"エラー: {request.error}\n詳細: {request.downloadHandler.text}");
            return false;
        }
    }

    [Serializable]
    private class UpdatePayload
    {
        public string username;
        public long score;
        public string created_at;
        public long attempt_count;
    }
    async Task<bool> UpdateScoreAsync(RankingEntry rankingEntry)
    {
        if (rankingEntry == null || string.IsNullOrEmpty(rankingEntry.client_token))
        {
            Debug.LogError("無効なRankingEntry、またはclient_tokenが空のため更新できません。");
            return false;
        }
        UpdatePayload payload = new()
        {
            username = rankingEntry.username,
            score = rankingEntry.score,
            created_at = rankingEntry.created_at.ToString("o"),
            attempt_count = rankingEntry.attempt_count
        };
        string json = JsonUtility.ToJson(payload);

        string requestUrl = $"{scoreTableUrl}?client_token=eq.{rankingEntry.client_token}";
        using UnityWebRequest request = new(requestUrl, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("apikey", supabaseAnonKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseAnonKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Prefer", "return=minimal");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"スコアの更新に成功しました。 client_token: {rankingEntry.client_token}");
            return true;
        }
        else
        {
            Debug.LogError($"スコアの更新に失敗しました。エラー: {request.error}\n詳細: {request.downloadHandler.text}");
            return false;
        }
    }

    public long test_score;
    public string test_username;
    public long test_attempt_count;
    public string test_clientToken;
    public async void Test()
    {
        var result = await SubmitScoreAsync2(test_score, test_username, _clientToken, test_attempt_count);
        Debug.Log($"result: {result}");
    }
    // [SerializeField] TMP_Text test_text;
    public async void Test2()
    {
        var result = await GetRankingAsync(10);
        for (int i = 0; i < result.Count; i++)
        {
            string str = $"rank: {i}, score: {result[i].score}, name: {result[i].username}, chientToken: {result[i].client_token}, attemptCount: {result[i].attempt_count}";
            Debug.Log(str);
            // test_text.text = str;
        }
    }
    public async void Test3()
    {
        var result = await GetUserScoreAsync2(test_clientToken);
        Debug.Log($"score: {result.score}, name: {result.username}, chientToken: {result.client_token}, attemptCount: {result.attempt_count}, created_at: {result.created_at}");
    }

    public async void Test4()
    {
        var result = await GetUserScoreAsync2(test_clientToken);
        result.attempt_count = test_attempt_count;
        result.username = test_username;
        result.score = test_score;
        bool isSuccess = await UpdateScoreAsync(result);
        Debug.Log($"isSuccess: {isSuccess}");
    }
    public async void Test5()
    {
        var result = await GetMyRankAsync();
        Debug.Log($"result: {result}");
    }
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
    [Column("attempt_count")]
    public long? AttemptCount { get; set; }
}
