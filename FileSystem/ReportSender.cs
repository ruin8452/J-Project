﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace J_Project.FileSystem
{
    /**
     *  @brief 보고서 서버 전송 관련 클래스
     *  @details CVS 파일을 JSON 형식으로 변환하여 서버에 전송
     *
     *  @author SSW
     *  @date 2020.02.25
     *  @version 1.0.0
     */
    public class ReportSender
    {
        public static string FIRST_TEST { get { return "FirstTest"; } }
        public static string SECOND_TEST { get { return "SecondTest"; } }

        HttpWebRequest HttpSocket = null;

        /**
         *  @brief 전송 주소 설정
         *  @details 보고서를 전송하기 위한 주소를 설정한다(고정)
         *  
         *  @param
         *  
         *  @return
         */
        private void SetHttp()
        {
            HttpSocket = (HttpWebRequest)WebRequest.Create(string.Format($"http://59.12.34.202:2099/update_Kapjin_TestReport"));
            HttpSocket.Method = WebRequestMethods.Http.Post;
            HttpSocket.ContentType = "application/json";
        }
        /**
         *  @brief 전송 주소 설정
         *  @details 보고서를 전송하기 위한 주소를 설정한다(임의)
         *  
         *  @param string ip - IP
         *  @param string port - 포트번호
         *  
         *  @return
         */
        public void SetHttp(string ip, string port)
        {
            HttpSocket = WebRequest.CreateHttp(string.Format($"http://{ip}:{port}"));
            HttpSocket.Method = WebRequestMethods.Http.Post;
            HttpSocket.ContentType = "application/json";
        }

        /**
         *  @brief CSV-JSON 변환기
         *  @details CSV 보고서 데이터를 JSON 형식으로 변환한다.
         *  
         *  @param string reportType - 보고서 종류(양산, 출하)
         *  @param string filePath - CSV 파일 경로
         *  
         *  @return byte[] - 전송할 데이터
         */
        private byte[] ConvertCvsToJson(string reportType, string filePath)
        {
            JArray jDataArr = new JArray();
            List<string[]> DataList = new List<string[]>();

            StreamReader reader = new StreamReader(filePath, Encoding.UTF8);
            while (!reader.EndOfStream)
                DataList.Add(reader.ReadLine().Split(','));

            reader.Close();

            foreach (var temp in DataList)
            {
                if (temp.Length == 4)
                    jDataArr.Add(JObject.FromObject(new { Position = "B" + (int.Parse(temp[0]) + 11).ToString(), TestValue = temp[2], PassOrFail = temp[3] }));
                else
                    jDataArr.Add(JObject.FromObject(new
                    {
                        B7 = temp[1],   // 검사자
                        B5 = temp[2],   // 제품명
                        D5 = temp[3],   // 제품코드
                        D6 = temp[4],   // 시리얼 넘버
                        B8 = temp[5],   // DCDC 시리얼
                        D8 = temp[6],   // PFC 시리얼
                        H8 = temp[7],   // MCU 시리얼
                        D7 = temp[8],   // 검사일
                        H5 = temp[9],   // 하드웨어 버전
                        H6 = temp[10],  // 소프트웨어 버전
                        H7 = temp[11],  // 검사 결과
                        B9 = temp[12],  // DCDC 번호
                        D9 = temp[13],  // PFC 번호
                        G9 = temp[14]   // MCU 번호
                    }));
            }

            JObject jObject = new JObject
            {
                { reportType, JsonConvert.SerializeObject(jDataArr) }
            };

            var serializer1 = new DataContractJsonSerializer(typeof(string));
            var ms1 = new MemoryStream();
            serializer1.WriteObject(ms1, jObject.ToString());
            ms1.Position = 0;

            return ms1.ToArray();
        }

        /**
         *  @brief 데이터 전송
         *  @details 변한된 데이터를 소켓을 통해 서버로 전송한다
         *  
         *  @param byte[] data - 전송할 데이터
         *  
         *  @return string - 전송 결과
         */
        private string DataSend(byte[] data)
        {
            HttpSocket.ContentLength = data.Length;
            Stream sender = null;
            string result = "";

            try
            {
                sender = HttpSocket.GetRequestStream();
                sender.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            finally
            {
                if (sender != null)
                    sender.Close();
            }

            return result;
        }

        /**
         *  @brief 응답 수신
         *  @details 데이터 전송후 서버로부터 받은 응답을 수신
         *  
         *  @param
         *  
         *  @return string - 응답
         */
        private string DataReceive()
        {
            string result = null;
            Stream receiver = null;
            StreamReader streamReader = null;

            try
            {
                var aa = HttpSocket.GetResponse() as HttpWebResponse;
                receiver = aa.GetResponseStream();
                streamReader = new StreamReader(receiver);
                result = streamReader.ReadToEnd();

                aa.Close();
            }
            catch (WebException e)
            {
                result = e.Message;
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            finally
            {
                if (receiver != null) receiver.Close();
                if (streamReader != null) streamReader.Close();
            }
            return result;
        }

        /**
         *  @brief 해석기
         *  @details 서버로 받은 응답 데이터를 해석
         *  
         *  @param string str - 응답받은 데이터
         *  @param out string errMessage - 에러 메세지
         *  
         *  @return bool - 서버의 응답 결과
         */
        private bool Deserialize(string str, out string errMessage)
        {
            Dictionary<string, string> jsonDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
            JObject result = JObject.Parse(jsonDic["update_Kapjin_TestReportResult"]);

            bool succ = (result["succ"] == null) ? false : (bool)result["succ"];
            errMessage = (string)result["errMsg"];

            return succ;
        }

        /**
         *  @brief 보고서 전송
         *  @details CSV 데이터 -> JSON 객체 -> 바이트 배열로 변환 후 서버에 전송하는 함수
         *  
         *  @param string reportType - 보고서 종류(양산, 출하)
         *  @param string filePath - CSV 파일 경로
         *  
         *  @return string - 전송 결과
         */
        public string Reporsend(string reportType, string filePath)
        {
            SetHttp();
            byte[] dataByte = ConvertCvsToJson(reportType, filePath);
            string result = DataSend(dataByte);

            return result;
        }

        /**
         *  @brief 응답 수신
         *  @details 보고서 데이터를 보낸 후 서버로부터의 응답을 받는 함수
         *  
         *  @param
         *  
         *  @return string - 에러 메세지
         */
        public string AnswerReceive()
        {
            string result = DataReceive();
            Deserialize(result, out string errMsg);

            return errMsg;
        }
    }
}
