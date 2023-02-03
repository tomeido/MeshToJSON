using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Windows.Forms;
using System.IO;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using System.Diagnostics;

namespace MeshToJSON
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        XYZ vertex1 = new XYZ();
        XYZ vertex2 = new XYZ();
        XYZ vertex3 = new XYZ();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get application and document objects
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            try
            {

                //Pick a group
                Selection sel = uiapp.ActiveUIDocument.Selection;
          
                Reference faceRef = sel.PickObject(ObjectType.Face, "Pick Face");
                GeometryObject a = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                Face f = a as Face;

                GetTrianglesFromFace(f);
                string str = vertex1.ToString() + vertex2.ToString() + vertex3.ToString();
                TaskDialog.Show("vertex", str);

                return Result.Succeeded;
            }

            //If the user right-clicks or presses Esc, handle the exception
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void GetTrianglesFromFace(Face face)
        {
            // Get mesh
            Mesh mesh = face.Triangulate();
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                MeshTriangle triangle = mesh.get_Triangle(i);
                vertex1 = triangle.get_Vertex(0);
                vertex2 = triangle.get_Vertex(1);
                vertex3 = triangle.get_Vertex(2);
            }
        }

    }
}
