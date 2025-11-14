using System;
using System.Windows.Forms;

namespace pcb_monitoring_program
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new AppStartContext());
        }
    }

    /// <summary>
    /// 애플리케이션 전체에서 로그인/메인 폼 전환을 담당하는 컨텍스트
    /// </summary>
    internal sealed class AppStartContext : ApplicationContext
    {
        private LoginForm? _loginForm;
        private MainForm? _mainForm;

        public AppStartContext()
        {
            ShowLogin();
        }

        private void ShowLogin()
        {
            _loginForm = new LoginForm();
            _loginForm.LoginSucceeded += HandleLoginSucceeded;
            _loginForm.FormClosed += HandleLoginClosed;
            _loginForm.Show();
        }

        private void HandleLoginSucceeded(object? sender, LoginSucceededEventArgs e)
        {
            _mainForm = new MainForm(e.UserId, e.Role);
            _mainForm.LogoutRequested += HandleLogoutRequested;
            _mainForm.FormClosed += HandleMainFormClosed;
            _mainForm.Show();
        }

        private void HandleLoginClosed(object? sender, FormClosedEventArgs e)
        {
            if (_loginForm != null)
            {
                _loginForm.LoginSucceeded -= HandleLoginSucceeded;
                _loginForm.FormClosed -= HandleLoginClosed;
                _loginForm = null;
            }

            if (_mainForm == null)
            {
                ExitThread();
            }
        }

        private void HandleLogoutRequested(object? sender, EventArgs e)
        {
            _mainForm?.Close();
        }

        private void HandleMainFormClosed(object? sender, FormClosedEventArgs e)
        {
            if (_mainForm != null)
            {
                _mainForm.LogoutRequested -= HandleLogoutRequested;
                _mainForm.FormClosed -= HandleMainFormClosed;
                _mainForm = null;
            }

            ShowLogin();
        }
    }
}
