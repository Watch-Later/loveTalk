using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using NLua; 

namespace loveTalk
{
    public class loveColliderCallback
    {
        public ColliderData a;
        public ColliderData b;
        public float minDistance;
        public LuaFunction fCallback; 
    }
    public class loveToyController
    {
        public static Dictionary<string, loveToyController> Controllers = new Dictionary<string, loveToyController>();


        public static void registerController(LuaTable tabl)
        {
            var nc = new loveToyController();
            nc.Name = (string)tabl["Name"];
            var callbacks = (LuaTable)tabl["Callbacks"];
            for (int i=0; i < callbacks.Keys.Count; i++)
            {
                nc.callbacks[i] = new loveColliderCallback();
                var mD = (int)((LuaTable)callbacks[i])["MinDistance"];
                var lC = (LuaFunction)((LuaTable)callbacks[i])["Callback"];
                nc.callbacks[i].minDistance = mD;
                nc.callbacks[i].fCallback = lC;
            }
            
        }

        public Dictionary<int, loveColliderCallback> callbacks = new Dictionary<int, loveColliderCallback>();
        public string Name;

        public void update(ColliderData[] colliders, loveToy Toy)
        {
            foreach (loveColliderCallback cb in callbacks.Values)
            {
               for (int i = 0; i < colliders.Length; i++)
               {
                    var compareCollider = colliders[i];
                    if (compareCollider.name==cb.a.name)                    
                        cb.a = compareCollider;                    
               }

               if (cb.a == null || cb.b == null) // check if both are set
                    continue; // if they aren't, continue. 
                var wasAnyCalled = false;  // no need to multi-tick an "any" collider. 
               
                for (int i = 0; i < colliders.Length; i++)
                {
                    var compareCollider = colliders[i];
                    var dist = 0f;
           
                    if (cb.b.any==true & !wasAnyCalled)
                    {
                        wasAnyCalled = true; 
                        dist = Vector3.Distance(cb.a.position, compareCollider.position);
                        if ( dist < cb.minDistance)
                        {
                            cb.fCallback.Call(cb, cb.a, compareCollider.position, dist, Toy);
                            continue; // nothing left to do. 
                        } 
                    } else
                    {
                        if (compareCollider.name==cb.b.name)
                        {
                            dist = Vector3.Distance(cb.a.position, compareCollider.position);
                            if (dist < cb.minDistance)
                            {
                                cb.fCallback.Call(cb, cb.a, compareCollider.position, dist, Toy);
                            } else
                            {
                                cb.fCallback.Call(cb, cb.a, null, -1, Toy);
                            }
                        }
                    }
                }
                if (!wasAnyCalled==false && cb.b.any == true)
                {
                    cb.fCallback.Call(cb, cb.a, null, -1, Toy);
                }
            }
        }
    }
}
