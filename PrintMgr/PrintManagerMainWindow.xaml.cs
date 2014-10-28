using System;
using System.Windows;
using System.Windows.Controls;
using SolidWorks.Interop.sldworks;

namespace PrintMgr
{
    public partial class PrintManagerMainWindow : Window
    {
        public PrintManagerMainWindow(PrintManager aPrintManager)
        {
            InitializeComponent();

            _printManager = aPrintManager;
        }

        //Data
        private readonly PrintManager _printManager;
        
        //Events
        private void ButtonPrintAll_Click(object sender, RoutedEventArgs e)
        {
            _printManager.PrintAll();
        }

        private void ButtonAddOpenDocks_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _printManager.AddOpenDocs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            ((Button) sender).IsEnabled = false;
        }
    }
}
