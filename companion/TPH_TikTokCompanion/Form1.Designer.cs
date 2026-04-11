namespace TPH_TikTokCompanion
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ── Top-level ─────────────────────────────────────────────
            tabMain            = new System.Windows.Forms.TabControl();
            tabLive            = new System.Windows.Forms.TabPage();
            tabRules           = new System.Windows.Forms.TabPage();

            // ── Live tab ──────────────────────────────────────────────
            lblUsernameHint    = new System.Windows.Forms.Label();
            txtUsername        = new System.Windows.Forms.TextBox();
            btnConnect         = new System.Windows.Forms.Button();
            lblStatus          = new System.Windows.Forms.Label();
            grpTest            = new System.Windows.Forms.GroupBox();
            btnTestFollow      = new System.Windows.Forms.Button();
            btnTestLike        = new System.Windows.Forms.Button();
            btnTestGift        = new System.Windows.Forms.Button();
            btnSpawnDoctor     = new System.Windows.Forms.Button();
            btnSpawnNurse      = new System.Windows.Forms.Button();
            btnSpawnJanitor    = new System.Windows.Forms.Button();
            btnSpawnAssistant  = new System.Windows.Forms.Button();
            btnInit            = new System.Windows.Forms.Button();
            lblLogHint         = new System.Windows.Forms.Label();
            btnCopyLog         = new System.Windows.Forms.Button();
            lstLog             = new System.Windows.Forms.ListBox();

            // ── Rules tab ─────────────────────────────────────────────
            grpFollow          = new System.Windows.Forms.GroupBox();
            lblFollowActionHint= new System.Windows.Forms.Label();
            cmbFollowAction    = new System.Windows.Forms.ComboBox();
            lblFollowAmount    = new System.Windows.Forms.Label();
            nudFollowAmount    = new System.Windows.Forms.NumericUpDown();

            grpLike            = new System.Windows.Forms.GroupBox();
            lblLikeActionHint  = new System.Windows.Forms.Label();
            cmbLikeAction      = new System.Windows.Forms.ComboBox();
            lblLikeAmount      = new System.Windows.Forms.Label();
            nudLikeAmount      = new System.Windows.Forms.NumericUpDown();
            lblLikeThreshold   = new System.Windows.Forms.Label();
            cmbLikeThreshold   = new System.Windows.Forms.ComboBox();

            grpGifts           = new System.Windows.Forms.GroupBox();
            dgvGifts           = new System.Windows.Forms.DataGridView();
            btnAddGift         = new System.Windows.Forms.Button();
            btnRemoveGift      = new System.Windows.Forms.Button();
            lblDefaultGift     = new System.Windows.Forms.Label();
            cmbDefaultGiftAction = new System.Windows.Forms.ComboBox();
            lblDefaultGiftAmount = new System.Windows.Forms.Label();
            nudDefaultGiftAmount = new System.Windows.Forms.NumericUpDown();

            btnSaveRules       = new System.Windows.Forms.Button();

            // ── Form ──────────────────────────────────────────────────
            ClientSize      = new System.Drawing.Size(500, 610);
            Text            = "TPH TikTok Live Mod";
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            Font            = new System.Drawing.Font("Segoe UI", 9f);

            // ── TabControl ────────────────────────────────────────────
            tabMain.Location  = new System.Drawing.Point(0, 0);
            tabMain.Size      = new System.Drawing.Size(500, 610);
            tabMain.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left |
                                System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom;
            tabMain.TabPages.Add(tabLive);
            tabMain.TabPages.Add(tabRules);

            tabLive.Text    = "Live";
            tabLive.Padding = new System.Windows.Forms.Padding(6);

            tabRules.Text    = "Rules";
            tabRules.Padding = new System.Windows.Forms.Padding(6);

            // ══════════════════════════════════════════════════════════
            // LIVE TAB
            // ══════════════════════════════════════════════════════════

            // lblUsernameHint
            lblUsernameHint.Text      = "TikTok Username:";
            lblUsernameHint.Location  = new System.Drawing.Point(6, 14);
            lblUsernameHint.Size      = new System.Drawing.Size(110, 20);
            lblUsernameHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // txtUsername
            txtUsername.Location        = new System.Drawing.Point(120, 12);
            txtUsername.Size            = new System.Drawing.Size(180, 23);
            txtUsername.PlaceholderText = "@username";

            // btnConnect
            btnConnect.Text     = "Connect";
            btnConnect.Location = new System.Drawing.Point(308, 11);
            btnConnect.Size     = new System.Drawing.Size(130, 25);
            btnConnect.Click   += btnConnect_Click;

            // lblStatus
            lblStatus.Text      = "Not connected";
            lblStatus.ForeColor = System.Drawing.Color.OrangeRed;
            lblStatus.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            lblStatus.Location  = new System.Drawing.Point(6, 42);
            lblStatus.Size      = new System.Drawing.Size(446, 20);

            // grpTest
            grpTest.Text     = "Test / Manual Commands";
            grpTest.Location = new System.Drawing.Point(6, 68);
            grpTest.Size     = new System.Drawing.Size(450, 142);

            // btnTestFollow
            btnTestFollow.Text     = "Test Follow";
            btnTestFollow.Location = new System.Drawing.Point(8, 24);
            btnTestFollow.Size     = new System.Drawing.Size(130, 30);
            btnTestFollow.Click   += btnTestFollow_Click;

            // btnTestLike
            btnTestLike.Text     = "Test Like ×50";
            btnTestLike.Location = new System.Drawing.Point(146, 24);
            btnTestLike.Size     = new System.Drawing.Size(130, 30);
            btnTestLike.Click   += btnTestLike_Click;

            // btnTestGift
            btnTestGift.Text     = "Test Gift";
            btnTestGift.Location = new System.Drawing.Point(284, 24);
            btnTestGift.Size     = new System.Drawing.Size(130, 30);
            btnTestGift.Click   += btnTestGift_Click;

            // Staff spawn test buttons (row 2, y=62)
            btnSpawnDoctor.Text     = "Spawn Doctor";
            btnSpawnDoctor.Location = new System.Drawing.Point(8, 62);
            btnSpawnDoctor.Size     = new System.Drawing.Size(102, 30);
            btnSpawnDoctor.Click   += btnSpawnDoctor_Click;

            btnSpawnNurse.Text     = "Spawn Nurse";
            btnSpawnNurse.Location = new System.Drawing.Point(118, 62);
            btnSpawnNurse.Size     = new System.Drawing.Size(102, 30);
            btnSpawnNurse.Click   += btnSpawnNurse_Click;

            btnSpawnJanitor.Text     = "Spawn Janitor";
            btnSpawnJanitor.Location = new System.Drawing.Point(228, 62);
            btnSpawnJanitor.Size     = new System.Drawing.Size(102, 30);
            btnSpawnJanitor.Click   += btnSpawnJanitor_Click;

            btnSpawnAssistant.Text     = "Spawn Assistant";
            btnSpawnAssistant.Location = new System.Drawing.Point(338, 62);
            btnSpawnAssistant.Size     = new System.Drawing.Size(104, 30);
            btnSpawnAssistant.Click   += btnSpawnAssistant_Click;

            // btnInit (row 3, y=100)
            btnInit.Text     = "Re-init Game Interface";
            btnInit.Location = new System.Drawing.Point(8, 100);
            btnInit.Size     = new System.Drawing.Size(295, 30);
            btnInit.Click   += btnInit_Click;

            grpTest.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnTestFollow, btnTestLike, btnTestGift,
                btnSpawnDoctor, btnSpawnNurse, btnSpawnJanitor, btnSpawnAssistant,
                btnInit
            });

            // lblLogHint
            lblLogHint.Text     = "Event Log:";
            lblLogHint.Location = new System.Drawing.Point(6, 218);
            lblLogHint.Size     = new System.Drawing.Size(100, 20);

            // btnCopyLog
            btnCopyLog.Text     = "Copy Log";
            btnCopyLog.Location = new System.Drawing.Point(360, 216);
            btnCopyLog.Size     = new System.Drawing.Size(96, 22);
            btnCopyLog.Click   += btnCopyLog_Click;

            // lstLog
            lstLog.Location            = new System.Drawing.Point(6, 242);
            lstLog.Size                = new System.Drawing.Size(450, 294);
            lstLog.HorizontalScrollbar = true;

            tabLive.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblUsernameHint, txtUsername, btnConnect,
                lblStatus, grpTest, lblLogHint, btnCopyLog, lstLog
            });

            // ══════════════════════════════════════════════════════════
            // RULES TAB
            // ══════════════════════════════════════════════════════════

            // ── grpFollow ─────────────────────────────────────────────
            grpFollow.Text     = "Follow";
            grpFollow.Location = new System.Drawing.Point(6, 8);
            grpFollow.Size     = new System.Drawing.Size(450, 80);

            lblFollowActionHint.Text      = "Action:";
            lblFollowActionHint.Location  = new System.Drawing.Point(10, 26);
            lblFollowActionHint.Size      = new System.Drawing.Size(55, 20);
            lblFollowActionHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            cmbFollowAction.Location         = new System.Drawing.Point(70, 24);
            cmbFollowAction.Size             = new System.Drawing.Size(180, 23);
            cmbFollowAction.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbFollowAction.SelectedIndexChanged += cmbFollowAction_SelectedIndexChanged;

            lblFollowAmount.Text      = "Amount: £";
            lblFollowAmount.Location  = new System.Drawing.Point(260, 26);
            lblFollowAmount.Size      = new System.Drawing.Size(70, 20);
            lblFollowAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblFollowAmount.Visible   = false;

            nudFollowAmount.Location  = new System.Drawing.Point(334, 24);
            nudFollowAmount.Size      = new System.Drawing.Size(100, 23);
            nudFollowAmount.Minimum   = 0;
            nudFollowAmount.Maximum   = 9999999;
            nudFollowAmount.Visible   = false;

            // second row (info hint)
            var lblFollowHint = new System.Windows.Forms.Label();
            lblFollowHint.Text      = "When someone follows during your live";
            lblFollowHint.ForeColor = System.Drawing.Color.Gray;
            lblFollowHint.Location  = new System.Drawing.Point(10, 52);
            lblFollowHint.Size      = new System.Drawing.Size(430, 18);

            grpFollow.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblFollowActionHint, cmbFollowAction,
                lblFollowAmount, nudFollowAmount, lblFollowHint
            });

            // ── grpLike ───────────────────────────────────────────────
            grpLike.Text     = "Likes";
            grpLike.Location = new System.Drawing.Point(6, 96);
            grpLike.Size     = new System.Drawing.Size(450, 108);

            lblLikeActionHint.Text      = "Action:";
            lblLikeActionHint.Location  = new System.Drawing.Point(10, 26);
            lblLikeActionHint.Size      = new System.Drawing.Size(55, 20);
            lblLikeActionHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            cmbLikeAction.Location         = new System.Drawing.Point(70, 24);
            cmbLikeAction.Size             = new System.Drawing.Size(180, 23);
            cmbLikeAction.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLikeAction.SelectedIndexChanged += cmbLikeAction_SelectedIndexChanged;

            lblLikeAmount.Text      = "£";
            lblLikeAmount.Location  = new System.Drawing.Point(260, 26);
            lblLikeAmount.Size      = new System.Drawing.Size(20, 20);
            lblLikeAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblLikeAmount.Visible   = false;

            nudLikeAmount.Location  = new System.Drawing.Point(282, 24);
            nudLikeAmount.Size      = new System.Drawing.Size(110, 23);
            nudLikeAmount.Minimum   = 0;
            nudLikeAmount.Maximum   = 9999999;
            nudLikeAmount.Visible   = false;

            // Threshold row
            lblLikeThreshold.Text      = "Trigger every:";
            lblLikeThreshold.Location  = new System.Drawing.Point(10, 58);
            lblLikeThreshold.Size      = new System.Drawing.Size(84, 20);
            lblLikeThreshold.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            cmbLikeThreshold.Location      = new System.Drawing.Point(98, 56);
            cmbLikeThreshold.Size          = new System.Drawing.Size(160, 23);
            cmbLikeThreshold.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            grpLike.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblLikeActionHint, cmbLikeAction,
                lblLikeAmount, nudLikeAmount,
                lblLikeThreshold, cmbLikeThreshold
            });

            // ── grpGifts ──────────────────────────────────────────────
            grpGifts.Text     = "Gifts";
            grpGifts.Location = new System.Drawing.Point(6, 212);
            grpGifts.Size     = new System.Drawing.Size(450, 310);

            // DataGridView
            dgvGifts.EditingControlShowing += dgvGifts_EditingControlShowing;

            ((System.ComponentModel.ISupportInitialize)dgvGifts).BeginInit();

            dgvGifts.Location              = new System.Drawing.Point(10, 22);
            dgvGifts.Size                  = new System.Drawing.Size(430, 200);
            dgvGifts.AllowUserToAddRows    = false;
            dgvGifts.AllowUserToDeleteRows = false;
            dgvGifts.RowHeadersVisible     = false;
            dgvGifts.AutoSizeColumnsMode   = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvGifts.SelectionMode         = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvGifts.MultiSelect           = false;
            dgvGifts.ScrollBars            = System.Windows.Forms.ScrollBars.Vertical;
            dgvGifts.ColumnHeadersHeightSizeMode =
                System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            var colGiftName = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = "Gift Name",
                Name       = "colGiftName",
                FillWeight = 40
            };
            var colGiftAction = new System.Windows.Forms.DataGridViewComboBoxColumn
            {
                HeaderText = "Action",
                Name       = "colGiftAction",
                FillWeight = 40
            };
            colGiftAction.Items.AddRange("Spawn Patient", "Spawn Doctor", "Spawn Nurse", "Spawn Janitor", "Spawn Assistant", "Spawn Random", "Add Money", "Take Money", "Nothing");
            var colGiftAmount = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = "Amount (£)",
                Name       = "colGiftAmount",
                FillWeight = 20
            };

            dgvGifts.Columns.AddRange(colGiftName, colGiftAction, colGiftAmount);
            ((System.ComponentModel.ISupportInitialize)dgvGifts).EndInit();

            btnAddGift.Text     = "+ Add Gift";
            btnAddGift.Location = new System.Drawing.Point(10, 230);
            btnAddGift.Size     = new System.Drawing.Size(100, 26);
            btnAddGift.Click   += btnAddGift_Click;

            btnRemoveGift.Text     = "− Remove";
            btnRemoveGift.Location = new System.Drawing.Point(118, 230);
            btnRemoveGift.Size     = new System.Drawing.Size(100, 26);
            btnRemoveGift.Click   += btnRemoveGift_Click;

            // Default gift row
            lblDefaultGift.Text      = "Default gift:";
            lblDefaultGift.Location  = new System.Drawing.Point(10, 268);
            lblDefaultGift.Size      = new System.Drawing.Size(76, 20);
            lblDefaultGift.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            cmbDefaultGiftAction.Location         = new System.Drawing.Point(90, 266);
            cmbDefaultGiftAction.Size             = new System.Drawing.Size(160, 23);
            cmbDefaultGiftAction.DropDownStyle    = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDefaultGiftAction.SelectedIndexChanged += cmbDefaultGiftAction_SelectedIndexChanged;

            lblDefaultGiftAmount.Text      = "£";
            lblDefaultGiftAmount.Location  = new System.Drawing.Point(258, 268);
            lblDefaultGiftAmount.Size      = new System.Drawing.Size(20, 20);
            lblDefaultGiftAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblDefaultGiftAmount.Visible   = false;

            nudDefaultGiftAmount.Location  = new System.Drawing.Point(280, 266);
            nudDefaultGiftAmount.Size      = new System.Drawing.Size(110, 23);
            nudDefaultGiftAmount.Minimum   = 0;
            nudDefaultGiftAmount.Maximum   = 9999999;
            nudDefaultGiftAmount.Visible   = false;

            grpGifts.Controls.AddRange(new System.Windows.Forms.Control[] {
                dgvGifts,
                btnAddGift, btnRemoveGift,
                lblDefaultGift, cmbDefaultGiftAction,
                lblDefaultGiftAmount, nudDefaultGiftAmount
            });

            // btnSaveRules
            btnSaveRules.Text     = "Save Rules";
            btnSaveRules.Location = new System.Drawing.Point(6, 530);
            btnSaveRules.Size     = new System.Drawing.Size(450, 32);
            btnSaveRules.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            btnSaveRules.Click   += btnSaveRules_Click;

            tabRules.Controls.AddRange(new System.Windows.Forms.Control[] {
                grpFollow, grpLike, grpGifts, btnSaveRules
            });

            // ── Wire up TabControl ────────────────────────────────────
            Controls.Add(tabMain);
        }

        // ── Field declarations ────────────────────────────────────────

        private System.Windows.Forms.TabControl          tabMain;
        private System.Windows.Forms.TabPage             tabLive;
        private System.Windows.Forms.TabPage             tabRules;

        // Live tab
        private System.Windows.Forms.Label               lblUsernameHint;
        private System.Windows.Forms.TextBox             txtUsername;
        private System.Windows.Forms.Button              btnConnect;
        private System.Windows.Forms.Label               lblStatus;
        private System.Windows.Forms.GroupBox            grpTest;
        private System.Windows.Forms.Button              btnTestFollow;
        private System.Windows.Forms.Button              btnTestLike;
        private System.Windows.Forms.Button              btnTestGift;
        private System.Windows.Forms.Button              btnSpawnDoctor;
        private System.Windows.Forms.Button              btnSpawnNurse;
        private System.Windows.Forms.Button              btnSpawnJanitor;
        private System.Windows.Forms.Button              btnSpawnAssistant;
        private System.Windows.Forms.Button              btnInit;
        private System.Windows.Forms.Label               lblLogHint;
        private System.Windows.Forms.Button              btnCopyLog;
        private System.Windows.Forms.ListBox             lstLog;

        // Rules tab – Follow
        private System.Windows.Forms.GroupBox            grpFollow;
        private System.Windows.Forms.Label               lblFollowActionHint;
        private System.Windows.Forms.ComboBox            cmbFollowAction;
        private System.Windows.Forms.Label               lblFollowAmount;
        private System.Windows.Forms.NumericUpDown       nudFollowAmount;

        // Rules tab – Like
        private System.Windows.Forms.GroupBox            grpLike;
        private System.Windows.Forms.Label               lblLikeActionHint;
        private System.Windows.Forms.ComboBox            cmbLikeAction;
        private System.Windows.Forms.Label               lblLikeAmount;
        private System.Windows.Forms.NumericUpDown       nudLikeAmount;
        private System.Windows.Forms.Label               lblLikeThreshold;
        private System.Windows.Forms.ComboBox            cmbLikeThreshold;

        // Rules tab – Gifts
        private System.Windows.Forms.GroupBox            grpGifts;
        private System.Windows.Forms.DataGridView        dgvGifts;
        private System.Windows.Forms.Button              btnAddGift;
        private System.Windows.Forms.Button              btnRemoveGift;
        private System.Windows.Forms.Label               lblDefaultGift;
        private System.Windows.Forms.ComboBox            cmbDefaultGiftAction;
        private System.Windows.Forms.Label               lblDefaultGiftAmount;
        private System.Windows.Forms.NumericUpDown       nudDefaultGiftAmount;

        private System.Windows.Forms.Button              btnSaveRules;
    }
}
