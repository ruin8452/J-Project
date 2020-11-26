using J_Project.Manager.EventArgsClass;
using J_Project.ViewModel.TestItem;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace J_Project.Manager
{
    public class TestExecution
    {
        List<TestItemUint> TestList = new List<TestItemUint>();

        public event EventHandler<TestRunCheckEventArgs>  BeforeAutoTestStart;
        public event EventHandler<TestStartEventArgs>     AutoTestStart;
        public event EventHandler<UnitTestStartEventArgs> UnitTestStart;
        public event EventHandler<UnitTestEndEventArgs>   UnitTestEnd;
        public event EventHandler<TestEndEventArgs>       AutoTestEnd;
        public event EventHandler<TestRunCheckEventArgs>  AfterAutoTestEnd;

        public event EventHandler<TestRunCheckEventArgs>  AutoTestPause;
        public event EventHandler<TestRunCheckEventArgs>  AutoTestStop;

        private DispatcherTimer AutoTestTimer = new DispatcherTimer();

        public TestExecution()
        {
            AutoTestTimer.Interval = TimeSpan.FromMilliseconds(50);
            AutoTestTimer.Tick += new EventHandler((object send, EventArgs e) =>
            {
                TestExecutionMethod();
            });
        }

        public void TestStart(List<TestItemUint> testList)
        {
            TestIndex = 0;
            AutoTestStepNum = 0;
            isNextTest = true;

            TestList = testList;
            OnBeforeAutoTestStart(new TestRunCheckEventArgs(true));
            AutoTestTimer.Start();
        }

        public void TestContinue()
        {
            // 끊어진 테스트부터 다시 시작
            isNextTest = true;
            AutoTestStepNum = 0;

            ((AllTestVM)TestList[TestIndex].TestExeUi.DataContext).UiReset();

            AutoTestTimer.Start();
        }

        public void TestPause()
        {
            AutoTestTimer.Stop();
            OnAutoTestPause(new TestRunCheckEventArgs(false));
        }

        public void TestStop()
        {
            TestIndex = 0;
            AutoTestStepNum = 0;
            isNextTest = true;

            AutoTestTimer.Stop();
            OnAutoTestStop(new TestRunCheckEventArgs(false));
        }

        // 테스트 자동 실행 메소드(타이머)
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
                        OnAfterAutoTestEnd(new TestRunCheckEventArgs(false));
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

        public void OnBeforeAutoTestStart(TestRunCheckEventArgs e)
        {
            BeforeAutoTestStart?.Invoke(this, e);
        }
        public void OnAutoTestStart(TestStartEventArgs e)
        {
            AutoTestStart?.Invoke(this, e);
        }
        public void OnUnitTestStart(UnitTestStartEventArgs e)
        {
            UnitTestStart?.Invoke(this, e);
        }
        public void OnUnitTestEnd(UnitTestEndEventArgs e)
        {
            UnitTestEnd?.Invoke(this, e);
        }
        public void OnAutoTestEnd(TestEndEventArgs e)
        {
            AutoTestEnd?.Invoke(this, e);
        }
        public void OnAfterAutoTestEnd(TestRunCheckEventArgs e)
        {
            AfterAutoTestEnd?.Invoke(this, e);
        }


        public void OnAutoTestPause(TestRunCheckEventArgs e)
        {
            AutoTestPause?.Invoke(this, e);
        }
        public void OnAutoTestStop(TestRunCheckEventArgs e)
        {
            AutoTestStop?.Invoke(this, e);
        }
    }
}