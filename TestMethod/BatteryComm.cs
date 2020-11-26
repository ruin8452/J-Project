﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.TestMethod.EventArgsClass;
using J_Project.ViewModel.SubWindow;
using PropertyChanged;

namespace J_Project.TestMethod
{
    // 테스트 시퀀스
    //   DC 50V 설정 > DC ON > 정류기 내부 온도 확인 > 설정 된 방 내부 온도와 비교 > 결과저장 > DC Off > 다음 테스트 딜레이
    public class BatteryComm : Test
    {
        public ObservableCollection<double> AcVolt       { get; set; }
        public ObservableCollection<double> AcCurr       { get; set; }
        public ObservableCollection<double> AcFreq       { get; set; }
        
        public ObservableCollection<double> Delay1       { get; set; }
        public ObservableCollection<double> NextTestWait { get; set; }

        #region 싱글톤 패턴 구현
        private static BatteryComm SingleTonObj = null;

        private BatteryComm()
        {
            MaxCase = 1;
            AcVolt = new ObservableCollection<double>() { 0 };
            AcCurr = new ObservableCollection<double>() { 0 };
            AcFreq = new ObservableCollection<double>() { 0 };

            Delay1 = new ObservableCollection<double>() { 0 };
            NextTestWait = new ObservableCollection<double>() { 0 };
        }

        public static BatteryComm GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new BatteryComm();
            return SingleTonObj;
        }
        #endregion

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
