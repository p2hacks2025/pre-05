using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // ファイル操作に必要

public class ShareManager : MonoBehaviour
{
    [Header("隠したいUI")]
    [SerializeField] private GameObject shareButton; // シェアするときだけ消すボタン

    public void OnClickShare()
    {
        // コルーチン（時間差処理）を開始
        StartCoroutine(TakeScreenshotAndShare());
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        // 1. シェアボタンを隠す（スクショにボタンが写るとダサいので）
        if (shareButton != null) shareButton.SetActive(false);

        // 2. 画面の描画が終わるまで待つ（重要）
        yield return new WaitForEndOfFrame();

        // 3. スクリーンショットを撮影してデータにする
        Texture2D ss = ScreenCapture.CaptureScreenshotAsTexture();

        // 4. ボタンを再表示
        if (shareButton != null) shareButton.SetActive(true);

        // 5. NativeShareを使ってシェア画面を呼び出す
        new NativeShare()
            .AddFile(ss, "result.png") // 画像を添付
            .SetSubject("好きランキング結果") // 件名（メール等用）
            .SetText("私の好きな画像ランキング！ #Unity #推しランキング") // 本文
            .SetCallback((result, shareTarget) => Debug.Log("シェア結果: " + result + ", アプリ: " + shareTarget))
            .Share(); // シェア実行！

        // メモリのお掃除
        Destroy(ss);
    }
}