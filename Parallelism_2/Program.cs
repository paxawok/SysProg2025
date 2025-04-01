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
            Console.WriteLine("=== Експерименти з паралельною обробкою масивiв ===\n");
            RunExperiments();

            Console.WriteLine("\n=== Завдання 2: Переривання циклу в околi заданого числа ===\n");
            RunBreakExample();

            Console.WriteLine("\n=== Завдання 3: Розпаралелювання за допомогою ForEach ===\n");
            RunForEachExample();

            Console.WriteLine("\n=== Завдання 4: Використання лямбда-виразу як тiла циклу ===\n");
            RunLambdaExample();

            Console.WriteLine("\nПрограма завершена. Натиснiть будь-яку клавiшу для виходу...");
            Console.ReadKey();
        }

        // Завдання 1: Експерименти з рiзними типами даних, розмiрами масивiв та складнiстю обчислень
        static void RunExperiments()
        {
            // Експеримент iз рiзними типами масивiв
            Console.WriteLine("Тип даних: int, Розмiр: 10,000,000, Формула: x = x / 10");
            RunBenchmark<int>(10000000, (x) => x / 10);

            Console.WriteLine("\nТип даних: double, Розмiр: 10,000,000, Формула: x = x / 10");
            RunBenchmark<double>(10000000, (x) => x / 10.0);

            // Експеримент iз рiзними розмiрами масивiв
            Console.WriteLine("\nТип даних: double, Розмiр: 1,000,000, Формула: x = x / pi");
            RunBenchmark<double>(1000000, (x) => x / Math.PI);

            Console.WriteLine("\nТип даних: double, Розмiр: 50,000,000, Формула: x = x / pi");
            RunBenchmark<double>(50000000, (x) => x / Math.PI);

            // Експеримент iз рiзною складнiстю обчислень
            Console.WriteLine("\nТип даних: double, Розмiр: 5,000,000, Формула: x = e^x / x^pi");
            RunBenchmark<double>(5000000, (x) => Math.Exp(x) / Math.Pow(x, Math.PI));

            Console.WriteLine("\nТип даних: double, Розмiр: 1,000,000, Формула: x = e^pi x / x^pi ");
            RunBenchmark<double>(1000000, (x) => Math.Exp(Math.PI * x) / Math.Pow(x, Math.PI));
        }

        // Унiверсальний метод для проведення експерименту
        static void RunBenchmark<T>(int size, Func<T, T> operation) where T : struct
        {
            // Створюємо масиви для рiзних типiв
            dynamic array = typeof(T) == typeof(int) ?
                new int[size] :
                new double[size];

            // iнiцiалiзуємо масив
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i + 1; // +1 щоб уникнути дiлення на нуль
            }

            // Створюємо секундомiр
            Stopwatch sw = new Stopwatch();

            // Послiдовне виконання
            sw.Start();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = operation(array[i]);
            }
            sw.Stop();

            // Зберiгаємо час послiдовного виконання
            double sequentialTime = sw.Elapsed.TotalSeconds;
            Console.WriteLine($"Послiдовне виконання: {sequentialTime:F4} секунд");
            
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

            // Зберiгаємо час паралельного виконання
            double parallelTime = sw.Elapsed.TotalSeconds;
            Console.WriteLine($"Паралельне виконання: {parallelTime:F4} секунд");

            // Розрахунок прискорення
            double speedup = parallelTime > 0 ?
                sequentialTime / parallelTime : 0;
            Console.WriteLine($"Прискорення: {speedup:F2}x");
        }

        // Завдання 2: Переривання циклу при входженнi в окiл числа
        static void RunBreakExample()
        {
            const int SIZE = 10000000;
            const double TARGET_VALUE = 5000.0;
            const double EPSILON = 0.5;     // Допустиме вiдхилення

            double[] data = new double[SIZE];

            // iнiцiалiзацiя масиву
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i / 1000.0;  // Масив значень вiд 0 до 10000
            }

            Console.WriteLine($"Шукаємо значення близькi до {TARGET_VALUE} з точнiстю ±{EPSILON}");

            // Запускаємо паралельний цикл iз можливiстю переривання
            ParallelLoopResult result = Parallel.For(0, data.Length, (i, state) =>
            {
                // Якщо потрапляємо в окiл числа TARGET_VALUE
                if (Math.Abs(data[i] - TARGET_VALUE) <= EPSILON)
                {
                    Console.WriteLine($"Знайдено пiдходяще значення {data[i]} на iндексi {i}");
                    state.Break(); // Перериваємо цикл
                }
            });

            // Перевiряємо результат
            if (!result.IsCompleted)
            {
                Console.WriteLine($"Цикл перервано на iтерацiї {result.LowestBreakIteration}");
            }
            else
            {
                Console.WriteLine("Цикл завершився повнiстю, пiдходящих значень не знайдено");
            }
        }

        // Завдання 3: Використання ForEach для паралельної обробки
        static void RunForEachExample()
        {
            const int SIZE = 1000000;
            double[] data = new double[SIZE];

            // iнiцiалiзацiя масиву
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i;
            }

            // Штучно додамо вiд'ємне число для демонстрацiї Break
            data[500000] = -1;

            Console.WriteLine("Запуск Parallel.ForEach з можливiстю переривання при знаходженнi вiд'ємного числа");

            // Використовуємо ForEach з параметром ParallelLoopState
            ParallelLoopResult result = Parallel.ForEach(data, (value, state) =>
            {
                // Перериваємо цикл при знаходженнi вiд'ємного числа
                if (value < 0)
                {
                    Console.WriteLine($"Знайдено вiд'ємне значення: {value}");
                    state.Break();
                    return;
                }

                // Складна операцiя для демонстрацiї
                double result = Math.Sin(value) * Math.Cos(value);

                // Виводимо результат через певнi iнтервали, щоб не засмiчувати консоль
                if (value % 100000 == 0)
                {
                    Console.WriteLine($"Оброблено значення {value}, результат: {result:F4}");
                }
            });

            // Перевiряємо результат
            if (!result.IsCompleted)
            {
                Console.WriteLine("ForEach цикл був перерваний");
            }
            else
            {
                Console.WriteLine("ForEach цикл завершився повнiстю");
            }
        }

        // Завдання 4: Використання лямбда-виразу як тiла циклу
        static void RunLambdaExample()
        {
            const int SIZE = 5000000;
            double[] data = new double[SIZE];

            // iнiцiалiзацiя масиву
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = i;
            }

            Console.WriteLine("Запуск Parallel.For з лямбда-виразом");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Використовуємо лямбда-вираз безпосередньо як тiло циклу
            Parallel.For(0, data.Length, i =>
            {
                // Складне обчислення в лямбда-виразi
                data[i] = Math.Pow(Math.Sin(data[i] / 1000), 2) +
                          Math.Pow(Math.Cos(data[i] / 1000), 2);

                // Виводимо кiлька значень для демонстрацiї
                if (i % 1000000 == 0)
                {
                    Console.WriteLine($"Елемент {i}: {data[i]:F6}");
                }
            });

            sw.Stop();
            Console.WriteLine($"Паралельне виконання з лямбда-виразом: {sw.Elapsed.TotalSeconds:F4} секунд");

            // Перевiряємо тотожнiсть sin^2 + cos^2 = 1
            bool isIdentityValid = true;
            for (int i = 0; i < data.Length; i += 100000)
            {
                if (Math.Abs(data[i] - 1.0) > 1e-10)
                {
                    isIdentityValid = false;
                    Console.WriteLine($"Помилка в тотожностi на iндексi {i}: {data[i]}");
                    break;
                }
            }

            if (isIdentityValid)
            {
                Console.WriteLine("Тотожнiсть sin2(x) + cos2(x) = 1 виконується для всiх перевiрених елементiв");
            }
        }
    }
}
