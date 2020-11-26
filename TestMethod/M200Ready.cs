using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    internal class M200Ready : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static M200Ready SingleTonObj = null;

        private M200Ready()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 200 };
            AcCurr = new ObservableCollection<double>() { 20 };
            AcFreq = new ObservableCollection<double>() { 50 };

            NextTestWait = new ObservableCollection<double>() { 0.5 };
        }

        public static M200Ready GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new M200Ready();
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