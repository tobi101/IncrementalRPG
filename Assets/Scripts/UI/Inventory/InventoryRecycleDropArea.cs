using System;
using Core.Items;
using Model;
using Spine.Unity;
using TMPro;
using UDND.Core;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Inventory
{
    public sealed class InventoryRecycleDropArea : DropAreaBase
    {
        private Player _player;
        private Graphic _graphic;
        private SkeletonGraphic _recycleGraphic;
        private Color _normalColor;
        private Action _onRecycled;
        private TMP_Text _salePreview;

        public void Configure(Player player, Graphic graphic, SkeletonGraphic recycleGraphic, TMP_Text salePreview,
            Action onRecycled = null)
        {
            _player = player;
            _graphic = graphic;
            _recycleGraphic = recycleGraphic;
            _normalColor = graphic.color;
            _onRecycled = onRecycled;
            _salePreview = salePreview;
            RefreshSalePreview(DragManager?.CurrentContext);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UDNDEvents.OnDragStarted += RefreshSalePreview;
            UDNDEvents.OnDragStackChanged += RefreshSalePreview;
            UDNDEvents.OnDragEnded += HideSalePreview;
            RefreshSalePreview(DragManager?.CurrentContext);
        }

        protected override void OnDisable()
        {
            UDNDEvents.OnDragStarted -= RefreshSalePreview;
            UDNDEvents.OnDragStackChanged -= RefreshSalePreview;
            UDNDEvents.OnDragEnded -= HideSalePreview;
            HideSalePreview();
            base.OnDisable();
        }

        private void RefreshSalePreview(DragContext context)
        {
            if (_salePreview == null)
                return;

            if (!CanAcceptDrop(context))
            {
                HideSalePreview();
                return;
            }

            var salePrice = BigDouble.Zero;
            foreach (var entry in context.Entries)
                salePrice += GetSalePrice(entry.Stack);

            _salePreview.text = $"<sprite name=\"gold\"> +{BigDoubleFormatter.Format(salePrice)}";
            _salePreview.gameObject.SetActive(true);
        }

        private void HideSalePreview()
        {
            if (_salePreview != null)
                _salePreview.gameObject.SetActive(false);
        }

        public override bool CanAcceptDrop(DragContext context)
        {
            if (!base.CanAcceptDrop(context))
                return false;

            foreach (var entry in context.Entries)
                if (entry.SourceInventory is EquipmentDragInventory)
                    return context.Entries.Count == 1;

            return true;
        }

        public override DropResult ProcessDrop(DragContext context)
        {
            if (!CanAcceptDrop(context))
                return DropResult.Failed("Invalid recycle drop");

            DropResult result;
            var entry = context.Entries[0];
            if (entry.SourceInventory is EquipmentDragInventory equipment)
            {
                var adapter = (GameItemAdapter)entry.Stack.PrimaryAdapter;
                if (!equipment.TryRecycle(adapter))
                    return DropResult.Failed("Equipped item is no longer available");

                OnProcessedEntry(entry.Stack, entry);
                result = DropResult.Succeeded(adapter, 1);
            }
            else
            {
                result = base.ProcessDrop(context);
            }

            if (result.Success)
            {
                PlayRecycleAnimation();
                _onRecycled?.Invoke();
            }

            return result;
        }

        protected override bool CanAcceptEntry(DragEntry entry)
        {
            if (entry.Stack?.PrimaryAdapter is not GameItemAdapter adapter)
                return false;

            return entry.SourceInventory is not EquipmentDragInventory equipment ||
                   entry.Stack.Count == 1 && equipment.IsCurrentItem(adapter);
        }

        protected override void OnProcessedEntry(ItemStack stack, DragEntry entry)
        {
            _player.GoldTotal += GetSalePrice(stack);
        }

        private static BigDouble GetSalePrice(ItemStack stack)
        {
            var salePrice = BigDouble.Zero;
            foreach (var adapter in stack.Adapters)
                salePrice += ((GameItemAdapter)adapter).SellPrice;

            return salePrice;
        }

        private void PlayRecycleAnimation()
        {
            if (_recycleGraphic == null)
                return;

            _recycleGraphic.Initialize(false);
            var animationState = _recycleGraphic.AnimationState;
            if (animationState == null)
                return;

            animationState.SetAnimation(0, "recycle", false).MixDuration = 0f;
            animationState.AddAnimation(0, "idle", true, 0f).SetMixDuration(0f, 0f);
        }

        protected override void OnHighlightChanged(bool highlighted, bool canAccept)
        {
            if (_graphic == null)
                return;

            _graphic.color = highlighted && canAccept
                ? new Color(1f, 0.45f, 0.2f, _normalColor.a)
                : _normalColor;
        }
    }
}
