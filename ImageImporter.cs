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

    [Header("Panels")]
    [SerializeField] private GameObject setupPanel, panel1, panel2, panel3, panel4; // ★panel4を追加

    [Header("Scripts")]
    [SerializeField] private DatabaseManager dbManager;
    [SerializeField] private BattleManager battleManager;

    void Start() { OnClickReset(); }

    private void HideAllPanels()
    {
        if (setupPanel) setupPanel.SetActive(false);
        if (panel1) panel1.SetActive(false);
        if (panel2) panel2.SetActive(false);
        if (panel3) panel3.SetActive(false);
        if (panel4) panel4.SetActive(false); // ★ここも追加
    }

    public void OnClickReset()
    {
        loadedSprites.Clear();
        loadedNames.Clear();
        if (imageGridContent) { foreach (Transform child in imageGridContent) Destroy(child.gameObject); }
        if (loadIdInputField) loadIdInputField.text = "";
        if (urlDisplayField) urlDisplayField.text = "";

        HideAllPanels();
        if (setupPanel) setupPanel.SetActive(true); // ★起動時はこれが必ずTrueになる
        UpdateButtons();
    }

    public void OnClickImport()
    {
        NativeGallery.GetImageFromGallery((path) => {
            if (string.IsNullOrEmpty(path)) return;
            Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024);
            if (texture == null) return;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            newSprite.name = fileName;
            loadedSprites.Add(newSprite);
            if (imageGridContent && imageItemPrefab)
            {
                GameObject obj = Instantiate(imageItemPrefab, imageGridContent);
                RawImage raw = obj.GetComponentInChildren<RawImage>();
                if (raw) raw.texture = texture;
            }
            UpdateButtons();
        }, "Select Image", "image/*");
    }

    public void OnClickLoadIdAndPlay()
    {
        string inputId = loadIdInputField.text;
        if (string.IsNullOrEmpty(inputId)) return;
        if (inputId.Contains("id=")) inputId = inputId.Split(new string[] { "id=" }, System.StringSplitOptions.None)[1];
        OnClickLoadIdFromList(inputId);
    }

    public void OnClickLoadIdFromList(string postId)
    {
        PlayerPrefs.SetString("LastPostId", postId);
        dbManager.GetPostData(postId, (title, names) => {
            loadedNames = names;
            HideAllPanels();
            if (panel1) panel1.SetActive(true);
            if (battleManager) battleManager.StartBattleText(names);
        });
    }

    public void OnClickStartBattle()
    {
        if (loadedSprites.Count < 2) return;
        HideAllPanels();
        if (panel1) panel1.SetActive(true);
        if (battleManager) battleManager.StartBattle(loadedSprites);
    }

    public void OnClickPostNamesOnly()
    {
        if (loadedSprites.Count < 2) return;
        string postId = System.Guid.NewGuid().ToString();
        List<string> names = new List<string>();
        foreach (Sprite s in loadedSprites) names.Add(s.name);
        dbManager.SavePostToDatabase(postId, "どっちが好き？", names, (url) => {
            HideAllPanels();
            if (panel3) panel3.SetActive(true);
            if (urlDisplayField) urlDisplayField.text = url;
        });
    }

    public void OnClickCopyURL() { if (urlDisplayField) GUIUtility.systemCopyBuffer = urlDisplayField.text; }
    private void UpdateButtons()
    {
        bool ok = loadedSprites.Count >= 2;
        if (startBattleButton) startBattleButton.interactable = ok;
        if (postFirebaseButton) postFirebaseButton.interactable = ok;
    }
}