using System;
using TMPro;
using UnityEngine.Localization;

namespace UI.Localization
{
    public sealed class LocalizedStringBinding : IDisposable
    {
        private readonly TMP_Text _text;
        private readonly LocalizedString.ChangeHandler _changeHandler;
        private LocalizedString _localizedString;

        public LocalizedStringBinding(TMP_Text text)
        {
            _text = text;
            _changeHandler = UpdateText;
        }

        public void Bind(LocalizedString localizedString)
        {
            if (ReferenceEquals(_localizedString, localizedString))
            {
                Refresh();
                return;
            }

            Clear();
            _localizedString = localizedString;

            if (_text == null)
                return;

            if (_localizedString == null || _localizedString.IsEmpty)
            {
                _text.text = string.Empty;
                return;
            }

            _localizedString.StringChanged += _changeHandler;
        }

        public void Refresh()
        {
            if (_localizedString == null || _localizedString.IsEmpty)
                return;

            _localizedString.RefreshString();
        }

        public void Clear()
        {
            if (_localizedString != null)
                _localizedString.StringChanged -= _changeHandler;

            _localizedString = null;

            if (_text != null)
                _text.text = string.Empty;
        }

        public void Dispose()
        {
            Clear();
        }

        private void UpdateText(string value)
        {
            if (_text != null)
                _text.text = value;
        }
    }
}
