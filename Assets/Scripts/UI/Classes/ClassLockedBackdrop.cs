using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassLockedBackdrop : MonoBehaviour
    {
        public Transform foreground;
        public Shader blurShader;
        private Material _blur;
        private readonly Dictionary<Image, Material> _original = new();

        public void SetLocked(bool locked)
        {
            Restore();
            if (!locked || blurShader == null) return;
            if (_blur == null) _blur = new Material(blurShader) { name = "Locked class blur" };
            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                if (foreground != null && image.transform.IsChildOf(foreground)) continue;
                _original[image] = image.material;
                image.material = _blur;
            }
        }
        private void Restore()
        {
            foreach (var pair in _original) if (pair.Key != null) pair.Key.material = pair.Value;
            _original.Clear();
        }
        private void OnDestroy() { Restore(); if (_blur != null) Destroy(_blur); }
    }
}
