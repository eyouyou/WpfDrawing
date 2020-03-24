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

        public override string TemplateName => throw new NotImplementedException();

        public override void Separate()
        {
            throw new NotImplementedException();
        }
    }
}
