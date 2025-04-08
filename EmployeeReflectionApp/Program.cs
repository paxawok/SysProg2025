using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmployeeReflectionApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // ���� �����������
    public class Employee
    {
        // 4 ����������
        public string FullName { get; set; }
        public int EmployeeId { get; set; }
        public decimal Salary { get; set; }
        public Dictionary<string, int> Skills { get; set; } // ��������

        // 2 ������������
        // ����������� �� �������������
        public Employee()
        {
            FullName = "Unknown";
            EmployeeId = 0;
            Salary = 0;
            Skills = new Dictionary<string, int>();
        }

        // ���������������� �����������
        public Employee(string fullName, int employeeId, decimal salary)
        {
            FullName = fullName;
            EmployeeId = employeeId;
            Salary = salary;
            Skills = new Dictionary<string, int>();
        }

        // 3 ������
        // ����� 1: ���������� ���� ��������
        public decimal CalculateAnnualSalary()
        {
            return Salary * 12;
        }

        // ����� 2: ��������� ��� ��������� �������
        public void AddSkill(string skillName, int proficiencyLevel)
        {
            if (Skills.ContainsKey(skillName))
            {
                Skills[skillName] = proficiencyLevel;
            }
            else
            {
                Skills.Add(skillName, proficiencyLevel);
            }
        }

        // ����� 3: ��������� ���������� ��� �����������
        public string GetEmployeeSummary()
        {
            string skillsList = string.Join(", ", Skills.Select(s => $"{s.Key} (г����: {s.Value})"));
            return $"ID: {EmployeeId}\n��'�: {FullName}\nг��� ��������: {CalculateAnnualSalary():C}\n�������: {skillsList}";
        }
    }

    // ���� ������� �����
    public partial class MainForm : Form
    {
        private TreeView treeView1;
        private Label label1;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            this.treeView1 = new TreeView();
            this.label1 = new Label();
            this.SuspendLayout();

            // treeView1
            this.treeView1.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            this.treeView1.Location = new Point(12, 45);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new Size(776, 393);
            this.treeView1.TabIndex = 0;

            // label1
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(295, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "����� ����� ���������� ����� ��������";

            // MainForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeView1);
            this.Name = "MainForm";
            this.Text = "�������� �����������";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // ��������� ��'���� �����������
            Employee employee = new Employee("���� ��������", 12345, 25000);

            // ��������� �������
            employee.AddSkill("�������������", 9);
            employee.AddSkill("����������", 8);
            employee.AddSkill("��������� ���������", 7);

            // ����������� ����� ��'���� ����� ��������
            DisplayObjectDetails(employee);
        }
        // �������� ����� ��� �������
        private void DisplayObjectDetails(object obj)
        {
            // �������� treeView
            treeView1.Nodes.Clear();

            // ��������� ���� ��'����
            Type type = obj.GetType();

            // ��������� ���������� �����
            TreeNode rootNode = new TreeNode($"{type.FullName} �����");

            // ��������� ����� ������������
            TreeNode propertiesNode = new TreeNode("����������");

            // ��������� ������������ ��'����
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // ������� ����� ����������
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                string valueString = "";

                // �������� ������� ��� ��������
                if (value is IDictionary dictionary)
                {
                    TreeNode dictionaryNode = new TreeNode($"{property.Name} ({property.PropertyType.Name})");

                    foreach (DictionaryEntry entry in dictionary)
                    {
                        dictionaryNode.Nodes.Add(new TreeNode($"����: {entry.Key}, ��������: {entry.Value}"));
                    }

                    propertiesNode.Nodes.Add(dictionaryNode);
                }
                else
                {
                    // ������������ ��������� ������������
                    valueString = value != null ? value.ToString() : "null";
                    propertiesNode.Nodes.Add(new TreeNode($"{property.Name} ({property.PropertyType.Name}): {valueString}"));
                }
            }

            // ��������� ����� ������
            TreeNode methodsNode = new TreeNode("������");

            // ��������� �������� ������ ����������
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // ������� ������� ������
            foreach (MethodInfo method in methods)
            {
                TreeNode methodNode = new TreeNode($"{method.Name}");

                // ��������� ���������
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    string paramsInfo = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    methodNode.Nodes.Add(new TreeNode($"���������: {paramsInfo}"));
                }
                else
                {
                    methodNode.Nodes.Add(new TreeNode("���������: ������"));
                }

                // ��������� ���� ����������
                methodNode.Nodes.Add(new TreeNode($"��� ����������: {method.ReturnType.Name}"));

                methodsNode.Nodes.Add(methodNode);
            }

            // ��������� ��� ����� �� ����������
            rootNode.Nodes.Add(propertiesNode);
            rootNode.Nodes.Add(methodsNode);

            // ��������� ���������� ����� �� TreeView
            treeView1.Nodes.Add(rootNode);

            // ����������� ��� �����
            treeView1.ExpandAll();
        }
    }
}