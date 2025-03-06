using System;
using System.Threading;
using System.Diagnostics;

namespace Multitreading
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Thread Priority Distribution Test - Variant 4");
            Console.WriteLine("6 threads with priorities: Medium, Below Normal, Above Normal, Highest, Lowest, Medium");
            Console.WriteLine("----------------------------------------------------------------------");

            // створюємо 6 потоків 
            MyThread mt1 = new MyThread("Thread 1 (Normal)");
            MyThread mt2 = new MyThread("Thread 2 (Below Normal)");
            MyThread mt3 = new MyThread("Thread 3 (Above Normal)");
            MyThread mt4 = new MyThread("Thread 4 (Highest)");
            MyThread mt5 = new MyThread("Thread 5 (Lowest)");
            MyThread mt6 = new MyThread("Thread 6 (Normal)");

            mt1.Thrd.Priority = ThreadPriority.Normal;
            mt2.Thrd.Priority = ThreadPriority.BelowNormal;
            mt3.Thrd.Priority = ThreadPriority.AboveNormal;
            mt4.Thrd.Priority = ThreadPriority.Highest;
            mt5.Thrd.Priority = ThreadPriority.Lowest;
            mt6.Thrd.Priority = ThreadPriority.Normal;

            // починаємо відлік 
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();

            mt1.Thrd.Start();
            mt2.Thrd.Start();
            mt3.Thrd.Start();
            mt4.Thrd.Start();
            mt5.Thrd.Start();
            mt6.Thrd.Start();

            mt1.Thrd.Join();
            mt2.Thrd.Join();
            mt3.Thrd.Join();
            mt4.Thrd.Join();
            mt5.Thrd.Join();
            mt6.Thrd.Join();

            totalTime.Stop();

            // загальна сума і відсотки
            long totalCounts = mt1.Count + mt2.Count + mt3.Count + mt4.Count + mt5.Count + mt6.Count;

            mt1.PercentageShare = (double)mt1.Count / totalCounts * 100;
            mt2.PercentageShare = (double)mt2.Count / totalCounts * 100;
            mt3.PercentageShare = (double)mt3.Count / totalCounts * 100;
            mt4.PercentageShare = (double)mt4.Count / totalCounts * 100;
            mt5.PercentageShare = (double)mt5.Count / totalCounts * 100;
            mt6.PercentageShare = (double)mt6.Count / totalCounts * 100;

            // результат
            Console.WriteLine("\n--- RESULTS ---");
            Console.WriteLine($"Total execution time: {totalTime.ElapsedMilliseconds} ms");
            Console.WriteLine($"Total iterations across all threads: {totalCounts:N0}");
            Console.WriteLine("\nCPU time distribution by thread priority:");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"{mt1.Thrd.Name} - Count: {mt1.Count:N0}, Share: {mt1.PercentageShare:F2}%");
            Console.WriteLine($"{mt2.Thrd.Name} - Count: {mt2.Count:N0}, Share: {mt2.PercentageShare:F2}%");
            Console.WriteLine($"{mt3.Thrd.Name} - Count: {mt3.Count:N0}, Share: {mt3.PercentageShare:F2}%");
            Console.WriteLine($"{mt4.Thrd.Name} - Count: {mt4.Count:N0}, Share: {mt4.PercentageShare:F2}%");
            Console.WriteLine($"{mt5.Thrd.Name} - Count: {mt5.Count:N0}, Share: {mt5.PercentageShare:F2}%");
            Console.WriteLine($"{mt6.Thrd.Name} - Count: {mt6.Count:N0}, Share: {mt6.PercentageShare:F2}%");

            // перевірка
            double totalPercentage = mt1.PercentageShare + mt2.PercentageShare + mt3.PercentageShare +
                                    mt4.PercentageShare + mt5.PercentageShare + mt6.PercentageShare;
            Console.WriteLine($"\nSum of all percentages: {totalPercentage:F2}% (should be 100%)");

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}0