using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// 사용자 활동 로그 모델
    public class UserLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public UserRole UserRole { get; set; }
        public UserActionType ActionType { get; set; }
        public string ActionDescription { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Details { get; set; }  // JSON 문자열
        public DateTime CreatedAt { get; set; }

        public UserLog()
        {
            CreatedAt = DateTime.Now;
        }
    }

    /// 사용자 활동 유형
    public enum UserActionType
    {
        Login,              // 로그인
        Logout,             // 로그아웃
        CreateUser,         // 사용자 생성
        UpdateUser,         // 사용자 수정
        DeleteUser,         // 사용자 삭제
        ResetPassword,      // 비밀번호 초기화
        CallOHT,            // OHT 호출
        ExportData,         // 데이터 내보내기
        ViewInspection,     // 검사 이력 조회
        ChangeSettings,     // 설정 변경
        Other               // 기타
    }
}
