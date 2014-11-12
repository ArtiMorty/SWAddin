using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace PropertiesMgr
{
    public class Names
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="aStackPanelNames"></param>
        public Names(PropertiesManager aPropertiesManager, PropertiesManagerWindow aPropertiesManagerWindow)
        {
            _propertiesManager = aPropertiesManager;
            
            ReadUsersNamesXml();

            GetCurretNames();

            InitializeNameControls(aPropertiesManagerWindow.StackPanelNames);

            //Date
            _datePicker = aPropertiesManagerWindow.DatePickerCreationDate;
            
            _checkBoxDate = aPropertiesManagerWindow.CheckBoxDate;
            _checkBoxDate.Checked += CheckBoxDate_Checked;
            _checkBoxDate.Unchecked += CheckBoxDate_Unhecked;

            if (aPropertiesManager.ContainsProperty("Дата"))
            {
                _checkBoxDate.IsChecked = true;
                try
                {
                    _datePicker.SelectedDate = DateTime.Parse(aPropertiesManager.GetPropertyValue("Дата"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    _datePicker.SelectedDate = DateTime.Today;
                }
            }

            //Format
            _comboBoxFormat = aPropertiesManagerWindow.ComboBoxFormat;
            var listFormats = new List<string> { "БЧ", "А4", "А3", "А2", "А1", "А0" };
            _comboBoxFormat.ItemsSource = listFormats;
            if (_propertiesManager.ContainsProperty("Формат"))
            {
                string currentFormat = _propertiesManager.GetPropertyValue("Формат");
                if (!listFormats.Contains(currentFormat))
                {
                    listFormats.Add(currentFormat);
                }
                
                _comboBoxFormat.Text = currentFormat;
            }

        }

        //Data
        private readonly PropertiesManager _propertiesManager;

        enum Developers
        {
            Razrabotal = 0,
            Proveril,
            Tcontr,
            UsersField,
            Ncontr,
            Utverdil,
            Company
        }

        private static readonly string[] Devs =
        {
            "Разработал",
            "Проверил",
            "Тконтр",
            "",
            "Нконтр",
            "Утв.",
            "Организация"
        };

        private static readonly string[] XmlTags =
        {
            "razrab",
            "prov",
            "tcontr",
            "other",
            "ncontr",
            "utv",
            "org"
        };

        private ComboBox[] _comboBoxsNames;

        private Dictionary<Developers, BindingList<string>> _usersNamesSetDictionary;
        private string[] _currentNames;

        private readonly DatePicker _datePicker;
        private readonly CheckBox _checkBoxDate;

        private readonly ComboBox _comboBoxFormat;
        //Methods
        private void InitializeNameControls(StackPanel aStackPanelNames)
        {
            _comboBoxsNames = new ComboBox[Devs.Length];

            for (int i = 0; i < Devs.Length; i++)
            {
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(5,0,5,0),
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Orientation = Orientation.Horizontal,
                };

                var label = new Label
                {
                    Content = Devs[i] + ":",
                };

                var comboBox = new ComboBox
                {
                    Width = 100,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsEditable = true,
                    ItemsSource = _usersNamesSetDictionary[(Developers)i]
                };

                if (_currentNames[i] != "")
                {
                    if (!_usersNamesSetDictionary[(Developers) i].Contains(_currentNames[i]))
                    {
                        _usersNamesSetDictionary[(Developers)i].Add(_currentNames[i]);
                    }
                    comboBox.SelectedValue = _currentNames[i];
                }


                stackPanel.Children.Add(label);
                stackPanel.Children.Add(comboBox);
                
                _comboBoxsNames[i] = comboBox;
                aStackPanelNames.Children.Add(stackPanel);
            }
        }

        private void GetCurretNames()
        {
            _currentNames = new string[Devs.Length];
            for (int i = 0; i < Devs.Length; ++i)
            {
                _currentNames[i] = _propertiesManager.GetPropertyValue(Devs[i]);
            }
        }

        private void ReadUsersNamesXml()
        {
            _usersNamesSetDictionary = new Dictionary<Developers, BindingList<string>>();

            XDocument xNames = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\XML\names.xml");


            for (int i = 0; i < Devs.Length; i++)
            {
                _usersNamesSetDictionary.Add((Developers)i, new BindingList<string>());
                XElement xel = xNames.Root.Element(XmlTags[i]);
                foreach (var el in xel.Elements())
                {
                    _usersNamesSetDictionary[(Developers)i].Add(el.Value);
                }
            }
        }

        public void Save()
        {
            for (int i = 0; i < Devs.Length; ++i)
            {
                _propertiesManager.SaveProperty(Devs[i], _comboBoxsNames[i].Text);
            }
            _propertiesManager.SaveProperty("Дата", 
                _datePicker.SelectedDate != null 
                ? _datePicker.SelectedDate.Value.ToShortDateString() 
                : "");
            _propertiesManager.SaveProperty("Формат", _comboBoxFormat.Text);
        }

        //events

        private void CheckBoxDate_Checked(object sender, EventArgs e)
        {
            _datePicker.IsEnabled = true;
            _datePicker.SelectedDate = DateTime.Today;
        }

        private void CheckBoxDate_Unhecked(object sender, EventArgs e)
        {
            _datePicker.IsEnabled = false;
            _datePicker.SelectedDate = null;

        }
    }
}
