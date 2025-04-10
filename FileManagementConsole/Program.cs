using System;
using System.IO;
using System.Text;

namespace FileManagementConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
            }
            catch (System.IO.IOException)
            {
                // ігноруємо помилку кодування
            }

            bool exit = false;

            while (!exit)
            {
                try
                {
                    Console.Clear();
                }
                catch
                {
                    // неможливо очистити консоль
                }

                Console.WriteLine("\n===== ФАЙЛОВИЙ МЕНЕДЖЕР =====");
                Console.WriteLine("1. Вивести структуру директорії");
                Console.WriteLine("2. Знайти файл");
                Console.WriteLine("3. Створити папку");
                Console.WriteLine("4. Редагувати текстовий файл");
                Console.WriteLine("0. Вийти");
                Console.Write("\nВаш вибір: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        ShowDirectoryStructure();
                        break;
                    case "2":
                        FindFile();
                        break;
                    case "3":
                        CreateDirectory();
                        break;
                    case "4":
                        EditTextFile();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Невірний вибір! Натисніть будь-яку клавішу...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        // показ структури директорії
        static void ShowDirectoryStructure()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // помилка очищення
            }

            Console.WriteLine("===== СТРУКТУРА ДИРЕКТОРІЇ =====");
            Console.WriteLine("Введіть шлях до директорії:");
            string path = Console.ReadLine();

            try
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"\nСтруктура директорії {path}:");
                    ShowDirStructure(path, "", true);
                }
                else
                {
                    Console.WriteLine("Директорія не існує!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        static void ShowDirStructure(string path, string indent, bool isLastItem = true)
        {
            // вивід директорії
            DirectoryInfo dir = new DirectoryInfo(path);

            // Символи для відображення дерева
            string dirPrefix = isLastItem ? "└── " : "├── ";
            Console.WriteLine($"{indent}{dirPrefix}[{dir.Name}]");

            // Новий відступ для вмісту цієї директорії
            string newIndent = indent + (isLastItem ? "    " : "│   ");

            try
            {
                // вивід файлів
                FileInfo[] files = dir.GetFiles();
                int fileCount = files.Length;
                int currentFile = 0;

                foreach (FileInfo file in files)
                {
                    currentFile++;
                    bool isLastFile = (currentFile == fileCount) && (dir.GetDirectories().Length == 0);

                    string filePrefix = isLastFile ? "└── " : "├── ";
                    Console.WriteLine($"{newIndent}{filePrefix}{file.Name} ({file.Length} байт)");
                }

                // рекурсивний вивід піддиректорій
                DirectoryInfo[] subDirs = dir.GetDirectories();
                int dirCount = subDirs.Length;
                int currentDir = 0;

                foreach (DirectoryInfo subDir in subDirs)
                {
                    currentDir++;
                    bool isLast = (currentDir == dirCount);

                    ShowDirStructure(subDir.FullName, newIndent, isLast);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"{newIndent} Доступ заборонено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{newIndent} Помилка: {ex.Message}");
            }
        }

        // пошук файлу
        static void FindFile()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // помилка очищення консолі
            }

            Console.WriteLine("===== ПОШУК ФАЙЛУ =====");

            Console.WriteLine("Введіть повне або часткове ім'я файлу:");
            string fileName = Console.ReadLine();

            if (string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("Ім'я файлу не може бути порожнім!");
                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Введіть шлях для пошуку:");
            string searchPath = Console.ReadLine();

            if (!Directory.Exists(searchPath))
            {
                Console.WriteLine("Вказана директорія не існує!");
                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
                return;
            }

            // використовуємо значення за замовчуванням
            bool isCaseSensitive = false;  // за замовчуванням не враховуємо регістр
            bool isExactMatch = false;     // за замовчуванням використовуємо частковий збіг

            Console.WriteLine($"Пошук '{fileName}' в {searchPath}...\n");

            int count = SearchForFile(fileName, searchPath, isCaseSensitive, isExactMatch);

            Console.WriteLine($"\nПошук завершено. Знайдено файлів: {count}");
            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        static int SearchForFile(string fileName, string searchPath, bool caseSensitive, bool exactMatch)
        {
            int foundCount = 0;

            try
            {
                // пошук в поточній директорії
                DirectoryInfo dir = new DirectoryInfo(searchPath);

                // отримання всіх файлів
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    bool isMatch = false;

                    if (exactMatch)
                    {
                        // точний збіг імені
                        if (caseSensitive)
                        {
                            isMatch = file.Name == fileName;
                        }
                        else
                        {
                            isMatch = file.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    else
                    {
                        // частковий збіг
                        if (caseSensitive)
                        {
                            isMatch = file.Name.Contains(fileName);
                        }
                        else
                        {
                            isMatch = file.Name.ToLower().Contains(fileName.ToLower());
                        }
                    }

                    if (isMatch)
                    {
                        // виведення інформації про знайдений файл
                        Console.WriteLine($"Знайдено в: {file.FullName}");
                        Console.WriteLine($"Розмір: {file.Length} байт");
                        Console.WriteLine($"Створено: {file.CreationTime}");
                        Console.WriteLine($"Змінено: {file.LastWriteTime}");
                        Console.WriteLine($"Атрибути: {file.Attributes}");
                        Console.WriteLine(new string('-', 40));
                        foundCount++;
                    }
                }

                // рекурсивний пошук у піддиректоріях
                DirectoryInfo[] subDirs = dir.GetDirectories();
                foreach (DirectoryInfo subDir in subDirs)
                {
                    try
                    {
                        foundCount += SearchForFile(fileName, subDir.FullName, caseSensitive, exactMatch);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Доступ до {subDir.FullName} заборонено");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Помилка при доступі до {subDir.FullName}: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Доступ до {searchPath} заборонено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при доступі до {searchPath}: {ex.Message}");
            }

            return foundCount;
        }

        // створення директорії
        static void CreateDirectory()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // помилка
            }

            Console.WriteLine("===== СТВОРЕННЯ ДИРЕКТОРІЇ =====");

            Console.WriteLine("Введіть шлях, де створити директорію:");
            string basePath = Console.ReadLine();

            if (!Directory.Exists(basePath))
            {
                Console.WriteLine("Вказаний шлях не існує!");
                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Введіть ім'я нової директорії:");
            string dirName = Console.ReadLine();

            if (string.IsNullOrEmpty(dirName))
            {
                Console.WriteLine("Ім'я директорії не може бути порожнім!");
                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
                return;
            }

            try
            {
                string newDirPath = Path.Combine(basePath, dirName);

                if (Directory.Exists(newDirPath))
                {
                    Console.WriteLine("Директорія з таким ім'ям вже існує!");
                }
                else
                {
                    Directory.CreateDirectory(newDirPath);
                    Console.WriteLine($"Директорія створена успішно: {newDirPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка створення директорії: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        // редагування текстового файлу
        static void EditTextFile()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // помилка
            }

            Console.WriteLine("===== РЕДАГУВАННЯ ТЕКСТОВОГО ФАЙЛУ =====");

            Console.WriteLine("Введіть повний шлях до файлу:");
            string filePath = Console.ReadLine();

            if (string.IsNullOrEmpty(filePath))
            {
                Console.WriteLine("Шлях не може бути порожнім!");
                Console.WriteLine("\nНатисніть будь-яку клавішу...");
                Console.ReadKey();
                return;
            }

            // перевіряємо чи це текстовий файл
            string extension = Path.GetExtension(filePath).ToLower();
            bool isTextFile = extension == ".txt" || extension == ".log" ||
                             extension == ".csv" || extension == ".xml" ||
                             extension == ".json" || extension == ".html" ||
                             extension == ".css" || extension == ".js" ||
                             extension == ".cs" || extension == ".config";

            if (!isTextFile)
            {
                Console.WriteLine("Попередження: Файл може не бути текстовим. Продовжити? (y/n)");
                string answer = Console.ReadLine();
                if (answer.ToLower() != "y")
                {
                    return;
                }
            }

            try
            {
                // перевіряємо існування файлу
                bool fileExists = File.Exists(filePath);
                string fileContent = "";

                if (fileExists)
                {
                    // читаємо вміст файлу
                    fileContent = File.ReadAllText(filePath);
                    Console.WriteLine("===== ПОТОЧНИЙ ВМІСТ ФАЙЛУ =====");
                    Console.WriteLine(fileContent);
                    Console.WriteLine("==================================");
                }
                else
                {
                    Console.WriteLine("Файл не існує. Буде створено новий файл.");
                }

                Console.WriteLine("\nВведіть новий вміст файлу (для завершення введіть пустий рядок):");

                StringBuilder newContent = new StringBuilder();
                string line;

                while (!string.IsNullOrEmpty(line = Console.ReadLine()))
                {
                    newContent.AppendLine(line);
                }

                Console.WriteLine("\nЗберегти зміни? (y/n)");
                string saveAnswer = Console.ReadLine();

                if (saveAnswer.ToLower() == "y")
                {
                    // зберігаємо файл
                    File.WriteAllText(filePath, newContent.ToString());
                    Console.WriteLine("Файл успішно збережено!");
                }
                else
                {
                    Console.WriteLine("Зміни скасовано.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка при роботі з файлом: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }
    }
}