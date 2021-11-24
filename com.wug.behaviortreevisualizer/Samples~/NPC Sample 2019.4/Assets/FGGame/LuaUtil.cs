using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WUG.BehaviorTreeVisualizer;
using XLua;

namespace FGGame
{
    static class LuaUtil
    {
        static LuaEnv luaEnv = new LuaEnv();
        static LuaUtil()
        {
            luaEnv.AddLoader((ref string filePath) => {
                var path = Application.dataPath + "/FGGame/Lua/";
                return File.ReadAllBytes(path + filePath.Replace('.', '/') + ".lua");
            });
        }
        
        public static NodeBase CreateTree<T>(T _=null) where T :class
        {
            var name = typeof(T).Name;
            var configtable = LoadConfig(name);
            var obj = luaEnv.DoString($"return require 'TreeCreator'")[0] as LuaTable;
            return obj.Get<LuaFunction>("CreateTree").Call(configtable)[0] as NodeBase;
        }

        public static LuaTable LoadConfig(string config)
        {
            var objs = luaEnv.DoString($"return require 'NodeConfigs.{config}'");
            return objs[0] as LuaTable;
        }
        public static LuaTable LoadConfig<T>(T _ = null) where T : class
        {
            return LoadConfig(typeof(T).Name);
        }
    }
}
