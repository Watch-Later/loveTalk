using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace loveTalk
{
    class Program
    {
        public static Lua LuaState;
        public static LuaFunction callHook;

        public static loveToy[] Toys = new loveToy[8];

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
            Console.WriteLine("Lua OK!");

            Console.Write("Initializing 3D device...");
            GuiController.init();
            Console.WriteLine("OK");


            while (true)
            {
                var colliders = ColliderCon.getColliders();

                for (int i=0; i < Toys.Length; i++)
                {
                    var toy = Toys[i];
                    if (toy != null && toy.Controller != null)
                        toy.Controller.update(colliders, toy);
                }
                GuiController.update();
            }


            Console.ReadLine();
        }
    }
}
