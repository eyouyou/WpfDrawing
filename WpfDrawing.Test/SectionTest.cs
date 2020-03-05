using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HevoDrawing;
using HevoDrawing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfDrawing.Test
{
    [TestClass]
    public class SectionTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tradeDay = DateTime.Now.Date;
            var day1 = tradeDay.AddHours(9).AddMinutes(30);
            var day2 = tradeDay.AddHours(10).AddMinutes(30);
            var day3 = tradeDay.AddHours(11).AddMinutes(30);

            var except_left = tradeDay.AddHours(11).AddMinutes(30).AddSeconds(1);
            var except_right = tradeDay.AddHours(13);

            var day4 = tradeDay.AddHours(14);
            var day5 = tradeDay.AddHours(15);
            var axes = new List<DateTime> { day1, day2, day3, day4, day5 };

            var except_sections = new List<Section>() { new Section() { Left = except_left.ToFormatVisualData(), Right = except_right.ToFormatVisualData() } };
            var splitValues = axes.Select(it => it.ToFormatVisualData()).ToList();
            var sections = Tools.ChangeToSections(splitValues, Tools.GetAverageRatiosWithZero(4));

            foreach (var item in except_sections)
            {
                item.ExceptFrom(sections);
            }
        }
        [TestMethod]
        public void TestMethod2()
        {
            var re = double.NaN;
            var a = re - 1;
            var b = 0.0;
            var c = 1 / b;
            var d = c.ToString("F2");
            //var a = ~(-187);
            //var sections = Tools.GetSectionsFromData(true, 100);
        }
    }
}
