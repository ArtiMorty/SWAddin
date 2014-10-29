using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PropertiesMgr
{
    public class PropertiesManager
    {
        public PropertiesManager(ISldWorks aSldWorks)
        {
            _propertiesManagerWindow = new PropertiesManagerWindow();

            _names = new Names(_propertiesManagerWindow.StackPanelNames);

            _propertiesManagerWindow.ShowDialog();
        }

        //Data
        private PropertiesManagerWindow _propertiesManagerWindow;
        private Names _names;

        //Methods
        private ModelDoc2 GetModel(ISldWorks aSldWorks)
        {
            var mod = (ModelDoc2)aSldWorks.SwApp.ActiveDoc;

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
                            mod = GetModelFromView(drawing);
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
    }

    
}
