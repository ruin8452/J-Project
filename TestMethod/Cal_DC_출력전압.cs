using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.ViewModel.SubWindow;

namespace J_Project.TestMethod
{
    public class Cal_DC_출력전압 : Test
    {
        public ObservableCollection<double> AcVolt       { get; set; }
        public ObservableCollection<double> AcCurr       { get; set; }
        public ObservableCollection<double> AcFreq       { get; set; }

        public ObservableCollection<double> LoadCurr     { get; set; }

        public ObservableCollection<double> DacLowerRef  { get; set; }
        public ObservableCollection<double> DacUpperRef  { get; set; }

        public ObservableCollection<double> AdcLowerRef  { get; set; }
        public ObservableCollection<double> AdcUpperRef  { get; set; }
        
        public ObservableCollection<double> DefaultRef   { get; set; }
        
        public ObservableCollection<double> Delay1       { get; set; }
        public ObservableCollection<double> Delay2       { get; set; }
        public ObservableCollection<double> Delay3       { get; set; }
        public ObservableCollection<double> Delay4       { get; set; }
        public ObservableCollection<double> Delay5       { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현
        private static Cal_DC_출력전압 SingleTonObj = null;

        private Cal_DC_출력전압()
        {
            MaxCase = 1;
            
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            LoadCurr = new ObservableCollection<double>() { 0 };

            DacLowerRef = new ObservableCollection<double>() { 0 };
            DacUpperRef = new ObservableCollection<double>() { 0 };

            AdcLowerRef = new ObservableCollection<double>() { 0 };
            AdcUpperRef = new ObservableCollection<double>() { 0 };

            DefaultRef = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            Delay2 = new ObservableCollection<double>() { 0 };
            Delay3 = new ObservableCollection<double>() { 0 };
            Delay4 = new ObservableCollection<double>() { 0 };
            Delay5 = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static Cal_DC_출력전압 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Cal_DC_출력전압();
            return SingleTonObj;
        }
        #endregion

        public static void Save()
        {
            Setting.WriteSetting(GetObj(), @"\Setting\TestSetting.ini");
        }
        public static void Load()
        {
            Setting.ReadSetting(GetObj(), @"\Setting\TestSetting.ini");
        }
    }
}
