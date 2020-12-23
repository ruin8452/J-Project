using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief AC_고전압_알람 테스트 세팅 데이터
     *  @details AC_고전압_알람 테스트 세팅 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.12.17
     *  @version 1.0.0 : 2020.02.25 - 최초 작성
     *  @version 2.0.0 : 2020.12.17 - 싱글톤 제거, 배열 제거
     */
    public class AC_고전압_알람 : Test
    {
        public double AcVoltInit { get; set; } = 0;
        public double AcCurrInit { get; set; } = 0;
        public double AcFreqInit { get; set; } = 0;

        public double AcVoltUp { get; set; } = 0;
        public double AcErrRate { get; set; } = 0;

        public double AcVoltReturn { get; set; } = 0;

        public double LoadCurr { get; set; } = 0;

        public double CheckTiming { get; set; } = 0;
        public double LimitMaxVolt { get; set; } = 0;
        public double LimitMinVolt { get; set; } = 0;

        public double Delay1 { get; set; } = 0;
        public double Delay2 { get; set; } = 0;
        public double Delay3 { get; set; } = 0;
        public double Delay4 { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}