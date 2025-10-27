///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///  Mac version adapted by Lucas
///////////////////////////////////////////////////////////////////////////////////////////////

#include "wa_api.h"
#include "Util.h"

wstring GetDebugConfig()
{
    char currentDir[PATH_MAX];
    getcwd(currentDir, sizeof(currentDir));

    wstring debugOutputPath = utf8_to_wstring(currentDir);
    wstring debugConfig = L"{ \"debug_log_level\": \"ALL\", \"debug_log_output_path\":\"" + debugOutputPath + L"\"}";

    return debugConfig;
}

wstring CreateConfig(bool enableDebug)
{
    wstring pass_key = ReadFileContentIntoWString(L"pass_key.txt");
    wstring licenseKey;
    wstring license;
    json jsonData;

    pass_key = TrimCarriageReturn(pass_key);
    if (pass_key.empty()) {
        wcerr << L"[ERROR] Pass key is empty or could not be loaded.\n";
        return L"";
    }

    LoadJsonFromFile("license.cfg", jsonData);
    licenseKey = utf8_to_wstring(jsonData["license_key"].get<string>());
    license = utf8_to_wstring(jsonData["license"].get<string>());


    // Setup the original config. If debug is enabled then overwrite
    // Note: restrict_bundle_search avoids looking in user specific folders on Mac because Apple limitis their access
    wstring json_config = L"{ \"config\" : { \"license_bytes\":\"" + license + L"\",\"license_key_bytes\":\"" + licenseKey + L"\",\"passkey_string\": \"" + pass_key + L"\", \"enable_pretty_print\": true, \"silent_mode\": true, \"restrict_bundle_search\": \"user_home|shared_folder|removable_media|reminders|photos|calendars|contacts|music\"} } }";
    if (enableDebug)
    {
        wstring debugConfig = GetDebugConfig();
        json_config = L"{ \"config\" : { \"license_bytes\":\"" + license + L"\",\"license_key_bytes\":\"" + licenseKey + L"\",\"passkey_string\": \"" + pass_key + L"\", \"enable_pretty_print\": true, \"silent_mode\": true, \"restrict_bundle_search\": \"user_home|shared_folder|removable_media|reminders|photos|calendars|contacts|music\"}, \"config_debug\":" + debugConfig + L"}";
    }

    return json_config;
}

int SetupOESIS() {
    const wstring json_config = CreateConfig(true);

    wa_wchar* json_out = NULL;
    int rc = wa_api_setup(json_config.c_str(), &json_out);

    if (!WAAPI_SUCCESS(rc)) {
        wcerr << L"[ERROR] Failed to initialize: \n";
        if (json_out) {
            wcerr << json_out;
            wa_api_free(json_out);  // Free memory after use
        }
        return rc;
    }

    wcout << L"[INFO] OESIS SDK successfully initialized.\n";
    return rc;
} 
