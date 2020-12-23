using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.ViewModel.SubWindow;

namespace J_Project.TestMethod
{
    /**
     *  @brief Cal_DC_출력전압 테스트 세팅 데이터
     *  @details Cal_DC_출력전압 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class Cal_DC_출력전압 : Test
    {
        public double AcVolt       { get; set; } = 0;
        public double AcCurr       { get; set; } = 0;
        public double AcFreq       { get; set; } = 0;

        public double LoadCurr     { get; set; } = 0;

        public double DacLowerRef  { get; set; } = 0;
        public double DacUpperRef  { get; set; } = 0;

        public double AdcLowerRef  { get; set; } = 0;
        public double AdcUpperRef  { get; set; } = 0;

        public double DefaultRef   { get; set; } = 0;

        public double Delay1       { get; set; } = 0;
        public double Delay2       { get; set; } = 0;
        public double Delay3       { get; set; } = 0;
        public double Delay4       { get; set; } = 0;
        public double Delay5       { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}
