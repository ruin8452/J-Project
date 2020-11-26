using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   AC 200 설정 > 1.5kW 부하 설정 > AC ON > Load ON > AC 270 설정 > Fault 인식 > AC 200 설정 > 제한시간 후 전압체크 > 결과저장 > 다음 테스트 딜레이
    public class RTC_TIME_체크 : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> LoadCurr { get; set; }

        public ObservableCollection<double> TimeErrRate { get; set; }
        public ObservableCollection<double> TimeErrRate2 { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> Delay2 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        public ObservableCollection<double> AcVolt2 { get; set; }
        public ObservableCollection<double> LoadCurr2 { get; set; }

        #region 싱글톤 패턴 구현

        private static RTC_TIME_체크 SingleTonObj = null;

        private RTC_TIME_체크()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            LoadCurr = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            Delay2 = new ObservableCollection<double>() { 0 };

            TimeErrRate = new ObservableCollection<double>() { 0 };
            TimeErrRate2 = new ObservableCollection<double>() { 0 };

            NextTestWait = new ObservableCollection<double>() { 0 };

            AcVolt2 = new ObservableCollection<double>() { 0 };
            LoadCurr2 = new ObservableCollection<double>() { 0 };
        }

        public static RTC_TIME_체크 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new RTC_TIME_체크();
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