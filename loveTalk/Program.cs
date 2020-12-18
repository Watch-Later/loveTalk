using System;
using System.Diagnostics;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using NLua;

namespace loveTalk
{
    class Program
    {
        public static Lua LuaState;
        public static LuaFunction callHook;
        public static ColliderData[] colliders; 

        public static loveToy[] Toys = new loveToy[8];
        public static Dictionary<string, loveToyController> controllers = new Dictionary<string, loveToyController>();


        private static void loadControllers()
        {
            var files = Directory.GetFiles("lovetalk/controllers","*.lua");
            for (int i=0; i < files.Length; i++)
            {
                LuaState.DoString("CTRL = {}");
                LuaState.DoFile(files[i]);
                var CTable = (LuaTable)LuaState["CTRL"];
                var data = loveToyController.fromLuaTable(CTable);
                controllers[data.Name] = data; 
            }
        }


        public class CallYourTherapistException : Exception
        {

        }



        static void Main(string[] args)
        {
            Console.Write("Waiting for ColliderCon interface...");
            while (!ColliderCon.connect())
            {
                System.Threading.Thread.Sleep(500); 
            }
            Console.WriteLine("Gotcha!");

            LuaState = new Lua();
            LuaState.LoadCLRPackage(); // Initialize CLR for lua state 
            //LStateLibaries.File.Setup(LuaState);
            //LStateLibaries.LuaString.Setup(LuaState);
            LuaState.DoString("dofile('lovetalk/preinit.lua')");
            LuaState.DoString("import('loveTalk','loveTalk')"); // Import lovetalk namespace
            LuaState.DoString("dofile('lovetalk/init.lua')");
            callHook = (LuaFunction)LuaState["modhook.Call"];
            loadControllers();
            Debug.WriteLine("Lua OK!");


            Debug.Write("Initializing 3D device...");
            GuiController.init();
            Debug.WriteLine("OK");

            var fc = 0;
            while (true)
            {
                fc++;
                if (fc > 3)
                {
                    colliders = ColliderCon.getColliders();

                    for (int i = 0; i < Toys.Length; i++)
                    {
                        var toy = Toys[i];
                        if (toy != null && toy.Controller != null)
                            toy.Controller.update(colliders, toy);
                    }
                    fc = 0;
                }

                GuiController.update();
            }


  
        }
    }
}
