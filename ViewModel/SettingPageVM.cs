using GalaSoft.MvvmLight.Command;
using Ivi.Visa;
using J_Project.Communication.CommFlags;
using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
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
    /**
     *  @brief 장비 세팅 화면 UI VM 클래스
     *  @details 장비 세팅 화면 UI에서 사용하는 변수 및 메소드를 포함하고 있는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class SettingPageVM
    {
        public TestOption Option { get; set; }

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

        public RelayCommand DcOnCommand { get; set; }
        public RelayCommand DcOffCommand { get; set; }

        public RelayCommand<string> AcConnectClickCommand { get; set; }
        public RelayCommand<string> DcConnectClickCommand { get; set; }
        public RelayCommand<string> LoadConnectClickCommand { get; set; }
        public RelayCommand<string> PmConnectClickCommand { get; set; }
        public RelayCommand<string> Dmm1ConnectClickCommand { get; set; }
        public RelayCommand<string> Dmm2ConnectClickCommand { get; set; }
        public RelayCommand<string> OscConnectClickCommand { get; set; }
        public RelayCommand<string> RectConnectClickCommand { get; set; }
        public RelayCommand<string> RemoteConnectClickCommand { get; set; }

        public RelayCommand AcDisConnectClickCommand { get; set; }
        public RelayCommand DcDisConnectClickCommand { get; set; }
        public RelayCommand LoadDisConnectClickCommand { get; set; }
        public RelayCommand PmDisConnectClickCommand { get; set; }
        public RelayCommand Dmm1DisConnectClickCommand { get; set; }
        public RelayCommand Dmm2DisConnectClickCommand { get; set; }
        public RelayCommand OscDisConnectClickCommand { get; set; }
        public RelayCommand RectDisConnectClickCommand { get; set; }
        public RelayCommand RemoteDisConnectClickCommand { get; set; }

        public RelayCommand IdListRenewalCommand { get; set; }
        public RelayCommand AllConnectClickCommand { get; set; }
        public RelayCommand AllDisConnectClickCommand { get; set; }

        public RelayCommand EquiIdInfoSaveCommand { get; set; }

        public SettingPageVM()
        {
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

            DcOnCommand = new RelayCommand(DcOn);
            DcOffCommand = new RelayCommand(DcOff);

            AcConnectClickCommand = new RelayCommand<string>(AcConnect);
            DcConnectClickCommand = new RelayCommand<string>(DcConnect);
            LoadConnectClickCommand = new RelayCommand<string>(LoadConnect);
            PmConnectClickCommand = new RelayCommand<string>(PmConnect);
            Dmm1ConnectClickCommand = new RelayCommand<string>(Dmm1Connect);
            Dmm2ConnectClickCommand = new RelayCommand<string>(Dmm2Connect);
            OscConnectClickCommand = new RelayCommand<string>(OscConnect);
            RectConnectClickCommand = new RelayCommand<string>(RectConnect);
            RemoteConnectClickCommand = new RelayCommand<string>(RemoteConnect);

            AcDisConnectClickCommand = new RelayCommand(AcDisConnect);
            DcDisConnectClickCommand = new RelayCommand(DcDisConnect);
            LoadDisConnectClickCommand = new RelayCommand(LoadDisConnect);
            PmDisConnectClickCommand = new RelayCommand(PmDisConnect);
            Dmm1DisConnectClickCommand = new RelayCommand(Dmm1DisConnect);
            Dmm2DisConnectClickCommand = new RelayCommand(Dmm2DisConnect);
            OscDisConnectClickCommand = new RelayCommand(OscDisConnect);
            RectDisConnectClickCommand = new RelayCommand(RectDisConnect);
            RemoteDisConnectClickCommand = new RelayCommand(RemoteDisConnect);

            IdListRenewalCommand = new RelayCommand(IdListRenewalClick);
            AllConnectClickCommand = new RelayCommand(AllConnect);
            AllDisConnectClickCommand = new RelayCommand(AllDisConnect);

            EquiIdInfoSaveCommand = new RelayCommand(SaveEquiIdInfo);

            if (EquiId.AutoConnect)
                AllConnectClickCommand.Execute(null);
        }

        /**
         *  @brief DC 소스 전원 ON
         *  @details DC 소스 전원을 ON 시키는 버튼을 누를 때 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         *  
         *  @see 정류기의 장비 ID를 얻기 위해서는 전원이 공급이 되어야 하는데 간편하게 공급하기 위해 장비 설정 화면에 삽입함
         */
        private void DcOn()
        {
            if (DcSource.GetObj().IsConnected == false)
            { MessageBox.Show("DC 소스 연결X"); return; }

            DcSource.GetObj().DcPowerCtrl(CtrlFlag.ON);
        }

        /**
         *  @brief DC 소스 전원 OFF
         *  @details DC 소스 전원을 OFF 시키는 버튼을 누를 때 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         *  
         *  @see 정류기의 장비 ID를 얻기 위해서는 전원이 공급이 되어야 하는데 간편하게 공급하기 위해 장비 설정 화면에 삽입함
         */
        private void DcOff()
        {
            if (DcSource.GetObj().IsConnected == false)
            { MessageBox.Show("DC 소스 연결X"); return; }

            DcSource.GetObj().DcPowerCtrl(CtrlFlag.OFF);
        }

        /**
         *  @brief 장비ID 저장
         *  @details 장비ID를 저장시키는 버튼을 누를 때 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void SaveEquiIdInfo()
        {
            EquiConnectID.Save();
        }

        /**
         *  @brief 장비ID 리스트 갱신
         *  @details 장비ID를 갱신시키는 버튼을 누를 때 동작하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
        private void IdListRenewalClick()
        {
            EquiIdList = new ObservableCollection<string>(MakeEquiIdList());
            EquiIdTreeItems = new ObservableCollection<TreeViewItem>(MakeEquiIdTreeItems());
        }

        /**
         *  @brief 장비ID 리스트 작성
         *  @details 장비ID 리스트를 작성하는 메소드(Comport, VISA ID)
         *  
         *  @param
         *  
         *  @return
         */
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

        /**
         *  @brief 장비ID 상세정보 리스트 작성
         *  @details 장비ID에 따라 상세 정보를 작성하는 메소드
         *  
         *  @param
         *  
         *  @return
         */
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

        /**
         *  @brief AC 접속
         *  @details AC 소스에 접속한다
         *  
         *  @param string idText - AC 소스의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief AC 접속 해제
         *  @details AC 소스로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void AcDisConnect()
        {
            AcSource.Disconnect();
        }

        #endregion AC 접속 및 해제

        #region DC 접속 및 해제

        /**
         *  @brief DC 접속
         *  @details DC 소스에 접속한다
         *  
         *  @param string idText - DC 소스의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief DC 접속 해제
         *  @details DC 소스로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DcDisConnect()
        {
            DcSource.Disconnect();
        }

        #endregion DC 접속 및 해제

        #region Load 접속 및 해제

        /**
         *  @brief Load 접속
         *  @details Load에 접속한다
         *  
         *  @param string idText - Load의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief Load 접속 해제
         *  @details Load로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void LoadDisConnect()
        {
            DcLoad.Disconnect();
        }

        #endregion Load 접속 및 해제

        #region 파워미터 접속 및 해제

        /**
         *  @brief 파워미터 접속
         *  @details 파워미터에 접속한다
         *  
         *  @param string idText - 파워미터의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief 파워미터 접속 해제
         *  @details 파워미터로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void PmDisConnect()
        {
            Powermeter.Disconnect();
        }

        #endregion 파워미터 접속 및 해제

        #region 멀티미터1 접속 및 해제

        /**
         *  @brief 멀티미터1 접속
         *  @details 멀티미터1에 접속한다
         *  
         *  @param string idText - 멀티미터1의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief 멀티미터1 접속 해제
         *  @details 멀티미터1로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void Dmm1DisConnect()
        {
            Dmm1.Disconnect();
        }

        #endregion 멀티미터1 접속 및 해제

        #region 멀티미터2 접속 및 해제

        /**
         *  @brief 멀티미터2 접속
         *  @details 멀티미터2에 접속한다
         *  
         *  @param string idText - 멀티미터2의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief 멀티미터2 접속 해제
         *  @details 멀티미터2로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void Dmm2DisConnect()
        {
            Dmm2.Disconnect();
        }

        #endregion 멀티미터2 접속 및 해제

        #region 오실로스코프 접속 및 해제

        /**
         *  @brief 오실로스코프 접속
         *  @details 오실로스코프에 접속한다
         *  
         *  @param string idText - 오실로스코프의 ID
         *  
         *  @return
         */
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

        /**
         *  @brief 오실로스코프 접속 해제
         *  @details 오실로스코프로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void OscDisConnect()
        {
            Osc.Disconnect();
        }

        #endregion 오실로스코프 접속 및 해제

        #region 정류기 접속 및 해제

        /**
         *  @brief 정류기 접속
         *  @details 정류기에 접속한다
         *  
         *  @param string idText - 정류기의 ID
         *  
         *  @return
         */
        private void RectConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                Rect.Connect(idText, 9600);
            else
                Rect.Connect(idText);
        }

        /**
         *  @brief 정류기 접속 해제
         *  @details 정류기로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void RectDisConnect()
        {
            Rect.Disconnect();
        }

        #endregion 정류기 접속 및 해제

        #region 원격 접속 및 해제

        /**
         *  @brief 리모트 접속
         *  @details 리모트에 접속한다
         *  
         *  @param string idText - 리모트의 ID
         *  
         *  @return
         */
        private void RemoteConnect(string idText)
        {
            if (string.IsNullOrEmpty(idText)) return;

            if (idText.StartsWith("COM", StringComparison.Ordinal) == true)
                Rmt.Connect(idText, 9600);
            else
                Rmt.Connect(idText);
        }

        /**
         *  @brief 리모트 접속 해제
         *  @details 리모트로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
        private void RemoteDisConnect()
        {
            Rmt.Disconnect();
        }

        #endregion 원격 접속 및 해제

        #region 모든 장비 접속 및 해제

        /**
         *  @brief 모든 장비 접속
         *  @details 모든 장비에 접속한다(정류가, 리모트 제외)
         *  
         *  @param
         *  
         *  @return
         */
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

        /**
         *  @brief 모든 장비 접속 해제
         *  @details 모든 장비로부터 접속 해제한다
         *  
         *  @param
         *  
         *  @return
         */
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