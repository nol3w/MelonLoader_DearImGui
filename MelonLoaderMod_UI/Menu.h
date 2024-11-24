#pragma once

inline const char* Tabs[] = { "First", "Second", "Third", "Fourth", "Fifth" };
enum Tab
{
    First,
    Second,
    Third,
    Fourth,
    Fifth
};
inline Tab currentTab;



inline const char* menuFontPath = "C:\\Windows\\Fonts\\consolab.ttf";
inline float fontSize = 13.0f;

void Menu();