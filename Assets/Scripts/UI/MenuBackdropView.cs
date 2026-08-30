using UnityEngine;

namespace UI
{
    public sealed class MenuBackdropView : MonoBehaviour
    {
        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
