using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VB_Lab1;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cursova
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void AnalisText()
        {
            listBox1.Items.Clear();
            string[] lines = textBox1.Lines;
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listView1.Items.Clear();



            List<Token> Tokens = new List<Token>();

            for (int i = 0; i < lines.Length; ++i)
            {
                // Передаем listBox2 для вывода ошибок
                Tokens.AddRange(LexAnakiz.TokensString(lines[i], listBox2));
            }


            int j = 0;
            for (int i = 0; i < Tokens.Count; ++i)
            {
                listBox1.Items.Add($" {j} : [{Tokens[i].Value} " +
                    $". {Tokens[i].Type}, {Tokens[i].Number} ]");
                j++;
            }


            // Выполнение синтаксического анализа
            LL LL = new LL(Tokens, listBox2, listBox3, listView1);

            LexAnakiz.clins();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AnalisText();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Выберите файл для анализа";
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string text = File.ReadAllText(openFileDialog.FileName);

                    textBox1.Text = text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке файла: " + ex.Message,
                                  "Ошибка", MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                }
            }
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
