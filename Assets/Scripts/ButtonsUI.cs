using UnityEngine;
using UnityEngine.UI;

// 外部サービス連携のテスト用にUIボタンの動作を定義するクラス
public class ButtonsUI : MonoBehaviour
{
    // Inspectorから設定するボタン
    [SerializeField]
    private Button achievementsButton; // 図鑑・実績などのボタン
    [SerializeField]
    private Button rankingButton;      // ランキング表示ボタン

    void Start()
    {
        // 1. Achievements (図鑑) ボタンが押されたら
        achievementsButton.onClick.AddListener(OnAchievementsClicked);
        // 2. Ranking ボタンが押されたら
        rankingButton.onClick.AddListener(OnRankingClicked);

        volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        volumeSlider.value = 0.5f;

        Debug.Log("ButtonsUIの初期設定が完了しました。");
    }

    // 図鑑ボタンが押されたときの処理
    private void OnAchievementsClicked()
    {
        GameManager.Instance.OpenAchievementsUI();
    }

    // ランキングボタンが押されたときの処理
    private void OnRankingClicked()
    {
        GameManager.Instance.OpenRankingsUI();
    }

    // UnityエディタからSliderコンポーネントをアタッチするための変数
    [SerializeField] private Slider volumeSlider;

    // スライダーの値（0.0〜1.0）を受け取り、マスター音量に設定するメソッド
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }
}