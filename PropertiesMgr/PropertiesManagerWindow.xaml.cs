using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace PropertiesMgr
{
    /// <summary>
    /// Interaction logic for PropertiesManagerWindow.xaml
    /// </summary>
    public partial class PropertiesManagerWindow : Window
    {
        public PropertiesManagerWindow(PropertiesManager aPropertiesManager)
        {
            InitializeComponent();

            _propertiesManager = aPropertiesManager;

            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd.MM.yy";
            Thread.CurrentThread.CurrentCulture = ci;

            DatePickerCreationDate.SelectedDateFormat = DatePickerFormat.Short;
        }

        private readonly PropertiesManager _propertiesManager;

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _propertiesManager.Save();
            Close();
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            _propertiesManager.Save();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            _propertiesManager.InitComponents();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _propertiesManager.AddConfiguration();
        }
    }
}
