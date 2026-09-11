using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // Feather both the sides and the ends; slow rotation should never expose hard wedges.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RewardRaysGraphic : MaskableGraphic
    {
        [SerializeField, Range(4, 16)] private int _rayCount = 8;
        [SerializeField, Range(4f, 24f)] private float _rayWidth = 16f;
        [SerializeField] private float _angleOffset = 9f;
        private static readonly float[] Radii = { .015f, .09f, .25f, .42f, .7f, 1f };
        private static readonly float[] RadialAlpha = { 0f, 1f, .65f, .32f, .08f, 0f };
        private static readonly float[] SideAlpha = { 0f, .6f, 1f, 1f, 1f, .6f, 0f };

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = rectTransform.rect;
            var center = rect.center;
            var radius = Mathf.Max(rect.width, rect.height) * 0.5f;
            for (var ray = 0; ray < _rayCount; ray++)
            {
                var start = mesh.currentVertCount;
                for (var ring = 0; ring < Radii.Length; ring++)
                for (var side = 0; side < SideAlpha.Length; side++)
                {
                    var angle = (ray * 360f / _rayCount + _angleOffset + (side / 6f - .5f) * _rayWidth) * Mathf.Deg2Rad;
                    var tint = color;
                    tint.a *= RadialAlpha[ring] * SideAlpha[side];
                    mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (radius * Radii[ring]), tint, Vector2.zero);
                    if (ring == 0 || side == 0)
                        continue;

                    var vertex = start + ring * SideAlpha.Length + side;
                    mesh.AddTriangle(vertex - 8, vertex - 7, vertex);
                    mesh.AddTriangle(vertex - 8, vertex, vertex - 1);
                }
            }
        }
    }
}
