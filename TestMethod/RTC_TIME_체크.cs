using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief RTC_TIME_체크 테스트 세팅 데이터
     *  @details RTC_TIME_체크 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class RTC_TIME_체크 : Test
    {
        public double AcVolt { get; set; } = 0;
        public double AcCurr { get; set; } = 0;
        public double AcFreq { get; set; } = 0;

        public double LoadCurr { get; set; } = 0;

        public double TimeErrRate { get; set; } = 0;
        public double TimeErrRate2 { get; set; } = 0;

        public double Delay1 { get; set; } = 0;
        public double Delay2 { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;

        public double AcVolt2 { get; set; } = 0;
        public double LoadCurr2 { get; set; } = 0;
    }
}