using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief 출력_과부하_보호 테스트 세팅 데이터
     *  @details 출력_과부하_보호 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class 출력_과부하_보호 : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> OverLoadCurr { get; set; }

        public ObservableCollection<double> CheckTiming { get; set; }
        public ObservableCollection<double> DcOutVolt { get; set; }
        public ObservableCollection<double> VoltErrRate { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> Delay2 { get; set; }
        public ObservableCollection<double> Delay3 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static 출력_과부하_보호 SingleTonObj = null;

        private 출력_과부하_보호()
        {
            MaxCase = 2;

            AcVolt = new ObservableCollection<double>() { 0, 0 };
            AcCurr = new ObservableCollection<double>() { 0, 0 };
            AcFreq = new ObservableCollection<double>() { 0, 0 };

            Delay1 = new ObservableCollection<double>() { 0, 0 };
            Delay2 = new ObservableCollection<double>() { 0, 0 };
            Delay3 = new ObservableCollection<double>() { 0, 0 };

            OverLoadCurr = new ObservableCollection<double>() { 0, 0 };

            CheckTiming = new ObservableCollection<double>() { 0, 0 };
            DcOutVolt = new ObservableCollection<double>() { 0, 0 };
            VoltErrRate = new ObservableCollection<double>() { 0, 0 };

            NextTestWait = new ObservableCollection<double>() { 0, 0 };
        }

        public static 출력_과부하_보호 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new 출력_과부하_보호();
            return SingleTonObj;
        }

        #endregion 싱글톤 패턴 구현

        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 ini파일에 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        public static void Save()
        {
            Setting.WriteSetting(GetObj(), @"\Setting\TestSetting.ini");
        }

        /**
         *  @brief 데이터 로드
         *  @details ini파일에서 해당 테스트의 설정값을 불러온다
         *  
         *  @param
         *  
         *  @return
         */
        public static void Load()
        {
            Setting.ReadSetting(GetObj(), @"\Setting\TestSetting.ini");
        }
    }
}