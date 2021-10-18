using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeuroGen
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Model
    {
        public string definition;
        public float fitness;

        public Model(string definition, float fitness)
        {
            this.definition = definition;
            this.fitness = fitness;
        }
    }

    struct RawModel
    {
        public IntPtr definition;
        public float fitness;
    }

    public class Database
    {
        private static string path;
        private static IntPtr models;

#if UNITY_IPHONE
        [DllImport("__Internal")]
#else
        [DllImport("persistent_models")]
#endif
        private static extern IntPtr pm_load_definition(string path);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern int pm_count(out IntPtr models);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern RawModel pm_get_model(out IntPtr models, int index);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void pm_free_string(IntPtr str);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void pm_free_models(out IntPtr models);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern IntPtr pm_load_file(string file_path);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void pm_save_file(out IntPtr models, string file_path);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void pm_add_to_stage(out IntPtr models, string definition, float fitness);

#if UNITY_IPHONE
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
#else
        [DllImport("persistent_models", CallingConvention = CallingConvention.Cdecl)]
#endif
        private static extern void pm_commit(out IntPtr models, int amount);

        public static void LoadFile(string path)
        {
            Database.path = path;
            models = pm_load_file(path);
        }

        public static void Close()
        {
            pm_free_models(out models);
        }

        public static int NumberOfEntries()
        {
            return pm_count(out models);
        }

        public static Model GetModel(int index)
        {
            var rawModel = pm_get_model(out models, index);

            var model = new Model(Marshal.PtrToStringAuto(rawModel.definition), rawModel.fitness
            );

            pm_free_string(rawModel.definition);

            return model;
        }

        public static void Commit(int amount)
        {
            pm_commit(out models, amount);
        }

        public static void Stage(string definition, float fitness)
        {
            pm_add_to_stage(out models, definition, fitness);
        }

        public static void SaveFile()
        {
            pm_save_file(out models, path);
        }
    }
}