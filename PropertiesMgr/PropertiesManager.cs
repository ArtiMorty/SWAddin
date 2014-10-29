using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PropertiesMgr
{
    public class PropertiesManager
    {
        public PropertiesManager(ISldWorks aSldWorks)
        {
            _modelDoc = GetModel(aSldWorks);

            _propertyManager = _modelDoc.Extension.get_CustomPropertyManager("");
            _docUsersProperties = ((string[]) _modelDoc.GetCustomInfoNames()).ToDictionary(
                x => x,
                x => _modelDoc.GetCustomInfoValue("", x)
                );

            _propertiesManagerWindow = new PropertiesManagerWindow();

            _names = new Names(_propertiesManagerWindow.StackPanelNames);

            _propertiesManagerWindow.ShowDialog();
        }

        //Data
        private PropertiesManagerWindow _propertiesManagerWindow;
        private Names _names;
        private ModelDoc2 _modelDoc;

        private Dictionary<string, string> _docUsersProperties;
        private ICustomPropertyManager _propertyManager;

        //Methods
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
                var chooser = new DocumentChooser(drawDocs);
                chooser.ShowDialog();
                ind = chooser.Index;
            }

            return drawDocs[ind];
        }

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
    }

    
}
