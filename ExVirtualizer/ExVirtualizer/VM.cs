using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;
using static ExVM.VM.VM_Data;
using static ExVM.VM.ParameterSystem.ParamererTypes;
using static ExVM.VM.Errors;
using static ExVM.VM.Filters;
using static ExVM.VM.Worker;
using static ExVM.VM.OpCodes;
using static ExVM.VM.Execute;
using static ExVM.VM.ParameterSystem;
using static ExVM.VM.Search;


public class ExVM
{
    public class Virtualization
    {
        public Virtualization()
        {

        }

        public static void Run(string values)
        {
            new Thread(() => // programın kapanmaması için, test yapılırken nelerin olduğunu görmeliyiz.
            {
                while (true) { }
            }).Start();

            new VM().RunVM(values);
        }

        public static string RunString(string values)
        {
            new Thread(() => // programın kapanmaması için, test yapılırken nelerin olduğunu görmeliyiz.
            {
                while (true) { }
            }).Start();

            return new VM().RunVM(values);
        }
    }

    public class VM
    {
        public VM()
        {

        }

        public string RunVM(string values)
        {
            if (values == null || values == "")
            {
                NullOpcodeError();
                return "";
            }
            if (values == "InvalidOpcode")
            {
                InvalidOpcodeError();
                return "";
            }
            Console.WriteLine("[*] runner working");

            values = Decrypt(values);
            

            return RunFilter(values);
        }

        public static string Decrypt(string text)
        {
            byte[] byteData = Convert.FromBase64String(text);

            return Encoding.UTF8.GetString(byteData);
        }

        public static string RunFilter(string values)
        {
            return Filter(values);
        }

        public class Filters
        {
            public static string Filter(string values)
            {
                int index = 0;
                oldvaluelist.Clear();

                string[] parcalar = SplitValues(values);

                string amk = "";

                foreach (string parca in parcalar)
                {
                    if (string.IsNullOrWhiteSpace(parca)) continue;
                    index++;

                    string opcode = SplitOpCodes(parca);
                    string targetmethod = null;
                    if (opcode.Contains("call"))
                    {
                        targetmethod = SplitMethod(parca);
                    }
                    
                    Console.WriteLine(opcode);
                    amk = RunWorker(opcode, index, targetmethod);
                }

                return amk;
            }

            public static string[] SplitValues(string values)
            {
                return values.Split(new string[] { "[?]" }, StringSplitOptions.None);
            }

            public static string SplitMethod(string parca)
            {
                string[] parcalar = Regex.Split(parca, "::");
                if (parcalar.Length > 1)
                {
                    string istenenParca = parcalar[1].Trim();
                    return istenenParca;
                }
                return parca;
            }

            public static string ReplaceMethodName(string parantezli)
            {
                int index = parantezli.IndexOf('(');

                if (index != -1)
                {
                    string parantezsiz = parantezli.Substring(0, index);
                    // result şimdi "sadc" olacak

                    return parantezsiz;
                }

                return null;
            }
            
            public static string SplitOpCodes(string parca)
            {
                string[] parcalar = parca.Split(':');
                string istenenParca = parcalar[1].Trim();
                return istenenParca;
            }       

            public static string SplitString(string value)
            {
                string metin = value;

                // İlk tırnak işaretinin index'ini bul
                int ilkTirnakIndex = metin.IndexOf('"');
                // Son tırnak işaretinin index'ini bul
                int sonTirnakIndex = metin.LastIndexOf('"');

                // İlk ve son tırnak işaretlerinin arasındaki kısmı al
                string istenenParca = metin.Substring(ilkTirnakIndex + 1, sonTirnakIndex - ilkTirnakIndex - 1);

                return istenenParca;
            }

            public static string SplitCall(string value)
            {
                // İlgili ifadeyi ayıklamak için bir diziyle bölüyoruz
                string[] parts = value.Split(' '); // Boşluklara göre bölmek

                // İlgili ifadenin sondaki iki kısmını almak
                string method = /*parts[parts.Length - 2] + " " + */parts[parts.Length - 1];

                return method;
            }

            public static string SplitIntValue(string opcodesrt)
            {
                string[] parcalar = opcodesrt.Split(' '); // Boşluk karakterine göre böler
                return parcalar[1];
            }
        }

        public class Search
        {
            public static string SearchFirstString()
            {
                for (int i = oldvaluelist.Count - 1; i >= 0; i--)
                {
                    var data = oldvaluelist[i];
                    var opcode = data.opcode;

                    //Console.WriteLine("opcode -> " + opcode);

                    if (opcode == LDSTR)
                    {
                        return data.operand;
                    } 
                }

                return null;
            }

            public static string SearchFirstToReturnValue(ValueData returndata)
            {
                for (int i = oldvaluelist.Count - 1; i >= 0; i--)
                {
                    var data = oldvaluelist[i];
                    var opcode = data.opcode;

                    //Console.WriteLine("opcode -> " + opcode);

                    if (opcode == returndata.opcode)
                    {
                        return returndata.operand;
                    }
                }

                return null;
            }

            public static ValueData SearchFirstReturnValueToAdd()
            {
                for (int i = oldvaluelist.Count - 1; i >= 0; i--)
                {
                    var data = oldvaluelist[i];
                    var opcode = data.opcode;

                    if (data.opcode == ADD) continue;

                    if (data.opcode == LDLOC_0 || data.opcode == LDLOC_1 || data.opcode == LDLOC_2 || data.opcode == LDLOC_3 || data.opcode == LDLOC_S)
                    {
                        Console.WriteLine("SearchFirstReturnValue.data.opcode -> " + opcode);
                        Console.WriteLine("SearchFirstReturnValue.data -> " + data);
                        Console.WriteLine("SearchFirstReturnValue.data.operand -> " + data.operand);

                        return data;
                    }

                    return new ValueData();
                }

                return new ValueData();
            }
        }

        public class ParameterSystem
        {
            public static string FindParameters(string value)
            {
                return value.Split('(', ')')[1];
            }

            public static object[] ParameterFilter(string value)
            {
                string[] splitted = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                List<object> parameters = new List<object>();

                foreach (string item in splitted)
                {
                    if (item == _String)
                    {
                        //parameters.Add("ADDEDSTRING");
                        parameters.Add(SearchFirstString());
                    }
                    else if (item == _Int32)
                    {
                        parameters.Add(500);
                    }
                    else if (item == _Boolean)
                    {
                        parameters.Add(true);
                    }
                }

                if (parameters.Count == 0) return null;

                return parameters.ToArray();
            }

            public static object[] GetParameters(string value)
            {

                return null;
            }

            public struct ParamererTypes
            {
                public static string _String = "System.String";
                public static string _Int32 = "System.Int32";
                public static string _Boolean = "System.Boolean";
            }
        }

        public class Worker
        {
            public static string RunWorker(string opcodesrt, int index, string targetmethod)
            {
                if (opcodesrt == NOP)
                {
                    Execute_NOP(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDSTR))
                {
                    Execute_LDSTR(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(CALL))
                {
                    Execute_CALL(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(ADD))
                {
                    Execute_ADD(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_0))
                {
                    Execute_LDCI4_0(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_1))
                {
                    Execute_LDCI4_1(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_2))
                {
                    Execute_LDCI4_2(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_3))
                {
                    Execute_LDCI4_3(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_4))
                {
                    Execute_LDCI4_4(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_5))
                {
                    Execute_LDCI4_5(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_6))
                {
                    Execute_LDCI4_6(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_7))
                {
                    Execute_LDCI4_7(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_8))
                {
                    Execute_LDCI4_8(opcodesrt, index, targetmethod);
                }
                else if (opcodesrt.Contains(LDCI4_S))
                {
                    Execute_LDCI4_S(opcodesrt, index, targetmethod);
                }

                return Search.SearchFirstString();
            }
        }

        public class Execute
        {
            public static void Execute_NOP(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_NOP working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = NOP;
                oldvalue.operand = null;
                oldvalue.index = index;

                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDSTR(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDSTR working");

                string value = SplitString(opcodesrt);

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDSTR;
                oldvalue.operand = value;
                oldvalue.index = index;

                oldvaluelist.Add(oldvalue);

                /*
                Console.WriteLine("LDSTR oldvalue.opcode -> " + oldvalue.opcode);
                Console.WriteLine("LDSTR oldvalue.operand -> " + oldvalue.operand);
                Console.WriteLine("LDSTR oldvalue.index -> " + oldvalue.index);
                */
            }

            public static void Execute_CALL(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_CALL working");

                string GetType = SplitCall(opcodesrt); // opcodesrt içindeki type çağrısını al

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = CALL;
                oldvalue.operand = GetType; // veya yöntem adı
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);

                /*
                Console.WriteLine("Execute_CALL opcodesrt -> " + opcodesrt);
                Console.WriteLine("Execute_CALL index -> " + index);
                Console.WriteLine("Execute_CALL targetmethod -> " + targetmethod);
                Console.WriteLine("Execute_CALL GetType -> " + GetType);
                */

                string methodname = ReplaceMethodName(targetmethod);
                Console.WriteLine("METHODNAME = " + methodname);
                string methodparameter = FindParameters(targetmethod);
                object[] parameters = ParameterFilter(methodparameter);

                try
                {
                    foreach (var param in parameters)
                    {
                        Console.WriteLine("params -> " + param);
                    }
                }
                catch { }


                Type type = Type.GetType(GetType);
                MethodInfo method = type.GetMethod(methodname); // Metot adı buraya yazılmalı
                //method.Invoke(null, new object[] { 10, "example" });
                method.Invoke(null, parameters);

                Console.WriteLine("\n");
                //Console.ReadKey();
            }

            public static void Execute_ADD(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_ADD working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = ADD;
                oldvalue.operand = null;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);

                // Önceki değerleri toplamak için bir değişken tanımla
                int sum = 0;

                // Önceki değerleri dolaşarak int değerleri topla
                foreach (var data in oldvaluelist)
                {
                    // Eğer değer null değilse ve operandı null değilse ve operand bir integer ise toplama ekle
                    if (!string.IsNullOrEmpty(data.operand) && int.TryParse(data.operand, out int value))
                    {
                        sum += value;
                    }
                }

                string result = sum.ToString();

                Console.WriteLine("Toplam: " + result);
            }

            public static void Execute_LDCI4_0(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_0 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_0;
                oldvalue.operand = 0.ToString();
                Console.WriteLine("Execute_LDCI4_0.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_1(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_1 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_1;
                oldvalue.operand = 1.ToString();
                Console.WriteLine("Execute_LDCI4_1.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_2(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_2 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_2;
                oldvalue.operand = 2.ToString();
                Console.WriteLine("Execute_LDCI4_2.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_3(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_3 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_3;
                oldvalue.operand = 3.ToString();
                Console.WriteLine("Execute_LDCI4_3.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_4(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_4 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_4;
                oldvalue.operand = 4.ToString();
                Console.WriteLine("Execute_LDCI4_4.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_5(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_5 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_5;
                oldvalue.operand = 5.ToString();
                Console.WriteLine("Execute_LDCI4_5.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_6(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_6 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_6;
                oldvalue.operand = 6.ToString();
                Console.WriteLine("Execute_LDCI4_6.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_7(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_7 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_7;
                oldvalue.operand = 7.ToString();
                Console.WriteLine("Execute_LDCI4_7.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_8(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_8 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_8;
                oldvalue.operand = 8.ToString();
                Console.WriteLine("Execute_LDCI4_8.operandvalue -> " + oldvalue.operand);

                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_LDCI4_S(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_LDCI4_S working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = LDCI4_S;

                string operandvalue = SplitIntValue(opcodesrt);
                Console.WriteLine("Execute_LDCI4_S.operandvalue -> " + operandvalue);

                oldvalue.operand = operandvalue;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_STLOC_0(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_STLOC_0 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = STLOC_0;
                oldvalue.operand = null;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_STLOC_1(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_STLOC_1 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = STLOC_1;
                oldvalue.operand = null;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_STLOC_2(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_STLOC_2 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = STLOC_2;
                oldvalue.operand = null;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_STLOC_3(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_STLOC_3 working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = STLOC_3;
                oldvalue.operand = null;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

            public static void Execute_STLOC_S(string opcodesrt, int index, string targetmethod)
            {
                Console.WriteLine("Execute_STLOC_S working");

                ValueData oldvalue = new ValueData();
                oldvalue.opcode = STLOC_S;
                oldvalue.operand = null;
                oldvalue.index = index;
                oldvaluelist.Add(oldvalue);
            }

        }

        public class OpCodes
        {
            public static string NOP = "nop";
            public static string LDSTR = "ldstr";
            public static string CALL = "call";
            public static string ADD = "add";

            public static string LDCI4_0 = "ldc.i4.0";
            public static string LDCI4_1 = "ldc.i4.1";
            public static string LDCI4_2 = "ldc.i4.2";
            public static string LDCI4_3 = "ldc.i4.3";
            public static string LDCI4_4 = "ldc.i4.4";
            public static string LDCI4_5 = "ldc.i4.5";
            public static string LDCI4_6 = "ldc.i4.6";
            public static string LDCI4_7 = "ldc.i4.7";
            public static string LDCI4_8 = "ldc.i4.8";
            public static string LDCI4_S = "ldc.i4.s";

            public static string STLOC_0 = "stloc.0";
            public static string STLOC_1 = "stloc.1";
            public static string STLOC_2 = "stloc.2";
            public static string STLOC_3 = "stloc.3";
            public static string STLOC_S = "stloc.s";

            public static string LDLOC_0 = "ldloc.0";
            public static string LDLOC_1 = "ldloc.1";
            public static string LDLOC_2 = "ldloc.2";
            public static string LDLOC_3 = "ldloc.3";
            public static string LDLOC_S = "ldloc.s";
        }

        public struct VM_Data
        {
            public static List<ValueData> oldvaluelist = new List<ValueData>();
            //public static /*dynamic*/ValueData oldvalue = new ValueData();
        }

        public struct ValueData
        {
            public string opcode;
            public string operand;
            public int index;
        }

        public class Errors
        {
            public static void NullOpcodeError()
            {
                MessageBox.Show("no opcode found in method body", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            public static void InvalidOpcodeError()
            {
                MessageBox.Show("invalid opcode", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
    }
}
