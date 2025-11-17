using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static pcb_monitoring_program.Views.Monitoring.BoxMonitoringView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace pcb_monitoring_program.Views.Monitoring
{
    public partial class LineMonitoringView : UserControl
    {
        private readonly List<string> _alarmHistory = new List<string>();
        // 현재 라인 상태
        private LineStatus _currentStatus = LineStatus.Idle;

        // 누적 가동 시간
        private TimeSpan _runAccum = TimeSpan.Zero;

        // 마지막으로 "가동중"으로 들어간 시각
        private DateTime? _runStartTime = null;
        public enum LineStatus
        {
            Run,    // 가동중 (초록)
            Idle,   // 대기중 (노랑)
            Down    // 다운/알람 (빨강)
        }
        public LineMonitoringView()
        {
            InitializeComponent();
            //this.Load += LineMonitoringView_Load;
            pnlStatusDot.Resize += pnlStatusDot_Resize;
            pnlStatusDot2.Resize += pnlStatusDot_Resize;
            pnlStatusDot3.Resize += pnlStatusDot_Resize;
            pnlStatusDot_Resize(pnlStatusDot, EventArgs.Empty);
            pnlStatusDot_Resize(pnlStatusDot2, EventArgs.Empty);
            pnlStatusDot_Resize(pnlStatusDot3, EventArgs.Empty);
        }

        private void LineMonitoringView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardLineMonitoring, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardLineStatus, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardLineAlarm, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.AddShadowRoundedPanel(cardLineMonitoring, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardLineStatus, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardLineAlarm, 16);

            SetStatus(LineStatus.Run);

            pnlStatusDot2.BackColor = Color.Red;
            lblStatusText2.Text = "비상 정지 . . .";

            pnlStatusDot3.BackColor = Color.Gold;
            lblStatusText3.Text = "대기중 . . .";
        }

        private void pnlStatusDot_Resize(object sender, EventArgs e)
        {
            var p = (Panel)sender;

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, p.Width - 1, p.Height - 1);
                p.Region = new Region(path);
            }
        }
        public void SetStatus(LineStatus status)
        {
            // 1) 상태가 바뀔 때 이전 상태가 가동중이었다면, 그동안 시간 누적
            if (_currentStatus == LineStatus.Run && _runStartTime.HasValue)
            {
                _runAccum += DateTime.Now - _runStartTime.Value;
                _runStartTime = null;
            }

            // 2) 새 상태가 가동중이면 시작 시각 기록
            if (status == LineStatus.Run)
            {
                _runStartTime = DateTime.Now;
            }

            _currentStatus = status;

            switch (status)
            {
                case LineStatus.Run:
                    pnlStatusDot.BackColor = Color.LimeGreen;
                    lblStatusText.Text = "가동중 . . .";
                    break;

                case LineStatus.Idle:
                    pnlStatusDot.BackColor = Color.Gold;
                    lblStatusText.Text = "대기중 . . .";
                    break;

                case LineStatus.Down:
                    pnlStatusDot.BackColor = Color.Red;
                    lblStatusText.Text = "비상 정지 . . .";
                    break;
            }

            // 3) 현재 시점 기준 누적 가동시간 계산(가동중이면 지금까지 합산)
            TimeSpan totalRun =
                _runAccum +
                (status == LineStatus.Run && _runStartTime.HasValue
                    ? DateTime.Now - _runStartTime.Value
                    : TimeSpan.Zero);

            // 4) 로그 기록 (가동시간을 같이 넘겨줌)
            AddAlarm(status, GetStatusLogMessage(status), "Line1", totalRun);

            pnlStatusDot.Invalidate();
        }
        private string GetStatusLogMessage(LineStatus status)
        {
            switch (status)
            {
                case LineStatus.Run: return "라인1 가동 재개";
                case LineStatus.Idle: return "라인1 대기 상태 전환";
                case LineStatus.Down: return "라인1 다운 상태 발생";
            }
            return "";
        }
        private string GetStatusText(LineStatus status)
        {
            switch (status)
            {
                case LineStatus.Run: return "가동중";
                case LineStatus.Idle: return "대기중";
                case LineStatus.Down: return "다운/알람";
            }
            return "-";
        }

        public void AddAlarm(LineStatus status, string message, string lineName, TimeSpan runTime)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string statusText = GetStatusText(status);
            string runText = runTime.ToString(@"hh\:mm\:ss");   // 01:23:45 이런 형식

            var item = new ListViewItem(time);   // 1열: 시간
            item.SubItems.Add(lineName);         // 2열: 라인
            item.SubItems.Add(statusText);       // 3열: 상태 ✅
            item.SubItems.Add(message);          // 4열: 메시지 ✅
            item.SubItems.Add(runText);          // 5열: 가동시간 ✅

            lvAlarmHistory.Items.Insert(0, item);

            if (lvAlarmHistory.Items.Count > 200)
                lvAlarmHistory.Items.RemoveAt(lvAlarmHistory.Items.Count - 1);
        }

        private void btnTestRun_Click(object sender, EventArgs e)
        {
            SetStatus(LineStatus.Run);
        }

        private void btnTestIdle_Click(object sender, EventArgs e)
        {
            SetStatus(LineStatus.Idle);
        }

        private void btnTestDown_Click(object sender, EventArgs e)
        {
            SetStatus(LineStatus.Down);
        }

        private void lvAlarmHistory_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // 디자이너(DesignMode)에서는 아무 것도 하지 말기
            if (this.DesignMode)
                return;

            // 방어 코드: 인덱스가 정상 범위인지 확인
            if (e.ColumnIndex < 0 || e.ColumnIndex >= lvAlarmHistory.Columns.Count)
                return;

            // 런타임에서만 폭 변경 막기
            e.Cancel = true;
            e.NewWidth = lvAlarmHistory.Columns[e.ColumnIndex].Width;
        }
    }
}

