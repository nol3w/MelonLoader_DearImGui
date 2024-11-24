#include "Core/imgui/imgui.h"
#include "Menu.h"
#include <xutility>
#include <Windows.h>

class SettingsData {
public:
    bool bDrawDemoWindow = false;
};

// change this. Also change in OnLateInitialize in Main.cs
const char* ModSharedMemory = "MyMod_SharedMemory";

HANDLE hMapFile = nullptr;
SettingsData* settings = nullptr;
SettingsData* sharedSettings = nullptr;

extern "C" __declspec(dllexport) void InitSharedMemory() {
    // Create shared memory
    hMapFile = CreateFileMapping(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, sizeof(SettingsData), ModSharedMemory);
    if (hMapFile == NULL) {
        // Handle error
        return;
    }

    // Map the shared memory into the process's address space
    sharedSettings = (SettingsData*)MapViewOfFile(hMapFile, FILE_MAP_ALL_ACCESS, 0, 0, sizeof(SettingsData));
    if (sharedSettings == NULL) {
        CloseHandle(hMapFile);
        return;
    }

    // init settings
    settings = new SettingsData();
}

void UpdateSharedMemory() {
    if (sharedSettings != nullptr && settings != nullptr) {
        memcpy(sharedSettings, settings, sizeof(SettingsData));
    }
}

void Menu()
{
    ImGui::Begin("Melon Loader Dear ImGui", 0, ImGuiWindowFlags_NoCollapse);

    for (int i = 0; i < std::size(Tabs); i++)
    {
        if (ImGui::Button(Tabs[i]))
            currentTab = static_cast<Tab>(i);

        if (i != std::size(Tabs) - 1)
            ImGui::SameLine();
    }

    ImGui::Separator();

    if (currentTab == Tab::First)
    {
        ImGui::Checkbox("Demo Window", &settings->bDrawDemoWindow);
    }

    if (settings->bDrawDemoWindow) ImGui::ShowDemoWindow();

    if (currentTab == Tab::Second)
    {

    }

    if (currentTab == Tab::Third)
    {

    }

    if (currentTab == Tab::Fourth)
    {

    }

    if (currentTab == Tab::Fifth)
    {

    }

    UpdateSharedMemory();
    Sleep(1);

    ImGui::End();
}

