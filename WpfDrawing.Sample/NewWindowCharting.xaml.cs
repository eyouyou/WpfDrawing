﻿using HevoDrawing.Charting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDrawing.Sample
{
    /// <summary>
    /// NewWindowCharting.xaml 的交互逻辑
    /// </summary>
    public partial class NewWindowCharting : Window
    {
        public NewWindowCharting()
        {
            InitializeComponent();
            Attention attention = new Attention();
            RectInteractionGroup rectInteractionGroup = new RectInteractionGroup(1, 1, attention);
            Content = rectInteractionGroup;
        }
    }
}
