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
        indexA = 0; indexB = 1; currentMatchNum = 1;
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
            if (textA) textA.text = battleNames[indexA];
            if (textB) textB.text = battleNames[indexB];
        }
        if (battleProgressBar) battleProgressBar.value = currentMatchNum;
    }

    public void OnClickButtonA() { if (scores != null) { scores[indexA]++; NextMatch(); } }
    public void OnClickButtonB() { if (scores != null) { scores[indexB]++; NextMatch(); } }

    private void NextMatch()
    {
        currentMatchNum++;
        indexB++;
        if (indexB >= battleNames.Count) { indexA++; indexB = indexA + 1; }
        if (indexA >= battleNames.Count - 1) EndBattle();
        else ShowCurrentMatch();
    }

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

        var sorted = FinalResults.OrderByDescending(x => x.score).ToList();
        sorted.Reverse();
        foreach (Transform child in resultContainer) Destroy(child.gameObject);

        int totalCount = battleNames.Count;

        foreach (var item in sorted)
        {
            GameObject obj = Instantiate(resultImagePrefab, resultContainer);
            obj.SetActive(true);

            // プレハブ自体の高さを確保
            LayoutElement layout = obj.GetComponent<LayoutElement>() ?? obj.AddComponent<LayoutElement>();
            layout.preferredHeight = isImageMode ? 400 : 150;

            // 親オブジェクトの RawImage を取得
            RawImage img = obj.GetComponent<RawImage>();
            // その子要素の TextMeshPro を取得
            TextMeshProUGUI txt = obj.GetComponentInChildren<TextMeshProUGUI>();

            if (img != null)
            {
                if (isImageMode && item.sprite != null)
                {
                    // 画像モード：通常表示
                    img.color = Color.white;
                    img.texture = item.sprite.texture;
                }
                else
                {
                    // 文字モード：親(RawImage)を透明にして、子(Text)は見えるようにする
                    // SetActive(false)をすると子も消えるため、これが必要
                    img.color = new Color(0, 0, 0, 0);
                }
            }

            if (txt != null)
            {
                int rank = sorted.Count(x => x.score > item.score) + 1;
                txt.text = $"No.{rank} </b>\n{item.name}</b>\nlike {item.score} / {totalCount - 1}";
                txt.alignment = TextAlignmentOptions.Center;

                // 文字モードのときは、親の画像枠に縛られないようテキストエリアを広げる
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

        // スクロールビューの再計算を強制
        Canvas.ForceUpdateCanvases();
        ScrollRect sr = resultContainer.GetComponentInParent<ScrollRect>();
        if (sr != null) sr.verticalNormalizedPosition = 1f;
    }
}