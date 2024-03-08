#pragma once 
/** @file wa_api_data_types.h
 *  @brief Defines data types
 *	@defgroup waapi_datatypes Data types
 *	@brief Defines data types
 */

#include "wa_api_defs.h"

/** 
 *	@typedef __int32 wa_int
 *	@brief Integer data type
 *	@ingroup waapi_datatypes
 */
typedef int wa_int;

/**
 *	@typedef wchar_t wa_wchar
 *	@brief Character data type
 *	@ingroup waapi_datatypes
 */
typedef wchar_t wa_wchar;

/**
 *	@typedef wa_event_handler
 *	@brief Event handler function pointer for registered events. The json_event passed to the handler MUST be freed with a call to wa_api_free.
 *	@ingroup waapi_datatypes
 */
typedef void (*wa_event_handler)( wa_wchar * json_event );
