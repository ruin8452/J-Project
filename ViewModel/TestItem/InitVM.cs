using GalaSoft.MvvmLight.Command;
using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief 초기세팅 테스트 클래스
     *  @details 초기세팅과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class InitVM : AllTestVM
    {
        private enum Seq
        {
            DC_ON,
            DELAY1,
            RECT_CONNECT,
            RTC_SET,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "초기세팅";
        public int CaseNum { get; set; }
        public 초기세팅 Init { get; set; }
        public TestOption Option { get; set; }

        public bool IsRightText { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand LoadPage { get; set; }
        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<object> UnitTestCommand { get; set; }

        private string[] ComportList;

        public InitVM(int caseNum)
        {
            CaseNum = caseNum;
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            //Init = new 초기세팅();
            Init = (초기세팅)Test.Load(new 초기세팅(), CaseNum);

            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            LoadPage = new RelayCommand(DataLoad);
            UnloadPage = new RelayCommand(DataSave);
            UnitTestCommand = new RelayCommand<object>(UnitTestClick);

#warning 임시 삽입
            ComportList = SerialPort.GetPortNames();
        }

        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DataSave()
        {
            Test.Save(Init, CaseNum);
        }

        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        private void DataLoad()
        {
            Init = (초기세팅)Test.Load(Init, CaseNum);
        }

        /**
         *  @brief 테스트 UI 텍스트 색 변경
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 변경시킨다
         *  
         *  @param int index - 세부 단계의 인덱스
         *  @param StateFlag stateFlag - 세부 단계의 테스트 결과
         *  
         *  @return
         */
        public override void TextColorChange(int index, StateFlag stateFlag)
        {
            if (stateFlag == StateFlag.PASS || stateFlag == StateFlag.INIT_END)
                ButtonColor[index] = Brushes.GreenYellow;
            else if (stateFlag == StateFlag.WAIT)
                ButtonColor[index] = Brushes.Yellow;
            else
                ButtonColor[index] = Brushes.Red;
        }

        /**
         *  @brief 테스트 UI 텍스트 색 리셋
         *  @details 양상 테스트 화면에서 해당 테스트 UI의 텍스트 색을 흰색으로 리셋
         *  
         *  @param 
         *  
         *  @return
         */
        public override void UiReset()
        {
            for (int i = 0; i < ButtonColor.Count; i++)
                ButtonColor[i] = Brushes.White;
        }

        // 올바른 값(정수 또는 실수)이 입력되었는지 판단하는 함수
        private void TextCheck(string text)
        {
            //string positiveNum = @"^(-?[0-9]+)?$";            // 정수 판단용 정규식
            //string activeNum   = @"^(-?[0-9]+(.?[0-9]+)?)?$"; // 정수 및 실수 판단용 정규식

            Regex regex = new Regex(@"^(-?[0-9]+(.?[0-9]+)?)+$"); // 정수 및 실수 판단용 정규식

            IsRightText = regex.IsMatch(text);
        }

        /**
         *  @brief 수동 테스트 동작
         *  @details 수동 모드 운영 시, 테스트 UI의 활성화된 버튼을 클릭했을 경우 실행
         *  
         *  @param object value - 2개의 데이터로 구성(1. 해당 테스트의 케이스 번호, 2. 해당 세부 단계의 인덱스 번호)
         *  
         *  @return
         */
        private void UnitTestClick(object value)
        {
            object[] parameter = (object[])value;

            //string result = Test.EquiConnectCheck();
            //if (result.Length > 0)
            //{
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //    return;
            //}

            int caseIndex = int.Parse(parameter[0].ToString());
            int unitIndex = int.Parse(parameter[1].ToString());
            int jumpNum = -1;

            TextColorChange(unitIndex, StateFlag.WAIT);
            StateFlag resultState = TestSeq(caseIndex, unitIndex, ref jumpNum);
            TextColorChange(unitIndex, resultState);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // 시퀀스 관련
        ///////////////////////////////////////////////////////////////////////////////////
        /**
         *  @brief 테스트 시퀀스
         *  @details 해당 테스트의 시퀀스를 담당 및 수행한다
         *  
         *  @param int caseNumbere - 해당 테스트의 케이스 번호
         *  @param int stepNumber - 실행할 세부 단계 번호
         *  @param ref int jumpStepNum - 점프할 세부 단계
         *  
         *  @return StateFlag - 수행 결과
         */
        public override StateFlag TestSeq(int caseNumber, int stepNumber, ref int jumpStepNum)
        {
            StateFlag result = StateFlag.NORMAL_ERR;
            Seq stepName = (Seq)stepNumber;

            switch (stepName)
            {
                case Seq.DC_ON:
                    ComportList = SerialPort.GetPortNames();

                    TestLog.AppendLine("[ DC ]");

                    result = DcSourceSet(Init.DcVolt, Init.DcCurr);
                    TestLog.AppendLine($"- DC 세팅 결과 : {result}");

                    if (result != StateFlag.PASS)
                    {
                        TestLog.AppendLine();
                        jumpStepNum = (int)Seq.END_TEST;
                        break;
                    }

                    result = DcSourceOn();
                    TestLog.AppendLine($"- DC 전원 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.END_TEST;
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(Init.Delay1);
                    result = StateFlag.PASS;
                    break;

                case Seq.RECT_CONNECT:
#warning 시리얼 포트 리스트 차집합으로 정류기 포트 산출 후 접속 기능 테스트 필요
                    string[] tempComport = SerialPort.GetPortNames();
                    EquiConnectID.GetObj().RectID = tempComport.Except(ComportList).ToArray().First();

                    TestLog.AppendLine("[ 정류기 접속 ]");
                    if (Rectifier.GetObj().IsConnected)
                    {
                        TestLog.AppendLine("- 접속 완료\n");
                        result = StateFlag.PASS;
                        break;
                    }

                    TestLog.Append("- 접속 시도 -> ");
                    Rectifier.GetObj().Connect(EquiConnectID.GetObj().RectID, 9600);
                    //Rectifier.GetObj().Connect(except, 9600);
                    Util.Delay(3);

                    if (Rectifier.GetObj().IsConnected)
                    {
                        TestLog.AppendLine("성공\n");
                        result = StateFlag.PASS;
                    }
                    else
                    {
                        TestLog.AppendLine("실패\n");
                        result = StateFlag.RECT_CONNECT_ERR;
                    }

                    break;

                case Seq.RTC_SET:
                    TestLog.AppendLine("[  RTC 설정 ]");
                    result = RtcSet();
                    TestLog.AppendLine($"- 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.END_TEST;
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(Init.NextTestWait);
                    result = StateFlag.PASS;
                    break;

                case Seq.END_TEST:
                    TestLog.AppendLine("[ 테스트 완료 ]\n");
                    result = StateFlag.INIT_END;
                    break;

                default:
                    break;
            }
            return result;
        }

        /**
         *  @brief RTC 설정
         *  @details 정류기의 RTC 값을 현재 시간으로 설정한다
         *  
         *  @param 
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag RtcSet()
        {
            bool cmdResult;

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.Append($"- RTC 설정 {i}회차 시도 -> ");

                ushort data1 = (ushort)(((DateTime.Now.Year - 2000) << 8) + DateTime.Now.Month);
                ushort data2 = (ushort)(((DateTime.Now.Day) << 8) + DateTime.Now.Hour);
                ushort data3 = (ushort)((DateTime.Now.Minute << 8) + DateTime.Now.Second);

                cmdResult = Rectifier.GetObj().RectCommand(CommandList.RTC_SET, data1, data2, data3);
                if (cmdResult)
                {
                    TestLog.AppendLine($"성공");
                    return StateFlag.PASS;
                }
                TestLog.AppendLine($"실패");
            }

            TestLog.AppendLine($"RTC 설정 실패");
            return StateFlag.RTC_ERR;
        }
    }
}