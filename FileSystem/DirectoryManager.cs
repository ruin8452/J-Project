using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.FileSystem
{
    /**
     *  @brief 폴더 관리자
     *  @details 테스트 운영시 필요한 폴더들을 관리하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    class DirectoryManager
    {
        public static string FirstCsvPath; // 1차 테스트 CSV 보고서 경로
        public static string FirstExcelPath; // 1차 테스트 CSV 보고서 경로

        public static string SecondCsvPath; // 2차 테스트 CSV 보고서 경로
        public static string SecondExcelPath; // 2차 테스트 EXCEL 보고서 경로

        /**
         *  @brief 세팅 폴더 생성
         *  @details 세팅 파일을 저장하는 폴더를 생성
         *  
         *  @param 
         *  
         *  @return 폴더트리 생성 성공 여부(true : 폴더트리 생성 성공, false : 폴더트리 생성 실패)
         */
        public static bool CreateSettingFolder()
        {
            if (Directory.Exists(Environment.CurrentDirectory + @"\Setting") == false)
            {
                try
                {
                    Directory.CreateDirectory(Environment.CurrentDirectory + @"\Setting");
                    return true;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("세팅 폴더 생성 실패 : " + e.Message);
                    return false;
                }
            }
            else
                return true;
        }

        /**
         *  @brief 보고서 폴더트리 생성
         *  @details 보고서팅 파일을 저장하는 폴더트리를 생성
         *  
         *  @param string modelName - 모델명
         *  @param string serialNum - 시리얼 번호
         *  @param string savePath - 루트 경로
         *  
         *  @return 폴더트리 생성 성공 여부(true : 폴더트리 생성 성공, false : 폴더트리 생성 실패)
         */
        public static bool CreateReportFolderTree(string modelName, string serialNum, string savePath)
        {
            try
            {
                if (string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(serialNum) || string.IsNullOrEmpty(savePath))
                {
                    throw new Exception("기본 정보가 누락되었습니다.");
                }

                FirstCsvPath = Directory.CreateDirectory($@"{savePath}\{modelName}\{serialNum}\양산 검사\CSV Report").FullName;
                FirstExcelPath = Directory.CreateDirectory($@"{savePath}\{modelName}\{serialNum}\양산 검사\EXCEL Report").FullName;

                SecondCsvPath = Directory.CreateDirectory($@"{savePath}\{modelName}\{serialNum}\출하 검사\CSV Report").FullName;
                SecondExcelPath = Directory.CreateDirectory($@"{savePath}\{modelName}\{serialNum}\출하 검사\EXCEL Report").FullName;

                return true;
            }
            catch(Exception e)
            {
                Debug.WriteLine("리포트 폴더트리 생성 실패 : " + e.Message);
                return false;
            }
        }

        /**
         *  @brief 로그 폴더 생성
         *  @details 로그 파일을 저장하는 폴더를 생성
         *  
         *  @param 
         *  
         *  @return 폴더트리 생성 성공 여부(true : 폴더트리 생성 성공, false : 폴더트리 생성 실패)
         */
        public static bool CreateLogFolderTree()
        {
            try
            {
                string path = Environment.CurrentDirectory;

                if (Directory.Exists(path + "\\LogData") == false)
                    Directory.CreateDirectory(path + "\\LogData");

                path += "\\LogData";

                if (Directory.Exists(path + "\\" + DateTime.Today.ToShortDateString()) == false)
                    Directory.CreateDirectory(path + "\\" + DateTime.Today.ToShortDateString());

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("로그 폴더트리 생성 실패 : " + e.Message);
                return false;
            }
        }
    }
}
