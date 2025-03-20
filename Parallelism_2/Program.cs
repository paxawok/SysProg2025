using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ParallelProcessingExperiments
{
    class Program
    {
        // Завдання 1: Експерименти з Parallel.For
        static void Main(string[] args)
        {
            Console.WriteLine("=== Експерименти з паралельною обробкою масивів ===\n");
            RunExperiments();

            Console.WriteLine("\n=== Завдання 2: Переривання циклу в околі заданого числа ===\n");
            RunBreakExample();

            Console.WriteLine("\n=== Завдання 3: Розпаралелювання за допомогою ForEach ===\n");
            RunForEachExample();

            Console.WriteLine("\n=== Завдання 4: Використання лямбда-виразу як тіла циклу ===\n");
            RunLambdaExample();

            Console.WriteLine("\nПрограма завершена. Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        // Завдання 1: Експерименти з різними типами даних, розмірами масивів та складністю обчислень
        static void RunExperiments()
        {
            // Експеримент із різними типами масивів
            Console.WriteLine("Тип даних: int, Розмір: 10,000,000, Формула: x = x / 10");
            RunBenchmark<int>(10000000, (x) => x / 10);

            Console.WriteLine("\nТип даних: double, Розмір: 10,000,000, Формула: x = x / 10");
            RunBenchmark<double>(10000000, (x) => x / 10.0);

            // Експеримент із різними розмірами масивів
            Console.WriteLine("\nТип даних: double, Розмір: 1,000,000, Формула: x = x / π");
            RunBenchmark<double>(1000000, (x) => x / Math.PI);

            Console.WriteLine("\nТип даних: double, Розмір: 50,000,000, Формула: x = x / π");
            RunBenchmark<double>(50000000, (x) => x / Math.PI);

            // Експеримент із різною складністю обчислень
            Console.WriteLine("\nТип даних: double, Розмір: 5,000,000, Формула: x = e^x / x^π");
            RunBenchmark<double>(5000000, (x) => Math.Exp(x) / Math.Pow(x, Math.PI));

            Console.WriteLine("\nТип даних: double, Розмір: 1,000,000, Формула: x = e^πx / x^π");
            RunBenchmark<double>(1000000, (x) => Math.Exp(Math.PI * x) / Math.Pow(x, Math.PI));
        }

        // Універсальний метод для проведення експерименту
        static void RunBenchmark<T>(int size, Func<T, T> operation) where T : struct
        {
            // Створюємо масиви для різних типів
            dynamic array = typeof(T) == typeof(int) ?
                new int[size] :
                new double[size];

            // Ініціалізуємо масив
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i + 1; // +1 щоб уникнути ділення на нуль
            }

            // Створюємо секундомір
            Stopwatch sw = new Stopwatch();

            // Послідовне виконання
            sw.Start();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = operation(array[i]);
            }
            sw.Stop();

            Console.WriteLine($"Послідовне виконання: {sw.Elapsed.TotalSeconds:F4} секунд");

            // Скидаємо масив
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i + 1;
            }

            // Паралельне виконання
            sw.Reset();
            sw.Start();
            Parallel.For(0, array.Length, (Action<int>)(i =>
            {
                array[i] = operation(array[i]);
            }));
            sw.Stop();

            Console.WriteLine($"Паралельне виконання: {sw.Elapsed.TotalSeconds:F4} секунд");

            // Розрахунок прискорення
            double speedup = sw.Elapsed.TotalSeconds > 0 ?
                sw.Elapsed.TotalSeconds / sw.Elapsed.TotalSeconds : 0;
            Console.WriteLine($"Прискорення: {speedup:F2}x");
        }

        // Завдання 2: Переривання циклу при входженні в окіл числа
        static void RunBreakExample()
        {
            const int SIZE = 10000000;
            const double TARGET_VALUE = 5000.0;
            const double EPSILON = 0.5;     // Допустиме відхилення

            double[] data = new double[SIZE];

            // Ініціалізація масиву
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i / 1000.0;  // Масив значень від 0 до 10000
            }

            Console.WriteLine($"Шукаємо значення близькі до {TARGET_VALUE} з точністю ±{EPSILON}");

            // Запускаємо паралельний цикл із можливістю переривання
            ParallelLoopResult result = Parallel.For(0, data.Length, (i, state) =>
            {
                // Якщо потрапляємо в окіл числа TARGET_VALUE
                if (Math.Abs(data[i] - TARGET_VALUE) <= EPSILON)
                {
                    Console.WriteLine($"Знайдено підходяще значення {data[i]} на індексі {i}");
                    state.Break(); // Перериваємо цикл
                }
            });

            // Перевіряємо результат
            if (!result.IsCompleted)
            {
                Console.WriteLine($"Цикл перервано на ітерації {result.LowestBreakIteration}");
            }
            else
            {
                Console.WriteLine("Цикл завершився повністю, підходящих значень не знайдено");
            }
        }

        // Завдання 3: Використання ForEach для паралельної обробки
        static void RunForEachExample()
        {
            const int SIZE = 1000000;
            double[] data = new double[SIZE];

            // Ініціалізація масиву
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i;
            }

            // Штучно додамо від'ємне число для демонстрації Break
            data[500000] = -1;

            Console.WriteLine("Запуск Parallel.ForEach з можливістю переривання при знаходженні від'ємного числа");

            // Використовуємо ForEach з параметром ParallelLoopState
            ParallelLoopResult result = Parallel.ForEach(data, (value, state) =>
            {
                // Перериваємо цикл при знаходженні від'ємного числа
                if (value < 0)
                {
                    Console.WriteLine($"Знайдено від'ємне значення: {value}");
                    state.Break();
                    return;
                }

                // Складна операція для демонстрації
                double result = Math.Sin(value) * Math.Cos(value);

                // Виводимо результат через певні інтервали, щоб не засмічувати консоль
                if (value % 100000 == 0)
                {
                    Console.WriteLine($"Оброблено значення {value}, результат: {result:F4}");
                }
            });

            // Перевіряємо результат
            if (!result.IsCompleted)
            {
                Console.WriteLine("ForEach цикл був перерваний");
            }
            else
            {
                Console.WriteLine("ForEach цикл завершився повністю");
            }
        }

        // Завдання 4: Використання лямбда-виразу як тіла циклу
        static void RunLambdaExample()
        {
            const int SIZE = 5000000;
            double[] data = new double[SIZE];

            // Ініціалізація масиву
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i;
            }

            Console.WriteLine("Запуск Parallel.For з лямбда-виразом");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Використовуємо лямбда-вираз безпосередньо як тіло циклу
            Parallel.For(0, data.Length, i =>
            {
                // Складне обчислення в лямбда-виразі
                data[i] = Math.Pow(Math.Sin(data[i] / 1000), 2) +
                          Math.Pow(Math.Cos(data[i] / 1000), 2);

                // Виводимо кілька значень для демонстрації
                if (i % 1000000 == 0)
                {
                    Console.WriteLine($"Елемент {i}: {data[i]:F6}");
                }
            });

            sw.Stop();
            Console.WriteLine($"Паралельне виконання з лямбда-виразом: {sw.Elapsed.TotalSeconds:F4} секунд");

            // Перевіряємо тотожність sin^2 + cos^2 = 1
            bool isIdentityValid = true;
            for (int i = 0; i < data.Length; i += 100000)
            {
                if (Math.Abs(data[i] - 1.0) > 1e-10)
                {
                    isIdentityValid = false;
                    Console.WriteLine($"Помилка в тотожності на індексі {i}: {data[i]}");
                    break;
                }
            }

            if (isIdentityValid)
            {
                Console.WriteLine("Тотожність sin2(x) + cos2(x) = 1 виконується для всіх перевірених елементів");
            }
        }
    }
}
