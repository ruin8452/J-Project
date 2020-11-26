namespace J_Project.Manager
{
    public enum StateFlag
    {
        PASS,

        WAIT,

        TEST_PAUSE,

        RECT_CONNECT_ERR,
        RECT_RESET_ERR,

        BATTERY_COMM_ERR,
        REMOTE_COMM_ERR,
        ID_ERROR,
        LED_ERROR,
        LOCAL_SWITCH_ERR,

        AC_VOLT_SET_ERR,
        AC_CURR_SET_ERR,
        AC_FREQ_SET_ERR,
        AC_ON_ERR,
        AC_OFF_ERR,
        AC_OUTPUT_ERR,

        DC_VOLT_SET_ERR,
        DC_CURR_SET_ERR,
        DC_ON_ERR,
        DC_OFF_ERR,
        DC_OUTPUT_ERR,

        LOAD_SET_ERR,
        LOAD_ON_ERR,
        LOAD_OFF_ERR,

        PM_AC_VOLT_RECIVE_ERR,
        PM_PF_RECIVE_ERR,
        PM_AC_POWER_RECIVE_ERR,
        PM_CURR_RMS_RECIVE_ERR,

        DMM1_SETTING_ERR,
        DMM2_SETTING_ERR,
        DMM_REAL_DC_VOLT_RECIVE_ERR,

        OSC_SETTING_ERR,

        RTC_ERR,
        AC_CAL_ERR,
        DC_VOLT_CAL_ERR,
        DC_CURR_CAL_ERR,
        OUTPUT_ERR,

        FAULT_ERROR,
        CONDITION_FAIL,

        TEST_NAME_NO_MATCH,
        TEST_SAVE_FAIL,
        PATH_ERR,
        EXCEL_ERR,
        NULL_VAULE_ERR,

        INIT_END,
        TEST_END,

        NORMAL_ERR
    }

    public class Fault
    {
        static string Result = "합격";
        static string Reason = "정상";

        public static void FaultReset()
        {
            Result = "합격";
            Reason = "정상";
        }

        public static void FaultResultReturn(out string result, out string reason)
        {
            result = Result;
            reason = Reason;
        }

        public static void FaultCheck(StateFlag state)
        {
            switch(state)
            {
                // 접속 관련 오류
                case StateFlag.RECT_CONNECT_ERR:
                    Result = "불합격";
                    Reason = "정류기 접속 오류";
                    break;
                case StateFlag.RECT_RESET_ERR:
                    Result = "불합격";
                    Reason = "정류기 리셋 오류";
                    break;

                case StateFlag.BATTERY_COMM_ERR:
                    Result = "테스트 강제 종료";
                    Reason = "배터리 통신 오류";
                    break;
                case StateFlag.REMOTE_COMM_ERR:
                    Result = "테스트 강제 종료";
                    Reason = "원격 통신 오류";
                    break;
                // LED 관련 오류
                case StateFlag.LED_ERROR:
                    Result = "테스트 강제 종료";
                    Reason = "LED 비일치";
                    break;
                // ID 관련 오류
                case StateFlag.ID_ERROR:
                    Result = "테스트 강제 종료";
                    Reason = "ID 비일치";
                    break;
                // 로컬 스위치 관련 오류
                case StateFlag.LOCAL_SWITCH_ERR:
                    Result = "테스트 강제 종료";
                    Reason = "로컬 스위치 오류";
                    break;

                // AC 파워 관련 오류
                case StateFlag.AC_VOLT_SET_ERR:
                    Result = "불합격";
                    Reason = "AC 파워 전압 설정 오류";
                    break;
                case StateFlag.AC_CURR_SET_ERR:
                    Result = "불합격";
                    Reason = "AC 파워 전류 설정 오류";
                    break;
                case StateFlag.AC_FREQ_SET_ERR:
                    Result = "불합격";
                    Reason = "AC 파워 주파수 설정 오류";
                    break;
                case StateFlag.AC_ON_ERR:
                    Result = "불합격";
                    Reason = "AC 파워 ON 오류";
                    break;
                case StateFlag.AC_OFF_ERR:
                    Result = "불합격";
                    Reason = "AC 파워 OFF 오류";
                    break;
                case StateFlag.AC_OUTPUT_ERR:
                    Result = "불합격";
                    Reason = "AC 파워 출력 전압 모니터링 오류";
                    break;

                // DC 파워 관련 오류
                case StateFlag.DC_VOLT_SET_ERR:
                    Result = "불합격";
                    Reason = "DC 파워 전압 설정 오류";
                    break;
                case StateFlag.DC_CURR_SET_ERR:
                    Result = "불합격";
                    Reason = "DC 파워 전류 설정 오류";
                    break;
                case StateFlag.DC_ON_ERR:
                    Result = "불합격";
                    Reason = "DC 파워 ON 오류";
                    break;
                case StateFlag.DC_OFF_ERR:
                    Result = "불합격";
                    Reason = "DC 파워 OFF 오류";
                    break;
                case StateFlag.DC_OUTPUT_ERR:
                    Result = "불합격";
                    Reason = "DC 파워 출력 전압 모니터링 오류";
                    break;

                // 부하 관련 오류
                case StateFlag.LOAD_SET_ERR:
                    Result = "불합격";
                    Reason = "부하 전류 설정 오류";
                    break;
                case StateFlag.LOAD_ON_ERR:
                    Result = "불합격";
                    Reason = "부하 ON 오류";
                    break;
                case StateFlag.LOAD_OFF_ERR:
                    Result = "불합격";
                    Reason = "부하 OFF 오류";
                    break;

                // PM 관련 오류
                case StateFlag.PM_AC_VOLT_RECIVE_ERR:
                    Result = "불합격";
                    Reason = "파워미터 AC전압 측정값 수신 오류";
                    break;
                case StateFlag.PM_PF_RECIVE_ERR:
                    Result = "불합격";
                    Reason = "파워미터 역률 측정값 수신 오류";
                    break;
                case StateFlag.PM_AC_POWER_RECIVE_ERR:
                    Result = "불합격";
                    Reason = "파워미터 입력전력 측정값 수신 오류";
                    break;
                case StateFlag.PM_CURR_RMS_RECIVE_ERR:
                    Result = "불합격";
                    Reason = "파워미터 소비전류 측정값 수신 오류";
                    break;


                // DMM 관련 오류
                case StateFlag.DMM1_SETTING_ERR:
                    Result = "불합격";
                    Reason = "DMM1 세팅 오류";
                    break;
                case StateFlag.DMM2_SETTING_ERR:
                    Result = "불합격";
                    Reason = "DMM2 세팅 오류";
                    break;
                case StateFlag.DMM_REAL_DC_VOLT_RECIVE_ERR:
                    Result = "불합격";
                    Reason = "DMM DC전압 측정값 수신 오류";
                    break;

                // OSC 관련 오류
                case StateFlag.OSC_SETTING_ERR:
                    Result = "불합격";
                    Reason = "OSC 세팅 오류";
                    break;

                // RTC 관련 오류
                case StateFlag.RTC_ERR:
                    Result = "불합격";
                    Reason = "RTC 오류";
                    break;

                // 정류기 관련 오류
                case StateFlag.OUTPUT_ERR:
                    Result = "불합격";
                    Reason = "출력 전압 오류";
                    break;

                // Cal 관련 오류
                case StateFlag.AC_CAL_ERR:
                    Result = "테스트 강제 종료";
                    Reason = "AC Cal 오류";
                    break;
                case StateFlag.DC_VOLT_CAL_ERR:
                    Result = "테스트 강제 종료";
                    Reason = "DC Volt Cal 오류";
                    break;
                case StateFlag.DC_CURR_CAL_ERR:
                    Result = "테스트 강제 종료";
                    Reason = "DC Curr Cal 오류";
                    break;

                // Fault 관련 오류
                case StateFlag.FAULT_ERROR:
                    Result = "불합격";
                    Reason = "Fault 비일치";
                    break;

                // 합격 조건 실패
                case StateFlag.CONDITION_FAIL:
                    Result = "불합격";
                    Reason = "합격 조건 불만족";
                    break;

                // 성적서 저장 실패
                case StateFlag.TEST_NAME_NO_MATCH:
                    Result = "불합격";
                    Reason = "테스트 이름 비일치";
                    break;
                case StateFlag.TEST_SAVE_FAIL:
                    Result = "불합격";
                    Reason = "테스트 결과 저장 실패";
                    break;
                case StateFlag.PATH_ERR:
                    Result = "불합격";
                    Reason = "저장 경로를 찾을 수 없음";
                    break;
                case StateFlag.EXCEL_ERR:
                    Result = "불합격";
                    Reason = "비정상적인 엑셀 프로그램";
                    break;
                case StateFlag.NULL_VAULE_ERR:
                    Result = "불합격";
                    Reason = "결과값이 없음";
                    break;

                case StateFlag.NORMAL_ERR:
                    Result = "불합격";
                    Reason = "알 수 없는 오류";
                    break;

                case StateFlag.INIT_END:
                    Result = "합격";
                    Reason = "초기 세팅 완료";
                    break;
            }
        }
    }
}
