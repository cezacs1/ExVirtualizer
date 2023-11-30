using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using System.Text;

public static class VM_Initalizer
{
    private static List<TypeDef> types = new List<TypeDef>();   

    public static void Init(ModuleDef module)
    {
        foreach (var type in module.GetTypes())
        {
            types.Add(type);
        }

        TypeDef vm_module = AddStaticClassToModule(module);

        var typeModule = ModuleDefMD.Load(typeof(ExVM).Module);
        var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(ExVM).MetadataToken));
        var members = InjectHelper.Inject(typeDef, vm_module/*module.GlobalType*/, module);
        var init = (MethodDef)members.Single(method => method.Name == "Run");
        var init2 = (MethodDef)members.Single(method => method.Name == "RunString");

        foreach (var type in types/*module.GetTypes()*/)
        {
            if (type.Name == "ExVM") continue;

            foreach (var meth in type.Methods)
            {
                // constructor ise atla
                if (meth.Name == ".ctor") continue;
                if (meth.Name == ".cctor") continue;
                if (meth.IsConstructor) continue;

                // statik değilse atla
                if (!meth.IsStatic) continue;

                // winapi & unmanagedexport ise atla
                if (meth.IsUnmanagedExport) continue;
                if (meth.IsPinvokeImpl) continue;

                // method body boşsa atla
                if (!meth.Body.HasInstructions) continue;


                string instructions = string.Empty;

                Console.WriteLine("Method -> " + meth.Name);
                foreach (var ins in meth.Body.Instructions)
                {
                    //Console.WriteLine("instuction -> " + ins);
                    instructions += ins.ToString() + "[?]";

                    //string renamestring = ins.ToString().Substring(0, 7);
                    //meth.Name = renamestring;
                }
                Console.WriteLine("instructions -> \n" + instructions);
                instructions = Encrypt(instructions);

                var met = meth;

                //cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
                string YourString = instructions;
                met.Body.Instructions.Clear();

                Console.WriteLine("met.ReturnType.ToString() -> " + met.ReturnType.ToString());
                
                if (met.ReturnType.ToString() == "System.Void")
                {
                    met.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldstr, YourString)); // Öncelikle stringi yükler
                    //met.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Ldc_I4_0)); // bool değer yükler (true için 1, false için 0)
                    met.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, init)); // Ardından Runner'ı çağırır
                    met.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Ret));
                }
                else
                {
                    met.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldstr, YourString)); // Öncelikle stringi yükler
                    //met.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Ldc_I4_1)); // bool değer yükler (true için 1, false için 0)
                    met.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Call, init2)); // Ardından Runner'ı çağırır
                    met.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Ret));
                }


                foreach (var md in module.GlobalType.Methods)
                {
                    if (md.Name != ".ctor") continue;
                    module.GlobalType.Remove(md);
                    break;
                }
            }
        }

        Console.WriteLine("machine inited");
    }
    
    public static TypeDef AddStaticClassToModule(ModuleDef module)
    {
        // Public static class adını belirleyelim
        string className = "ExVM";

        // Yeni sınıf oluşturulması
        TypeDef newClass = new TypeDefUser(/*"virtualization"*/"", className);
        newClass.Attributes = TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed;
        //module.Types.Add(newClass);
        module.Types.Insert(0, newClass);

        Console.WriteLine("AddStaticClassToModule public static class added to the module.");
        return newClass;
    }

    public static string Encrypt(string text)
    {
        // Stringi byte dizisine dönüştür.
        byte[] byteData = Encoding.UTF8.GetBytes(text);

        // Base64'e dönüştürüp değeri döndür.
        return Convert.ToBase64String(byteData);
    }
}

