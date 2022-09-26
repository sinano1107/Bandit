using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    public void FetchHelloWorld()
    {
        // StartCoroutineを使って実行
        StartCoroutine(Method());

        static IEnumerator Method()
        {
            // UnityWebRequestを生成
            UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:8000");

            // SendWebRequestを実行し、送受信開始
            yield return request.SendWebRequest();

            // ハンドリング
            switch (request.result)
            {
                case UnityWebRequest.Result.InProgress:
                    Debug.Log("通信中");
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log($"通信成功: {request.downloadHandler.text}");
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    Debug.Log("サーバとの通信に失敗");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.Log("サーバがエラー応答を返した");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.Log("データの処理中にエラーが発生");
                    break;
                default: throw new Exception("未知のエラーが発生");
            }
        }
    }

    public void FetchVertices()
    {
        StartCoroutine(Method());

        static IEnumerator Method()
        {
            UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:8000/vertices");

            yield return request.SendWebRequest();

            if (request.result is UnityWebRequest.Result.Success)
            {
                int n = int.Parse(request.downloadHandler.text);

                Debug.Log($"n = {n}");

                // 頂点数nのランダム多角形を生成
                GameObject.Find("RandomPolygon")
                    .GetComponent<RandomPolygon>()
                    .Generate(n);
            }
            else Debug.Log(request.result);
        }
    }
}
