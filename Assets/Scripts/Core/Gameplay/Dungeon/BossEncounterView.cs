using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Gameplay.Dungeon
{
    internal sealed class BossEncounterView
    {
        private const float ShieldQuadScale = 2.35f; // Includes the halo outside the shield's rim.
        private const float LinkWidth = 0.38f;
        private const float FlashDuration = 0.24f;
        private static readonly int FlowTimeId = Shader.PropertyToID("_FlowTime");
        private static readonly int HitFlashId = Shader.PropertyToID("_HitFlash");

        private readonly GameObject _root;
        private readonly Mesh _quad;
        private readonly MeshRenderer _shield;
        private readonly MeshRenderer[] _links;
        private readonly MaterialPropertyBlock _properties = new();
        private float _flowTime;
        private float _flashTimeLeft;

        public BossEncounterView(BossEncounterConfig config, Vector3 bossPosition, Vector3[] totemPositions)
        {
            _root = new GameObject("Boss encounter effects");
            _root.SetActive(false);
            _root.transform.position = bossPosition;
            _quad = CreateQuad();

            _shield = CreateSurface("Shield", config.shieldMaterial);
            _shield.transform.localPosition = config.shieldCenterOffset;
            var shieldSize = config.shieldRadii * ShieldQuadScale;
            _shield.transform.localScale = new Vector3(shieldSize.x, shieldSize.y, 1f);
            _shield.sortingOrder = 1;

            _links = new MeshRenderer[totemPositions.Length];
            for (var i = 0; i < totemPositions.Length; i++)
            {
                var start = (Vector2)(totemPositions[i] - bossPosition) + config.totemLinkOffset;
                var fromCenter = start - config.shieldCenterOffset;
                var inShieldSpace = new Vector2(fromCenter.x / config.shieldRadii.x, fromCenter.y / config.shieldRadii.y);
                var end = config.shieldCenterOffset + fromCenter / Mathf.Max(1f, inShieldSpace.magnitude);
                var direction = end - start;

                var link = CreateSurface($"Totem link {i + 1}", config.linkMaterial);
                link.transform.localPosition = (start + end) * 0.5f;
                link.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                link.transform.localScale = new Vector3(direction.magnitude, LinkWidth, 1f);
                _links[i] = link;
            }
        }

        public Bounds ShieldBounds => _shield.bounds;

        public void Show() => _root.SetActive(true);
        public void SetShieldVisible(bool visible) => _shield.enabled = visible;
        public void SetLinkActive(int index, bool active) => _links[index].enabled = active;

        public void FlashShield()
        {
            _flashTimeLeft = FlashDuration;
            UpdateShieldProperties();
        }

        public void Tick(float deltaTime)
        {
            _flowTime += deltaTime;
            _flashTimeLeft = Mathf.Max(0f, _flashTimeLeft - deltaTime);

            UpdateShieldProperties();

            _properties.SetFloat(HitFlashId, 0f);
            for (var i = 0; i < _links.Length; i++)
            {
                if (!_links[i].enabled) continue;
                _properties.SetFloat(FlowTimeId, _flowTime + i * 0.31f);
                _links[i].SetPropertyBlock(_properties);
            }
        }

        public void Dispose()
        {
            _root.SetActive(false);
            Object.Destroy(_root);
            Object.Destroy(_quad);
        }

        private void UpdateShieldProperties()
        {
            _properties.SetFloat(FlowTimeId, _flowTime);
            _properties.SetFloat(HitFlashId, _flashTimeLeft / FlashDuration);
            _shield.SetPropertyBlock(_properties);
        }

        private MeshRenderer CreateSurface(string name, Material material)
        {
            var child = new GameObject(name);
            child.transform.SetParent(_root.transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = _quad;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private static Mesh CreateQuad()
        {
            return new Mesh
            {
                name = "Boss effect surface",
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f), new Vector3(-0.5f, 0.5f),
                    new Vector3(0.5f, 0.5f), new Vector3(0.5f, -0.5f)
                },
                uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right },
                triangles = new[] { 0, 1, 2, 0, 2, 3 }
            };
        }
    }
}
