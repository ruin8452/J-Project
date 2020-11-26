using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    public class DcOutCheck : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }

        public ObservableCollection<double> DefaultRef { get; set; }

        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static DcOutCheck SingleTonObj = null;

        private DcOutCheck()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };

            DefaultRef = new ObservableCollection<double>() { 0 };

            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static DcOutCheck GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new DcOutCheck();
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