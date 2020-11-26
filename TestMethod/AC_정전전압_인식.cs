using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod.EventArgsClass;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using J_Project.ViewModel.SubWindow;
using J_Project.FileSystem;

namespace J_Project.TestMethod
{
    /**
     *  @brief AC_정전전압_인식 세팅 데이터
     *  @details AC_정전전압_인식 세팅 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class AC_정전전압_인식 : Test
    {
        public ObservableCollection<double> AcVoltInit   { get; set; }
        public ObservableCollection<double> AcCurrInit   { get; set; }
        public ObservableCollection<double> AcFreqInit   { get; set; }

        public ObservableCollection<double> AcVoltOut    { get; set; }
        public ObservableCollection<double> AcErrRate    { get; set; }

        public ObservableCollection<double> AcVoltReturn { get; set; }

        public ObservableCollection<double> LoadCurr     { get; set; }

        public ObservableCollection<double> CheckTiming  { get; set; }
        public ObservableCollection<double> LimitMaxVolt { get; set; }
        public ObservableCollection<double> LimitMinVolt { get; set; }
        
        public ObservableCollection<double> Delay1       { get; set; }
        public ObservableCollection<double> Delay2       { get; set; }
        public ObservableCollection<double> Delay3       { get; set; }
        public ObservableCollection<double> Delay4       { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현
        private static AC_정전전압_인식 SingleTonObj = null;

        private AC_정전전압_인식()
        {
            MaxCase = 1;
            AcVoltInit = new ObservableCollection<double>() { 0 };
            AcCurrInit = new ObservableCollection<double>() { 0 };
            AcFreqInit = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };

            LoadCurr = new ObservableCollection<double>() { 0 };

            Delay2 = new ObservableCollection<double>() { 0 };
            Delay3 = new ObservableCollection<double>() { 0 };
            Delay4 = new ObservableCollection<double>() { 0 };

            AcVoltOut = new ObservableCollection<double>() { 0 };
            AcErrRate = new ObservableCollection<double>() { 0 };

            AcVoltReturn = new ObservableCollection<double>() { 0 };

            CheckTiming = new ObservableCollection<double>() { 0 };
            LimitMaxVolt = new ObservableCollection<double>() { 0 };
            LimitMinVolt = new ObservableCollection<double>() { 0 };

            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static AC_정전전압_인식 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new AC_정전전압_인식();
            return SingleTonObj;
        }
        #endregion

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
