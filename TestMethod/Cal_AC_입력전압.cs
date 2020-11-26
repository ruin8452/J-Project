using System;
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
    // AC 80 설정 > AC ON > 하한값에 PM 측정값 삽입 설정 > 정류기 & PM값 비교확인 > 
    // AC 260 설정 > 상한값에 PM 측정값 삽입 설정 > 정류기 & PM값 비교확인 > 결과 저장 > AC OFF > 다음 테스트 딜레이
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