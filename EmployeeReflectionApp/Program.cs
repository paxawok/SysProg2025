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

    // клас співробітника
    public class Employee
    {
        // 4 властивості
        public string FullName { get; set; }
        public int EmployeeId { get; set; }
        public decimal Salary { get; set; }
        public Dictionary<string, int> Skills { get; set; } // колекція

        // 2 конструктори
        // конструктор за замовчуванням
        public Employee()
        {
            FullName = "Unknown";
            EmployeeId = 0;
            Salary = 0;
            Skills = new Dictionary<string, int>();
        }

        // параметризований конструктор
        public Employee(string fullName, int employeeId, decimal salary)
        {
            FullName = fullName;
            EmployeeId = employeeId;
            Salary = salary;
            Skills = new Dictionary<string, int>();
        }

        // 3 методи
        // метод 1: розрахунок річної зарплати
        public decimal CalculateAnnualSalary()
        {
            return Salary * 12;
        }

        // метод 2: додавання або оновлення навички
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

        // метод 3: отримання інформації про співробітника
        public string GetEmployeeSummary()
        {
            string skillsList = string.Join(", ", Skills.Select(s => $"{s.Key} (Рівень: {s.Value})"));
            return $"ID: {EmployeeId}\nІм'я: {FullName}\nРічна зарплата: {CalculateAnnualSalary():C}\nНавички: {skillsList}";
        }
    }

    // клас головної форми
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
            this.label1.Text = "Аналіз класу Співробітник через рефлексію";

            // MainForm
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeView1);
            this.Name = "MainForm";
            this.Text = "Рефлексія Співробітника";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // створення об'єкту співробітника
            Employee employee = new Employee("Іван Петренко", 12345, 25000);

            // додавання навичок
            employee.AddSkill("Програмування", 9);
            employee.AddSkill("Комунікація", 8);
            employee.AddSkill("Управління проектами", 7);

            // відображення даних об'єкта через рефлексію
            DisplayObjectDetails(employee);
        }
        // головний метод для рефлексії
        private void DisplayObjectDetails(object obj)
        {
            // очищення treeView
            treeView1.Nodes.Clear();

            // отримання типу об'єкта
            Type type = obj.GetType();

            // створення кореневого вузла
            TreeNode rootNode = new TreeNode($"{type.FullName} Деталі");

            // створення вузла властивостей
            TreeNode propertiesNode = new TreeNode("Властивості");

            // отримання властивостей об'єкта
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // обробка кожної властивості
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                string valueString = "";

                // особлива обробка для колекцій
                if (value is IDictionary dictionary)
                {
                    TreeNode dictionaryNode = new TreeNode($"{property.Name} ({property.PropertyType.Name})");

                    foreach (DictionaryEntry entry in dictionary)
                    {
                        dictionaryNode.Nodes.Add(new TreeNode($"Ключ: {entry.Key}, Значення: {entry.Value}"));
                    }

                    propertiesNode.Nodes.Add(dictionaryNode);
                }
                else
                {
                    // форматування звичайних властивостей
                    valueString = value != null ? value.ToString() : "null";
                    propertiesNode.Nodes.Add(new TreeNode($"{property.Name} ({property.PropertyType.Name}): {valueString}"));
                }
            }

            // створення вузла методів
            TreeNode methodsNode = new TreeNode("Методи");

            // отримання публічних методів екземпляра
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            // обробка кожного методу
            foreach (MethodInfo method in methods)
            {
                TreeNode methodNode = new TreeNode($"{method.Name}");

                // отримання параметрів
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0)
                {
                    string paramsInfo = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    methodNode.Nodes.Add(new TreeNode($"Параметри: {paramsInfo}"));
                }
                else
                {
                    methodNode.Nodes.Add(new TreeNode("Параметри: відсутні"));
                }

                // отримання типу повернення
                methodNode.Nodes.Add(new TreeNode($"Тип повернення: {method.ReturnType.Name}"));

                methodsNode.Nodes.Add(methodNode);
            }

            // додавання всіх вузлів до кореневого
            rootNode.Nodes.Add(propertiesNode);
            rootNode.Nodes.Add(methodsNode);

            // додавання кореневого вузла до TreeView
            treeView1.Nodes.Add(rootNode);

            // розгортання всіх вузлів
            treeView1.ExpandAll();
        }
    }
}