using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingMenu : MonoBehaviour
{
    [Header("UI Configuration")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nameChangeButton;
    [SerializeField] private GameObject rankingEntryPrefab; // RankingEntry.cs がアタッチされたプレハブ
    [SerializeField] private Transform personalEntryParent;
    [SerializeField] private Transform contentParent; // RankingEntryを生成する親オブジェクト

    [Header("Sub-UI Prefabs")]
    [SerializeField] private GameObject nameChangeUIPrefab; // NameChangeUI.cs がアタッチされたプレハブ

    [Header("State Info")]
    [SerializeField] private TextMeshProUGUI loadingText; // ロード中の表示用

    private async void Start()
    {
        // ボタンにイベントを登録
        closeButton.onClick.AddListener(OnCloseButtonClicked);
        nameChangeButton.onClick.AddListener(OnNameChangeButtonClicked);

        // ランキング表示の開始
        await RefreshRankingMenu();
    }

    public async Task RefreshRankingMenu()
    {
        var refreshTask = RefreshRankingDisplay();
        var personalTask = DisplayPersonalRank();

        // 両方の完了を待つ
        await Task.WhenAll(refreshTask, personalTask);
    }

    /// <summary>
    /// ランキングデータを取得し、UIを更新します。
    /// </summary>
    async Task RefreshRankingDisplay()
    {
        if (loadingText != null) loadingText.text = "Loading Ranking...";

        // 既存のリスト要素を全てクリア
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // --- RankingManagerからデータを取得 ---
        List<RankingManager.RankingEntry> rankingData = await RankingManager.Instance.GetRankingAsync(30); // 上位30件を取得

        if (loadingText != null) loadingText.text = ""; // ロード完了

        if (rankingData == null || rankingData.Count == 0)
        {
            // データがない場合の処理
            if (loadingText != null) loadingText.text = "ランキングデータがありません。";
            return;
        }

        // --- 取得したデータをループで表示 ---
        for (int i = 0; i < rankingData.Count; i++)
        {
            GameObject entryObj = Instantiate(rankingEntryPrefab, contentParent);
            RankingEntry entry = entryObj.GetComponent<RankingEntry>();

            if (entry != null)
            {
                // 順位はリストのインデックス+1
                entry.SetData(i + 1, rankingData[i]);
            }
        }
    }

    /// <summary>
    /// 自分のランクを計算し、専用のUIに表示します。
    /// </summary>
    async Task DisplayPersonalRank()
    {
        // 既存の自分のエントリをクリア (更新のため)
        foreach (Transform child in personalEntryParent)
        {
            Destroy(child.gameObject);
        }

        // --- 自分のデータと順位を取得 ---

        // 1. 自分のデータ (スコア) を取得
        RankingManager.RankingEntry userEntry = await RankingManager.Instance.GetMyScoreAsync();

        if (userEntry == null)
        {
            // スコア未登録の場合はここで終了
            Debug.Log("Personal score not found. Cannot display rank.");
            return;
        }

        // 2. 自分の順位を計算
        int rank = await RankingManager.Instance.GetUserRankAsync2();

        if (rank > 0)
        {
            // --- UIに表示 ---

            // プレハブを生成
            GameObject entryObj = Instantiate(rankingEntryPrefab, personalEntryParent);
            RankingEntry entry = entryObj.GetComponent<RankingEntry>();

            if (entry != null)
            {
                entry.SetData(rank, userEntry, true);
                // entryObj.GetComponent<Image>().color = new Color(0.8f, 1f, 0.8f); // 自分の行を目立たせる
            }
        }
        else
        {
            Debug.Log("Failed to calculate personal rank.");
        }
    }

    private void OnCloseButtonClicked()
    {
        // 閉じるボタンが押されたらこのオブジェクトを破棄
        Destroy(gameObject);
    }

    private void OnNameChangeButtonClicked()
    {
        if (nameChangeUIPrefab != null)
        {
            // 名前変更UIを生成し、このインスタンスを渡す
            GameObject changeUI = Instantiate(nameChangeUIPrefab);
            NameChangeUI uiScript = changeUI.GetComponent<NameChangeUI>();

            if (uiScript != null)
            {
                // 生成されたUIに、このRankingMenuインスタンスを渡す
                uiScript.Initialize(this);
            }
            else
            {
                Debug.LogError("NameChangeUIPrefabに NameChangeUI スクリプトが見つかりません。");
            }
        }
    }
}