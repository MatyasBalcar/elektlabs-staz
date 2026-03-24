using Knihovna.Properties;
using System.ComponentModel;
using System.Globalization;

namespace Knihovna.Localization
{
    public sealed class LocalizationManager : INotifyPropertyChanged
    {
        private static readonly LocalizationManager _instance = new();

        public static LocalizationManager Instance => _instance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key] => Resources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

        private LocalizationManager()
        {
        }

        public void NotifyLanguageChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}
