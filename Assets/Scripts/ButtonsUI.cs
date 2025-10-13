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
    private const string MasterVolumeKey = "MasterVolume";

    void Start()
    {
        // 1. Achievements (図鑑) ボタンが押されたら
        achievementsButton.onClick.AddListener(OnAchievementsClicked);
        // 2. Ranking ボタンが押されたら
        rankingButton.onClick.AddListener(OnRankingClicked);

        volumeSlider.onValueChanged.AddListener(SetMasterVolume);

        // PlayerPrefsから保存された音量設定を読み込む（保存された値がなければデフォルト値として0.5fを使用）
        float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f);

        // 読み込んだ値でスライダーの見た目と実際の音量を初期化
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;
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
        // マスター音量を設定
        AudioListener.volume = volume;

        // --- 音量設定の保存 ---
        // 変更された音量設定をPlayerPrefsに保存
        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save(); // 念のためSaveを呼び出して即時書き込み
    }
}