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
    public class Cal_DC_출력전류 : Test
    {
        public ObservableCollection<double> M200AcVolt     { get; set; }
        public ObservableCollection<double> M200AcCurr     { get; set; }
        public ObservableCollection<double> M200AcFreq     { get; set; }
        
        public ObservableCollection<double> M100AcVolt     { get; set; }
        public ObservableCollection<double> M100AcCurr     { get; set; }
        public ObservableCollection<double> M100AcFreq     { get; set; }
                                                           
        public ObservableCollection<double> DacLowerRef    { get; set; }
        public ObservableCollection<double> DacUpperRef    { get; set; }

        public ObservableCollection<double> AdcLowerRef    { get; set; }
        public ObservableCollection<double> AdcUpperRef    { get; set; }

        public ObservableCollection<double> DacLoadCurr    { get; set; }
        public ObservableCollection<double> AdcLoadCurr    { get; set; }
        public ObservableCollection<double> M100LoadCurr   { get; set; }
        
        public ObservableCollection<double> DefaultM200Ref { get; set; }
        public ObservableCollection<double> DefaultM100Ref { get; set; }

        public ObservableCollection<double> Delay1         { get; set; }
        public ObservableCollection<double> Delay2         { get; set; }
        public ObservableCollection<double> Delay3         { get; set; }
        public ObservableCollection<double> Delay4         { get; set; }
        public ObservableCollection<double> Delay5         { get; set; }
        public ObservableCollection<double> Delay6         { get; set; }
        public ObservableCollection<double> Delay7         { get; set; }
        public ObservableCollection<double> Delay8         { get; set; }
        public ObservableCollection<double> Delay9         { get; set; }
        public ObservableCollection<double> NextTestWait   { get; set; }

        #region 싱글톤 패턴 구현
        private static Cal_DC_출력전류 SingleTonObj = null;

        private Cal_DC_출력전류()
        {
            MaxCase = 1;
            
            M200AcVolt = new ObservableCollection<double>() { 0 };
            M200AcCurr = new ObservableCollection<double>() { 0 };
            M200AcFreq = new ObservableCollection<double>() { 0 };

            M100AcVolt = new ObservableCollection<double>() { 0 };
            M100AcCurr = new ObservableCollection<double>() { 0 };
            M100AcFreq = new ObservableCollection<double>() { 0 };

            DacLowerRef = new ObservableCollection<double>() { 0 };
            DacUpperRef = new ObservableCollection<double>() { 0 };
                                                               
            AdcLowerRef = new ObservableCollection<double>() { 0 };
            AdcUpperRef = new ObservableCollection<double>() { 0 };

            DacLoadCurr = new ObservableCollection<double>() { 0 };
            AdcLoadCurr = new ObservableCollection<double>() { 0 };
            M100LoadCurr = new ObservableCollection<double>() { 0 };

            DefaultM200Ref = new ObservableCollection<double>() { 0 };
            DefaultM100Ref = new ObservableCollection<double>() { 0 };

            Delay1  = new ObservableCollection<double>() { 0 };
            Delay2  = new ObservableCollection<double>() { 0 };
            Delay3  = new ObservableCollection<double>() { 0 };
            Delay4  = new ObservableCollection<double>() { 0 };
            Delay5  = new ObservableCollection<double>() { 0 };
            Delay6  = new ObservableCollection<double>() { 0 };
            Delay7  = new ObservableCollection<double>() { 0 };
            Delay8  = new ObservableCollection<double>() { 0 };
            Delay9  = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static Cal_DC_출력전류 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Cal_DC_출력전류();
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
