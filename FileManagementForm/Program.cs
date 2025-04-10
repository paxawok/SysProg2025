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

            // ��������� �� ������ ������� ����� ��������� ���������
            FileManagerForm form = new FileManagerForm();
            Application.Run(form);
        }
    }

    // ���� �� ���������� ����� ��������� ���������
    public class FileManagerForm : Form
    {
        private string currentPath = "";
        private Dictionary<string, Icon> fileIcons;

        // ���������� ����������
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

        // ��������� ���������� �����
        private void InitializeComponent()
        {
            this.Text = "�������� �������� (���. ������)";
            this.Size = new Size(800, 600);
            this.MinimumSize = new Size(600, 400);

            // ������� ������
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 250;
            this.Controls.Add(splitContainer);

            // ������ ���������
            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.AfterSelect += TreeView_AfterSelect;
            splitContainer.Panel1.Controls.Add(treeView);

            // ������ �����
            listView = new ListView();
            listView.Dock = DockStyle.Fill;
            listView.View = View.Details;
            listView.Columns.Add("��'�", 200);
            listView.Columns.Add("�����", 100);
            listView.Columns.Add("���", 100);
            listView.Columns.Add("���� ����", 150);
            listView.DoubleClick += ListView_DoubleClick;
            splitContainer.Panel2.Controls.Add(listView);

            // ������
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            this.Controls.Add(statusStrip);
            statusStrip.Dock = DockStyle.Bottom;

            // ������ �����������
            toolStrip = new ToolStrip();
            toolStrip.Items.Add("�������", null, RefreshButton_Click);
            toolStrip.Items.Add("���� �����", null, NewFolderButton_Click);
            toolStrip.Items.Add("��������", null, DeleteButton_Click);
            this.Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;
        }

        // ������������ �����
        private void LoadDrives()
        {
            treeView.Nodes.Clear();

            // ��������� ������ �����
            string[] drives = Directory.GetLogicalDrives();
            foreach (string drive in drives)
            {
                TreeNode driveNode = new TreeNode(drive);
                driveNode.Tag = drive;
                treeView.Nodes.Add(driveNode);
                LoadSubDirs(driveNode);
            }
        }

        // ������������ �����������
        private void LoadSubDirs(TreeNode parentNode)
        {
            if (parentNode == null || parentNode.Tag == null)
            {
                return; // ������ �� null
            }

            string path = parentNode.Tag.ToString();

            try
            {
                parentNode.Nodes.Clear();

                // ���������� �� ��������� ����
                if (!Directory.Exists(path))
                {
                    return;
                }

                // ����������
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] subDirs;

                try
                {
                    subDirs = dir.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    // ���� ���� �������, ������ ����������� �����
                    TreeNode noAccessNode = new TreeNode("���� �������");
                    parentNode.Nodes.Add(noAccessNode);
                    return;
                }
                catch (Exception)
                {
                    // ���� �������
                    TreeNode errorNode = new TreeNode("������� �������");
                    parentNode.Nodes.Add(errorNode);
                    return;
                }

                // ������ ������� ��������
                foreach (DirectoryInfo subDir in subDirs)
                {
                    try
                    {
                        TreeNode dirNode = new TreeNode(subDir.Name);
                        dirNode.Tag = subDir.FullName;
                        parentNode.Nodes.Add(dirNode);

                        // ������ ��������
                        dirNode.Nodes.Add(new TreeNode("..."));
                    }
                    catch
                    {
                        // �������� ������� ��� ������� ���������
                        continue;
                    }
                }

                // ���� ���� ������� ����������, ������ ������������� �����
                if (parentNode.Nodes.Count == 0)
                {
                    TreeNode emptyNode = new TreeNode("���� ����������");
                    parentNode.Nodes.Add(emptyNode);
                }
            }
            catch (Exception ex)
            {
                // �������� ������� �������
                // �� �������� MessageBox ���, ��� �� ��������������� ���������
                TreeNode errorNode = new TreeNode($"�������: {ex.Message}");
                parentNode.Nodes.Clear();
                parentNode.Nodes.Add(errorNode);
            }
        }

        // ���� ��������
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                // �������� �� null
                if (e != null && e.Node != null && e.Node.Tag != null)
                {
                    currentPath = e.Node.Tag.ToString();
                    statusLabel.Text = currentPath;

                    // ��������� ��������
                    if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "...")
                    {
                        LoadSubDirs(e.Node);
                    }

                    ShowFiles(currentPath);
                }
                else
                {
                    // ������� �������, ���� Tag = null
                    MessageBox.Show("�������: ����� �� ������ �����.", "�������",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // �������� ������� ����-���� ����� �������
                MessageBox.Show($"������� ��� ����� ��������: {ex.Message}", "�������",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ����� �����
        private void ShowFiles(string path)
        {
            try
            {
                // ���������� �� ���� �� ������� � �� �� ����
                if (string.IsNullOrEmpty(path))
                {
                    // ���� ���� �������, ������ ������� ������
                    listView.Items.Clear();
                    return;
                }

                if (!Directory.Exists(path))
                {
                    MessageBox.Show($"��������� {path} �� ����.", "�������",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                listView.Items.Clear();

                // ����� �����
                DirectoryInfo dir = new DirectoryInfo(path);

                try
                {
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirs)
                    {
                        ListViewItem item = new ListViewItem(subDir.Name);
                        item.Tag = subDir.FullName;
                        item.SubItems.Add("<DIR>");
                        item.SubItems.Add("�����");
                        item.SubItems.Add(subDir.LastWriteTime.ToString());
                        listView.Items.Add(item);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // ���������� ������� ������� ������� �� ��������
                    MessageBox.Show($"���� ������� �� �������� {path}", "��������� ������",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                try
                {
                    // ����� �����
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
                    // ������� ������� ��� �������� �����
                    MessageBox.Show($"������� ��� ������ �����: {ex.Message}", "�������",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // �������� ������� ��� ����-���� ����� �������
                MessageBox.Show($"�������� �������: {ex.Message}", "�������",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ������������ ������
        private string FormatSize(long size)
        {
            string[] sizes = { "�", "��", "��", "��", "��" };
            double formattedSize = size;
            int order = 0;

            while (formattedSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                formattedSize = formattedSize / 1024;
            }

            return string.Format("{0:0.##} {1}", formattedSize, sizes[order]);
        }

        // �������� ���
        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                string selectedPath = listView.SelectedItems[0].Tag.ToString();

                if (Directory.Exists(selectedPath))
                {
                    // ������� �� �����
                    currentPath = selectedPath;
                    statusLabel.Text = currentPath;
                    ShowFiles(currentPath);

                    // ����������� �����
                    foreach (TreeNode node in treeView.Nodes)
                    {
                        FindNodeByPath(node, selectedPath);
                    }
                }
                else if (File.Exists(selectedPath))
                {
                    // �������� �����
                    try
                    {
                        System.Diagnostics.Process.Start(selectedPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"�� ������� �������: {ex.Message}", "�������",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ����� �����
        private bool FindNodeByPath(TreeNode node, string path)
        {
            try
            {
                // �������� �� null
                if (node == null || node.Tag == null || string.IsNullOrEmpty(path))
                {
                    return false;
                }

                if (node.Tag.ToString() == path)
                {
                    treeView.SelectedNode = node;
                    return true;
                }

                // �����������
                if (node.Nodes.Count == 1 && node.Nodes[0].Text == "...")
                {
                    try
                    {
                        LoadSubDirs(node);
                    }
                    catch
                    {
                        // �������� ������� ��� ����������� �����������
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
                        // �������� ������� ��� ������� ������� �����
                        continue;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                // ��������� ������� �������
                return false; // � ������� ������� ��������� false
            }
        }

        // ������ �����
        private void InitFileIcons()
        {
            fileIcons = new Dictionary<string, Icon>();
            // ������ ����� ������ ���
        }

        // ���������
        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                ShowFiles(currentPath);
            }
        }

        // ��������� �����
        private void NewFolderButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentPath))
            {
                string folderName = Microsoft.VisualBasic.Interaction.InputBox(
                    "��'� ���� �����:", "���� �����", "���� �����");

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
                        MessageBox.Show($"������� ���������: {ex.Message}", "�������",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // ���������
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                string selectedPath = listView.SelectedItems[0].Tag.ToString();
                string itemName = listView.SelectedItems[0].Text;

                DialogResult result = MessageBox.Show($"�������� '{itemName}'?",
                                        "ϳ�����������", MessageBoxButtons.YesNo,
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
                        MessageBox.Show($"������� ���������: {ex.Message}", "�������",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}