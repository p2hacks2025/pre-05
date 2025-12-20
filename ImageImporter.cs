using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageImporter : MonoBehaviour
{
    [Header("Data Lists")]
    public List<Sprite> loadedSprites = new List<Sprite>();
    public List<string> loadedNames = new List<string>();

    [Header("UI Components")]
    [SerializeField] private TMP_InputField loadIdInputField;
    [SerializeField] private TMP_InputField urlDisplayField;
    [SerializeField] private Button startBattleButton, postFirebaseButton;
    [SerializeField] private Transform imageGridContent;
    [SerializeField] private GameObject imageItemPrefab;

    // ★ 追加：中央表示用 RawImage
    [SerializeField] private RawImage previewRawImage;

    [Header("Panels")]
    [SerializeField] private GameObject setupPanel;
    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;
    [SerializeField] private GameObject panel3;
    [SerializeField] private GameObject panel4;

    [Header("Scripts")]
    [SerializeField] private DatabaseManager dbManager;
    [SerializeField] private BattleManager battleManager;

    void Start()
    {
        OnClickReset();
    }

    private void HideAllPanels()
    {
        if (setupPanel) setupPanel.SetActive(false);
        if (panel1) panel1.SetActive(false);
        if (panel2) panel2.SetActive(false);
        if (panel3) panel3.SetActive(false);
        if (panel4) panel4.SetActive(false);
    }

    public void OnClickReset()
    {
        loadedSprites.Clear();
        loadedNames.Clear();

        if (imageGridContent)
        {
            foreach (Transform child in imageGridContent)
            {
                Destroy(child.gameObject);
            }
        }

        if (loadIdInputField) loadIdInputField.text = "";
        if (urlDisplayField) urlDisplayField.text = "";

        // ★ 中央画像を消す
        if (previewRawImage)
        {
            previewRawImage.texture = null;
            previewRawImage.color = new Color(1, 1, 1, 0);
        }

        HideAllPanels();
        if (setupPanel) setupPanel.SetActive(true);

        UpdateButtons();
    }

    // =========================
    // 画像読み込み
    // =========================
    public void OnClickImport()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (string.IsNullOrEmpty(path)) return;

            Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024);
            if (texture == null) return;

            // ★ 中央に画像を表示
            if (previewRawImage != null)
            {
                previewRawImage.texture = texture;
                previewRawImage.color = Color.white;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            newSprite.name = fileName;
            loadedSprites.Add(newSprite);

            // 一覧に追加
            if (imageGridContent && imageItemPrefab)
            {
                GameObject obj = Instantiate(imageItemPrefab, imageGridContent);
                RawImage raw = obj.GetComponentInChildren<RawImage>();
                if (raw) raw.texture = texture;
            }

            UpdateButtons();

        }, "Select Image", "image/*");
    }

    // =========================
    // ID からロード
    // =========================
    public void OnClickLoadIdAndPlay()
    {
        string inputId = loadIdInputField.text;
        if (string.IsNullOrEmpty(inputId)) return;

        if (inputId.Contains("id="))
        {
            inputId = inputId.Split(new string[] { "id=" }, System.StringSplitOptions.None)[1];
        }

        OnClickLoadIdFromList(inputId);
    }

    public void OnClickLoadIdFromList(string postId)
    {
        PlayerPrefs.SetString("LastPostId", postId);

        dbManager.GetPostData(postId, (title, names) =>
        {
            loadedNames = names;

            HideAllPanels();
            if (panel1) panel1.SetActive(true);

            if (battleManager)
            {
                battleManager.StartBattleText(names);
            }
        });
    }

    // =========================
    // バトル開始
    // =========================
    public void OnClickStartBattle()
    {
        if (loadedSprites.Count < 2) return;

        HideAllPanels();
        if (panel1) panel1.SetActive(true);

        if (battleManager)
        {
            battleManager.StartBattle(loadedSprites);
        }
    }

    // =========================
    // Firebase 投稿（名前のみ）
    // =========================
    public void OnClickPostNamesOnly()
    {
        if (loadedSprites.Count < 2) return;

        string postId = System.Guid.NewGuid().ToString();
        List<string> names = new List<string>();

        foreach (Sprite s in loadedSprites)
        {
            names.Add(s.name);
        }

        dbManager.SavePostToDatabase(postId, "どっちが好き？", names, (url) =>
        {
            HideAllPanels();
            if (panel3) panel3.SetActive(true);
            if (urlDisplayField) urlDisplayField.text = url;
        });
    }

    public void OnClickCopyURL()
    {
        if (urlDisplayField)
        {
            GUIUtility.systemCopyBuffer = urlDisplayField.text;
        }
    }

    // =========================
    // ボタン制御
    // =========================
    private void UpdateButtons()
    {
        bool ok = loadedSprites.Count >= 2;

        if (startBattleButton) startBattleButton.interactable = ok;
        if (postFirebaseButton) postFirebaseButton.interactable = ok;
    }
}
