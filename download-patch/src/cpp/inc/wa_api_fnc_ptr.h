#pragma once 
/** @file wa_api_fnc_ptr.h
 *	@brief Defines function names and pointers to make explicit dynamic linking simpler
 *	@defgroup waapi_helper_pointers Explicit linking helpers
 *	@brief Defines function names and pointers to make explicit dynamic linking simpler
 */

#include "wa_api_data_types.h"

#ifdef _WAAPI_WINDOWS_
#define WAAPI_STDCALL __stdcall
#else
#define WAAPI_STDCALL
#endif

/**
 *	@brief The helper name define for explicit dynamic linking for \ref wa_api_setup
 *	@ingroup waapi_helper_pointers
 */
#define WAAPI_SETUP_NAME "wa_api_setup"

/**
 *	@typedef WAAPI_SETUP 
 *
 *	@brief The helper function pointer define for explicit dynamic linking for \ref wa_api_setup
 *	@ingroup waapi_helper_pointers
 *
 *	@note doxygen documentation generation omits the @c __stdcall calling convention, see source of \ref wa_api_fnc_ptr.h
 */
typedef wa_int (WAAPI_STDCALL *WAAPI_SETUP) ( const wa_wchar *, wa_wchar ** );

/**
 *	@brief The helper name define for explicit dynamic linking for \ref wa_api_invoke
 *	@ingroup waapi_helper_pointers
 */
#define WAAPI_INVOKE_NAME "wa_api_invoke"

/**
 *	@typedef WAAPI_INVOKE
 *	@brief The helper function pointer define for explicit dynamic linking for \ref wa_api_invoke
 *	@ingroup waapi_helper_pointers
 *
 *	@note doxygen documentation generation omits the @c __stdcall calling convention, see source of \ref wa_api_fnc_ptr.h
 */
typedef wa_int (WAAPI_STDCALL *WAAPI_INVOKE) ( const wa_wchar *, wa_wchar ** );

/**
 *	@brief The helper name define for explicit dynamic linking for \ref wa_api_free
 *	@ingroup waapi_helper_pointers
 */
#define WAAPI_FREE_NAME "wa_api_free"

/**
 *	@typedef WAAPI_FREE
 *	@brief The helper function pointer define for explicit dynamic linking for \ref wa_api_free
 *	@ingroup waapi_helper_pointers
 *
 *	@note doxygen documentation generation omits the @c __stdcall calling convention, see source of \ref wa_api_fnc_ptr.h
 */
typedef wa_int (WAAPI_STDCALL *WAAPI_FREE) ( wa_wchar * );

/**
 *	@brief The helper name define for explicit dynamic linking for \ref wa_api_teardown
 *	@ingroup waapi_helper_pointers
 */
#define WAAPI_TEARDOWN_NAME "wa_api_teardown"

/**
 *	@typedef WAAPI_TEARDOWN 
 *
 *	@brief The helper function pointer define for explicit dynamic linking for \ref wa_api_teardown
 *	@ingroup waapi_helper_pointers
 *
 *	@note doxygen documentation generation omits the @c __stdcall calling convention, see source of \ref wa_api_fnc_ptr.h
 */
typedef wa_int (WAAPI_STDCALL *WAAPI_TEARDOWN) ();

/**
*	@brief The helper name define for explicit dynamic linking for \ref wa_api_register_handler
*	@ingroup waapi_helper_pointers
*/
#define WAAPI_REGISTER_HANDLER_NAME "wa_api_register_handler"

/**
*	@typedef WAAPI_REGISTER_HANDLER
*
*	@brief The helper function pointer define for explicit dynamic linking for \ref wa_api_register_handler
*	@ingroup waapi_helper_pointers
*
*	@note doxygen documentation generation omits the @c __stdcall calling convention, see source of \ref wa_api_fnc_ptr.h
*/
typedef wa_int (WAAPI_STDCALL *WAAPI_REGISTER_HANDLER) ( const wa_wchar*, wa_event_handler, wa_int* );

/**
*	@brief The helper name define for explicit dynamic linking for \ref wa_api_unregister_handler
*	@ingroup waapi_helper_pointers
*/
#define WAAPI_UNREGISTER_HANDLER_NAME "wa_api_unregister_handler"

/**
*	@typedef WAAPI_REGISTER_HANDLER
*
*	@brief The helper function pointer define for explicit dynamic linking for \ref wa_api_unregister_handler
*	@ingroup waapi_helper_pointers
*
*	@note doxygen documentation generation omits the @c __stdcall calling convention, see source of \ref wa_api_fnc_ptr.h
*/
typedef wa_int( WAAPI_STDCALL *WAAPI_UNREGISTER_HANDLER ) ( const wa_int );
