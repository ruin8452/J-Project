using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace J_Project.UI.UserController
{
    /// <summary>
    /// CircleProgressbar.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CircleProgressbar : UserControl
    {
        public static readonly DependencyProperty InsideBrushProperty = DependencyProperty.Register("InsideBrush", typeof(Brush), typeof(CircleProgressbar));
        public Brush InsideBrush
        {
            get { return (Brush)this.GetValue(InsideBrushProperty); }
            set { this.SetValue(InsideBrushProperty, value); }
        }

        public static readonly DependencyProperty ProgressBrushProperty = DependencyProperty.Register("ProgressBrush", typeof(Brush), typeof(CircleProgressbar));
        public Brush ProgressBrush
        {
            get { return (Brush)this.GetValue(ProgressBrushProperty); }
            set { this.SetValue(ProgressBrushProperty, value); }
        }

        public static readonly DependencyProperty ProgressThicknessProperty = DependencyProperty.Register("ProgressThickness", typeof(int), typeof(CircleProgressbar));
        public int ProgressThickness
        {
            get { return (int)this.GetValue(ProgressThicknessProperty); }
            set { this.SetValue(ProgressThicknessProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(int), typeof(CircleProgressbar));
        public int Value
        {
            get { return (int)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CircleProgressbar));
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public CircleProgressbar()
        {
            InitializeComponent();
        }
    }

    [ValueConversion(typeof(int), typeof(double))]
    class ValueToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value * 0.01 * 360;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((double)value / 360 * 100);
        }
    }
}