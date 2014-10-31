using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System.ComponentModel;

namespace PropertiesMgr
{
    public class MaterialsAndMass
    {
        public MaterialsAndMass(PropertiesManager aPropertiesManager, PropertiesManagerWindow aWindow)
        {

            _propertiesManager = aPropertiesManager;
            _swModelDocExt = aPropertiesManager.SwModelDocExtension;
            
            _comboBoxMaterial = aWindow.ComboBoxMaterial;
            _comboBoxMaterial.SelectionChanged += cmbxMarerial_SelectedIndexChanged;
            
            _comboBoxProkatName = aWindow.ComboBoxProkatName;
            _comboBoxProkatName.SelectionChanged += cmbxSortamentName_SelectedIndexChanged;

            _textBoxProkatStandart = aWindow.TextBoxProkatStandart;
            _textBoxProkatStandart.TextWrapping = TextWrapping.NoWrap;
            
            _textBoxProkatSize = aWindow.TextBoxProkatSize;
            _textBoxProkatSize.TextWrapping = TextWrapping.NoWrap;
            
            _checkBoxSortament = aWindow.CheckBoxSortament;
            _checkBoxSortament.Checked += checkBoxSortament_Checked;
            _checkBoxSortament.Unchecked += checkBoxSortament_Unchecked;

            _labelDensity = aWindow.LabelDensity;
            _lebelMass = aWindow.LabelMass;

            _checkBoxNoMass = aWindow.CheckBoxNoMass;
            _checkBoxNoMass.Checked += checkBoxNoMass_Checked;
            _checkBoxNoMass.Unchecked += checkBoxNoMass_Unchecked;

            _checkBoxMassInTable = aWindow.CheckBoxMassInTable;
            _checkBoxMassInTable.Checked += checkBoxMassInTable_Checked;
            _checkBoxMassInTable.Unchecked += checkBoxMassInTable_Unchecked;

            if (_propertiesManager.Section == "Сборочные единицы")
            {
                _checkBoxSortament.IsEnabled = false;
                _comboBoxProkatName.IsEnabled = false;
                _textBoxProkatSize.IsEnabled = false;
                _textBoxProkatStandart.IsEnabled = false;
                _comboBoxMaterial.IsEnabled = false;
            }

            _currentSortament = GetCurrentSortament();
            _currentMaterial = GetCurrentMaterial();

            SortamentList = new BindingList<Sortament>();
            MaterialList = new BindingList<Material>();

            _comboBoxProkatName.ItemsSource = SortamentList;
            _comboBoxProkatName.DisplayMemberPath = "Tip";
            _comboBoxProkatName.SelectedValuePath = "Name";

            _comboBoxMaterial.ItemsSource = MaterialList;
            _comboBoxMaterial.DisplayMemberPath = "Name";
            _comboBoxMaterial.SelectedValuePath = "Density";

            ReadMaterialXml(MaterialList);
            AddCurrentMaterialToList(MaterialList);
            ReadProkatXml(SortamentList);
            AddCurrentProkatToList(SortamentList);
            
            //Thread materialThread = new Thread(ReadMaterialXML);
            //Thread prokatThread = new Thread(ReadProkatXML);
            //materialThread.Start();
            //prokatThread.Start();
            //materialThread.Join();
            //prokatThread.Join();

            if(_currentMaterial != null) _comboBoxMaterial.Text = _currentMaterial.Name;

            if(_currentSortament != null) _textBoxProkatSize.Text = _currentSortament.Size;

            _massProp = _swModelDocExt.GetMassProperties(1, 0);
            string curMassProp = _propertiesManager.GetPropertyValue("Масса");
                

            //задать точность
            if (_massProp != null && _massProp[5] > 10)//если масса менее 10кг, точность 2 знака
            {
                _swModelDocExt.SetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsMassPropDecimalPlaces,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified,
                    0
                    );
            }
            else
            {
                _swModelDocExt.SetUserPreferenceInteger(
                    (int)swUserPreferenceIntegerValue_e.swUnitsMassPropDecimalPlaces,
                    (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified,
                    2
                    );
            }
            
            switch (curMassProp)
            {
                case "-":
                    _checkBoxNoMass.IsChecked = true;
                    //_lebelMass.Content = "-";
                    //_checkBoxMassInTable.IsEnabled = false;
                    break;
                case "см.табл.":
                    _checkBoxMassInTable.IsChecked = true;
                    //_lebelMass.Content = "см.табл.";
                    //_checkBoxNoMass.IsEnabled = false;
                    break;
                default:
                {
                    int intMassUnit = _swModelDocExt.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsMassPropMass,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified
                        );
                    int massDecimal = _swModelDocExt.GetUserPreferenceInteger(
                        (int)swUserPreferenceIntegerValue_e.swUnitsMassPropDecimalPlaces,
                        (int)swUserPreferenceOption_e.swDetailingNoOptionSpecified
                        );

                    if(_massProp != null)_lebelMass.Content = Math.Round(_massProp[5], massDecimal) + MassUnits[intMassUnit];
                }
                    break;
            }
        }
        
        #region Data

        private readonly PropertiesManager _propertiesManager;
        private readonly IModelDocExtension _swModelDocExt;
        
        //maretial
        private BindingList<Material> MaterialList { get; set; }
        private readonly ComboBox _comboBoxMaterial;
        private readonly Material _currentMaterial;
        private int _materialIndex;
        
        //prokat
        private BindingList<Sortament> SortamentList { get; set; }
        private readonly ComboBox _comboBoxProkatName;
        private readonly TextBox _textBoxProkatSize;
        private readonly TextBox _textBoxProkatStandart;
        private readonly CheckBox _checkBoxSortament;
        private readonly Sortament _currentSortament;
        private int _sortamentIndex;

        //Mass and density
        private readonly double[] _massProp;
        private static readonly string[] MassUnits = { "", "мг", "г", "кг", "pounds" };
        
        private readonly Label _labelDensity;
        private readonly Label _lebelMass;
        private readonly CheckBox _checkBoxNoMass;
        private readonly CheckBox _checkBoxMassInTable;

        #endregion

        //events mass
        //private void chckMassInTable_CheckedChanged(object sender, EventArgs e)//См. таблицу
        //{
        //    if (_checkBoxMassInTable.Checked)
        //    {
        //        _checkBoxNoMass.Enabled = false;
        //        _lblMass.Text = "см.табл.";
        //    }
        //    else
        //    {
        //        _checkBoxNoMass.Enabled = true;
        //        _lblMass.Text = Math.Round(_massProp[5], 2) + MassUnits[3];
        //    }
        //}

        //Без массы
        private void checkBoxNoMass_Checked(object sender, RoutedEventArgs e)
        {
            _checkBoxMassInTable.IsEnabled = false;
            _lebelMass.Content = "-";
        }
        private void checkBoxNoMass_Unchecked(object sender, RoutedEventArgs e)
        {
            _checkBoxMassInTable.IsEnabled = true;
            _lebelMass.Content = Math.Round(_massProp[5], 2) + MassUnits[3];
        }


        //См. таблицу
        private void checkBoxMassInTable_Checked(object sender, RoutedEventArgs e)
        {
            _checkBoxNoMass.IsEnabled = false;
            _lebelMass.Content = "см.табл.";
        }
        private void checkBoxMassInTable_Unchecked(object sender, RoutedEventArgs e)
        {
            _checkBoxNoMass.IsEnabled = true;
            _lebelMass.Content = Math.Round(_massProp[5], 2) + MassUnits[3];
        }


        //events material
        private void checkBoxSortament_Checked(object sender, EventArgs e)//Сотрамент
        {
            
            _comboBoxProkatName.IsEnabled = true;
            _textBoxProkatSize.IsEnabled = true;
            _textBoxProkatStandart.IsEnabled = true;
            
        }
        private void checkBoxSortament_Unchecked(object sender, EventArgs e)//Сотрамент
        {
            _comboBoxProkatName.SelectedIndex = -1;
            _comboBoxProkatName.IsEnabled = false;
            _textBoxProkatSize.Text = null;
            _textBoxProkatSize.IsEnabled = false;
            _textBoxProkatStandart.IsEnabled = false;
        }

        private void cmbxMarerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UpdateDensity();
            _labelDensity.Content = MaterialList[_comboBoxMaterial.SelectedIndex].Density.ToString();
        }
        private void cmbxSortamentName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _textBoxProkatStandart.Text = 
                ((ComboBox)sender).SelectedIndex == -1 
                ? "" 
                : SortamentList[_comboBoxProkatName.SelectedIndex].Standart;
        }


        //methods
        private static void ReadMaterialXml(BindingList<Material> aMaterialList)//read users materials from xml
        {
            //aMaterialList.Add(new Material("", 1000));
            XDocument xNames = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\XML\materials.xml");
            var materials = xNames.Root.Elements("material");
            foreach (XElement item in materials)
            {
                aMaterialList.Add(new Material(item.Element("name").Value, Int32.Parse(item.Element("density").Value)));
            }
        }

        private static void ReadProkatXml(BindingList<Sortament> aSortamentList)//read users prokat from xml
        {
            //aSortamentList.Add(new Sortament("", "", ""));
            XDocument xProkat = XDocument.Load(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\XML\prokat.xml");
            var prokat = xProkat.Root.Elements("prokat");
            foreach (XElement item in prokat)
            {
                string prokatName = item.Element("type").Value;
                string prokatStandart = item.Element("standart").Value;
                string prokatTip = item.Element("name").Value;

                aSortamentList.Add(new Sortament(prokatName, prokatStandart, prokatTip));
            }
        }

        private void AddCurrentProkatToList(BindingList<Sortament> aSortamentList)
        {
            _checkBoxSortament.IsChecked = true;
            if (_currentSortament != null)
            {
                _sortamentIndex = GetSortamentIndexByStandart(_currentSortament.Standart);

                if (_sortamentIndex < 0)
                {
                    aSortamentList.Add(_currentSortament);
                    _sortamentIndex = aSortamentList.Count - 1;
                    _comboBoxProkatName.SelectedIndex = _sortamentIndex;
                }
                else
                {
                    _comboBoxProkatName.SelectedIndex = _sortamentIndex;
                }
                _textBoxProkatStandart.Text = aSortamentList[_sortamentIndex].Standart;
            }
            else
            {
                _checkBoxSortament.IsChecked = false;
            }
        }

        private void AddCurrentMaterialToList(BindingList<Material> aMaterialList)
        {
            if (_currentMaterial != null)
            {
                _materialIndex = GetMaterialIndex();

                if (_materialIndex < 0)
                {
                    aMaterialList.Add(_currentMaterial);
                    int currentMaterialPosition = aMaterialList.Count - 1;
                    _comboBoxMaterial.SelectedIndex = currentMaterialPosition;
                }
                else
                {
                    _comboBoxMaterial.SelectedIndex = _materialIndex;
                }
            }
        }

        private void UpdateDensity()
        {
            _labelDensity.Content = MaterialList[_comboBoxMaterial.SelectedIndex].Density.ToString();
        }

        public void UpdateMaterialsLists()
        {
            SortamentList.Clear();
            MaterialList.Clear();
            
            ReadProkatXml(SortamentList);
            AddCurrentProkatToList(SortamentList);

            ReadMaterialXml(MaterialList);
            AddCurrentMaterialToList(MaterialList);
        }

        private int GetSortamentIndexByStandart(string aStandart)
        {
            int index = -1;
            for (int i = 0; i < SortamentList.Count; ++i)
            {
                if (SortamentList[i].Standart == aStandart)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private int GetMaterialIndex()
        {
            int index = -1;
            for (int i = 0; i < MaterialList.Count; ++i)
            {
                if (MaterialList[i].Name == _currentMaterial.Name)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private Material GetCurrentMaterial()
        {
            if (_propertiesManager.ContainsProperty("СП_материал"))
            {
                var mat = new Material();
                mat.Name = _propertiesManager.GetPropertyValue("СП_материал");
                mat.Density = _propertiesManager.Density;
                return mat;
            }
            return null;
        }
        
        private Sortament GetCurrentSortament()
        {

            if (_propertiesManager.ContainsProperty("СП_пр_стандарт"))
            {
                var sortament = new Sortament();

                sortament.Standart = _propertiesManager.GetPropertyValue("СП_пр_стандарт");

                if (_propertiesManager.ContainsProperty("СП_прокат"))
                {
                    sortament.Name = _propertiesManager.GetPropertyValue("СП_прокат");
                    sortament.Tip = sortament.Name;
                }

                if (_propertiesManager.ContainsProperty("СП_пр_размер"))
                {
                    sortament.Size = _propertiesManager.GetPropertyValue("СП_пр_размер");
                }
                return sortament;
            }
            return null;
        }

        public void Save()
        {
            #region save material
            string sortamentStandart = _textBoxProkatStandart.Text;
            int sortamentIndex = GetSortamentIndexByStandart(sortamentStandart);

            string sortamentName = sortamentIndex >= 0 ? SortamentList[sortamentIndex].Name : _comboBoxProkatName.Text;
            string sortamentFull = "";
            if (_checkBoxSortament.IsChecked.HasValue)
            {
                sortamentFull = "<FONT size=1.8> <FONT size=3>" + sortamentName +
                                " <STACK size=1>" + _textBoxProkatSize.Text + ' ' + sortamentStandart +
                                "<OVER>" +
                                _comboBoxMaterial.Text +
                                "</STACK>";
            }
            else
            {
                sortamentFull = _comboBoxMaterial.Text;
            }

            _propertiesManager.SaveProperty("СП_прокат", sortamentName);
            _propertiesManager.SaveProperty("СП_пр_размер", _textBoxProkatSize.Text);
            _propertiesManager.SaveProperty("СП_пр_стандарт", sortamentStandart);
            _propertiesManager.SaveProperty("Материал", sortamentFull);

            _propertiesManager.SaveProperty("СП_материал", _comboBoxMaterial.Text);
            #endregion

            #region save mass

            if (_checkBoxNoMass.IsChecked.HasValue)
            {
                _propertiesManager.SaveProperty("Масса", "-");
            }
            else if (_checkBoxMassInTable.IsChecked.HasValue)
            {
                _propertiesManager.SaveProperty("Масса", "см.табл.");
            }
            else
            {
                _propertiesManager.SaveProperty("Масса", "\"SW-Mass@" + _propertiesManager.ModelName + "." + _propertiesManager.FileExtension + "\"");
            }
            #endregion

        }
    }

    public class Sortament
    {
        public Sortament() { }
        public Sortament( string aName, string aStandart, string aTip )
        {
            Name = aName;
            Standart = aStandart;
            Tip = aTip;
        }

        public Sortament(string aName, string aStandart, string aTip, string aSize)
        {
            Name = aName;
            Standart = aStandart;
            Tip = aTip;
            Size = aSize;
        }

        public string Name { get; set; }
        public string Standart { get; set; }
        public string Tip { get; set; }
        public string Size { get; set; }

        
        //public static bool operator ==(Sortament s1, Sortament s2)
        //{
        //    if (s1 == null && s2 == null) return true;
        //    if (s1 != null || s2 != null) return false;
        //    return s1.Standart == s2.Standart;
        //    //return s2 != null && (s1 != null && s1.Standart == s2.Standart);
        //}

        //public static bool operator !=(Sortament s1, Sortament s2)
        //{
        //    return !(s1.Standart == s2.Standart);
        //    //return s2 != null && (s1 != null && s1.Standart != s2.Standart);
        //}
    }

    public class Material
    {
        public Material() { }
        public Material(string aName, int aDensity)
        {
            Name = aName;
            Density = aDensity;
        }
        public string Name { get; set; }
        public int Density { get; set; }
    }
}
