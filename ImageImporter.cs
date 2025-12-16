using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageImporter : MonoBehaviour
{
    // 読み込んだ画像を保存しておくリスト
    public List<Sprite> loadedSprites = new List<Sprite>();

    [Header("UI Preview (Optional)")]
    [SerializeField] private Image debugPreviewImage; // 読み込み確認用

    [Header("Buttons")]
    [SerializeField] private Button startBattleButton;

    [Header("Panels")]
    [SerializeField] private GameObject setupPanel; // 最初の画面
    [SerializeField] private GameObject panel1;     // バトル画面
    [SerializeField] private GameObject panel2;     // 結果画面（リセット時に消すために必要）

    [Header("Scripts")]
    [SerializeField] private BattleManager battleManager;

    void Start()
    {
        // アプリ起動時の初期化
        if (startBattleButton != null)
        {
            startBattleButton.interactable = false; // 最初は押せない
        }

        // パネルの初期状態
        if (setupPanel != null) setupPanel.SetActive(true);
        if (panel1 != null) panel1.SetActive(false);
        if (panel2 != null) panel2.SetActive(false);
    }

    // 「画像を読み込む」ボタンの処理
    public void OnClickImport()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            // パスがnullでなければ画像が選択されたということ
            if (path != null)
            {
                // テクスチャ読み込み
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024);
                if (texture == null)
                {
                    Debug.LogError("テクスチャの読み込みに失敗しました");
                    return;
                }

                // Sprite作成
                Sprite newSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );

                // リストに追加
                loadedSprites.Add(newSprite);

                // プレビュー表示（もし設定されていれば）
                if (debugPreviewImage != null)
                {
                    debugPreviewImage.sprite = newSprite;
                    debugPreviewImage.preserveAspect = true;
                }

                // 2枚以上になったらバトル開始ボタンを有効化
                if (loadedSprites.Count >= 2 && startBattleButton != null)
                {
                    startBattleButton.interactable = true;
                }

                Debug.Log($"読み込み成功！ 現在の枚数: {loadedSprites.Count}");
            }
        });
    }

    // 「バトル開始」ボタンの処理
    public void OnClickStartBattle()
    {
        Debug.Log("バトルモードへ移行します！");

        // パネル切り替え
        if (setupPanel != null) setupPanel.SetActive(false);
        if (panel1 != null) panel1.SetActive(true);

        // バトルマネージャー起動
        if (battleManager != null)
        {
            battleManager.StartBattle(loadedSprites);
        }
    }

    // 「最初に戻る（リセット）」ボタンの処理
    public void OnClickResetButton()
    {
        Debug.Log("リセットします。");

        // 1. 画像リストをクリア
        loadedSprites.Clear();

        // 2. スタートボタンを無効化
        if (startBattleButton != null)
        {
            startBattleButton.interactable = false;
        }

        // 3. パネルを初期状態に戻す
        if (panel2 != null) panel2.SetActive(false); // 結果画面を消す
        if (panel1 != null) panel1.SetActive(false); // 念のため
        if (setupPanel != null) setupPanel.SetActive(true); // 最初の画面を出す

        // 4. プレビュー画像もクリア
        if (debugPreviewImage != null)
        {
            debugPreviewImage.sprite = null;
        }
    }
}