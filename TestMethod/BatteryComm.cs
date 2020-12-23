using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using J_Project.TestMethod.EventArgsClass;
using J_Project.ViewModel.SubWindow;
using PropertyChanged;

namespace J_Project.TestMethod
{
    /**
     *  @brief 배터리 통신 테스트 세팅 데이터
     *  @details 배터리 통신 테스트 세팅 데이터 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class BatteryComm : Test
    {
        public double AcVolt       { get; set; } = 0;
        public double AcCurr       { get; set; } = 0;
        public double AcFreq       { get; set; } = 0;

        public double Delay1       { get; set; } = 0;
        public double NextTestWait { get; set; } = 0;
    }
}
