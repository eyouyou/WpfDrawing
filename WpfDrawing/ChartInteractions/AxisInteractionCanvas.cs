using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfDrawing.Abstraction;

namespace WpfDrawing
{
    /// <summary>
    /// 取消所有外部plot
    /// </summary>
    public class AxisInteractionCanvas : InteractionCanvas
        , IIntersectable
    {
        public ICrossConfiguaration CrossOption => Cross;
        public IToolTipConfiguaration ToolTipOption => DataToolTip;

        public event IntersectChangedHandler IntersectChanged;

        private Point LastHitPoint;
        public AxisInteractionCanvas(RectDrawingCanvas canvas) : base(canvas)
        {
            var _canvas = canvas ?? throw new ArgumentNullException("please");

            Cross = new CrossVisual(this);
            DataToolTip = new ToolTipVisual(this);

            MouseMove += AxisInteractionVisual_MouseMove;
            IsCrossShow = true;
            IsToolTipShow = true;

            canvas.InteractionCanvas = this;
        }

        private void AxisInteractionVisual_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ParentElement == null || !ParentElement.IsMouseOver)
            {
                return;
            }
            var point = e.GetPosition(this);
            Plot(point, EventMessage.MouseOn);

            Cross.VisualData = VisualData;
            DataToolTip.VisualData = VisualData;
            Cross.PlotToParent(point, EventMessage.MouseOn);
            DataToolTip.PlotToParent(point, EventMessage.MouseOn);
        }

        private bool _isCrossShow = false;
        public bool IsCrossShow
        {
            get => _isCrossShow;
            set
            {
                if (value && !_isCrossShow)
                {
                    Cross.IsVisualEnable = true;
                }
                else if (!value && _isCrossShow)
                {
                    Cross.IsVisualEnable = false;
                }
                _isCrossShow = value;
            }
        }
        private bool _isToolTipShow = false;

        public bool IsToolTipShow
        {
            get => _isToolTipShow;
            set
            {
                if (value && !_isToolTipShow)
                {
                    DataToolTip.IsVisualEnable = true;
                }
                else if (!value && _isToolTipShow)
                {
                    DataToolTip.IsVisualEnable = false;
                }
                _isToolTipShow = value;
            }
        }
        public CrossVisual Cross { get; private set; }
        public ToolTipVisual DataToolTip { get; private set; }

        public override RectVisualContextData DefaultData => RectChartVisualData.Empty;

        public ToolTip Tip { get => DataToolTip.Tip; set => DataToolTip.Tip = value; }

        Dictionary<ComponentKey, ElementPosition> SeriesHitList = new Dictionary<ComponentKey, ElementPosition>();
        public override void Plot(Point point, EventMessage @event)
        {
            var vdata = VisualData.TransformVisualData<RectVisualContextData>();
            if (vdata.IsBad)
            {
                return;
            }
            //series上的悬浮控件 标记当前位置
            SeriesHitList.Clear();

            var seriesDatas = new List<SeriesData>();

            VisualData.Items[ContextDataItem.SeriesData] = seriesDatas;

            var currentPoint = point;

            var nearestX = currentPoint.X;
            var nearestY = currentPoint.Y;

            var isHint = false;
            foreach (var item in DataSources)
            {
                if (item.Value is ChartDataSource dataSource)
                {
                    var plotArea = dataSource.ConnectVisual.PlotArea;
                    var series = dataSource.SeriesCollection;
                    if (!isHint && plotArea.Contains(currentPoint))
                    {
                        foreach (SeriesVisual series_item in series)
                        {
                            if (!series_item.IsVisualEnable)
                            {
                                continue;
                            }
                            var series_plot = series_item.PlotArea;
                            var xAxis = dataSource.FindById(series_item.XAxisId) as DiscreteAxis;
                            var value = xAxis.GetValue(xAxis.OffsetPostion(currentPoint.X));
                            if (value.IsBad())
                            {
                                continue;
                            }

                            var key = new ComponentKey(series_item.ParentCanvas.Id, series_item.Id);
                            if (series_item.HitElement.Content != null
                                && dataSource.GetMappingAxisY(series_item.Id) is ContinuousAxis yAxis
                                && series_item.VisualData is RectChartVisualData series_data
                                && series_data.Data.ContainsKey(value))
                            {
                                var x = xAxis.GetPosition(value).X + xAxis.Start.X;
                                var y = yAxis.GetPosition(series_data.Data[value]).Y + xAxis.Start.Y;
                                seriesDatas.Add(new SeriesData() { Color = series_item.Color, Id = series_item.Id, Name = series_item.Name, XValue = value, YValue = series_data.Data[value], AxisX = xAxis, AxisY = (AxisVisual<IVariable>)yAxis });
                                //获取y坐标

                                nearestX = x;

                                if (!series_item.HitElement.IsAdded)
                                {
                                    AddElement(series_item.HitElement.Content);
                                    series_item.HitElement.IsAdded = true;
                                }
                                var leftTop = new Point(x - series_item.HitElement.Width / 2, y - series_item.HitElement.Height / 2);

                                if (series_plot.Contains(new Point(x, y)))
                                {
                                    SeriesHitList.Add(key, new ElementPosition(series_item.HitElement.Content, true, leftTop.X, leftTop.Y, series_item.HitElement.ZIndex));
                                }
                                else
                                {
                                    SeriesHitList.Add(key, new ElementPosition(series_item.HitElement.Content));
                                }

                                if (Cross.IsYDataAttract)
                                {
                                    nearestY = y;
                                }
                            }
                            else
                            {
                                SeriesHitList.Add(key, new ElementPosition(series_item.HitElement.Content));
                            }

                        }
                    }
                    else
                    {
                        foreach (SeriesVisual series_item in series)
                        {
                            SeriesHitList.Add(new ComponentKey(series_item.ParentCanvas.Id, series_item.Id), new ElementPosition(series_item.HitElement.Content));
                        }
                    }

                    var hitPoint = new Point(nearestX, nearestY);
                    VisualData.Items[ContextDataItem.HitPointer] = hitPoint;
                    //优化
                    if (LastHitPoint.X == nearestX)
                    {
                        return;
                    }
                    LastHitPoint = hitPoint;

                    if (IsCrossShow)
                    {
                        foreach (var seriesHitItem in SeriesHitList)
                        {
                            seriesHitItem.Value.Render();
                        }
                    }
                    IntersectChanged?.Invoke(seriesDatas.ToDictionary(it => it.Name, it => it));
                    isHint = true;
                }
            }

        }

        public override void Hide()
        {
            Cross.Hide();
            DataToolTip.Hide();

            if (SeriesHitList == null)
            {
                return;
            }
            foreach (var item in SeriesHitList)
            {
                item.Value.Render(true);
            }
        }
    }
}
