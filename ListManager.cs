using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ListManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject listPanel;    // Panel 4
    [SerializeField] private Transform listContainer; // ScrollViewのContent
    [SerializeField] private GameObject listItemPrefab; // 1行分のボタン付きプレハブ

    [Header("Scripts")]
    [SerializeField] private DatabaseManager dbManager;
    [SerializeField] private ImageImporter imageImporter;

    // 一覧を開くボタン（スタートボタン等）からこれを呼び出す
    public void ShowPostList()
    {
        if (listPanel == null) return;

        listPanel.SetActive(true);

        // 既存のリスト表示を一旦すべて削除（重複防止）
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        // DatabaseManagerを通じてFirebaseから全データを取得
        dbManager.GetAllPosts((posts, ids) => {
            for (int i = 0; i < posts.Count; i++)
            {
                string postId = ids[i];
                // タイトルを取得（なければ"無題"）
                string title = posts[i].ContainsKey("title") ? posts[i]["title"].ToString() : "無題の対戦";

                // プレハブを生成
                GameObject item = Instantiate(listItemPrefab, listContainer);

                // プレハブ内のテキストにタイトルをセット
                TextMeshProUGUI titleText = item.GetComponentInChildren<TextMeshProUGUI>();
                if (titleText)
                {
                    titleText.text = title;
                }

                // プレハブ内の「遊ぶ」ボタンを取得して機能を割り当て
                Button playBtn = item.GetComponentInChildren<Button>();
                if (playBtn)
                {
                    // ボタンを押した時の処理
                    playBtn.onClick.AddListener(() => {
                        imageImporter.OnClickLoadIdFromList(postId);
                        listPanel.SetActive(false); // リスト画面を閉じる
                    });
                }
            }
        });
    }

    // 閉じるボタン用のメソッド
    public void OnClickCloseList()
    {
        if (listPanel != null)
        {
            listPanel.SetActive(false);
        }
    }
}