using UnityEngine;
using System.Collections.Generic;

public class AchievementsManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static AchievementsManager Instance { get; private set; }

    // インスペクタから設定する実績データの配列
    public List<Achievement> achievements = new List<Achievement>();

    // PlayerPrefsに保存するキーのプレフィックス
    private const string UNLOCK_KEY_PREFIX = "Achievement_Unlocked_";

    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ゲーム開始時に実績解除状況を読み込む
            LoadAchievementsStatus();
        }
        else
        {
            // 既に存在する場合は自身を破棄
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// PlayerPrefsから各実績の解除状況を読み込みます。
    /// </summary>
    private void LoadAchievementsStatus()
    {
        foreach (var achievement in achievements)
        {
            // "Achievement_Unlocked_ACH_001" のようなキーで検索
            string key = UNLOCK_KEY_PREFIX + achievement.id.ToString();

            // 0=未解除, 1=解除済み として保存
            int status = PlayerPrefs.GetInt(key, 0); // 初期値は0 (未解除)
            achievement.isUnlocked = status == 1;
        }
        Debug.Log("実績解除状況をPlayerPrefsから読み込みました。");
    }

    /// <summary>
    /// 指定されたスコアに基づいて実績の解除を試みます。
    /// </summary>
    /// <param name="score">達成したスコア（例: ガチャで出たランダムな数字）</param>
    public Achievement TryUnlockAchievement(long score)
    {

        // 全ての実績をチェック
        for (int i = 0; i < achievements.Count; i++)
        {
            var achievement = achievements[i];
            // (1) 既に解除されていないか
            if (achievement.isUnlocked)
            {
                continue;
            }

            // (2) スコアが解除条件を満たしているか (最小値 <= スコア <= 最大値)
            if (score >= achievement.minScore && score <= achievement.maxScore)
            {
                // 実績を解除
                achievement.isUnlocked = true;
                // 解除状況をPlayerPrefsに保存
                SaveAchievementStatus(achievement);

                Debug.Log($"実績解除: {achievement.displayName} (スコア: {score})");
                return achievement;
            }
        }

        return null;
    }

    /// <summary>
    /// 特定の実績の解除状況をPlayerPrefsに保存します。
    /// </summary>
    /// <param name="achievement">保存する実績オブジェクト</param>
    private void SaveAchievementStatus(Achievement achievement)
    {
        string key = UNLOCK_KEY_PREFIX + achievement.id.ToString();
        // 解除済みなら 1 を保存
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save(); // 忘れずに保存
    }
}

[System.Serializable]
public class Achievement
{
    // インスペクタから設定する情報
    public AchievementID id;              // 実績のユニークな識別子 (Enum)
    public string displayName = "ビギナーズラック"; // 実績の表示名
    public int minScore = 1;              // 解除に必要なスコアの最小値
    public int maxScore = 100;            // 解除に必要なスコアの最大値 (この範囲内のスコアで解除)
    [TextArea]
    public string comment = "初めての実績解除！おめでとう！"; // 解除時のコメント

    // ゲーム中に管理する情報
    public bool isUnlocked = false;       // 解除されたかどうか
}
public enum AchievementID
{
    NONE, // 無効な状態を示すためによく使われます
    ACH1,
    ACH2,
    ACH3,
    ACH4,
    ACH5,
    ACH6,
    ACH7,
    ACH8,
    ACH9,
    ACH10,
    ACH11,
    ACH12,
    ACH13,
    ACH14,
    ACH15,
    ACH16,
}