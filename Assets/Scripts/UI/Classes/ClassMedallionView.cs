using Core.Classes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Classes
{
    public sealed class ClassMedallionView : MonoBehaviour
    {
        public Image halo;
        public Image aura;
        public Image runes;
        public Image sigil;
        public ClassEmblemImage element;

        public void Bind(ClassDefinition definition)
        {
            halo.sprite = definition.emblemHalo;
            aura.sprite = definition.emblemAura;
            runes.sprite = definition.emblemRunes;
            sigil.sprite = definition.sigil;
            element.sprite = definition.emblem;
        }
    }
}
