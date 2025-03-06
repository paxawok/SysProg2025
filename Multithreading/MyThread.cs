using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Multitreading
{

    class MyThread
    {
        // Описуємо клас потоку
        public long Count;
        public Thread Thrd;
        public double PercentageShare;

        private static bool stop = false;
        private static string currentName;
        private static readonly object lockObj = new object(); // синхронізація між об'єктами
        private Stopwatch stopwatch = new Stopwatch(); // таймер

        // конструктор
        public MyThread(string name)
        {
            Count = 0;
            Thrd = new Thread(Run);
            Thrd.Name = name;
            if (currentName == null)
                currentName = name;
        }

        // описуємо запуск
        void Run()
        {
            stopwatch.Start();
            Console.WriteLine($"Thread {Thrd.Name} is beginning.");

            try
            {
                do
                {
                    Count++;

                    if (Count % 10000000 == 0) 
                    {
                        lock (lockObj)
                        {
                            if (currentName != Thrd.Name)
                            {
                                currentName = Thrd.Name;
                                Console.WriteLine($"In thread {currentName}");
                            }
                        }
                    }
                } while (stop == false && Count < 2000000000);

                // Якщо потік закінчився, інші теж стопаються
                if (!stop)
                {
                    lock (lockObj)
                    {
                        stop = true;
                        Console.WriteLine($"Thread {Thrd.Name} reached count limit and signaled stop.");
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Thread {Thrd.Name} is completed. Time elapsed: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }

}
