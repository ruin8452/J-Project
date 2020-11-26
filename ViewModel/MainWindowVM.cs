using J_Project.Data;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
using PropertyChanged;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;

namespace J_Project.ViewModel
{
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

        public string GuiVersion { get; set; } = "200626.1.0.14";    // 날짜.메인.서브.패치

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

            DateMonitor.Interval = TimeSpan.FromMilliseconds(1000);
            DateMonitor.Tick += NowTime;
            DateMonitor.Start();
        }

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

        private void NowTime(object sender, EventArgs e)
        {
            PcTime = DateTime.Now;
        }

        private void AcConnect()
        {
            if (AcSource.IsConnected == false)
                AcSource.Connect(EquiConnectID.GetObj().AcSourceID, 9600);
            else
                AcSource.Disconnect();
        }
        private void DcConnect()
        {
            if (DcSource.IsConnected == false)
                DcSource.Connect(EquiConnectID.GetObj().DcSourceID);
            else
                DcSource.Disconnect();
        }
        private void LoadConnect()
        {
            if (DcLoad.IsConnected == false)
                DcLoad.Connect(EquiConnectID.GetObj().LoadID);
            else
                DcLoad.Disconnect();
        }
        private void PmConnect()
        {
            if (Powermeter.IsConnected == false)
                Powermeter.Connect(EquiConnectID.GetObj().PmID);
            else
                Powermeter.Disconnect();
        }
        private void Dmm1Connect()
        {
            if (Dmm1.IsConnected == false)
                Dmm1.Connect(EquiConnectID.GetObj().Dmm1ID);
            else
                Dmm1.Disconnect();
        }
        private void Dmm2Connect()
        {
            if (Dmm2.IsConnected == false)
                Dmm2.Connect(EquiConnectID.GetObj().Dmm2ID);
            else
                Dmm2.Disconnect();
        }
        private void OscConnect()
        {
            if (Osc.IsConnected == false)
                Osc.Connect(EquiConnectID.GetObj().OscID);
            else
                Osc.Disconnect();
        }
        private void RectConnect()
        {
            if (Rect.IsConnected == false)
                Rect.Connect(EquiConnectID.GetObj().RectID, 9600);
            else
                Rect.Disconnect();
        }
        private void RmtConnect()
        {
            if (Rmt.IsConnected == false)
                Rmt.Connect(EquiConnectID.GetObj().RemoteID, 9600);
            else
                Rmt.Disconnect();
        }
    }
}