using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.FileSystem
{
    [ImplementPropertyChanged]
    public class Logger
    {
        public StringBuilder Log { get; set; }

        #region 싱글톤 패턴 구현
        private static Logger SingleTonObj = null;

        private Logger()
        {
            Log = new StringBuilder();
        }

        /**
         *  @brief 해당 클래스의 객체 획득
         *  @details 싱글톤 패턴 적용으로 인한 인스턴스 획득 함수
         *  
         *  @param
         *  
         *  @return 해당 클래스의 인스턴스
         */
        public static Logger GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new Logger();
            return SingleTonObj;
        }
        #endregion

        public void AddLogging(string logText)
        {
            Log.Append(logText);
        }
    }
}
