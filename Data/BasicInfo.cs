using J_Project.FileSystem;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.Data
{
    /**
     *  @brief 기본정보에 대한 데이터를 관리
     *  @details 기본정보에 대한 데이터를 저장, 로드 하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class BasicInfo
    {
        public string Checker { get; set; }
        public string ModelName { get; set; }
        public string ProductCode { get; set; }
        public string SerialNumber { get; set; }
        public string DcdcSerial { get; set; }
        public string PfcSerial { get; set; }
        public string McuSerial { get; set; }
        public string CheckDate;
        public string HwVersion { get; set; }
        public string SwVersion { get; set; }
        public string TestResult;
        public string DcdcNumber { get; set; }
        public string PfcNumber { get; set; }
        public string McuNumber { get; set; }
        public string FirstReportOpenPath { get; set; }
        public string SecondReportOpenPath { get; set; }
        public string ReportSavePath { get; set; }

        #region 싱글톤 패턴 구현
        private static BasicInfo SingleTonObj = null;

        private BasicInfo()
        {
        }

        public static BasicInfo GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new BasicInfo();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        /**
         *  @brief 데이터 저장
         *  @details 변수에 저장되어 있는 기본정보 데이터를 ini파일에 저장
         *
         *  @param 
         *
         *  @return
         */
        public static void Save()
        {
            Setting.WriteSetting(GetObj(), @"\Setting\BasicInfo.ini");
        }

        /**
         *  @brief 데이터 불러오기
         *  @details ini파일에 저장되어 있는 기본정보 데이터를 변수로 불러오기
         *
         *  @param 
         *
         *  @return
         */
        public static void Load()
        {
            Setting.ReadSetting(GetObj(), @"\Setting\BasicInfo.ini");
        }
    }
}
