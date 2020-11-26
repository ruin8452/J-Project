using Ivi.Visa;
using J_Project.Communication.CommFlags;
using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.ViewModel.CommandClass;
using Microsoft.WindowsAPICodePack.Dialogs;
using NationalInstruments.Visa;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel
{
    [ImplementPropertyChanged]
    public class SettingPageVM
    {
        public TestOption Option { get; set; }

        public string InfoSaveText { get; set; }
        public SolidColorBrush InfoSaveColor { get; set; }

        public BasicInfo Info { get; set; }
        public EquiConnectID EquiId { get; set; }

        public ObservableCollection<string> EquiIdList { get; set; }
        public ObservableCollection<TreeViewItem> EquiIdTreeItems { get; set; }

        public AcSource AcSource { get; set; }
        public DcSource DcSource { get; set; }
        public DcLoad DcLoad { get; set; }
        public PowerMeter Powermeter { get; set; }
        public Dmm1 Dmm1 { get; set; }
        public Dmm2 Dmm2 { get; set; }
        public Oscilloscope Osc { get; set; }
        public Rectifier Rect { get; set; }
        public Remote Rmt { get; set; }

        public bool AutoConnectFlag { get; set; }
        public string SettingStateText { get; set; }
        public object SettingUiFrame { get; set; }

        public ICommand UnloadPage { get; set; }
        public ICommand DcOnCommand { get; set; }
        public ICommand DcOffCommand { get; set; }
        public ICommand OscSetCommand { get; set; }
        public ICommand Dmm1SetCommand { get; set; }
        public ICommand Dmm2SetCommand { get; set; }

        public ICommand FirstReportOpenClickCommand { get; set; }
        public ICommand SecondReportOpenClickCommand { get; set; }
        public ICommand ReportSaveClickCommand { get; set; }

        public ICommand AcConnectClickCommand { get; set; }
        public ICommand DcConnectClickCommand { get; set; }
        public ICommand LoadConnectClickCommand { get; set; }
        public ICommand PmConnectClickCommand { get; set; }
        public ICommand Dmm1ConnectClickCommand { get; set; }
        public ICommand Dmm2ConnectClickCommand { get; set; }
        public ICommand OscConnectClickCommand { get; set; }
        public ICommand RectConnectClickCommand { get; set; }
        public ICommand RemoteConnectClickCommand { get; set; }

        public ICommand AcDisConnectClickCommand { get; set; }
        public ICommand DcDisConnectClickCommand { get; set; }
        public ICommand LoadDisConnectClickCommand { get; set; }
        public ICommand PmDisConnectClickCommand { get; set; }
        public ICommand Dmm1DisConnectClickCommand { get; set; }
        public ICommand Dmm2DisConnectClickCommand { get; set; }
        public ICommand OscDisConnectClickCommand { get; set; }
        public ICommand RectDisConnectClickCommand { get; set; }
        public ICommand RemoteDisConnectClickCommand { get; set; }

        public ICommand IdListRenewalCommand { get; set; }
        public ICommand AllConnectClickCommand { get; set; }
        public ICommand AllDisConnectClickCommand { get; set; }

        public ICommand BasicInfoSaveCommand { get; set; }
        public ICommand EquiIdInfoSaveCommand { get; set; }

        public SettingPageVM()
        {
            InfoSaveColor = Brushes.White;

            Info = BasicInfo.GetObj();
            EquiId = EquiConnectID.GetObj();
            Option = TestOption.GetObj();

            AcSource = AcSource.GetObj();
            DcSource = DcSource.GetObj();
            DcLoad = DcLoad.GetObj();
            Powermeter = PowerMeter.GetObj();
            Dmm1 = Dmm1.GetObj();
            Dmm2 = Dmm2.GetObj();
            Osc = Oscilloscope.GetObj();
            Rect = Rectifier.GetObj();
            Rmt = Remote.GetObj();

            EquiIdList = new ObservableCollection<string>(MakeEquiIdList());
            EquiIdTreeItems = new ObservableCollection<TreeViewItem>(MakeEquiIdTreeItems());

            UnloadPage = new BaseCommand(DataSave);
            OscSetCommand = new BaseCommand(OscSetting);
            Dmm1SetCommand = new BaseCommand(Dmm1Setting);
            Dmm2SetCommand = new BaseCommand(Dmm2Setting);
            DcOnCommand = new BaseCommand(DcOn);
            DcOffCommand = new BaseCommand(DcOff);

            FirstReportOpenClickCommand = new BaseCommand(FirstReportOpenDialog);
            SecondReportOpenClickCommand = new BaseCommand(SecondReportOpenDialog);
            ReportSaveClickCommand = new BaseCommand(ReportSaveDialog);

            AcConnectClickCommand = new ComboCommand(AcConnect);
            DcConnectClickCommand = new ComboCommand(DcConnect);
            LoadConnectClickCommand = new ComboCommand(LoadConnect);
            PmConnectClickCommand = new ComboCommand(PmConnect);
            Dmm1ConnectClickCommand = new ComboCommand(Dmm1Connect);
            Dmm2ConnectClickCommand = new ComboCommand(Dmm2Connect);
            OscConnectClickCommand = new ComboCommand(OscConnect);
            RectConnectClickCommand = new ComboCommand(RectConnect);
            RemoteConnectClickCommand = new ComboCommand(RemoteConnect);

            AcDisConnectClickCommand = new BaseCommand(AcDisConnect);
            DcDisConnectClickCommand = new BaseCommand(DcDisConnect);
            LoadDisConnectClickCommand = new BaseCommand(LoadDisConnect);
            PmDisConnectClickCommand = new BaseCommand(PmDisConnect);
            Dmm1DisConnectClickCommand = new BaseCommand(Dmm1DisConnect);
            Dmm2DisConnectClickCommand = new BaseCommand(Dmm2DisConnect);
            OscDisConnectClickCommand = new BaseCommand(OscDisConnect);
            RectDisConnectClickCommand = new BaseCommand(RectDisConnect);
            RemoteDisConnectClickCommand = new BaseCommand(RemoteDisConnect);

            IdListRenewalCommand = new BaseCommand(IdListRenewalClick);
            AllConnectClickCommand = new BaseCommand(AllConnect);
            AllDisConnectClickCommand = new BaseCommand(AllDisConnect);

            BasicInfoSaveCommand = new BaseCommand(SaveBasicInfo);
            EquiIdInfoSaveCommand = new BaseCommand(SaveEquiIdInfo);

            if (EquiId.AutoConnect)
                AllConnectClickCommand.Execute(null);
        }

        private void DataSave()
        {
            TestOption.Save();
        }

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

        private void DcOn()
        {
            if (DcSource.GetObj().IsConnected == false)
            { MessageBox.Show("DC 소스 연결X"); return; }

            DcSource.GetObj().DcPowerCtrl(CtrlFlag.ON);
        }

        private void DcOff()
        {
            if (DcSource.GetObj().IsConnected == false)
            { MessageBox.Show("DC 소스 연결X"); return; }

            DcSource.GetObj().DcPowerCtrl(CtrlFlag.OFF);
        }

        private void SaveBasicInfo()
        {
            if (string.IsNullOrEmpty(Info.Checker) || string.IsNullOrEmpty(Info.ModelName) || string.IsNullOrEmpty(Info.SerialNumber) ||
                string.IsNullOrEmpty(Info.FirstReportOpenPath) || string.IsNullOrEmpty(Info.SecondReportOpenPath) ||
                string.IsNullOrEmpty(Info.ReportSavePath))
            {
                InfoSaveColor = System.Windows.Application.Current.Resources["LedRed"] as SolidColorBrush;
                InfoSaveText = "필수 항목이 비어있습니다.";
            }
            else
            {
                InfoSaveColor = Brushes.White;
                InfoSaveText = "저장을 완료했습니다";
                BasicInfo.Save();
            }
        }

        private void SaveEquiIdInfo()
        {
            EquiConnectID.Save();
        }

        private void FirstReportOpenDialog()
        {
            using OpenFileDialog openReport = new OpenFileDialog
            {
                Title = "양산 성적서 양식 파일 선택",
                Filter = "Excel파일(*.xlsx)|*.xlsx|Excel파일 (*.xls)|*.xls|csv (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (Info.FirstReportOpenPath.Length > 0)
            {
                string folderPath = Info.FirstReportOpenPath.Substring(0, Info.FirstReportOpenPath.LastIndexOf('\\'));
                openReport.InitialDirectory = folderPath;
            }
            else
                openReport.InitialDirectory = Environment.CurrentDirectory;

            if (openReport.ShowDialog() == DialogResult.OK)   // 다이얼 로그에서 OK버튼을 눌렀을 경우
                Info.FirstReportOpenPath = openReport.FileName;
        }

        private void SecondReportOpenDialog()
        {
            using OpenFileDialog openReport = new OpenFileDialog
            {
                Title = "출하 성적서 양식 파일 선택",
                Filter = "Excel파일(*.xlsx)|*.xlsx|Excel파일 (*.xls)|*.xls|csv (*.csv)|*.csv|All files (*.*)|*.*"
            };

            if (Info.SecondReportOpenPath.Length > 0)
            {
                string folderPath = Info.SecondReportOpenPath.Substring(0, Info.SecondReportOpenPath.LastIndexOf('\\'));
                openReport.InitialDirectory = folderPath;
            }
            else
                openReport.InitialDirectory = Environment.CurrentDirectory;

            if (openReport.ShowDialog() == DialogResult.OK)   // 다이얼 로그에서 OK버튼을 눌렀을 경우
                Info.SecondReportOpenPath = openReport.FileName;
        }

        // 성적서 저장 폴더 선택 버튼 클릭
        private void ReportSaveDialog()
        {
            using CommonOpenFileDialog saveRepoPath = new CommonOpenFileDialog
            {
                Title = "성적서 저장 경로 선택",
                IsFolderPicker = true // 폴더 선택 가능 여부 설정
            };

            if (Info.ReportSavePath.Length > 0)
                saveRepoPath.InitialDirectory = Info.ReportSavePath;
            else
                saveRepoPath.InitialDirectory = Environment.CurrentDirectory;

            if (saveRepoPath.ShowDialog() == CommonFileDialogResult.Ok)
                Info.ReportSavePath = saveRepoPath.FileName;
        }

        private void IdListRenewalClick()
        {
            EquiIdList = new ObservableCollection<string>(MakeEquiIdList());
            EquiIdTreeItems = new ObservableCollection<TreeViewItem>(MakeEquiIdTreeItems());
        }

        private List<string> MakeEquiIdList()
        {
            List<string> equiIdList = new List<string>();

            try
            {
                equiIdList.AddRange(SerialPort.GetPortNames());

                // VISA ID Find 패턴 문자열 : (ASRL|GPIB|TCPIP|USB)?*
                using var rm = new ResourceManager();
                equiIdList.AddRange(rm.Find("(GPIB|TCPIP|USB)?*").ToList());
            }
            catch (Exception)
            { }

            return equiIdList;
        }

        private List<TreeViewItem> MakeEquiIdTreeItems()
        {
            List<TreeViewItem> equiIdList = new List<TreeViewItem>();

            try
            {
                foreach (var item in SerialPort.GetPortNames())
                {
                    TreeViewItem tempItem = new TreeViewItem { Header = item };

                    equiIdList.Add(tempItem);
                }

                // VISA ID Find 패턴 문자열 : (ASRL|GPIB|TCPIP|USB)?*
                using var rm = new ResourceManager();
                foreach (var item in rm.Find("(USB)?*"))
                {
                    IUsbSession usbSession = GlobalResourceManager.Open(item) as IUsbSession;

                    TreeViewItem tempItem = new TreeViewItem
                    {
                        Header = item,
                        IsExpanded = true,
                        ItemsSource = new string[] { "제조사 : " + usbSession.ManufacturerName,
                                                     "모델명 : " + usbSession.ModelName}
                    };

                    equiIdList.Add(tempItem);
                }
            }
            catch (Exception)
            { }

            return equiIdList;
        }

        #region AC 접속 및 해제

        // AC 접속
        private void AcConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = AcSource.Connect(idText, 9600);
            else
                ConnectResult = AcSource.Connect(idText);

            if (ConnectResult == "Connected!")
                AcSource.EquiCheck();
        }

        private void AcDisConnect()
        {
            AcSource.Disconnect();
        }

        #endregion AC 접속 및 해제

        #region DC 접속 및 해제

        // DC 접속
        private void DcConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = DcSource.Connect(idText, 9600);
            else
                ConnectResult = DcSource.Connect(idText);

            if (ConnectResult == "Connected!")
                DcSource.EquiCheck();
        }

        private void DcDisConnect()
        {
            DcSource.Disconnect();
        }

        #endregion DC 접속 및 해제

        #region Load 접속 및 해제

        // Load 접속
        private void LoadConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = DcLoad.Connect(idText, 9600);
            else
                ConnectResult = DcLoad.Connect(idText);

            if (ConnectResult == "Connected!")
                DcLoad.EquiCheck();
        }

        private void LoadDisConnect()
        {
            DcLoad.Disconnect();
        }

        #endregion Load 접속 및 해제

        #region 파워미터 접속 및 해제

        // 파워미터 접속
        private void PmConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = Powermeter.Connect(idText, 9600);
            else
                ConnectResult = Powermeter.Connect(idText);

            if (ConnectResult == "Connected!")
                Powermeter.EquiCheck();
        }

        private void PmDisConnect()
        {
            Powermeter.Disconnect();
        }

        #endregion 파워미터 접속 및 해제

        #region 멀티미터1 접속 및 해제

        // 멀티미터1 접속
        private void Dmm1Connect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = Dmm1.Connect(idText, 9600);
            else
                ConnectResult = Dmm1.Connect(idText);

            if (ConnectResult == "Connected!")
                Dmm1.EquiCheck();
        }

        private void Dmm1DisConnect()
        {
            Dmm1.Disconnect();
        }

        #endregion 멀티미터1 접속 및 해제

        #region 멀티미터2 접속 및 해제

        // 멀티미터2 접속
        private void Dmm2Connect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = Dmm2.Connect(idText, 9600);
            else
                ConnectResult = Dmm2.Connect(idText);

            if (ConnectResult == "Connected!")
                Dmm2.EquiCheck();
        }

        private void Dmm2DisConnect()
        {
            Dmm2.Disconnect();
        }

        #endregion 멀티미터2 접속 및 해제

        #region 오실로스코프 접속 및 해제

        // 스코프 접속
        private void OscConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            string ConnectResult;
            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                ConnectResult = Osc.Connect(idText, 9600);
            else
                ConnectResult = Osc.Connect(idText);

            if (ConnectResult == "Connected!")
                Osc.EquiCheck();
        }

        private void OscDisConnect()
        {
            Osc.Disconnect();
        }

        #endregion 오실로스코프 접속 및 해제

        #region 정류기 접속 및 해제

        // 정류기 접속
        private void RectConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                Rect.Connect(idText, 9600);
            else
                Rect.Connect(idText);
        }

        private void RectDisConnect()
        {
            Rect.Disconnect();
        }

        #endregion 정류기 접속 및 해제

        #region 원격 접속 및 해제

        // 정류기 접속
        private void RemoteConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                Rmt.Connect(idText, 9600);
            else
                Rmt.Connect(idText);
        }

        private void RemoteDisConnect()
        {
            Rmt.Disconnect();
        }

        #endregion 원격 접속 및 해제

        #region 모든 장비 접속 및 해제

        public void AllConnect()
        {
            if (AcSource.IsConnected == false && Option.IsFullAuto) AcConnectClickCommand.Execute(EquiId.AcSourceID);
            if (DcSource.IsConnected == false) DcConnectClickCommand.Execute(EquiId.DcSourceID);
            if (DcLoad.IsConnected == false && Option.IsFullAuto) LoadConnectClickCommand.Execute(EquiId.LoadID);
            if (Powermeter.IsConnected == false) PmConnectClickCommand.Execute(EquiId.PmID);
            if (Dmm1.IsConnected == false) Dmm1ConnectClickCommand.Execute(EquiId.Dmm1ID);
            if (Dmm2.IsConnected == false) Dmm2ConnectClickCommand.Execute(EquiId.Dmm2ID);
            if (Osc.IsConnected == false) OscConnectClickCommand.Execute(EquiId.OscID);
            //if (Rmt.IsConnected == false)                             RemoteConnectClickCommand.Execute(EquiId.RemoteID);
            //if (Rect.IsConnected == false)                          RectConnectClickCommand.Execute(EquiId.RectID);
        }

        public void AllDisConnect()
        {
            if (Option.IsFullAuto) AcDisConnectClickCommand.Execute(null);
            DcDisConnectClickCommand.Execute(null);
            if (Option.IsFullAuto) LoadDisConnectClickCommand.Execute(null);
            PmDisConnectClickCommand.Execute(null);
            Dmm1DisConnectClickCommand.Execute(null);
            Dmm2DisConnectClickCommand.Execute(null);
            OscDisConnectClickCommand.Execute(null);
            RemoteDisConnectClickCommand.Execute(null);
            RectDisConnectClickCommand.Execute(null);
        }

        #endregion 모든 장비 접속 및 해제
    }
}