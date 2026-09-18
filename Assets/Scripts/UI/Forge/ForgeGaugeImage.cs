using UnityEngine;
using UnityEngine.UI;

namespace UI.Forge
{
    // Keep the authored curved silhouette while fitting its colored ranges to the QTE rules.
    public sealed class ForgeGaugeImage : Image
    {
        private float _center = .06f, _all = .24f, _one = .6f;
        public void SetRanges(float center, float all, float one)
        { _center = center; _all = all; _one = one; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (sprite == null) return;
            var r = GetPixelAdjustedRect();
            var uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            var ranges = new[] { 0f, _center, _all, _one, 1f };
            var textureRanges = new[] { 0f, .09f, .34f, .7f, 1f };
            const int segments = 100;
            for (var i = 0; i <= segments; i++)
            {
                var x = i / (float)segments;
                var distance = Mathf.Abs(x - .5f) * 2f;
                var mapped = 1f;
                for (var j = 1; j < ranges.Length; j++)
                    if (distance <= ranges[j]) { mapped = Mathf.Lerp(textureRanges[j - 1], textureRanges[j], Mathf.InverseLerp(ranges[j - 1], ranges[j], distance)); break; }
                var u = .5f + Mathf.Sign(x - .5f) * mapped * .5f;
                mesh.AddVert(new Vector3(r.xMin + r.width * x, r.yMin), color, new Vector2(Mathf.Lerp(uv.x, uv.z, u), uv.y));
                mesh.AddVert(new Vector3(r.xMin + r.width * x, r.yMax), color, new Vector2(Mathf.Lerp(uv.x, uv.z, u), uv.w));
                if (i == 0) continue;
                var k = i * 2;
                mesh.AddTriangle(k - 2, k - 1, k + 1); mesh.AddTriangle(k - 2, k + 1, k);
            }
        }
    }
}
