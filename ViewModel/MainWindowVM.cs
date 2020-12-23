using J_Project.Data;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;

namespace J_Project.ViewModel
{
    /**
     *  @brief 메인 화면 UI VM 클래스
     *  @details 메인 화면 UI에서 사용하는 변수 및 메소드를 포함하고 있는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class MainWindowVM
    {
        public AcSource AcSource { get; set; }
        public DcSource DcSource { get; set; }
        public DcLoad DcLoad { get; set; }
        public PowerMeter Powermeter { get; set; }
        public Dmm1 Dmm1 { get; set; }
        public Dmm2 Dmm2 { get; set; }
        public Oscilloscope Osc { get; set; }
        public Rectifier Rect { get; set; }
        public Remote Rmt { get; set; }

        public DateTime PcTime { get; set; }

        public string GuiVersion { get; set; } = "201211.1.2.0";    // 날짜.메인.서브.패치

        public RelayCommand ClosingCommand { get; set; }
        public RelayCommand OpenCvsFolderCommand { get; set; }
        public RelayCommand OpenExcelFolderCommand { get; set; }
        public RelayCommand OpenCvsFolder2Command { get; set; }
        public RelayCommand OpenExcelFolder2Command { get; set; }

        public RelayCommand AcConnectCommand { get; set; }
        public RelayCommand DcConnectCommand { get; set; }
        public RelayCommand LoadConnectCommand { get; set; }
        public RelayCommand PmConnectCommand { get; set; }
        public RelayCommand Dmm1ConnectCommand { get; set; }
        public RelayCommand Dmm2ConnectCommand { get; set; }
        public RelayCommand OscConnectCommand { get; set; }
        public RelayCommand RectConnectCommand { get; set; }
        public RelayCommand RmtConnectCommand { get; set; }

        private DispatcherTimer DateMonitor = new DispatcherTimer();

        public MainWindowVM()
        {
            BasicInfo.Load();
            EquiConnectID.Load();
            TestOption.Load();

            AcSource = AcSource.GetObj();
            DcSource = DcSource.GetObj();
            DcLoad = DcLoad.GetObj();
            Powermeter = PowerMeter.GetObj();
            Dmm1 = Dmm1.GetObj();
            Dmm2 = Dmm2.GetObj();
            Osc = Oscilloscope.GetObj();
            Rect = Rectifier.GetObj();
            Rmt = Remote.GetObj();

            ClosingCommand = new RelayCommand(ClosingClick);
            OpenCvsFolderCommand = new RelayCommand(OpenCvsFolder);
            OpenExcelFolderCommand = new RelayCommand(OpenExcelFolder);
            OpenCvsFolder2Command = new RelayCommand(OpenCvsFolder2);
            OpenExcelFolder2Command = new RelayCommand(OpenExcelFolder2);

            AcConnectCommand = new RelayCommand(AcConnect);
            DcConnectCommand = new RelayCommand(DcConnect);
            LoadConnectCommand = new RelayCommand(LoadConnect);
            PmConnectCommand = new RelayCommand(PmConnect);
            Dmm1ConnectCommand = new RelayCommand(Dmm1Connect);
            Dmm2ConnectCommand = new RelayCommand(Dmm2Connect);
            OscConnectCommand = new RelayCommand(OscConnect);
            RectConnectCommand = new RelayCommand(RectConnect);
            RmtConnectCommand = new RelayCommand(RmtConnect);

            DateMonitor.Interval = TimeSpan.FromMilliseconds(700);
            DateMonitor.Tick += NowTime;
            DateMonitor.Start();
        }

        /**
         *  @brief 종료 클릭
         *  @details 프로그램 종료 버튼 클릭시 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void ClosingClick()
        {
            if (AcSource.IsConnected) AcSource.Disconnect();
            if (DcSource.IsConnected) DcSource.Disconnect();
            if (DcLoad.IsConnected) DcLoad.Disconnect();
            if (Powermeter.IsConnected) Powermeter.Disconnect();
            if (Dmm1.IsConnected) Dmm1.Disconnect();
            if (Dmm2.IsConnected) Dmm2.Disconnect();
            if (Osc.IsConnected) Osc.Disconnect();
            if (Rect.IsConnected) Rect.Disconnect();

            // 프로그램 프로세스 킬
            Process process = Process.GetCurrentProcess();
            process.Kill();
        }

        /**
         *  @brief 양산 CSV 보고서 폴더 열기
         *  @details 양산 CSV 보고서 폴더 열기 버튼 클릭시 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void OpenCvsFolder()
        {
            try
            {
                Process.Start(DirectoryManager.FirstCsvPath);
            }
            catch(Exception)
            {
                MessageBox.Show("경로를 찾을 수 없습니다.");
            }
        }
        /**
         *  @brief 양산 엑셀 보고서 폴더 열기
         *  @details 양산 엑셀 보고서 폴더 열기 버튼 클릭시 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void OpenExcelFolder()
        {
            try
            {
                Process.Start(DirectoryManager.FirstExcelPath);
            }
            catch (Exception)
            {
                MessageBox.Show("경로를 찾을 수 없습니다.");
            }
        }
        /**
         *  @brief 출하 CSV 보고서 폴더 열기
         *  @details 출하 CSV 보고서 폴더 열기 버튼 클릭시 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void OpenCvsFolder2()
        {
            try
            {
                Process.Start(DirectoryManager.SecondCsvPath);
            }
            catch (Exception)
            {
                MessageBox.Show("경로를 찾을 수 없습니다.");
            }
        }
        /**
         *  @brief 출하 엑셀 보고서 폴더 열기
         *  @details 출하 엑셀 보고서 폴더 열기 버튼 클릭시 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void OpenExcelFolder2()
        {
            try
            {
                Process.Start(DirectoryManager.SecondExcelPath);
            }
            catch (Exception)
            {
                MessageBox.Show("경로를 찾을 수 없습니다.");
            }
        }

        /**
         *  @brief 현재 시간 갱신
         *  @details 현재 시간을 갱신시키는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void NowTime(object sender, EventArgs e)
        {
            PcTime = DateTime.Now;
        }

        /**
         *  @brief AC 소스 접속
         *  @details AC 소스 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void AcConnect()
        {
            string ConnectResult;
            if (AcSource.IsConnected == false)
            {
                if (EquiConnectID.GetObj().AcSourceID.StartsWith("COM", StringComparison.Ordinal) == true)
                    ConnectResult = AcSource.Connect(EquiConnectID.GetObj().AcSourceID, 9600);
                else
                    ConnectResult = AcSource.Connect(EquiConnectID.GetObj().AcSourceID);

                if (ConnectResult == "Connected!")
                    AcSource.EquiCheck();


            }
            else
                AcSource.Disconnect();
        }
        /**
         *  @brief DC 소스 접속
         *  @details DC 소스 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void DcConnect()
        {
            string ConnectResult;
            if (DcSource.IsConnected == false)
            {
                ConnectResult = DcSource.Connect(EquiConnectID.GetObj().DcSourceID);
                if (ConnectResult == "Connected!")
                    DcSource.EquiCheck();
            }
            else
                DcSource.Disconnect();
        }
        /**
         *  @brief 부하 접속
         *  @details 부하 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void LoadConnect()
        {
            string ConnectResult;
            if (DcLoad.IsConnected == false)
            {
                ConnectResult = DcLoad.Connect(EquiConnectID.GetObj().LoadID);
                if (ConnectResult == "Connected!")
                    DcLoad.EquiCheck();
            }
            else
                DcLoad.Disconnect();
        }
        /**
         *  @brief 파워미터 접속
         *  @details 파워미터 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void PmConnect()
        {
            string ConnectResult;
            if (Powermeter.IsConnected == false)
            {
                ConnectResult = Powermeter.Connect(EquiConnectID.GetObj().PmID);
                if (ConnectResult == "Connected!")
                    Powermeter.EquiCheck();
            }
            else
                Powermeter.Disconnect();
        }
        /**
         *  @brief DMM1 접속
         *  @details DMM1 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void Dmm1Connect()
        {
            string ConnectResult;
            if (Dmm1.IsConnected == false)
            {
                ConnectResult = Dmm1.Connect(EquiConnectID.GetObj().Dmm1ID);
                if (ConnectResult == "Connected!")
                    Dmm1.EquiCheck();
            }
            else
                Dmm1.Disconnect();
        }
        /**
         *  @brief DMM2 접속
         *  @details DMM2 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void Dmm2Connect()
        {
            string ConnectResult;
            if (Dmm2.IsConnected == false)
            {
                if (EquiConnectID.GetObj().Dmm2ID.StartsWith("COM", StringComparison.Ordinal) == true)
                    ConnectResult = Dmm2.Connect(EquiConnectID.GetObj().Dmm2ID, 9600);
                else
                    ConnectResult = Dmm2.Connect(EquiConnectID.GetObj().Dmm2ID);

                if (ConnectResult == "Connected!")
                    Dmm2.EquiCheck();
            }
            else
                Dmm2.Disconnect();
        }
        /**
         *  @brief 오실로스코프 접속
         *  @details 오실로스코프 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void OscConnect()
        {
            string ConnectResult;
            if (Osc.IsConnected == false)
            {
                ConnectResult = Osc.Connect(EquiConnectID.GetObj().OscID);
                if (ConnectResult == "Connected!")
                    Osc.EquiCheck();
            }
            else
                Osc.Disconnect();
        }
        /**
         *  @brief 정류기 접속
         *  @details 정류기 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void RectConnect()
        {
            if (Rect.IsConnected == false)
                Rect.Connect(EquiConnectID.GetObj().RectID, 9600);
            else
                Rect.Disconnect();
        }
        /**
         *  @brief 리모트 통신기 접속
         *  @details 리모트 통신기 장비에 접속하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void RmtConnect()
        {
            if (Rmt.IsConnected == false)
                Rmt.Connect(EquiConnectID.GetObj().RemoteID, 9600);
            else
                Rmt.Disconnect();
        }
    }
}