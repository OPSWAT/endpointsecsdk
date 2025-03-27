///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
#include "ProcessCallback.h"
#include "Utils.h"

using namespace std;


int SetupOESIS()
{
	wstring pass_key = L"";
	pass_key = Utils::ReadFileContentIntoWString(Utils::GetCurrentFolderPath() + L"/pass_key.txt");
	const wstring json_config = L"{ \"config\" : { \"passkey_string\": \"" + pass_key + L"\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";


	wa_wchar* json_out = NULL;
	int rc = wa_api_setup(json_config.c_str(), &json_out);

	if (!WAAPI_SUCCESS(rc))
	{
		wcout << L"Failed to initialize: \n";
		wcout << json_out;
	}

	return rc;
}


int DetectProducts(wstring* result)
{
	const wstring json_in = L"{ \"input\" : { \"method\" : 0} }";

	wa_wchar* json_out = NULL;
	int rc = wa_api_invoke(json_in.c_str(), &json_out);
	*result += json_out;

	return rc;
}


int main()
{
	if (WAAPI_SUCCESS(SetupOESIS()))
	{
		wstring productResult;

		if (WAAPI_SUCCESS(DetectProducts(&productResult)))
		{
			wcout << productResult;
		}
		else
		{
			wcout << "Failed:";
			wcout << productResult;
		}
	}

	return 0;
}
