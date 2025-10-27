package main

/*
#cgo CFLAGS: -I../inc
#cgo windows,amd64 LDFLAGS: -L../libs/win/x64 -lwaapi
#cgo windows,arm64 LDFLAGS: -L../libs/win/arm64 -lwaapi
#cgo linux LDFLAGS: -L../libs/linux -lwaapi
#cgo darwin LDFLAGS: -L../libs/mac -lwaapi -Wl,-rpath,../libs/mac

// Workaround for ARM64 Clang not recognizing __int32
#ifdef __clang__
    #ifdef _WIN32
        #ifndef __int32
            #define __int32 int
        #endif
    #endif
#endif

#include "wa_api.h"
#include <stdlib.h>
#include <wchar.h>
#include <stdint.h>
#include <string.h>
*/
import "C"
import (
	"encoding/json"
	"flag"
	"fmt"
	"log"
	"os"
	"path/filepath"
	"strings"
	"unsafe"
)

type LicenseConfig struct {
	LicenseKey string `json:"license_key"`
	License    string `json:"license"`
}

// Helper function to convert Go string to wide character string
func stringToWChar(s string) *C.wa_wchar {
	// Use C.CString for simpler string conversion
	cstr := C.CString(s)
	defer C.free(unsafe.Pointer(cstr))

	// Convert to wide char using mbstowcs
	len := C.mbstowcs(nil, cstr, 0)
	if len == C.size_t(^uint(0)) { // -1 cast to size_t
		return nil
	}

	// Allocate wide char buffer
	wstr := (*C.wchar_t)(C.malloc((len + 1) * C.size_t(unsafe.Sizeof(C.wchar_t(0)))))
	if wstr == nil {
		return nil
	}

	// Convert string
	C.mbstowcs(wstr, cstr, len+1)
	return (*C.wa_wchar)(wstr)
}

// Helper function to convert wide character string to Go string
func wCharToString(wcharPtr *C.wa_wchar) string {
	if wcharPtr == nil {
		return ""
	}

	// Convert wide char to multibyte using wcstombs
	len := C.wcstombs(nil, (*C.wchar_t)(wcharPtr), 0)
	if len == C.size_t(^uint(0)) { // -1 cast to size_t
		return ""
	}

	// Allocate buffer for multibyte string
	mbs := (*C.char)(C.malloc(len + 1))
	if mbs == nil {
		return ""
	}
	defer C.free(unsafe.Pointer(mbs))

	// Convert string
	C.wcstombs(mbs, (*C.wchar_t)(wcharPtr), len+1)
	return C.GoString(mbs)
}

func main() {
	// Parse CLI flags
	licenseDir := flag.String("license-dir", ".", "Directory containing pass_key.txt and license.cfg")
	debug := flag.Bool("debug", false, "Enable debug logging in SDK config")
	flag.Parse()

	// Read passkey
	passkeyPath := filepath.Join(*licenseDir, "pass_key.txt")
	data, err := os.ReadFile(passkeyPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: could not read pass_key.txt from %s: %v\n", *licenseDir, err)
		os.Exit(1)
	}
	finalPasskey := strings.TrimSpace(string(data))
	if finalPasskey == "" {
		fmt.Fprintln(os.Stderr, "Error: pass_key.txt is empty")
		os.Exit(1)
	}

	// Read license.cfg
	licenseCfgPath := filepath.Join(*licenseDir, "license.cfg")
	licenseCfgData, err := os.ReadFile(licenseCfgPath)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: could not read license.cfg from %s: %v\n", *licenseDir, err)
		os.Exit(1)
	}
	var lic LicenseConfig
	err = json.Unmarshal(licenseCfgData, &lic)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: could not parse license.cfg: %v\n", err)
		os.Exit(1)
	}
	if lic.LicenseKey == "" || lic.License == "" {
		fmt.Fprintln(os.Stderr, "Error: license.cfg must contain license_key and license fields")
		os.Exit(1)
	}

	// Build config JSON
	config := map[string]interface{}{
		"config": map[string]interface{}{
			"license_bytes":       lic.License,
			"license_key_bytes":   lic.LicenseKey,
			"passkey_string":      finalPasskey,
			"enable_pretty_print": true,
			"silent_mode":         true,
		},
	}
	if *debug {
		cwd, _ := os.Getwd()
		config["config_debug"] = map[string]interface{}{
			"debug_log_level":       "ALL",
			"debug_log_output_path": cwd,
		}
	}
	configJSONBytes, err := json.Marshal(config)
	if err != nil {
		fmt.Fprintf(os.Stderr, "Error: could not marshal config JSON: %v\n", err)
		os.Exit(1)
	}
	configJSON := string(configJSONBytes)
	// fmt.Println("configJSON:", configJSON)

	// Initialize the SDK
	err = initializeSDK(configJSON)
	if err != nil {
		log.Fatalf("Failed to initialize SDK: %v", err)
	}
	defer teardownSDK()

	// Detect all products
	response, err := detectProducts()
	if err != nil {
		log.Fatalf("Failed to detect products: %v", err)
	}

	// Print the JSON response
	fmt.Println("DetectProducts Response:")
	fmt.Println(response)
}

// Change initializeSDK to accept configJSON string directly
func initializeSDK(configJSON string) error {
	// Convert Go string to wide character string
	configWChar := stringToWChar(configJSON)
	defer C.free(unsafe.Pointer(configWChar))

	var jsonOut *C.wa_wchar

	// Call wa_api_setup
	result := int(C.wa_api_setup(configWChar, &jsonOut))

	if jsonOut != nil {
		defer C.wa_api_free(jsonOut)
	}

	if result < 0 {
		if jsonOut != nil {
			errorStr := wCharToString(jsonOut)
			return fmt.Errorf("initialization failed (code %d): %s", result, errorStr)
		}
		return fmt.Errorf("initialization failed with error code: %d", result)
	}

	log.Println("MDES SDK initialized successfully")
	return nil
}

func detectProducts() (string, error) {
	requestJSON := `{ "input" : { "method" : 0 }}`

	// Convert Go string to wide character string
	requestWChar := stringToWChar(requestJSON)
	defer C.free(unsafe.Pointer(requestWChar))

	var jsonOut *C.wa_wchar

	// Call wa_api_invoke
	result := int(C.wa_api_invoke(requestWChar, &jsonOut))

	if jsonOut == nil {
		return "", fmt.Errorf("no response from SDK (code %d)", result)
	}
	defer C.wa_api_free(jsonOut)

	// Convert response back to Go string
	response := wCharToString(jsonOut)

	if result != 0 {
		return response, fmt.Errorf("SDK call failed with error code: %d", result)
	}

	return response, nil
}

func teardownSDK() {
	result := int(C.wa_api_teardown())
	if result != 0 {
		log.Printf("Warning: teardown failed with error code: %d", result)
	} else {
		log.Println("MDES SDK teardown completed")
	}
}
