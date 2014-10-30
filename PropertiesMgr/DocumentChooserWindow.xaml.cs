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
using SolidWorks.Interop.sldworks;

namespace PropertiesMgr
{
    /// <summary>
    /// Interaction logic for DocumentChooserWindow.xaml
    /// </summary>
    public partial class DocumentChooserWindow : Window
    {
        public DocumentChooserWindow(List<ModelDoc2> aDocsList)
        {
            InitializeComponent();

            var names = aDocsList.Select(m => m.GetPathName()).ToList();

            ListBoxDocs.ItemsSource = names;
        }

        public int Index { get; private set; }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Index = ListBoxDocs.SelectedIndex;
            Close();
        }
    }
}
