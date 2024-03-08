#include "OESIS.h"
#include "Utils.h"
#include "Download.h"
#include "CheckHash.h"
#include "Applier.h"

wstring GetDestinationFile(json downloadDetails)
{
	wstring downloadUrl = utf8_decode(downloadDetails["url"]);

	// Parse this to a file name
	return downloadUrl.substr(downloadUrl.rfind('/') + 1);
}

int DownloadPatch(const json downloadDetails)
{
	int result = -1;
	wstring downloadUrl = utf8_decode(downloadDetails["url"]);
	wstring destinationFile = GetDestinationFile(downloadDetails);
	string patchName = downloadDetails["title"];

	cout << "Downloading Patch: " << patchName;


	int downloadResult = DownloadFile(downloadUrl, destinationFile);
	//
	// Now check the hash
	//
	if (downloadResult == 0)
	{
		result = ERROR_HASH; // Set the default error 
		if (downloadDetails.contains("expected_sha256"))
		{
			for (int i = 0; i < downloadDetails["expected_sha256"].size(); i++)
			{
				wstring currentHash = utf8_decode(downloadDetails["expected_sha256"][i]);

				// Now validate the Hash
				if (ValidateHash256(destinationFile, currentHash))
				{
					result = 0;
					break;
				}
			}

			if (result != 0)
			{
				cout << "Hash Check Failed";
			}
		}
	}
	else
	{
		cout << "Downloading Failed: " << downloadResult;
		result = ERROR_DOWNLOAD;
	}

	return result;
}


int LookupAndDownloadPatch(int signatureId, int patchId, wstring token)
{
	int result = -1;
	SetupOESIS(L"Error");

	json downloadDetails = GetLatestInstaller(signatureId, 0, token, L"", 0, patchId, L"");
	
	if (!downloadDetails.contains("error"))
	{
		int downloadResult = DownloadPatch(downloadDetails);

		if (result == 0)
		{
			cout << "Download Complete";
		}
		else
		{
			result = downloadResult;
		}
	}


	return 0;
}