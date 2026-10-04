using Core.Items;
using TMPro;
using UnityEngine;

namespace UI.Classes
{
    public sealed class ClassLocalizedLabel : MonoBehaviour
    {
        public string key;
        public void Refresh() => GetComponent<TMP_Text>().text = ItemText.Get(key);
    }
}
