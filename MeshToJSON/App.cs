using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Media;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace MeshToJSON
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabname = "JSON";
            string panelname = "Mesh to JSON";
            //creating the bitimages
            BitmapImage btn1image = new BitmapImage(new Uri(@"C:\RevitAddin\MeshToJSON\MeshToJSON\Resources\32_JSONicon01.png"));

            //creating the tab
            application.CreateRibbonTab(tabname);

            //creating the panel
            var addinpanel = application.CreateRibbonPanel(tabname, panelname);

            //creating the button
            var button1 = new PushButtonData("Mesh to JSON button1", "Mesh to JSON", Assembly.GetExecutingAssembly().Location, "MeshToJSON.Command");
            button1.ToolTip = "JSON";
            button1.LongDescription = "JSON";
            button1.LargeImage = btn1image;

            var btn1 = addinpanel.AddItem(button1) as PushButton;

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

    }
}
