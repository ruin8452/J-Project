using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
        public TestOption Option { get; set; }

        public Dmm1 Dmm1 { get; set; }
        public Dmm2 Dmm2 { get; set; }
        public Oscilloscope Osc { get; set; }

        public ICommand UnloadPage { get; set; }
        public ICommand OscSetCommand { get; set; }
        public ICommand Dmm1SetCommand { get; set; }
        public ICommand Dmm2SetCommand { get; set; }

        public TestSettingPageVM()
        {
            Option = TestOption.GetObj();

            Dmm1 = Dmm1.GetObj();
            Dmm2 = Dmm2.GetObj();
            Osc = Oscilloscope.GetObj();

            UnloadPage = new BaseCommand(DataSave);
            OscSetCommand = new BaseCommand(OscSetting);
            Dmm1SetCommand = new BaseCommand(Dmm1Setting);
            Dmm2SetCommand = new BaseCommand(Dmm2Setting);
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
    }
}
