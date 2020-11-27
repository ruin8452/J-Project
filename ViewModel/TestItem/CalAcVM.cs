using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.UI.SubWindow;
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
     *  @brief AC CAL 테스트 클래스
     *  @details AC CAL과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class CalAcVM : AllTestVM
    {
        private enum Seq
        {
            AC_UPPER_ON,
            DELAY1,
            UPPER_POINT_CAL,
            DELAY2,
            AC_LOWER_ON,
            DELAY3,
            LOWER_POINT_CAL,
            NEXT_TEST_DELAY,
            END_TEST
        }

        private const int ERR_RATE = 5;

        public static string TestName { get; } = "Cal AC 입력전압";
        public Cal_AC_입력전압 AcCal { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public ICommand UnloadPage { get; set; }
        public ICommand UnitTestCommand { get; set; }

        public CalAcVM()
        {
            TestLog = new StringBuilder();

            TotalStepNum = (int)Seq.END_TEST + 1;

            Cal_AC_입력전압.Load();
            AcCal = Cal_AC_입력전압.GetObj();
            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

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
            Cal_AC_입력전압.Save();
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
                case Seq.AC_UPPER_ON: // AC 상한 설정
                    TestLog.AppendLine("[ AC 상한 ]");

                    // 팝업 전 확인 후 통과
                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(AcCal.AcVoltUpper[caseNumber] - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    // 자동, 반자동 분기
                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(AcCal.AcVoltUpper[caseNumber], AcCal.AcCurrUpper[caseNumber], AcCal.AcFreqUpper[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}");

                        if (result != StateFlag.PASS)
                        {
                            jumpStepNum = (int)Seq.END_TEST;
                            break;
                        }

                        result = AcSourceOn();
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.END_TEST;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(AcCal.AcVoltUpper[caseNumber], 2, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.END_TEST;
                    }
                    break;

                case Seq.DELAY1:
                    TestLog.AppendLine("[ 딜레이1 ]\n");
                    Util.Delay(AcCal.Delay1[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.UPPER_POINT_CAL:
                    TestLog.AppendLine("[ AC 상한 CAL ]");
                    result = CalCheck(1);
                    TestLog.AppendLine($"- CAL 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.END_TEST;
                    break;

                case Seq.DELAY2:
                    TestLog.AppendLine("[ 딜레이2 ]\n");
                    Util.Delay(AcCal.Delay2[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.AC_LOWER_ON: // AC 하한 설정
                    TestLog.AppendLine("[ AC 하한 ]");

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(AcCal.AcVoltLower[caseNumber], AcCal.AcCurrLower[caseNumber], AcCal.AcFreqLower[caseNumber]);
                        TestLog.AppendLine($"- AC 세팅 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.END_TEST;
                    }
                    else
                    {
                        TestLog.AppendLine($"- AC 설정 팝업");

                        result = AcCtrlWin(AcCal.AcVoltLower[caseNumber], 2, AcCheckMode.NORMAL);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.END_TEST;
                    }
                    break;

                case Seq.DELAY3:
                    TestLog.AppendLine("[ 딜레이3 ]\n");
                    Util.Delay(AcCal.Delay3[caseNumber]);
                    result = StateFlag.PASS;
                    break;

                case Seq.LOWER_POINT_CAL:
                    TestLog.AppendLine("[ AC 하한 CAL ]");
                    result = CalCheck(0);
                    TestLog.AppendLine($"- CAL 결과 : {result}\n");

                    if (result != StateFlag.PASS)
                        jumpStepNum = (int)Seq.NEXT_TEST_DELAY;
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(AcCal.NextTestWait[caseNumber]);
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
         *  @brief AC CAL 수행
         *  @details AC CAL을 진행하고 잘 됐는지 검사한다
         *  
         *  @param ushort upDown - 상한, 하한 포인트 선택
         *                         1: 상한 AC CAL, 0 : 하한 AC CAL
         *  
         *  @return StateFlag - 수행 결과
         */
        private StateFlag CalCheck(ushort upDown)
        {
            double pmAcMonitor;
            double rectAcMonitor;

            for (int i = 0; i < MAX_CAL_TRY_COUNT; i++)
            {
                TestLog.AppendLine($"- 시도 {i}회차");

                pmAcMonitor = PowerMeter.GetObj().AcVolt;
                if (double.IsNaN(pmAcMonitor) || pmAcMonitor == 0)
                {
                    TextWindow textWindow = new TextWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    textWindow.ShowDialog();
                    pmAcMonitor = TextViewModel.Number;
                }
                TestLog.AppendLine($"- 현재 AC : {pmAcMonitor}");

                TestLog.Append($"- CAL 시도 -> ");
                bool cmdResult = Rectifier.GetObj().RectCommand(CommandList.AC_IN_CAL, upDown, (ushort)(pmAcMonitor * 100.0));
                TestLog.AppendLine($"{cmdResult}");
                if (!cmdResult) continue;

                Util.Delay(1);

                rectAcMonitor = Rectifier.GetObj().AcInputVolt;
                pmAcMonitor = PowerMeter.GetObj().AcVolt;
                if (double.IsNaN(pmAcMonitor) || pmAcMonitor == 0)
                {
                    TextWindow textWindow = new TextWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    textWindow.ShowDialog();
                    pmAcMonitor = TextViewModel.Number;
                }

                TestLog.AppendLine($"- 파워미터 AC : {pmAcMonitor}");
                TestLog.AppendLine($"- 정류기 AC : {rectAcMonitor}");
                TestLog.AppendLine($"- 현재오차 : {Math.Abs(pmAcMonitor - rectAcMonitor)}");
                TestLog.AppendLine($"- 허용오차 : {ERR_RATE}");

                if (Math.Abs(pmAcMonitor - rectAcMonitor) <= ERR_RATE)
                {
                    TestLog.AppendLine($"- CAL 완료");
                    return StateFlag.PASS;
                }
            }

            TestLog.AppendLine($"- CAL 실패");
            return StateFlag.AC_CAL_ERR;
        }
    }
}