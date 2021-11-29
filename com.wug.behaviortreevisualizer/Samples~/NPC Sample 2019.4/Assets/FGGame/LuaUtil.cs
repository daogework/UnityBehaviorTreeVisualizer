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
        static LuaEnv lua = new LuaEnv();
        static LuaUtil()
        {
            var s = @"function ReloadModule(name)
                        local status, err = xpcall(function()
                            package.loaded[name] = nil
                            return require(name)
                        end, debug.traceback)
                        if status then
                            return err
                        else
                            print(err)
                        end
                    end";
            lua.DoString(s);
            lua.AddLoader((ref string filePath) => {
                var path = Application.dataPath + "/FGGame/Lua/";
                return File.ReadAllBytes(path + filePath.Replace('.', '/') + ".lua");
            });
        }

        //public static void Reload()
        //{
            

        //}
        
        public static NodeBase CreateTree<T>(T _=null) where T :class
        {
            var name = typeof(T).Name;
            return CreateTree(name);
        }

        public static NodeBase CreateTree(string name)
        {
            var configtable = LoadConfig(name);
            var obj = lua.DoString($"return ReloadModule 'TreeCreator'")[0] as LuaTable;
            return obj.Get<LuaFunction>("CreateTree").Call(configtable)[0] as NodeBase;
        }

        public static LuaTable LoadConfig(string config)
        {
            var objs = lua.DoString($"return ReloadModule 'NodeConfigs.{config}'");
            return objs[0] as LuaTable;
        }
        public static LuaTable LoadConfig<T>(T _ = null) where T : class
        {
            return LoadConfig(typeof(T).Name);
        }

        class LuaDrawData : INodeDrawData
        {
            public string name { get; set; }
            public NodeBase BehaviorTree { get; set; }
        }

        [TreeDatasGetter]
        public static IList<INodeDrawData> GetTreeDatas()
        {
            var datas = new List<INodeDrawData>();
            var path = Application.dataPath + "/FGGame/Lua/NodeConfigs";
            var dirinfo = new DirectoryInfo(path);
            var files = dirinfo.GetFiles();
            foreach (var item in files)
            {
                if (item.Name.EndsWith(".lua"))
                {
                    var name = item.Name.Replace(".lua", "");
                    datas.Add(new LuaDrawData() { name = name , BehaviorTree = CreateTree(name) });
                }
            }
            return datas;
        }
        [TreeDatasSaver]
        public static void TreeDatasSaver(NodeBase tree, string name)
        {

        }
    }
}
