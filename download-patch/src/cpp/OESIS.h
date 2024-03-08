#pragma once
#include "Utils.h"

int SetupOESIS(wstring outputFile);
void TeardownOESIS();

json GetLatestInstaller(int signature_id, int download, wstring token, wstring language, int index, int patch_id, wstring path);
json InstallFromFile(string path, int force_close);