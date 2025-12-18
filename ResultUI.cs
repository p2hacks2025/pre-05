using UnityEngine;
using UnityEngine.UI; // RawImage用
using TMPro;          // TextMeshPro用

public class ResultItem : MonoBehaviour
{
    // ここにInspector画面でドラッグ＆ドロップして紐付けます
    public RawImage iconImage;          // 画像を表示するところ
    public TextMeshProUGUI rankText;    // 「1位」などを表示するところ
    public TextMeshProUGUI commentText; // ★ここに新規作成したセリフ用テキストを入れる
}