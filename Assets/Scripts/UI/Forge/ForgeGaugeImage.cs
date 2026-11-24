using UnityEngine;
using UnityEngine.UI;

namespace UI.Forge
{
    public sealed class ForgeGaugeImage : Image
    {
        // Top edge of mini-game.png, measured from either side toward the center.
        // X is a fraction of the sprite width, Y is the inset in source pixels.
        private static readonly Vector2[] TopEdge =
        {
            new(0f, 9f),
            new(.005f, 24f),
            new(.01f, 34f),
            new(.02f, 50f),
            new(.035f, 67f),
            new(.05f, 81f),
            new(.075f, 99f),
            new(.10f, 115f),
            new(.15f, 138f),
            new(.20f, 156f),
            new(.25f, 170f),
            new(.30f, 180f),
            new(.35f, 188f),
            new(.40f, 193f),
            new(.45f, 197f),
            new(.50f, 198f)
        };
        private const float SpriteHeight = 377f;
        private const float TextureEdgePadding = 3f;
        private static readonly float[] TextureBands = { 0f, .15f, .33f, .455f, .545f, .67f, .85f, 1f };

        private float _center = .06f, _all = .24f, _one = .6f;

        public void SetRanges(float center, float all, float one)
        {
            _center = center;
            _all = all;
            _one = one;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (sprite == null) return;

            var rect = GetPixelAdjustedRect();
            var uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            var leftCenter = (1f - _center) * .5f;
            var leftAll = (1f - _all) * .5f;
            var leftOne = (1f - _one) * .5f;
            var bands = new[] { 0f, leftOne, leftAll, leftCenter, 1f - leftCenter, 1f - leftAll, 1f - leftOne, 1f };
            for (var band = 1; band < bands.Length; band++)
            {
                var left = bands[band - 1];
                var right = bands[band];
                if (left >= right) continue;

                var leftU = TextureBands[band - 1];
                var rightU = TextureBands[band];
                var segments = Mathf.CeilToInt(256f * Mathf.Max(right - left, rightU - leftU));
                for (var i = 0; i <= segments; i++)
                {
                    // Fit the mesh to the frame at x and sample the colored part of the sprite at u.
                    var fraction = i / (float)segments;
                    var x = Mathf.Lerp(left, right, fraction);
                    var u = Mathf.Lerp(leftU, rightU, fraction);
                    var positionX = Mathf.Lerp(rect.xMin, rect.xMax, x);
                    var top = rect.yMax - rect.height * (GetTopInset(x) / SpriteHeight);
                    var textureU = Mathf.Lerp(uv.x, uv.z, u);
                    var topV = Mathf.Lerp(uv.w, uv.y, (GetTopInset(u) + TextureEdgePadding) / SpriteHeight);
                    var vertex = mesh.currentVertCount;
                    mesh.AddVert(new Vector3(positionX, rect.yMin), color, new Vector2(textureU, uv.y));
                    mesh.AddVert(new Vector3(positionX, top), color, new Vector2(textureU, topV));

                    if (i == 0) continue;
                    mesh.AddTriangle(vertex - 2, vertex - 1, vertex + 1);
                    mesh.AddTriangle(vertex - 2, vertex + 1, vertex);
                }
            }
        }

        private static float GetTopInset(float x)
        {
            var fromSide = Mathf.Min(x, 1f - x);
            for (var i = 1; i < TopEdge.Length; i++)
            {
                var next = TopEdge[i];
                if (fromSide > next.x) continue;
                var previous = TopEdge[i - 1];
                return Mathf.Lerp(previous.y, next.y, Mathf.InverseLerp(previous.x, next.x, fromSide));
            }

            return TopEdge[TopEdge.Length - 1].y;
        }
    }
}
