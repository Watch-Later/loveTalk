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

        static void Main(string[] args)
        {

            ColliderCon.connect();

            var wtf = BTManager.getDevices();
            Console.WriteLine(wtf[0].Name);
            var w = new loveToy(wtf[0]);
            w.connect().Wait();
            w.setVibration(0.1f);
            
            /*
            LuaState = new Lua();
            LuaState.LoadCLRPackage(); // Initialize CLR for lua state 
            //LStateLibaries.File.Setup(LuaState);
            //LStateLibaries.LuaString.Setup(LuaState);
            LuaState.DoString("dofile('lovetalk/preinit.lua')");
            LuaState.DoString("import('loveTalk','loveTalk')"); // Import lovetalk namespace
            LuaState.DoString("dofile('lovetalk/init.lua')");
            callHook = (LuaFunction)LuaState["modhook.Call"];
            */
            Console.ReadLine();
        }
    }
}
