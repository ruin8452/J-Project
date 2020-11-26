using J_Project.Manager;

// EXCEL 표준(xls)
using NPOI.HSSF.UserModel;

// EXCEL 공통
using NPOI.SS.UserModel;

// EXCEL 확장(xlsx)
using NPOI.XSSF.UserModel;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace J_Project.FileSystem
{
    /**
     *  @brief 엑셀 성적서 관련 클래스
     *  @details
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class ExcelReport
    {
        private string Checker = string.Empty;       /// <summary> 테스트 진행자     </summary>
        private string ModelName = string.Empty;     /// <summary> 모델명            </summary>
        private string ProductCode = string.Empty;   /// <summary> 제품 코드         </summary>
        private string SerialNumber = string.Empty;  /// <summary> 제품 시리얼 번호  </summary>
        private string DcdcSerial = string.Empty;    /// <summary> DCDC 시리얼 번호  </summary>
        private string PfcSerial = string.Empty;     /// <summary> PFC 시리얼 번호   </summary>
        private string McuSerial = string.Empty;     /// <summary> MCU 시리얼 번호   </summary>
        private string HwVersion = string.Empty;     /// <summary> 하드웨어 버전     </summary>
        private string SwVersion = string.Empty;     /// <summary> 펌웨어 버전       </summary>
        private string DcdcNumber = string.Empty;    /// <summary> DCDC 번호         </summary>
        private string PfcNumber = string.Empty;     /// <summary> PFC 번호          </summary>
        private string McuNumber = string.Empty;     /// <summary> MCU 번호          </summary>
        private string OpenPath = string.Empty;      /// <summary> 성적서 원본 경로  </summary>
        private string SavePath = string.Empty;      /// <summary> 성적서 저장 경로  </summary>

        #region 싱글톤 패턴 구현

        private static ExcelReport _excelReport = null;

        private ExcelReport()
        {
        }

        /**
         *  @brief 해당 클래스의 객체 획득
         *  @details 싱글톤 패턴 적용으로 인한 인스턴스 획득 함수
         *
         *  @param
         *
         *  @return 해당 클래스의 인스턴스
         */

        public static ExcelReport GetObj()
        {
            if (_excelReport == null) _excelReport = new ExcelReport();
            return _excelReport;
        }

        #endregion 싱글톤 패턴 구현

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /**
         *  @brief 기본 정보를 설정
         *  @details 기본 정보를 변수에 저장
         *
         *  @param checker - 테스트 진행자 이름
         *  @param modelName - 정류기 모델 이름
         *  @param productCode - 제품 코드
         *  @param serialNum - 제품 시리얼 넘버
         *  @param dcdcSerial - DCDC 시리얼 넘버
         *  @param pfcSerial - PFC 시리얼 넘버
         *  @param mcuSerial - MCU 시리얼 넘버
         *  @param hwVer - 하드웨어 버전
         *  @param swVer - 펌웨어 버전
         *  @param dcdcNum - DCDC 번호
         *  @param pfcNum - PFC 번호
         *  @param mcuNum - MCU 제품 번호
         *
         *  @return
         */
        public void SetReportInfo(string checker, string modelName, string productCode, string serialNum, 
            string dcdcSerial, string pfcSerial, string mcuSerial, string hwVer, string swVer, string dcdcNum, string pfcNum, string mcuNum)
        {
            Checker = checker;
            ModelName = modelName;
            ProductCode = productCode;
            SerialNumber = serialNum;
            DcdcSerial = dcdcSerial;
            PfcSerial = pfcSerial;
            McuSerial = mcuSerial;
            HwVersion = hwVer;
            SwVersion = swVer;
            DcdcNumber = dcdcNum;
            PfcNumber = pfcNum;
            McuNumber = mcuNum;
        }

        /**
         *  @brief 기본 정보를 설정
         *  @details 기본 정보를 변수에 저장
         *
         *  @param params string[] info - 기본 정보에 대한 데이터들
         *
         *  @return
         */
        public void SetReportInfo(params string[] info)
        {
            Checker = info[0];
            ModelName = info[1];
            ProductCode = info[2];
            SerialNumber = info[3];
            DcdcSerial = info[4];
            PfcSerial = info[5];
            McuSerial = info[6];
            HwVersion = info[7];
            SwVersion = info[8];
            DcdcNumber = info[9];
            PfcNumber = info[10];
            McuNumber = info[11];
        }

        public void SetPath(string openPath, string savaPath)
        {
            OpenPath = openPath;
            SavePath = savaPath;
        }

        /**
         *  @brief 엑셀 성적서 저장
         *  @details
         *
         *  @param testData 저장할 테스트 데이터
         *
         *  @return 수행 결과
         */
        public StateFlag ReportSave(List<string[]> testData)
        {
            IWorkbook workbook;
            ISheet sheet;

            var ver = "xlsx";

            StateFlag saveResult = StateFlag.NORMAL_ERR;

            ExcelProcessKill();

            try
            {
                workbook = GetWorkbook(OpenPath, ver);

                // 워크시트 정보 가져오기
                sheet = workbook.GetSheet("성적서");
            }
            catch (COMException e)
            {
                if ((uint)e.ErrorCode == 0x80080005)
                    saveResult = StateFlag.EXCEL_ERR;
                else
                    saveResult = StateFlag.PATH_ERR;

                return saveResult;
            }
            catch (InvalidOperationException)
            {
                saveResult = StateFlag.NULL_VAULE_ERR;
                return saveResult;
            }
            catch (Exception)
            {
                saveResult = StateFlag.TEST_SAVE_FAIL;
                return saveResult;
            }

            // 데이터 삽입
            foreach (var data in testData)
            {
                if (data[0] == "0")
                {
                    // 기본 정보 저장
                    GetCell(sheet, "B7").SetCellValue(data[1]); // B7, 검사자
                    GetCell(sheet, "B5").SetCellValue(data[2]); // B5, 모델명
                    GetCell(sheet, "D5").SetCellValue(data[3]); // D5, 제품코드
                    GetCell(sheet, "D6").SetCellValue(data[4]); // D6, 시리얼 번호
                    GetCell(sheet, "B8").SetCellValue(data[5]); // B8, DCDC 시리얼
                    GetCell(sheet, "D8").SetCellValue(data[6]); // D8, PFC 시리얼
                    GetCell(sheet, "H8").SetCellValue(data[7]); // H8, MCU 시리얼
                    GetCell(sheet, "D7").SetCellValue(data[8]); // D7, 검사 날짜
                    GetCell(sheet, "H5").SetCellValue(data[9]); // H5, 하드웨어 버전
                    GetCell(sheet, "H6").SetCellValue(data[10]); // H6, 소프트웨어 버전
                    GetCell(sheet, "H7").SetCellValue(data[11]); // H7, 검사 결과
                    GetCell(sheet, "B9").SetCellValue(data[12]); // B9, DCDC 번호
                    GetCell(sheet, "D9").SetCellValue(data[13]); // D9, PFC 번호
                    GetCell(sheet, "G9").SetCellValue(data[14]); // G9, MCU 번호
                }
                else
                    saveResult = WriteResultReport(sheet, int.Parse(data[0]), data[2], data[3]);
            }

            // 리포트 저장, 종료
            WriteExcel(workbook, SavePath);
            workbook.Close();

            return saveResult;
        }

        /**
         *  @brief 성적서에 테스트 결과 저장
         *  @details
         *
         *  @param workSheet 워크시트
         *  @param order 테스트 순서
         *  @param reson 테스트 값
         *  @param result 테스트 합불여부
         *
         *  @return 테스트 결과 플래그
         */
        private StateFlag WriteResultReport(ISheet workSheet, int order, string reson, string result)
        {
            int startRowNum = 10;
            int row = order + startRowNum;

            GetCell(workSheet, row, 6).SetCellValue(reson);
            GetCell(workSheet, row, 7).SetCellValue(result);

            return StateFlag.PASS;   // 매칭되는 테스트 항목에 데이터를 삽입한 경우
        }

        // Workbook 읽어드리기
        public IWorkbook GetWorkbook(string filename, string version)
        {
            try
            {
                using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                //표준 xls 버젼
                if ("xls".Equals(version))
                {
                    return new HSSFWorkbook(stream);
                }
                //확장 xlsx 버젼
                else if ("xlsx".Equals(version))
                {
                    return new XSSFWorkbook(stream);
                }
            }
            catch (Exception e)
            {
            }
            throw new NotSupportedException();
        }

        // Sheet로 부터 Row를 취득, 생성하기
        public IRow GetRow(ISheet sheet, int rownum)
        {
            var row = sheet.GetRow(rownum);
            if (row == null)
            {
                row = sheet.CreateRow(rownum);
            }
            return row;
        }

        // Row로 부터 Cell를 취득, 생성하기
        public ICell GetCell(IRow row, int cellnum)
        {
            var cell = row.GetCell(cellnum);
            if (cell == null)
            {
                cell = row.CreateCell(cellnum);
            }
            return cell;
        }

        public ICell GetCell(ISheet sheet, int rownum, int cellnum)
        {
            var row = GetRow(sheet, rownum);
            return GetCell(row, cellnum);
        }

        public ICell GetCell(ISheet sheet, string cell)
        {
            Regex rowFilter = new Regex("[0-9]");
            Regex columnFilter = new Regex("[A-Z]");

            int rowNum = int.Parse(rowFilter.Match(cell).Value) - 1;

            byte[] temp = Encoding.ASCII.GetBytes(columnFilter.Match(cell).Value);
            int columnNum = Convert.ToInt32(temp[0]) - 'A';

            var row = GetRow(sheet, rowNum);
            return GetCell(row, columnNum);
        }

        // 엑셀 저장
        public void WriteExcel(IWorkbook workbook, string filepath)
        {
            using var file = new FileStream(filepath, FileMode.Create, FileAccess.Write);
            workbook.Write(file);
        }

        /**
         *  @brief 엑셀 프로세스 킬 함수
         *  @details 테스트 진행시 성적서가 열려있으면 안되기 때문에 열려있는 모든 엑셀 프로세스를 종료
         *
         *  @param
         *
         *  @return
         */
        private void ExcelProcessKill()
        {
            Process[] processesList = Process.GetProcessesByName("Excel");

            if (processesList.Length < 1)
                return;

            foreach (var process in processesList)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}