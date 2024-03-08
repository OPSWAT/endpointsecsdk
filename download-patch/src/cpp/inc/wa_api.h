#pragma once 
/** @file wa_api.h
 *	@brief Defines the interface
 *
 *	@defgroup waapi_integration_interface Functions
 *	@brief The C integration interface. Include @ref wa_api.h to integrate the API.
 */

#include "wa_api_defs.h"                    // defines our exporters/importers
#include "wa_api_error_codes.h"             // defines our error codes
#include "wa_api_fnc_ptr.h"                 // defines our function pointer helpers
#include "wa_api_data_types.h"              // defines our data types
#include "wa_api_product_categories.h"      // defines our product types
#include "wa_api_config_options.h"          // defines our configuration options
#include "wa_api_invoke_common.h"           // defines our common methods for products
#include "wa_api_invoke_detect.h"           // defines our detection methods
#include "wa_api_invoke_method.h"			// defines our category specific product methods
#include "wa_api_json_keys.h"               // defines our json object keys that map to json values

#ifdef __cplusplus
extern "C"
{
#endif

	/**
	 *	@brief Setup the interface with the given license key and configuration.
	 *	@ingroup waapi_integration_interface
	 *
	 *	@param[in] json_config The configuration options for the interface.
	 *	@param[out] json_out The initialization results. It can be:
	 *	{
	 *		"config" :
	 *		{
	 *			"module" : "gears sdk"
	 *			...Current configuration settings of Gears Sdk components
	 *		}
	 *	}
	 *
	 * OR
	 *
	 *	{
	 *		"configs" :
	 *		[
	 *			{
	 *				...Current configuration settings of a component
	 *			},
	 *			{
	 *				...Current configuration settings of another component
	 *			}
	 *			...
	 *		],
	 *		"errors" :
	 *		[
	 *			{
	 *				...Initialization error description of a component
	 *			},
	 *			{
	 *				...Initialization error description of another component
	 *			}
	 *			...
	 *		]
	 *	}
	 *
	 *	@returns An @ref waapi_errorcodes "error code"
	 */
	WAAPI_DECLFUNCTION wa_api_setup( const wa_wchar * json_config , wa_wchar ** json_out );

	/**
	 *	@brief Register an event handler for the given event. When the event is triggered the handler will be invoked.
	 *
	 *	@param[in] json_event_info The information about the event to monitor.
	 *	@param[in] event_handler The handler to be invoked when the event is triggered.
	 *		NOTE: The json string passed to event_handler MUST be freed with a call to wa_api_free.
	 *	@param[out] handler_id The assigned handler id. Used to unregister the handler, if needed.
	 *
	 *	@returns An @ref waapi_errorcodes "error code"
	 *	@retval	WAAPI_ERROR_NOT_INITIALIZED if wa_api_setup has not been called.
	 *	@retval	WAAPI_ERROR_INVALID_INPUT_ARGS if handler_id is NULL.
	 *	@retval	WAAPI_ERROR_INVALID_JSON if json_event_info is not a valid json input string.
	 */
	WAAPI_DECLFUNCTION wa_api_register_handler( const wa_wchar* json_event_info, wa_event_handler event_handler, wa_int* handler_id );

	/**
	 *	@brief Will unregister the handler
	 *
	 *	@param[in] handler_id The id of the handler to remove
	 *
	 *	@returns An @ref waapi_errorcodes "error code"
	 */
	WAAPI_DECLFUNCTION wa_api_unregister_handler( const wa_int handler_id );

	/**
	 *	@brief Performs an invocation call to the component.
	 *	@ingroup waapi_integration_interface
	 *
	 *	@param[in] json_in The invocation input to dispatch to the component.
	 *	@param[out] json_out The invocation output results. User can never pass @c NULL, must pass a valid wa_wchar pointer, which can be @c NULL, that can be allocated with content.
	 *
	 *	@returns An @ref waapi_errorcodes "error code"
	 */
	WAAPI_DECLFUNCTION wa_api_invoke( const wa_wchar * json_in, wa_wchar ** json_out );

	/**
	 *	@brief Will release the memory of the pointer allocated from any double pointer passed to any API call.
	 *	@ingroup waapi_integration_interface
	 *
	 *	@param[in,out] json_data The pointer to the memory that is to be released
	 *
	 *	@returns A 4V @ref waapi_errorcodes "error code"
	 */
	WAAPI_DECLFUNCTION wa_api_free( wa_wchar * json_data );

	/**
	 *	@brief Deinitializes the interface and destructs all internal structures created.
	 *	@ingroup waapi_integration_interface
	 *
	 *	@returns An @ref waapi_errorcodes "error code"
	 */
	WAAPI_DECLFUNCTION wa_api_teardown();

#ifdef __cplusplus
} // extern "C"
#endif
