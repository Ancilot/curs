using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace VB_Lab1
{
    public class Token
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public int Number { get; set; } //Номер токена

        public Token(string type, string value, int number)
        {
            Type = type;
            Value = value;
            Number = number;

        }
        public override string ToString()
        {
            return $"({Type}, {Number})";
        }
    }

    internal class LexAnakiz
    {
        enum State { S, I, H, R } // Состояния: S- начальное, I- идентификатор, H- число, R- разделитель

        // Проверка, является ли символ английской буквой
        private static bool English_Letter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
        // Проверка, является ли символ цифрой
        private static bool Nubmer(char c)
        {
            return (c >= '0' && c <= '9');
        }

        // Проверка, является ли символ буквой или цифрой
        private static bool IsLetterOrDigit(char c)
        {
            return English_Letter(c) || Nubmer(c);
        }

        // Проверка, является ли символ пробелом
        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }
        public static readonly List<string> Ident = new List<string>();
        public static readonly List<string> Lit = new List<string>();

        public void Error(int kod, int index, ListBox listBox3)//обработчик ошибок
        {
            string errorMessage = $"Лексическая ошибка #{kod} на позиции {index}: ";

            switch (kod)
            {
                case 1:
                    errorMessage = "Лексических ошибок нет";
                    break;
            }
            // Выводим ошибку в ListBox3
            if (listBox3 != null)
            {
                listBox3.Items.Add(errorMessage);
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }

        public static List<Token> TokensString(string inputString, ListBox ListBox)
        {

            List<Token> Tokens = new List<Token>(); // Список для хранения токеноd
                                                    // Таблицы для хранения идентификаторов и литералов

            //                      0    1       2      3       4     5     6     7       8        9       10    11       12
            string[] SlWord = { "Byte", "Long", "as", "Not", "End", "Or", "Dim", "If", "Then", "ElseIf", "Else", "Mod", "And" };
            //                         0    1   2    3    4    5   6     7    
            string[] SingleRazdel = { ",", "*", "/", "-", "+", "(", ")","#"}; // Одиночные разделители
            //                       10    11    12    13    14  15
            string[] SostRazdel = { "<=", ">=", "<>", ">", "<", "=" }; // Составные разделители



            State state = State.S; // Начальное состояние
            string buffer = "";
            int repeatState = 1;

            if (inputString=="") return Tokens;
            int g =0;
            int n = inputString.Length;
            while (g<n && inputString[g] == ' ' ) { g++; }
            if (g==n) return Tokens;
            
            for (int i = 0; i < n; i++)
            {
                char c = inputString[i]; // Текущий символ
                if (repeatState == 1) repeatState = 0;
                for (int j = -1; j < repeatState; ++j)
                {

                    switch (state)
                    {
                        case State.S:
                            if (English_Letter(c))
                            {
                                state = State.I; // Переход в состояние идентификатора
                                buffer += c;
                            }
                            else if (Nubmer(c))
                            {
                                state = State.H;
                                buffer += c;
                            }
                            else if (IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else if (SingleRazdel.Contains(c.ToString()))
                            {
                                Tokens.Add(new Token("R", c.ToString(), Array.IndexOf(SingleRazdel, c.ToString())));
                            }
                            else if (SostRazdel.Contains(c.ToString()))
                            {
                                state = State.R;
                                buffer += c;
                            }
                            else 
                            {
                                ListBox.Items.Add($"Лексическая ошибка: неизвестный символ {c}");
                            }
                            break;
                        case State.I:
                            if (IsLetterOrDigit(c))
                            {
                                buffer += c;
                            }
                            else
                            {
                                // substring обрезать первые 20 символов
                                if (buffer.Length > 20)
                                {
                                    buffer = buffer.Substring(0, 20);
                                }
                                if (SlWord.Contains(buffer))
                                {
                                    Tokens.Add(new Token("K", buffer, Array.IndexOf(SlWord, buffer)));
                                }
                                else
                                {
                                    if (!Ident.Contains(buffer))
                                    {
                                        Ident.Add(buffer);
                                    }
                                    Tokens.Add(new Token("I", buffer, Ident.IndexOf(buffer)));
                                } // Добавление идентификатора или ключевого слова
                                buffer = "";
                                state = State.S;
                                i--; // Возврат к текущему символу для повторной обработки
                            }
                            break;

                        case State.H:
                            if (Nubmer(c))
                            {
                                buffer += c;
                            }
                            else
                            {
                              if (!Lit.Contains(buffer))
                              {
                                 Lit.Add(buffer);
                              }
                                Tokens.Add(new Token("L", buffer, Lit.IndexOf(buffer)));
                                buffer = "";
                                state = State.S;
                                i--;
                            }
                            break;

                        case State.R:
                            if (SostRazdel.Contains(buffer + c))
                            {
                                buffer += c; // Накопление составного разделителя
                            }
                            else
                            {
                                
                                Tokens.Add(new Token("R", buffer, Array.IndexOf(SostRazdel, buffer) + 10)); 
                                buffer = "";
                                state = State.S;
                                i--;
                            }
                            break;

                    }

                }

            }
            if (buffer != "")
            {
                switch (state)
                {
                    case State.I:
                        if (SlWord.Contains(buffer))
                        {
                            Tokens.Add(new Token("K", buffer, Array.IndexOf(SlWord, buffer)));
                        }
                        else
                        {
                            if (!Ident.Contains(buffer))
                            {
                                Ident.Add(buffer);
                            }
                            Tokens.Add(new Token("I", buffer, Ident.IndexOf(buffer)));
                        }

                        break;
                    case State.H:

                        if (ulong.TryParse(buffer, out ulong number))
                        {
                            if (number > 65535)
                            {
                                ListBox.Items.Add($"Лексическая ошибка: число {buffer} превышает допустимое значение 65535");break;
                            }

                        }
                        if (!Lit.Contains(buffer))
                        {
                            Lit.Add(buffer);
                        }
                        Tokens.Add(new Token("L", buffer, Lit.IndexOf(buffer)));
                        break;
                    case State.R:

                        if (buffer.Length > 0)
                            Tokens.Add(new Token("R", buffer, Array.IndexOf(SostRazdel, buffer) + 10));
                        else
                            Tokens.Add(new Token("R", buffer, Array.IndexOf(SingleRazdel, buffer)));
                        break; // R-разделитель

                }
            }
            buffer = "#";
            Tokens.Add(new Token("R", buffer, Array.IndexOf(SingleRazdel, buffer)));
            // Формируем строку с токенами
            return Tokens; // Возвращаем строку с токенами

        }

        public static void clins()
        {
            Ident.Clear();
            Lit.Clear();

        }

    }
}



