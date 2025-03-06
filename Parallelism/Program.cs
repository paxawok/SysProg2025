using System;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTasksDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main Thread is starting.");

            // Завдання 1: затримка 200мс * id задачі
            Task task1 = Task.Factory.StartNew(() =>
            {
                int taskId = Task.CurrentId ?? 1;
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(200 * taskId);
                    Console.WriteLine($"Task {taskId}, counter = {i}");
                }
                Console.WriteLine($"Task {taskId} is done.");
            });

            // Завдання 2: затримка 200мс * id задачі
            Task task2 = Task.Factory.StartNew(() =>
            {
                int taskId = Task.CurrentId ?? 2;
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(200 * taskId);
                    Console.WriteLine($"Task {taskId}, iteration {i}");
                }
            });

            // Очікування завершення двох задач
            Task.WaitAll(task1, task2);

            Console.WriteLine("Tasks 1 and 2 completed.\n");

            // Паралельне виконання за допомогою Invoke()
            Parallel.Invoke(
                () =>
                {
                    Console.WriteLine("Lambda Task 1 is starting.");
                    Thread.Sleep(500);
                    Console.WriteLine("Lambda Task 1 is done.");
                },
                () =>
                {
                    Console.WriteLine("Lambda Task 2 is starting.");
                    Thread.Sleep(700);
                    Console.WriteLine("Lambda Task 2 is done.");
                }
            );

            Console.WriteLine("All tasks done.");
            Console.ReadLine();
        }
    }
}