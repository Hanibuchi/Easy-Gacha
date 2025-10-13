using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class NameChangeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;

    private RankingMenu parentMenu;

    /// <summary>
    /// このUIを初期化し、親メニューの参照を設定します。
    /// </summary>
    public void Initialize(RankingMenu parent)
    {
        this.parentMenu = parent;

        if (!string.IsNullOrEmpty(RankingManager.Instance.Username))
        {
            nameInputField.text = RankingManager.Instance.Username;
        }

        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    bool isClicked = false;
    public async void OnConfirmButtonClicked()
    {
        string newUsername = nameInputField.text;
        if (string.IsNullOrEmpty(newUsername))
        {
            Debug.LogWarning("ユーザー名を入力してください。");
            return;
        }
        if (isClicked)
            return;
        isClicked = true;

        // ToDo: ここで新しいユーザー名でスコアを再登録し、Usernameを更新する処理を実装
        await RankingManager.Instance.ChangeUserName(newUsername);

        Debug.Log($"ユーザー名を {newUsername} に変更しました。");

        // 親メニューにランキング更新を要求
        await parentMenu.RefreshRankingMenu();

        Destroy(gameObject);
    }
}