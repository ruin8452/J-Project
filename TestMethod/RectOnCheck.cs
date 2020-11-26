using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   DC 50V 설정 > DC ON > 정류기 내부 온도 확인 > 설정 된 방 내부 온도와 비교 > 결과저장 > DC Off > 다음 테스트 딜레이
    public class RectOnCheck : Test
    {
        public ObservableCollection<double> DcVolt { get; set; }
        public ObservableCollection<double> DcCurr { get; set; }

        public ObservableCollection<double> RoomTemp { get; set; }
        public ObservableCollection<double> ErrorRate { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static RectOnCheck SingleTonObj = null;

        private RectOnCheck()
        {
            MaxCase = 1;
            DcVolt = new ObservableCollection<double>() { 0 };
            DcCurr = new ObservableCollection<double>() { 0 };

            RoomTemp = new ObservableCollection<double>() { 0 };
            ErrorRate = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static RectOnCheck GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new RectOnCheck();
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