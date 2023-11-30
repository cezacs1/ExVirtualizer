using System;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace ExVirtualizer
{
    public class ExVirt
    {
        public static void Protect(string file)
        {
            if (file == "")
            {
                Console.WriteLine("File not found.");
            }
            else
            {
                try
                {
                    string file_to_obf = file;
                    ModuleContext modCtx = ModuleDef.CreateModuleContext();
                    var module = ModuleDefMD.Load(file_to_obf, modCtx);

                    VM_Initalizer.Init(module);

                    if (file_to_obf.EndsWith(".exe"))
                    {
                        var path = file_to_obf.Remove(file_to_obf.Length - 4) + ".ExVM.exe";
                        module.Write(path, new ModuleWriterOptions(module) { Logger = DummyLogger.NoThrowInstance });

                        Console.WriteLine("Saved -> " + path);
                    }

                    //SAVE FILE TO PATH DLL
                    if (file_to_obf.EndsWith(".dll"))
                    {
                        var path = file_to_obf.Remove(file_to_obf.Length - 4) + ".ExVM.dll";
                        module.Write(path, new ModuleWriterOptions(module) { Logger = DummyLogger.NoThrowInstance });

                        Console.WriteLine("Saved -> " + path);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error -> " + ex);
                }
            }

            Console.ReadKey();
        }
    }
}
