using System;
using Core.Classes;
using Core.TestSkillTree;
using Core.TestSkillTree.View;
using IncrementalRPG.Scripts.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Button button;
        public Image highlight;
        public NodeView nodeView;
        public ClassNodeDefinition Definition { get; private set; }
        private int _level;
        private NodeState _state;
        private Action<ClassNodeDefinition> _hover;
        private Action<ClassNodeDefinition> _exit;

        public void Bind(ClassNodeDefinition node, ClassDefinition owner, ClassProgressionService service,
            NodeCircleSpriteConfig circles, AudioManager audio, Action<ClassNodeDefinition> purchase,
            Action<ClassNodeDefinition> hover, Action<ClassNodeDefinition> exit)
        {
            Definition = node;
            _hover = hover;
            _exit = exit;
            _level = service.Level(owner, node);
            _state = VisualState(service.PurchaseStatus(owner, node));
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => purchase(node));
            UIButtonAudio.EnsureOn(button, true, false);
            nodeView.Bind(node, _state, _level, circles, audio, () => button.onClick.Invoke());
        }

        public void OnPointerEnter(PointerEventData eventData) => _hover?.Invoke(Definition);
        public void OnPointerExit(PointerEventData eventData) => _exit?.Invoke(Definition);
        private void OnDisable() => _exit?.Invoke(Definition);

        public void Refresh(ClassDefinition owner, ClassProgressionService service, bool selected)
        {
            var status = service.PurchaseStatus(owner, Definition);
            _state = VisualState(status);
            nodeView.Refresh(_state);
            var level = service.Level(owner, Definition);
            if (level != _level)
            {
                nodeView.PlayLevelUpgrade(level);
                _level = level;
            }
            highlight.color = selected ? owner.color : new Color(owner.color.r, owner.color.g, owner.color.b,
                status == ClassPurchaseStatus.Available ? 0.3f : 0.05f);
        }

        private static NodeState VisualState(ClassPurchaseStatus status) => status switch
        {
            ClassPurchaseStatus.Available => NodeState.Affordable,
            ClassPurchaseStatus.NotEnoughGold => NodeState.Unaffordable,
            ClassPurchaseStatus.Complete => NodeState.Complete,
            _ => NodeState.Locked
        };
    }
}
