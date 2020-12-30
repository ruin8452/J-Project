namespace J_Project.TestMethod
{
    /**
     *  @brief 누설전류 테스트 세팅 데이터
     *  @details 누설전류 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.12.28
     *  @version 1.0.0
     */
    public class 누설전류 : Test
    {
        public double AcVolt { get; set; } = 0;
        public double AcCurr { get; set; } = 0;
        public double AcFreq { get; set; } = 0;

        public double LoadCurr { get; set; } = 0;

        public double LimitLeakage { get; set; } = 0;

        public double Delay1 { get; set; } = 0;
        public double Delay2 { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}