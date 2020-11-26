using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   AC 200 설정 > 0.3kW 부하 설정 > AC ON > Load ON > 오실로스코프 설정 > OSC 잡음전압 체크 > 결과저장 > AC Off > Load Off > 다음 테스트 딜레이
    //   AC 200 설정 > 1.5kW 부하 설정 > AC ON > Load ON > 오실로스코프 설정 > OSC 잡음전압 체크 > 결과저장 > AC Off > Load Off > 다음 테스트 딜레이
    //   AC 200 설정 > 3.0kW 부하 설정 > AC ON > Load ON > 오실로스코프 설정 > OSC 잡음전압 체크 > 결과저장 > AC Off > Load Off > 다음 테스트 딜레이
    public class 리플_노이즈 : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> LoadCurr { get; set; }

        public ObservableCollection<int> LimitNoiseVolt { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> Delay2 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 리플_노이즈 SingleTonObj = null;

        private 리플_노이즈()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            LoadCurr = new ObservableCollection<double>() { 0 };

            LimitNoiseVolt = new ObservableCollection<int>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            Delay2 = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static 리플_노이즈 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 리플_노이즈();
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