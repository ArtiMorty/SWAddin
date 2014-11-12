using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PropertiesMgr
{
    public class PropertiesManager
    {
        public PropertiesManager(ISldWorks aSldWorks)
        {
            _modelDoc = GetModel(aSldWorks);

            _swPropertyManager = _modelDoc.Extension.get_CustomPropertyManager("");

            var propNames = new object();
            var propTypes = new object();
            var propValues = new object();

            _swPropertyManager.GetAll(ref propNames, ref propTypes, ref propValues);

            _docUsersPropertiesDictionary = new Dictionary<string, string>();
            for (int i = 0; i < ((string[])propNames).Length; i++)
            {
                _docUsersPropertiesDictionary.Add(((string[])propNames)[i], ((string[])propValues)[i]);
            }

            GetModelName();

            _propertiesManagerWindow = new PropertiesManagerWindow(this)
            {
                LabelWrightStatus = {Content = "Доступен для записи", Background = Brushes.LawnGreen}
            };

            //Read wright acсess

            if (_modelDoc.IsOpenedReadOnly())
            {
                if (!_modelDoc.SetReadOnlyState(false))//try to get wright access
                {
                    _propertiesManagerWindow.LabelWrightStatus.Content = "Не доступен для записи";
                    _propertiesManagerWindow.LabelWrightStatus.Background = Brushes.Red;
                    _propertiesManagerWindow.ButtonSave.IsEnabled = false;
                    _propertiesManagerWindow.ButtonApply.IsEnabled = false;
                }
            }


            _propertiesManagerWindow.ShowDialog();
            
        }

        //Data
        private readonly PropertiesManagerWindow _propertiesManagerWindow;
        
        private Names _names;
        private DesignationAndName _designationAndName;
        private MaterialsAndMass _materialsAndMass;
        
        private readonly ModelDoc2 _modelDoc;

        private readonly Dictionary<string, string> _docUsersPropertiesDictionary;
        private readonly ICustomPropertyManager _swPropertyManager;

        public string ModelName { get; private set; }

        public string FileExtension
        {
            get { return SwExtension[_modelDoc.GetType()]; }
        }

        public string Section
        {
            get { return GetPropertyValue("Раздел"); }
        }

        public swDocumentTypes_e SwDocumentType
        {
            get { return (swDocumentTypes_e)_modelDoc.GetType(); }
        }

        public IModelDocExtension SwModelDocExtension
        {
            get { return _modelDoc.Extension; }
        }

        public int Density
        {
            get
            {
                return (int)_modelDoc.GetUserPreferenceDoubleValue(
                    (int)swUserPreferenceDoubleValue_e.swMaterialPropertyDensity
                    );
            }
            set
            {
                _modelDoc.SetUserPreferenceDoubleValue(
                    (int)swUserPreferenceDoubleValue_e.swMaterialPropertyDensity,
                    value
                    );
            }
        }
        
        
        //Methods
        public void InitComponents()
        {
            _names = new Names(this, _propertiesManagerWindow);
            _designationAndName = new DesignationAndName(this, _propertiesManagerWindow);
            _materialsAndMass = new MaterialsAndMass(this, _propertiesManagerWindow);

            
        }
        
        private ModelDoc2 GetModel(ISldWorks aSldWorks)
        {
            var mod = (ModelDoc2)aSldWorks.ActiveDoc;

            if ((swDocumentTypes_e)mod.GetType() == swDocumentTypes_e.swDocDRAWING)
            {
                var drawing = (DrawingDoc)mod;
                var selMgr = (SelectionMgr)mod.SelectionManager;
                var selectedType = (swSelectType_e)selMgr.GetSelectedObjectType3(1, -1);
                try
                {
                    switch (selectedType)
                    {
                        case swSelectType_e.swSelDRAWINGVIEWS:
                            mod = drawing.ActiveDrawingView.ReferencedDocument;
                            break;
                        case swSelectType_e.swSelANNOTATIONTABLES:
                            mod = GetModelFromSpec(selMgr) ?? GetModelFromDrw(drawing);
                            break;
                        default:
                            mod = GetModelFromDrw(drawing);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            return mod;
        }

        private ModelDoc2 GetModelFromDrw(IDrawingDoc aDrw)
        {
            var drawDocs = new List<ModelDoc2>();

            var views = aDrw.GetViews();

            foreach (object[] sheet in views)
            {
                foreach (object v in sheet)
                {
                    ModelDoc2 tempModel = ((View)v).ReferencedDocument;
                    if (tempModel != null && !drawDocs.Contains(tempModel))
                    {
                        drawDocs.Add(tempModel);
                    }
                }
            }

            if (drawDocs.Count == 0)
            {
                MessageBox.Show(
                    "Ошибка!",
                    "",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }

            int ind = 0;
            if (drawDocs.Count > 1)
            {
                var chooser = new DocumentChooserWindow(drawDocs);
                chooser.ShowDialog();
                ind = chooser.Index;
            }

            return drawDocs[ind];
        }

        private static readonly string[] SwExtension = { "none", "sldprt", "sldasm", "slddrw", "sldSDM" };

        /// <summary>
        /// Extracts link to a model from bom table
        /// </summary>
        /// <param name="aSelMgr"></param>
        /// <returns></returns>
        private static ModelDoc2 GetModelFromSpec(ISelectionMgr aSelMgr)
        {
            int firstRow = 0;
            int lastRow = 0;
            int firstColumn = 0;
            int lastColumn = 0;

            try
            {
                var swTableAnnotation = (TableAnnotation)aSelMgr.GetSelectedObject6(1, -1);
                swTableAnnotation.GetCellRange(ref firstRow, ref lastRow, ref firstColumn, ref lastColumn);
                var bomTable = (BomTableAnnotation)swTableAnnotation;
                Object[] currentDoc = bomTable.GetComponents(firstRow);
                return currentDoc != null ? ((Component2)currentDoc[0]).GetModelDoc2() : null;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public bool ContainsProperty(string aPropertyName)
        {
            return _docUsersPropertiesDictionary.ContainsKey(aPropertyName);
        }

        public string GetPropertyValue(string aPropertyName)
        {
            string value = "";
            if (ContainsProperty(aPropertyName)) value = _docUsersPropertiesDictionary[aPropertyName];
            return value;
        }

        public void Save()
        {
            _names.Save();
            _designationAndName.Save();
            _materialsAndMass.Save();
        }

        public void SaveProperty(string aPropertyName, string aPropertyValue)
        {
            if (_docUsersPropertiesDictionary.ContainsKey(aPropertyName))
            {
                if (string.IsNullOrEmpty(aPropertyValue))
                {
                    _swPropertyManager.Delete(aPropertyName);
                }
                else
                {
                    _swPropertyManager.Set(aPropertyName, aPropertyValue);
                }
            }
            else if(!string.IsNullOrEmpty(aPropertyValue))
            {
                _swPropertyManager.Add2(aPropertyName, (int)swCustomInfoType_e.swCustomInfoText, aPropertyValue);
            }
            
        }

        private void GetModelName()
        {
            string path = _modelDoc.GetPathName();
            if (path != "")
            {
                int dotPosition = path.LastIndexOf('.');
                int slashPosition = path.LastIndexOf('\\');
                ModelName = path.Substring(slashPosition + 1, dotPosition - slashPosition - 1);
            }
            else
            {
                ModelName = "Сохраните файл.";
            }
        }

        public void AddConfiguration()
        {
            try
            {
                int confNumber = ((string[])_modelDoc.GetConfigurationNames()).Length;
                string confName = confNumber < 10 ? "0" + confNumber : confNumber.ToString();
                string path = _modelDoc.GetPathName();
                int dotPosition = path.LastIndexOf('.');
                int slashPosition = path.LastIndexOf('\\');
                string fileName = path.Substring(slashPosition + 1, dotPosition - slashPosition - 1);
                _modelDoc.AddConfiguration3(confName, "", fileName + "-" + confName, (int)swConfigurationOptions2_e.swConfigOption_UseAlternateName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

    }

    
}
