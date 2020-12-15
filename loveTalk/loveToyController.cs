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
        public string Name; 
        public float minDistance;
        public LuaFunction fCallback;
    }
    public struct loveColliderCallbackParameters
    {
        public ColliderData a;
        public ColliderData b;           
    }

    public class loveToyController
    {
        public static Dictionary<string, loveToyController> Controllers = new Dictionary<string, loveToyController>();
        public static loveToyController fromLuaTable(LuaTable tabl)
        {
            var nc = new loveToyController();
            nc.Name = (string)tabl["Name"];
            var callbacks = (LuaTable)tabl["Callbacks"];
            for (int i=0; i < callbacks.Keys.Count; i++)
            {
                nc.Callbacks[i] = new loveColliderCallback();
                var mD = (int)((LuaTable)callbacks[i])["MinDistance"];
                var lC = (LuaFunction)((LuaTable)callbacks[i])["Callback"];
                nc.Callbacks[i].minDistance = mD;
                nc.Callbacks[i].fCallback = lC;
                nc.Callbacks[i].Name = null;
                nc.CallbackData[i] = new loveColliderCallbackParameters();
            }
            return nc;
        }


        public Dictionary<int, loveColliderCallback> Callbacks = new Dictionary<int, loveColliderCallback>();
        public Dictionary<int, loveColliderCallbackParameters> CallbackData = new Dictionary<int, loveColliderCallbackParameters>();
        public string Name;

        private  bool handleWildcardColliderCallback(loveColliderCallback cb,  loveColliderCallbackParameters cbParam, loveToy Toy, ColliderData[] colliders)
        {
            var colliderInfoA = ColliderCon.matchName(colliders, cbParam.a.name);
            if (colliderInfoA == null)
                return true; // hax. 
            for (int i=0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                var dist = Vector3.Distance(colliderInfoA.position, col.position);
                if (dist < cb.minDistance)
                {
                    cb.fCallback.Call(Toy, dist, cb, colliderInfoA, col);
                    return true; 
                }
            }
            cb.fCallback.Call(Toy, -1, cb, colliderInfoA, null);
            return true;
        }

        public void update(ColliderData[] colliders, loveToy Toy)
        {

            for (int i=0; i < Callbacks.Count; i++)
            {
                var cbFunc = Callbacks[i];
                var cbParam = CallbackData[i];

                if (cbParam.a == null || cbParam.b == null)
                    continue;

                if (cbParam.b.any)
                    if (handleWildcardColliderCallback(cbFunc, cbParam, Toy, colliders)) // bool for scope hack lol
                        continue;

                var colliderInfoA = ColliderCon.matchName(colliders, cbParam.a.name);
                var colliderInfoB = ColliderCon.matchName(colliders, cbParam.b.name);

                if (colliderInfoA == null || colliderInfoB == null)
                    continue;

                var dist = Vector3.Distance(colliderInfoA.position, colliderInfoB.position);
                if (dist < cbFunc.minDistance)
                    cbFunc.fCallback.Call(Toy, dist, cbFunc, colliderInfoA, colliderInfoB);
                else
                    cbFunc.fCallback.Call(Toy, -1 , cbFunc, colliderInfoA, colliderInfoB);
            }
        }
    }
}
