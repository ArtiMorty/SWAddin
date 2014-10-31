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
using SolidWorks.Interop.swconst;

namespace PropertiesMgr
{
    public class DesignationAndName
    {
        public DesignationAndName(PropertiesManager aPropertiesManager, PropertiesManagerWindow aPropertiesManagerWindow)
        {
            _propertiesManager = aPropertiesManager;
            
            aPropertiesManagerWindow.LabelDesignation.Content = aPropertiesManager.ModelName;

            _textBoxName = aPropertiesManagerWindow.TextBoxModelName;

            aPropertiesManagerWindow.LabelDesignation.Content = aPropertiesManager.ModelName;

            _textBoxName.Text = _propertiesManager.ContainsProperty("Наименование")
                ? aPropertiesManager.GetPropertyValue("Наименование")
                : "";

            ReadUsersDocumentXml();

            _comboBoxSpecSection = aPropertiesManagerWindow.ComboBoxSpecSection;
            _comboBoxSpecSection.ItemsSource = _specSections;
            
            if (_propertiesManager.ContainsProperty("Раздел"))
            {
                string curSection = _propertiesManager.GetPropertyValue("Раздел");
                if (!_specSections.Contains(curSection))
                {
                    _specSections.Add(curSection);
                }
                _comboBoxSpecSection.SelectedValue = curSection;
            }
            else
            {
                switch (_propertiesManager.SwDocumentType)
                {
                    case (swDocumentTypes_e.swDocASSEMBLY):
                        _comboBoxSpecSection.SelectedIndex = 1;
                        break;
                    default:
                        _comboBoxSpecSection.SelectedIndex = 0;
                        break;
                }
            }

            
        }

        //Data
        private readonly PropertiesManager _propertiesManager;

        private readonly TextBox _textBoxName;

        private BindingList<string> _specSections;

        private readonly ComboBox _comboBoxSpecSection;
        
        //Methods
        public void Save()
        {
            _propertiesManager.SaveProperty("Наименование", _textBoxName.Text);
            _propertiesManager.SaveProperty("Раздел", _comboBoxSpecSection.Text);
        }

        private void ReadUsersDocumentXml()
        {
            _specSections = new BindingList<string>
            {
                "Детали",
                "Сборочные единицы",
                "Стандартные изделия",
                "Прочие изделия"
            };

            try
            {
                XDocument xNames = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\XML\document.xml");

                XElement xel = xNames.Root.Element("specsection");
                foreach (var el in xel.Elements())
                {
                    _specSections.Add(el.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
