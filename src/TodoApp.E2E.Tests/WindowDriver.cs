using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;

namespace TodoApp.E2E.Tests
{
    /// <summary>
    /// ToDo 管理画面の自動操作を行うドライバーです。
    /// </summary>
    public class WindowDriver : IDisposable
    {
        private readonly Application _application;
        private readonly UIA3Automation _automation;
        private readonly Window _mainWindow;

        /// <summary>
        /// 実行ファイルのパスを指定してアプリケーションを起動します。
        /// </summary>
        public WindowDriver(string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath))
            {
                throw new ArgumentException("exePath");
            }

            var workingDirectory = Path.GetDirectoryName(exePath);
            _automation = new UIA3Automation();
            _application = Application.Launch(new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false
            });

            var mainWindowResult = Retry.WhileNull(
                () => _application.GetMainWindow(_automation),
                TimeSpan.FromSeconds(10));
            _mainWindow = mainWindowResult.Result;

            if (_mainWindow == null)
            {
                throw new InvalidOperationException("メインウィンドウが見つかりません。");
            }
        }

        /// <summary>
        /// テストデータ JSON ファイルのパスを取得します。
        /// </summary>
        public string TasksJsonPath
        {
            get
            {
                var process = Process.GetProcessById(_application.ProcessId);
                var fileName = process.MainModule.FileName;
                var workingDirectory = Path.GetDirectoryName(fileName);
                return Path.Combine(workingDirectory, "tasks.json");
            }
        }

        public void EnterTitle(string title)
        {
            var textBox = WaitForByName("TitleTextBox").AsTextBox();
            textBox.Text = title;
        }

        public void SetDueDate(DateTime date)
        {
            var picker = WaitForByName("DueDatePicker").AsDateTimePicker();
            try
            {
                picker.SelectedDate = date;
            }
            catch (Exception)
            {
                picker.Focus();
                Wait.UntilInputIsProcessed();
                Keyboard.Type(date.ToString("yyyy/MM/dd"));
                Keyboard.Type(VirtualKeyShort.RETURN);
                Wait.UntilInputIsProcessed();
            }
        }

        public void SelectPriority(string priority)
        {
            int index;
            switch (priority)
            {
                case "高":
                    index = 0;
                    break;
                case "中":
                    index = 1;
                    break;
                case "低":
                    index = 2;
                    break;
                default:
                    index = 1;
                    break;
            }
            SelectComboBoxItem("PriorityComboBox", index);
        }

        public void ClickAdd()
        {
            WaitForByName("AddButton").AsButton().Click();
        }

        public void ClickEdit()
        {
            WaitForByName("EditButton").AsButton().Click();
        }

        public void ClickDelete()
        {
            WaitForByName("DeleteButton").AsButton().Click();
        }

        public void ClickToggle()
        {
            WaitForByName("ToggleButton").AsButton().Click();
        }

        public void SelectFilter(string filter)
        {
            int index;
            switch (filter)
            {
                case "全て":
                    index = 0;
                    break;
                case "未完了":
                    index = 1;
                    break;
                case "完了":
                    index = 2;
                    break;
                default:
                    index = 0;
                    break;
            }
            SelectComboBoxItem("FilterComboBox", index);
        }

        private void SelectComboBoxItem(string comboBoxName, int index)
        {
            var items = new[] { string.Empty, string.Empty, string.Empty };
            if (comboBoxName == "PriorityComboBox")
            {
                items = new[] { "高", "中", "低" };
            }
            else if (comboBoxName == "FilterComboBox")
            {
                items = new[] { "全て", "未完了", "完了" };
            }

            var itemName = items[index];
            var comboBox = WaitForByName(comboBoxName);
            _mainWindow.SetForeground();
            comboBox.Focus();
            Wait.UntilInputIsProcessed();
            Keyboard.TypeSimultaneously(VirtualKeyShort.ALT, VirtualKeyShort.DOWN);
            Wait.UntilInputIsProcessed();

            var desktop = _automation.GetDesktop();
            var list = Retry.WhileNull(
                () => desktop.FindFirstDescendant(_automation.ConditionFactory.ByName(comboBoxName).And(_automation.ConditionFactory.ByControlType(ControlType.List))),
                TimeSpan.FromSeconds(2)).Result;

            if (list != null)
            {
                var listItem = Retry.WhileNull(
                    () => list.FindFirstDescendant(_automation.ConditionFactory.ByName(itemName).And(_automation.ConditionFactory.ByControlType(ControlType.ListItem))),
                    TimeSpan.FromSeconds(2)).Result;

                if (listItem != null)
                {
                    listItem.Click();
                    Wait.UntilInputIsProcessed();
                }
            }
        }

        public int GetRowCount()
        {
            var grid = WaitForByName("TasksGridView").AsDataGridView();
            return grid.Rows.Length;
        }

        public string GetCellText(int rowIndex, int columnIndex)
        {
            var grid = WaitForByName("TasksGridView").AsDataGridView();
            var rows = grid.Rows;
            if (rowIndex < 0 || rowIndex >= rows.Length)
            {
                return string.Empty;
            }

            var cells = rows[rowIndex].Cells;
            var headerText = GetHeaderText(grid, columnIndex);
            var cell = FindCellByNamePrefix(cells, headerText);
            if (cell == null)
            {
                return string.Empty;
            }

            var cellValue = GetCellValue(cell);
            if (cellValue == "(null)")
            {
                cellValue = null;
            }

            if (headerText == "ステータス" || cellValue == null)
            {
                var isCompletedCell = FindCellByNamePrefix(cells, "完了");
                if (isCompletedCell != null)
                {
                    var completedValue = GetCellValue(isCompletedCell);
                    return completedValue == "True" ? "完了" : "未完了";
                }
            }

            return cellValue ?? string.Empty;
        }

        private string GetHeaderText(DataGridView grid, int columnIndex)
        {
            if (grid.Header == null)
            {
                return null;
            }

            var headerTexts = grid.Header.Columns
                .Select(c => c.Text)
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();

            if (columnIndex >= 0 && columnIndex < headerTexts.Length)
            {
                return headerTexts[columnIndex];
            }

            return null;
        }

        private DataGridViewCell FindCellByNamePrefix(DataGridViewCell[] cells, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return null;
            }

            foreach (var cell in cells)
            {
                string name = null;
                try
                {
                    name = cell.Properties.Name.Value;
                }
                catch (Exception)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(name) && name.StartsWith(prefix + " Row"))
                {
                    return cell;
                }
            }

            return null;
        }

        private string GetCellValue(DataGridViewCell cell)
        {
            try
            {
                return cell.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SelectRow(int rowIndex)
        {
            var grid = WaitForByName("TasksGridView").AsDataGridView();
            var rows = grid.Rows;
            if (rowIndex >= 0 && rowIndex < rows.Length)
            {
                rows[rowIndex].Click();
            }
        }

        public void ClickMessageBoxOk(string title)
        {
            var desktop = _automation.GetDesktop();
            var dialog = Retry.WhileNull(
                () => desktop.FindFirstDescendant(_automation.ConditionFactory.ByName(title).And(_automation.ConditionFactory.ByControlType(ControlType.Window))),
                TimeSpan.FromSeconds(5)).Result;

            if (dialog != null)
            {
                var okButton = dialog.FindFirstDescendant(_automation.ConditionFactory.ByName("OK"));
                if (okButton != null)
                {
                    okButton.AsButton().Click();
                }
            }
        }

        public void ClickMessageBoxYes(string title)
        {
            var desktop = _automation.GetDesktop();
            var dialog = Retry.WhileNull(
                () => desktop.FindFirstDescendant(_automation.ConditionFactory.ByName(title).And(_automation.ConditionFactory.ByControlType(ControlType.Window))),
                TimeSpan.FromSeconds(5)).Result;

            if (dialog != null)
            {
                var yesButton = dialog.FindFirstDescendant(_automation.ConditionFactory.ByName("はい").Or(_automation.ConditionFactory.ByName("Yes")));
                if (yesButton != null)
                {
                    yesButton.AsButton().Click();
                }
            }
        }

        public void Dispose()
        {
            if (_application != null)
            {
                _application.Close();
            }

            if (_automation != null)
            {
                _automation.Dispose();
            }
        }

        private AutomationElement WaitForByName(string name)
        {
            var result = Retry.WhileNull(
                () => _mainWindow.FindFirstDescendant(_automation.ConditionFactory.ByName(name)),
                TimeSpan.FromSeconds(5));

            var element = result.Result;
            if (element == null)
            {
                throw new InvalidOperationException("コントロールが見つかりません: " + name);
            }

            return element;
        }
    }
}
