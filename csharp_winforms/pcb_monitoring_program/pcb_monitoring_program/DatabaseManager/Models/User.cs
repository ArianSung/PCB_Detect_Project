using System;

namespace pcb_monitoring_program.DatabaseManager.Models
{
    /// <summary>
    /// 사용자 계정 모델 (v3.1 스키마)
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }

        public User()
        {
            IsActive = true;
            CreatedAt = DateTime.Now;
        }
    }

    /// 사용자 권한 레벨
    public enum UserRole
    {
        Admin,      // 관리자 - 모든 권한
        Operator,   // 작업자 - 조회, 내보내기, OHT 호출
        Viewer      // 조회자 - 읽기 전용
    }
}
