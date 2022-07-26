using J_Project.Equipment;
using J_Project.Manager;
using PropertyChanged;
using System;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using J_Project.ViewModel.TestItem;
using J_Project.UI.TestSeq.TestSetting;
using System.Windows.Controls;

namespace J_Project.ViewModel
{
    /**
     *  @brief 테스트 세팅 화면 UI VM 클래스
     *  @details 테스트 세팅 화면 UI에서 사용하는 변수 및 메소드를 포함하고 있는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    class TestSettingPageVM
    {
        public Page TestUi { get; set; }
        public TestOption Option { get; set; }

        public Dmm1 Dmm1 { get; set; }
        public Dmm2 Dmm2 { get; set; }
        public Oscilloscope Osc { get; set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand OscSetCommand { get; set; }
        public RelayCommand Dmm1SetCommand { get; set; }
        public RelayCommand Dmm2SetCommand { get; set; }
        public RelayCommand<object> ListSelectedCommand { get; set; }
        public ObservableCollection<TestItemSetUnit> ListTestItems { get; set; }

        public TestSettingPageVM()
        {
            Option = TestOption.GetObj();

            Dmm1 = Dmm1.GetObj();
            Dmm2 = Dmm2.GetObj();
            Osc = Oscilloscope.GetObj();

            ListTestItems = MakeList();

            UnloadPage = new RelayCommand(DataSave);
            OscSetCommand = new RelayCommand(OscSetting);
            Dmm1SetCommand = new RelayCommand(Dmm1Setting);
            Dmm2SetCommand = new RelayCommand(Dmm2Setting);
            ListSelectedCommand = new RelayCommand<object>(ListViewSelected);
        }

        /**
         *  @brief 데이터 저장
         *  @details 테스트 옵션의 데이트를 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DataSave()
        {
            TestOption.Save();
        }

        /**
         *  @brief 오실로스코프 세팅
         *  @details 오실로스코프의 초기 세팅을 담당
         *  
         *  @param
         *  
         *  @return
         */
        private void OscSetting()
        {
            if (Oscilloscope.GetObj().IsConnected == false)
            { MessageBox.Show("오실로스코프 연결X"); return; }

            if (Oscilloscope.GetObj().CouplingMode() != "AC")
            { MessageBox.Show("커플링 설정 실패"); return; }

            if (Oscilloscope.GetObj().TimeScale() != 0.025)
            { MessageBox.Show("시간스케일 설정 실패"); return; }

            if (Oscilloscope.GetObj().RangeScale() != 0.1)
            { MessageBox.Show("범위 설정 실패"); return; }

            if (Oscilloscope.GetObj().MeasurementMode() != "PK2PK")
            { MessageBox.Show("측정모드 설정 실패"); return; }

            MessageBox.Show("설정 완료");
        }

        /**
         *  @brief DMM1 세팅
         *  @details DMM1의 초기 세팅을 담당
         *  
         *  @param
         *  
         *  @return
         */
        private void Dmm1Setting()
        {
            if (Dmm1.GetObj().IsConnected == false)
            { MessageBox.Show("DMM1 연결X"); return; }

            if (Dmm1.GetObj().DisplayVolt() != "VOLT")
            { MessageBox.Show("디스플레이 설정 실패"); return; }

            if (Dmm1.GetObj().AutoRangeSet() != 1)
            { MessageBox.Show("범위 설정 실패"); return; }

            MessageBox.Show("설정 완료");
        }

        /**
         *  @brief DMM2 세팅
         *  @details DMM2의 초기 세팅을 담당
         *  
         *  @param
         *  
         *  @return
         */
        private void Dmm2Setting()
        {
            if (Dmm2.GetObj().IsConnected == false)
            { MessageBox.Show("DMM2 연결X"); return; }

            if (Dmm2.GetObj().DisplayVolt() != "VOLT")
            { MessageBox.Show("디스플레이 설정 실패"); return; }

            if (Dmm2.GetObj().AutoRangeSet() != 1)
            { MessageBox.Show("범위 설정 실패"); return; }

            MessageBox.Show("설정 완료");
        }

        /**
         *  @brief 테스트 항목 선택 시 UI 변경
         *  @details 테스트 리스트에서 항목을 선택할 경우, 메인 화면의 테스트 스퀀스 뷰를 변경시키는 역할
         *  
         *  @param object selectedItem - 선택한 테스트 항목
         *  
         *  @return
         */
        private void ListViewSelected(object selectedItem)
        {
            TestItemSetUnit Item = selectedItem as TestItemSetUnit;
            try
            {
                TestUi = Item.TestSetUi;
            }
            catch (Exception) { }
        }

        /**
         *  @brief 리스트뷰 소스 제작
         *  @details 테스트 세팅 목록에 들어가는 테스트의 리스트뷰에 대한 소스를 만든다.
         *  
         *  @param
         *  
         *  @return ObservableCollection<TestItemUint> - 테스트 리스트뷰 소스
         */
        private ObservableCollection<TestItemSetUnit> MakeList()
        {
            ObservableCollection<TestItemSetUnit> TestList = new ObservableCollection<TestItemSetUnit>();

            TestItemSetUnit init        = new TestItemSetUnit(InitVM.TestName, new 초기세팅_Setting_UI(0));
            TestItemSetUnit calReady    = new TestItemSetUnit(CalReadyVM.TestName, new CalReady_Setting_UI(0));
            TestItemSetUnit acCal       = new TestItemSetUnit(CalAcVM.TestName, new Cal_AC_입력전압_Setting_UI(0));
            TestItemSetUnit voltCal     = new TestItemSetUnit(CalDcVoltVM.TestName, new Cal_DC_출력전압_Setting_UI(0));
            TestItemSetUnit currCal     = new TestItemSetUnit(CalDcCurrVM.TestName, new Cal_DC_출력전류_Setting_UI(0));
            TestItemSetUnit m200        = new TestItemSetUnit(M200ReadyVM.TestName, new M200Ready_Setting_UI(0));
            TestItemSetUnit m100        = new TestItemSetUnit(M100ReadyVM.TestName, new M100Ready_Setting_UI(0));
            TestItemSetUnit inrush      = new TestItemSetUnit(InrushVM.TestName, new 돌입전류_Setting_UI(0));
            TestItemSetUnit id          = new TestItemSetUnit(IdChangeVM.TestName, new IdChange_Setting_UI(0));
            TestItemSetUnit temp        = new TestItemSetUnit(TempVM.TestName, new 온도센서_점검_Setting_UI(0));
            TestItemSetUnit leakage     = new TestItemSetUnit(LeakageVM.TestName, new 누설전류_Setting_UI(0));
            TestItemSetUnit local       = new TestItemSetUnit(LocalSwitchVM.TestName, new LocalSwitch_Setting_UI(0));
            TestItemSetUnit remote      = new TestItemSetUnit(RemoteCommVM.TestName, new RemoteComm_Setting_UI(0));
            TestItemSetUnit bat         = new TestItemSetUnit(BatteryCommVM.TestName, new BatteryComm_Setting_UI(0));
            TestItemSetUnit led         = new TestItemSetUnit(LedCheckVM.TestName, new LedCheck_Setting_UI(0));
            TestItemSetUnit regul200V_1 = new TestItemSetUnit(RegulM200VM.TestName + " 1", new 레귤레이션_200V_Setting_UI(0));
            TestItemSetUnit regul200V_2 = new TestItemSetUnit(RegulM200VM.TestName + " 2", new 레귤레이션_200V_Setting_UI(1));
            TestItemSetUnit regul200V_3 = new TestItemSetUnit(RegulM200VM.TestName + " 3", new 레귤레이션_200V_Setting_UI(2));
            TestItemSetUnit regul200V_4 = new TestItemSetUnit(RegulM200VM.TestName + " 4", new 레귤레이션_200V_Setting_UI(3));
            TestItemSetUnit regul200V_5 = new TestItemSetUnit(RegulM200VM.TestName + " 5", new 레귤레이션_200V_Setting_UI(4));
            TestItemSetUnit regul200V_6 = new TestItemSetUnit(RegulM200VM.TestName + " 6", new 레귤레이션_200V_Setting_UI(5));
            TestItemSetUnit regul200V_7 = new TestItemSetUnit(RegulM200VM.TestName + " 7", new 레귤레이션_200V_Setting_UI(6));
            TestItemSetUnit regul200V_8 = new TestItemSetUnit(RegulM200VM.TestName + " 8", new 레귤레이션_200V_Setting_UI(7));
            TestItemSetUnit regul200V_9 = new TestItemSetUnit(RegulM200VM.TestName + " 9", new 레귤레이션_200V_Setting_UI(8));
            TestItemSetUnit noise       = new TestItemSetUnit(NoiseVM.TestName, new 리플_노이즈_Setting_UI(0));
            TestItemSetUnit pf          = new TestItemSetUnit(PowerFactorVM.TestName, new 역률_Setting_UI(0));
            TestItemSetUnit effic       = new TestItemSetUnit(EfficiencyVM.TestName, new 효율_Setting_UI(0));
            TestItemSetUnit outLow      = new TestItemSetUnit(OutputLowVM.TestName + " 1", new 출력_저전압_보호_Setting_UI(0));
            TestItemSetUnit outHigh     = new TestItemSetUnit(OutputHighVM.TestName, new 출력_고전압_보호_Setting_UI(0));
            TestItemSetUnit acLow1      = new TestItemSetUnit(AcLowVM.TestName + " 1", new AC_저전압_알람_Setting_UI(0));
            TestItemSetUnit acHigh      = new TestItemSetUnit(AcHighVM.TestName, new AC_고전압_알람_Setting_UI(0));
            TestItemSetUnit outOver1    = new TestItemSetUnit(OutputOverVM.TestName, new 출력_과부하_보호_Setting_UI(0));
            TestItemSetUnit regul100V_1 = new TestItemSetUnit(RegulM100VM.TestName + " 1", new 레귤레이션_100V_Setting_UI(0));
            TestItemSetUnit regul100V_2 = new TestItemSetUnit(RegulM100VM.TestName + " 2", new 레귤레이션_100V_Setting_UI(1));
            TestItemSetUnit regul100V_3 = new TestItemSetUnit(RegulM100VM.TestName + " 3", new 레귤레이션_100V_Setting_UI(2));
            TestItemSetUnit regul100V_4 = new TestItemSetUnit(RegulM100VM.TestName + " 4", new 레귤레이션_100V_Setting_UI(3));
            TestItemSetUnit regul100V_5 = new TestItemSetUnit(RegulM100VM.TestName + " 5", new 레귤레이션_100V_Setting_UI(4));
            TestItemSetUnit regul100V_6 = new TestItemSetUnit(RegulM100VM.TestName + " 6", new 레귤레이션_100V_Setting_UI(5));
            TestItemSetUnit regul100V_7 = new TestItemSetUnit(RegulM100VM.TestName + " 7", new 레귤레이션_100V_Setting_UI(6));
            TestItemSetUnit regul100V_8 = new TestItemSetUnit(RegulM100VM.TestName + " 8", new 레귤레이션_100V_Setting_UI(7));
            TestItemSetUnit regul100V_9 = new TestItemSetUnit(RegulM100VM.TestName + " 9", new 레귤레이션_100V_Setting_UI(8));
            TestItemSetUnit acLow2      = new TestItemSetUnit(AcLowVM.TestName + " 2", new AC_저전압_알람_Setting_UI(1));
            TestItemSetUnit acOut       = new TestItemSetUnit(AcBlackOutVM.TestName, new AC_정전전압_인식_Setting_UI(0));
            TestItemSetUnit outOver2    = new TestItemSetUnit(OutputOverVM.TestName + " 2", new 출력_과부하_보호_Setting_UI(1));
            TestItemSetUnit rtc         = new TestItemSetUnit(RtcCheckVM.TestName, new RTC_TIME_체크_Setting_UI(0));
            //TestItemSetUnit noload      = new TestItemSetUnit(NoLoadVM.TestName, new 무부하_전원_ON_Setting_UI(0));
            TestItemSetUnit serial      = new TestItemSetUnit(SerialSaveVM.TestName, new SerialSave_Setting_UI(0));

            TestList.Add(init);         // 초기세팅
            TestList.Add(calReady);     // CAL 준비
            TestList.Add(acCal);        // AC CAL
            TestList.Add(voltCal);      // 전압 CAL
            TestList.Add(currCal);      // 전류 CAL
            TestList.Add(m200);         // M200 모드 준비
            TestList.Add(m100);         // M100 모드 준비
            TestList.Add(inrush);       // 돌입전류
            TestList.Add(id);           // ID 변경
            TestList.Add(leakage);      // 누설전류
            TestList.Add(local);        // 로컬 스위치
            TestList.Add(remote);       // 리모트
            TestList.Add(bat);          // 배터리 통신
            TestList.Add(temp);         // 온도
            TestList.Add(led);          // LED
            TestList.Add(regul200V_1);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_2);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_3);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_4);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_5);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_6);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_7);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_8);  // 라인, 로드 레귤레이션 200V
            TestList.Add(regul200V_9);  // 라인, 로드 레귤레이션 200V
            TestList.Add(noise);        // 리플 노이즈
            TestList.Add(pf);           // 역률
            TestList.Add(effic);        // 효율
            TestList.Add(outLow);       // 출력 저전압
            TestList.Add(outHigh);      // 출력 고전압
            TestList.Add(acLow1);       // 입력 저전압
            TestList.Add(acHigh);       // 입력 고전압
            TestList.Add(outOver1);     // 출력 과부하
            TestList.Add(regul100V_1);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_2);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_3);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_4);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_5);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_6);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_7);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_8);  // 라인, 로드 레귤레이션 100V
            TestList.Add(regul100V_9);  // 라인, 로드 레귤레이션 100V
            TestList.Add(acLow2);       // 입력 저전압
            TestList.Add(acOut);        // 정전
            TestList.Add(outOver2);     // 출력 과부하
            TestList.Add(rtc);          // RTC
            //TestList.Add(noload);       // 무부하
            TestList.Add(serial);       // 시리얼 저장

            return TestList;
        }
    }
}
