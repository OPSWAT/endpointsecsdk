///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
#include "RealTimeMonitoring.h"
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

void HandleInstallCallback(wa_wchar* json_event)
{
	wcout << json_event;
	wcout << L"\n---------------------------------------------------------------------------------\n";
}

void HandleStateCallback(wa_wchar* json_event)
{
	wcout << json_event;
	wcout << L"\n---------------------------------------------------------------------------------\n";
}


vector<wa_int> RegisterCallbacks()
{
	vector<wa_int> handler_ids;
	wa_int handler_id;

	wstring json_config = L"{\"event_type\": 1}";
	wa_api_register_handler(json_config.c_str(), HandleProcessCallback, &handler_id);
	handler_ids.push_back(handler_id);

	json_config = L"{\"event_type\": 2}";
	wa_api_register_handler(json_config.c_str(), HandleInstallCallback, &handler_id);
	handler_ids.push_back(handler_id);

	// Set this up to listen for the GetFirewallState(1007) event for Windows Firewall(288)
	json_config = L"{\"event_type\": 10, \"config\": {\"signature\":288,\"method\":1007}}";
	wa_api_register_handler(json_config.c_str(), HandleStateCallback, &handler_id);
	handler_ids.push_back(handler_id);

	return handler_ids;
}

void UnRegisterCallbacks(const std::vector<wa_int>& handler_ids)
{
	for (wa_int id : handler_ids)
	{
		wa_api_unregister_handler(id);
	}
}


int main()
{
	//
	// This will monitor processes and print out data for 10 minutes
	//
	if (WAAPI_SUCCESS(SetupOESIS()))
	{
		std::vector<wa_int> handler_ids = RegisterCallbacks();

		int timeToRun = 60 * 10; // 10 minutes
		while (timeToRun > 0)
		{
			Sleep(1000); // Sleep for 1 second
			timeToRun--;
		}

		UnRegisterCallbacks(handler_ids);
	}

	return 0;
}
