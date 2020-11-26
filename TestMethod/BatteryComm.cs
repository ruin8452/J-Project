using System;
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
    /**
     *  @brief 배터리 통신 테스트 세팅 데이터
     *  @details 배터리 통신 테스트 세팅 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
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
