using System;
using Core.Classes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassNodeView : MonoBehaviour
    {
        public Button button;
        public Image icon;
        public Image highlight;
        public GameObject lockIcon;
        public TMP_Text levelText;
        public TMP_Text nameText;
        public ClassNodeDefinition Definition { get; private set; }
        public void Bind(ClassNodeDefinition node, Action<ClassNodeDefinition> select)
        {
            Definition = node;
            icon.sprite = node.icon;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => select(node));
            UIButtonAudio.EnsureOn(button, true);
        }
        public void Refresh(ClassDefinition owner, ClassProgressionService service, bool selected)
        {
            var status = service.PurchaseStatus(owner, Definition);
            var locked = status is ClassPurchaseStatus.ClassLocked or ClassPurchaseStatus.RequirementsMissing;
            lockIcon.SetActive(locked);
            icon.enabled = !locked;
            icon.color = Color.white;
            highlight.color = selected ? owner.color : new Color(owner.color.r, owner.color.g, owner.color.b,
                status == ClassPurchaseStatus.Available ? 0.3f : 0.05f);
            levelText.text = $"{service.Level(owner, Definition)} / {Definition.LevelLimit}";
            levelText.gameObject.SetActive(!locked && Definition.LevelLimit > 1);
            nameText.text = Definition.displayName.GetLocalizedString();
        }
    }
}
