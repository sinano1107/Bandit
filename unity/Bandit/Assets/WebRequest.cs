using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequest : MonoBehaviour
{
    public void RequestStart()
    {
        StartCoroutine(Method());
        static IEnumerator Method()
        {
            UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:8000/start");
            yield return request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.Success)
            {
                int n = int.Parse(request.downloadHandler.text);
                Debug.Log($"n = {n}");
                // 頂点数=nの多角形を生成
                GameObject.Find("RandomPolygon")
                    .GetComponent<RandomPolygon>()
                    .Generate(n);
            }
            else Debug.Log(request.result);
        }
    }

    public void RequestUpdate(bool reward)
    {
        StartCoroutine(RequestUpdateMethod(reward));
    }

    private IEnumerator RequestUpdateMethod(bool reward)
    {
        UnityWebRequest request = UnityWebRequest.Get($"http://127.0.0.1:8000/update?reward={reward}");
        yield return request.SendWebRequest();
        if (request.result is UnityWebRequest.Result.Success)
        {
            int n = int.Parse(request.downloadHandler.text);
            Debug.Log($"n = {n}");
            // 頂点数=nの多角形を生成
            GameObject.Find("RandomPolygon")
                .GetComponent<RandomPolygon>()
                .Generate(n);
        }
        else Debug.Log(request.result);
    }
}
