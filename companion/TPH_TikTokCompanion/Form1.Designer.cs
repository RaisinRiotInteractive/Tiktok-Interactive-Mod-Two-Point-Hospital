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
            // ══════════════════════════════════════════════════════════
            // FORM SETUP
            // ══════════════════════════════════════════════════════════
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize      = new System.Drawing.Size(850, 750);
            MinimumSize     = new System.Drawing.Size(800, 600);
            Text            = "TPH TikTok Live Mod";
            StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            Font            = new System.Drawing.Font("Segoe UI", 9f);

            // Controls initialization
            pnlSidebar        = new System.Windows.Forms.Panel();
            lblAppTitle       = new System.Windows.Forms.Label();
            lblAppSub         = new System.Windows.Forms.Label();
            pnlSidebarSep     = new System.Windows.Forms.Panel();
            pnlNavLiveAccent  = new System.Windows.Forms.Panel();
            btnNavLive        = new System.Windows.Forms.Button();
            pnlNavRulesAccent = new System.Windows.Forms.Panel();
            btnNavRules       = new System.Windows.Forms.Button();
            lblVersion        = new System.Windows.Forms.Label();

            pnlContent     = new System.Windows.Forms.Panel();
            pnlLive        = new System.Windows.Forms.TableLayoutPanel();
            pnlRules       = new System.Windows.Forms.TableLayoutPanel();

            // Live Panel Components
            pnlConnectCard = new System.Windows.Forms.GroupBox();
            lblUsernameHint= new System.Windows.Forms.Label();
            txtUsername    = new System.Windows.Forms.TextBox();
            btnConnect     = new System.Windows.Forms.Button();
            lblStatus      = new System.Windows.Forms.Label();

            grpTest           = new System.Windows.Forms.GroupBox();
            flpTestButtons    = new System.Windows.Forms.FlowLayoutPanel();
            btnTestFollow     = new System.Windows.Forms.Button();
            btnTestLike       = new System.Windows.Forms.Button();
            btnTestGift       = new System.Windows.Forms.Button();
            btnSpawnDoctor    = new System.Windows.Forms.Button();
            btnSpawnNurse     = new System.Windows.Forms.Button();
            btnSpawnJanitor   = new System.Windows.Forms.Button();
            btnSpawnAssistant = new System.Windows.Forms.Button();
            btnKillPatient    = new System.Windows.Forms.Button();
            btnFireStaff      = new System.Windows.Forms.Button();
            btnInit           = new System.Windows.Forms.Button();

            pnlLogHeader = new System.Windows.Forms.Panel();
            lblLogHint   = new System.Windows.Forms.Label();
            btnCopyLog   = new System.Windows.Forms.Button();
            lstLog       = new System.Windows.Forms.ListBox();

            // Rules Panel Components
            grpFollow           = new System.Windows.Forms.GroupBox();
            lblFollowActionHint  = new System.Windows.Forms.Label();
            cmbFollowAction      = new System.Windows.Forms.ComboBox();
            lblFollowAmount      = new System.Windows.Forms.Label();
            nudFollowAmount      = new System.Windows.Forms.NumericUpDown();

            grpLike              = new System.Windows.Forms.GroupBox();
            lblLikeActionHint    = new System.Windows.Forms.Label();
            cmbLikeAction        = new System.Windows.Forms.ComboBox();
            lblLikeAmount        = new System.Windows.Forms.Label();
            nudLikeAmount        = new System.Windows.Forms.NumericUpDown();
            lblLikeThreshold     = new System.Windows.Forms.Label();
            cmbLikeThreshold     = new System.Windows.Forms.ComboBox();

            grpGifts             = new System.Windows.Forms.GroupBox();
            dgvGifts             = new System.Windows.Forms.DataGridView();
            pnlGiftButtons       = new System.Windows.Forms.Panel();
            btnAddGift           = new System.Windows.Forms.Button();
            btnRemoveGift        = new System.Windows.Forms.Button();
            lblDefaultGift       = new System.Windows.Forms.Label();
            cmbDefaultGiftAction = new System.Windows.Forms.ComboBox();
            lblDefaultGiftAmount = new System.Windows.Forms.Label();
            nudDefaultGiftAmount = new System.Windows.Forms.NumericUpDown();
            btnSaveRules         = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)dgvGifts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudFollowAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLikeAmount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudDefaultGiftAmount).BeginInit();
            SuspendLayout();

            // ══════════════════════════════════════════════════════════
            // SIDEBAR
            // ══════════════════════════════════════════════════════════
            pnlSidebar.Dock  = System.Windows.Forms.DockStyle.Left;
            pnlSidebar.Width = 200;

            lblAppTitle.Text      = "TPH TIKTOK";
            lblAppTitle.Font      = new System.Drawing.Font("Segoe UI", 12f, System.Drawing.FontStyle.Bold);
            lblAppTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblAppTitle.Location  = new System.Drawing.Point(0, 22);
            lblAppTitle.Size      = new System.Drawing.Size(200, 28);
            lblAppTitle.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            lblAppSub.Text      = "LIVE MOD COMPANION";
            lblAppSub.Font      = new System.Drawing.Font("Segoe UI", 7.5f, System.Drawing.FontStyle.Bold);
            lblAppSub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblAppSub.Location  = new System.Drawing.Point(0, 52);
            lblAppSub.Size      = new System.Drawing.Size(200, 18);
            lblAppSub.Anchor    = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            pnlSidebarSep.Location = new System.Drawing.Point(20, 84);
            pnlSidebarSep.Size     = new System.Drawing.Size(160, 1);
            pnlSidebarSep.Anchor   = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            pnlNavLiveAccent.Location  = new System.Drawing.Point(0, 100);
            pnlNavLiveAccent.Size      = new System.Drawing.Size(4, 44);
            btnNavLive.Text      = "   \u25b6   DASHBOARD";
            btnNavLive.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnNavLive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnNavLive.FlatAppearance.BorderSize = 0;
            btnNavLive.Location  = new System.Drawing.Point(4, 100);
            btnNavLive.Size      = new System.Drawing.Size(196, 44);
            btnNavLive.Cursor    = System.Windows.Forms.Cursors.Hand;
            btnNavLive.Click    += btnNavLive_Click;

            pnlNavRulesAccent.Location = new System.Drawing.Point(0, 144);
            pnlNavRulesAccent.Size     = new System.Drawing.Size(4, 44);
            btnNavRules.Text      = "   \u2699   INTERACTIONS";
            btnNavRules.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnNavRules.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnNavRules.FlatAppearance.BorderSize = 0;
            btnNavRules.Location  = new System.Drawing.Point(4, 144);
            btnNavRules.Size      = new System.Drawing.Size(196, 44);
            btnNavRules.Cursor    = System.Windows.Forms.Cursors.Hand;
            btnNavRules.Click    += btnNavRules_Click;

            lblVersion.Text      = "v1.2.0";
            lblVersion.Dock      = System.Windows.Forms.DockStyle.Bottom;
            lblVersion.Height    = 30;
            lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            pnlSidebar.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblAppTitle, lblAppSub, pnlSidebarSep,
                pnlNavLiveAccent, btnNavLive,
                pnlNavRulesAccent, btnNavRules,
                lblVersion
            });

            // ══════════════════════════════════════════════════════════
            // CONTENT PANEL
            // ══════════════════════════════════════════════════════════
            pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;

            // ══════════════════════════════════════════════════════════
            // LIVE PANEL (DASHBOARD)
            // ══════════════════════════════════════════════════════════
            pnlLive.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlLive.Padding = new System.Windows.Forms.Padding(16);
            pnlLive.AutoScroll = true;
            pnlLive.ColumnCount = 1;
            pnlLive.RowCount = 3;
            pnlLive.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            pnlLive.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            pnlLive.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // Connection Card
            pnlConnectCard.Text = "TIKTOK LIVE CONNECTION";
            pnlConnectCard.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlConnectCard.Padding = new System.Windows.Forms.Padding(12, 24, 12, 12);
            
            lblUsernameHint.Text = "Target Username:";
            lblUsernameHint.Location = new System.Drawing.Point(15, 35);
            lblUsernameHint.AutoSize = true;

            txtUsername.Location = new System.Drawing.Point(15, 55);
            txtUsername.Size = new System.Drawing.Size(300, 28);
            txtUsername.PlaceholderText = "@username";

            btnConnect.Text = "CONNECT";
            btnConnect.Location = new System.Drawing.Point(325, 54);
            btnConnect.Size = new System.Drawing.Size(120, 30);
            btnConnect.Click += btnConnect_Click;

            lblStatus.Text = "DISCONNECTED";
            lblStatus.Name = "lblStatus";
            lblStatus.Location = new System.Drawing.Point(15, 90);
            lblStatus.Size = new System.Drawing.Size(400, 20);
            lblStatus.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);

            pnlConnectCard.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblUsernameHint, txtUsername, btnConnect, lblStatus
            });

            // Test Commands Group
            grpTest.Text = "TEST COMMANDS";
            grpTest.Dock = System.Windows.Forms.DockStyle.Fill;
            grpTest.Padding = new System.Windows.Forms.Padding(12, 24, 12, 12);

            flpTestButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            flpTestButtons.AutoScroll = true;
            flpTestButtons.Controls.AddRange(new System.Windows.Forms.Control[] {
                btnTestFollow, btnTestLike, btnTestGift,
                btnSpawnDoctor, btnSpawnNurse, btnSpawnJanitor, btnSpawnAssistant,
                btnKillPatient, btnFireStaff,
                btnInit
            });

            btnTestFollow.Text = "Test Follow";
            btnTestFollow.Size = new System.Drawing.Size(140, 32);
            btnTestFollow.Click += btnTestFollow_Click;

            btnTestLike.Text = "Test Like x50";
            btnTestLike.Size = new System.Drawing.Size(140, 32);
            btnTestLike.Click += btnTestLike_Click;

            btnTestGift.Text = "Test Gift";
            btnTestGift.Size = new System.Drawing.Size(140, 32);
            btnTestGift.Click += btnTestGift_Click;

            btnSpawnDoctor.Text = "Spawn Doctor";
            btnSpawnDoctor.Size = new System.Drawing.Size(140, 32);
            btnSpawnDoctor.Click += btnSpawnDoctor_Click;

            btnSpawnNurse.Text = "Spawn Nurse";
            btnSpawnNurse.Size = new System.Drawing.Size(140, 32);
            btnSpawnNurse.Click += btnSpawnNurse_Click;

            btnSpawnJanitor.Text = "Spawn Janitor";
            btnSpawnJanitor.Size = new System.Drawing.Size(140, 32);
            btnSpawnJanitor.Click += btnSpawnJanitor_Click;

            btnSpawnAssistant.Text = "Spawn Assistant";
            btnSpawnAssistant.Size = new System.Drawing.Size(140, 32);
            btnSpawnAssistant.Click += btnSpawnAssistant_Click;

            btnKillPatient.Text = "Kill Patient";
            btnKillPatient.Size = new System.Drawing.Size(140, 32);
            btnKillPatient.Click += btnKillPatient_Click;

            btnFireStaff.Text = "Fire Staff";
            btnFireStaff.Size = new System.Drawing.Size(140, 32);
            btnFireStaff.Click += btnFireStaff_Click;

            btnInit.Text = "Re-init Game Interface";
            btnInit.Size = new System.Drawing.Size(300, 32);
            btnInit.Click += btnInit_Click;

            grpTest.Controls.Add(flpTestButtons);

            // Event Log
            var pnlLogContainer = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill, Padding = new System.Windows.Forms.Padding(0, 10, 0, 0) };
            pnlLogHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlLogHeader.Height = 30;
            lblLogHint.Text = "EVENT LOG";
            lblLogHint.Location = new System.Drawing.Point(0, 5);
            lblLogHint.AutoSize = true;
            btnCopyLog.Text = "Copy Log";
            btnCopyLog.Size = new System.Drawing.Size(100, 24);
            btnCopyLog.Location = new System.Drawing.Point(400, 2);
            btnCopyLog.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCopyLog.Click += btnCopyLog_Click;
            pnlLogHeader.Controls.AddRange(new System.Windows.Forms.Control[] { lblLogHint, btnCopyLog });

            lstLog.Dock = System.Windows.Forms.DockStyle.Fill;
            lstLog.Font = new System.Drawing.Font("Consolas", 9f);
            pnlLogContainer.Controls.AddRange(new System.Windows.Forms.Control[] { lstLog, pnlLogHeader });

            pnlLive.Controls.Add(pnlConnectCard, 0, 0);
            pnlLive.Controls.Add(grpTest, 0, 1);
            pnlLive.Controls.Add(pnlLogContainer, 0, 2);

            // ══════════════════════════════════════════════════════════
            // RULES PANEL (INTERACTIONS)
            // ══════════════════════════════════════════════════════════
            pnlRules.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlRules.Padding = new System.Windows.Forms.Padding(16);
            pnlRules.AutoScroll = true;
            pnlRules.ColumnCount = 1;
            pnlRules.RowCount = 4;
            pnlRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            pnlRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            pnlRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            pnlRules.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));

            // Follow Card
            grpFollow.Text = "FOLLOW EVENT";
            grpFollow.Dock = System.Windows.Forms.DockStyle.Fill;
            grpFollow.Padding = new System.Windows.Forms.Padding(12, 24, 12, 12);
            
            lblFollowActionHint.Text = "Action:";
            lblFollowActionHint.Location = new System.Drawing.Point(15, 45);
            lblFollowActionHint.AutoSize = true;
            cmbFollowAction.Location = new System.Drawing.Point(85, 42);
            cmbFollowAction.Size = new System.Drawing.Size(180, 23);
            cmbFollowAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbFollowAction.SelectedIndexChanged += cmbFollowAction_SelectedIndexChanged;

            lblFollowAmount.Text = "Amount: \u00a3";
            lblFollowAmount.Location = new System.Drawing.Point(260, 45);
            lblFollowAmount.AutoSize = true;
            lblFollowAmount.Visible = false;
            nudFollowAmount.Location = new System.Drawing.Point(330, 42);
            nudFollowAmount.Size = new System.Drawing.Size(100, 23);
            nudFollowAmount.Maximum = 9999999;
            nudFollowAmount.Visible = false;

            var lblFollowHint = new System.Windows.Forms.Label { Text = "Triggered when someone follows the stream", Font = new System.Drawing.Font("Segoe UI", 8f), ForeColor = System.Drawing.Color.Gray, Location = new System.Drawing.Point(70, 70), AutoSize = true };
            grpFollow.Controls.AddRange(new System.Windows.Forms.Control[] { lblFollowActionHint, cmbFollowAction, lblFollowAmount, nudFollowAmount, lblFollowHint });

            // Likes Card
            grpLike.Text = "LIKES EVENT";
            grpLike.Dock = System.Windows.Forms.DockStyle.Fill;
            grpLike.Padding = new System.Windows.Forms.Padding(12, 24, 12, 12);

            lblLikeActionHint.Text = "Action:";
            lblLikeActionHint.Location = new System.Drawing.Point(15, 45);
            lblLikeActionHint.AutoSize = true;
            cmbLikeAction.Location = new System.Drawing.Point(85, 42);
            cmbLikeAction.Size = new System.Drawing.Size(180, 23);
            cmbLikeAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbLikeAction.SelectedIndexChanged += cmbLikeAction_SelectedIndexChanged;

            lblLikeAmount.Text = "\u00a3";
            lblLikeAmount.Location = new System.Drawing.Point(260, 45);
            lblLikeAmount.AutoSize = true;
            lblLikeAmount.Visible = false;
            nudLikeAmount.Location = new System.Drawing.Point(280, 42);
            nudLikeAmount.Size = new System.Drawing.Size(100, 23);
            nudLikeAmount.Maximum = 9999999;
            nudLikeAmount.Visible = false;

            lblLikeThreshold.Text = "Trigger every:";
            lblLikeThreshold.Location = new System.Drawing.Point(15, 80);
            lblLikeThreshold.AutoSize = true;
            cmbLikeThreshold.Location = new System.Drawing.Point(120, 77);
            cmbLikeThreshold.Size = new System.Drawing.Size(150, 23);
            cmbLikeThreshold.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            grpLike.Controls.AddRange(new System.Windows.Forms.Control[] { lblLikeActionHint, cmbLikeAction, lblLikeAmount, nudLikeAmount, lblLikeThreshold, cmbLikeThreshold });

            // Gifts Card
            grpGifts.Text = "GIFT REWARDS";
            grpGifts.Dock = System.Windows.Forms.DockStyle.Fill;
            grpGifts.Padding = new System.Windows.Forms.Padding(12, 32, 12, 12);
            dgvGifts.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvGifts.AllowUserToAddRows = false;
            dgvGifts.RowHeadersVisible = false;
            dgvGifts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvGifts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvGifts.EditingControlShowing += dgvGifts_EditingControlShowing;

            var colGiftName = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "Gift Name", Name = "colGiftName", FillWeight = 30 };
            var colGiftAction = new System.Windows.Forms.DataGridViewComboBoxColumn { HeaderText = "Action", Name = "colGiftAction", FillWeight = 40 };
            colGiftAction.Items.AddRange("Spawn Patient", "Spawn Doctor", "Spawn Nurse", "Spawn Janitor", "Spawn Assistant", "Spawn Random", "Add Money", "Take Money", "Kill Patient", "Fire Staff", "Nothing");
            var colGiftAmount = new System.Windows.Forms.DataGridViewTextBoxColumn { HeaderText = "Amount (\u00a3)", Name = "colGiftAmount", FillWeight = 30 };
            dgvGifts.Columns.AddRange(colGiftName, colGiftAction, colGiftAmount);

            pnlGiftButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlGiftButtons.Height = 40;
            btnAddGift.Text = "+ Add Gift";
            btnAddGift.Location = new System.Drawing.Point(0, 8);
            btnAddGift.Size = new System.Drawing.Size(100, 26);
            btnAddGift.Click += btnAddGift_Click;
            btnRemoveGift.Text = "− Remove";
            btnRemoveGift.Location = new System.Drawing.Point(110, 8);
            btnRemoveGift.Size = new System.Drawing.Size(100, 26);
            btnRemoveGift.Click += btnRemoveGift_Click;
            lblDefaultGift.Text = "Default:";
            lblDefaultGift.Location = new System.Drawing.Point(220, 11);
            lblDefaultGift.AutoSize = true;
            cmbDefaultGiftAction.Location = new System.Drawing.Point(290, 8);
            cmbDefaultGiftAction.Size = new System.Drawing.Size(130, 23);
            cmbDefaultGiftAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDefaultGiftAction.SelectedIndexChanged += cmbDefaultGiftAction_SelectedIndexChanged;
            lblDefaultGiftAmount.Text = "\u00a3";
            lblDefaultGiftAmount.Location = new System.Drawing.Point(410, 11);
            lblDefaultGiftAmount.AutoSize = true;
            lblDefaultGiftAmount.Visible = false;
            nudDefaultGiftAmount.Location = new System.Drawing.Point(430, 8);
            nudDefaultGiftAmount.Size = new System.Drawing.Size(80, 23);
            nudDefaultGiftAmount.Maximum = 9999999;
            nudDefaultGiftAmount.Visible = false;
            pnlGiftButtons.Controls.AddRange(new System.Windows.Forms.Control[] { btnAddGift, btnRemoveGift, lblDefaultGift, cmbDefaultGiftAction, lblDefaultGiftAmount, nudDefaultGiftAmount });
            grpGifts.Controls.Add(dgvGifts);
            grpGifts.Controls.Add(pnlGiftButtons);

            // Save Button
            var pnlSaveContainer = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill, Padding = new System.Windows.Forms.Padding(0, 10, 0, 0) };
            btnSaveRules.Text = "SAVE ALL CONFIGURATION";
            btnSaveRules.Dock = System.Windows.Forms.DockStyle.Fill;
            btnSaveRules.Font = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
            btnSaveRules.Click += btnSaveRules_Click;
            pnlSaveContainer.Controls.Add(btnSaveRules);

            pnlRules.Controls.Add(grpFollow, 0, 0);
            pnlRules.Controls.Add(grpLike, 0, 1);
            pnlRules.Controls.Add(grpGifts, 0, 2);
            pnlRules.Controls.Add(pnlSaveContainer, 0, 3);

            // ══════════════════════════════════════════════════════════
            // FINAL ASSEMBLY
            // ══════════════════════════════════════════════════════════
            pnlContent.Controls.Add(pnlRules);
            pnlContent.Controls.Add(pnlLive);
            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);

            ((System.ComponentModel.ISupportInitialize)dgvGifts).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudFollowAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLikeAmount).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudDefaultGiftAmount).EndInit();
            
            ApplyTheme();
            ResumeLayout(false);
        }

        // ── Field declarations ────────────────────────────────────────

        // Sidebar
        private System.Windows.Forms.Panel               pnlSidebar;
        private System.Windows.Forms.Label               lblAppTitle;
        private System.Windows.Forms.Label               lblAppSub;
        private System.Windows.Forms.Panel               pnlSidebarSep;
        private System.Windows.Forms.Panel               pnlNavLiveAccent;
        private System.Windows.Forms.Button              btnNavLive;
        private System.Windows.Forms.Panel               pnlNavRulesAccent;
        private System.Windows.Forms.Button              btnNavRules;
        private System.Windows.Forms.Label               lblVersion;

        // Content structure
        private System.Windows.Forms.Panel               pnlContent;
        private System.Windows.Forms.TableLayoutPanel    pnlLive;
        private System.Windows.Forms.TableLayoutPanel    pnlRules;
        private System.Windows.Forms.GroupBox            pnlConnectCard;
        private System.Windows.Forms.Panel               pnlLogHeader;

        // Live — connection
        private System.Windows.Forms.Label               lblUsernameHint;
        private System.Windows.Forms.TextBox             txtUsername;
        private System.Windows.Forms.Button              btnConnect;
        private System.Windows.Forms.Label               lblStatus;

        // Live — test commands
        private System.Windows.Forms.GroupBox            grpTest;
        private System.Windows.Forms.FlowLayoutPanel     flpTestButtons;
        private System.Windows.Forms.Button              btnTestFollow;
        private System.Windows.Forms.Button              btnTestLike;
        private System.Windows.Forms.Button              btnTestGift;
        private System.Windows.Forms.Button              btnSpawnDoctor;
        private System.Windows.Forms.Button              btnSpawnNurse;
        private System.Windows.Forms.Button              btnSpawnJanitor;
        private System.Windows.Forms.Button              btnSpawnAssistant;
        private System.Windows.Forms.Button              btnKillPatient;
        private System.Windows.Forms.Button              btnFireStaff;
        private System.Windows.Forms.Button              btnInit;

        // Live — log
        private System.Windows.Forms.Label               lblLogHint;
        private System.Windows.Forms.Button              btnCopyLog;
        private System.Windows.Forms.ListBox             lstLog;

        // Rules — Follow
        private System.Windows.Forms.GroupBox            grpFollow;
        private System.Windows.Forms.Label               lblFollowActionHint;
        private System.Windows.Forms.ComboBox            cmbFollowAction;
        private System.Windows.Forms.Label               lblFollowAmount;
        private System.Windows.Forms.NumericUpDown       nudFollowAmount;

        // Rules — Likes
        private System.Windows.Forms.GroupBox            grpLike;
        private System.Windows.Forms.Label               lblLikeActionHint;
        private System.Windows.Forms.ComboBox            cmbLikeAction;
        private System.Windows.Forms.Label               lblLikeAmount;
        private System.Windows.Forms.NumericUpDown       nudLikeAmount;
        private System.Windows.Forms.Label               lblLikeThreshold;
        private System.Windows.Forms.ComboBox            cmbLikeThreshold;

        // Rules — Gifts
        private System.Windows.Forms.GroupBox            grpGifts;
        private System.Windows.Forms.DataGridView        dgvGifts;
        private System.Windows.Forms.Panel               pnlGiftButtons;
        private System.Windows.Forms.Button              btnAddGift;
        private System.Windows.Forms.Button              btnRemoveGift;
        private System.Windows.Forms.Label               lblDefaultGift;
        private System.Windows.Forms.ComboBox            cmbDefaultGiftAction;
        private System.Windows.Forms.Label               lblDefaultGiftAmount;
        private System.Windows.Forms.NumericUpDown       nudDefaultGiftAmount;

        private System.Windows.Forms.Button              btnSaveRules;
    }
}
