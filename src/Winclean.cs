using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using System.Reflection;

[assembly: AssemblyTitle("Winclean")]
[assembly: AssemblyDescription("Portable Windows application removal and leftovers cleaner")]
[assembly: AssemblyCompany("Winclean contributors")]
[assembly: AssemblyProduct("Winclean")]
[assembly: AssemblyVersion("5.3.1.0")]
[assembly: AssemblyFileVersion("5.3.1.0")]

namespace Winclean
{
    internal static class AppPreferences
    {
        private static readonly string DirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Winclean");
        private static readonly string AccentFile = Path.Combine(DirectoryPath, "accent.cfg");
        private static readonly string UserFile = Path.Combine(DirectoryPath, "user.cfg");
        private static readonly KeyValuePair<string, Color>[] Accents = new KeyValuePair<string, Color>[] {
            new KeyValuePair<string, Color>("Blue", Color.FromArgb(59, 130, 246)),
            new KeyValuePair<string, Color>("Green", Color.FromArgb(34, 197, 94)),
            new KeyValuePair<string, Color>("Aqua", Color.FromArgb(6, 182, 212)),
            new KeyValuePair<string, Color>("Red", Color.FromArgb(239, 68, 68)),
            new KeyValuePair<string, Color>("Purple", Color.FromArgb(168, 85, 247)),
            new KeyValuePair<string, Color>("Yellow", Color.FromArgb(234, 179, 8)),
            new KeyValuePair<string, Color>("White", Color.FromArgb(226, 232, 240)),
            new KeyValuePair<string, Color>("Gray", Color.FromArgb(148, 163, 184)),
            new KeyValuePair<string, Color>("Light Blue", Color.FromArgb(96, 165, 250)),
            new KeyValuePair<string, Color>("Light Green", Color.FromArgb(74, 222, 128)),
            new KeyValuePair<string, Color>("Light Aqua", Color.FromArgb(34, 211, 238)),
            new KeyValuePair<string, Color>("Light Red", Color.FromArgb(248, 113, 113)),
            new KeyValuePair<string, Color>("Light Purple", Color.FromArgb(192, 132, 252)),
            new KeyValuePair<string, Color>("Light Yellow", Color.FromArgb(250, 204, 21)),
            new KeyValuePair<string, Color>("Bright White", Color.FromArgb(255, 255, 255))
        };
        public static string AccentName { get; private set; }
        public static string UserName { get; private set; }
        public static Color Accent { get { return Accents.First(item => item.Key == (String.IsNullOrEmpty(AccentName) ? "Light Aqua" : AccentName)).Value; } }
        public static string[] AccentNames { get { return Accents.Select(item => item.Key).ToArray(); } }
        public static Color GetAccentColor(string name) { return Accents.First(item => item.Key == name).Value; }

        public static void Load()
        {
            try { AccentName = File.Exists(AccentFile) ? File.ReadAllText(AccentFile, Encoding.UTF8).Trim() : "Light Aqua"; }
            catch { AccentName = "Light Aqua"; }
            if (!Accents.Any(item => item.Key == AccentName)) AccentName = "Light Aqua";
            try { UserName = File.Exists(UserFile) ? File.ReadAllText(UserFile, Encoding.UTF8).Trim() : Environment.UserName; }
            catch { UserName = Environment.UserName; }
            if (String.IsNullOrWhiteSpace(UserName)) UserName = Environment.UserName;
        }

        public static void SetAccent(string value)
        {
            if (!Accents.Any(item => item.Key == value)) throw new ArgumentException("Choose an accent from the available list.", "value");
            AccentName = value;
            Save(AccentFile, AccentName);
        }

        public static void SetUserName(string value)
        {
            UserName = (value ?? "").Trim();
            if (UserName.Length == 0) UserName = Environment.UserName;
            if (UserName.Length > 40) UserName = UserName.Substring(0, 40);
            Save(UserFile, UserName);
        }

        private static void Save(string path, string value)
        {
            Directory.CreateDirectory(DirectoryPath);
            File.WriteAllText(path, value, Encoding.UTF8);
        }
    }

    internal static class UiTheme
    {
        public static readonly Color Background = Color.FromArgb(15, 23, 42);
        public static readonly Color Surface = Color.FromArgb(30, 41, 59);
        public static readonly Color Elevated = Color.FromArgb(39, 52, 72);
        public static readonly Color Text = Color.FromArgb(226, 232, 240);
        public static readonly Color Muted = Color.FromArgb(148, 163, 184);
        public static readonly Color Border = Color.FromArgb(51, 65, 85);
        public static Color AccentText
        {
            get
            {
                double accent = RelativeLuminance(AppPreferences.Accent);
                double darkContrast = (accent + 0.05) / (RelativeLuminance(Background) + 0.05);
                double lightContrast = (RelativeLuminance(Text) + 0.05) / (accent + 0.05);
                return darkContrast >= lightContrast ? Background : Text;
            }
        }

        private static double RelativeLuminance(Color color)
        {
            Func<double, double> linearize = delegate(double channel)
            {
                channel /= 255.0;
                return channel <= 0.04045 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
            };
            return 0.2126 * linearize(color.R) + 0.7152 * linearize(color.G) + 0.0722 * linearize(color.B);
        }

        public static void Apply(Form form)
        {
            form.BackColor = Background;
            form.ForeColor = Text;
            ApplyControls(form.Controls);
            foreach (Control control in form.Controls) StyleTabs(control);
        }

        private static void StyleTabs(Control control)
        {
            if (control is TabControl)
            {
                var tabs = (TabControl)control;
                tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
                tabs.ItemSize = new Size(112, 32);
                tabs.DrawItem += delegate(object sender, DrawItemEventArgs e)
                {
                    bool selected = e.Index == tabs.SelectedIndex;
                    Color fill = selected ? Elevated : Background;
                    using (var brush = new SolidBrush(fill)) e.Graphics.FillRectangle(brush, e.Bounds);
                    using (var brush =                    new SolidBrush(selected ? AppPreferences.Accent : Muted))
                        TextRenderer.DrawText(e.Graphics, tabs.TabPages[e.Index].Text, tabs.Font, e.Bounds, brush.Color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    if (selected) using (var brush = new SolidBrush(AppPreferences.Accent)) e.Graphics.FillRectangle(brush, e.Bounds.Left + 14, e.Bounds.Bottom - 3, e.Bounds.Width - 28, 3);
                };
            }
            foreach (Control child in control.Controls) StyleTabs(child);
        }

        private static void ApplyControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is TabPage) control.BackColor = Background;
                else if (control is Panel || control is TableLayoutPanel || control is FlowLayoutPanel || control is SplitContainer) control.BackColor = Surface;
                if (control is TextBoxBase || control is ComboBox || control is ListBox || control is CheckedListBox || control is RichTextBox || control is NumericUpDown)
                {
                    control.BackColor = Elevated;
                    control.ForeColor = Text;
                }
                if (control is CheckBox || control is RadioButton)
                {
                    control.BackColor = Background;
                    control.ForeColor = Text;
                }
                if (control is Button)
                {
                    var button = (Button)control;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = Border;
                    button.FlatAppearance.BorderSize = 1;
                    bool primary = String.Equals(button.Tag as string, "primary", StringComparison.Ordinal);
                    bool card = String.Equals(button.Tag as string, "card", StringComparison.Ordinal);
                    button.BackColor = primary ? AppPreferences.Accent : (card ? Surface : Elevated);
                    button.ForeColor = primary ? AccentText : (card ? AppPreferences.Accent : Text);
                }
                if (control is DataGridView)
                {
                    var grid = (DataGridView)control;
                    grid.BackgroundColor = Surface;
                    grid.GridColor = Border;
                    grid.DefaultCellStyle.BackColor = Surface;
                    grid.DefaultCellStyle.ForeColor = Text;
                    grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
                    grid.DefaultCellStyle.SelectionForeColor = Text;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Elevated;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
                    grid.RowHeadersDefaultCellStyle.BackColor = Elevated;
                    grid.RowHeadersDefaultCellStyle.ForeColor = Text;
                }
                if (control is Label || control is LinkLabel || control is GroupBox)
                {
                    control.BackColor = Color.Transparent;
                    if (control.ForeColor == SystemColors.ControlText) control.ForeColor = Text;
                }
                else if (!(control is Button) && !(control is TextBoxBase) && !(control is ComboBox) && !(control is ListBox) && !(control is CheckedListBox) && !(control is RichTextBox) && !(control is NumericUpDown) && !(control is DataGridView) && !(control is TabPage) && !(control is Panel) && !(control is TableLayoutPanel) && !(control is FlowLayoutPanel) && !(control is SplitContainer))
                {
                    if (control.ForeColor == SystemColors.ControlText) control.ForeColor = Text;
                }
                if (control.HasChildren) ApplyControls(control.Controls);
            }
        }
    }

    internal sealed class AppEntry
    {
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
        public string Source { get; set; }
        public string UninstallCommand { get; set; }
        public string RegistryPath { get; set; }
        public RegistryHive Hive { get; set; }
        public RegistryView View { get; set; }
        public string InstallLocation { get; set; }
        public string PackageFullName { get; set; }
        public string PackageFamilyName { get; set; }
        public bool IsProtected { get; set; }

        public string Identity
        {
            get
            {
                return Source == "Store app"
                    ? Source + "|" + PackageFullName
                    : Source + "|" + Hive + "|" + View + "|" + RegistryPath;
            }
        }

        public string LocationText
        {
            get { return String.IsNullOrWhiteSpace(InstallLocation) ? "Not listed" : InstallLocation; }
        }
    }

    internal sealed class CleanupCandidate
    {
        public string Label;
        public string Path;
        public bool IsRegistry;
        public RegistryHive Hive;
        public RegistryView View;
        public string RegistryPath;

        public override string ToString()
        {
            return Label + "  —  " + Path;
        }
    }

    internal sealed class MainForm : Form
    {            private readonly Color ink = UiTheme.Text;
            private readonly Color muted = UiTheme.Muted;
        private Color blue { get { return AppPreferences.Accent; } }
        private readonly DataGridView grid = new DataGridView();
        private readonly TabControl tabs = new TabControl();
        private readonly TabControl toolTabs = new TabControl();
        private readonly Label welcome = new Label();
        private readonly ComboBox accentPicker = new ComboBox();
        private readonly TextBox userNameInput = new TextBox();
        private readonly TextBox search = new TextBox();
        private readonly Label countLabel = new Label();
        private readonly Label detailName = new Label();
        private readonly Label detailPublisher = new Label();
        private readonly Label detailVersion = new Label();
        private readonly Label detailSource = new Label();
        private readonly Label detailLocation = new Label();
        private readonly Label status = new Label();
        private readonly Button uninstallButton = new Button();
        private readonly Button refreshButton = new Button();
        private readonly ToolTip tips = new ToolTip();
        private readonly List<AppEntry> allApps = new List<AppEntry>();
        private AppEntry selected;
        private bool busy;

        public MainForm()
        {
            AppPreferences.Load();
            Text = "Winclean";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(920, 630);
            Size = new Size(1180, 790);
            BackColor = UiTheme.Background;
            ForeColor = ink;
            Font = new Font("Segoe UI", 9F);
            BuildLayout();
            Shown += async delegate { await LoadAppsAsync(null); };
        }

        private void BuildLayout()
        {
            var shell = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = BackColor, Padding = new Padding(22, 18, 22, 16) };
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(shell);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
            var title = new Label { Text = "Winclean", Font = new Font("Segoe UI Semibold", 23F, FontStyle.Bold), ForeColor = ink, AutoSize = true, Location = new Point(0, 0) };
            var subtitle = new Label { Text = "A cleaner, safer Windows toolkit", Font = new Font("Segoe UI", 10F), ForeColor = muted, AutoSize = true, Location = new Point(2, 43) };
            welcome.Text = GetWelcomeText();
            welcome.AutoSize = true;
            welcome.ForeColor = muted;
            welcome.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(welcome);
            header.Resize += delegate { welcome.Location = new Point(Math.Max(600, header.ClientSize.Width - welcome.Width), 42); };
            shell.Controls.Add(header, 0, 0);

            tabs.Dock = DockStyle.Fill;
            tabs.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            shell.Controls.Add(tabs, 0, 1);

            tabs.TabPages.Add(BuildCleanupPage());
            var toolsPage = new TabPage("Tools") { BackColor = BackColor, Padding = new Padding(0) };
            toolTabs.Dock = DockStyle.Fill;
            toolTabs.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            toolsPage.Controls.Add(toolTabs);
            tabs.TabPages.Add(toolsPage);
            toolTabs.TabPages.Add(BuildToolsPage());
            toolTabs.TabPages.Add(BuildAppsPage());
            toolTabs.TabPages.Add(BuildSettingsPage());
            UiTheme.Apply(this);
        }

        private string GetWelcomeText()
        {
            string free = "unknown free";
            try
            {
                string root = Environment.GetEnvironmentVariable("SystemDrive") ?? Path.GetPathRoot(Environment.SystemDirectory);
                DriveInfo drive = new DriveInfo(root);
                if (drive.IsReady) free = String.Format("{0:0.0} GB free on {1}", drive.AvailableFreeSpace / 1073741824.0, drive.Name.TrimEnd('\\'));
            }
            catch { }
            return "Welcome, " + AppPreferences.UserName + "  ·  " + free;
        }

        private TabPage BuildAppsPage()
        {
            var appsPage = new TabPage("App manager") { BackColor = BackColor, Padding = new Padding(0) };
            var appLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = BackColor, Padding = new Padding(0, 10, 0, 0) };
            appLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            appLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            appLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            appsPage.Controls.Add(appLayout);
            var searchPanel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Background };
            searchPanel.Controls.Add(new Label { Text = "Search installed apps", AutoSize = true, ForeColor = muted, Location = new Point(0, 2) });
            search.SetBounds(0, 23, 430, 28);
            search.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            search.Font = new Font("Segoe UI", 10F);
            search.BorderStyle = BorderStyle.FixedSingle;
            search.TextChanged += delegate { ShowFilteredApps(); };
            searchPanel.Controls.Add(search);
            countLabel.AutoSize = true;
            countLabel.ForeColor = muted;
            countLabel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            countLabel.Location = new Point(460, 29);
            searchPanel.Resize += delegate { search.Width = Math.Max(280, searchPanel.ClientSize.Width - countLabel.Width - 38); countLabel.Left = searchPanel.ClientSize.Width - countLabel.Width; };
            searchPanel.Controls.Add(countLabel);
            appLayout.Controls.Add(searchPanel, 0, 0);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2, SplitterWidth = 6, BackColor = UiTheme.Border, BorderStyle = BorderStyle.FixedSingle };
            split.Panel1.BackColor = UiTheme.Surface;
            split.Panel2.BackColor = UiTheme.Surface;
            split.HandleCreated += delegate { split.Panel1MinSize = 360; split.Panel2MinSize = 250; SetAppSplitPosition(split, true); };
            split.SizeChanged += delegate { SetAppSplitPosition(split, false); };
            appLayout.Controls.Add(split, 0, 1);

            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = UiTheme.Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeight = 38;
            grid.RowTemplate.Height = 34;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = UiTheme.Elevated;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = muted;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 65, 85);
            grid.DefaultCellStyle.SelectionForeColor = ink;
            grid.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "APPLICATION", DataPropertyName = "Name", FillWeight = 130 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PUBLISHER", DataPropertyName = "Publisher", FillWeight = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "VERSION", DataPropertyName = "Version", FillWeight = 60 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TYPE", DataPropertyName = "Source", FillWeight = 75 });
            grid.SelectionChanged += delegate { UpdateSelection(); };
            grid.CellDoubleClick += async delegate { if (selected != null) await UninstallSelectedAsync(); };
            grid.DataBindingComplete += delegate { foreach (DataGridViewRow row in grid.Rows) { var app = row.DataBoundItem as AppEntry; if (app != null) { row.Cells[0].ToolTipText = app.Name; row.Cells[1].ToolTipText = app.Publisher; row.Cells[2].ToolTipText = app.Version; row.Cells[3].ToolTipText = app.Source; } } };
            split.Panel1.Controls.Add(grid);

            var details = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Padding = new Padding(18) };
            split.Panel2.Controls.Add(details);
            var detailsHeader = new Label { Text = "APP DETAILS", AutoSize = true, Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold), ForeColor = muted, Location = new Point(18, 19) };
            details.Controls.Add(detailsHeader);
            detailName.SetBounds(18, 52, 260, 54);
            detailName.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            detailName.ForeColor = ink;
            detailName.AutoEllipsis = true;
            details.Controls.Add(detailName);
            AddDetail(details, "Publisher", detailPublisher, 125);
            AddDetail(details, "Version", detailVersion, 185);
            AddDetail(details, "Source", detailSource, 245);
            AddDetail(details, "Install location", detailLocation, 305);
            detailLocation.Height = 80;
            detailLocation.AutoEllipsis = true;
            status.SetBounds(18, 395, 260, 64);
            status.ForeColor = muted;
            status.AutoSize = false;
            status.Text = "Select an app to see its details.";
            status.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            details.Controls.Add(status);

            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 8, 0, 0), BackColor = UiTheme.Surface };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
            refreshButton.Text = "Refresh list";
            StyleButton(refreshButton, false);
            refreshButton.Dock = DockStyle.Fill;
            refreshButton.Click += async delegate { await LoadAppsAsync(selected == null ? null : selected.Identity); };
            uninstallButton.Text = "Uninstall selected…";
            StyleButton(uninstallButton, true);
            uninstallButton.Dock = DockStyle.Fill;
            uninstallButton.Enabled = false;
            uninstallButton.Click += async delegate { await UninstallSelectedAsync(); };
            footer.Controls.Add(refreshButton, 0, 0);
            footer.Controls.Add(uninstallButton, 1, 0);
            appLayout.Controls.Add(footer, 0, 2);
            tips.SetToolTip(uninstallButton, "Review the app's registered uninstaller before continuing. Leftover cleanup is a separate optional step.");
            tips.SetToolTip(refreshButton, "Read the installed app list again from Windows.");
            return appsPage;
        }

        private TabPage BuildCleanupPage()
        {
            var page = new TabPage("Maintenance") { BackColor = BackColor, Padding = new Padding(10) };
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.Background };
            var layout = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, ColumnCount = 2, RowCount = 7, Padding = new Padding(6), BackColor = UiTheme.Background };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            scroll.Controls.Add(layout);
            page.Controls.Add(scroll);
            layout.Controls.Add(MaintenanceCard("Temporary files", "Clean the current user's temporary files. Files in use are skipped.", async delegate { await RunMaintenanceAsync("Temporary files", RemoveTemporaryFiles); }), 0, 0);
            layout.Controls.Add(MaintenanceCard("Browser cache", "Remove cache folders only. Cookies, history and passwords are untouched.", async delegate { await RunMaintenanceAsync("Browser cache", RemoveBrowserCaches); }), 1, 0);
            layout.Controls.Add(MaintenanceCard("Windows Update downloads", "Clear downloaded update files. Windows may download them again later.", async delegate { await RunMaintenanceAsync("Windows Update downloads", RemoveUpdateCache); }), 0, 1);
            layout.Controls.Add(MaintenanceCard("Recycle Bin", "Review and confirm before permanently emptying the Recycle Bin.", async delegate { await EmptyRecycleBinAsync(); }), 1, 1);
            layout.Controls.Add(MaintenanceCard("Disk analysis", "Measure common cleanup targets without deleting anything.", async delegate { await AnalyzeDiskAsync(); }), 0, 2);
            layout.Controls.Add(MaintenanceCard("Component cleanup", "Run Windows DISM component cleanup (not Windows repair).", async delegate { await RunMaintenanceAsync("Component cleanup", RunComponentCleanup); }), 1, 2);
            layout.Controls.Add(MaintenanceCard("Winget app updates", "Update apps with Windows Package Manager. Package agreements are accepted.", async delegate { await RunMaintenanceAsync("App updates", RunWingetUpdates); }), 0, 3);
            layout.Controls.Add(MaintenanceCard("Recommended apps", "Install LocalSend or Java using winget. Review package details first.", async delegate { await InstallRecommendedAppAsync(); }), 1, 3);
            layout.Controls.Add(MaintenanceCard("Disk Cleanup", "Open Windows Disk Cleanup so you can choose categories yourself.", async delegate { await RunMaintenanceAsync("Disk Cleanup", RunDiskCleanup); }), 0, 4);
            layout.Controls.Add(MaintenanceCard("Run everything, then exit", "Temp cleanup, update cache, Disk Cleanup, system repair and app updates.", async delegate { await RunEverythingAsync(); }), 0, 5);
            layout.Controls.Add(MaintenanceCard("Deep clean + repair", "Temp cleanup, update cache, Disk Cleanup and system repair.", async delegate { await RunDeepCleanAsync(); }), 1, 5);
            var note = new Label { Text = "These longer workflows ask once before starting, may open Windows tools or request UAC, and stop at the first failed step.\r\n\r\nCleanups affect this Windows installation. Review prompts before confirming.", Dock = DockStyle.Fill, ForeColor = UiTheme.Muted, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12) };
            layout.Controls.Add(note, 0, 6);
            layout.SetColumnSpan(note, 2);
            return page;
        }

        private TabPage BuildSettingsPage()
        {
            var page = new TabPage("Preferences") { BackColor = UiTheme.Background, Padding = new Padding(24) };
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, RowCount = 6, Padding = new Padding(12), BackColor = UiTheme.Background };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            page.Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "Accent color", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = muted }, 0, 0);
            accentPicker.DropDownStyle = ComboBoxStyle.DropDownList;
            accentPicker.DrawMode = DrawMode.OwnerDrawFixed;
            accentPicker.ItemHeight = 22;
            accentPicker.DropDownWidth = 220;
            accentPicker.Width = 240;
            accentPicker.Items.AddRange(AppPreferences.AccentNames);
            accentPicker.DrawItem += delegate(object sender, DrawItemEventArgs e)
            {
                int index = e.Index >= 0 ? e.Index : accentPicker.SelectedIndex;
                if (index < 0 || index >= accentPicker.Items.Count) return;
                string name = Convert.ToString(accentPicker.Items[index]);
                Color fill = (e.State & DrawItemState.Selected) != 0 ? UiTheme.Elevated : UiTheme.Background;
                using (var background = new SolidBrush(fill)) e.Graphics.FillRectangle(background, e.Bounds);
                var swatch = new Rectangle(e.Bounds.Left + 6, e.Bounds.Top + 4, 20, Math.Max(12, e.Bounds.Height - 8));
                using (var brush = new SolidBrush(AppPreferences.GetAccentColor(name))) e.Graphics.FillRectangle(brush, swatch);
                using (var pen = new Pen(UiTheme.Border)) e.Graphics.DrawRectangle(pen, swatch);
                TextRenderer.DrawText(e.Graphics, name, accentPicker.Font, new Rectangle(swatch.Right + 8, e.Bounds.Top, e.Bounds.Width - swatch.Right - 8, e.Bounds.Height), UiTheme.Text, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                if ((e.State & DrawItemState.Focus) != 0) e.DrawFocusRectangle();
            };
            accentPicker.SelectedItem = AppPreferences.AccentName;
            accentPicker.SelectedIndexChanged += delegate { ApplyAccent(); };
            layout.Controls.Add(accentPicker, 1, 0);
            layout.Controls.Add(new Label { Text = "Display name", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = muted }, 0, 1);
            userNameInput.Text = AppPreferences.UserName;
            userNameInput.Width = 240;
            layout.Controls.Add(userNameInput, 1, 1);
            var save = new Button { Text = "Save name", Tag = "primary", Width = 130, Height = 34 };
            save.Click += delegate { AppPreferences.SetUserName(userNameInput.Text); welcome.Text = GetWelcomeText(); ActivityLog.Write("Display name updated"); };
            layout.Controls.Add(save, 1, 2);
            var reset = new Button { Text = "Reset name to Windows account", Width = 230, Height = 34 };
            reset.Click += delegate { userNameInput.Text = Environment.UserName; AppPreferences.SetUserName(Environment.UserName); welcome.Text = GetWelcomeText(); };
            layout.Controls.Add(reset, 1, 3);
            var rate = new Button { Text = "Rate Winclean…", Width = 150, Height = 34 };
            rate.Click += delegate { ShowRating(); };
            layout.Controls.Add(new Label { Text = "Feedback", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = muted }, 0, 4);
            layout.Controls.Add(rate, 1, 4);
            var restart = new Button { Text = "Restart Winclean", Width = 150, Height = 34 };
            restart.Click += delegate { if (MessageBox.Show(this, "Close and reopen Winclean? Any running operation must finish first.", "Restart Winclean", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes && !busy) Application.Restart(); };
            layout.Controls.Add(new Label { Text = "Application", AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = muted }, 0, 5);
            layout.Controls.Add(restart, 1, 5);
            return page;
        }

        private TabPage BuildToolsPage()
        {
            var page = new TabPage("System tools") { BackColor = BackColor, Padding = new Padding(12) };
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = UiTheme.Background };
            var layout = new TableLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Top, ColumnCount = 2, RowCount = 5, Padding = new Padding(6), BackColor = UiTheme.Background };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            scroll.Controls.Add(layout);
            page.Controls.Add(scroll);
            layout.Controls.Add(MaintenanceCard("System file repair", "DISM RestoreHealth followed by System File Checker.", async delegate { await RunMaintenanceAsync("System file repair", RunRepair); }), 0, 0);
            layout.Controls.Add(MaintenanceCard("Disk health", "Run a read-only or repair CHKDSK check on a selected drive.", async delegate { await RunDiskCheckAsync(); }), 1, 0);
            layout.Controls.Add(MaintenanceCard("Memory diagnostic", "Open Windows Memory Diagnostic. Testing requires a restart.", delegate { if (MessageBox.Show(this, "Save your work first. Windows Memory Diagnostic can restart the PC to test memory. Open the tool now?", "Memory diagnostic", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes) Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "mdsched.exe")); return Task.CompletedTask; }), 0, 1);
            layout.Controls.Add(MaintenanceCard("Restore point", "Request a Windows restore point. Windows may limit creation to once per day.", async delegate { await RunMaintenanceAsync("Restore point", CreateRestorePoint); }), 1, 1);
            layout.Controls.Add(MaintenanceCard("System information", "View Windows, CPU, memory and disk details.", delegate { ShowSystemInfo(); return Task.CompletedTask; }), 0, 2);
            layout.Controls.Add(MaintenanceCard("Network repair", "Flush DNS or reset Winsock. Winsock changes require a restart.", async delegate { await RunNetworkToolsAsync(); }), 1, 2);
            layout.Controls.Add(MaintenanceCard("Drivers", "Open Windows Update, list drivers or export a backup.", async delegate { await RunDriverToolsAsync(); }), 0, 3);
            layout.Controls.Add(MaintenanceCard("Folder locker", "Password-protect access to a folder; restore its saved permissions to unlock.", async delegate { await RunFolderLockerAsync(); }), 1, 3);
            layout.Controls.Add(MaintenanceCard("Activity log", "View Winclean's local record of actions and results.", delegate { ShowActivityLog(); return Task.CompletedTask; }), 0, 4);
            return page;
        }

        private void ApplyAccent()
        {
            if (accentPicker.SelectedItem == null) return;
            try
            {
                AppPreferences.SetAccent(Convert.ToString(accentPicker.SelectedItem));
                ApplyAccentToControls(Controls);
                Invalidate(true);
                ActivityLog.Write("Accent changed to " + AppPreferences.AccentName);
            }
            catch (Exception ex) { MessageBox.Show(this, "Could not save the accent color.\r\n\r\n" + ex.Message, "Preferences", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void ApplyAccentToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Button && (String.Equals(control.Tag as string, "primary", StringComparison.Ordinal) || String.Equals(control.Tag as string, "card", StringComparison.Ordinal)))
                {
                    bool primary = String.Equals(control.Tag as string, "primary", StringComparison.Ordinal);
                    control.BackColor = primary ? AppPreferences.Accent : UiTheme.Surface;
                    control.ForeColor = primary ? UiTheme.AccentText : AppPreferences.Accent;
                }
                if (control is TabControl) control.Invalidate();
                if (control.HasChildren) ApplyAccentToControls(control.Controls);
            }
        }

        private TabPage BuildMaintenancePage()
        {
            return BuildCleanupPage();
        }

        private Control MaintenanceCard(string title, string description, Func<Task> action)
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface, Margin = new Padding(6), Padding = new Padding(12), BorderStyle = BorderStyle.FixedSingle };
            var button = new Button { Text = title, Tag = "card", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Location = new Point(12, 10), FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, BackColor = UiTheme.Surface, ForeColor = blue, Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            button.Click += async delegate { if (!busy) await action(); };
            var label = new Label { Text = description, Location = new Point(14, 42), Size = new Size(Math.Max(180, panel.ClientSize.Width - 28), 38), Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right, ForeColor = muted, AutoEllipsis = true };
            panel.Resize += delegate { label.Width = Math.Max(180, panel.ClientSize.Width - 28); };
            panel.Controls.Add(button);
            panel.Controls.Add(label);
            return panel;
        }

        private async Task RunMaintenanceAsync(string title, Action action)
        {
            if (MessageBox.Show(this, "Run " + title + "?\r\n\r\nWinclean will call Windows' built-in tool and show its output. Check the output before closing it.", title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            SetBusy(true, title + " is running...");
            ActivityLog.Write("Started: " + title);
            try
            {
                await Task.Run(action);
                ActivityLog.Write("Finished: " + title);
                MessageBox.Show(this, title + " finished. Review Windows' output for details.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ActivityLog.Write("Failed: " + title + " (" + ex.Message + ")");
                MessageBox.Show(this, title + " failed to run.\r\n\r\n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false, null); }
        }

        private async Task RunEverythingAsync()
        {
            string description = "This will run, in order:\r\n\u2022 Current-user temporary file cleanup\r\n\u2022 Windows Update download cleanup (UAC may appear)\r\n\u2022 Windows Disk Cleanup (choose categories in its window)\r\n\u2022 DISM and System File Checker repair (UAC may appear)\r\n\u2022 All available winget app updates\r\n\r\nThis may take a long time. Continue?";
            if (MessageBox.Show(this, description, "Run all maintenance", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

            Action[] actions = { RemoveTemporaryFiles, RemoveUpdateCache, RunDiskCleanup, RunRepair, RunWingetUpdates };
            string[] names = { "Temporary files", "Windows Update downloads", "Disk Cleanup", "System file repair", "Winget app updates" };
            string previousTitle = Text;
            string currentStep = "Starting maintenance";
            SetBusy(true, currentStep + "...");
            Text = "Winclean - Run all maintenance";
            ActivityLog.Write("Started: Run all maintenance");
            bool closeAfterRun = false;
            try
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    currentStep = names[i];
                    Text = "Winclean - " + (i + 1).ToString() + "/" + actions.Length.ToString() + " " + currentStep;
                    status.Text = (i + 1).ToString() + "/" + actions.Length.ToString() + "  " + currentStep + "...";
                    ActivityLog.Write("Started: " + currentStep + " (Run all)");
                    await Task.Run(actions[i]);
                    ActivityLog.Write("Finished: " + currentStep + " (Run all)");
                }
                ActivityLog.Write("Finished: Run all maintenance");
                MessageBox.Show(this, "All maintenance steps finished. Review each Windows tool's output for details. Winclean will now close.", "Run everything", MessageBoxButtons.OK, MessageBoxIcon.Information);
                closeAfterRun = true;
            }
            catch (Exception ex)
            {
                ActivityLog.Write("Run all maintenance stopped at " + currentStep + ": " + ex.Message);
                MessageBox.Show(this, "Run all maintenance stopped at '" + currentStep + "'. Later steps were not run.\r\n\r\n" + ex.Message, "Maintenance stopped", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Text = previousTitle;
                SetBusy(false, null);
                if (closeAfterRun) Close();
            }
        }

        private async Task RunDeepCleanAsync()
        {
            string description = "This will run, in order:\r\n• Current-user temporary file cleanup\r\n• Windows Update download cleanup (UAC may appear)\r\n• Windows Disk Cleanup (choose categories in its window)\r\n• DISM and System File Checker repair (UAC may appear)\r\n\r\nThis may take a long time. Continue?";
            if (MessageBox.Show(this, description, "Deep clean + repair", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

            Action[] actions = { RemoveTemporaryFiles, RemoveUpdateCache, RunDiskCleanup, RunRepair };
            string[] names = { "Temporary files", "Windows Update downloads", "Disk Cleanup", "System file repair" };
            string previousTitle = Text;
            string currentStep = "Starting maintenance";
            SetBusy(true, currentStep + "...");
            Text = "Winclean - Deep clean + repair";
            ActivityLog.Write("Started: Deep clean + repair");
            try
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    currentStep = names[i];
                    Text = "Winclean - " + (i + 1).ToString() + "/" + actions.Length.ToString() + " " + currentStep;
                    status.Text = (i + 1).ToString() + "/" + actions.Length.ToString() + "  " + currentStep + "...";
                    ActivityLog.Write("Started: " + currentStep + " (Deep clean + repair)");
                    await Task.Run(actions[i]);
                    ActivityLog.Write("Finished: " + currentStep + " (Deep clean + repair)");
                }
                ActivityLog.Write("Finished: Deep clean + repair");
                MessageBox.Show(this, "Deep clean + repair finished. Review each Windows tool's output for details.", "Deep clean + repair", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ActivityLog.Write("Deep clean + repair stopped at " + currentStep + ": " + ex.Message);
                MessageBox.Show(this, "Deep clean + repair stopped at '" + currentStep + "'. Later steps were not run.\r\n\r\n" + ex.Message, "Maintenance stopped", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { Text = previousTitle; SetBusy(false, null); }
        }

        private void ShowActivityLog()
        {
            using (var dialog = new ActivityLogDialog()) dialog.ShowDialog(this);
        }

        private void ShowRating()
        {
            using (var dialog = new RatingDialog())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                ActivityLog.Write("User rating: " + dialog.Rating.ToString() + "/10 (local only)");
                MessageBox.Show(this, "Thanks for the feedback. The rating stays on this PC and is not sent anywhere.", "Winclean feedback", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task RunNetworkToolsAsync()
        {
            using (var dialog = new NetworkToolsDialog())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.SelectedAction == 0) return;
                bool resetWinsock = dialog.SelectedAction == 2 || dialog.SelectedAction == 3;
                bool flushDns = dialog.SelectedAction == 1 || dialog.SelectedAction == 3;
                if (resetWinsock && MessageBox.Show(this, "Resetting Winsock may affect VPN, security, or network-filter software. You should restart Windows afterwards. Continue?", "Confirm Winsock reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                await RunMaintenanceAsync(flushDns && resetWinsock ? "Flush DNS and reset Winsock" : (flushDns ? "Flush DNS cache" : "Reset Winsock"), delegate
                {
                    if (flushDns)
                    {
                        string ipconfig = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ipconfig.exe");
                        using (var process = Process.Start(new ProcessStartInfo { FileName = ipconfig, Arguments = "/flushdns", UseShellExecute = true }))
                        {
                            if (process == null) throw new InvalidOperationException("ipconfig did not start.");
                            process.WaitForExit();
                            if (process.ExitCode != 0) throw new InvalidOperationException("ipconfig returned " + process.ExitCode + ".");
                        }
                    }
                    if (resetWinsock) AppScanner.RunPowerShellElevated("netsh winsock reset; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }");
                });
            }
        }

        private async Task RunFolderLockerAsync()
        {
            var choice = MessageBox.Show(this, "Yes = lock a folder. No = unlock a folder managed by Winclean. Cancel = do nothing.\r\n\r\nThis is an access-control lock, not encryption. An administrator can still access the files.", "Folder locker", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button3);
            if (choice == DialogResult.Cancel) return;
            if (choice == DialogResult.Yes)
            {
                using (var picker = new FolderBrowserDialog { Description = "Choose a personal folder to lock. Do not select Windows, application, or profile root folders.", ShowNewFolderButton = false })
                {
                    if (picker.ShowDialog(this) != DialogResult.OK) return;
                    string password;
                    if (!PromptForPassword(this, "Set folder password", true, out password)) return;
                    try
                    {
                        await Task.Run(delegate { FolderLockManager.Lock(picker.SelectedPath, password); });
                        MessageBox.Show(this, "Folder locked. To unlock it later, choose Folder locker > Unlock and select the same folder. The original permissions are backed up locally.\r\n\r\nThis does not encrypt the files and is not a substitute for a backup.", "Folder locked", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex) { MessageBox.Show(this, "Winclean could not lock this folder. Its permissions were restored if the lock did not complete.\r\n\r\n" + ex.Message, "Folder lock failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
            else
            {
                string[] folders = FolderLockManager.GetLockedFolders();
                if (folders.Length == 0)
                {
                    MessageBox.Show(this, "No Winclean-locked folders were found for this Windows account.", "Folder locker", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string path;
                using (var picker = new LockedFolderDialog(folders))
                    if (picker.ShowDialog(this) != DialogResult.OK) return;
                    else path = picker.SelectedPath;
                string password;
                if (!PromptForPassword(this, "Unlock folder", false, out password)) return;
                try
                {
                    await Task.Run(delegate { FolderLockManager.Unlock(path, password); });
                    MessageBox.Show(this, "Folder unlocked. Its original permissions have been restored.", "Folder unlocked", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(this, "Winclean could not unlock this folder. The saved permissions are still available to retry.\r\n\r\n" + ex.Message, "Folder unlock failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private static bool PromptForPassword(IWin32Window owner, string title, bool confirm, out string password)
        {
            using (var dialog = new PasswordDialog(title, confirm))
            {
                if (dialog.ShowDialog(owner) == DialogResult.OK)
                {
                    password = dialog.Password;
                    return true;
                }
            }
            password = null;
            return false;
        }

        private async Task RunDriverToolsAsync()
        {
            string driverCount = "Driver package count unavailable.";
            try
            {
                string output = await Task.Run(delegate { return AppScanner.RunPowerShell("(& pnputil.exe /enum-drivers 2>$null | Select-String -Pattern 'oem[0-9]+\\.inf' | Measure-Object).Count", true); });
                int count;
                if (Int32.TryParse(output.Trim(), out count)) driverCount = count.ToString("N0") + " driver packages are in the driver store.";
            }
            catch { }
            var choice = MessageBox.Show(this, driverCount + "\r\n\r\nYes = open Windows Update. No = choose another driver tool. Cancel = do nothing.", "Driver tools", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
            if (choice == DialogResult.Cancel) return;
            if (choice == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo { FileName = "ms-settings:windowsupdate", UseShellExecute = true });
                return;
            }
            var secondary = MessageBox.Show(this, "Yes = list installed driver packages. No = export a driver backup to your Desktop. Cancel = go back.", "Driver tools", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
            if (secondary == DialogResult.Cancel) return;
            if (secondary == DialogResult.Yes)
            {
                await RunMaintenanceAsync("List installed drivers", delegate { using (var p = Process.Start(new ProcessStartInfo { FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "pnputil.exe"), Arguments = "/enum-drivers", UseShellExecute = true })) { if (p == null) throw new InvalidOperationException("pnputil did not start."); p.WaitForExit(); } });
                return;
            }
            string destination = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "winclean-drivers");
            if (MessageBox.Show(this, "Export third-party driver packages to:\r\n" + destination + "\r\n\r\nContinue?", "Driver backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            await RunMaintenanceAsync("Driver backup", delegate { AppScanner.RunPowerShellElevated("$p='" + destination.Replace("'", "''") + "'; New-Item -ItemType Directory -Force -Path $p | Out-Null; pnputil.exe /export-driver '*' $p; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }"); });
        }

        private async Task InstallRecommendedAppAsync()
        {
            var choice = MessageBox.Show(this, "Yes = install LocalSend. No = install Oracle Java Runtime Environment. Cancel = do nothing.\r\n\r\nWinclean will use winget and accept the package/source agreements.", "Install recommended app", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
            if (choice == DialogResult.Cancel) return;
            string package = choice == DialogResult.Yes ? "localsend" : "Oracle.JavaRuntimeEnvironment";
            await RunMaintenanceAsync("Install " + package, delegate { EnsureWinget(); using (var p = Process.Start(new ProcessStartInfo { FileName = "winget.exe", Arguments = "install --id " + package + " -e --accept-package-agreements --accept-source-agreements", UseShellExecute = true })) { if (p == null) throw new InvalidOperationException("winget did not start."); p.WaitForExit(); if (p.ExitCode != 0) throw new InvalidOperationException("winget returned " + p.ExitCode + "."); } });
        }

        private async Task EmptyRecycleBinAsync()
        {
            if (MessageBox.Show(this, "Empty the Recycle Bin for all drives? This cannot be undone.", "Empty Recycle Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            SetBusy(true, "Emptying the Recycle Bin...");
            try { await Task.Run(delegate { AppScanner.RunPowerShell("Clear-RecycleBin -Force -ErrorAction Stop", false); }); MessageBox.Show(this, "Recycle Bin cleanup finished.", "Recycle Bin", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(this, "Recycle Bin cleanup failed.\r\n\r\n" + ex.Message, "Recycle Bin", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetBusy(false, null); }
        }

        private async Task AnalyzeDiskAsync()
        {
            SetBusy(true, "Measuring cleanup targets...");
            try
            {
                var report = await Task.Run(delegate
                {
                    string script = @"$targets=New-Object System.Collections.Generic.List[string];$targets.Add($env:TEMP);$targets.Add((Join-Path $env:WINDIR 'Temp'));$targets.Add((Join-Path $env:PUBLIC 'temp'));$targets.Add((Join-Path $env:SystemDrive '$Recycle.Bin'));$targets.Add((Join-Path $env:WINDIR 'SoftwareDistribution\\Download'));$targets.Add((Join-Path $env:ProgramData 'Microsoft\\Windows\\DeliveryOptimization\\Cache'));$targets.Add((Join-Path $env:ProgramData 'Microsoft\\Windows\\WER\\ReportArchive'));$roots=@(@{Path=(Join-Path $env:LOCALAPPDATA 'Google\\Chrome\\User Data');Caches=@('Cache','Code Cache','GPUCache')},@{Path=(Join-Path $env:LOCALAPPDATA 'Microsoft\\Edge\\User Data');Caches=@('Cache','Code Cache','GPUCache')},@{Path=(Join-Path $env:LOCALAPPDATA 'Mozilla\\Firefox\\Profiles');Caches=@('cache2')});foreach($root in $roots){if(Test-Path -LiteralPath $root.Path){foreach($profile in Get-ChildItem -LiteralPath $root.Path -Directory -Force -ErrorAction SilentlyContinue){foreach($cache in $root.Caches){$targets.Add((Join-Path $profile.FullName $cache))}}}};foreach($path in $targets){$bytes=0L;$count=0L;if(Test-Path -LiteralPath $path){Get-ChildItem -LiteralPath $path -Force -Recurse -File -ErrorAction SilentlyContinue|ForEach-Object{$bytes+=$_.Length;$count++}};'{0}|{1:N0} MB|{2:N0} files' -f $path,($bytes/1MB),$count }";
                    return AppScanner.RunPowerShell(script, true);
                });
                MessageBox.Show(this, report, "Disk analysis (read-only)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(this, "Analysis failed. Nothing was changed.\r\n\r\n" + ex.Message, "Disk analysis", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { SetBusy(false, null); }
        }

        private async Task RunDiskCheckAsync()
        {
            using (var dialog = new DiskCheckDialog(DriveInfo.GetDrives().Where(drive => drive.IsReady).Select(drive => drive.Name).ToArray()))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string drive = dialog.SelectedDrive;
                bool repair = dialog.RepairErrors;
                if (repair && MessageBox.Show(this, "CHKDSK /F changes filesystem metadata and may schedule the repair for your next restart. Back up important files first. Continue?", "Confirm disk repair", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
                await RunMaintenanceAsync((repair ? "Check and repair " : "Read-only disk check ") + drive, delegate
                {
                    string exe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "chkdsk.exe");
                    string args = drive.TrimEnd('\\') + (repair ? " /F" : "");
                    using (var p = Process.Start(new ProcessStartInfo { FileName = exe, Arguments = args, UseShellExecute = true, Verb = repair ? "runas" : null })) { if (p != null) p.WaitForExit(); }
                });
            }
        }

        private void ShowSystemInfo()
        {
            string script = "$os=Get-CimInstance Win32_OperatingSystem;$cpu=Get-CimInstance Win32_Processor|Select-Object -First 1;$cs=Get-CimInstance Win32_ComputerSystem; 'Windows: '+$os.Caption+' ('+$os.Version+' build '+$os.BuildNumber+')';'CPU: '+$cpu.Name;'Memory: '+[math]::Round($cs.TotalPhysicalMemory/1GB,1)+' GB';Get-CimInstance Win32_LogicalDisk -Filter 'DriveType=3'|ForEach-Object{'Disk '+$_.DeviceID+': '+[math]::Round($_.FreeSpace/1GB,1)+' GB free of '+[math]::Round($_.Size/1GB,1)+' GB'}";
            try { MessageBox.Show(this, AppScanner.RunPowerShell(script, true), "System information", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "System information", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static void RemoveTemporaryFiles()
        {
            RemoveContents(Path.GetTempPath());
            string script = "$roots=@((Join-Path $env:WINDIR 'Temp'),(Join-Path $env:PUBLIC 'temp'));foreach($root in $roots){if(-not(Test-Path -LiteralPath $root -PathType Container)){continue};try{if(([IO.File]::GetAttributes($root) -band [IO.FileAttributes]::ReparsePoint) -ne 0){continue}}catch{continue};$pending=New-Object 'System.Collections.Generic.Stack[string]';$dirs=New-Object 'System.Collections.Generic.List[string]';$pending.Push($root);while($pending.Count -gt 0){$current=$pending.Pop();try{foreach($entry in [IO.Directory]::EnumerateFileSystemEntries($current)){try{$attr=[IO.File]::GetAttributes($entry);if(($attr -band [IO.FileAttributes]::ReparsePoint) -ne 0){if(($attr -band [IO.FileAttributes]::Directory) -ne 0){[IO.Directory]::Delete($entry,$false)}else{[IO.File]::Delete($entry)};continue};if(($attr -band [IO.FileAttributes]::Directory) -ne 0){$dirs.Add($entry);$pending.Push($entry)}else{[IO.File]::SetAttributes($entry,[IO.FileAttributes]::Normal);[IO.File]::Delete($entry)}}catch{}}}catch{}};for($i=$dirs.Count-1;$i -ge 0;$i--){try{[IO.Directory]::Delete($dirs[$i],$false)}catch{}}}";
            AppScanner.RunPowerShellElevated(script);
        }

        private static void RemoveBrowserCaches()
        {
            string[] roots = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data")
            };
            foreach (string root in roots)
            {
                if (!Directory.Exists(root)) continue;
                foreach (string profile in Directory.GetDirectories(root))
                foreach (string pattern in new string[] { "Cache", "Code Cache", "GPUCache" })
                    RemoveContents(Path.Combine(profile, pattern));
            }
            string firefox = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla", "Firefox", "Profiles");
            if (Directory.Exists(firefox)) foreach (string profile in Directory.GetDirectories(firefox))
            {
                RemoveContents(Path.Combine(profile, "cache2"));
                RemoveContents(Path.Combine(profile, "startupCache"));
            }
        }

        private static void RemoveUpdateCache()
        {
            string script = "$ErrorActionPreference='Stop';$names=@('wuauserv','bits');$stopped=@();try{foreach($n in $names){$s=Get-Service -Name $n -ErrorAction SilentlyContinue;if($s -and $s.Status -eq 'Running'){Stop-Service -Name $n -Force -ErrorAction Stop;$stopped+=$n}};$p=Join-Path $env:WINDIR 'SoftwareDistribution\\Download';if(Test-Path -LiteralPath $p){Get-ChildItem -LiteralPath $p -Force -ErrorAction SilentlyContinue|Remove-Item -Force -Recurse -ErrorAction Stop};try{Delete-DeliveryOptimizationCache -Force -ErrorAction SilentlyContinue}catch{}}finally{foreach($n in $stopped){Start-Service -Name $n -ErrorAction SilentlyContinue}};$dism=Join-Path $env:WINDIR 'System32\\Dism.exe';& $dism /online /Cleanup-Image /StartComponentCleanup;if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}";
            AppScanner.RunPowerShellElevated(script);
        }

        private static void RunWingetUpdates()
        {
            EnsureWinget();
            using (var p = Process.Start(new ProcessStartInfo { FileName = "winget.exe", Arguments = "upgrade --all --accept-package-agreements --accept-source-agreements", UseShellExecute = true })) { if (p == null) throw new InvalidOperationException("winget did not start."); p.WaitForExit(); if (p.ExitCode != 0) throw new InvalidOperationException("winget returned " + p.ExitCode + "."); }
        }

        private static void EnsureWinget()
        {
            try { using (var p = Process.Start(new ProcessStartInfo { FileName = "where.exe", Arguments = "winget", UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true })) { p.WaitForExit(); if (p.ExitCode != 0) throw new InvalidOperationException("winget was not found. Install or update App Installer from Microsoft Store."); } }
            catch (System.ComponentModel.Win32Exception) { throw new InvalidOperationException("winget was not found. Install or update App Installer from Microsoft Store."); }
        }

        private static void RunDiskCleanup()
        {
            string cleanmgr = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "cleanmgr.exe");
            if (!File.Exists(cleanmgr)) throw new InvalidOperationException("Windows Disk Cleanup (cleanmgr.exe) is not available on this system.");
            string script = "$ErrorActionPreference='Stop';$root='HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VolumeCaches';$protected=@('Recycle Bin','Previous Installations','Windows ESD installation files','User file versions','Downloads');$saved=@{};Get-ChildItem -LiteralPath $root -ErrorAction Stop|ForEach-Object{$key=Get-Item -LiteralPath $_.PSPath;$exists=$key.GetValueNames() -contains 'StateFlags0001';$saved[$_.PSPath]=[pscustomobject]@{Exists=$exists;Value=$(if($exists){$key.GetValue('StateFlags0001',$null,'DoNotExpandEnvironmentNames')});Kind=$(if($exists){$key.GetValueKind('StateFlags0001')})};if($protected -contains $_.PSChildName){Remove-ItemProperty -LiteralPath $_.PSPath -Name StateFlags0001 -ErrorAction SilentlyContinue}else{New-ItemProperty -LiteralPath $_.PSPath -Name StateFlags0001 -PropertyType DWord -Value 2 -Force|Out-Null}};try{$p=Start-Process -FilePath '" + cleanmgr.Replace("'", "''") + "' -ArgumentList '/sagerun:1' -Wait -PassThru;if($p.ExitCode -ne 0){exit $p.ExitCode}}finally{foreach($path in $saved.Keys){$state=$saved[$path];if($state.Exists){New-ItemProperty -LiteralPath $path -Name StateFlags0001 -PropertyType $state.Kind -Value $state.Value -Force|Out-Null}else{Remove-ItemProperty -LiteralPath $path -Name StateFlags0001 -ErrorAction SilentlyContinue}}}";
            AppScanner.RunPowerShellElevated(script);
        }

        private static void RunComponentCleanup()
        {
            using (var p = Process.Start(new ProcessStartInfo { FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "Dism.exe"), Arguments = "/Online /Cleanup-Image /StartComponentCleanup", UseShellExecute = true, Verb = "runas" })) { if (p == null) throw new InvalidOperationException("DISM did not start."); p.WaitForExit(); if (p.ExitCode != 0) throw new InvalidOperationException("DISM returned " + p.ExitCode + "."); }
        }

        private static void RunRepair()
        {
            string dism = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "Dism.exe");
            using (var p = Process.Start(new ProcessStartInfo { FileName = dism, Arguments = "/Online /Cleanup-Image /RestoreHealth", UseShellExecute = true, Verb = "runas" }))
            {
                if (p == null) throw new InvalidOperationException("DISM did not start.");
                p.WaitForExit();
                if (p.ExitCode != 0) throw new InvalidOperationException("DISM returned " + p.ExitCode + ". SFC was not run because the component store repair failed.");
            }
            using (var p = Process.Start(new ProcessStartInfo { FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "sfc.exe"), Arguments = "/scannow", UseShellExecute = true, Verb = "runas" })) { if (p == null) throw new InvalidOperationException("SFC did not start."); p.WaitForExit(); if (p.ExitCode != 0) throw new InvalidOperationException("SFC returned " + p.ExitCode + "."); }
        }

        private static void CreateRestorePoint()
        {
            AppScanner.RunPowerShellElevated("$ErrorActionPreference='Stop'; Checkpoint-Computer -Description 'Winclean' -RestorePointType MODIFY_SETTINGS -ErrorAction Stop");
        }

        private static void RemoveContents(string path)
        {
            if (!Directory.Exists(path)) return;
            DirectoryInfo root;
            try { root = new DirectoryInfo(path); if ((root.Attributes & FileAttributes.ReparsePoint) != 0) return; }
            catch { return; }
            foreach (string file in Directory.GetFiles(path))
            {
                try
                {
                    if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0) continue;
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch { }
            }
            foreach (string directory in Directory.GetDirectories(path))
            {
                try
                {
                    var child = new DirectoryInfo(directory);
                    if ((child.Attributes & FileAttributes.ReparsePoint) != 0) continue;
                    RemoveContents(directory);
                    Directory.Delete(directory, false);
                }
                catch { }
            }
        }

        private static void SetAppSplitPosition(SplitContainer split, bool initialize)
        {
            int maxDistance = split.ClientSize.Width - split.Panel2MinSize - split.SplitterWidth;
            int minDistance = split.Panel1MinSize;
            if (maxDistance < minDistance) return;
            int preferred = Math.Max(minDistance, Math.Min(split.ClientSize.Width - 300, maxDistance));
            if (initialize || split.SplitterDistance < minDistance || split.SplitterDistance > maxDistance)
                split.SplitterDistance = preferred;
        }

        private void AddDetail(Control parent, string caption, Label value, int y)
        {
            parent.Controls.Add(new Label { Text = caption.ToUpperInvariant(), Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold), ForeColor = muted, AutoSize = true, Location = new Point(18, y) });
            value.SetBounds(18, y + 19, 260, 30);
            value.Font = new Font("Segoe UI", 9.5F);
            value.ForeColor = ink;
            value.AutoEllipsis = true;
            parent.Controls.Add(value);
        }

        private void StyleButton(Button button, bool primary)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = UiTheme.Border;
            button.Tag = primary ? "primary" : null;
            button.BackColor = primary ? blue : UiTheme.Elevated;
            button.ForeColor = primary ? Color.White : ink;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.Margin = new Padding(8, 0, 0, 0);
        }

        private async Task LoadAppsAsync(string preserveIdentity)
        {
            if (busy) return;
            busy = true;
            Cursor = Cursors.WaitCursor;
            refreshButton.Enabled = false;
            status.Text = "Reading installed applications...";
            try
            {
                var result = await Task.Run(delegate { return AppScanner.Load(); });
                allApps.Clear();
                allApps.AddRange(result);
                ShowFilteredApps();
                if (!String.IsNullOrEmpty(preserveIdentity))
                {
                    for (int i = 0; i < grid.Rows.Count; i++)
                    {
                        var item = grid.Rows[i].DataBoundItem as AppEntry;
                        if (item != null && item.Identity == preserveIdentity)
                        {
                            grid.Rows[i].Selected = true;
                            break;
                        }
                    }
                }
                SetStatus(allApps.Count.ToString("N0") + " applications found. Select one to see options.");
            }
            catch (Exception ex)
            {
                SetStatus("Could not read the installed-app list: " + ex.Message);
            }
            finally { SetBusy(false, null); }
        }

        private void ShowFilteredApps()
        {
            string term = (search.Text ?? "").Trim();
            IEnumerable<AppEntry> filtered = allApps;
            if (term.Length > 0)
                filtered = filtered.Where(a => Contains(a.Name, term) || Contains(a.Publisher, term) || Contains(a.Source, term));
            grid.DataSource = null;
            grid.DataSource = filtered.OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
            countLabel.Text = grid.Rows.Count.ToString("N0") + " shown  /  " + allApps.Count.ToString("N0") + " total";
            countLabel.Left = Math.Max(search.Right + 18, countLabel.Parent == null ? 450 : countLabel.Parent.ClientSize.Width - countLabel.Width);
            UpdateSelection();
        }

        private static bool Contains(string text, string term)
        {
            return !String.IsNullOrEmpty(text) && text.IndexOf(term, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private void UpdateSelection()
        {
            if (grid.CurrentRow != null && grid.CurrentRow.DataBoundItem is AppEntry)
                selected = (AppEntry)grid.CurrentRow.DataBoundItem;
            else selected = null;

            detailName.Text = selected == null ? "No app selected" : selected.Name;
            detailPublisher.Text = selected == null ? "-" : (String.IsNullOrWhiteSpace(selected.Publisher) ? "Unknown" : selected.Publisher);
            detailVersion.Text = selected == null ? "-" : (String.IsNullOrWhiteSpace(selected.Version) ? "Not listed" : selected.Version);
            detailSource.Text = selected == null ? "-" : selected.Source;
            detailLocation.Text = selected == null ? "-" : selected.LocationText;
            bool hasUninstaller = selected != null && (selected.Source == "Store app" || !String.IsNullOrWhiteSpace(selected.UninstallCommand));
            status.Text = selected == null
                ? "Select an app to see its details."
                : (selected.IsProtected ? "This is a Windows component marked as non-removable."
                    : (!hasUninstaller ? "Windows has no registered uninstaller for this entry. Use Settings > Apps to manage it."
                    : "Uninstall opens the app's own uninstaller. Deep clean is a separate, reviewed step."));
            uninstallButton.Enabled = hasUninstaller && !selected.IsProtected && !busy;
        }

        private async Task UninstallSelectedAsync()
        {
            AppEntry app = selected;
            if (app == null || app.IsProtected || busy) return;
            string explanation = app.Source == "Store app"
                ? "Winclean will remove this package from the current Windows account only. Its local app data may remain until you review the optional deep-clean scan."
                : "Winclean will open the uninstaller registered by this application. It will not run a silent uninstall. Review the command below before continuing.";
            string command = app.Source == "Store app" ? "Remove-AppxPackage -Package '" + app.PackageFullName + "' (current account only)" : AppScanner.GetUninstallPreview(app);
            if (command.StartsWith("Not supported by Winclean:", StringComparison.Ordinal))
            {
                MessageBox.Show(this, app.Name + "\r\n\r\n" + explanation + "\r\n\r\n" + command + "\r\n\r\nUse Windows Settings > Apps > Installed apps to remove it instead.", "Use Windows Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this, app.Name + "\r\n\r\n" + explanation + "\r\n\r\nRegistered action:\r\n" + command + "\r\n\r\nContinue?", "Uninstall " + app.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                return;

            SetBusy(true, "Waiting for the uninstaller to finish...");
            ActivityLog.Write("Uninstall requested: " + app.Name);
            try
            {
                int exitCode = await Task.Run(delegate { return AppScanner.Uninstall(app); });
                if (exitCode != 0 && exitCode != 3010 && exitCode != 1641)
                {
                    ActivityLog.Write("Uninstaller failed: " + app.Name + " (exit code " + exitCode.ToString() + ")");
                    SetBusy(false, null);
                    MessageBox.Show(this, "The uninstaller returned exit code " + exitCode + ". The app may still be installed. Winclean will refresh the list, but will not clean leftovers automatically.", "Uninstall result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadAppsAsync(null);
                    return;
                }
                SetBusy(false, null);
                ActivityLog.Write("Uninstaller finished: " + app.Name + " (exit code " + exitCode.ToString() + ")");
                await LoadAppsAsync(null);
                if (exitCode == 3010 || exitCode == 1641)
                {
                    MessageBox.Show(this, "The uninstaller requested a restart. Winclean will not offer deep cleanup until after Windows restarts. Refresh the app list afterwards to check for leftovers.", "Restart required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                bool stillInstalled = allApps.Any(a => String.Equals(a.Identity, app.Identity, StringComparison.OrdinalIgnoreCase));
                if (stillInstalled)
                {
                    MessageBox.Show(this, "The uninstaller finished, but this app is still listed in Windows. Winclean will not offer deep cleanup because the uninstall may not have completed.", "App still installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (MessageBox.Show(this, "Uninstall finished. Do a deep-clean scan for leftover files and registry entries?\r\n\r\nYou will review every candidate before anything is removed. Nothing is selected automatically.", "Deep clean leftovers?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    await ScanAndCleanAsync(app, true);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                ActivityLog.Write("Uninstall could not start: " + app.Name + " (" + ex.Message + ")");
                MessageBox.Show(this, "The uninstaller could not be started or its elevation was cancelled.\r\n\r\n" + ex.Message, "Uninstall cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ActivityLog.Write("Uninstall failed: " + app.Name + " (" + ex.Message + ")");
                MessageBox.Show(this, "Winclean could not complete the uninstall request.\r\n\r\n" + ex.Message, "Uninstall failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetBusy(false, null); }
        }

        private async Task ScanAndCleanAsync(AppEntry app, bool launchedAfterUninstall)
        {
            if (app == null || busy) return;
            busy = true;
            Cursor = Cursors.WaitCursor;
            refreshButton.Enabled = false;
            status.Text = "Checking for exact-name leftovers...";
            List<CleanupCandidate> candidates;
            ActivityLog.Write("Deep-clean scan started: " + app.Name);
            try { candidates = await Task.Run(delegate { return AppScanner.FindLeftovers(app); }); }
            catch (Exception ex)
            {
                ActivityLog.Write("Deep-clean scan failed: " + app.Name + " (" + ex.Message + ")");
                SetBusy(false, null);
                MessageBox.Show(this, "The leftover scan failed. Nothing was changed.\r\n\r\n" + ex.Message, "Scan failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally { SetBusy(false, null); }

            if (candidates.Count == 0)
            {
                ActivityLog.Write("Deep-clean scan found no candidates: " + app.Name);
                MessageBox.Show(this, launchedAfterUninstall ? "Uninstall finished. No exact-name leftovers were found." : "No exact-name leftovers were found. Nothing was changed.", "Deep-clean scan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dialog = new CleanupForm(app, candidates))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                List<CleanupCandidate> selectedCandidates = dialog.SelectedCandidates;
                SetBusy(true, "Backing up registry entries and cleaning selected leftovers...");
                ActivityLog.Write("Deep-clean confirmed: " + selectedCandidates.Count.ToString() + " candidates for " + app.Name);
                try
                {
                    var result = await Task.Run(delegate { return AppScanner.Clean(selectedCandidates); });
                    var text = String.Join(Environment.NewLine, result.ToArray());
                    ActivityLog.Write("Deep-clean finished: " + app.Name + " (" + result.Count.ToString() + " result lines)");
                    MessageBox.Show(this, text + "\r\n\r\nRegistry backups are kept in the Winclean Backups folder. Deleted folders were sent to the Recycle Bin.", "Deep-clean results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ActivityLog.Write("Deep-clean failed: " + app.Name + " (" + ex.Message + ")");
                    MessageBox.Show(this, "Cleanup stopped. Any registry keys removed before the error were exported first.\r\n\r\n" + ex.Message, "Deep-clean error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally { SetBusy(false, null); }
            }
        }

        private void SetBusy(bool value, string message)
        {
            busy = value;
            Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            refreshButton.Enabled = !value;
            foreach (TabPage page in tabs.TabPages)
                foreach (Control control in page.Controls)
                    SetChildButtonsEnabled(control, !value);
            UpdateSelection();
            if (message != null) status.Text = message;
        }

        private static void SetChildButtonsEnabled(Control control, bool enabled)
        {
            foreach (Control child in control.Controls)
            {
                if (child is Button) child.Enabled = enabled;
                else SetChildButtonsEnabled(child, enabled);
            }
        }

        private void SetStatus(string text)
        {
            if (selected == null) status.Text = text;
        }
    }

    internal static class ActivityLog
    {
        private static readonly string LogDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Winclean", "Logs");
        private static readonly string LogFile = Path.Combine(LogDirectory, "activity.log");

        public static void Write(string message)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(LogFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + (message ?? "") + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        public static string ReadRecent()
        {
            if (!File.Exists(LogFile)) return "No activity has been recorded yet.";
            const int maxBytes = 262144;
            using (var stream = new FileStream(LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                long start = Math.Max(0, stream.Length - maxBytes);
                stream.Position = start;
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    string text = reader.ReadToEnd();
                    if (start > 0)
                    {
                        int firstLine = text.IndexOfAny(new char[] { '\r', '\n' });
                        text = firstLine < 0 ? "" : text.Substring(firstLine + 1).TrimStart('\r', '\n');
                        return "(Showing the most recent activity; older entries were omitted.)" + Environment.NewLine + text;
                    }
                    return text;
                }
            }
        }

        public static string DirectoryPath { get { return LogDirectory; } }
    }

    internal sealed class ActivityLogDialog : Form
    {
        private readonly TextBox content = new TextBox();

        public ActivityLogDialog()
        {
            Text = "Winclean activity";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(620, 400);
            Size = new Size(780, 520);
            BackColor = UiTheme.Background;
            Font = new Font("Segoe UI", 9F);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "Recent Winclean activity", Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold), ForeColor = UiTheme.Text, AutoSize = true }, 0, 0);
            content.Dock = DockStyle.Fill;
            content.Multiline = true;
            content.ReadOnly = true;
            content.WordWrap = false;
            content.ScrollBars = ScrollBars.Both;
            content.BackColor = UiTheme.Elevated;
            content.ForeColor = UiTheme.Text;
            content.Font = new Font("Consolas", 9F);
            layout.Controls.Add(content, 0, 1);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 8, 0, 0) };
            var close = new Button { Text = "Close", Width = 88, Height = 30, DialogResult = DialogResult.Cancel };
            var openFolder = new Button { Text = "Open log folder", Width = 120, Height = 30 };
            var refresh = new Button { Text = "Refresh", Width = 88, Height = 30 };
            openFolder.Click += delegate { Process.Start(new ProcessStartInfo { FileName = ActivityLog.DirectoryPath, UseShellExecute = true }); };
            refresh.Click += delegate { LoadContent(); };
            buttons.Controls.Add(close);
            buttons.Controls.Add(openFolder);
            buttons.Controls.Add(refresh);
            layout.Controls.Add(buttons, 0, 2);
            AcceptButton = close;
            CancelButton = close;
            UiTheme.Apply(this);
            LoadContent();
        }

        private void LoadContent()
        {
            try { content.Text = ActivityLog.ReadRecent(); content.SelectionStart = content.TextLength; content.ScrollToCaret(); }
            catch (Exception ex) { content.Text = "Could not read the activity log: " + ex.Message; }
        }
    }

    internal sealed class PasswordDialog : Form
    {
        private readonly TextBox password = new TextBox();
        private readonly TextBox confirmation = new TextBox();
        private readonly bool confirmPassword;

        public string Password { get { return password.Text; } }

        public PasswordDialog(string title, bool confirm)
        {
            confirmPassword = confirm;
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(430, confirm ? 205 : 160);
            Font = new Font("Segoe UI", 9F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = confirm ? 5 : 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            if (confirm) { layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); }
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = confirm ? "Choose a password (at least 8 characters):" : "Enter the folder password:", AutoSize = true }, 0, 0);
            password.Dock = DockStyle.Fill;
            password.UseSystemPasswordChar = true;
            password.MaxLength = 256;
            layout.Controls.Add(password, 0, 1);
            int buttonRow;
            if (confirm)
            {
                layout.Controls.Add(new Label { Text = "Confirm password:", AutoSize = true }, 0, 2);
                confirmation.Dock = DockStyle.Fill;
                confirmation.UseSystemPasswordChar = true;
                confirmation.MaxLength = 256;
                layout.Controls.Add(confirmation, 0, 3);
                buttonRow = 4;
            }
            else buttonRow = 2;
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 7, 0, 0) };
            var ok = new Button { Text = "Continue", Width = 95, Height = 30 };
            var cancel = new Button { Text = "Cancel", Width = 85, Height = 30, DialogResult = DialogResult.Cancel };
            ok.Click += delegate
            {
                if (confirmPassword && password.Text.Length < 8)
                {
                    MessageBox.Show(this, "Use a password of at least 8 characters.", "Password too short", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    password.Focus();
                    return;
                }
                if (confirmPassword && !String.Equals(password.Text, confirmation.Text, StringComparison.Ordinal))
                {
                    MessageBox.Show(this, "The passwords do not match.", "Password mismatch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    confirmation.Clear();
                    confirmation.Focus();
                    return;
                }
                if (password.Text.Length == 0)
                {
                    MessageBox.Show(this, "Enter the folder password.", "Password required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    password.Focus();
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 0, buttonRow);
            AcceptButton = ok;
            CancelButton = cancel;
            UiTheme.Apply(this);
        }
    }

    internal sealed class LockedFolderDialog : Form
    {
        private readonly ComboBox folders = new ComboBox();
        public string SelectedPath { get { return folders.SelectedItem as string; } }

        public LockedFolderDialog(IEnumerable<string> paths)
        {
            Text = "Choose a locked folder";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(540, 145);
            Font = new Font("Segoe UI", 9F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "Select the folder whose original permissions you want to restore:", AutoSize = true }, 0, 0);
            folders.Dock = DockStyle.Fill;
            folders.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (string path in paths) folders.Items.Add(path);
            if (folders.Items.Count > 0) folders.SelectedIndex = 0;
            layout.Controls.Add(folders, 0, 1);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var ok = new Button { Text = "Continue", Width = 95, Height = 30, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Cancel", Width = 85, Height = 30, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 0, 2);
            AcceptButton = ok;
            CancelButton = cancel;
            UiTheme.Apply(this);
        }
    }

    internal sealed class NetworkToolsDialog : Form
    {
        public int SelectedAction { get; private set; }

        public NetworkToolsDialog()
        {
            Text = "Network repair";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(510, 225);
            Font = new Font("Segoe UI", 9F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "Choose a network repair action. Reset Winsock may require a restart and can affect VPN or security software.", Dock = DockStyle.Fill, ForeColor = UiTheme.Muted }, 0, 0);
            var choices = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            AddChoice(choices, "Flush DNS", 1);
            AddChoice(choices, "Reset Winsock", 2);
            AddChoice(choices, "Do both", 3);
            layout.Controls.Add(choices, 0, 1);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var cancel = new Button { Text = "Cancel", Width = 90, Height = 30, DialogResult = DialogResult.Cancel };
            footer.Controls.Add(cancel);
            layout.Controls.Add(footer, 0, 2);
            CancelButton = cancel;
            UiTheme.Apply(this);
        }

        private void AddChoice(Control parent, string text, int action)
        {
            var button = new Button { Text = text, Width = 145, Height = 38, Tag = action == 1 ? "primary" : null, Margin = new Padding(0, 8, 8, 0) };
            button.Click += delegate { SelectedAction = action; DialogResult = DialogResult.OK; Close(); };
            parent.Controls.Add(button);
        }
    }

    internal sealed class DiskCheckDialog : Form
    {
        private readonly ComboBox drive = new ComboBox();
        private readonly CheckBox repair = new CheckBox();
        public string SelectedDrive { get { return drive.SelectedItem as string; } }
        public bool RepairErrors { get { return repair.Checked; } }

        public DiskCheckDialog(string[] drives)
        {
            Text = "Disk health check";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(460, 195);
            Font = new Font("Segoe UI", 9F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "Choose a drive to check:", AutoSize = true }, 0, 0);
            drive.DropDownStyle = ComboBoxStyle.DropDownList;
            drive.Dock = DockStyle.Left;
            drive.Width = 150;
            foreach (string item in drives) drive.Items.Add(item);
            if (drive.Items.Count > 0) drive.SelectedIndex = 0;
            layout.Controls.Add(drive, 0, 1);
            repair.Text = "Attempt to fix file-system errors (/F; may require restart)";
            repair.AutoSize = true;
            repair.Anchor = AnchorStyles.Left;
            layout.Controls.Add(repair, 0, 2);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 6, 0, 0) };
            var ok = new Button { Text = "Run check", Width = 100, Height = 30, DialogResult = DialogResult.OK, Tag = "primary" };
            var cancel = new Button { Text = "Cancel", Width = 85, Height = 30, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 0, 3);
            AcceptButton = ok;
            CancelButton = cancel;
            ok.Enabled = drives.Length > 0;
            UiTheme.Apply(this);
        }
    }

    internal sealed class CleanupForm : Form
    {
        private readonly CheckedListBox items = new CheckedListBox();
        public List<CleanupCandidate> SelectedCandidates
        {
            get { return items.CheckedItems.Cast<CleanupCandidate>().ToList(); }
        }

        public CleanupForm(AppEntry app, List<CleanupCandidate> candidates)
        {
            Text = "Review leftovers - " + app.Name;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(650, 450);
            Size = new Size(780, 540);
            BackColor = UiTheme.Background;
            Font = new Font("Segoe UI", 9F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 4, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            Controls.Add(layout);

            layout.Controls.Add(new Label { Text = "Review leftovers", Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), AutoSize = true, ForeColor = UiTheme.Text }, 0, 0);
            layout.Controls.Add(new Label { Text = "Only exact app-name matches are listed. Nothing is selected by default. App data can include settings or user files. Check only entries you recognize. Registry keys are exported before removal; folders go to the Recycle Bin.", AutoSize = false, Dock = DockStyle.Fill, ForeColor = UiTheme.Muted }, 0, 1);
            items.Dock = DockStyle.Fill;
            items.CheckOnClick = true;
            items.IntegralHeight = false;
            items.BorderStyle = BorderStyle.FixedSingle;
            items.Font = new Font("Segoe UI", 9F);
            foreach (var candidate in candidates) items.Items.Add(candidate, false);
            layout.Controls.Add(items, 0, 2);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 7, 0, 0) };
            var clean = new Button { Text = "Clean selected", Width = 135, Height = 32, DialogResult = DialogResult.None, Tag = "primary", FlatStyle = FlatStyle.Flat };
            clean.FlatAppearance.BorderSize = 0;
            clean.Click += delegate
            {
                if (items.CheckedItems.Count == 0) MessageBox.Show(this, "Select at least one leftover to clean.", "Nothing selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else DialogResult = DialogResult.OK;
            };
            var cancel = new Button { Text = "Cancel", Width = 90, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat };
            buttons.Controls.Add(clean);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 0, 3);
            AcceptButton = clean;
            CancelButton = cancel;
            UiTheme.Apply(this);
        }
    }

    internal sealed class RatingDialog : Form
    {
        private readonly NumericUpDown rating = new NumericUpDown();
        public int Rating { get { return Decimal.ToInt32(rating.Value); } }

        public RatingDialog()
        {
            Text = "Rate Winclean";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(390, 190);
            Font = new Font("Segoe UI", 9F);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "How would you rate Winclean?", AutoSize = true, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold) }, 0, 0);
            rating.Minimum = 1;
            rating.Maximum = 10;
            rating.Value = 10;
            rating.Width = 120;
            rating.Anchor = AnchorStyles.Left;
            layout.Controls.Add(rating, 0, 1);
            layout.Controls.Add(new Label { Text = "Your rating is recorded locally only; it is never sent.", AutoSize = true, ForeColor = UiTheme.Muted, Anchor = AnchorStyles.Left }, 0, 2);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 5, 0, 0) };
            var submit = new Button { Text = "Save rating", Width = 105, Height = 30, Tag = "primary", DialogResult = DialogResult.OK };
            var skip = new Button { Text = "Cancel", Width = 85, Height = 30, DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(submit);
            buttons.Controls.Add(skip);
            layout.Controls.Add(buttons, 0, 3);
            AcceptButton = submit;
            CancelButton = skip;
            UiTheme.Apply(this);
        }
    }

    internal static class AppScanner
    {
        private const string UninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private static readonly string[] CorePackages = new string[] {
            "Microsoft.WindowsStore", "Microsoft.DesktopAppInstaller", "Microsoft.WindowsTerminal",
            "Microsoft.SecHealthUI", "Microsoft.Windows.StartMenuExperienceHost",
            "Microsoft.Windows.ShellExperienceHost", "Microsoft.AAD.BrokerPlugin", "Microsoft.LockApp",
            "Microsoft.AccountsControl", "Microsoft.BioEnrollment", "Microsoft.CredDialogHost",
            "Microsoft.ECApp", "Microsoft.Windows.CloudExperienceHost", "Microsoft.UI.Xaml",
            "Microsoft.VCLibs", "Microsoft.Win32WebViewHost", "Microsoft.Windows.Apprep.ChxApp"
        };

        public static List<AppEntry> Load()
        {
            var apps = new List<AppEntry>();
            foreach (RegistryHive hive in new RegistryHive[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
            {
                foreach (RegistryView view in new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    try
                    {
                        using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
                        using (var root = baseKey.OpenSubKey(UninstallKey))
                        {
                            if (root == null) continue;
                            foreach (string subName in root.GetSubKeyNames())
                            {
                                try
                                {
                                    using (var key = root.OpenSubKey(subName))
                                    {
                                        if (key == null) continue;
                                        string name = Value(key, "DisplayName");
                                        if (String.IsNullOrWhiteSpace(name) || Dword(key, "SystemComponent") == 1 ||
                                            Dword(key, "NoRemove") == 1 ||
                                            !String.IsNullOrWhiteSpace(Value(key, "ParentKeyName")) ||
                                            String.Equals(Value(key, "ReleaseType"), "Update", StringComparison.OrdinalIgnoreCase)) continue;
                                        string uninstall = Value(key, "UninstallString");
                                        bool msi = Dword(key, "WindowsInstaller") == 1;
                                        if (String.IsNullOrWhiteSpace(uninstall) && msi && IsGuid(subName))
                                            uninstall = "msiexec.exe /x " + subName;
                                        string registryPath = UninstallKey + "\\" + subName;
                                        var entry = new AppEntry {
                                            Name = name.Trim(), Publisher = Value(key, "Publisher"), Version = Value(key, "DisplayVersion"),
                                            Source = hive == RegistryHive.CurrentUser ? "Desktop app (user)" : "Desktop app (device)",
                                            UninstallCommand = Environment.ExpandEnvironmentVariables(uninstall ?? ""), RegistryPath = registryPath,
                                            Hive = hive, View = view, InstallLocation = Environment.ExpandEnvironmentVariables(Value(key, "InstallLocation") ?? ""),
                                            IsProtected = IsProtectedDesktopComponent(name, Value(key, "Publisher"), Value(key, "ReleaseType"))
                                        };
                                        apps.Add(entry);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
            apps.AddRange(LoadStoreApps());
            return apps
                .GroupBy(a => a.Identity, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private static bool IsProtectedDesktopComponent(string name, string publisher, string releaseType)
        {
            if (!String.IsNullOrWhiteSpace(releaseType)) return true;
            if (String.IsNullOrWhiteSpace(name)) return false;
            string normalized = name.Trim();
            string[] protectedPrefixes = {
                "Update for ", "Security Update for ", "Hotfix for ", "Service Pack for ",
                "Microsoft Visual C++", "Microsoft .NET", ".NET Runtime", ".NET Desktop Runtime",
                "Microsoft Windows Desktop Runtime", "Microsoft Edge WebView2 Runtime", "Windows Driver Package"
            };
            if (protectedPrefixes.Any(prefix => normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))) return true;
            if (!String.IsNullOrWhiteSpace(publisher) && publisher.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >= 0
                && (normalized.IndexOf("Security Update", StringComparison.OrdinalIgnoreCase) >= 0
                    || normalized.IndexOf("Service Pack", StringComparison.OrdinalIgnoreCase) >= 0
                    || normalized.IndexOf("Hotfix", StringComparison.OrdinalIgnoreCase) >= 0)) return true;
            return false;
        }

        private static List<AppEntry> LoadStoreApps()
        {
            var apps = new List<AppEntry>();
            string script = "$ErrorActionPreference='Stop'; Get-AppxPackage | ForEach-Object { $v=$_.Version.ToString(); $n=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_.Name)); $f=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_.PackageFullName)); $p=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_.Publisher)); $l=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_.InstallLocation)); $family=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_.PackageFamilyName)); $fw=[int]$_.IsFramework; $nr=[int]$_.NonRemovable; Write-Output ($n+'|'+$f+'|'+$p+'|'+$v+'|'+$l+'|'+$family+'|'+$fw+'|'+$nr) }";
            try
            {
                string output = RunPowerShell(script, true);
                foreach (string line in output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] p = line.Trim().Split('|');
                    if (p.Length != 8) continue;
                    string name = Decode(p[0]);
                    if (String.IsNullOrWhiteSpace(name) || p[6] == "1") continue;
                    var entry = new AppEntry { Name = name, PackageFullName = Decode(p[1]), Publisher = Decode(p[2]), Version = p[3],
                        InstallLocation = Decode(p[4]), PackageFamilyName = Decode(p[5]), Source = "Store app", IsProtected = p[7] == "1" || CorePackages.Contains(name, StringComparer.OrdinalIgnoreCase) };
                    apps.Add(entry);
                }
            }
            catch { }
            return apps;
        }

        public static int Uninstall(AppEntry app)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (app.IsProtected) throw new InvalidOperationException("This Windows component is protected from removal by Winclean.");
            if (app.Source == "Store app")
            {
                string package = (app.PackageFullName ?? "").Replace("'", "''");
                string script = "$ErrorActionPreference='Stop'; Remove-AppxPackage -Package '" + package + "' -ErrorAction Stop";
                RunPowerShell("try { " + script + "; exit 0 } catch { [Console]::Error.WriteLine($_); exit 1 }", false);
                return 0;
            }

            List<string> values = PrepareUninstallCommand(app.UninstallCommand);
            string workingDirectory = Path.GetDirectoryName(values[0]);
            var info = new ProcessStartInfo { FileName = values[0], Arguments = String.Join(" ", values.Skip(1).Select(QuoteArgument).ToArray()), UseShellExecute = true, WorkingDirectory = String.IsNullOrWhiteSpace(workingDirectory) ? Environment.CurrentDirectory : workingDirectory };
            using (var process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException("Windows did not start the registered uninstaller.");
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        public static string GetUninstallPreview(AppEntry app)
        {
            if (app == null) return "Not supported by Winclean: no app was selected.";
            if (app.IsProtected) return "Not supported by Winclean: this Windows component is protected from removal.";
            try
            {
                var values = PrepareUninstallCommand(app.UninstallCommand);
                return String.Join(" ", values.Select(QuoteArgument).ToArray());
            }
            catch (Exception ex) { return "Not supported by Winclean: " + ex.Message; }
        }

        private static List<string> PrepareUninstallCommand(string original)
        {
            List<string> values = RemoveSilentSwitches(ParseUninstallCommand(original));
            ValidateUninstaller(values);
            if (String.Equals(Path.GetFileName(values[0]), "msiexec.exe", StringComparison.OrdinalIgnoreCase))
            {
                Guid productCode = Guid.Empty;
                bool found = values.Skip(1).Any(value => Guid.TryParse(value, out productCode));
                if (!found) throw new InvalidOperationException("The registered Windows Installer command does not contain a valid product code.");
                return new List<string> {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "msiexec.exe"),
                    "/x", productCode.ToString("B")
                };
            }
            return values;
        }

        private static List<string> ParseUninstallCommand(string original)
        {
            string command = Environment.ExpandEnvironmentVariables(original ?? "");
            int brace = command.IndexOf('{');
            if (command.IndexOf("msiexec", StringComparison.OrdinalIgnoreCase) >= 0 && brace >= 0)
            {
                int end = command.IndexOf('}', brace);
                if (end > brace) command = "msiexec.exe /x " + command.Substring(brace, end - brace + 1);
            }
            if (String.IsNullOrWhiteSpace(command)) throw new InvalidOperationException("No uninstall command was registered.");
            int argc;
            IntPtr argv = CommandLineToArgvW(command, out argc);
            if (argv == IntPtr.Zero || argc < 1) throw new InvalidOperationException("The registered uninstall command could not be parsed.");
            try
            {
                var values = new List<string>();
                for (int i = 0; i < argc; i++) values.Add(Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size)));
                return values;
            }
            finally { LocalFree(argv); }
        }

        private static List<string> RemoveSilentSwitches(List<string> values)
        {
            var silent = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "/s", "-s", "/silent", "-silent", "/verysilent", "-verysilent", "/suppressmsgboxes", "-suppressmsgboxes", "/qn", "/quiet", "-quiet", "--quiet", "/passive", "-passive", "/norestart", "-norestart" };
            var result = new List<string>();
            for (int i = 0; i < values.Count; i++)
            {
                if (i == 0 || !silent.Contains(values[i])) { result.Add(values[i]); continue; }
                if (i + 1 < values.Count && String.Equals(values[i], "/quiet", StringComparison.OrdinalIgnoreCase) && String.Equals(values[i + 1], "/uninstall", StringComparison.OrdinalIgnoreCase))
                { result.Add(values[++i]); }
                else if (i + 1 < values.Count && String.Equals(values[i], "/s", StringComparison.OrdinalIgnoreCase) && values[i + 1].StartsWith("-", StringComparison.Ordinal)) { result.Add(values[++i]); }
            }
            return result;
        }

        private static void ValidateUninstaller(List<string> values)
        {
            if (values == null || values.Count == 0) throw new InvalidOperationException("No uninstaller command was registered.");
            string executable = Path.GetFileName(values[0]).ToLowerInvariant();
            string[] blocked = { "cmd.exe", "command.com", "powershell.exe", "pwsh.exe", "wscript.exe", "cscript.exe", "mshta.exe", "rundll32.exe", "regsvr32.exe" };
            if (blocked.Contains(executable)) throw new InvalidOperationException("This app registered a script or command interpreter as its uninstaller. For safety, use Windows Settings > Apps to uninstall it.");
            if (!executable.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Winclean only starts registered .exe uninstallers. Use Windows Settings > Apps for this app.");
            if (executable != "msiexec.exe" && !Path.IsPathRooted(values[0])) throw new InvalidOperationException("Winclean only starts an uninstaller executable with an explicit path. Use Windows Settings > Apps for this app.");
            if (executable != "msiexec.exe" && !File.Exists(values[0])) throw new InvalidOperationException("The registered uninstaller file could not be found: " + values[0]);
        }

        public static List<CleanupCandidate> FindLeftovers(AppEntry app)
        {
            if (app == null) throw new ArgumentNullException("app");
            var candidates = new List<CleanupCandidate>();
            var roots = new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) };
            if (!String.IsNullOrWhiteSpace(app.PackageFamilyName))
                AddDirectory(candidates, "Store app data", Path.Combine(roots[0], "Packages", app.PackageFamilyName), roots[0]);

            string appFolder = SafeFolderName(app.Name);
            if (!String.IsNullOrEmpty(appFolder))
                foreach (string root in roots)
                    AddDirectory(candidates, "App data / settings", Path.Combine(root, appFolder), root);

            string installRoot = String.IsNullOrWhiteSpace(app.InstallLocation) ? null : GetInstallRoot(Environment.ExpandEnvironmentVariables(app.InstallLocation));
            if (!String.IsNullOrWhiteSpace(app.InstallLocation) && installRoot != null && IsSafeInstallLocation(app.InstallLocation, app.Name))
                AddDirectory(candidates, "Application install folder", app.InstallLocation, installRoot);

            if (!String.IsNullOrWhiteSpace(app.RegistryPath) && RegistryPathExists(app.Hive, app.View, app.RegistryPath))
                candidates.Add(new CleanupCandidate { Label = "Uninstall registration", Path = HiveName(app.Hive) + "\\" + DisplayRegistryPath(app.RegistryPath), IsRegistry = true, Hive = app.Hive, View = app.View, RegistryPath = app.RegistryPath });

            string leaf = SanitizeRegistryName(app.Name);
            if (leaf.Length >= 3)
            {
                foreach (RegistryHive hive in new RegistryHive[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
                foreach (RegistryView view in new RegistryView[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    string path = @"SOFTWARE\" + leaf;
                    if (RegistryPathExists(hive, view, path) && !candidates.Any(c => c.IsRegistry && c.Hive == hive && c.View == view && String.Equals(c.RegistryPath, path, StringComparison.OrdinalIgnoreCase)))
                        candidates.Add(new CleanupCandidate { Label = "App settings registry key", Path = HiveName(hive) + "\\" + DisplayRegistryPath(path), IsRegistry = true, Hive = hive, View = view, RegistryPath = path });
                }
            }
            return candidates.GroupBy(c => (c.IsRegistry ? "R|" + c.Hive + "|" + c.View + "|" + c.RegistryPath : "D|" + c.Path), StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        }

        private static void AddDirectory(List<CleanupCandidate> list, string label, string path, string root)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(path) || !Directory.Exists(path) || !IsWithin(path, root) || IsRoot(path, root) || HasReparsePoint(path)) return;
                list.Add(new CleanupCandidate { Label = label, Path = Path.GetFullPath(path), IsRegistry = false });
            }
            catch { }
        }

        public static List<string> Clean(List<CleanupCandidate> candidates)
        {
            var results = new List<string>();
            string backupRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Winclean", "Backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            foreach (var candidate in candidates)
            {
                try
                {
                    if (candidate.IsRegistry)
                    {
                        Directory.CreateDirectory(backupRoot);
                        string backup = Path.Combine(backupRoot, SafeFileName(candidate.Label + "-" + candidate.Hive + "-" + candidate.RegistryPath + "-" + candidate.View) + "-" + Guid.NewGuid().ToString("N") + ".reg");
                        if (!ExportRegistry(candidate, backup))
                        {
                            results.Add("Skipped registry key (backup failed): " + candidate.Path);
                            continue;
                        }
                        string regPath = HiveName(candidate.Hive) + "\\" + DisplayRegistryPath(candidate.RegistryPath);
                        string deleteArgs = "delete " + QuoteArgument(regPath) + " /f /reg:" + (candidate.View == RegistryView.Registry32 ? "32" : "64");
                        var deleteInfo = new ProcessStartInfo { FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "reg.exe"), Arguments = deleteArgs, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
                        if (candidate.Hive == RegistryHive.LocalMachine) deleteInfo.Verb = "runas";
                        using (var process = Process.Start(deleteInfo))
                        {
                            if (process == null) { results.Add("Could not start registry cleanup: " + candidate.Path); continue; }
                            process.WaitForExit();
                            if (process.ExitCode != 0) { results.Add("Registry key was not removed (exit " + process.ExitCode + "): " + candidate.Path); continue; }
                        }
                        results.Add("Removed registry key (backup saved): " + candidate.Path);
                    }
                    else
                    {
                        if (!Directory.Exists(candidate.Path)) { results.Add("Already gone: " + candidate.Path); continue; }
                        if (HasReparsePoint(candidate.Path)) { results.Add("Skipped folder containing a junction/symlink: " + candidate.Path); continue; }
                        FileSystem.DeleteDirectory(candidate.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin, UICancelOption.ThrowException);
                        results.Add("Sent folder to Recycle Bin: " + candidate.Path);
                    }
                }
                catch (Exception ex) { results.Add("Could not clean " + candidate.Path + " - " + ex.Message); }
            }
            if (results.Count == 0) results.Add("Nothing selected; no changes made.");
            return results;
        }

        private static bool ExportRegistry(CleanupCandidate candidate, string output)
        {
            string regPath = HiveName(candidate.Hive) + "\\" + DisplayRegistryPath(candidate.RegistryPath);
            var info = new ProcessStartInfo { FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "reg.exe"), Arguments = "export " + QuoteArgument(regPath) + " " + QuoteArgument(output) + " /y /reg:" + (candidate.View == RegistryView.Registry32 ? "32" : "64"), UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden };
            if (candidate.Hive == RegistryHive.LocalMachine) info.Verb = "runas";
            using (var process = Process.Start(info)) { if (process == null) return false; process.WaitForExit(); return process.ExitCode == 0 && File.Exists(output); }
        }

        private static bool RegistryPathExists(RegistryHive hive, RegistryView view, string path)
        {
            try { using (var root = RegistryKey.OpenBaseKey(hive, view)) using (var key = root.OpenSubKey(path)) return key != null; }
            catch { return false; }
        }

        private static bool IsSafeInstallLocation(string location, string appName)
        {
            try
            {
                string full = Path.GetFullPath(Environment.ExpandEnvironmentVariables(location)).TrimEnd(Path.DirectorySeparatorChar);
                string leaf = Path.GetFileName(full);
                if (Normalize(leaf) != Normalize(appName)) return false;
                return GetInstallRoot(full) != null;
            }
            catch { return false; }
        }

        private static string GetInstallRoot(string path)
        {
            string full = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path)).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string[] allowed = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) };
            return allowed.FirstOrDefault(r => !String.IsNullOrWhiteSpace(r) && IsWithin(full, r));
        }

        private static bool IsWithin(string path, string root)
        {
            if (String.IsNullOrWhiteSpace(path) || String.IsNullOrWhiteSpace(root)) return false;
            string p = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string r = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return p.StartsWith(r, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRoot(string path, string root)
        {
            return String.Equals(Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasReparsePoint(string path)
        {
            try
            {
                var pending = new Stack<string>();
                pending.Push(path);
                while (pending.Count > 0)
                {
                    var dir = new DirectoryInfo(pending.Pop());
                    if ((dir.Attributes & FileAttributes.ReparsePoint) != 0) return true;
                    foreach (var child in dir.GetFileSystemInfos())
                    {
                        if ((child.Attributes & FileAttributes.ReparsePoint) != 0) return true;
                        if ((child.Attributes & FileAttributes.Directory) != 0) pending.Push(child.FullName);
                    }
                }
            }
            catch { return true; }
            return false;
        }

        private static string SafeFolderName(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return null;
            string name = value.Trim();
            if (name == "." || name == ".." || name.IndexOf('\\') >= 0 || name.IndexOf('/') >= 0 || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return null;
            return name;
        }

        private static string SanitizeRegistryName(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return "";
            string name = value.Trim().TrimEnd('.');
            if (name.IndexOf('\\') >= 0 || name.IndexOf('/') >= 0 || name == "." || name == "..") return "";
            return name;
        }

        private static string Normalize(string value)
        {
            if (String.IsNullOrWhiteSpace(value)) return "";
            var chars = value.Where(Char.IsLetterOrDigit).Select(Char.ToLowerInvariant).ToArray();
            return new String(chars);
        }

        private static string SafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            string clean = new String(value.Select(c => invalid.Contains(c) || c == '\\' || c == '/' || c == ':' ? '_' : c).ToArray());
            return clean.Length > 100 ? clean.Substring(0, 100) : clean;
        }

        private static bool IsGuid(string value)
        {
            Guid ignored;
            return Guid.TryParse(value, out ignored);
        }

        private static string Value(RegistryKey key, string name)
        {
            try { object o = key.GetValue(name); return o == null ? null : Convert.ToString(o); }
            catch { return null; }
        }

        private static int Dword(RegistryKey key, string name)
        {
            try { return Convert.ToInt32(key.GetValue(name, 0)); }
            catch { return 0; }
        }

        private static string Decode(string text)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(text)); }
            catch { return ""; }
        }

        private static string HiveName(RegistryHive hive)
        {
            return hive == RegistryHive.CurrentUser ? "HKEY_CURRENT_USER" : "HKEY_LOCAL_MACHINE";
        }

        private static string DisplayRegistryPath(string path)
        {
            if (path.StartsWith("SOFTWARE\\", StringComparison.OrdinalIgnoreCase)) return path;
            return path;
        }

        public static void RunPowerShellElevated(string script)
        {
            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            string ps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
            var info = new ProcessStartInfo { FileName = ps, Arguments = "-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand " + encoded, UseShellExecute = true, Verb = "runas", WindowStyle = ProcessWindowStyle.Normal };
            using (var process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException("Windows PowerShell could not be started.");
                process.WaitForExit();
                if (process.ExitCode != 0) throw new InvalidOperationException("The elevated operation returned exit code " + process.ExitCode + ".");
            }
        }

        public static string RunPowerShell(string script, bool capture)
        {
            string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            string ps = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
            var info = new ProcessStartInfo { FileName = ps, Arguments = "-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand " + encoded, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = capture, RedirectStandardError = capture };
            using (var process = Process.Start(info))
            {
                if (process == null) throw new InvalidOperationException("Windows PowerShell could not be started.");
                string stdout = capture ? process.StandardOutput.ReadToEnd() : "";
                if (capture) process.WaitForExit();
                else process.WaitForExit();
                if (process.ExitCode != 0) throw new InvalidOperationException(capture ? process.StandardError.ReadToEnd() : "PowerShell returned exit code " + process.ExitCode + ".");
                return stdout;
            }
        }
        private static string QuoteArgument(string value)
        {
            if (value == null) return "\"\"";
            if (value.Length > 0 && value.All(c => !Char.IsWhiteSpace(c) && c != '"')) return value;
            var b = new StringBuilder("\"");
            int slashes = 0;
            foreach (char c in value)
            {
                if (c == 92) { slashes++; continue; }
                if (c == '"') { b.Append((char)92, slashes * 2 + 1); b.Append('"'); slashes = 0; continue; }
                b.Append((char)92, slashes); slashes = 0; b.Append(c);
            }
            b.Append((char)92, slashes * 2);
            b.Append('"');
            return b.ToString();
        }

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string commandLine, out int argc);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);
    }

    internal static class FolderLockManager
    {
        private sealed class LockRecord
        {
            public string Path;
            public byte[] Salt;
            public byte[] PasswordHash;
            public string BackupFile;
        }

        private static readonly string DataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Winclean", "Locks");
        private static readonly string RecordFile = Path.Combine(DataDirectory, "folders.dat");

        public static string[] GetLockedFolders()
        {
            return LoadRecords().Select(record => record.Path).OrderBy(path => path, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        public static void Lock(string path, string password)
        {
            string full = ValidateFolder(path);
            if (String.IsNullOrWhiteSpace(password) || password.Length < 8) throw new InvalidOperationException("Use a password of at least 8 characters.");
            List<LockRecord> records = LoadRecords();
            if (records.Any(record => String.Equals(record.Path, full, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Winclean already manages a lock for this folder.");

            DirectorySecurity security = new DirectoryInfo(full).GetAccessControl(AccessControlSections.Access);
            byte[] originalDescriptor = security.GetSecurityDescriptorBinaryForm();
            byte[] salt = new byte[16];
            using (var random = new RNGCryptoServiceProvider()) random.GetBytes(salt);
            byte[] hash;
            using (var derive = new Rfc2898DeriveBytes(password, salt, 120000)) hash = derive.GetBytes(32);
            string backupName = Guid.NewGuid().ToString("N") + ".acl";
            Directory.CreateDirectory(DataDirectory);
            string backupPath = Path.Combine(DataDirectory, backupName);
            File.WriteAllBytes(backupPath, originalDescriptor);

            try
            {
                security.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None, AccessControlType.Deny));
                new DirectoryInfo(full).SetAccessControl(security);
                records.Add(new LockRecord { Path = full, Salt = salt, PasswordHash = hash, BackupFile = backupName });
                SaveRecords(records);
            }
            catch
            {
                RestoreAccess(full, originalDescriptor);
                try { File.Delete(backupPath); } catch { }
                throw;
            }
        }

        public static void Unlock(string path, string password)
        {
            string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            List<LockRecord> records = LoadRecords();
            LockRecord record = records.FirstOrDefault(item => String.Equals(item.Path, full, StringComparison.OrdinalIgnoreCase));
            if (record == null) throw new InvalidOperationException("This folder is not listed as locked by Winclean for this account.");
            byte[] actual;
            using (var derive = new Rfc2898DeriveBytes(password ?? "", record.Salt, 120000)) actual = derive.GetBytes(record.PasswordHash.Length);
            if (!ConstantTimeEquals(actual, record.PasswordHash)) throw new InvalidOperationException("The password is incorrect.");
            Guid backupId;
            if (record.BackupFile == null || !record.BackupFile.EndsWith(".acl", StringComparison.OrdinalIgnoreCase)
                || !Guid.TryParseExact(record.BackupFile.Substring(0, record.BackupFile.Length - 4), "N", out backupId))
                throw new InvalidOperationException("The saved permissions backup entry is invalid; Winclean will not alter this folder's access rules.");
            string backupPath = Path.Combine(DataDirectory, record.BackupFile);
            if (!File.Exists(backupPath)) throw new InvalidOperationException("The saved original permissions backup is missing; Winclean will not alter this folder's access rules.");
            RestoreAccess(full, File.ReadAllBytes(backupPath));
            records.Remove(record);
            SaveRecords(records);
            try { File.Delete(backupPath); } catch { }
        }

        private static string ValidateFolder(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) throw new InvalidOperationException("Choose an existing folder.");
            string full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            if (String.IsNullOrEmpty(full) || String.Equals(full, Path.GetPathRoot(full).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Drive roots cannot be locked.");
            var info = new DirectoryInfo(full);
            if ((info.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("Junctions and symbolic links cannot be locked.");
            for (DirectoryInfo parent = info; parent != null; parent = parent.Parent)
                if ((parent.Attributes & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("A parent folder is a junction or symbolic link; choose a direct path instead.");

            string profileRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (String.Equals(full, Path.GetFullPath(profileRoot).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The Windows profile root is protected; choose a folder inside it instead.");
            string[] protectedRoots = new string[] {
                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                DataDirectory
            };
            foreach (string root in protectedRoots)
                if (!String.IsNullOrWhiteSpace(root) && IsSameOrWithin(full, root))
                    throw new InvalidOperationException("Windows, application data, program files and Winclean data folders are protected.");
            return full;
        }

        private static bool IsSameOrWithin(string path, string root)
        {
            string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar);
            return String.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase)
                || normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static void RestoreAccess(string path, byte[] descriptor)
        {
            var security = new DirectorySecurity();
            security.SetSecurityDescriptorBinaryForm(descriptor, AccessControlSections.Access);
            new DirectoryInfo(path).SetAccessControl(security);
        }

        private static List<LockRecord> LoadRecords()
        {
            var records = new List<LockRecord>();
            if (!File.Exists(RecordFile)) return records;
            foreach (string line in File.ReadAllLines(RecordFile, Encoding.UTF8))
            {
                string[] fields = line.Split('|');
                if (fields.Length != 4) continue;
                try
                {
                    Guid backupId;
                    string backupFile = fields[3];
                    if (!backupFile.EndsWith(".acl", StringComparison.OrdinalIgnoreCase)
                        || !Guid.TryParseExact(backupFile.Substring(0, backupFile.Length - 4), "N", out backupId)) continue;
                    records.Add(new LockRecord {
                        Path = Encoding.UTF8.GetString(Convert.FromBase64String(fields[0])),
                        Salt = Convert.FromBase64String(fields[1]),
                        PasswordHash = Convert.FromBase64String(fields[2]),
                        BackupFile = backupFile
                    });
                }
                catch { }
            }
            return records;
        }

        private static void SaveRecords(List<LockRecord> records)
        {
            Directory.CreateDirectory(DataDirectory);
            string temp = RecordFile + ".tmp";
            File.WriteAllLines(temp, records.Select(record =>
                Convert.ToBase64String(Encoding.UTF8.GetBytes(record.Path)) + "|" +
                Convert.ToBase64String(record.Salt) + "|" +
                Convert.ToBase64String(record.PasswordHash) + "|" + record.BackupFile).ToArray(), Encoding.UTF8);
            if (File.Exists(RecordFile)) File.Replace(temp, RecordFile, null);
            else File.Move(temp, RecordFile);
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e) { LogCrash(e.Exception); MessageBox.Show("Winclean hit an unexpected error. Details were saved under %LOCALAPPDATA%\\Winclean\\Logs.\r\n\r\n" + e.Exception.Message, "Winclean error", MessageBoxButtons.OK, MessageBoxIcon.Error); };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { LogCrash(e.ExceptionObject as Exception); };
            try { Application.Run(new MainForm()); }
            catch (Exception ex) { LogCrash(ex); MessageBox.Show("Winclean could not start. Details were saved under %LOCALAPPDATA%\\Winclean\\Logs.\r\n\r\n" + ex.ToString(), "Winclean startup error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static void LogCrash(Exception exception)
        {
            try
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Winclean", "Logs");
                Directory.CreateDirectory(folder);
                File.AppendAllText(Path.Combine(folder, "app-error.log"), DateTime.Now.ToString("u") + Environment.NewLine + (exception == null ? "Unknown exception" : exception.ToString()) + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }
    }
}
