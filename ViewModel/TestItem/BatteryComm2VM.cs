using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief 배터리 통신 테스트 클래스(출하용)
     *  @details 배터리 통신과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class BatteryComm2VM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            DELAY1,
            COMM_CHECK,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        private int TestOrterNum = (int)SecondTestOrder.Battery;
        public static string TestName { get; } = "배터리 통신 확인";
        public BatteryComm BatComm { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<object> UnitTestCommand { get; set; }

        public BatteryComm2VM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            BatteryComm.Load();
            BatComm = BatteryComm.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            SecondOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName, "판단불가", "불합격" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new RelayCommand(DataSave);
            UnitTestCommand = new RelayCommand<object>(UnitTestClick);
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
            BatteryComm.Save();
            Setting.WriteSetting(BatComm);
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
                case Seq.AC_ON: // 초기 AC 설정
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(BatComm.AcVolt[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    // 자동, 반자동 분기
                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(BatComm.AcVolt[caseNumber], BatComm.AcCurr[caseNumber], BatComm.AcFreq[caseNumber]);
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

                        result = AcCtrlWin(BatComm.AcVolt[caseNumber], AC_ERR_RANGE, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(BatComm.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.COMM_CHECK: // 통신 점검
                    TestLog.AppendLine("[ 배터리 통신 점검 ]");
                    result = BatteryCommCheck(ref resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(BatComm.NextTestWait[caseNumber]);
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
         *  @brief 배터리 통신 정상 검사
         *  @details 배터리 통신이 정상적인지 검사한다.
         *           현재 통신 연결되어 있는 배터리만 검사한다
         *  
         *  @param ref (string, string) resultData - 테스트 결과
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag BatteryCommCheck(ref (string, string) resultData)
        {
            Rectifier rect = Rectifier.GetObj();
            byte batFail;
            byte batEx;
            ushort batCmd;

            bool connState = false;

            TestLog.AppendLine($"- 배터리 연결 확인");
            foreach (var temp in rect.BatteryComm)
                connState |= temp;

            if (!connState)
            {
                TestLog.AppendLine($"- 테스트 불합격 : 연결된 배터리 없음");
                resultData = ("연결된 배터리 없음", "불합격");
                return StateFlag.BATTERY_COMM_ERR;
            }

            // 8개의 배터리를 검사
            for (int i = 0; i < rect.BatteryComm.Count; i++)
            {
                // 연결되어 있지 않으면 패스
                if (!rect.BatteryComm[i]) continue;

                TestLog.AppendLine($"- {i + 1}번 배터리 연결 확인");

                batFail = (byte)(1 << i);
                batEx = (byte)(1 << i);
                batCmd = (ushort)(batFail << 8 | batEx);

                // 시도 횟수만큼 반복
                for (int j = 0; j < MAX_TRY_COUNT; j++)
                {
                    rect.RectCommand(CommandList.BATTERY_TEST, 1, batCmd); // 1 : 메뉴얼모드
                    TestLog.AppendLine($"- {i + 1}번 배터리 컨트롤 시도");
                    Util.Delay(1);

                    // 테스트 조건 검사
                    if (rect.Flag_BatFail && rect.Flag_BatEx == false)
                    {
                        TestLog.AppendLine($"- 테스트 실패 : {i + 1}번 배터리 통신 실패");
                        continue;
                    }
                    else
                    {
                        TestLog.AppendLine($"- {i + 1}번 배터리 컨트롤 성공");
                        rect.RectCommand(CommandList.BATTERY_TEST, 0, batCmd); // 0 : 자동모드

                        TestLog.AppendLine($"- 테스트 합격");
                        resultData = ("OK", "합격");
                        return StateFlag.PASS;
                    }
                }
            }

            resultData = ("배터리 통신 실패", "불합격");
            TestLog.AppendLine($"- 테스트 불합격");
            return StateFlag.BATTERY_COMM_ERR;
        }
    }
}