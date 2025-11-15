using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pcb_monitoring_program;
using pcb_monitoring_program.Views.Statistics;

namespace pcb_monitoring_program.Views.Statistics
{
    public partial class InspectionHistoryView : UserControl
    {
        private bool _isInternalUpdate = false;

        public event EventHandler OpenDetailsRequested;

        public InspectionHistoryView()
        {
            InitializeComponent();
        }

        private void ApplyButtonStyle(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(64, 64, 64);
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Cursor = Cursors.Hand;
                }
                else if (ctrl.HasChildren)
                {
                    ApplyButtonStyle(ctrl); // 내부 컨트롤(패널 등) 재귀 호출
                }
            }
        }

        private void InspectionHistoryView_Load(object sender, EventArgs e)
        {
            UiStyleHelper.MakeRoundedPanel(cardfilter, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardSearchresult, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardday, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardDefectType, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardCameraID, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardDefectLocation, radius: 16, back: Color.FromArgb(44, 44, 44));
            UiStyleHelper.MakeRoundedPanel(cardproductionline, radius: 16, back: Color.FromArgb(44, 44, 44));

            UiStyleHelper.AddShadowRoundedPanel(cardfilter, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardSearchresult, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardday, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardDefectType, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardCameraID, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardDefectLocation, 16);
            UiStyleHelper.AddShadowRoundedPanel(cardproductionline, 16);

            UiStyleHelper.MakeRoundedButton(btn_filterSearch, 24);
            UiStyleHelper.AttachDropShadow(btn_filterSearch, radius: 16, offset: 4);

            // 🔹 이 한 줄로 모든 버튼 스타일 적용
            ApplyButtonStyle(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenDetailsRequested?.Invoke(this, EventArgs.Empty);

            if (sender is Button btn)
                UiStyleHelper.HighlightButton(btn);

            // 🔹 InspectionHistoryForm 열기
            InspectionHistoryDetailForm form = new InspectionHistoryDetailForm();
            form.StartPosition = FormStartPosition.CenterParent; // 부모 기준 중앙 정렬
            form.Show();
        }

        //////////////////////////////////// 불량 유형 : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateDefectTypeAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            // 네 개가 모두 체크되어 있으면 All 체크, 아니면 All 해제
            CB_IH_DefectType_All.Checked =
                CB_IH_DefectType_Normal.Checked &&
                CB_IH_DefectType_ComponentDefect.Checked &&
                CB_IH_DefectType_SolderingDefect.Checked &&
                CB_IH_DefectType_Scrap.Checked;

            _isInternalUpdate = false;
        }

        private void CB_DefectType_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return; // 내부 업데이트면 무시 (옵션이지만 깔끔)

            bool isChecked = CB_IH_DefectType_All.Checked;

            // 이벤트 루프 방지 위해 temporarily flag 사용
            _isInternalUpdate = true;

            CB_IH_DefectType_Normal.Checked = isChecked;
            CB_IH_DefectType_ComponentDefect.Checked = isChecked;
            CB_IH_DefectType_SolderingDefect.Checked = isChecked;
            CB_IH_DefectType_Scrap.Checked = isChecked;

            _isInternalUpdate = false;
        }

        private void CB_DefectType_Normal_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
        }

        private void CB_DefectType_ComponentDefect_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
        }

        private void CB_DefectType_SolderingDefect_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
        }

        private void CB_DefectType_Scrap_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectTypeAllState();
        }

        //////////////////////////////////////////// 카메라 ID : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateCameraIDAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            // 네 개가 모두 체크되어 있으면 All 체크, 아니면 All 해제
            CB_IH_CameraID_All.Checked =
                CB_IH_CameraID_CAM01.Checked &&
                CB_IH_CameraID_CAM02.Checked &&
                CB_IH_CameraID_CAM03.Checked;

            _isInternalUpdate = false;
        }

        private void CB_CameraID_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return; // 내부 업데이트면 무시 (옵션이지만 깔끔)

            bool isChecked = CB_IH_CameraID_All.Checked;

            // 이벤트 루프 방지 위해 temporarily flag 사용
            _isInternalUpdate = true;

            CB_IH_CameraID_CAM01.Checked = isChecked;
            CB_IH_CameraID_CAM02.Checked = isChecked;
            CB_IH_CameraID_CAM03.Checked = isChecked;

            _isInternalUpdate = false;
        }

        private void CB_CameraID_CAM01_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
        }

        private void CB_CameraID_CAM02_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
        }

        private void CB_CameraID_CAM03_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCameraIDAllState();
        }

        //////////////////////////// 불량 위치 : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateDefectLocationAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            // 네 개가 모두 체크되어 있으면 All 체크, 아니면 All 해제
            CB_IH_DefectLocation_All.Checked =
                CB_IH_DefectLocation_Upper.Checked &&
                CB_IH_DefectLocation_Lower.Checked &&
                CB_IH_DefectLocation_Left.Checked &&
                CB_IH_DefectLocation_Right.Checked;

            _isInternalUpdate = false;
        }

        private void CB_DefectLocation_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return; // 내부 업데이트면 무시 (옵션이지만 깔끔)

            bool isChecked = CB_IH_DefectLocation_All.Checked;

            // 이벤트 루프 방지 위해 temporarily flag 사용
            _isInternalUpdate = true;

            CB_IH_DefectLocation_Upper.Checked = isChecked;
            CB_IH_DefectLocation_Lower.Checked = isChecked;
            CB_IH_DefectLocation_Left.Checked = isChecked;
            CB_IH_DefectLocation_Right.Checked = isChecked;

            _isInternalUpdate = false;
        }

        private void CB_DefectLocation_Upper_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectLocationAllState();
        }

        private void CB_DefectLocation_Lower_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectLocationAllState();
        }

        private void CB_DefectLocation_Left_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectLocationAllState();
        }

        private void CB_DefectLocation_Right_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDefectLocationAllState();
        }

        /////////////////////////////// 생산 라인 : 네 개의 개별 체크박스 상태가 변경될 때마다 All 체크박스 상태 업데이트
        private void UpdateProductionLineAllState()
        {
            if (_isInternalUpdate) return;

            _isInternalUpdate = true;

            // 네 개가 모두 체크되어 있으면 All 체크, 아니면 All 해제
            CB_IH_ProductionLine_All.Checked =
                CB_IH_ProductionLine_1.Checked &&
                CB_IH_ProductionLine_2.Checked &&
                CB_IH_ProductionLine_3.Checked;

            _isInternalUpdate = false;
        }

        private void CB_ProductionLine_All_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInternalUpdate) return; // 내부 업데이트면 무시 (옵션이지만 깔끔)

            bool isChecked = CB_IH_ProductionLine_All.Checked;

            // 이벤트 루프 방지 위해 temporarily flag 사용
            _isInternalUpdate = true;

            CB_IH_ProductionLine_1.Checked = isChecked;
            CB_IH_ProductionLine_2.Checked = isChecked;
            CB_IH_ProductionLine_3.Checked = isChecked;

            _isInternalUpdate = false;
        }

        private void CB_ProductionLine_1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateProductionLineAllState();
        }

        private void CB_ProductionLine_2_CheckedChanged(object sender, EventArgs e)
        {
            UpdateProductionLineAllState();
        }

        private void CB_ProductionLine_3_CheckedChanged(object sender, EventArgs e)
        {
            UpdateProductionLineAllState();
        }
    }
}
