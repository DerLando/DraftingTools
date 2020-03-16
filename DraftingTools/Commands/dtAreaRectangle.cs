using System;
using DraftingTools.Getters;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace DraftingTools.Commands
{
    public class dtAreaRectangle : Command
    {
        private double Area = 10;
        private bool Flip = false;

        static dtAreaRectangle _instance;
        public dtAreaRectangle()
        {
            _instance = this;
        }

        ///<summary>The only instance of the dtAreaRectangle command.</summary>
        public static dtAreaRectangle Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "dtAreaRectangle"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var gar = new GetAreaRectangle(Area, Flip);
            gar.SetCommandPrompt("Select points and define area for rectangle");

            if (!gar.GetLine())
            {
                return Result.Failure;
            }

            gar.Bake(doc);

            doc.Views.Redraw();

            Area = gar.Area.CurrentValue;
            Flip = gar.Flip.CurrentValue;

            return Result.Success;
        }
    }

    internal class GetAreaRectangle : GetLineBase
    {
        public OptionDouble Area;
        public OptionToggle Flip;

        private Rectangle3d Rectangle;

        public GetAreaRectangle(double area, bool flip)
        {
            Area = new OptionDouble(area);

            AddOptionDouble("Area", ref Area);

            Flip = new OptionToggle(flip, "Right", "Left");
            AddOptionToggle("Side", ref Flip);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            base.OnDynamicDraw(e);
            if (From == Point3d.Unset) return;

            var cPlane = e.RhinoDoc.Views.ActiveView.ActiveViewport.ConstructionPlane();
            Line = new Line(From, e.CurrentPoint);

            var yAxis = Vector3d.CrossProduct(Line.Direction, cPlane.ZAxis);
            if (Flip.CurrentValue) yAxis.Reverse();
            var plane = new Plane(From, Line.Direction, yAxis);
            yAxis.Unitize();

            var sideLength = Area.CurrentValue / Line.Length;
            Rectangle = new Rectangle3d(plane, Line.Length, sideLength);

            var color = e.RhinoDoc.Layers.CurrentLayer.Color;
            e.Display.DrawPolyline(Rectangle.ToPolyline(), color);
        }

        public override void Bake(RhinoDoc doc)
        {
            doc.Objects.AddRectangle(Rectangle);
        }

    }
}