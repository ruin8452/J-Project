using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   AC 200 설정 > 정류기 출력 전압 39V 설정 > Fault 확인 > 결과저장 > 다음 테스트 딜레이
    public class 출력_저전압_보호 : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> DcOutVolt { get; set; }
        public ObservableCollection<double> DcErrRate { get; set; }
        public ObservableCollection<double> LimitDcOutVolt { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> Delay2 { get; set; }
        public ObservableCollection<double> Delay3 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 출력_저전압_보호 SingleTonObj = null;

        private 출력_저전압_보호()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            DcOutVolt = new ObservableCollection<double>() { 0 };
            DcErrRate = new ObservableCollection<double>() { 0 };

            LimitDcOutVolt = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            Delay2 = new ObservableCollection<double>() { 0 };
            Delay3 = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static 출력_저전압_보호 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 출력_저전압_보호();
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