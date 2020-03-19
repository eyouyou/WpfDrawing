using HevoDrawing;
using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace HevoDrawing.Charting
{
    public abstract class ChartPackBase : UserControl, IDataAvailable
    {
        protected ChartVisual ChartVisual = new ChartVisual();
        public IIntersectable Intersection => ChartVisual;
        public IChartComponentizable Components => ChartVisual.DataSource as ChartDataSource;

        public abstract void StartDataFeed();
        public abstract void StopDataFeed();

        
        public DrawingGrid Templete { get; set; }
    }
}
