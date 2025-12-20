using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BattleResultData
{
    public string name;
    public Sprite sprite;
    public int score;
}

public class BattleManager : MonoBehaviour
{
    [Header("UI Components (Battle)")]
    [SerializeField] private RawImage rawImageA;
    [SerializeField] private RawImage rawImageB;
    [SerializeField] private TextMeshProUGUI textA;
    [SerializeField] private TextMeshProUGUI textB;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private Slider battleProgressBar;

    [Header("Result Components (Panel2)")]
    [SerializeField] private GameObject panel2;
    [SerializeField] private Transform resultContainer;
    [SerializeField] private GameObject resultImagePrefab;

    public List<BattleResultData> FinalResults { get; private set; }

    private List<Sprite> battleImages;
    private List<string> battleNames;
    private int[] scores;
    private bool isImageMode;
    private int indexA, indexB, currentMatchNum, totalMatchNum;

    // ===============================
    // バトル開始
    // ===============================
    public void StartBattle(List<Sprite> images)
    {
        isImageMode = true;
        battleImages = images;
        battleNames = images.Select(s => s.name).ToList();
        InitializeBattle();
    }

    public void StartBattleText(List<string> names)
    {
        isImageMode = false;
        battleImages = null;
        battleNames = names;
        InitializeBattle();
    }

    private void InitializeBattle()
    {
        scores = new int[battleNames.Count];
        indexA = 0;
        indexB = 1;
        currentMatchNum = 1;
        totalMatchNum = (battleNames.Count * (battleNames.Count - 1)) / 2;

        if (battleProgressBar)
        {
            battleProgressBar.minValue = 0;
            battleProgressBar.maxValue = totalMatchNum;
            battleProgressBar.value = 1;
        }

        if (battlePanel) battlePanel.SetActive(true);
        if (panel2) panel2.SetActive(false);

        ShowCurrentMatch();
    }

    // ===============================
    // 現在の対戦表示
    // ===============================
    private void ShowCurrentMatch()
    {
        if (rawImageA) rawImageA.gameObject.SetActive(isImageMode);
        if (rawImageB) rawImageB.gameObject.SetActive(isImageMode);
        if (textA) textA.gameObject.SetActive(!isImageMode);
        if (textB) textB.gameObject.SetActive(!isImageMode);

        if (isImageMode)
        {
            rawImageA.texture = battleImages[indexA].texture;
            rawImageB.texture = battleImages[indexB].texture;
        }
        else
        {
            textA.text = battleNames[indexA];
            textB.text = battleNames[indexB];
        }

        if (battleProgressBar)
            battleProgressBar.value = currentMatchNum;
    }

    // ===============================
    // ボタン処理
    // ===============================
    public void OnClickButtonA()
    {
        scores[indexA]++;
        NextMatch();
    }

    public void OnClickButtonB()
    {
        scores[indexB]++;
        NextMatch();
    }

    private void NextMatch()
    {
        currentMatchNum++;
        indexB++;

        if (indexB >= battleNames.Count)
        {
            indexA++;
            indexB = indexA + 1;
        }

        if (indexA >= battleNames.Count - 1)
            EndBattle();
        else
            ShowCurrentMatch();
    }

    // ===============================
    // 結果表示
    // ===============================
    private void EndBattle()
    {
        if (battlePanel) battlePanel.SetActive(false);
        if (panel2) panel2.SetActive(true);

        FinalResults = new List<BattleResultData>();

        for (int i = 0; i < battleNames.Count; i++)
        {
            FinalResults.Add(new BattleResultData
            {
                name = battleNames[i],
                sprite = isImageMode ? battleImages[i] : null,
                score = scores[i]
            });
        }

        // スコア順（低 → 高）
        var sorted = FinalResults.OrderBy(x => x.score).ToList();

        foreach (Transform child in resultContainer)
            Destroy(child.gameObject);

        int totalCount = battleNames.Count;

        foreach (var item in sorted)
        {
            GameObject obj = Instantiate(resultImagePrefab, resultContainer);
            obj.SetActive(true);

            LayoutElement layout = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            layout.preferredHeight = isImageMode ? 400 : 150;

            RawImage img = obj.GetComponent<RawImage>();
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();

            if (img != null)
            {
                if (isImageMode && item.sprite != null)
                {
                    img.color = Color.white;
                    img.texture = item.sprite.texture;
                }
                else
                {
                    img.color = new Color(0, 0, 0, 0);
                }
            }

            if (txt != null)
            {
                int rank = sorted.Count(x => x.score > item.score) + 1;

                // ===============================
                // ★ 色分け（No.4以降は黒）★
                // ===============================
                switch (rank)
                {
                    case 1: // 金
                        txt.color = new Color(1f, 0.84f, 0f);
                        break;
                    case 2: // 銀
                        txt.color = new Color(0.75f, 0.75f, 0.75f);
                        break;
                    case 3: // 銅
                        txt.color = new Color(0.8f, 0.5f, 0.2f);
                        break;
                    default: // No.4 以降
                        txt.color = Color.black;
                        break;
                }

                txt.text =
                    $"No.{rank}\n" +
                    $"{item.name}\n" +
                    $"like {item.score} / {totalCount - 1}";

                txt.alignment = TextAlignmentOptions.Center;

                if (!isImageMode)
                {
                    RectTransform rt = txt.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = new Vector2(10, 10);
                    rt.offsetMax = new Vector2(-10, -10);
                }
            }
        }

        Canvas.ForceUpdateCanvases();
        ScrollRect sr = resultContainer.GetComponentInParent<ScrollRect>();
        if (sr != null) sr.verticalNormalizedPosition = 1f;
    }
}
