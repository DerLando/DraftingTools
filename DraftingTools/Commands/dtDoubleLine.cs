using System;
using System.Collections.Generic;
using System.Drawing.Text;
using DraftingTools.Getters;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace DraftingTools.Commands
{
    public class dtDoubleLine : Command
    {
        private double Width { get; set; } = 0.25;
        static dtDoubleLine _instance;
        public dtDoubleLine()
        {
            _instance = this;
        }

        ///<summary>The only instance of the dtDoubleLine command.</summary>
        public static dtDoubleLine Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "dtDoubleLine"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            var gdl = new GetDoubleLine(Width);
            gdl.SetCommandPrompt("Pick points for double line");

            if (!gdl.GetLine()) return Result.Failure;

            gdl.Bake(doc);

            Width = gdl.LineWidth.CurrentValue;

            doc.Views.Redraw();

            return Result.Success;
        }

    }

    internal class GetDoubleLine : GetLineBase
    {
        public OptionDouble LineWidth;

        private Line LineLeft;
        private Line LineRight;

        public GetDoubleLine(double width)
        {
            LineWidth = new OptionDouble(width);

            AddOptionDouble("Linewidth", ref LineWidth);
        }

        protected override void OnDynamicDraw(GetPointDrawEventArgs e)
        {
            base.OnDynamicDraw(e);
            if (From == Point3d.Unset) return;

            var cPlane = e.RhinoDoc.Views.ActiveView.ActiveViewport.ConstructionPlane();
            var line = CalculateAndSetLines(From, e.CurrentPoint, cPlane);

            var color = e.RhinoDoc.Layers.CurrentLayer.Color;
            e.Display.DrawLine(LineLeft, color);
            e.Display.DrawLine(LineRight, color);
            e.Display.DrawDottedLine(line, color);
        }

        private Line CalculateAndSetLines(Point3d from, Point3d to, Plane plane)
        {
            Line = new Line(from, to);
            var xAxis = Vector3d.CrossProduct(Line.Direction, plane.ZAxis);
            xAxis.Unitize();

            LineLeft = Line;
            LineLeft.Transform(Transform.Translation(xAxis * LineWidth.CurrentValue * -0.5));

            LineRight = Line;
            LineRight.Transform(Transform.Translation(xAxis * LineWidth.CurrentValue * 0.5));

            return Line;
        }

        public override void Bake(RhinoDoc doc)
        {
            doc.Objects.AddLine(LineLeft);
            doc.Objects.AddLine(LineRight);
        }
    }
}