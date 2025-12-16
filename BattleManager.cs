using System.Collections.Generic;
using System.Linq; // ソート機能に必要
using UnityEngine;
using UnityEngine.UI; // RawImage等を扱う
using TMPro;          // TextMeshProを扱う

public class BattleManager : MonoBehaviour
{
    [Header("UI Components (Battle)")]
    [SerializeField] private RawImage rawImageA; // 上の画像
    [SerializeField] private RawImage rawImageB; // 下の画像
    [SerializeField] private GameObject battlePanel; // Panel1

    [Header("Result Components (Panel2)")]
    [SerializeField] private GameObject panel2;       // 結果画面
    [SerializeField] private Transform resultContainer; // Content
    [SerializeField] private GameObject resultImagePrefab; // プレハブ

    // 内部データ
    private List<Sprite> battleImages;
    private int[] scores; // 各画像の勝ち点

    private int indexA = 0;
    private int indexB = 1;

    // バトルの初期化（ここが呼ばれるまで scores は null です）
    public void StartBattle(List<Sprite> images)
    {
        battleImages = images;
        scores = new int[battleImages.Count]; // ここで初めて箱が作られる
        indexA = 0;
        indexB = 1;

        if (battlePanel != null) battlePanel.SetActive(true);
        if (panel2 != null) panel2.SetActive(false);

        ShowCurrentMatch();
    }

    // 現在の対戦を表示
    private void ShowCurrentMatch()
    {
        // 画像A
        Texture texA = battleImages[indexA].texture;
        rawImageA.texture = texA;
        FitAspectRatio(rawImageA, texA);

        // 画像B
        Texture texB = battleImages[indexB].texture;
        rawImageB.texture = texB;
        FitAspectRatio(rawImageB, texB);
    }

    // アスペクト比を合わせる便利関数
    private void FitAspectRatio(RawImage targetImage, Texture texture)
    {
        var fitter = targetImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            float ratio = (float)texture.width / texture.height;
            fitter.aspectRatio = ratio;
        }
    }

    // Aが勝ち
    public void OnClickButtonA()
    {
        // ★修正: エラー防止の安全装置
        // まだ画像が読み込まれていないのにボタンを押してもエラーにならないようにする
        if (scores == null || scores.Length == 0) return;

        scores[indexA]++;
        NextMatch();
    }

    // Bが勝ち
    public void OnClickButtonB()
    {
        // ★修正: エラー防止の安全装置
        if (scores == null || scores.Length == 0) return;

        scores[indexB]++;
        NextMatch();
    }

    // 次の試合へ
    private void NextMatch()
    {
        indexB++;
        if (indexB >= battleImages.Count)
        {
            indexA++;
            indexB = indexA + 1;
        }

        if (indexA >= battleImages.Count - 1)
        {
            EndBattle();
        }
        else
        {
            ShowCurrentMatch();
        }
    }

    // 結果発表
    private void EndBattle()
    {
        Debug.Log("全試合終了！Panel2へ移動します。");

        if (battlePanel != null) battlePanel.SetActive(false);
        if (panel2 != null) panel2.SetActive(true);

        // データリスト作成
        List<ImageScorePair> resultList = new List<ImageScorePair>();
        for (int i = 0; i < battleImages.Count; i++)
        {
            resultList.Add(new ImageScorePair { sprite = battleImages[i], score = scores[i] });
        }

        // 表示順序：点数が低い順（左→右で順位が上がるように）
        var sortedList = resultList.OrderBy(x => x.score).ToList();

        // 以前の結果を削除
        foreach (Transform child in resultContainer)
        {
            Destroy(child.gameObject);
        }

        int maxBattles = battleImages.Count - 1;

        // 結果生成ループ
        foreach (var item in sortedList)
        {
            GameObject obj = Instantiate(resultImagePrefab, resultContainer);

            // 画像セット（プレハブ内の構造が変わっても探せるようにInChildrenを使用）
            RawImage rawImg = obj.GetComponentInChildren<RawImage>();
            if (rawImg != null)
            {
                rawImg.texture = item.sprite.texture;
                FitAspectRatio(rawImg, item.sprite.texture);
            }

            // テキストセット（TextMeshPro対応 & 順位計算つき）
            TextMeshProUGUI scoreText = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (scoreText != null)
            {
                // 自分より点数が高い人の数を数えて順位を決める（同率順位対応）
                // 例：3点, 3点, 1点 → 1位, 1位, 3位
                int rank = resultList.Count(x => x.score > item.score) + 1;

                scoreText.text = $"第{rank}位\n好き率 {item.score}／{maxBattles}";
            }
        }
    }

    class ImageScorePair
    {
        public Sprite sprite;
        public int score;
    }
}