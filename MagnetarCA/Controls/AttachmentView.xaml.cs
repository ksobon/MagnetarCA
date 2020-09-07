using System.Windows;
using System.Windows.Input;

namespace MagnetarCA.Controls
{
    /// <summary>
    /// Interaction logic for AttachmentView.xaml
    /// </summary>
    public partial class AttachmentView
    {
        public AttachmentView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register("ButtonCommand",
            typeof(ICommand), typeof(AttachmentView), new UIPropertyMetadata(null));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        public static readonly DependencyProperty ButtonParameterProperty = DependencyProperty.Register("ButtonParameter",
            typeof(object), typeof(AttachmentView), new UIPropertyMetadata(null));

        public object ButtonParameter
        {
            get { return GetValue(ButtonParameterProperty); }
            set { SetValue(ButtonParameterProperty, value); }
        }
    }
}
