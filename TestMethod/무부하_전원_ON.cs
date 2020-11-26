using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   AC 220 설정 > AC ON > 파워미터로 소비전류 체크 > 결과저장 > AC OFF > 다음 테스트 딜레이
    public class 무부하_전원_ON : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> LimitCurrRms { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 무부하_전원_ON SingleTonObj = null;

        private 무부하_전원_ON()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };

            LimitCurrRms = new ObservableCollection<double>() { 0 };

            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static 무부하_전원_ON GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 무부하_전원_ON();
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