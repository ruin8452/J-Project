using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief 돌입전류 테스트 세팅 데이터
     *  @details 돌입전류 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.12.28
     *  @version 1.0.0
     */
    public class 돌입전류 : Test
    {
        public double AcVolt { get; set; } = 0;
        public double AcCurr { get; set; } = 0;
        public double AcFreq { get; set; } = 0;

        public double LimitInrush { get; set; } = 0;

        public double NextTestWait { get; set; } = 0;
    }
}