using System.Windows;

namespace MagnetarCA.Utils.WpfUtils
{
    public abstract class ModifierBase
    {
        public abstract void Apply(DependencyObject target);
    }
}
