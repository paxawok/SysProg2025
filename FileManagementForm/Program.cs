using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace FileManagement
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // створення та запуск головної форми файлового менеджера
            FileManagerForm form = new FileManagerForm();
            Application.Run(form);
        }
    }

    // клас із реалізацією форми файлового менеджера
    public class FileManagerForm : Form
    {
        private string currentPath = "";
        private Dictionary<string, Icon> fileIcons;

        // компоненти інтерфейсу
        private SplitContainer splitContainer;
        private TreeView treeView;
        private ListView listView;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStrip toolStrip;

        public FileManagerForm()
        {
            InitializeComponent();
            LoadDrives();
            InitFileIcons();
        }

        // створення компонентів форми
        private void InitializeComponent()
        {
            this.Text = "Файловий менеджер (лаб. робота)";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);

            // основна панель
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 250;
            this.Controls.Add(splitContainer);

            // дерево директорій
            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.AfterSelect += TreeView_AfterSelect;
            splitContainer.Panel1.Controls.Add(treeView);

            // список файлів
            listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.Columns.Add("Ім'я", 200);
            listView.Columns.Add("Розмір", 100);
            listView.Columns.Add("Тип", 100);
            listView.Columns.Add("Дата зміни", 150);
            listView.DoubleClick += ListView_DoubleClick;
            splitContainer.Panel2.Controls.Add(listView);

            // статус
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
            statusStrip.Dock = DockStyle.Bottom;

            // панель інструментів
            toolStrip = new ToolStrip();
            toolStrip.Items.Add("Оновити", null, RefreshButton_Click);
            toolStrip.Items.Add("Нова папка", null, NewFolderButton_Click);
            toolStrip.Items.Add("Видалити", null, DeleteButton_Click);
            this.Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;
        }

        // завантаження дисків
        private void LoadDrives()
        {
            treeView.Nodes.Clear();

            // отримання списку дисків
            string[] drives = Directory.GetLogicalDrives();
            foreach (string drive in drives)
            {
                TreeNode driveNode = new TreeNode(drive);
                driveNode.Tag = drive;
                treeView.Nodes.Add(driveNode);
                LoadSubDirs(driveNode);
            }
        }

        // завантаження піддиректорій
        private void LoadSubDirs(TreeNode parentNode)
        {
            if (parentNode == null || parentNode.Tag == null)
            {
                return; // захист від null
            }

            string path = parentNode.Tag.ToString();

            try
            {
                parentNode.Nodes.Clear();

                // перевіряємо чи директорія існує
                if (!Directory.Exists(path))
                {
                    return;
                }

                // піддиректорії
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] subDirs;

                try
                {
                    subDirs = dir.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    // якщо немає доступу, додаємо спеціальний вузол
                    TreeNode noAccessNode = new TreeNode("Немає доступу");
                    parentNode.Nodes.Add(noAccessNode);
                    return;
                }
                catch (Exception)
                {
                    // інші помилки
                    TreeNode errorNode = new TreeNode("Помилка доступу");
                    parentNode.Nodes.Add(errorNode);
                    return;
                }

                // додаємо виявлені директорії
                foreach (DirectoryInfo subDir in subDirs)
                {
                    try
                    {
                        TreeNode dirNode = new TreeNode(subDir.Name);
                        dirNode.Tag = subDir.FullName;
                        parentNode.Nodes.Add(dirNode);

                        // додаємо заглушку
                        dirNode.Nodes.Add(new TreeNode("..."));
                    }
                    catch
                    {
                        // ігноруємо помилки для окремих директорій
                        continue;
                    }
                }

                // якщо немає жодного підкаталогу, додаємо інформаційний вузол
                if (parentNode.Nodes.Count == 0)
                {
                    TreeNode emptyNode = new TreeNode("Немає підкаталогів");
                    parentNode.Nodes.Add(emptyNode);
                }
            }
            catch (Exception ex)
            {
                // загальна обробка винятків
                // не показуємо MessageBox тут, щоб не перевантажувати інтерфейс
                TreeNode errorNode = new TreeNode($"Помилка: {ex.Message}");
                parentNode.Nodes.Clear();
                parentNode.Nodes.Add(errorNode);
            }
        }

        // вибір директорії
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                // перевірка на null
                if (e != null && e.Node != null && e.Node.Tag != null)
                {
                    currentPath = e.Node.Tag.ToString();
                    statusLabel.Text = currentPath;

                    // розкриття директорії
                    if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "...")
                    {
                        LoadSubDirs(e.Node);
                    }

                    ShowFiles(currentPath);
                }
                else
                {
                    // обробка випадку, коли Tag = null
                    MessageBox.Show("Помилка: вузол не містить шляху.", "Помилка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // загальна обробка будь-яких інших винятків
                MessageBox.Show($"Помилка при виборі директорії: {ex.Message}", "Помилка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // показ файлів
        private void ShowFiles(string path)
        {
            try
            {
                // перевіряємо чи шлях не порожній і чи він існує
                if (string.IsNullOrEmpty(path))
                {
                    // якщо шлях порожній, просто очищаємо список
                    listView.Items.Clear();
                    return;
                }

                if (!Directory.Exists(path))
                {
                    MessageBox.Show($"Директорія {path} не існує.", "Помилка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                listView.Items.Clear();

                // показ папок
                DirectoryInfo dir = new DirectoryInfo(path);

                try
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        ListViewItem item = new ListViewItem(subDir.Name);
                        item.Tag = subDir.FullName;
                        item.SubItems.Add("<DIR>");
                        item.SubItems.Add("Папка");
                        item.SubItems.Add(subDir.LastWriteTime.ToString());
                        listView.Items.Add(item);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // специфічна обробка помилки доступу до директорії
                    MessageBox.Show($"Немає доступу до директорії {path}", "Обмежений доступ",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                try
                {
                    // показ файлів
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        ListViewItem item = new ListViewItem(file.Name);
                        item.Tag = file.FullName;
                        item.SubItems.Add(FormatSize(file.Length));
                        item.SubItems.Add(file.Extension);
                        item.SubItems.Add(file.LastWriteTime.ToString());
                        listView.Items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    // обробка помилок при отриманні файлів
                    MessageBox.Show($"Помилка при читанні файлів: {ex.Message}", "Помилка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // загальна обробка для будь-яких інших винятків
                MessageBox.Show($"Загальна помилка: {ex.Message}", "Помилка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // форматування розміру
        private string FormatSize(long size)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ", "ТБ" };
            double formattedSize = size;
            int order = 0;

            while (formattedSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                formattedSize = formattedSize / 1024;
            }

            return string.Format("{0:0.##} {1}", formattedSize, sizes[order]);
        }

        // подвійний клік
        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                string selectedPath = listView.SelectedItems[0].Tag.ToString();

                if (Directory.Exists(selectedPath))
                {
                    // перехід до папки
                    currentPath = selectedPath;
                    statusLabel.Text = currentPath;
                    ShowFiles(currentPath);

                    // знаходження вузла
                    foreach (TreeNode node in treeView.Nodes)
                    {
                        FindNodeByPath(node, selectedPath);
                    }
                }
                else if (File.Exists(selectedPath))
                {
                    // відкриття файлу
                    try
                    {
                        System.Diagnostics.Process.Start(selectedPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не вдалося відкрити: {ex.Message}", "Помилка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // пошук вузла
        private bool FindNodeByPath(TreeNode node, string path)
        {
            try
            {
                // перевірка на null
                if (node == null || node.Tag == null || string.IsNullOrEmpty(path))
                {
                    return false;
                }

                if (node.Tag.ToString() == path)
                {
                    treeView.SelectedNode = node;
                    return true;
                }

                // розгортання
                if (node.Nodes.Count == 1 && node.Nodes[0].Text == "...")
                {
                    try
                    {
                        LoadSubDirs(node);
                    }
                    catch
                    {
                        // ігноруємо помилки при завантаженні піддиректорій
                    }
                }

                foreach (TreeNode childNode in node.Nodes)
                {
                    try
                    {
                        if (FindNodeByPath(childNode, path))
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        // ігноруємо помилки для окремих дочірніх вузлів
                        continue;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                // глобальна обробка винятків
                return false; // у випадку помилки повертаємо false
            }
        }

        // іконки файлів
        private void InitFileIcons()
        {
            fileIcons = new Dictionary<string, Icon>();
            // іконки можна додати тут
        }

        // оновлення
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                ShowFiles(currentPath);
            }
        }

        // створення папки
        private void NewFolderButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Ім'я нової папки:", "Нова папка", "Нова папка");

                if (!string.IsNullOrEmpty(folderName))
                {
                    try
                    {
                        string newPath = Path.Combine(currentPath, folderName);
                        Directory.CreateDirectory(newPath);
                        ShowFiles(currentPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка створення: {ex.Message}", "Помилка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // видалення
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                string selectedPath = listView.SelectedItems[0].Tag.ToString();
                string itemName = listView.SelectedItems[0].Text;

                DialogResult result = MessageBox.Show($"Видалити '{itemName}'?",
                                        "Підтвердження", MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (Directory.Exists(selectedPath))
                        {
                            Directory.Delete(selectedPath, true);
                        }
                        else if (File.Exists(selectedPath))
                        {
                            File.Delete(selectedPath);
                        }
                        ShowFiles(currentPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}