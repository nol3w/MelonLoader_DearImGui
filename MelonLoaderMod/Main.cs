using MelonLoader;
using UnityEngine;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Diagnostics;


[assembly: MelonInfo(typeof(MelonLoaderDearImGui.Main), "MelonLoader Dear ImGui Base", "1.0.0", "nolew")]
[assembly: MelonGame("Game Developer", "Game Name")]

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct SettingsData
{
    [MarshalAs(UnmanagedType.I1)]
    public bool bDrawDemoWindow;

}

namespace MelonLoaderDearImGui
{

    public class Main : MelonMod
    {
        [DllImport(@"Your_Mod.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitSharedMemory();

        SettingsData settings;
        MemoryMappedFile? mmf;
        MemoryMappedViewAccessor? accessor;
        bool bDLLInjected = false;

        public override void OnLateInitializeMelon()
        {
            try
            {
                // Inject DLL
                int processId = Process.GetProcessesByName("YourGameExe")[0].Id;

                if (Injector.InjectDll(processId, @"Your_Mod.dll"))
                    bDLLInjected = true;
                else
                    Application.Quit();

                InitSharedMemory();

                // Open shared memory
                mmf = MemoryMappedFile.OpenExisting("MyMod_SharedMemory");
                accessor = mmf.CreateViewAccessor();
                Log.Msg("Shared memory successfully opened.");
            }
            catch (FileNotFoundException)
            {
                Log.Error("Shared memory not found. Ensure the C++ DLL is injected and running.");
            }
        }

        private static T ReadStructure<T>(MemoryMappedViewAccessor accessor) where T : struct
        {
            T structure;
            accessor.Read(0, out structure);
            return structure;
        }

        public override void OnUpdate()
        {
            if (bDLLInjected && accessor != null)
            {
                try
                {
                    settings = ReadStructure<SettingsData>(accessor);
                }
                catch (Exception ex)
                {
                    LoggerInstance.Error($"Error reading shared memory: {ex.Message}");
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Log.Msg($"Draw Demo Window = {settings.bDrawDemoWindow}");
            }
        }

        public override void OnApplicationQuit()
        {
            accessor?.Dispose();
            mmf?.Dispose();
            base.OnApplicationQuit();
        }

    }
}