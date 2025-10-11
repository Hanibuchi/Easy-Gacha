using TMPro;
using UnityEngine;
using UnityEngine.UI;

// RankingManagerで定義した HighScore モデルが必要です。
// public class HighScore : BaseModel { ... }

/// <summary>
/// ランキングリストの1行分の情報を表示するUIコンポーネント。
/// </summary>
public class RankingEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI usernameText;


    /// <summary>
    /// ランキングエントリに情報を設定し、UIに表示します。
    /// </summary>
    /// <param name="rank">順位</param>
    /// <param name="data">HighScoreデータ</param>
    public void SetData(int rank, HighScore data, bool isMyScore = false)
    {
        rankText.text = $"{rank}";
        // スコアはlong型なので、適切な形式で表示します。
        scoreText.text = data.Score.ToString("N0"); // 例: 1,234,567 の形式
        usernameText.text = data.Username;
        if (isMyScore)
        {
            rankText.fontStyle = FontStyles.Underline;
            scoreText.fontStyle = FontStyles.Underline;
            usernameText.fontStyle = FontStyles.Underline;
        }
    }
}