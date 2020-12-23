using J_Project.FileSystem;
using System.Collections.ObjectModel;

namespace J_Project.TestMethod
{
    /**
     *  @brief 초기세팅 테스트 세팅 데이터
     *  @details 초기세팅 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class 초기세팅 : Test
    {
        public double DcVolt { get; set; } = 0;
        public double DcCurr { get; set; } = 0;

        public double Delay1 { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}