using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        }

        private PropertiesManager _propertiesManager;

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
    }
}
