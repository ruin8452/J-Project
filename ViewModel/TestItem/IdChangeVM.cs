﻿using GalaSoft.MvvmLight.Command;
using J_Project.Equipment;
using J_Project.Manager;
using J_Project.TestMethod;
using J_Project.UI.SubWindow;
using J_Project.ViewModel.SubWindow;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace J_Project.ViewModel.TestItem
{
    /**
     *  @brief ID-Set 확인 테스트 클래스
     *  @details ID-Set 확인과 관련된 시퀀스 및 UI관련을 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    internal class IdChangeVM : AllTestVM
    {
        private enum Seq
        {
            AC_ON,
            ID_CHANGE,
            RESULT_SAVE,
            NEXT_TEST_DELAY,
            END_TEST
        }

        public static string TestName { get; } = "ID-Set 확인";
        public int CaseNum { get; set; }
        public IdChange IdChange { get; set; }
        public TestOption Option { get; set; }

        public ObservableCollection<SolidColorBrush> ButtonColor { get; private set; }

        public RelayCommand UnloadPage { get; set; }
        public RelayCommand<int> UnitTestCommand { get; set; }

        public IdChangeVM(int caseNum)
        {
            CaseNum = caseNum;
            TestLog = new StringBuilder();

            TestOrterNum = (int)FirstTestOrder.IdSet + caseNum;
            TotalStepNum = (int)Seq.END_TEST + 1;

            IdChange = new IdChange();
            Test.Load(IdChange, CaseNum);

            Option = TestOption.GetObj();
            ButtonColor = new ObservableCollection<SolidColorBrush>();

            FirstOrder[TestOrterNum] = new string[] { TestOrterNum.ToString(), TestName + caseNum.ToString(), "판단불가", "NG(불합격)" };

            for (int i = 0; i < TotalStepNum; i++)
                ButtonColor.Add(Brushes.White);

            UnloadPage = new RelayCommand(DataSave);
            UnitTestCommand = new RelayCommand<int>(UnitTestClick);
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
            Test.Save(IdChange, CaseNum);
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
         *  @param int unitIndex - 테스트 시퀀스 번호
         *  
         *  @return
         */
        private void UnitTestClick(int unitIndex)
        {
            //string result = Test.EquiConnectCheck();
            //if (result.Length > 0)
            //{
            //    MessageBox.Show($"다음의 장비의 연결이 원할하지 않습니다.\n\n{result}", "장비 연결");
            //    return;
            //}

            int jumpNum = -1;

            TextColorChange(unitIndex, StateFlag.WAIT);
            StateFlag resultState = TestSeq(unitIndex, ref jumpNum);
            TextColorChange(unitIndex, resultState);
        }

        ///////////////////////////////////////////////////////////////////////////////////
        // 시퀀스 관련
        ///////////////////////////////////////////////////////////////////////////////////
        /**
         *  @brief 테스트 시퀀스
         *  @details 해당 테스트의 시퀀스를 담당 및 수행한다
         *  
         *  @param int stepNumber - 실행할 세부 단계 번호
         *  @param ref int jumpStepNum - 점프할 세부 단계
         *  
         *  @return StateFlag - 수행 결과
         */
        public override StateFlag TestSeq(int stepNumber, ref int jumpStepNum)
        {
            StateFlag result = StateFlag.NORMAL_ERR;
            Seq stepName = (Seq)stepNumber;

            switch (stepName)
            {
                case Seq.AC_ON:
                    TestLog.AppendLine("[ AC ]");

                    double acVolt = PowerMeter.GetObj().AcVolt;
                    TestLog.AppendLine($"- AC : {acVolt}");
                    if (Math.Abs(IdChange.AcVolt - acVolt) <= AC_ERR_RANGE)
                    {
                        result = StateFlag.PASS;
                        break;
                    }

                    if (Option.IsFullAuto)
                    {
                        result = AcSourceSet(IdChange.AcVolt, IdChange.AcCurr, IdChange.AcFreq);
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

                        result = AcCtrlWin(IdChange.AcVolt, AC_ERR_RANGE);
                        TestLog.AppendLine($"- AC 전원 결과 : {result}\n");

                        if (result != StateFlag.PASS)
                            jumpStepNum = (int)Seq.RESULT_SAVE;
                    }
                    break;

                case Seq.ID_CHANGE:  // ID 변경
                    TestLog.AppendLine("[ ID 변경 ]");

                    TestLog.AppendLine($"- ID 변경 팝업");

                    IdChangeWindow idChangeWindow = new IdChangeWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    idChangeWindow.ShowDialog();
                    subWinResult = IdChangeWinViewModel.PassOrFail;

                    if (subWinResult == true)
                    {
                        TestLog.AppendLine($"- 테스트 합격\n");
                        resultData = ("OK", "OK(합격)");
                        result = StateFlag.PASS;
                    }
                    else if (subWinResult == false)
                    {
                        TestLog.AppendLine($"- 테스트 불합격\n");
                        resultData = ("ID 변경 오류", "NG(불합격)");
                        result = StateFlag.ID_ERROR;
                    }
                    break;

                case Seq.RESULT_SAVE: // 결과 저장
                    TestLog.AppendLine("[ 결과 저장 ]");
                    result = ResultDataSave(TestOrterNum, resultData);
                    TestLog.AppendLine($"- 결과 : {result}\n");
                    break;

                case Seq.NEXT_TEST_DELAY:
                    TestLog.AppendLine("[ 다음 테스트 딜레이 ]\n");
                    Util.Delay(IdChange.NextTestWait);
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
    }
}