using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    internal class CalReady : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static CalReady SingleTonObj = null;

        private CalReady()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 200 };
            AcCurr = new ObservableCollection<double>() { 40 };
            AcFreq = new ObservableCollection<double>() { 50 };

            Delay1 = new ObservableCollection<double>() { 10 };

            NextTestWait = new ObservableCollection<double>() { 0.5 };
        }

        public static CalReady GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new CalReady();
            return SingleTonObj;
        }

        #endregion 싱글톤 패턴 구현

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