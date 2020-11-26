﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.ViewModel.SubWindow;

namespace J_Project.TestMethod
{
    /**
     *  @brief Cal_AC_입력전압 테스트 세팅 데이터
     *  @details Cal_AC_입력전압 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class Cal_AC_입력전압 : Test
    {
        public ObservableCollection<double> AcVoltLower  { get; set; }
        public ObservableCollection<double> AcCurrLower  { get; set; }
        public ObservableCollection<double> AcFreqLower  { get; set; }

        public ObservableCollection<double> AcVoltUpper  { get; set; }
        public ObservableCollection<double> AcCurrUpper  { get; set; }
        public ObservableCollection<double> AcFreqUpper  { get; set; }

        public ObservableCollection<double> Delay1       { get; set; }
        public ObservableCollection<double> Delay2       { get; set; }
        public ObservableCollection<double> Delay3       { get; set; }
        public ObservableCollection<double> Delay4       { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현
        private static Cal_AC_입력전압 SingleTonObj = null;

        private Cal_AC_입력전압()
        {
            MaxCase = 1;
            
            AcVoltLower = new ObservableCollection<double>() { 0 };
            AcCurrLower = new ObservableCollection<double>() { 0 };
            AcFreqLower = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            Delay2 = new ObservableCollection<double>() { 0 };
            Delay3 = new ObservableCollection<double>() { 0 };
            Delay4 = new ObservableCollection<double>() { 0 };

            AcVoltUpper = new ObservableCollection<double>() { 0 };
            AcCurrUpper = new ObservableCollection<double>() { 0 };
            AcFreqUpper = new ObservableCollection<double>() { 0 };

            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static Cal_AC_입력전압 GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Cal_AC_입력전압();
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