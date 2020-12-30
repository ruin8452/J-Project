namespace J_Project.TestMethod
{
    /**
     *  @brief AC_저전압_알람 테스트 세팅 데이터
     *  @details AC_저전압_알람 테스트 세팅 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class AC_저전압_알람 : Test
    {
        public double AcVoltInit   { get; set; } = 0;
        public double AcCurrInit   { get; set; } = 0;
        public double AcFreqInit   { get; set; } = 0;

        public double AcVoltDrop   { get; set; } = 0;
        public double AcErrRate    { get; set; } = 0;

        public double AcVoltReturn { get; set; } = 0;

        public double LoadCurr     { get; set; } = 0;

        public double CheckTiming  { get; set; } = 0;
        public double LimitMaxVolt { get; set; } = 0;
        public double LimitMinVolt { get; set; } = 0;

        public double Delay1       { get; set; } = 0;
        public double Delay2       { get; set; } = 0;
        public double Delay3       { get; set; } = 0;
        public double Delay4       { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}
