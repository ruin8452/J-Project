using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   AC 200 설정 > 0.3kW 부하 설정 > AC ON > Load ON > DMM 전압값 체크 > 결과저장 > AC OFF > Load OFF > 다음 테스트 딜레이
    //   AC 200 설정 > 1.5kW 부하 설정 > AC ON > Load ON > DMM 전압값 체크 > 결과저장 > AC OFF > Load OFF > 다음 테스트 딜레이
    //   AC 200 설정 > 3.0kW 부하 설정 > AC ON > Load ON > DMM 전압값 체크 > 결과저장 > AC OFF > Load OFF > 다음 테스트 딜레이
    internal class 로드_레귤레이션 : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> LoadCurr { get; set; }

        public ObservableCollection<double> CheckTiming { get; set; }
        public ObservableCollection<double> LimitMaxVolt { get; set; }
        public ObservableCollection<double> LimitMinVolt { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> Delay2 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 로드_레귤레이션 SingleTonObj = null;

        private 로드_레귤레이션()
        {
            MaxCase = 3;
            AcVolt = new ObservableCollection<double>() { 0, 0, 0 };
            AcCurr = new ObservableCollection<double>() { 0, 0, 0 };
            AcFreq = new ObservableCollection<double>() { 0, 0, 0 };

            Delay1 = new ObservableCollection<double>() { 0, 0, 0 };

            LoadCurr = new ObservableCollection<double>() { 0, 0, 0 };

            Delay2 = new ObservableCollection<double>() { 0, 0, 0 };

            CheckTiming = new ObservableCollection<double>() { 0, 0, 0 };
            LimitMaxVolt = new ObservableCollection<double>() { 0, 0, 0 };
            LimitMinVolt = new ObservableCollection<double>() { 0, 0, 0 };

            NextTestWait = new ObservableCollection<double>() { 0, 0, 0 };
        }

        public static 로드_레귤레이션 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 로드_레귤레이션();
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