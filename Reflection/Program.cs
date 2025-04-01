using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ReflectionLabEmployee
{
    // Клас співробітника
    public class Employee
    {
        // Властивості (4 шт.)
        public string FullName { get; set; }
        public string Position { get; private set; }
        public decimal Salary { get; protected set; }
        public List<string> Projects { get; set; }

        // Конструктор за замовчуванням
        public Employee()
        {
            FullName = "Невідомий співробітник";
            Position = "Не вказано";
            Salary = 0m;
            Projects = new List<string>();
        }

        // Параметризований конструктор
        public Employee(string fullName, string position, decimal initialSalary)
        {
            FullName = fullName;
            Position = position;
            Salary = initialSalary > 0 ? initialSalary : 0m;
            Projects = new List<string>();
        }

        // Метод призначення проекту
        public void AssignProject(string projectName)
        {
            if (!string.IsNullOrWhiteSpace(projectName) && !Projects.Contains(projectName))
            {
                Projects.Add(projectName);
            }
        }

        // Метод підвищення
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

        // Метод отримання деталей
        public string GetEmployeeDetails()
        {
            string projectsStr = Projects.Count > 0 ? string.Join(", ", Projects) : "Немає";
            return $"ПІБ: {FullName}, Посада: {Position}, Зарплата: {Salary:C}, Проекти: {projectsStr}";
        }
    }

    // Головна форма додатку
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
            // Налаштування форми
            this.Size = new Size(500, 500);
            this.Text = "Рефлексія властивостей співробітника";

            // TreeView
            treeViewReflection = new TreeView
            {
                Location = new Point(20, 20),
                Size = new Size(460, 300)
            };
            this.Controls.Add(treeViewReflection);

            // Кнопка показу властивостей
            btnShowReflection = CreateButton("Показати властивості", 20, 330, ShowPropertiesReflection);

            // Кнопка показу методів
            btnShowMethods = CreateButton("Показати методи", 200, 330, ShowMethodsReflection);

            // Кнопка виклику методу
            btnInvokeMethod = CreateButton("Викликати метод", 20, 370, InvokeMethodReflection);

            // Кнопка пошуку властивостей
            btnFindProperties = CreateButton("Знайти властивості", 200, 370, FindPropertiesReflection);

            // Кнопка створення екземпляра
            btnCreateInstance = CreateButton("Створити екземпляр", 20, 410, CreateInstanceReflection);
        }

        // Допоміжний метод створення кнопок
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

        // Показ властивостей через рефлексію
        private void ShowPropertiesReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee("Іван Петров", "Розробник", 50000m);
            employee.AssignProject("Система обліку");
            employee.AssignProject("Мобільний додаток");

            treeViewReflection.Nodes.Clear();
            DisplayPropertiesInTreeView(employee, treeViewReflection);
        }

        // Показ методів через рефлексію
        private void ShowMethodsReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee();
            treeViewReflection.Nodes.Clear();
            DisplayMethodsInTreeView(employee, treeViewReflection);
        }

        // Виклик методу через рефлексію
        private void InvokeMethodReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee("Марія Іванова", "Менеджер", 45000m);
            employee.AssignProject("CRM-система");

            treeViewReflection.Nodes.Clear();
            InvokeMethodDynamically(employee, "GetEmployeeDetails");
        }

        // Пошук властивостей через рефлексію
        private void FindPropertiesReflection(object sender, EventArgs e)
        {
            Employee employee = new Employee("Олег Сидоров", "Аналітик", 55000m);

            treeViewReflection.Nodes.Clear();
            FindPropertiesByType(employee, typeof(string), treeViewReflection);
        }

        // Створення екземпляра через рефлексію
        private void CreateInstanceReflection(object sender, EventArgs e)
        {
            treeViewReflection.Nodes.Clear();

            object newEmployee = CreateInstanceDynamically(
                typeof(Employee),
                new object[] { "Новий Співробітник", "Стажер", 30000m }
            );

            if (newEmployee != null)
            {
                DisplayPropertiesInTreeView(newEmployee, treeViewReflection);
            }
        }

        // Відображення властивостей у TreeView
        private void DisplayPropertiesInTreeView(object obj, TreeView treeView)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            TreeNode rootNode = new TreeNode($"Об'єкт класу: {type.Name}");
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
                        TreeNode collectionNode = new TreeNode($"{prop.Name} : {prop.PropertyType.Name} (Колекція)");

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
                    rootNode.Nodes.Add($"{prop.Name} : Помилка ({ex.Message})");
                }
            }

            treeView.ExpandAll();
        }

        // Відображення методів у TreeView
        private void DisplayMethodsInTreeView(object obj, TreeView treeView)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            TreeNode methodsNode = new TreeNode("Методи класу");
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

        // Виклик методу динамічно
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

                    TreeNode resultNode = new TreeNode($"Результат виклику {methodName}");
                    resultNode.Nodes.Add(result?.ToString() ?? "null");

                    treeViewReflection.Nodes.Add(resultNode);
                    treeViewReflection.ExpandAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка виклику методу: {ex.Message}");
            }
        }

        // Пошук властивостей за типом
        private void FindPropertiesByType(object obj, Type targetType, TreeView treeView)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            TreeNode typeSearchNode = new TreeNode($"Властивості типу {targetType.Name}");
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
                    typeSearchNode.Nodes.Add($"{prop.Name} : Помилка ({ex.Message})");
                }
            }

            treeView.ExpandAll();
        }

        // Створення екземпляра динамічно
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
                MessageBox.Show($"Помилка створення екземпляра: {ex.Message}");
                return null;
            }
        }

        // Точка входу додатку
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}