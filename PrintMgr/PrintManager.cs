using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace PrintMgr
{
    public class PrintManager
    {
        public PrintManager(ISldWorks aSldWorks)
        {
            _sldWorks = aSldWorks;

            DrwDoc = (ModelDoc2)_sldWorks.ActiveDoc;

            _pmWindow = new PrintManagerMainWindow(this);
            _sheetSettings = new List<SheetSettings>();

            _docsAndSheets = new Dictionary<ModelDoc2, List<SheetSettings>>();

            AddSheetsToMainWindow(DrwDoc);

            _openDocuments =
                ((object[]) aSldWorks.GetDocuments()).Where(
                    x =>
                        ((ModelDoc2) x).GetType() == (int) swDocumentTypes_e.swDocDRAWING &&
                        ((DrawingDoc) x) != DrwDoc);

            _pmWindow.Show();
        }

        //Data
        private ISldWorks _sldWorks;

        public static ModelDoc2 DrwDoc { get; private set; }

        private readonly PrintManagerMainWindow _pmWindow;

        private readonly List<SheetSettings> _sheetSettings;

        private readonly IEnumerable<object> _openDocuments;

        private readonly Dictionary<ModelDoc2, List<SheetSettings>> _docsAndSheets;
        
        
        //Methods
        private void AddSheetsToMainWindow(ModelDoc2 aDrawingDoc)
        {
            var sheets = (string[])((DrawingDoc)aDrawingDoc).GetSheetNames();

            _pmWindow.StackPanelSheets.Children.Add(new Label
            {
                Margin = new Thickness(0, 0, 0, 1),
                Height = 25,

                Content = (aDrawingDoc).GetPathName(),
            });

            _docsAndSheets.Add(aDrawingDoc, new List<SheetSettings>());
            
            for (int i = 0; i < sheets.Length; i++)
            {
                var shtSet = new SheetSettings(aDrawingDoc, ((DrawingDoc)aDrawingDoc).get_Sheet(sheets[i]), i);

                _sheetSettings.Add(shtSet);
                _pmWindow.StackPanelSheets.Children.Add(shtSet.GridSheet);

                _docsAndSheets[aDrawingDoc].Add(shtSet);
            }
        }

        public void AddOpenDocs()
        {
            foreach (var openDocument in _openDocuments)
            {
                AddSheetsToMainWindow((ModelDoc2)openDocument);
            }
        }

        public void PrintAll()
        {
            //for (int i = _sheetSettings.Count - 1; i >= 0; --i)
            //{
            //    if(_sheetSettings[i].Printable) _sheetSettings[i].PrintSheet();
            //}
            foreach (KeyValuePair<ModelDoc2, List<SheetSettings>> docAndSheets in _docsAndSheets)
            {
                foreach (SheetSettings sheet in docAndSheets.Value)
                {
                    if (sheet.Printable) sheet.PrintSheet();
                }
            }
        }
    }
}
