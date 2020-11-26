using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    public class 초기세팅 : Test
    {
        public ObservableCollection<double> DcVolt { get; set; }
        public ObservableCollection<double> DcCurr { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 초기세팅 SingleTonObj = null;

        private 초기세팅()
        {
            MaxCase = 1;
            DcVolt = new ObservableCollection<double>() { 0 };
            DcCurr = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static 초기세팅 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 초기세팅();
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