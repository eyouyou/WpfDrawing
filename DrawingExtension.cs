using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WPFAnimation
{
    public interface IDrawingBuilder
    {
        bool IsClosed { get; set; }
        Point Start { get; set; }
        Point End { get; set; }
        bool Reset { get; set; }
        void Build(StreamGeometryContext ctx);
    }
    public partial class DrawingFactory
    {
        public Point EndPoint { get; set; }
        private IList<IDrawingBuilder> DrawingBuilders { get; set; }
        public DrawingFactory()
        {
            DrawingBuilders = new List<IDrawingBuilder>();
        }
        public StreamGeometry Build()
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                for (int i = 0; i < DrawingBuilders.Count; i++)
                {
                    var item = DrawingBuilders[i];
                    if (i == 0|| item.Reset)
                    {
                        ctx.BeginFigure(item.Start, true, item.IsClosed);
                        //EndPoint=
                    }
                    if (i != 0)
                    {
                        item.Start = EndPoint;
                    }
                    
                    item.Build(ctx);

                    EndPoint = item.End;

                }
            }
            return geometry;
        }
        public DrawingFactory ContinueDrawing(IDrawingBuilder drawingBuilder)
        {
            DrawingBuilders.Add(drawingBuilder);
            return this;
        }
    }
    public static class DrawingExtension
    {
        public static LineBuilder BuildLineSegment(this LineBuilder builder, StreamGeometryContext ctx)
        {
            var end_point = builder.GetEndPoint();
            ctx.BeginFigure(builder.Start, true, false);
            ctx.LineTo(end_point, true, false);
            return builder;
        }
        public static LineBuilder ContinueDrawingLine(this LineBuilder builder, StreamGeometryContext ctx)
        {
            var end_point = builder.GetEndPoint();
            ctx.LineTo(end_point, true, false);
            builder.End = end_point;
            return builder;
        }
        public static StreamGeometry BuildRectangle(this RectangleBuilder param)
        {
            DrawingFactory factory = new DrawingFactory();
            return factory.ContinueDrawing(new LineBuilder() { Start = param.Start, Angle = param.Angle, LineLength = param.Height, }).
                ContinueDrawing(new LineBuilder() { Angle = 90 - param.Angle, LineLength = param.Width, }).
                ContinueDrawing(new LineBuilder() { Angle = param.Angle - 180, LineLength = param.Height, }).
                Build();
        }
        public static StreamGeometry BuildRectangle2(this RectangleBuilder param)
        {
            DrawingFactory factory = new DrawingFactory();
            return factory.ContinueDrawing(new LineBuilder() { Start = param.Start, Angle = 90 - param.Angle, LineLength = param.Height, }).
                ContinueDrawing(new LineBuilder() { Angle = 90 - param.Angle, LineLength = param.Width, }).
                ContinueDrawing(new LineBuilder() { Angle = param.Angle - 180, LineLength = param.Height, }).
                Build();
        }
        public static StreamGeometry BuildLine(this LineBuilder param)
        {
            DrawingFactory factory = new DrawingFactory();
            return factory.ContinueDrawing(param).Build();
        }

    }
}
