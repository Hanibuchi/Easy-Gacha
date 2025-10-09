using UnityEngine;
using UnityEngine.UI; // Buttonを使用する場合

public class AchievementsMenu : MonoBehaviour
{
    // === インスペクタで設定する項目 ===
    [Header("Entry References")]
    // シーン内のUIに配置されたAchievementEntryの配列
    [SerializeField] private AchievementEntry[] achievementEntries; 

    [Header("Close Button")]
    [SerializeField] private Button closeButton; // 閉じるボタン (インスペクタで設定)

    private void Start()
    {
        // (1) 閉じるボタンのリスナーを設定
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMenu);
        }

        // (2) 実績管理マネージャーからデータを取得
        if (AchievementsManager.Instance == null)
        {
            Debug.LogError("AchievementsManagerのインスタンスが見つかりません。シングルトンが正しく初期化されているか確認してください。");
            return;
        }

        var achievementDataList = AchievementsManager.Instance.achievements;

        // (3) 実績データをAchievementEntryに反映
        UpdateMenu(achievementDataList);
    }

    /// <summary>
    /// 実績データを各エントリーUIに渡して表示を更新します。
    /// </summary>
    /// <param name="dataList">AchievementsManagerから取得した実績データのリスト</param>
    private void UpdateMenu(System.Collections.Generic.List<Achievement> dataList)
    {
        for (int i = 0; i < achievementEntries.Length; i++)
        {
            if (i < dataList.Count)
            {
                // 対応する実績データがあれば初期化
                achievementEntries[i].Initialize(dataList[i]);
                achievementEntries[i].gameObject.SetActive(true); // UIを有効にする
            }
            else
            {
                // 実績データが足りない場合は、余分なUIを非表示にする
                achievementEntries[i].gameObject.SetActive(false);
            }
        }
        Debug.Log($"実績メニューを更新しました。表示数: {Mathf.Min(achievementEntries.Length, dataList.Count)}");
    }

    /// <summary>
    /// メニュー画面を閉じ、自身のGameObjectを破棄します。
    /// </summary>
    public void CloseMenu()
    {
        // 実績メニューのルートGameObjectを破棄
        Destroy(gameObject);
        Debug.Log("実績メニューを閉じました。");
    }
}