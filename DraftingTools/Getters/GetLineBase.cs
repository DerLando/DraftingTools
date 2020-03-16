using Rhino;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace DraftingTools.Getters
{
    internal abstract class GetLineBase : GetPoint
    {
        public Point3d From { get; set; } = Point3d.Unset;
        public Point3d To { get; set; } = Point3d.Unset;
        internal Line Line = Line.Unset;

        public abstract void Bake(RhinoDoc doc);

        public virtual bool GetLine()
        {
            int index = 0;
            while (true)
            {
                var rc = Get();
                if (CommandResult() != Rhino.Commands.Result.Success)
                {
                    return false;
                }

                if (rc == GetResult.Point)
                {
                    switch (index)
                    {
                        case 0:
                            From = Point();
                            break;
                        case 1:
                            To = Point();
                            break;
                        default:
                            RhinoApp.WriteLine($"AreaRectangle ERROR: Got 3 points for line!");
                            return false;
                    }
                    index += 1;
                }

                if (index >= 2) { break; }
            }

            Line = new Line(From, To);

            return true;
        }
    }
}