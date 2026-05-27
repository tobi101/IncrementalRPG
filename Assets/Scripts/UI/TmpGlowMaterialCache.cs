using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI
{
    internal static class TmpGlowMaterialCache
    {
        private const string RuntimeGlowName = "_RuntimeGlow";

        private static readonly Dictionary<CacheKey, Material> Materials = new();

        public static Material Get(Material sourceMaterial, TmpGlowStyle style)
        {
            if (sourceMaterial == null || style == null)
                return null;

            if (!TmpGlowStyle.SupportsGlow(sourceMaterial))
                return null;

            var key = new CacheKey(sourceMaterial.GetInstanceID(), style.GetInstanceID());

            if (!Materials.TryGetValue(key, out var material) || material == null)
            {
                material = new Material(sourceMaterial)
                {
                    name = $"{sourceMaterial.name}_{style.name}{RuntimeGlowName}",
                    hideFlags = HideFlags.DontSave
                };

                Materials[key] = material;
            }

            style.ApplyTo(material);
            return material;
        }

        public static bool IsRuntimeGlowMaterial(Material material)
        {
            return material != null && material.name.Contains(RuntimeGlowName);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            foreach (var material in Materials.Values)
            {
                if (material == null)
                    continue;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Object.DestroyImmediate(material);
                else
#endif
                    Object.Destroy(material);
            }

            Materials.Clear();
        }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            private readonly int _sourceMaterialId;
            private readonly int _styleId;

            public CacheKey(int sourceMaterialId, int styleId)
            {
                _sourceMaterialId = sourceMaterialId;
                _styleId = styleId;
            }

            public bool Equals(CacheKey other)
            {
                return _sourceMaterialId == other._sourceMaterialId && _styleId == other._styleId;
            }

            public override bool Equals(object obj)
            {
                return obj is CacheKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_sourceMaterialId * 397) ^ _styleId;
                }
            }
        }
    }
}
