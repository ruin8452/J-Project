using J_Project.Manager.EventArgsClass;
using J_Project.ViewModel.TestItem;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace J_Project.Manager
{
    /**
     *  @brief 테스트 실행 엔진
     *  @details 테스트를 실행시키고 이벤트를 발생시키는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class TestExecution
    {
        List<TestItemUnit> TestList = new List<TestItemUnit>();

        public event EventHandler<TestRunCheckEventArgs>  BeforeAutoTestStart;
        public event EventHandler<TestStartEventArgs>     AutoTestStart;
        public event EventHandler<UnitTestStartEventArgs> UnitTestStart;
        public event EventHandler<UnitTestEndEventArgs>   UnitTestEnd;
        public event EventHandler<TestEndEventArgs>       AutoTestEnd;
        public event EventHandler<EventArgs>  AfterAutoTestEnd;

        public event EventHandler<EventArgs>  AutoTestPause;
        public event EventHandler<EventArgs>  AutoTestStop;

        private DispatcherTimer AutoTestTimer = new DispatcherTimer();

        public TestExecution()
        {
            AutoTestTimer.Interval = TimeSpan.FromMilliseconds(50);
            AutoTestTimer.Tick += new EventHandler((object send, EventArgs e) =>
            {
                TestExecutionMethod();
            });
        }

        /**
         *  @brief 테스트 시작
         *  @details 테스트 시작 시 변수를 초기화하고, 시작 이벤트를 발생
         *  
         *  @param List<TestItemUint> testList - 체크된 테스트 리스트
         *  
         *  @return
         */
        public void TestStart(List<TestItemUnit> testList)
        {
            TestIndex = 0;
            AutoTestStepNum = 0;
            isNextTest = true;

            TestList = testList;
            OnBeforeAutoTestStart(new TestRunCheckEventArgs(true));
            AutoTestTimer.Start();
        }

        /**
         *  @brief 테스트 이어하기
         *  @details 테스트 이어하기 시 변수를 설정하고, 테스트를 시작
         *  
         *  @param
         *  
         *  @return
         */
        public void TestContinue()
        {
            // 끊어진 테스트부터 다시 시작
            isNextTest = true;
            AutoTestStepNum = 0;

            ((AllTestVM)TestList[TestIndex].TestExeUi.DataContext).UiReset();

            AutoTestTimer.Start();
        }

        /**
         *  @brief 테스트 일시정지
         *  @details 테스트 일시정지 시 테스트를 멈추고, 일시정지 이벤트를 발생
         *  
         *  @param
         *  
         *  @return
         */
        public void TestPause()
        {
            AutoTestTimer.Stop();
            OnAutoTestPause(new TestRunCheckEventArgs(false));
        }

        /**
         *  @brief 테스트 정지
         *  @details 테스트 정지 시, 정지 이벤트를 발생
         *  
         *  @param
         *  
         *  @return
         */
        public void TestStop()
        {
            TestIndex = 0;
            AutoTestStepNum = 0;
            isNextTest = true;

            AutoTestTimer.Stop();
            OnAutoTestStop(new TestRunCheckEventArgs(false));
        }

        /**
         *  @brief 테스트 자동 실행기
         *  @details 타이머를 통해 테스트를 가동시킨다. 테스트를 단계별로 실행시키고 결과에 따라 처리한다
         *  
         *  @param
         *  
         *  @return
         */
        int TestIndex = 0;  // 테스트의 인덱스 변수
        bool isNextTest = true;  // 다음 테스트로 인덱스가 변경되었는지 판단하는 변수
        int AutoTestStepNum = 0;
        private void TestExecutionMethod()
        {
            string resultStr, reasonStr;
            int jumpIndex = -1;

            if (isNextTest)
            {
                OnAutoTestStart(new TestStartEventArgs(TestList[TestIndex].TestIndex, TestList[TestIndex].CaseIndex));
                isNextTest = false;
            }

            OnUnitTestStart(new UnitTestStartEventArgs(TestList[TestIndex].TestExeUi.DataContext, AutoTestStepNum, ((AllTestVM)TestList[TestIndex].TestExeUi.DataContext).TotalStepNum));
            StateFlag result = ((AllTestVM)TestList[TestIndex].TestExeUi.DataContext).TestSeq(TestList[TestIndex].CaseIndex, AutoTestStepNum, ref jumpIndex);
            OnUnitTestEnd(new UnitTestEndEventArgs(TestList[TestIndex].TestIndex, TestList[TestIndex].CaseIndex, AutoTestStepNum, result));

            Fault.FaultCheck(result);

            // 테스트 세부 단계 실행 결과에 따른 분기
            switch(result)
            {
                case StateFlag.WAIT:
                    break;

                case StateFlag.TEST_PAUSE:
                    TestPause();
                    break;


                case StateFlag.INIT_END:
                case StateFlag.TEST_END:
                    Fault.FaultResultReturn(out resultStr, out reasonStr);
                    OnAutoTestEnd(new TestEndEventArgs(TestList[TestIndex].TestIndex, TestList[TestIndex].CaseIndex, resultStr, reasonStr));
                    Fault.FaultReset();

                    isNextTest = true;

                    TestIndex++;
                    if (TestIndex >= TestList.Count) // 마지막 테스트였을 경우
                    {
                        AutoTestTimer.Stop();
                        OnAfterAutoTestEnd(new EventArgs());
                    }
                    else
                    {
                        AutoTestStepNum = 0;
                    }
                    break;

                default:
                    if (jumpIndex != -1)
                        AutoTestStepNum = jumpIndex;
                    else
                        AutoTestStepNum++;
                    break;
            }
        }

        /**
         *  @brief 테스트 실행 전 이벤트
         *  @details 테스트가 실행되기 전에 이벤트를 발생시킨다
         *  
         *  @param TestRunCheckEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnBeforeAutoTestStart(TestRunCheckEventArgs e)
        {
            BeforeAutoTestStart?.Invoke(this, e);
        }
        /**
         *  @brief 테스트 실행 이벤트
         *  @details 각각의 테스트가 실행될 때 이벤트를 발생시킨다
         *  
         *  @param TestStartEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnAutoTestStart(TestStartEventArgs e)
        {
            AutoTestStart?.Invoke(this, e);
        }
        /**
         *  @brief 세부 단계 테스트 실행 이벤트
         *  @details 테스트의 세부 단계가 실행될 때 이벤트를 발생시킨다
         *  
         *  @param UnitTestStartEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnUnitTestStart(UnitTestStartEventArgs e)
        {
            UnitTestStart?.Invoke(this, e);
        }
        /**
         *  @brief 세부 단계 테스트 종료 이벤트
         *  @details 테스트의 세부 단계가 종료될 때 이벤트를 발생시킨다
         *  
         *  @param UnitTestEndEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnUnitTestEnd(UnitTestEndEventArgs e)
        {
            UnitTestEnd?.Invoke(this, e);
        }
        /**
         *  @brief 테스트 종료 이벤트
         *  @details 테스트가 종료될 때 이벤트를 발생시킨다
         *  
         *  @param TestEndEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnAutoTestEnd(TestEndEventArgs e)
        {
            AutoTestEnd?.Invoke(this, e);
        }
        /**
         *  @brief 모든 테스트 종료 이벤트
         *  @details 모든 테스트가 종료됐을 때 이벤트를 발생시킨다
         *  
         *  @param TestRunCheckEventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnAfterAutoTestEnd(EventArgs e)
        {
            AfterAutoTestEnd?.Invoke(this, e);
        }


        /**
         *  @brief 테스트 일시정지 이벤트
         *  @details 테스트가 일시정지됐을 때 이벤트를 발생시킨다
         *  
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnAutoTestPause(EventArgs e)
        {
            AutoTestPause?.Invoke(this, e);
        }
        /**
         *  @brief 테스트 정지 이벤트
         *  @details 테스트가 정지됐을 때 이벤트를 발생시킨다
         *  
         *  @param EventArgs e - 이벤트 변수
         *  
         *  @return
         */
        public void OnAutoTestStop(EventArgs e)
        {
            AutoTestStop?.Invoke(this, e);
        }
    }
}