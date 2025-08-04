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
    // Detect just the patching products here
    const wstring json_in = L"{ \"input\" : { \"method\" : 0, \"categories\": [12]} }";
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

vector<int> ExtractSignatureIDs(const json& jsonData) {
    vector<int> signatureIDs;

    try {
        // Navigate to the detected products array
        if (jsonData.contains("result") && jsonData["result"].contains("detected_products")) {
            const auto& detectedProducts = jsonData["result"]["detected_products"];

            // Iterate over each detected product
            for (const auto& product : detectedProducts) {
                if (product.contains("signature")) {
                    int signatureID = product["signature"].get<int>();
                    signatureIDs.push_back(signatureID);
                    cout << "[DEBUG] Found signature ID: " << signatureID << endl;
                }
            }
        }
        else {
            cerr << "[ERROR] Invalid JSON structure: 'result' or 'detected_products' not found." << endl;
        }
    }
    catch (const std::exception& ex) {
        cerr << "[ERROR] Exception during extraction: " << ex.what() << endl;
    }

    return signatureIDs;
}

int GetMissingPatches(wstring* result, int signatureID) {
    // Convert signatureID to wstring
    wstring signatureIDStr = to_wstring(signatureID);

    // Construct the JSON input string correctly
    wstring json_in = L"{ \"input\" : { \"method\" : 1013, \"signature\": " + signatureIDStr + L" } }";
    wa_wchar* json_out = NULL;

    // Invoke the API method
    int rc = wa_api_invoke(json_in.c_str(), &json_out);

    // Check the result
    if (WAAPI_SUCCESS(rc)) {
        if (json_out) {
            *result = json_out;  // Store the output in the result
            wa_api_free(json_out);  // Free memory after use
            wcout << L"[INFO] GetMissingPatches succeeded for signature ID " << signatureID << L".\n";
        }
        else {
            wcerr << L"[ERROR] GetMissingPatches succeeded but returned a null output for signature ID " << signatureID << L".\n";
        }
    }
    else {
        
        // Make sure to check this is not expected.  Not Implemented means that the patch provider is not supportted.
        if(rc != WAAPI_ERROR_COMPONENT_METHOD_NOT_IMPLEMENTED)
        {
            wcerr << L"[ERROR] GetMissingPatches failed for signature ID " << signatureID
            << L" with error code: " << rc << L".\n";
            if (json_out) {
                wcerr << json_out;
                wa_api_free(json_out);  // Free memory after use
            }
        }
    }

    return rc;
}

void ProcessSignatures(const vector<int>& signatureIDs) {
    for (int signatureID : signatureIDs) {
        wstring result;
        int status = GetMissingPatches(&result, signatureID);

        if (status == 0) {
            wcout << L"[INFO] Success for signature ID " << signatureID << L": " << result << endl;
        }
        else {
            
            if(status != WAAPI_ERROR_COMPONENT_METHOD_NOT_IMPLEMENTED)
            {
                wcerr << L"[ERROR] Failed to get missing patches for signature ID " << signatureID << endl;
            }
        }
    }
}

int main() {
    PrintCurrentDirectory();

    if (WAAPI_SUCCESS(SetupOESIS())) {
        wstring productResult;
        if (WAAPI_SUCCESS(DetectProducts(&productResult))) {
            vector<int> signatureIDs = ExtractSignatureIDs(json::parse(wstring_to_utf8(productResult)));
            ProcessSignatures(signatureIDs);
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
