﻿using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief CalReady 테스트 세팅 데이터
     *  @details CalReady 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class CalReady : Test
    {
        public ObservableCollection<double> AcVolt { get; set; }
        public ObservableCollection<double> AcCurr { get; set; }
        public ObservableCollection<double> AcFreq { get; set; }

        public ObservableCollection<double> Delay1 { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현

        private static CalReady SingleTonObj = null;

        private CalReady()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 200 };
            AcCurr = new ObservableCollection<double>() { 40 };
            AcFreq = new ObservableCollection<double>() { 50 };

            Delay1 = new ObservableCollection<double>() { 10 };

            NextTestWait = new ObservableCollection<double>() { 0.5 };
        }

        public static CalReady GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new CalReady();
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