using HevoDrawing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HevoDrawing.Charting
{
    public class GenericChartTemplate : ChartTemplate
    {
        public GenericChartTemplate(Chart chart) : base(chart)
        {
            Content = chart;
        }

        public override string TemplateName => "基础模板";

        public override void Separate()
        {
        }
    }
}
