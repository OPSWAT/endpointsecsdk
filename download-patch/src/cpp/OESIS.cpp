
#include "wa_api.h"
#include <string>
#include "Utils.h"
using namespace std;

int Invoke(map<string, string> param, wstring& json_out)
{
	wstring json_in = L"";
	wa_wchar* json_out_tmp = NULL;
	CreateJsonIn(json_in, param);
	int rc = wa_api_invoke(json_in.c_str(), &json_out_tmp);
	json_out = json_out_tmp;
	return rc;
}


int LoadPatchDatabase(wstring& json_out)
{
	wstring json_in = L"";
	map<string, string> param = {
		{"method", to_string(WA_VMOD_PATCH_LOAD_DATABASE)},
		{"dat_input_source_file", "patch.dat"},
		{"dat_input_checksum_file", "ap_checksum.dat"}
	};

	int rc = Invoke(param, json_out);
	return rc;
}

int ConsumeOfflineVmodDatabase(wstring& json_out)
{
	wstring json_in = L"";
	map<string, string> param = {
		{"method", to_string(WAAPI_MID_CONSUME_OFFLINE_VMOD_DATABASE)},
		{"dat_input_source_file", "v2mod-vuln-oft.dat"}
	};

	int rc = Invoke(param, json_out);
	return rc;
}


int SetupOESIS(wstring outputFile)
{
	wstring pass_key = L"";
	pass_key = ReadFileContentIntoWString(GetCurrentFolderPath() + L"/pass_key.txt");
	const wstring json_config = L"{ \"config\" : { \"passkey_string\": \"" + pass_key + L"\", \"enable_pretty_print\": true, \"online_mode\": true, \"silent_mode\": true } }";


	wa_wchar* json_out = NULL;
	int rc = wa_api_setup(json_config.c_str(), &json_out);

	if (!WAAPI_SUCCESS(rc))
	{
		WriteWStringToFile(outputFile, json_out);
		return rc;
	}

	//
	// Load the vmod.dat
	//
	wstring wjson_out;
	rc = ConsumeOfflineVmodDatabase(wjson_out);
	if (!WAAPI_SUCCESS(rc))
	{
		WriteWStringToFile(outputFile, wjson_out);
		return rc;
	}

	//
	// Now load the database files
	//
	rc = LoadPatchDatabase(wjson_out);
	if (!WAAPI_SUCCESS(rc))
	{
		WriteWStringToFile(outputFile, wjson_out);
		return rc;
	}

	return rc;
}



void TeardownOESIS()
{
	wa_api_teardown();

}

json GetLatestInstaller(int signature_id, int download, wstring token, wstring language, int index, int patch_id, wstring path)
{
	wstring json_out;
	json jsonOut;

	map<string, string> param = {
		{"method", to_string(WA_VMOD_PATCH_GET_LATEST_INSTALLER)},
		{"signature", to_string(signature_id)},
		{"download", to_string(download)},
		{"index", to_string(index)}
	};

	if (!token.empty() != NULL)
	{
		param.insert({ "token", WStringToString(token) });
	}

	if (!language.empty() != NULL)
	{
		param.insert({ "language", WStringToString(language) });
	}

	if (!path.empty() != NULL)
	{
		param.insert({ "path", WStringToString(path)});
	}

	if (patch_id != 0)
	{
		param.insert({ "patch_id", to_string(patch_id)});
	}
	
    
	int rc = Invoke(param, json_out);
    GetJsonValues(jsonOut, rc, json_out, { "title", "type_id", "patch_id", "index", "eula", "release_note","release_date",
                                                    "url", "minimum_version", "language", "architecture", "file_type", "path", "expected_sha256" });

    return jsonOut;
}



json InstallFromFile(int signature_id, string path, int force_close, int background) {
	wstring json_out;
	json jsonOut;
	wstring json_in = L"";
	map<string, string> param = {
		{"method", to_string(WA_VMOD_PATCH_INSTALL_FROM_FILES)},
		{"signature", to_string(signature_id)},
		{"path", path}
	};

	if (force_close == 1)
	{
		param["force_close"] = to_string(1);
	}

	if (background > 0)
	{
		param["background"] = to_string(background);
	}

	int rc = Invoke(param, json_out);
	GetJsonValues(jsonOut, rc, json_out, {});
	return jsonOut;
}

