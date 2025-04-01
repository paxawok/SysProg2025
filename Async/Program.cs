using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsyncProgrammingDemo
{
    public partial class Form1 : Form
    {
        // Об'єкт для керування скасуванням асинхронної операції
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            // Деактивуємо кнопку Start під час виконання операції
            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            lblResult.Text = "Операція виконується...";
            progressBar.Value = 0;
            lblProgress.Text = "0%";

            // Створюємо новий об'єкт для відміни операції
            cts = new CancellationTokenSource();

            try
            {
                // Створюємо об'єкт для відслідковування прогресу
                IProgress<int> progress = new Progress<int>((percentComplete) =>
                {
                    // Оновлюємо UI з відсотком виконання
                    lblProgress.Text = $"{percentComplete}%";
                    progressBar.Value = percentComplete;
                });

                // Викликаємо асинхронну операцію і очікуємо результат
                int result = await PerformLongOperation(100, progress, cts.Token);
                lblResult.Text = $"Результат: {result}";
            }
            catch (OperationCanceledException)
            {
                // Операція була скасована користувачем
                lblResult.Text = "Операція скасована!";

                // Не показувати діалог про помилку у режимі відлагодження
                // Цей виняток є очікуваним при скасуванні
            }
            catch (Exception ex)
            {
                // Сталася помилка під час виконання
                lblResult.Text = $"Помилка: {ex.Message}";
            }
            finally
            {
                // Повертаємо UI в початковий стан
                btnStart.Enabled = true;
                btnCancel.Enabled = false;
                cts?.Dispose();
                cts = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Скасовуємо поточну операцію
            cts?.Cancel();
            btnCancel.Enabled = false;
        }

        /// <summary>
        /// Метод, що імітує довготривалу операцію з обчисленнями
        /// </summary>
        /// <param name="iterations">Кількість ітерацій</param>
        /// <param name="progress">Інтерфейс для повідомлення про хід виконання</param>
        /// <param name="token">Токен скасування</param>
        /// <returns>Результат виконання</returns>
        private Task<int> PerformLongOperation(int iterations, IProgress<int> progress, CancellationToken token)
        {
            return Task.Run(() =>
            {
                int result = 0;

                for (int i = 1; i <= iterations; i++)
                {
                    // Перевіряємо, чи не скасовано операцію
                    token.ThrowIfCancellationRequested();

                    // Виконуємо якісь "важкі" обчислення
                    Thread.Sleep(100); // Імітуємо довгу операцію
                    result += i;

                    // Повідомляємо про прогрес
                    int percentComplete = (int)((double)i / iterations * 100);
                    progress.Report(percentComplete);
                }

                return result;
            }, token);
        }

        // Код дизайнера форми
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 15);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(120, 30);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(138, 15);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 75);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(350, 25);
            this.progressBar.TabIndex = 2;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(12, 55);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(21, 17);
            this.lblProgress.TabIndex = 3;
            this.lblProgress.Text = "0%";
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Location = new System.Drawing.Point(12, 110);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(59, 17);
            this.lblResult.TabIndex = 4;
            this.lblResult.Text = "Готово до запуску";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Location = new System.Drawing.Point(270, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 57);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Test of controls response";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(103, 21);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.Text = "radioButton1";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Checked = true;
            this.radioButton2.Location = new System.Drawing.Point(115, 19);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(103, 21);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "radioButton2";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(6, 42);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(103, 21);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "radioButton3";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 153);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Async programming";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}