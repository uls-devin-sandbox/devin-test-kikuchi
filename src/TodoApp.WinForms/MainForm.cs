using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TodoApp.Core;

namespace TodoApp.WinForms
{
    /// <summary>
    /// ToDo 管理画面です。
    /// </summary>
    public class MainForm : Form
    {
        private readonly TaskService _service;

        // 入力コントロール
        private TextBox _txtTitle;
        private DateTimePicker _dtpDueDate;
        private ComboBox _cmbPriority;
        private Button _btnAdd;
        private Button _btnCancel;
        private ComboBox _cmbFilter;

        // 一覧
        private DataGridView _dgvTasks;

        // 操作ボタン
        private Button _btnEdit;
        private Button _btnDelete;
        private Button _btnToggle;

        // 編集対象
        private TaskItem _editingTask;

        public MainForm(TaskService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            _service = service;
            InitializeComponent();
            ResetInput();
            RefreshData();
        }

        private void InitializeComponent()
        {
            Text = "ToDo 管理画面";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Padding = new Padding(12);

            // 入力エリア
            var lblTitle = new Label { Text = "タイトル:", Location = new Point(12, 15), AutoSize = true };
            _txtTitle = new TextBox { Location = new Point(80, 12), Size = new Size(200, 20), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, AccessibleName = "TitleTextBox" };

            var lblDueDate = new Label { Text = "期限:", Location = new Point(300, 15), AutoSize = true };
            _dtpDueDate = new DateTimePicker { Location = new Point(350, 12), Size = new Size(120, 20), Format = DateTimePickerFormat.Short, Anchor = AnchorStyles.Top | AnchorStyles.Right, AccessibleName = "DueDatePicker" };

            var lblPriority = new Label { Text = "優先度:", Location = new Point(490, 15), AutoSize = true };
            _cmbPriority = new ComboBox { Location = new Point(550, 12), Size = new Size(80, 20), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Right, AccessibleName = "PriorityComboBox" };
            _cmbPriority.Items.AddRange(new object[] { "高", "中", "低" });

            _btnAdd = new Button { Text = "追加", Location = new Point(640, 10), Size = new Size(60, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right, AccessibleName = "AddButton" };
            _btnAdd.Click += BtnAdd_Click;

            _btnCancel = new Button { Text = "キャンセル", Location = new Point(710, 10), Size = new Size(60, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right, AccessibleName = "CancelButton" };
            _btnCancel.Click += BtnCancel_Click;

            // フィルタ
            var lblFilter = new Label { Text = "フィルタ:", Location = new Point(12, 50), AutoSize = true };
            _cmbFilter = new ComboBox { Location = new Point(80, 47), Size = new Size(100, 20), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left, AccessibleName = "FilterComboBox" };
            _cmbFilter.Items.AddRange(new object[] { "全て", "未完了", "完了" });
            _cmbFilter.SelectedIndex = 0;
            _cmbFilter.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;

            // 一覧
            _dgvTasks = new DataGridView
            {
                Location = new Point(12, 80),
                Size = new Size(760, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                AccessibleName = "TasksGridView"
            };

            var colId = new DataGridViewTextBoxColumn { DataPropertyName = "Id", Name = "colId", Visible = false };
            var colTitle = new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "タイトル", Name = "colTitle", Width = 240 };
            var colDueDate = new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "期限", Name = "colDueDate", Width = 100 };
            var colPriority = new DataGridViewTextBoxColumn { DataPropertyName = "Priority", HeaderText = "優先度", Name = "colPriority", Width = 80 };
            var colIsCompleted = new DataGridViewCheckBoxColumn { DataPropertyName = "IsCompleted", HeaderText = "完了", Name = "colIsCompleted", Width = 50 };
            var colStatus = new DataGridViewTextBoxColumn { HeaderText = "ステータス", Name = "colStatus", Width = 80, ReadOnly = true };

            _dgvTasks.Columns.AddRange(colId, colTitle, colDueDate, colPriority, colIsCompleted, colStatus);
            _dgvTasks.CellFormatting += DgvTasks_CellFormatting;

            // 操作ボタン
            _btnEdit = new Button { Text = "編集", Location = new Point(12, 490), Size = new Size(80, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Left, AccessibleName = "EditButton" };
            _btnEdit.Click += BtnEdit_Click;

            _btnDelete = new Button { Text = "削除", Location = new Point(100, 490), Size = new Size(80, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Left, AccessibleName = "DeleteButton" };
            _btnDelete.Click += BtnDelete_Click;

            _btnToggle = new Button { Text = "完了/未完了", Location = new Point(190, 490), Size = new Size(100, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Left, AccessibleName = "ToggleButton" };
            _btnToggle.Click += BtnToggle_Click;

            Controls.Add(lblTitle);
            Controls.Add(_txtTitle);
            Controls.Add(lblDueDate);
            Controls.Add(_dtpDueDate);
            Controls.Add(lblPriority);
            Controls.Add(_cmbPriority);
            Controls.Add(_btnAdd);
            Controls.Add(_btnCancel);
            Controls.Add(lblFilter);
            Controls.Add(_cmbFilter);
            Controls.Add(_dgvTasks);
            Controls.Add(_btnEdit);
            Controls.Add(_btnDelete);
            Controls.Add(_btnToggle);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var title = _txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("タイトルを入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dueDate = _dtpDueDate.Value;
            var priority = ParsePriority(_cmbPriority.Text);

            if (_editingTask == null)
            {
                _service.Add(title, dueDate, priority);
            }
            else
            {
                _service.Update(_editingTask.Id, title, dueDate, priority);
            }

            ResetInput();
            RefreshData();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            ResetInput();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_dgvTasks.CurrentRow == null)
            {
                MessageBox.Show("編集するタスクを選択してください。", "選択エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _editingTask = (TaskItem)_dgvTasks.CurrentRow.DataBoundItem;
            _txtTitle.Text = _editingTask.Title;
            _dtpDueDate.Value = _editingTask.DueDate == DateTime.MinValue ? DateTime.Today : _editingTask.DueDate;
            _cmbPriority.Text = FormatPriority(_editingTask.Priority);
            _btnAdd.Text = "保存";
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_dgvTasks.CurrentRow == null)
            {
                MessageBox.Show("削除するタスクを選択してください。", "選択エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = (TaskItem)_dgvTasks.CurrentRow.DataBoundItem;
            var result = MessageBox.Show("「" + task.Title + "」を削除しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            _service.Remove(task.Id);
            ResetInput();
            RefreshData();
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            if (_dgvTasks.CurrentRow == null)
            {
                MessageBox.Show("切り替えるタスクを選択してください。", "選択エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = (TaskItem)_dgvTasks.CurrentRow.DataBoundItem;
            _service.ToggleComplete(task.Id);
            RefreshData();
        }

        private void CmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void DgvTasks_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null)
            {
                return;
            }

            var columnName = _dgvTasks.Columns[e.ColumnIndex].Name;
            if (columnName == "colPriority" && e.Value is Priority)
            {
                var priority = (Priority)e.Value;
                e.Value = FormatPriority(priority);
                e.FormattingApplied = true;
            }
            else if (columnName == "colDueDate" && e.Value is DateTime)
            {
                var dueDate = (DateTime)e.Value;
                e.Value = dueDate == DateTime.MinValue ? string.Empty : dueDate.ToString("yyyy/MM/dd");
                e.FormattingApplied = true;
            }
            else if (columnName == "colStatus")
            {
                var task = (TaskItem)_dgvTasks.Rows[e.RowIndex].DataBoundItem;
                if (task != null)
                {
                    e.Value = task.IsCompleted ? "完了" : "未完了";
                    e.FormattingApplied = true;
                }
            }
        }

        private void RefreshData()
        {
            bool? filter = GetFilter();
            var tasks = _service.GetFiltered(filter).ToList();
            _dgvTasks.DataSource = new BindingList<TaskItem>(tasks);
        }

        private void ResetInput()
        {
            _editingTask = null;
            _txtTitle.Clear();
            _dtpDueDate.Value = DateTime.Today;
            _cmbPriority.SelectedIndex = 1;
            _btnAdd.Text = "追加";
        }

        private bool? GetFilter()
        {
            switch (_cmbFilter.SelectedIndex)
            {
                case 1:
                    return false;
                case 2:
                    return true;
                default:
                    return null;
            }
        }

        private static string FormatPriority(Priority priority)
        {
            switch (priority)
            {
                case Priority.High:
                    return "高";
                case Priority.Low:
                    return "低";
                default:
                    return "中";
            }
        }

        private static Priority ParsePriority(string text)
        {
            switch (text)
            {
                case "高":
                    return Priority.High;
                case "低":
                    return Priority.Low;
                default:
                    return Priority.Medium;
            }
        }
    }
}
