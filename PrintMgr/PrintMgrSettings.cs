using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Reflection;
using System.Xml.Linq;

namespace PrintMgr
{
    public static class PrintMgrSettings
    {
        static PrintMgrSettings()
        {
            XDocument settingsFile = XDocument.Load(Path + "SheetFormats.xml");
            Formats = new Dictionary<string, FormatSettings>();

            Printers = new Dictionary<string, List<PaperSize>>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                var tempPrinterSettings = new PrinterSettings {PrinterName = printer};
                DefaultPrinterName = tempPrinterSettings.IsDefaultPrinter ? printer : null;
                Printers.Add(printer, new List<PaperSize>());
                var pSet = new PrinterSettings { PrinterName = printer };
                foreach (PaperSize ps in pSet.PaperSizes)
                {
                    Printers[printer].Add(ps);
                }
            }

            if (settingsFile.Root != null)
            {
                var formats = settingsFile.Root.Elements("format");
                foreach (XElement format in formats)
                {
                    string formatName = format.Element("name").Value;
                    int height = Convert.ToInt32(format.Element("height").Value);
                    int width = Convert.ToInt32(format.Element("width").Value);
                    string printer = format.Element("printer").Value;
                    int scale = Convert.ToInt32(format.Element("scale").Value);
                    string s = format.Element("paperkind").Value;
                    var sheetKind = (PaperKind) (Convert.ToInt32(format.Element("paperkind").Value));
                    Formats.Add(formatName, new FormatSettings(height, width, printer, scale, sheetKind));
                }
            }
        }

        //Data
        private static readonly string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";

        private static readonly Dictionary<string, FormatSettings> Formats;

        public static readonly Dictionary<string, List<PaperSize>> Printers;

        public static readonly string DefaultPrinterName;
        
        //Methods
        public static void GetSheetSettings(int aHeight, int aWidth, out string aPrinter, out PaperKind aPaperKind,out int aScale, out string aFormatName)
        {
            foreach (KeyValuePair<string, FormatSettings> formatSettingse in Formats)
            {
                if (formatSettingse.Value.Height == aHeight && formatSettingse.Value.Width == aWidth)
                {
                    aPrinter = formatSettingse.Value.Printer;
                    aPaperKind = formatSettingse.Value.SheetKind;
                    aScale = formatSettingse.Value.Scale;
                    aFormatName = formatSettingse.Key;
                    return;
                }
            }
            aPrinter = "";
            aPaperKind = PaperKind.Custom;
            aScale = 0;
            aFormatName = "";
        }

        public static IEnumerable<PaperSize> GetAvailablePapersizes(string aPrinterName)
        {
            return Printers.ContainsKey(aPrinterName) ? Printers[aPrinterName] : null;
        }
    }

    class FormatSettings
    {
        public FormatSettings(int aHeight, int aWidth, string aPrinter, int aScale, PaperKind aPaperKind)
        {
            Height = aHeight;
            Width = aWidth;
            Printer = aPrinter;
            Scale = aScale;
            SheetKind = aPaperKind;
        }
        public int Height{ get; private set; }
        public int Width{ get; private set; }
        public string Printer{ get; private set; }
        public int Scale{ get; private set; }
        public PaperKind SheetKind { get; private set; }
    }
}
