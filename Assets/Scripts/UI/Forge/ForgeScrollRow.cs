using System;
using Core.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Forge
{
    public sealed class ForgeScrollRow : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _frame;
        [SerializeField] private Sprite _selectedFrame;
        [SerializeField] private Sprite _normalFrame;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _count;
        private string _id;
        private Action<string> _select;
        private void Awake() => _button.onClick.AddListener(() => _select?.Invoke(_id));
        public void Bind(ItemDefinition definition, int count, bool selected, bool interactable, Action<string> select)
        {
            _id = definition.itemId;
            _select = select;
            _frame.sprite = selected ? _selectedFrame : _normalFrame;
            _icon.sprite = definition.icon;
            _count.text = "×" + count;
            _button.interactable = interactable;
        }
    }
}
