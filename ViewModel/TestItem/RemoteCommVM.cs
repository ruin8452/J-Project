using J_Project.Data;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.CommandClass;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief Remote 통신 테스트 클래스(양산용)
     *  @details Remote 통신과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class RemoteCommVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            REMOTE_CHANGE,
            COMM_CHECK,
            CONN_CHANGE,
            COMM_CHECK2,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        private int TestOrterNum = (int)FirstTestOrder.Remote;
        public static string TestName { get; } = "Remote 통신 테스트";
        public RemoteComm RemoteComm { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public RemoteCommVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            RemoteComm.Load();
            RemoteComm = RemoteComm.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            FirstOrder[TestOrterNum - 1] = new string[] { TestOrterNum.ToString(), TestName, "판단불가", "불합격" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new BaseCommand(DataSave);
            UnitTestCommand = new BaseObjCommand(UnitTestClick);
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
            RemoteComm.Save();
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
            if (stateFlag == StateFlag.PASS || stateFlag == StateFlag.TEST_END)
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
                case Seq.AC_ON:
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(RemoteComm.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(RemoteComm.AcVolt[caseNumber], RemoteComm.AcCurr[caseNumber], RemoteComm.AcFreq[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                            break;
                        }

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(RemoteComm.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.REMOTE_CHANGE:
                    TestLog.AppendLine("[ 리모트 변경 ]\n");
                    if (!Rectifier.GetObj().LocalRemoteLed)
                        result = StateFlag.PASS;
                    else
                    {
                        MessageBox.Show("스위치를 리모트로 바꿔주세요");
                        result = StateFlag.WAIT;
                    }
                    break;

                case Seq.COMM_CHECK: // 통신 점검
                    TestLog.AppendLine("[ 리모트 통신 점검 ]");
                    result = Remote12Check(ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.CONN_CHANGE: // 커넥터 변경
                    TestLog.AppendLine("[ 커넥터 변경 ]");
                    MessageBox.Show("커넥터를 변경해주세요.");
                    result = StateFlag.PASS;
                    break;

                case Seq.COMM_CHECK2: // 통신 점검
                    TestLog.AppendLine("[ 리모트 통신 점검2 ]");
                    result = Remote34Check(ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, TestName, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(RemoteComm.NextTestWait[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.END_TEST:
                    TestLog.AppendLine("[ 테스트 완료 ]\n");
                    result = StateFlag.TEST_END;
                    break;

                default:
                    break;
            }
            return result;
        }

        /**
         *  @brief 리모트 스위치 1,2 통신 검사
         *  @details 리모트 스위치 1,2 통신이 원할한지 검사한다
         *  
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag Remote12Check(ref (string, string) resultData)
        {
            int loop;
            Rectifier rect = Rectifier.GetObj();
            Remote rmt = Remote.GetObj();

            resultData = ("원격 통신 실패", "불합격");

            TestLog.AppendLine($"- 리모트 통신 접속");
            if (!rmt.IsConnected)
                rmt.Connect(EquiConnectID.GetObj().RemoteID, 9600);
            Util.Delay(0.5);

            for (int i = 0; i < 2; i++)
            {
                TestLog.AppendLine($"- DC 스위치 {i + 1}번 테스트 중");

                for (loop = 0; loop < 5; loop++)
                {
                    rmt.RemoteCommand(RemoteCommandList.DC_SWITCH1 + i, 1);
                    TestLog.AppendLine($"- Switch On 전송");

                    Util.Delay(1.2);
                    TestLog.Append($"- 전압 검사 -> ");
                    if (rect.DcSwOutVolt[i] < 10)
                    {
                        TestLog.AppendLine($"불합격 : {rect.DcSwOutVolt[i]}\n");
                        continue;
                    }
                    TestLog.AppendLine($"합격 : {rect.DcSwOutVolt[i]}\n");
                    break;
                }
                if (loop >= 5)
                {
                    TestLog.AppendLine($"- Switch 조작 실패");
                    rmt.Disconnect();
                    return StateFlag.REMOTE_COMM_ERR;
                }

                ///////////////////////////////////////////////////////////

                for (loop = 0; loop < 5; loop++)
                {
                    rmt.RemoteCommand(RemoteCommandList.DC_SWITCH1 + i, 0);
                    TestLog.AppendLine($"- Switch Off 전송");

                    Util.Delay(1.2);
                    TestLog.Append($"- 전압 검사 -> ");
                    if (rect.DcSwOutVolt[i] > 10)
                    {
                        TestLog.AppendLine($"불합격 : {rect.DcSwOutVolt[i]}\n");
                        continue;
                    }
                    TestLog.AppendLine($"합격 : {rect.DcSwOutVolt[i]}\n");
                    break;
                }
                if (loop >= 5)
                {
                    TestLog.AppendLine($"- Switch 조작 실패");
                    rmt.Disconnect();
                    return StateFlag.REMOTE_COMM_ERR;
                }

                ///////////////////////////////////////////////////////////

                for (loop = 0; loop < 5; loop++)
                {
                    rmt.RemoteCommand(RemoteCommandList.DC_SWITCH1 + i, 1);
                    TestLog.AppendLine($"- Switch On 전송");

                    Util.Delay(1.2);
                    TestLog.Append($"- 전압 검사 -> ");
                    if (rect.DcSwOutVolt[i] < 10)
                    {
                        TestLog.AppendLine($"불합격 : {rect.DcSwOutVolt[i]}\n");
                        continue;
                    }
                    TestLog.AppendLine($"합격 : {rect.DcSwOutVolt[i]}\n");
                    break;
                }
                if (loop >= 5)
                {
                    TestLog.AppendLine($"- Switch 조작 실패");
                    rmt.Disconnect();
                    return StateFlag.REMOTE_COMM_ERR;
                }
            }

            TestLog.AppendLine($"- 테스트 합격");
            resultData = ("OK", "합격");
            rmt.Disconnect();
            return StateFlag.PASS;
        }

        /**
         *  @brief 리모트 스위치 3,4 통신 검사
         *  @details 리모트 스위치 3,4 통신이 원할한지 검사한다
         *  
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag Remote34Check(ref (string, string) resultData)
        {
            int loop;
            Rectifier rect = Rectifier.GetObj();
            Remote rmt = Remote.GetObj();

            resultData = ("원격 통신 실패", "불합격");

            TestLog.AppendLine($"- 리모트 통신 접속");
            if (!rmt.IsConnected)
                rmt.Connect(EquiConnectID.GetObj().RemoteID, 9600);
            Util.Delay(1);

            for (int i = 2; i < 4; i++)
            {
                TestLog.AppendLine($"- DC 스위치 {i + 1}번 테스트 중");

                for (loop = 0; loop < 5; loop++)
                {
                    rmt.RemoteCommand(RemoteCommandList.DC_SWITCH1 + i, 1);
                    TestLog.AppendLine($"- Switch On 전송");

                    Util.Delay(1.2);
                    TestLog.Append($"- 전압 검사 -> ");
                    if (rect.DcSwOutVolt[i] < 10)
                    {
                        TestLog.AppendLine($"불합격 : {rect.DcSwOutVolt[i]}\n");
                        continue;
                    }
                    TestLog.AppendLine($"합격 : {rect.DcSwOutVolt[i]}\n");
                    break;
                }
                if (loop >= 5)
                {
                    TestLog.AppendLine($"- Switch 조작 실패");
                    rmt.Disconnect();
                    return StateFlag.REMOTE_COMM_ERR;
                }

                ///////////////////////////////////////////////////////////

                for (loop = 0; loop < 5; loop++)
                {
                    rmt.RemoteCommand(RemoteCommandList.DC_SWITCH1 + i, 0);
                    TestLog.AppendLine($"- Switch Off 전송");

                    Util.Delay(1.2);
                    TestLog.Append($"- 전압 검사 -> ");
                    if (rect.DcSwOutVolt[i] > 10)
                    {
                        TestLog.AppendLine($"불합격 : {rect.DcSwOutVolt[i]}\n");
                        continue;
                    }
                    TestLog.AppendLine($"합격 : {rect.DcSwOutVolt[i]}\n");
                    break;
                }
                if (loop >= 5)
                {
                    TestLog.AppendLine($"- Switch 조작 실패");
                    rmt.Disconnect();
                    return StateFlag.REMOTE_COMM_ERR;
                }

                ///////////////////////////////////////////////////////////

                for (loop = 0; loop < 5; loop++)
                {
                    rmt.RemoteCommand(RemoteCommandList.DC_SWITCH1 + i, 1);
                    TestLog.AppendLine($"- Switch On 전송");

                    Util.Delay(1.2);
                    TestLog.Append($"- 전압 검사 -> ");
                    if (rect.DcSwOutVolt[i] < 10)
                    {
                        TestLog.AppendLine($"불합격 : {rect.DcSwOutVolt[i]}\n");
                        continue;
                    }
                    TestLog.AppendLine($"합격 : {rect.DcSwOutVolt[i]}\n");
                    break;
                }
                if (loop >= 5)
                {
                    TestLog.AppendLine($"- Switch 조작 실패");
                    rmt.Disconnect();
                    return StateFlag.REMOTE_COMM_ERR;
                }
            }

            TestLog.AppendLine($"- 테스트 합격");
            resultData = ("OK", "합격");
            rmt.Disconnect();
            return StateFlag.PASS;
        }
    }
}