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
     *  @brief 장비ID에 대한 데이터를 관리
     *  @details 장비ID에 대한 데이터를 저장, 로드 하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    [ImplementPropertyChanged]
    public class EquiConnectID
    {
        public string AcSourceID { get; set; }
        public string DcSourceID { get; set; }
        public string LoadID { get; set; }
        public string PmID { get; set; }
        public string Dmm1ID { get; set; }
        public string Dmm2ID { get; set; }
        public string OscID { get; set; }
        public string RectID { get; set; }
        public string RemoteID { get; set; }

        public bool AutoConnect { get; set; }

        #region 싱글톤 패턴 구현
        private static EquiConnectID SingleTonObj = null;

        private EquiConnectID()
        {
        }

        public static EquiConnectID GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new EquiConnectID();
            return SingleTonObj;
        }
        #endregion 싱글톤 패턴 구현

        /**
         *  @brief 데이터 저장
         *  @details 변수에 저장되어 있는 장비ID 데이터를 ini파일에 저장
         *
         *  @param 
         *
         *  @return
         */
        public static void Save()
        {
            Setting.WriteSetting(GetObj(), @"\Setting\EquiId.ini");
        }

        /**
         *  @brief 데이터 불러오기
         *  @details ini파일에 저장되어 있는 장비ID 데이터를 변수로 불러오기
         *
         *  @param 
         *
         *  @return
         */
        public static void Load()
        {
            Setting.ReadSetting(GetObj(), @"\Setting\EquiId.ini");
        }
    }
}
