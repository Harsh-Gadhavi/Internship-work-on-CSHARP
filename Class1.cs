using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(DiagramGenerator.DiagramCommands))]

namespace DiagramGenerator
{
    public class DiagramCommands
    {
        [CommandMethod("DrawExactDiagramWithDimensions")]
        public void DrawExactDiagramWithDimensions()
        {
            // Step 1: Get the active AutoCAD document
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            // Step 2: Start a transaction to safely modify the drawing
            using (Transaction trans = doc.Database.TransactionManager.StartTransaction())
            {
                // Access the BlockTable and ModelSpace
                BlockTable bt = (BlockTable)trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);


                DimStyleTable dst = (DimStyleTable)trans.GetObject(doc.Database.DimStyleTableId, OpenMode.ForWrite);
                DimStyleTableRecord dimStyle = new DimStyleTableRecord
                {
                    Name = "CustomDimStyle",
                    Dimtxt = 5 // Set text height to 5
                };

                ObjectId dimStyleId = dst.Add(dimStyle);
                trans.AddNewlyCreatedDBObject(dimStyle, true);

                doc.Database.Dimstyle = dimStyleId; // Set the new dimension style as current

                // Step 3: Draw the outer rectangle (base shape)
                Point2d[] outerRectPoints = {
                    new Point2d(0, 0),
                    new Point2d(100, 0),
                    new Point2d(100, 50),
                    new Point2d(180, 50),
                    new Point2d(180, 0),
                    new Point2d(330, 0),
                    new Point2d(330, 100),
                    new Point2d(480, 100),
                    new Point2d(480, 250),
                    new Point2d(330, 250),
                    new Point2d(330, 200),
                    new Point2d(100, 200),
                    new Point2d(100, 250),
                    new Point2d(0, 250),
                };

                Polyline outerRectangle = CreatePolyline(outerRectPoints, true);
                btr.AppendEntity(outerRectangle);
                trans.AddNewlyCreatedDBObject(outerRectangle, true);

                // Step 4: Draw circles
                // Left circle
                Circle leftCircle = new Circle(new Point3d(215, 120, 0), Vector3d.ZAxis, 40);
                btr.AppendEntity(leftCircle);
                trans.AddNewlyCreatedDBObject(leftCircle, true);

                // Right circle
                Circle rightCircle = new Circle(new Point3d(405, 175, 0), Vector3d.ZAxis, 30);
                btr.AppendEntity(rightCircle);
                trans.AddNewlyCreatedDBObject(rightCircle, true);

                // Step 5: Add horizontal dimensions
                // Bottom width dimensions
                AddLinearDimension(btr, trans, new Point3d(0, 0, 0), new Point3d(100, 0, 0), new Point3d(50, -20, 0));
                AddLinearDimension(btr, trans, new Point3d(100, 0, 0), new Point3d(180, 0, 0), new Point3d(140, -20, 0));
                AddLinearDimension(btr, trans, new Point3d(180, 0, 0), new Point3d(330, 0, 0), new Point3d(255, -20, 0));

                // Top width dimension for 150
                AddLinearDimension(btr, trans, new Point3d(330, 250, 0), new Point3d(480, 250, 0), new Point3d(405, 270, 0));

                // Top section dimension for 115
                AddLinearDimension(btr, trans, new Point3d(100, 200, 0), new Point3d(215, 200, 0), new Point3d(157.5, 220, 0));

                // Step 6: Add vertical dimensions
                // Left side height
                AddLinearDimension(btr, trans, new Point3d(0, 0, 0), new Point3d(0, 250, 0), new Point3d(-20, 125, 0));

                // Right vertical section (100 dimension)
                AddLinearDimension(btr, trans, new Point3d(480, 100, 0), new Point3d(480, 250, 0), new Point3d(500, 125, 0));

                // Upper vertical section (50 dimension)
                AddLinearDimension(btr, trans, new Point3d(100, 0, 0), new Point3d(100, 50, 0), new Point3d(80, 25, 0));

                // Step 7: Add radial dimensions
                // Left circle diameter dimension (Ø80)
                AddDiametricDimension(btr, trans, new Point3d(215, 120, 0), new Point3d(255, 120, 0), 80);

                // Right circle radius dimension (R30)
                AddRadialDimension(btr, trans, new Point3d(405, 175, 0), new Point3d(435, 175, 0), 30);

                // Step 8: Commit transaction to save changes to the drawing database
                trans.Commit();
            }
        }

        private Polyline CreatePolyline(Point2d[] points, bool isClosed)
        {
            Polyline polyline = new Polyline();
            for (int i = 0; i < points.Length; i++)
            {
                polyline.AddVertexAt(i, points[i], 0, 0, 0);
            }
            polyline.Closed = isClosed;
            return polyline;
        }

        private void AddLinearDimension(BlockTableRecord btr, Transaction trans, Point3d startPoint, Point3d endPoint, Point3d dimLinePoint)
        {
            AlignedDimension dim = new AlignedDimension(startPoint, endPoint, dimLinePoint, "", ObjectId.Null);
            btr.AppendEntity(dim);
            trans.AddNewlyCreatedDBObject(dim, true);
        }

        private void AddRadialDimension(BlockTableRecord btr, Transaction trans, Point3d centerPoint, Point3d chordPoint, double radius)
        {
            RadialDimension dim = new RadialDimension(centerPoint, chordPoint, radius, "", ObjectId.Null);
            btr.AppendEntity(dim);
            trans.AddNewlyCreatedDBObject(dim, true);
        }

        private void AddDiametricDimension(BlockTableRecord btr, Transaction trans, Point3d centerPoint, Point3d chordPoint, double diameter)
        {
            DiametricDimension dim = new DiametricDimension(centerPoint, chordPoint, diameter, "", ObjectId.Null);
            btr.AppendEntity(dim);
            trans.AddNewlyCreatedDBObject(dim, true);
        }
    }
}
