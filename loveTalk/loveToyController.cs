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
    public class loveColliderCallbackParameters
    {
        public ColliderData a;
        public ColliderData b;           
    }

    public class loveToyController
    {

        public static loveToyController fromLuaTable(LuaTable tabl)
        {
            var nc = new loveToyController();
            nc.Name = (string)tabl["Name"];
            var callbacks = (LuaTable)tabl["Callbacks"];
            for (int i=0; i < callbacks.Keys.Count; i++)
            {
                var table = (LuaTable)callbacks[i + 1];
                nc.Callbacks[i] = new loveColliderCallback();
                var lC = (LuaFunction)(table["Callback"]);
                var nM = (string)(table["Name"]);
                var mD = Convert.ToSingle(table["MinDistance"].ToString());

               
          
                nc.Callbacks[i].minDistance = (float)mD;
                nc.Callbacks[i].fCallback = lC;
                nc.Callbacks[i].Name = nM;
                nc.CallbackData[i] = new loveColliderCallbackParameters();
            }
            return nc;
        }

        public loveToyController copy()
        {
            var nlc = new loveToyController();
            nlc.Callbacks = Callbacks;
            nlc.Name = Name;
            nlc.CallbackData = new Dictionary<int, loveColliderCallbackParameters>();
            for (int i = 0; i < Callbacks.Keys.Count; i++) 
                nlc.CallbackData[i] = new loveColliderCallbackParameters();

            return nlc;
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
                if (col.name == colliderInfoA.name)
                    continue;
                var dist = Vector3.Distance(colliderInfoA.position, col.position) - (colliderInfoA.radius + col.radius);
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
                // Check if both are assigned
                if (cbParam.a == null || cbParam.b == null)
                    continue;

                // check if it has any collider search checked
                if (cbParam.b.any)
                    if (handleWildcardColliderCallback(cbFunc, cbParam, Toy, colliders)) // bool for scope hack lol, indent next line (skip iteration)
                        continue;

                // load collider info 
                var colliderInfoA = ColliderCon.matchName(colliders, cbParam.a.name);
                var colliderInfoB = ColliderCon.matchName(colliders, cbParam.b.name); 

                if (colliderInfoA == null || colliderInfoB == null) // none found 
                    continue; // Skip iteration
                // 
                var radialDistance = (colliderInfoA.radius + colliderInfoB.radius);
                var dist = Vector3.Distance(colliderInfoA.position, colliderInfoB.position);
                if (dist < cbFunc.minDistance) // check if daddy loves me uwu
                    cbFunc.fCallback.Call(Toy, dist, cbFunc, colliderInfoA, colliderInfoB, dist);
                else
                    cbFunc.fCallback.Call(Toy, -1 , cbFunc, colliderInfoA, colliderInfoB, dist);
            }
        }
    }
}
