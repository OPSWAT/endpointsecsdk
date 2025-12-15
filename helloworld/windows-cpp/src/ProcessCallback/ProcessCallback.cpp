///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
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


void HandleProcessCallback(wa_wchar* json_event)
{
	wcout << json_event;
	wcout << L"\n---------------------------------------------------------------------------------\n";
}


wa_int RegisterCallbacks()
{
	wa_int handler_id;

	const wstring json_config = L"{\"event_type\": 1}";
	wa_api_register_handler(json_config.c_str(), HandleProcessCallback, &handler_id);

	return handler_id;
}

void UnRegisterCallbacks(wa_int handler_id)
{
	wa_api_unregister_handler(handler_id);
}


int main()
{
	//
	// This will monitor processes and print out data for 2 minutes
	//
	if (WAAPI_SUCCESS(SetupOESIS()))
	{
		wa_int handler_id = RegisterCallbacks();
		
		int timeToRun = 60 * 2; // 2 minutes
		while (timeToRun > 0)
		{
			Sleep(1000);
			timeToRun--;
		}
	
		UnRegisterCallbacks(handler_id);
	}

	return 0;
}
