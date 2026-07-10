using System;
using System.Windows.Forms;
using TodoApp.Core;

namespace TodoApp.WinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tasks.json");
            var repository = new JsonTaskRepository(filePath);
            var service = new TaskService(repository);

            Application.Run(new MainForm(service));
        }
    }
}
