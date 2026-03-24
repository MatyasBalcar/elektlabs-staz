using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace Knihovna.Localization
{
    [MarkupExtensionReturnType(typeof(object))]
    public class LocExtension : MarkupExtension
    {
        public LocExtension()
        {
        }

        public LocExtension(string key)
        {
            Key = key;
        }

        [ConstructorArgument("key")]
        public string Key { get; set; } = string.Empty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationManager.Instance,
                Mode = BindingMode.OneWay,
                FallbackValue = Key
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
