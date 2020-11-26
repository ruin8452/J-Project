using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   AC 220 설정 > 3.0kW 부하 설정 > AC ON > Load ON > 효율 계산 > 결과저장 > AC OFF > Load OFF > 다음 테스트 딜레이
    public class 효율 : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> LoadCurr { get; set; }

        public ObservableCollection<double> LimitEfficiency { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> Delay2 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 효율 SingleTonObj = null;

        private 효율()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            LoadCurr = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            Delay2 = new ObservableCollection<double>() { 0 };

            LimitEfficiency = new ObservableCollection<double>() { 0 };

            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static 효율 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 효율();
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