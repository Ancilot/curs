using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VB_Lab1;
using System.Windows.Forms;
using System.Diagnostics.Metrics;

namespace cursova
{
    public class LL
    {
        int index;
        private ListBox listBox1;
        private ListBox listBox2;
        private ListView listView1;
        List<Token> Tokens;

         //семантика проверки 
        List<string> masType = new List<string>();
        List<string> masOpr = new List<string>();


        // Стеки для построения матрицы вывода
        Stack<Token> stekOper = new Stack<Token>();  // Для операций
        Stack<Token> stekOperand = new Stack<Token>(); // Для первого операнда
        Stack<String> stekResult = new Stack<String>();
        string result = "";
        int tempVarCounter = 0; // Счетчик временных переменных

        // генерация
        string gen = "";
        List<string> ListLong = new List<string>();

        public LL(List<Token> tokens, ListBox listBox1, ListBox listBox2, ListView listView1)
        {
            this.listBox1 = listBox1;
            this.listBox2 = listBox2;
            this.listView1 = listView1;
            Tokens = tokens;
            int x = prog();
        }
        private void Error(int k, int index)
        {
            string errorMessage = $"Синтаксическая ошибка {k} на позиции {index}:";
            switch (k)
            {
                case -1:
                    errorMessage = "Ошибок нет";
                    break;
                case 0:
                    errorMessage += "Ожидалась решетка '#'";
                        break;
                case 1:
                    errorMessage += "Ожидалось ключевое слово 'Dim'";
                    break;
                case 2:
                    errorMessage += "Ожидался идентификатор";
                    break;
                case 3:
                    errorMessage += "Ожидалось ключевое слово 'as'";
                    break;
                case 4:
                    errorMessage += "Ожидалось ключевое слово 'Long' или 'Byte'";
                    break;
                case 5:
                    errorMessage += "Ожидался условный оператор 'If' или индефикатор";
                    break;
                case 6:
                    errorMessage += "Ожидалcя оператор 'End If'";
                    break;
                case 7:
                    errorMessage += "Ожидалось ключевое слово 'If'";
                    break;
                case 8:
                    errorMessage += "Ожидался условный оператор 'End If', 'ElseIf' или 'Else'";
                    break;
                case 9:
                    errorMessage += "Ожидался оператор '='";
                    break;
                case 10:
                    errorMessage += "Ожидались индефикатор или литерал";
                    break;
                case 11:
                    errorMessage += "Ожидался оператор '+', '-', '*', '/', 'Mod'";
                    break;
                case 12:
                    errorMessage += "Ожидалось ключевое слово 'Then'";
                    break;
                case 13:
                    errorMessage += "Ожидалась закрывающая скобка ')'";
                    break;
                case 14:
                    errorMessage += "Ожидалcя индефикатор, литерал или открывающая скобка '('";
                    break;
                case 15:
                    errorMessage += "Ожидался оператор сравнения '<', '<=', '>', '>=', '=', '<>' ";
                    break;
                case 16:
                    errorMessage += "Ожидался условный оператор 'If'";
                    break;
                default:
                    errorMessage += "Неизвестная синтаксическая ошибка";
                    break;
            }
            // Добавляем информацию о текущем токене
            if (index < Tokens.Count)
            {
                errorMessage += $". Обнаружено: {TokenType(Tokens[index].Type)}";
                if (!string.IsNullOrEmpty(Tokens[index].Value))
                {
                    errorMessage += $" '{Tokens[index].Value}'";
                }
            }
            else
            {
                errorMessage += ". Достигнут конец входных данных";
            }

            // Выводим ошибку в ListBox
            if (listBox1 != null)
            {
                listBox1.Items.Add(errorMessage);
            }
            else
            {
                Console.WriteLine(errorMessage);
            }

        }
        private void Error2(int kod, int index)//обработчик ошибок
        {
            string errorMessage = $"Семантическая ошибка #{kod} на позиции {index}: ";

            switch (kod)
            {
                case 2:
                    errorMessage += "переменная уже объявлена";
                    break;
                case 3:
                    errorMessage += "имеется не объявленная переменная";
                    break;
                case 4:
                    errorMessage += "переменная должна быть объявлена и инициализирована до её первого использования";
                    break;
                default:
                    errorMessage += "Неизвестная семантическая ошибка";
                    break;  
            }
            // Выводим ошибку в ListBox
            if (listBox1 != null)
            {
                listBox1.Items.Add(errorMessage);
            }
            else
            {
                Console.WriteLine(errorMessage);
            }
        }
        private string TokenType(string type)
        {
            switch (type)
            {
                case "K":
                    return "ключевое слово";
                case "R":
                    return "разделитель";
                case "I":
                    return "идентификатор";
                case "L":
                    return "Число";
                default:
                    return type;
            }
        }
        public int prog()
        {
            //Прог>::=<спис_опис>#<спис_опер>#$ |Dim
            int kod = -1;
            index = 0;

            kod = spis_opis(ref index);
            if (kod >= 0) return kod;

            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))//#
            {
                Error(0, index); return 0;
            }
            index++;

            kod = spis_oper(ref index);
            if (kod >= 0) return kod;

            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))//#
            {
                Error(0, index); return 0;
            }
            index++;
           // Проверка на выход за пределы
            if (index >= Tokens.Count)
            {
                Error(-1, index); return -1; // или другое значение ошибки
            }

            return -1;
        }

        public int spis_opis(ref int index) 
        {
            //<спис_опис>::=<опис><x>
            int kod = -1;
            kod = opis(ref index);
            if (kod >= 0) return kod;

            kod = X(ref index);
            return kod;
        }

        public int X(ref int index)
        {
            //<x>::= ε                |# | If id
            //< x >::=#<опис><x>      |# | Dim
            int kod = -1;
            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))//#
            {
                Error(0, index); return 0;
            }
            //проверка на эпсилон
            if (!(Tokens[index+1].Type == "K" && Tokens[index+1].Number == 6)) // Dim
           {
            return -1;
           }
            index++;
           
            kod = opis(ref index);
            if (kod >= 0) return kod;

            kod = X(ref index);
            return kod;
        }

        public int opis(ref int index) 
        {
            //< опис >::= Dim < спис_перем > |Dim
            int kod = -1;
            if (!(Tokens[index].Type == "K" && Tokens[index].Number == 6))//Dim
            {
                Error(1, index); return 1;
            }
            index++;

            kod = spis_perem(ref index);
            return kod;

        }
        public int spis_perem(ref int index) 
        {
            // <спис_перем>::=<перем><y> |id
            int kod = -1;
            kod = perem(ref index);
            if (kod >= 0) return kod;

            kod = Y(ref index);
            return kod;
        }
        public int Y(ref int index)
        {
            // <y>::= ε                  |#
            // < y >::=,< перем >< y >   |,
            int kod = -1;

            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 0)) // ,
            {
                return -1;
            }
            index++;
            kod = perem(ref index);
            if (kod >= 0) return kod;

            kod = Y(ref index);
            return kod;
        }

        public int perem(ref int index) 
        {
            //<перем>::= id as <тип> |id
            int kod = -1;
            if (!(Tokens[index].Type == "I")) // id
            {
                Error(2, index); return 2;
            }
            listBox2.Items.Add($"Объявление переменной: {Tokens[index].Value}");
            if (masType.Contains(Tokens[index].Value))
            {
                Error2(2, index); return 2;
            }
            else
            {
                masType.Add(Tokens[index].Value);
            }
            index++;

            if (!(Tokens[index].Type == "K" && Tokens[index].Number == 2))//as
            {
                Error(3, index); return 3;
            }
            index++;

            kod = tip(ref index);
            return kod;

        }
        public int tip(ref int index) 
        {
           
            if ((Tokens[index].Type == "K" && Tokens[index].Number == 1)|| (Tokens[index].Type == "K" && Tokens[index].Number == 0))//long или Byte
            {
                if (Tokens[index].Number == 1) 
                {
                    ListLong.Add(Tokens[index - 2].Value);
                }

                listBox2.Items.Add($" Тип: {Tokens[index].Value}");
                index++;
                return -1;
            }
            else 
            {
                Error(4, index); return 4; // ошибка ожидался лонг или байт
            }
        }

        public int spis_oper(ref int index) 
        {
            //<спис_опер>:=<опер><z>   | If id
             int kod = -1;
            kod = oper(ref index);
            if (kod >= 0) return kod;

            kod = Z(ref index);
            return kod;
        }
        public int Z(ref int index)
        {
            //< z >:= ε           |# | $ End
            //                         ElseIf
            //                         Else
            //<z>:=#<опер><z>     |# | If id
            int kod = -1;
 
            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))//#
            {
                Error(0, index); return 0;
            }
            if (index >= Tokens.Count-1)
            {
                return -1; // или другое значение ошибки
            }

            if (!((Tokens[index + 1].Type == "K" && Tokens[index + 1].Number == 7) || (Tokens[index + 1].Type == "I"))) // If id
            {
                return -1;
            }
            index++;
            kod = oper(ref index);
            if (kod >= 0) return kod;

            kod = Z(ref index);
            return kod;
        }

        public  int oper(ref int index) 
        {
            //<опер>::=<условный>     | If
            //< опер >::=< пр_ар >    | id
            int kod = -1;
            if (Tokens[index].Type == "K" && Tokens[index].Number == 7 ) // If
            {
                kod = yslovn(ref index);
                if (kod >= 0) return kod;
                return -1;
            }
            if (Tokens[index].Type == "I" ) // id
            {
                kod = pr_ar(ref index);
                if (kod >= 0) return kod;
                return -1;
            }

            Error(5, index); return 5; // ожидалось if или id

        }

        public int yslovn(ref int index)
        {
            // <условный>::= If <A><хвост>  | If
            int kod = -1;
            listBox2.Items.Add($"Начало условия");
            if (!(Tokens[index].Type == "K" && Tokens[index].Number == 7))//If
            {
                Error(16, index); return 16;
            }
            listBox2.Items.Add($" Блок: If"); 
            index++;
            kod = A(ref index);
            if (kod >= 0) return kod;

            kod = xvost(ref index);
            if (kod >= 0) return kod;
            return -1;
        }

        public int xvost(ref int index) 
        {
            //<хвост>::= End If                          |End
            //< хвост >::= ElseIf<A> < хвост >           |ElseIf
            //< хвост >::= Else # <спис_опер> # End If   |Else

            int kod = -1;
            if ((Tokens[index].Type == "K" && Tokens[index].Number == 4))//End
            {
                index++;
                if ((Tokens[index].Type == "K" && Tokens[index].Number == 7))//If
                {
                    listBox2.Items.Add($"Конец условия: End If");
                    index++;
                    return -1;
                }
            }
            if ((Tokens[index].Type == "K" && Tokens[index].Number == 9))//ElseIf
            {
                listBox2.Items.Add($"Блок: ElseIf");
                index++;
                kod = A(ref index);
                if (kod >= 0) return kod;

                kod = xvost(ref index);
                if (kod >= 0) return kod;
                return -1;
            }
            if ((Tokens[index].Type == "K" && Tokens[index].Number == 10))//Else
            {
                listBox2.Items.Add($"Блок: Else");
                index++;
                if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))//#
                {
                    Error(0, index); return 0;
                }
                index++;
                kod = spis_oper(ref index);
                if (kod >= 0) return kod;

                if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))//#
                {
                    Error(0, index); return 0;
                }
                index++;
                if ((Tokens[index].Type == "K" && Tokens[index].Number == 4))//End
                {
                    index++;
                    if (!(Tokens[index].Type == "K" && Tokens[index].Number == 7))//If
                    {
                        Error(7, index); return 7; // ожидалось If
                    }
                    listBox2.Items.Add($"Конец условия: End If");
                    index++;
                    return -1;
                }
                 Error(6, index); return 6; // ожидалось End If
            }
            Error(8, index); return 8;// ожидалось End If ElseIf Else
        }
        public int pr_ar(ref int index)
        {  // <пр_ар>::= id= <элем><p>  | id
            int kod = -1;
            if (!(Tokens[index].Type == "I"))//id
            {
                Error(2, index); return 2; // ожидался id
            }
            if (masType.Contains(Tokens[index].Value))
            {
                if (!masOpr.Contains(Tokens[index].Value))
                {
                    masOpr.Add(Tokens[index].Value);
                }
            }
            else
            {
                Error2(3, index); return 3;
            }
            gen += $"{Tokens[index].Value}";
            index++;
            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 15))//=
            {
                Error(9, index); return 9; // ожидалось =
            }
            // Сохраняем операцию присваивания (=)
            stekOper.Push(Tokens[index]);

            gen += $"{Tokens[index].Value}";
            index++;

            kod = elem(ref index);
            if (kod >= 0) return kod;

            kod = P(ref index);
            if (kod >= 0) return kod;
            Token operation1 = stekOper.Pop();
            ListViewItem item1 = new ListViewItem(operation1.Value);
            if (stekOperand.Count > 1)
            {
                Token operand1 = stekOperand.Pop();
                item1.SubItems.Add(operand1.Value);
                Token operand2 = stekOperand.Pop();
                item1.SubItems.Add(operand2.Value);
                // здесь сравнинь операнды и если они разных ипов привести к наибольшему
                if((ListLong.Contains(operand1.Value) || ListLong.Contains(operand2.Value) || (operand1.Type == "L") || (operand2.Type == "L")) && !(((operand1.Type == "L") && (operand2.Type == "L")) || (ListLong.Contains(operand1.Value) && ListLong.Contains(operand2.Value))))
                {
                    listBox2.Items.Add($" Приведение к типу Long");
                    item1.SubItems.Add(result);
                    listView1.Items.Add(item1);
                    Token operation = stekOper.Pop();
                    ListViewItem item = new ListViewItem(operation.Value);
                    item.SubItems.Add(result);
                    listView1.Items.Add(item);
                    listBox2.Items.Add($"           Присваиванье значения: " + gen);
                    gen = "";
                }
                else
                {
                    item1.SubItems.Add(result);
                    listView1.Items.Add(item1);
                    Token operation = stekOper.Pop();
                    ListViewItem item = new ListViewItem(operation.Value);
                    item.SubItems.Add(result);
                    listView1.Items.Add(item);
                    listBox2.Items.Add($"   Присваиванье значения: " + gen);
                    gen = "";
                }
                
            }
            else
            {

                Token operand = stekOperand.Pop();
                item1.SubItems.Add(operand.Value);
                listView1.Items.Add(item1);
                listBox2.Items.Add($"   Присваиванье значения: " + gen);
                gen = "";
            }
            return -1;
        }
        public int P(ref int index) 
        {
            // <p>::= ε                       |#$
            //< p >::= < оп >< элем >         | + - * / Mod
            int kod = -1;
            if (!((Tokens[index].Type == "R" && (Tokens[index].Number == 4 || Tokens[index].Number == 3 || Tokens[index].Number == 1 || Tokens[index].Number == 2))|| (Tokens[index].Type == "K" && Tokens[index].Number == 11))) // + - * / Mod
            {
                return -1;
            }
            result = $"D{tempVarCounter++}";
            kod = op(ref index);
            if (kod >= 0) return kod;
            kod = elem(ref index);
            return kod;
        }

        public int elem(ref int index) 
        {
           // < элем >::= id    | id
           //< элем >::= lit    | lit
            if ((Tokens[index].Type == "I" || (Tokens[index].Type == "L")))//id или lit
            {
                if ((Tokens[index].Type == "I" && !masOpr.Contains(Tokens[index].Value)))
                {
                    Error2(4, index); return 4;
                }
                // Сохраняем первый операнд
                stekOperand.Push(Tokens[index]);
                gen += $"{Tokens[index].Value}";
                index++;
                return -1;
            }
            else 
            { 
                Error(10, index); return 10;// ошибка ожидался индификатор или литерал
            }
        }
        public int op(ref int index) 
        {
            //< оп >::= +     |+
            //< оп >::= -     |-
            //< оп >::= *     |*
            //< оп >::= /     |/
            //< оп >::= Mod   |Mod
           
            if ((Tokens[index].Type == "R" && (Tokens[index].Number == 4 || Tokens[index].Number == 3 || Tokens[index].Number == 1 || Tokens[index].Number == 2)) || (Tokens[index].Type == "K" && Tokens[index].Number == 11))// ошибка ожидались + -* /
            {
                // Добавляем операцию в стек
                stekOper.Push(Tokens[index]);
                gen += $" {Tokens[index].Value} ";
                index++;
                return -1;
            }
             Error(11, index); return 11;// ошибка ожидались + - * / Mod
            
        }
        public int A(ref int index) 
        {
            // <A>::=<E> Then #<спис_опер> #  |Not id lit (
            int kod = -1;
            kod = E(ref index);
            if (kod >= 0) return kod;
            if (!(Tokens[index].Type == "K" && Tokens[index].Number == 8))// then
            {
                Error(12, index); return 12; // ожидалось Then
            }
            listBox2.Items.Add($"  Если {result} = True");
            listBox2.Items.Add($"  Тогда:");

            index++;
            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))// #
            {
                Error(0, index); return 0; // ожидалось #
            }
            index++;

            kod = spis_oper(ref index);
            if (kod >= 0) return kod;

            if (!(Tokens[index].Type == "R" && Tokens[index].Number == 7))// #
            {
                Error(0, index); return 0; // ожидалось #
            }
            index++;

            return -1; 
        }
        public int E(ref int index) 
        {
            // <E>::=<T><n>  | Not id lit (
            int kod = -1;
            kod = T(ref index);
            if (kod >= 0) return kod;
            kod = n(ref index);
            return kod;
        }
        public int n(ref int index)
        {
            //< n >::= ε             |) Then
            //< n >::= Or<T> < n >   |Or
            int kod = -1;
            if (!(Tokens[index].Type == "K" && Tokens[index].Number == 5)) // Or
            {
                return -1;
            }
            //gen += " Или ";
            stekResult.Push(result);
            stekOper.Push(Tokens[index]);
            index++;

            kod = T(ref index);
            if (kod >= 0) return kod;
            stekResult.Push(result);
            kod = n(ref index);

            Token operation1 = stekOper.Pop();
            ListViewItem item1 = new ListViewItem(operation1.Value);
            String operand1 = stekResult.Pop();
            item1.SubItems.Add(operand1);
            String operand2 = stekResult.Pop();
            item1.SubItems.Add(operand2);
            listBox2.Items.Add($" Операция ИЛИ: {operand1} {operation1.Value} {operand2}");
            result = $"D{tempVarCounter++}";
            item1.SubItems.Add(result);
            listView1.Items.Add(item1);
            listBox2.Items.Add($"   Результат операции: {result}");
            return kod;
        }
        public int T(ref int index) 
        {
            // <T>::=<G><m>   | Not id lit (
            int kod = -1;
            kod = G(ref index);
            if (kod >= 0) return kod;
            kod = m(ref index);
            return kod;
        }
        public int m(ref int index) 
        {
            // <m>::= ε              |Or ) Then
            //< m >::= And<G> < m >  |And
            int kod = -1;
            if (!(Tokens[index].Type == "K" && Tokens[index].Number == 12)) // And
            {
                return -1;
            }
            //gen += " И ";
            stekResult.Push(result);
            // Добавляем операцию в стек
            stekOper.Push(Tokens[index]);
            index++;

            kod = G(ref index);
            if (kod >= 0) return kod;
            stekResult.Push(result);
            kod = m(ref index);
            
            Token operation1 = stekOper.Pop();
            ListViewItem item1 = new ListViewItem(operation1.Value);
            String operand1 = stekResult.Pop();
            item1.SubItems.Add(operand1);
            String operand2 = stekResult.Pop();
            item1.SubItems.Add(operand2);
            listBox2.Items.Add($" Операция И: {operand1} {operation1.Value} {operand2}");
            result = $"D{tempVarCounter++}";
            listBox2.Items.Add($"   Результат операции: {result}");
            item1.SubItems.Add(result);
            listView1.Items.Add(item1);
            return kod;
        }

        public int G(ref int index) 
        {
            //<G>::= Not <G>    |Not
            //< G >::= < L >    |id lit(
            int kod = -1;
            if ((Tokens[index].Type == "K" && Tokens[index].Number == 3)) // Not
            {
                //gen += "Не ";
                stekOper.Push(Tokens[index]);
                index++;
                kod = G(ref index);
                if (kod >= 0) return kod;
                Token operation1 = stekOper.Pop();
                listBox2.Items.Add($" Операция отрицания: {operation1.Value} {result}");
                ListViewItem item1 = new ListViewItem(operation1.Value);
                item1.SubItems.Add(result);
                item1.SubItems.Add("");
                result = $"D{tempVarCounter++}";
                listBox2.Items.Add($"    Результат операции: {result}");
                item1.SubItems.Add(result);
                listView1.Items.Add(item1);
                return -1;
            }
            kod = L(ref index);
            if (kod >= 0) return kod;
            return -1; 
        }
        public int L(ref int index) 
        {
            // <L>::= <F><s> |id lit (
            int kod = -1;
            kod = F(ref index);
            if (kod >= 0) return kod;
            kod = s(ref index);
            return kod;
        }
        public int s(ref int index) 
        {
            // < s >::= ε               |And Or ) Then
            // < s >::= < оп_ср >< F >  | < <= > >= = <>
            int kod = -1;

            int op;
            if (!(Tokens[index].Type == "R" && (Tokens[index].Number == 10 || Tokens[index].Number == 11 || Tokens[index].Number == 12 || Tokens[index].Number == 13 || Tokens[index].Number == 14 || Tokens[index].Number == 15))) // < <= > >= = <>
            {
                return -1;
            }
            op = Tokens[index].Number;
            kod = op_sr(ref index);
            if (kod >= 0) return kod;

            // Сохраняем первый операнд
            kod = F(ref index);
            Token operation1 = stekOper.Pop();
            // Сохраняем первый операнд
            listBox2.Items.Add(" Операция сравнения: " + gen);
            ListViewItem item1 = new ListViewItem(operation1.Value);
            Token operand1 = stekOperand.Pop();
            item1.SubItems.Add(operand1.Value);
            Token operand2 = stekOperand.Pop();
            item1.SubItems.Add(operand2.Value);
            result = $"D{tempVarCounter++}";
            listBox2.Items.Add($"    Результат операции: {result}");
            item1.SubItems.Add(result);
            listView1.Items.Add(item1);
            gen = "";
            return kod; 
        }
        public int F(ref int index) 
        {
            //<F>::= id            |id
            //< F >::= lit         |lit
            //< F >::= (< E >)     |(
            int kod = -1;
            if ((Tokens[index].Type == "I" && !(masOpr.Contains(Tokens[index].Value))))
            {
                Error2(4, index); return 4;
            }

            if ((Tokens[index].Type == "I") || (Tokens[index].Type == "L"))//id или lit
            {
                gen += Tokens[index].Value;
                stekOperand.Push(Tokens[index]);
                index++;
                return -1;
            }
            if ((Tokens[index].Type == "R" && Tokens[index].Number == 5))//(
            {
                index++;
                kod = E(ref index);
                if (kod >= 0) return kod;

                if (!(Tokens[index].Type == "R" && Tokens[index].Number == 6))
                {
                    Error(13, index); return 13; // ожидалось )
                }
                index++;
                return -1;
            }
            Error(14, index); return 14; // ожидалось id lit или (
        }
       
        public int op_sr(ref int index) 
        {
            // <оп_ср>::= <
            // < оп_ср >::= <=
            // < оп_ср >::= >
            // < оп_ср >::= >=
            // < оп_ср >::= =
            // < оп_ср >::= <>
            if ((Tokens[index].Type == "R" && (Tokens[index].Number == 10 || Tokens[index].Number == 11 || Tokens[index].Number == 12 || Tokens[index].Number == 13 || Tokens[index].Number == 14 || Tokens[index].Number == 15))) // < <= > >= = <>
            {
                // Добавляем операцию в стек
                gen += Tokens[index].Value;
                stekOper.Push(Tokens[index]);
                index++;
                return -1;
            }
            Error(15, index); return 15;// ожидалось <, <=, >, >=, =, <>
        }

    }
}