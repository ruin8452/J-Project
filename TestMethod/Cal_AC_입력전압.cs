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
     *  @brief Cal_AC_입력전압 테스트 세팅 데이터
     *  @details Cal_AC_입력전압 테스트 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class Cal_AC_입력전압 : Test
    {
        public double AcVoltLower  { get; set; } = 0;
        public double AcCurrLower  { get; set; } = 0;
        public double AcFreqLower  { get; set; } = 0;

        public double AcVoltUpper  { get; set; } = 0;
        public double AcCurrUpper  { get; set; } = 0;
        public double AcFreqUpper  { get; set; } = 0;

        public double Delay1       { get; set; } = 0;
        public double Delay2       { get; set; } = 0;
        public double Delay3       { get; set; } = 0;
        public double Delay4       { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}