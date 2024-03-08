#pragma once
/** @file wa_api_defs.h
 *  @brief Defines boiler plate for used by 4V exports/imports function signatures
 */
#define __OPSWAT__

#ifdef _WIN32
	/**
	*	@brief Defines the Windows platform
	*/
	#ifndef _WAAPI_WINDOWS_
	#define _WAAPI_WINDOWS_ 1
	#endif
	#define WAAPI_DLL_EXT L"dll"
#elif __APPLE__
	/**
	*	@brief Defines the Apple platform
	*/
	#ifndef _WAAPI_APPLE_
	#define _WAAPI_APPLE_ 1
	#endif
	#define WAAPI_DLL_EXT L"dylib"
#elif __linux__
	/**
	*	@brief Defines the Linux platform
	*/
	#ifndef _WAAPI_LINUX_
	#define _WAAPI_LINUX_ 1
	#endif
	#define WAAPI_DLL_EXT L"so"
#endif

// Module id used in output json when returning an array of results from multile components
#define WAAPI_MODULE_4V L"libwaapi." WAAPI_DLL_EXT

// Module id used in getDeploymentPath as the above one containts the extension which we do not need
#define WAAPI_MODULE_4V_SIMPLE L"libwaapi"

#ifdef _WAAPI_WINDOWS_
#ifdef _WAAPI_EXPORTS_
/**
 *  @brief Defines export function names of the API
 */
#define WAAPI_DECLSPEC __declspec(dllexport)
#else
/**
 *  @brief Defines import function names of the API
 */
#define WAAPI_DECLSPEC __declspec(dllimport)
#endif

/**
 *  @brief Defines the export/import macro for class declarations
 */
#define WAAPI_DECLCLASS WAAPI_DECLSPEC

/** 
 *	@brief Defines the export/import macro for function declarations
 */
#define WAAPI_DECLFUNCTION WAAPI_DECLSPEC __int32 __stdcall

/**
 *	@brief Defines the macro for internal function declarations that are not exported or imported
 */
#define WAAPI_DECLINTERNAL __int32 __stdcall

/**
 *	@brief Defines the macro for internal returns without __stdcall
 */
#define WAAPI_RETURN __int32

#endif // end of _WAAPI_WINDOWS_

#if defined(_WAAPI_APPLE_) || defined(_WAAPI_LINUX_)

#undef _WAAPI_WINDOWS_

/**
 *  @brief Defines import function names of the API
 */
#define WAAPI_DECLSPEC __attribute__((visibility("default")))

/**
 *  @brief Defines the export/import macro for class declarations
 */
#define WAAPI_DECLCLASS WAAPI_DECLSPEC 

/**
 *	@brief Defines the export/import macro for function declarations
 */
#define WAAPI_DECLFUNCTION WAAPI_DECLSPEC int

/**
 *	@brief Defines the macro for internal function declarations that are not exported or imported
 */
#define WAAPI_DECLINTERNAL int

/**
 *	@brief Defines the macro for internal returns without __fastcall
 */
#define WAAPI_RETURN int
#endif // end of _WAAPI_APPLE_ || _WAAPI_LINUX_