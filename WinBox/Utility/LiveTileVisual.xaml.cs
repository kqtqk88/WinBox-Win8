using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WinBox.Utility
{
    public partial class LiveTileVisual : UserControl
    {
        public LiveTileVisual()
        {
            InitializeComponent();
        }

        public void SetProperties(string title, ImageSource imageSource)
        {
            _title.Text = title;
            _icon.Source = imageSource;
        }
    }
}
