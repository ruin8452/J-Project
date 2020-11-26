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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace J_Project.UI.UserController
{
    /// <summary>
    /// ToggleSwitch.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        Thickness ThicknessRight = new Thickness(326.18, 10, 10, 10);
        Thickness ThicknessLeft = new Thickness(10, 10, 326.18, 10);
        
        public static readonly DependencyProperty OnBrushProperty = DependencyProperty.Register("OnBrush", typeof(Brush), typeof(ToggleSwitch));
        public Brush OnBrush
        {
            get { return (Brush)this.GetValue(OnBrushProperty); }
            set
            {
                this.SetValue(OnBrushProperty, value);
                if (IsToggled) Back.Fill = OnBrush;
            }
        }

        public static readonly DependencyProperty OffBrushProperty = DependencyProperty.Register("OffBrush", typeof(Brush), typeof(ToggleSwitch));
        public Brush OffBrush
        {
            get { return (Brush)this.GetValue(OffBrushProperty); }
            set
            {
                this.SetValue(OffBrushProperty, value);
                if (!IsToggled) Back.Fill = OffBrush;
            }
        }

        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register("IsToggled", typeof(bool), typeof(ToggleSwitch));
        public bool IsToggled
        {
            get { return (bool)this.GetValue(IsToggledProperty); }
            set
            {
                this.SetValue(IsToggledProperty, value);

                if (IsToggled)
                {
                    Back.Fill = OnBrush;
                    Dot.Margin = ThicknessLeft;
                }
                else
                {
                    Back.Fill = OffBrush;
                    Dot.Margin = ThicknessRight;
                }
            }
        }

        public ToggleSwitch()
        {
            InitializeComponent();

            if(IsToggled)
            {
                Back.Fill = OnBrush;
                Dot.Margin = ThicknessLeft;
            }
            else
            {
                Back.Fill = OffBrush;
                Dot.Margin = ThicknessRight;
            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(IsToggled)
            {
                Dot.Margin = ThicknessRight;
                Back.Fill = OffBrush;
                IsToggled = false;
            }
            else
            {
                Dot.Margin = ThicknessLeft;
                Back.Fill = OnBrush;
                IsToggled = true;
            }
        }
    }
}
