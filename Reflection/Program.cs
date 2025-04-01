using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ReflectionLabEmployee
{
    // ���� �����������
    public class Employee
    {
        // ���������� (4 ��.)
        public string FullName { get; set; }
        public string Position { get; private set; }
        public decimal Salary { get; protected set; }
        public List<string> Projects { get; set; }

        // ����������� �� �������������
        public Employee()
        {
            FullName = "�������� ����������";
            Position = "�� �������";
            Salary = 0m;
            Projects = new List<string>();
        }

        // ���������������� �����������
        public Employee(string fullName, string position, decimal initialSalary)
        {
            FullName = fullName;
            Position = position;
            Salary = initialSalary > 0 ? initialSalary : 0m;
            Projects = new List<string>();
        }

        // ����� ����������� �������
        public void AssignProject(string projectName)
        {
            if (!string.IsNullOrWhiteSpace(projectName) && !Projects.Contains(projectName))
            {
                Projects.Add(projectName);
            }
        }

        // ����� ���������
        public bool Promote(string newPosition, decimal salaryIncrease)
        {
            if (!string.IsNullOrWhiteSpace(newPosition) && salaryIncrease > 0)
            {
                Position = newPosition;
                Salary += salaryIncrease;
                return true;
            }
            return false;
        }

        // ����� ��������� �������
        public string GetEmployeeDetails()
        {
            string projectsStr = Projects.Count > 0 ? string.Join(", ", Projects) : "����";
            return $"ϲ�: {FullName}, ������: {Position}, ��������: {Salary:C}, �������: {projectsStr}";
        }
    }

    // ������� ����� �������
    public partial class MainForm : Form
    {
        private TreeView treeViewReflection;
        private Button btnShowReflection;
        private Button btnShowMethods;
        private Button btnInvokeMethod;
        private Button btnFindProperties;
        private Button btnCreateInstance;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // ������������ �����
            this.Size = new Size(500, 500);
            this.Text = "�������� ������������ �����������";

            // TreeView
            treeViewReflection = new TreeView
            {
                Location = new Point(20, 20),
                Size = new Size(460, 300)
            };
            this.Controls.Add(treeViewReflection);

            // ������ ������ ������������
            btnShowReflection = CreateButton("�������� ����������", 20, 330, ShowPropertiesReflection);

            // ������ ������ ������
            btnShowMethods = CreateButton("�������� ������", 200, 330, ShowMethodsReflection);

            // ������ ������� ������
            btnInvokeMethod = CreateButton("��������� �����", 20, 370, InvokeMethodReflection);

            // ������ ������ ������������
            btnFindProperties = CreateButton("������ ����������", 200, 370, FindPropertiesReflection);

            // ������ ��������� ����������
            btnCreateInstance = CreateButton("�������� ���������", 20, 410, CreateInstanceReflection);
        }

        // ��������� ����� ��������� ������
        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(170, 30)
            };
            btn.Click += clickHandler;
            this.Controls.Add(btn);
            return btn;
        }

        // ����� ������������ ����� ��������
        private void ShowPropertiesReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee("���� ������", "���������", 50000m);
            employee.AssignProject("������� �����");
            employee.AssignProject("�������� �������");

            treeViewReflection.Nodes.Clear();
            DisplayPropertiesInTreeView(employee, treeViewReflection);
        }

        // ����� ������ ����� ��������
        private void ShowMethodsReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee();
            treeViewReflection.Nodes.Clear();
            DisplayMethodsInTreeView(employee, treeViewReflection);
        }

        // ������ ������ ����� ��������
        private void InvokeMethodReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee("���� �������", "��������", 45000m);
            employee.AssignProject("CRM-�������");

            treeViewReflection.Nodes.Clear();
            InvokeMethodDynamically(employee, "GetEmployeeDetails");
        }

        // ����� ������������ ����� ��������
        private void FindPropertiesReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee("���� �������", "�������", 55000m);

            treeViewReflection.Nodes.Clear();
            FindPropertiesByType(employee, typeof(string), treeViewReflection);
        }

        // ��������� ���������� ����� ��������
        private void CreateInstanceReflection(object sender, EventArgs e)
        {
            treeViewReflection.Nodes.Clear();

            object newEmployee = CreateInstanceDynamically(
                typeof(Employee),
                new object[] { "����� ����������", "������", 30000m }
            );

            if (newEmployee != null)
            {
                DisplayPropertiesInTreeView(newEmployee, treeViewReflection);
            }
        }

        // ³���������� ������������ � TreeView
        private void DisplayPropertiesInTreeView(object obj, TreeView treeView)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            TreeNode rootNode = new TreeNode($"��'��� �����: {type.Name}");
            treeView.Nodes.Add(rootNode);

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            );

            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    object propValue = prop.GetValue(obj);

                    if (propValue is IEnumerable<string> collection && !(propValue is string))
                    {
                        TreeNode collectionNode = new TreeNode($"{prop.Name} : {prop.PropertyType.Name} (��������)");

                        int index = 0;
                        foreach (var item in collection)
                        {
                            collectionNode.Nodes.Add($"[{index++}]: {item}");
                        }

                        rootNode.Nodes.Add(collectionNode);
                    }
                    else
                    {
                        rootNode.Nodes.Add($"{prop.Name} : {prop.PropertyType.Name} = {propValue}");
                    }
                }
                catch (Exception ex)
                {
                    rootNode.Nodes.Add($"{prop.Name} : ������� ({ex.Message})");
                }
            }

            treeView.ExpandAll();
        }

        // ³���������� ������ � TreeView
        private void DisplayMethodsInTreeView(object obj, TreeView treeView)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            TreeNode methodsNode = new TreeNode("������ �����");
            treeView.Nodes.Add(methodsNode);

            MethodInfo[] methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly
            );

            foreach (MethodInfo method in methods)
            {
                string methodDescription = $"{method.ReturnType.Name} {method.Name}(";

                ParameterInfo[] parameters = method.GetParameters();
                methodDescription += string.Join(", ",
                    parameters.Select(p => $"{p.ParameterType.Name} {p.Name}")
                );
                methodDescription += ")";

                methodsNode.Nodes.Add(methodDescription);
            }

            treeView.ExpandAll();
        }

        // ������ ������ ��������
        private void InvokeMethodDynamically(object obj, string methodName, object[] parameters = null)
        {
            try
            {
                Type type = obj.GetType();
                MethodInfo method = parameters == null
                    ? type.GetMethod(methodName)
                    : type.GetMethod(methodName, parameters.Select(p => p.GetType()).ToArray());

                if (method != null)
                {
                    object result = method.Invoke(obj, parameters);

                    TreeNode resultNode = new TreeNode($"��������� ������� {methodName}");
                    resultNode.Nodes.Add(result?.ToString() ?? "null");

                    treeViewReflection.Nodes.Add(resultNode);
                    treeViewReflection.ExpandAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ������� ������: {ex.Message}");
            }
        }

        // ����� ������������ �� �����
        private void FindPropertiesByType(object obj, Type targetType, TreeView treeView)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            TreeNode typeSearchNode = new TreeNode($"���������� ���� {targetType.Name}");
            treeView.Nodes.Add(typeSearchNode);

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            ).Where(p => p.PropertyType == targetType).ToArray();

            foreach (PropertyInfo prop in properties)
            {
                try
                {
                    object propValue = prop.GetValue(obj);
                    typeSearchNode.Nodes.Add($"{prop.Name} : {propValue}");
                }
                catch (Exception ex)
                {
                    typeSearchNode.Nodes.Add($"{prop.Name} : ������� ({ex.Message})");
                }
            }

            treeView.ExpandAll();
        }

        // ��������� ���������� ��������
        private object CreateInstanceDynamically(Type type, object[] constructorParams = null)
        {
            try
            {
                if (constructorParams == null || constructorParams.Length == 0)
                {
                    return Activator.CreateInstance(type);
                }

                ConstructorInfo constructor = type.GetConstructor(
                    constructorParams.Select(p => p.GetType()).ToArray()
                );

                return constructor?.Invoke(constructorParams);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ��������� ����������: {ex.Message}");
                return null;
            }
        }

        // ����� ����� �������
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}