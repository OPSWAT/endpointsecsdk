///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OPSWAT MetaDefender Endpoint Security SDK
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///  Mac version adapted by Lucas
///////////////////////////////////////////////////////////////////////////////////////////////
#include "SDKInit.h"

int DetectProducts(wstring* result) {
    const wstring json_in = L"{ \"input\" : { \"method\" : 0} }";
    wa_wchar* json_out = NULL;
    int rc = wa_api_invoke(json_in.c_str(), &json_out);

    if (WAAPI_SUCCESS(rc)) {
        *result += json_out;
    }
    else {
        wcerr << L"[ERROR] Product detection failed.\n";
        if (json_out) {
            wcerr << json_out;
            wa_api_free(json_out);  // Free memory after use
        }
    }

    if (json_out) {
        wa_api_free(json_out);  // Always free memory to prevent leaks
    }

    return rc;
}

int main() {
    PrintCurrentDirectory();

    if (WAAPI_SUCCESS(SetupOESIS())) {
        wstring productResult;
        if (WAAPI_SUCCESS(DetectProducts(&productResult))) {
            wcout << productResult;
        }
        else {
            wcerr << L"[ERROR] Failed to detect products.\n";
            wcerr << productResult;
        }
        
        wa_api_teardown();
    }
    else {
        wcerr << L"[ERROR] Failed to initialize OESIS.\n";
    }

    return 0;
} 
