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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace MeshToJSON
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Command : IExternalCommand
    {
        XYZ vertex0 = new XYZ();
        XYZ vertex1 = new XYZ();
        XYZ vertex2 = new XYZ();
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

                string JSONpath = @"C:\ProgramData\Autodesk\Revit\Addins\2021\SmartRouting\smrvertex.json";

                /*
                //single vertex
                Reference faceRef = sel.PickObject(ObjectType.Face, "Pick Face");
                GeometryObject a = doc.GetElement(faceRef).GetGeometryObjectFromReference(faceRef);
                Face f = a as Face;
                GetTrianglesFromFace(f);
                string str = vertex1.ToString() + vertex2.ToString() + vertex3.ToString();
                TaskDialog.Show("vertex", str);
                */



                ////multi vertex
                //string str = "";
                //IList<Reference> facesRef = sel.PickObjects(ObjectType.Face, "Pick Faces");
                //foreach (Reference r in facesRef)
                //{
                //    GeometryObject a = doc.GetElement(r).GetGeometryObjectFromReference(r);
                //    Face f = a as Face;
                //    GetTrianglesFromFace(f);
                //    str += vertex0.ToString() + vertex1.ToString() + vertex2.ToString();
                //}
                //TaskDialog.Show("vertex", str);



                //multi vertex to JSON
                IList<Reference> facesRef = sel.PickObjects(ObjectType.Face, "Pick Faces");
                var jCoord = new JArray();

                //vertex length test
                //Face f2 = doc.GetElement(facesRef[0]).GetGeometryObjectFromReference(facesRef[0]) as Face;
                //f2.Triangulate().get_Triangle(0).get_Vertex(3);

                for (int i = 0; i < facesRef.Count; i++)
                {
                    GeometryObject a = doc.GetElement(facesRef[i]).GetGeometryObjectFromReference(facesRef[i]);
                    Face f = a as Face;

                    //GetTrianglesFromFace(f);
                    //Get mesh
                    Mesh mesh = f.Triangulate();

                    for (int j = 0; j < mesh.NumTriangles; j++)
                    {
                        MeshTriangle triangle = mesh.get_Triangle(j);

                        vertex0 = triangle.get_Vertex(0);
                        vertex1 = triangle.get_Vertex(1);
                        vertex2 = triangle.get_Vertex(2);

                        var v0 = new JObject();
                        var v1 = new JObject();
                        var v2 = new JObject();

                        v0.Add("X", vertex0.X.ToString());
                        v0.Add("Y", vertex0.Y.ToString());
                        v0.Add("Z", vertex0.Z.ToString());
                        v1.Add("X", vertex1.X.ToString());
                        v1.Add("Y", vertex1.Y.ToString());
                        v1.Add("Z", vertex1.Z.ToString());
                        v2.Add("X", vertex2.X.ToString());
                        v2.Add("Y", vertex2.Y.ToString());
                        v2.Add("Z", vertex2.Z.ToString());

                        var strjson = JObject.FromObject(new { index = i });

                        strjson.Add("v0", v0);
                        strjson.Add("v1", v1);
                        strjson.Add("v2", v2);

                        jCoord.Add(strjson);
                    }
                }
                JObject o = new JObject();
                o["faces"] = jCoord;

                CreateJSONfile(JSONpath, o.ToString());


                //TaskDialog.Show("facesRef.Count", facesRef.Count().ToString());

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


                ////newtonsoft.json 테스트
                //var h = PointDataImport(@"C:\temp\PointData.json");
                //TaskDialog.Show("filepath", h[0].ToString());

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

        static void CreateJSONfile(string dataPath, string strjson)
        {
            File.WriteAllText(dataPath, strjson);
        }


        private void GetTrianglesFromFace(Face face)
        {
            // Get mesh
            Mesh mesh = face.Triangulate();
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                MeshTriangle triangle = mesh.get_Triangle(i);
                vertex0 = triangle.get_Vertex(0);
                vertex1 = triangle.get_Vertex(1);
                vertex2 = triangle.get_Vertex(2);
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


        ////newtonsoft.json 테스트
        //public List<int> PointDataImport(string filePath)
        //{
        //    string jsonPath = filePath;
        //    string str = string.Empty;
        //    string users = string.Empty;
        //    List<int> Data = new List<int>();

        //    using (StreamReader file = File.OpenText(jsonPath))
        //    using (JsonTextReader reader = new JsonTextReader(file))
        //    {
        //        JObject json = (JObject)JToken.ReadFrom(reader);
        //        JToken token = json["data"]?[0];

        //        while (token != null)
        //        {
        //            var data = int.Parse(token["id"]?.ToString() ?? string.Empty);

        //            Data.Add(data);
        //            token = token.Next;
        //        }
        //    }
        //    return Data;
        //}
    }

}
