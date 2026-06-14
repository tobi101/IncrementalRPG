using IncrementalRPG.Scripts.Core;
using UI;
using UnityEngine;
using Utils;

namespace Core.Gameplay
{
    public sealed class DamagePopupService : IService
    {
        private DamagePopupLayerView _layerView;
        private bool _missingLayerWarningLogged;

        public void Initialize()
        {
            ResolveLayerView();
            _layerView?.InitializePool();
        }

        public void Update(float deltaTime) { }

        public void ShowDamage(BigDouble amount, Vector3 worldPosition)
        {
            if (amount <= BigDouble.Zero)
                return;

            if (_layerView == null && !ResolveLayerView())
                return;

            _layerView.ShowDamage(amount, worldPosition);
        }

        private bool ResolveLayerView()
        {
            _layerView = Object.FindFirstObjectByType<DamagePopupLayerView>(FindObjectsInactive.Include);
            if (_layerView != null)
                return true;

            if (!_missingLayerWarningLogged)
            {
                Debug.LogWarning("[DamagePopupService] DamagePopupLayerView was not found in the scene. Damage popups are disabled.");
                _missingLayerWarningLogged = true;
            }

            return false;
        }
    }
}
