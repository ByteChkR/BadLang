#pragma once

struct LFAssemblyPatchSite
{
public:
    int Offset{};
    int Size{};
    LFAssemblyPatchSite(int offset, int size);
    LFAssemblyPatchSite();
};
