using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTasksDemo
{
    class Program
    {
        // Метод для першого завдання
        public static void TaskMethod(object taskNumber)
        {
            int num = (int)taskNumber;
            Console.WriteLine($"Task {num} (ID: {Task.CurrentId}) started");

            for (int i = 0; i < 5; i++)
            {
                // затримка пропорційно ідентифікатору задачі
                int delay = 200 * Task.CurrentId.Value;
                Thread.Sleep(delay);
                Console.WriteLine($"Task {num} (ID: {Task.CurrentId}) step {i}, waited for {delay}ms");
            }

            Console.WriteLine($"Task {num} (ID: {Task.CurrentId}) completed");
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Завдання 1 i 2: Двi паралельнi задачi з очiкуванням WaitAll() ===");

            // створення і запуск двох задач
            Task task1 = new Task(TaskMethod, 1);
            Task task2 = new Task(TaskMethod, 2);

            Console.WriteLine("Starting tasks...");
            task1.Start();
            task2.Start();

            Console.WriteLine($"ID of task1: {task1.Id}");
            Console.WriteLine($"ID of task2: {task2.Id}");

            Task.WaitAll(task1, task2);

            Console.WriteLine("All tasks completed!");
            Console.WriteLine();

            // завдання 3: лямбда-вираз як задача
            Console.WriteLine("=== Завдання 3: Задача у виглядi лямбда-виразу ===");

            Task lambdaTask = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"Lambda task (ID: {Task.CurrentId}) started");

                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(300);
                    Console.WriteLine($"Lambda task step {i}");
                }

                Console.WriteLine($"Lambda task (ID: {Task.CurrentId}) completed");
            });

            lambdaTask.Wait();
            Console.WriteLine("Lambda task finished!");
            Console.WriteLine();

            // завдання 4: Parallel.Invoke з лямбда-виразами
            Console.WriteLine("=== Завдання 4: Parallel.Invoke з лямбда-виразами ===");

            Parallel.Invoke(
                // перший лямбда-вираз
                () =>
                {
                    Console.WriteLine($"First lambda in Invoke (Thread ID: {Thread.CurrentThread.ManagedThreadId}) started");
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(250);
                        Console.WriteLine($"First lambda: step {i}");
                    }
                    Console.WriteLine("First lambda completed");
                },

                // другий лямбда-вираз
                () =>
                {
                    Console.WriteLine($"Second lambda in Invoke (Thread ID: {Thread.CurrentThread.ManagedThreadId}) started");
                    for (int i = 0; i < 4; i++)
                    {
                        Thread.Sleep(200);
                        Console.WriteLine($"Second lambda: step {i}");
                    }
                    Console.WriteLine("Second lambda completed");
                }
            );

            Console.WriteLine("All invoke lambdas completed!");

            Console.WriteLine("\nProgram finished. Press any key to exit.");
            Console.ReadKey();
        }
    }
}