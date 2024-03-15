#include "Applier.h"
#include <iostream>


bool FileExists(const std::string& name)
{
	FILE *file;
	if (fopen_s(&file,name.c_str(), "r") == 0) {
		fclose(file);
		return true;
	}
	else 
	{
		return false;
	}
}

void printUsage()
{
	wcout << L"\n";
	wcout << L"To run this application specify the signature ID of the application you would like to download\n\n";
	wcout << L"Example: download-patch 3521\n";
	wcout << L"This example would download Foxit Reader\n";
	wcout << L"\n";
}


int wmain(int argc, wchar_t** argv)
{
	if (!FileExists("license.cfg") || !FileExists("pass_key.txt"))
	{
		wcout << L"\nError:  In order to run this application a license.cfg and pass_key.txt needs to be included in the working directory.\n";
		return -1;
	}


	wstring signatureIDString;
	if (argc > 1)
	{
		signatureIDString = argv[1];
	}
	else
	{
		printUsage();
		return -2;
	}


	try
	{
		int signatureID = std::stoi(signatureIDString);

		// Currently Hard coded to download Foxit Pro
		int result = LookupAndDownloadPatch(signatureID, 0, L"");
	
		if (result == 0)
		{
			wcout << L"Successfully downloaded patch!";
		}
		else
		{
			wcout << L"The patch associated with the signatureID " << signatureIDString << L" could not be downloaded ";
		}
	
	}
	catch (...)
	{
		cout << "\nError:  Signature ID is not valid.  Check values and try again.\n";
		printUsage();
	}

	return 0;
}