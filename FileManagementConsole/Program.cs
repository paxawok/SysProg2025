using System;
using System.IO;

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

                Console.WriteLine("===== ФАЙЛОВИЙ МЕНЕДЖЕР =====");
                Console.WriteLine("1. Вивести структуру директорії");
                Console.WriteLine("2. Знайти файл");
                Console.WriteLine("3. Графічний файловий менеджер");
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
                        Console.WriteLine("Для запуску графічного менеджера запустіть проєкт FileManagement");
                        Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
                        Console.ReadKey();
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

        // перше завдання - показ структури директорії
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

            Console.WriteLine("=== СТРУКТУРА ДИРЕКТОРІЇ ===");
            Console.WriteLine("Введіть шлях до директорії:");
            string path = Console.ReadLine();

            try
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Структура директорії {path}:");
                    ShowDirStructure(path, "");
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

        static void ShowDirStructure(string path, string indent)
        {
            // вивід директорії
            DirectoryInfo dir = new DirectoryInfo(path);
            Console.WriteLine($"{indent}[{dir.Name}]");

            try
            {
                // вивід файлів
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    Console.WriteLine($"{indent}  {file.Name} ({file.Length} байт)");
                }

                // рекурсивний вивід піддиректорій
                DirectoryInfo[] subDirs = dir.GetDirectories();
                foreach (DirectoryInfo subDir in subDirs)
                {
                    ShowDirStructure(subDir.FullName, indent + "    ");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"{indent}  Доступ заборонено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{indent}  Помилка: {ex.Message}");
            }
        }

        // друге завдання - пошук файлу
        static void FindFile()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                // помилка
            }

            Console.WriteLine("=== ПОШУК ФАЙЛУ ===");

            Console.WriteLine("Введіть ім'я файлу:");
            string fileName = Console.ReadLine();

            Console.WriteLine("Введіть шлях для пошуку:");
            string searchPath = Console.ReadLine();

            if (Directory.Exists(searchPath))
            {
                Console.WriteLine($"Шукаємо '{fileName}' в {searchPath}...\n");
                int count = SearchForFile(fileName, searchPath);
                Console.WriteLine($"\nПошук завершено. Знайдено файлів: {count}");
            }
            else
            {
                Console.WriteLine("Неправильний шлях!");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            Console.ReadKey();
        }

        static int SearchForFile(string fileName, string searchPath)
        {
            int foundCount = 0;

            try
            {
                // пошук в поточній директорії
                DirectoryInfo dir = new DirectoryInfo(searchPath);

                // перевірка файлів
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        // інформація про знайдений файл
                        Console.WriteLine($"Знайдено: {file.FullName}");
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
                    foundCount += SearchForFile(fileName, subDir.FullName);
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
    }
}