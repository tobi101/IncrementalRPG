using Core.Items;
using Model;
using Spine.Unity;
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

        public void Configure(Player player, Graphic graphic, SkeletonGraphic recycleGraphic)
        {
            _player = player;
            _graphic = graphic;
            _recycleGraphic = recycleGraphic;
            _normalColor = graphic.color;
        }

        public override DropResult ProcessDrop(DragContext context)
        {
            var result = base.ProcessDrop(context);
            if (result.Success)
                PlayRecycleAnimation();

            return result;
        }

        protected override bool CanAcceptEntry(DragEntry entry)
        {
            return entry.Stack?.PrimaryAdapter is GameItemAdapter;
        }

        protected override void OnProcessedEntry(ItemStack stack, DragEntry entry)
        {
            var salePrice = BigDouble.Zero;
            foreach (var adapter in stack.Adapters)
                salePrice += ((GameItemAdapter)adapter).SellPrice;

            _player.GoldTotal += salePrice;
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
            _graphic.color = highlighted && canAccept
                ? new Color(1f, 0.45f, 0.2f, _normalColor.a)
                : _normalColor;
        }
    }
}
