using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RandomPolygon : MonoBehaviour
{
    const float range = 1.0f;
    const int maxTry = 50;
    private List<Vector3> vertices = new();
    private List<int> triangles = new();
    private List<(int a, int b)> sides = new();

    // 任意の頂点数のランダムな多角形を生成する
    public void Generate(int n)
    {
        if (n < 3) throw new Exception("頂点数が3以下の多角形は作れません");

        // リセット
        PolygonReset();

        // 生成
        for (int i = 3; i <= n; i++)
        {
            if (i == 3) Appearance();
            else if (i == 4) Division();
            else Increase();
        }

        // 表示
        Display();
    }

    // (0)リセット
    private void PolygonReset()
    {
        vertices = new List<Vector3> { };
        triangles = new List<int> { };
        sides = new List<(int a, int b)> { };
    }

    // (1)出現
    private void Appearance()
    {
        // 3つの頂点を生成
        for (int i = 0; i < 3; i++)
            vertices.Add(new Vector3(
                Random.Range(-range, range),
                Random.Range(-range, range)
            ));

        // 適切な順序で繋ぐ
        triangles.AddRange(GetCorrectOrder(0, 1, 2));

        // 辺を保存する
        for (int i = 0; i < 3; i++) sides.Add((i, (i + 1) % 3));
    }

    // (2)分裂
    private void Division()
    {
        // 増殖元の辺を選択
        int choice = Random.Range(0, 3);

        // 増殖元の辺を構成する点(a,b)を取得
        var (a_index, b_index) = sides[choice];
        Vector3 a = vertices[a_index];
        Vector3 b = vertices[b_index];

        // a,b以外の一点oを取得
        int o_index = 3 - a_index - b_index;
        Vector3 o = vertices[o_index];

        // 新たに生成する点cの存在範囲
        float x_max = Math.Max(a.x, b.x) + range;
        float x_min = Math.Min(a.x, b.x) - range;
        float y_max = Math.Max(a.y, b.y) + range;
        float y_min = Math.Min(a.y, b.y) - range;

        // cを選択
        Vector3 c = new(Random.Range(x_max, x_min), Random.Range(y_max, y_min));

        float o_cross_z = GetCrossZ(a, b, o);
        float c_cross_z = GetCrossZ(a, b, c);

        // a,bを含んだ直線によってoとcが分断されていない限り、cを選択し直す
        while (o_cross_z * c_cross_z > 0)
        {
            c = new(Random.Range(x_max, x_min), Random.Range(y_max, y_min));
            c_cross_z = GetCrossZ(a, b, c);
        }

        // cを追加
        vertices.Add(c);

        // 三角形a,b,cを追加
        triangles.AddRange(GetCorrectOrder(a_index, b_index, 3));

        // 増殖元の辺を削除し、新たな二辺を追加する
        sides.RemoveAt(choice);
        sides.Add((a_index, 3));
        sides.Add((b_index, 3));
    }

    // (3)増殖
    private void Increase()
    {
        // 何個目の頂点を作り出すか(0スタート)
        int index = vertices.Count;

        // 増殖元の辺を選択
        int choice = Random.Range(0, sides.Count);

        // 増殖元の辺を構成する点(a,b)を取得
        var (a_index, b_index) = sides[choice];
        Vector3 a = vertices[a_index];
        Vector3 b = vertices[b_index];

        // 新たに生成する点cの存在範囲
        float x_max = Math.Max(a.x, b.x) + range;
        float x_min = Math.Min(a.x, b.x) - range;
        float y_max = Math.Max(a.y, b.y) + range;
        float y_min = Math.Min(a.y, b.y) - range;

        // cを選択
        Vector3 c = new(Random.Range(x_max, x_min), Random.Range(y_max, y_min));

        // cが既存の三角形に含まれていないかつ、追加する事になる辺が既存の辺と衝突しないcが見つかるまで選択し直す
        var count = 1;
        while(IsInPolygon(c) || IsItFoldedBack(c, a, b))
        {
            if (count > maxTry)
            {
                Debug.Log($"{maxTry}回試行しましたが適切な座標が見つかりませんでした");
                // 辺の選択からやり直す
                choice = Random.Range(0, sides.Count);
                (a_index, b_index) = sides[choice];
                a = vertices[a_index];
                b = vertices[b_index];
                x_max = Math.Max(a.x, b.x) + range;
                x_min = Math.Min(a.x, b.x) - range;
                y_max = Math.Max(a.y, b.y) + range;
                y_min = Math.Min(a.y, b.y) - range;
                count = 0;
            }
            c = new Vector3(Random.Range(x_max, x_min), Random.Range(y_max, y_min));
            count += 1;
        }

        // cを追加
        vertices.Add(c);

        // 三角形a,b,cを追加
        triangles.AddRange(GetCorrectOrder(a_index, b_index, index));

        // 増殖元の辺を削除し、新たな二辺を追加する
        sides.RemoveAt(choice);
        sides.Add((a_index, index));
        sides.Add((b_index, index));
    }

    // 表示
    private void Display()
    {
        // メッシュに頂点と三角形を代入
        Mesh mesh = new();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        // MeshFilterを通してMeshRendererにメッシュをセット
        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
    }

    // 外積のZ成分を返す
    private float GetCrossZ(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;

        return Vector3.Cross(side1, side2).z;
    }

    // 3つの頂点を受け取り、正しい順序で返す
    private List<int> GetCorrectOrder(int v0_index, int v1_index, int v2_index)
    {
        Vector3 v0 = vertices[v0_index];
        Vector3 v1 = vertices[v1_index];
        Vector3 v2 = vertices[v2_index];
        float z = GetCrossZ(v0, v1, v2);

        return (z > 0)
            ? new List<int> { v0_index, v2_index, v1_index }
            : new List<int> { v0_index, v1_index, v2_index };
    }

    // 点Pが多角形内に存在するか調べる
    private bool IsInPolygon(Vector3 P)
    {
        // 点Pが三角形内に存在するか調べる
        bool IsInTriangle(Vector3 p, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            bool b0, b1, b2;

            b0 = GetCrossZ(p, v0, v1) < 0;
            b1 = GetCrossZ(p, v1, v2) < 0;
            b2 = GetCrossZ(p, v2, v0) < 0;

            return (b0 == b1) && (b1 == b2);
        }

        // 構成要素である全三角形との内外判定
        for (int i = 0; i < triangles.Count / 3; i++)
        {
            int index = i * 3;

            Vector3 v0, v1, v2;
            v0 = vertices[triangles[index]];
            v1 = vertices[triangles[index + 1]];
            v2 = vertices[triangles[index + 2]];

            if (IsInTriangle(P, v0, v1, v2)) return true;
        }

        return false;
    }

    // 点Pと点a,bを結んだ辺が既存の辺と衝突しているか調べる
    private bool IsItFoldedBack(Vector3 P, Vector3 a, Vector3 b)
    {
        for (int i = 0; i < sides.Count; i++)
        {
            var (x_index, y_index) = sides[i];
            Vector3 x = vertices[x_index];
            Vector3 y = vertices[y_index];

            // aとPを結んだ辺との交差判定
            float s = GetCrossZ(x, y, P);
            float t = GetCrossZ(x, y, a);
            float u = GetCrossZ(P, a, x);
            float v = GetCrossZ(P, a, y);
            if ((s * t < 0) && (u * v < 0)) return true;

            // bとPを結んだ辺との交差判定
            t = GetCrossZ(x, y, b);
            u = GetCrossZ(P, b, x);
            v = GetCrossZ(P, b, y);
            if ((s * t < 0) && (u * v < 0)) return true;
        }

        return false;
    }
}
