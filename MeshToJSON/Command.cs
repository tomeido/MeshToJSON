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
            IEnumerable<Document> doc2 = GetLinkedDocuments(doc);

            try
            {
                //Pick a group
                Selection sel = uiapp.ActiveUIDocument.Selection;


                /*
                //single vertex
                Reference faceRef = sel.PickObject(ObjectType.Face, "Pick Face");
                GeometryObject a = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                Face f = a as Face;
                GetTrianglesFromFace(f);
                string str = vertex1.ToString() + vertex2.ToString() + vertex3.ToString();
                TaskDialog.Show("vertex", str);
                */



                //multi vertex
                string str = "";
                IList<Reference> facesRef = sel.PickObjects(ObjectType.Face, "Pick Faces");
                foreach (Reference r in facesRef)
                {
                    GeometryObject a = doc.GetElement(r).GetGeometryObjectFromReference(r);
                    Face f = a as Face;
                    GetTrianglesFromFace(f);
                    str += vertex1.ToString() + vertex2.ToString() + vertex3.ToString();
                }
                TaskDialog.Show("vertex", str);



                /*
                Reference faceRef = sel.PickObject(ObjectType.Face, "Please pick a planar face to set the work plane. ESC for cancel.");
                GeometryObject geoObject = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                PlanarFace planarFace = geoObject as PlanarFace;
                */


                /*
                Reference pickedRef = sel.PickObject(ObjectType.PointOnElement, "Please select a Face");
                Element elem = doc.GetElement(pickedRef.ElementId);
                XYZ pos = pickedRef.GlobalPoint;
                string s = pickedRef.ConvertToStableRepresentation(doc);
                string[] tab_str = s.Split(':');
                string id = tab_str[tab_str.Length - 3];
                int ID;
                Int32.TryParse(id, out ID);
                Type et = elem.GetType();


                if (typeof(RevitLinkType) == et 
                    || typeof(RevitLinkInstance) == et 
                    || typeof(Instance) == et)
                {
                    foreach (Document d in doc2)
                    {
                        if (elem.Name.Contains(d.Title))
                        {
                            Element element = d.GetElement(
                              new ElementId(ID));

                            Options ops = new Options();
                            ops.ComputeReferences = true;

                            // write the name of the element and the 
                            // number of solids in this only for 
                            // control to show the possibilities

                            MessageBox.Show(element.Name,
                              element.get_Geometry(ops)
                                .Objects.Size.ToString());

                            GeometryObject obj
                              = element.get_Geometry(ops)
                                .Objects.get_Item(0);

                            // test all surfaces of solids in the 
                            // element and return the one containing 
                            // the picked point as a planarface to 
                            // build my sketchplan

                            foreach (GeometryObject obj2 in
                              element.get_Geometry(Options).Objects)
                            {
                                if (obj2.GetType() == typeof(Solid))
                                {
                                    Solid solid2 = obj2 as Solid;
                                    foreach (Face face2 in solid2.Faces)
                                    {
                                        try
                                        {
                                            if (face2.Project(pos)
                                              .XYZPoint.DistanceTo(pos) == 0)
                                            {
                                                return face2 as PlanarFace;
                                            }
                                        }
                                        catch (NullReferenceException)
                                        {
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                */


                string json = "{\n";
                json += "\t" + "\"data\": [\n";




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


        public static IEnumerable<ExternalFileReference> GetLinkedFileReferences(Document _document)
        {
            var collector = new FilteredElementCollector(
              _document);

            var linkedElements = collector
              .OfClass(typeof(RevitLinkType))
              .Select(x => x.GetExternalFileReference())
              .ToList();

            return linkedElements;
        }


        public static IEnumerable<Document> GetLinkedDocuments(Document _document)
        {
            var linkedfiles = GetLinkedFileReferences(
              _document);

            var linkedFileNames = linkedfiles
              .Select(x => ModelPathUtils
               .ConvertModelPathToUserVisiblePath(
                 x.GetAbsolutePath())).ToList();

            return _document.Application.Documents
              .Cast<Document>()
              .Where(doc => linkedFileNames.Any(
               fileName => doc.PathName.Equals(fileName)));
        }

    }
}
