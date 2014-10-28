using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;


namespace PrintMgr
{
    public class SheetSettings
    {
        public SheetSettings(ModelDoc2 aModelDoc ,Sheet aSheet, int aSheetNumber)
        {
            _modelDoc = aModelDoc;
            _sheet = aSheet;
            _sheetNumber = aSheetNumber + 1;
            double widht = 0;
            double height = 0;
            _sheet.GetSize(ref widht, ref height);

            Printable = true;

            _width = (int)(widht * 1000);//convert to mm
            _height = (int)(height*1000);

            if (_height > _width)
            {
                PageOrientation = swPageSetupOrientation_e.swPageSetupOrient_Portrait;

                int temp = _height;
                _height = _width;
                _width = temp;
            }
            else
            {
                PageOrientation = swPageSetupOrientation_e.swPageSetupOrient_Landscape;
            }
            try
            {
                _printer = "";
                PrintMgrSettings.GetSheetSettings(_height, _width, out _printer, out _sheetKind, out _scale, out _formatName);
            }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            
            _sheetName = _sheet.GetName();

            _availablePrinters = new BindingList<string>();
            
            SetControls();

            _comboBoxPrinter.Text = PrintMgrSettings.Printers.ContainsKey(_printer)
                ? _printer
                : PrintMgrSettings.DefaultPrinterName;
        }

        
        //Data
        private readonly ModelDoc2 _modelDoc;
        private Sheet _sheet;
        
        public bool Printable { get; private set; }

        private readonly string _sheetName;
        private readonly int _sheetNumber;

        private readonly string _formatName;
        private readonly int _height;
        private readonly int _width;

        private readonly PaperKind _sheetKind;

        private swPageSetupOrientation_e PageOrientation { get; set; }
        
        private int _scale;

        private readonly string _printer;
        
        private readonly BindingList<string> _availablePrinters;

        //Controls

        public Grid GridSheet { get; private set; }
        private CheckBox _checkBoxPrintable;
        private Label _labelName;
        private Label _labelFormat;
        private Image _imageOrientation;
        private ComboBox _comboBoxPrinter;
        private TextBox _textBoxScale;
        private CheckBox _checkBoxFit;
        //private Image _imageStatus;
        private Button _buttonPrintSheet;

        private ComboBox _comboBoxPrinterPaperSizes;

        //Methods

        private void SetControls()
        {
            const int step = 5;
            const int elementHeight = 22;
            GridSheet = new Grid
            {
                Margin = new Thickness(0, 0, 0, 1),
                Height = 30,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.LightSteelBlue
            };

            //CheckBox - print sheet or not
            _checkBoxPrintable = new CheckBox
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 15,
                IsChecked = true
            };
            _checkBoxPrintable.Checked += CheckBoxPrintable_Checked;
            _checkBoxPrintable.Unchecked += CheckBoxPrintable_Unchecked;
            GridSheet.Children.Add(_checkBoxPrintable);

            //Label - sheet name
            _labelName = new Label
            {
                Content = _sheetName,
                Margin = new Thickness(_checkBoxPrintable.Margin.Left + _checkBoxPrintable.Width + step, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 50,
                Background = Brushes.White
            };
            GridSheet.Children.Add(_labelName);

            //Label - format name
            _labelFormat = new Label
            {
                Content = _formatName + "(" + _height + "x" + _width + ")",
                Margin = new Thickness(_labelName.Margin.Left + _labelName.Width + step, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 100,
                Background = Brushes.White
            };
            GridSheet.Children.Add(_labelFormat);

            //Image - sheet orientation
            _imageOrientation = new Image
            {
                Margin = new Thickness(_labelFormat.Margin.Left + _labelFormat.Width + step, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 25,
                Width = 25,
            };

            var orientation = new BitmapImage();
            orientation.BeginInit();
            orientation.UriSource = PageOrientation == swPageSetupOrientation_e.swPageSetupOrient_Portrait 
                ? new Uri("img/img_portrait.png", UriKind.Relative) 
                : new Uri("img/img_landscape.png", UriKind.Relative);
            orientation.EndInit();
            _imageOrientation.Source = orientation;
            GridSheet.Children.Add(_imageOrientation);

            //ComboBox - available printers
            _comboBoxPrinter = new ComboBox
            {
                Margin = new Thickness(_imageOrientation.Margin.Left + _imageOrientation.Width + step, 0, 0, 0),
                Height = elementHeight,
                Width = 200,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = _availablePrinters,
            };
            _comboBoxPrinter.ItemsSource = PrintMgrSettings.Printers;
            _comboBoxPrinter.DisplayMemberPath = "Key";
            _comboBoxPrinter.SelectedValuePath = "Value";
            _comboBoxPrinter.SelectionChanged += ComboBoxPrinter_SelectionChanged;
            
            GridSheet.Children.Add(_comboBoxPrinter);

            //ComboBox - available paper sizes
            _comboBoxPrinterPaperSizes = new ComboBox
            {
                Margin = new Thickness(_comboBoxPrinter.Margin.Left + _comboBoxPrinter.Width + step, 0, 0, 0),
                Height = elementHeight,
                Width = 130,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                ItemsSource = PrintMgrSettings.GetAvailablePapersizes(_printer),
                DisplayMemberPath = "PaperName"
            };

            foreach (PaperSize ps in _comboBoxPrinterPaperSizes.Items)
            {
                if (ps.Kind == _sheetKind)
                {
                    _comboBoxPrinterPaperSizes.SelectedItem = ps;
                    break;
                }
            }

            GridSheet.Children.Add(_comboBoxPrinterPaperSizes);

            //TextBox - printing scale
            _textBoxScale = new TextBox
            {
                Margin = new Thickness(_comboBoxPrinterPaperSizes.Margin.Left + _comboBoxPrinterPaperSizes.Width + step, 0, 0, 0),
                Height = elementHeight,
                Width = 35,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = _scale.ToString(),
                IsEnabled = _scale != 0
            };
            _textBoxScale.KeyDown += TextBoxScaleOnKeyDown;
            _textBoxScale.TextChanged += TextBoxScaleOnTextChanged;
            GridSheet.Children.Add(_textBoxScale);

            //CheckBox - fit sheet to paper size
            _checkBoxFit = new CheckBox
            {
                Margin = new Thickness(_textBoxScale.Margin.Left + _textBoxScale.Width + step, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "Fit",
                IsChecked = _scale == 0,
                Width = 30,
                ToolTip = "В размер страницы"
            };
            _checkBoxFit.Checked += CheckBoxFit_Checked;
            _checkBoxFit.Unchecked += CheckBoxFit_UnChecked;
            GridSheet.Children.Add(_checkBoxFit);

            _buttonPrintSheet = new Button
            {
                Margin = new Thickness(_checkBoxFit.Margin.Left + _checkBoxFit.Width + step, 0, 0, 0),
                Height = elementHeight,
                Width = 30,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = "Print"
            };
            _buttonPrintSheet.Click += ButtonPrintSheet_Click;
            GridSheet.Children.Add(_buttonPrintSheet);
        }
        
        public void AddPrinter(string aPrinter)
        {
            _availablePrinters.Add(aPrinter);
        }

        public void PrintSheet()
        {
            try
            {
                var pageSetup = (PageSetup)_modelDoc.PageSetup;
                pageSetup.Orientation = (int)PageOrientation;
                pageSetup.ScaleToFit = _scale == 0;
                
                

                if (_scale == 0)
                {
                    pageSetup.ScaleToFit = true;
                }
                else
                {
                    pageSetup.ScaleToFit = false;
                    pageSetup.Scale2 = _scale;
                }
                pageSetup.PrinterPaperSize = (int) _sheetKind;
                int[] s = {_sheetNumber};
                _modelDoc.Extension.PrintOut3(s, 1, true, _printer, "", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Events
        //_checkBoxFit
        private void CheckBoxFit_Checked(object sender, RoutedEventArgs e)
        {
            _scale = 0;
            _textBoxScale.Text = "0";
            _textBoxScale.IsEnabled = false;
        }
        private void CheckBoxFit_UnChecked(object sender, RoutedEventArgs e)
        {
            _scale = 100;
            _textBoxScale.Text = "100";
            _textBoxScale.IsEnabled = true;
        }

        //_checkBoxPrintable
        private void CheckBoxPrintable_Checked(object sender, RoutedEventArgs e)
        {
            Printable = true;
        }
        private void CheckBoxPrintable_Unchecked(object sender, RoutedEventArgs e)
        {
            Printable = false;
        }

        //_comboBoxPrinter
        private void ComboBoxPrinter_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var s = ((ComboBox) sender).SelectedValue;
            _comboBoxPrinterPaperSizes.ItemsSource = (List<PaperSize>)s;
        }

        //_buttonPrintSheet
        private void ButtonPrintSheet_Click(object sender, RoutedEventArgs e)
        {
            PrintSheet();
        }

        //_textBoxScale
        private void TextBoxScaleOnTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            int scale;
            try
            {
                scale = Convert.ToInt32(((TextBox)sender).Text);
            }
            catch (Exception ex)
            {
                ((TextBox)sender).Text = "100";
                ((TextBox)sender).SelectAll();
                MessageBox.Show(ex.Message);
                return;
            }

            if (scale > 1000)
            {
                ((TextBox)sender).Text = "1000";
                ((TextBox)sender).SelectAll();
                return;
            }
            _scale = scale;
        }
        private static void TextBoxScaleOnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }

        }

    }

    
}
