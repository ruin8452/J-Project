using J_Project.Equipment;
using J_Project.FileSystem;
using J_Project.Manager;
using System.Text;

namespace J_Project
{
    public enum PowerFlag
    {
        AC_POWER,
        DC_POWER,
        LOAD
    }

    public class Test
    {
        public int MaxCase;

        public static string EquiConnectCheck(bool isTestStart, string TestType)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (AcSource.GetObj().IsConnected == false && TestOption.GetObj().IsFullAuto) stringBuilder.Append("AC 파워\n");
            if (DcSource.GetObj().IsConnected == false && TestType == "FirstTest") stringBuilder.Append("DC 파워\n");
            if (DcLoad.GetObj().IsConnected == false && TestOption.GetObj().IsFullAuto) stringBuilder.Append("Load\n");
            if (PowerMeter.GetObj().IsConnected == false) stringBuilder.Append("파워미터\n");
            if (Dmm1.GetObj().IsConnected == false) stringBuilder.Append("DMM1\n");
            if (Dmm2.GetObj().IsConnected == false && TestType == "FirstTest") stringBuilder.Append("DMM2\n");
            if (Oscilloscope.GetObj().IsConnected == false) stringBuilder.Append("오실로스코프\n");
            //if (Remote.GetObj().IsConnected == false)            stringBuilder.Append("원격\n");
            if (Rectifier.GetObj().IsConnected == false && !isTestStart) stringBuilder.Append("정류기\n");

            return stringBuilder.ToString();
        }


        /**
         *  @brief 데이터 저장
         *  @details 해당 테스트의 설정값을 ini파일에 저장한다
         *  
         *  @param
         *  
         *  @return
         */
        public static void Save(object classes, int caseNum)
        {
            Setting.WriteSetting(classes, caseNum, @"\Setting\TestSetting.ini");
        }

        /**
         *  @brief 데이터 로드
         *  @details ini파일에서 해당 테스트의 설정값을 불러온다
         *  
         *  @param
         *  
         *  @return
         */
        public static object Load(object classes, int caseNum)
        {
            Setting.ReadSetting(classes, caseNum, @"\Setting\TestSetting.ini");
            return classes;
        }
    }
}