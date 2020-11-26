using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.TestMethod.EventArgsClass;
using J_Project.ViewModel.SubWindow;
using PropertyChanged;

namespace J_Project.TestMethod
{
    /**
     *  @brief AC_저전압_알람 테스트 세팅 데이터
     *  @details AC_저전압_알람 테스트 세팅 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class AC_저전압_알람 : Test
    {
        public ObservableCollection<double> AcVoltInit   { get; set; }
        public ObservableCollection<double> AcCurrInit   { get; set; }
        public ObservableCollection<double> AcFreqInit   { get; set; }

        public ObservableCollection<double> AcVoltDrop   { get; set; }
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
        private static AC_저전압_알람 SingleTonObj = null;

        private AC_저전압_알람()
        {
            MaxCase = 2;

            AcVoltInit = new ObservableCollection<double>() { 0, 0 };
            AcCurrInit = new ObservableCollection<double>() { 0, 0 };
            AcFreqInit = new ObservableCollection<double>() { 0, 0 };

            Delay1 = new ObservableCollection<double>() { 0, 0 };

            LoadCurr = new ObservableCollection<double>() { 0, 0 };

            Delay2 = new ObservableCollection<double>() { 0, 0 };
            Delay3 = new ObservableCollection<double>() { 0, 0 };
            Delay4 = new ObservableCollection<double>() { 0, 0 };

            AcVoltDrop = new ObservableCollection<double>() { 0, 0 };
            AcErrRate = new ObservableCollection<double>() { 0, 0 };

            AcVoltReturn = new ObservableCollection<double>() { 0, 0 };

            CheckTiming = new ObservableCollection<double>() { 0, 0 };
            LimitMaxVolt = new ObservableCollection<double>() { 0, 0 };
            LimitMinVolt = new ObservableCollection<double>() { 0, 0 };

            NextTestWait = new ObservableCollection<double>() { 0, 0 };
        }

        public static AC_저전압_알람 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new AC_저전압_알람();
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
