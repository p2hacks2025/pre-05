using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ShareManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private DatabaseManager dbManager;

    [Header("Input")]
    [SerializeField] private TMP_InputField titleInputField; // タイトル入力欄

    // 【シェアボタン】から呼び出す
    public void OnClickShareAndUpload()
    {
        UploadAndProcess((url) => {
            ExecuteSNSShare(url);
        });
    }

    // 【投稿ボタン】から呼び出す
    public void OnClickPostOnly()
    {
        UploadAndProcess((url) => {
            // SNSは開かず、完了ログや通知を出す
            Debug.Log("<color=cyan>投稿が完了しました！ URL: " + url + "</color>");
            // 必要であればここで「投稿しました」という簡易ポップアップなどを出す
        });
    }

    private void UploadAndProcess(System.Action<string> onComplete)
    {
        if (battleManager == null || battleManager.FinalResults == null || battleManager.FinalResults.Count == 0) return;

        // タイトルが空ならデフォルト名にする
        string title = string.IsNullOrEmpty(titleInputField.text) ? "どっちが好き？" : titleInputField.text;

        string newPostId = System.Guid.NewGuid().ToString();
        List<string> names = battleManager.FinalResults.Select(x => x.name).ToList();

        Debug.Log("Firebaseに保存中...");
        dbManager.SavePostToDatabase(newPostId, title, names, (url) => {
            onComplete?.Invoke(url);
        });
    }

    private void ExecuteSNSShare(string shareUrl)
    {
        var winner = battleManager.FinalResults.Last(); // 1位
        string shareText = $"「どっちが好き？」で対戦したよ！\n1位は【{winner.name}】でした！\n遊んでみてね！\n#どっちが好きアプリ";
        string twitterUrl = "https://twitter.com/intent/tweet?text=" + System.Uri.EscapeDataString(shareText + "\n" + shareUrl);

        GUIUtility.systemCopyBuffer = shareUrl;
        Application.OpenURL(twitterUrl);
    }
}