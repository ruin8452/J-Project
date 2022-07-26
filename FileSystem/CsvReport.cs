using J_Project.Manager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace J_Project.FileSystem
{
    public enum FileWriteType
    {
        OVER_WRITE,         // 덮어쓰기
        CONTINUE_WRITE,     // 이어쓰기
        NEW_WRITE           // 새로쓰기
    }

    /**
     *  @brief CSV 보고서
     *  @details CSV 보고서 관리를 담당하는 클래스
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class CsvReport
    {
        #region 싱글톤 패턴 구현
        private static CsvReport SingleTonObj = null;

        private CsvReport() { }

        /**
         *  @brief 해당 클래스의 객체 획득
         *  @details 싱글톤 패턴 적용으로 인한 인스턴스 획득 함수
         *  
         *  @param
         *  
         *  @return 해당 클래스의 인스턴스
         */
        public static CsvReport GetObj()
        {
            if (SingleTonObj == null) SingleTonObj = new CsvReport();
            return SingleTonObj;
        }
        #endregion

        /**
         *  @brief 리포트 파일 삭제
         *  @details 기존에 있는 리포트 파일을 삭제한다
         *  
         *  @param string path - 파일 경로
         *  
         *  @return
         */
        public void ReportFileDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch(Exception e)
            {
                Debug.WriteLine("보고서 파일 삭제 실패 : " + e.Message);
            }
        }

        /**
         *  @brief 파일 경로 설정
         *  @details 파일을 저장할 경로를 반환한다.(파일명 포함 - 파일명 형식 : {검사횟수}_{검사자}_{날짜}.csv)
         *  
         *  @param string checker - 검사자
         *  @param string savePath - 저장할 폴더 경로
         *  @param FileWriteType writeType - 기존 파일에 대한 처리(덮어쓰기, 이어쓰기, 새로쓰기)
         *  
         *  @return string 저장 경로(파일명 포함)
         */
        public string SetFilePath(string checker, string savePath, FileWriteType writeType)
        {
            int tryCount = 0;

            string[] reportName = Directory.GetFiles(savePath, "*.csv");

            foreach(string str in reportName)
            {
                string strTryCount = str.Split('\\').Last().Split('_').First();
                int temp = int.Parse(strTryCount);

                tryCount = Math.Max(tryCount, temp);
            }

            // 해당 장비의 가장 최근에 작성한 CSV파일
            if (writeType == FileWriteType.OVER_WRITE)
            {
                ReportFileDelete(reportName.Last()); // 해당 장비의 가장 최근에 작성한 CSV파일을 제거
            }
            else if (writeType == FileWriteType.CONTINUE_WRITE)
            { }
            else if (writeType == FileWriteType.NEW_WRITE)
                tryCount += 1;

            return string.Format(@"{0}\{1}_{2}_{3}.csv", savePath, tryCount, checker, DateTime.Today.ToShortDateString());
        }

        /**
         *  @brief 파일 저장
         *  @details 파일에 데이터를 담아 저장한다
         *           한 줄의 데이터를 저장
         *  
         *  @param string path - 파일 경로
         *  @param params string[] datas - 저장할 데이터
         *  
         *  @return StateFlag 정상 수행 여부
         */
        public StateFlag ReportSave(string path, params string[] datas)
        {
            try
            {
                StringBuilder saveText = new StringBuilder();

                using (StreamWriter csvStream = new StreamWriter(path, true, Encoding.UTF8))
                {
                    for (int i = 0; i < datas.Length; i++)
                    {
                        saveText.Append(datas[i].ToString());
                        saveText.Append(",");
                    }
                    saveText.Remove(saveText.Length - 1, 1); // 마지막에 붙은 ','를 제거

                    csvStream.WriteLine(saveText);
                }

                return StateFlag.PASS;
            }
            catch(Exception)
            {
                return StateFlag.TEST_SAVE_FAIL;
            }
        }

        /**
         *  @brief 파일 저장
         *  @details 파일에 데이터를 담아 저장한다
         *           여러 줄의 데이터를 저장
         *  
         *  @param string path - 파일 경로
         *  @param List<string[]> testData - 저장할 데이터
         *  
         *  @return StateFlag 정상 수행 여부
         */
        public StateFlag ReportSave(string path, List<string[]> testData)
        {
            using StreamWriter csvStream = new StreamWriter(path, false, Encoding.UTF8);

            foreach (var datas in testData)
            {
                try
                {
                    string saveStr;

                    if (datas == null)
                        saveStr = "-";
                    else
                        saveStr = string.Join(",", datas);

                    csvStream.WriteLine(saveStr);
                }
                catch (Exception)
                {
                    return StateFlag.TEST_SAVE_FAIL;
                }
            }

            return StateFlag.PASS;
        }

        /**
         *  @brief 불합격한 테스트 필터링
         *  @details 경로에 있는 보고서 파일을 검사하여 불합격한 테스트를 필터링 하는 함수
         *  
         *  @param string path - 파일 경로
         *  
         *  @return List<string> 불합격한 테스트명 리스트
         */
        public List<string> FailTest(string path)
        {
            List<string> TestName = new List<string>();

            foreach (var data in CsvReader(path))
            {
                if (data.Contains("NG(불합격)"))
                    TestName.Add(data[1]);
            }

            return TestName;
        }

        /**
         *  @brief 보고서 변환기
         *  @details CSV 보고서를 엑셀 보고서로 변환하는 함수
         *  
         *  @param string csvTarget - 변환하고자 하는 CSV 보고서 경로
         *  @param string baseExcelReport - 엑셀 보고서 양식의 경로
         *  
         *  @return 
         */
        public void ReportConverter(string csvTarget, string baseExcelReport)
        {
            try
            {
                string savePath = csvTarget.Replace("CSV Report", "EXCEL Report").Replace(".csv", ".xlsx");

                ExcelReport.GetObj().SetPath(baseExcelReport, savePath);
                ExcelReport.GetObj().ReportSave(CsvReader(csvTarget));
            }
            catch (Exception e)
            {
                MessageBox.Show("엑셀 보고서 생성 실패 : " + e.Message);
            }
        }

        /**
         *  @brief CSV 보고서 리더기
         *  @details CSV 보고서를 읽는 함수
         *  
         *  @param string csvTargetPath - 읽고자 하는 CSV 보고서 경로
         *  
         *  @return List<string[]> 보고서에 저장되어 있는 테스트 데이터 리스트
         */
        public List<string[]> CsvReader(string csvTargetPath)
        {
            List<string[]> data = new List<string[]>();

            try
            {
                using (StreamReader reader = new StreamReader(csvTargetPath, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        string str = reader.ReadLine();
                        data.Add(str.Split(','));
                    }
                }
            }
            catch(Exception)
            {
                using (StreamWriter csvStream = new StreamWriter(csvTargetPath, true, Encoding.UTF8))
                {
                    csvStream.Write("");
                }
            }
            return data;
        }
    }
}
