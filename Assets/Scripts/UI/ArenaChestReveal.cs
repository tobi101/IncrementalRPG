using Entity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class ArenaChestReveal : MonoBehaviour
    {
        private SkeletonGraphic _chest;
        private Button _button;
        private SpawnBurnReveal _totemReveal;
        private Material _originalMaterial;
        private Material _burnMaterial;
        private float _elapsed;

        internal void Begin(SkeletonGraphic chest, Button button, SpawnBurnReveal totemReveal)
        {
            _chest = chest;
            _button = button;
            _totemReveal = totemReveal;
            _originalMaterial = chest.material;
            _burnMaterial = new Material(totemReveal.GetComponent<MeshRenderer>().sharedMaterial)
            {
                mainTexture = chest.mainTexture
            };

            // SkeletonGraphic scales Spine vertices into canvas units before the shader sees them.
            var meshScale = chest.MeshScale;
            _burnMaterial.SetTextureScale("_FadeTex", _burnMaterial.GetTextureScale("_FadeTex") / meshScale);
            _burnMaterial.SetTextureScale("_FadeBurnTex", _burnMaterial.GetTextureScale("_FadeBurnTex") / meshScale);
            _burnMaterial.SetFloat(SpawnBurnReveal.FadeAmount, totemReveal.GetFadeAmount(0f));
            chest.material = _burnMaterial;
        }

        private void Update()
        {
            if (!_button.IsInteractable() || !Application.isFocused)
                return;

            _elapsed += Time.deltaTime;
            _burnMaterial.SetFloat(SpawnBurnReveal.FadeAmount, _totemReveal.GetFadeAmount(_elapsed));
            if (_elapsed >= _totemReveal.Duration)
            {
                _chest.material = _originalMaterial;
                Destroy(this);
            }
        }

        private void OnDestroy() => Destroy(_burnMaterial);
    }
}
