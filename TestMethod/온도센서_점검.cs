using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief 온도센서_점검 테스트 세팅 데이터
     *  @details 온도센서_점검 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class 온도센서_점검 : Test
    {
        public double AcVolt { get; set; } = 0;
        public double AcCurr { get; set; } = 0;
        public double AcFreq { get; set; } = 0;

        public double RoomTemp { get; set; } = 0;
        public double ErrorRate { get; set; } = 0;

        public double Delay1 { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}