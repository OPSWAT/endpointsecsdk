/****** THIS FILE IS AUTOMATICALLY GENERATED. DO NOT EDIT OR YOUR CHANGES WILL BE OVERWRITTEN! ******/
#pragma once
/**	@file wa_api_invoke_detect.h
 *	@brief Defines component method identifiers for detection interface.
 *	@defgroup WAAPI_component_detect Detection
 */

/**
 *	@brief Will detect installed products for a given <a href='c_sdk.html#product-categories'>product category</a> on the endpoint.<br>Because a single product can come in many versions, installations and other variations, most products will have multiple unique definitions. The 'id' refers to this definition of the product, and is used in future calls to the API to further manipulate or query information about the product.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_DETECT_PRODUCTS 0

/**
 *	@brief Will attempt to discover and detect all known and unknown installed products on the endpoint. This product detection is independent from normal API detection, but results of this method will contain a mixture of known products and the generically detected ones.<br>Because a single product can come in many versions, installations and other variations, some products will have multiple definitions. Further, generically detected products do not have unique identifiers - and consequently cannot be querried for extraneous information using API methods.</br><strong>Note:</strong> this method requires the use of the 'Addon' library (libwaaddon.*).
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_DISCOVER_PRODUCTS 100001

/**
 *	@brief Will return information about local operating system (name, version, etc.).
 *	@ingroup WAAPI_component_detect
 *	@returns 
 */
#define WAAPI_MID_GET_OS_INFO 1

/**
 *	@brief Will retrieve information about the status of your current license.
 *	@ingroup WAAPI_component_detect
 *	@returns 
 */
#define WAAPI_MID_GET_LICENSE_INFO 2

/**
 *	@brief Updates the license info.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_UPDATE_LICENSE_INFO 3

/**
 *	@brief Will retrieve the device identity and generate a new locally unique uuid.
 *	@ingroup WAAPI_component_detect
 *	@returns 
 */
#define WAAPI_MID_GET_DEVICE_IDENTITY 4

/**
 *	@brief Will retrieve all completed asynchronous jobs that were made using the <a href='c_guide_async_api.html'>Async API</a>. This methed can be polled for jobs until there are no remaining jobs to process.<br>The Async API is used by setting up the MetaDefender Endpoint Security SDK with the appropriate configuration value for <a href='c_sdk.html#WAAPI_CONFIG_ASYNC_API_MODE'>WAAPI_CONFIG_ASYNC_API_MODE</a>. Depending on that configuration setting the 'async_job' key may then be needed to perform asynchronous calls.<br>This call will return <a href='c_return_codes.html#WAAPI_OK_MORE_RESULTS'>WAAPI_OK_MORE_RESULTS</a> if there are still more async jobs to be processed.
 *	@ingroup WAAPI_component_detect
 *	@returns 
 */
#define WAAPI_MID_QUERY_ASYNC_RESULTS 5

/**
 *	@brief Will retrieve information about the current, active user account.
 *	@ingroup WAAPI_component_detect
 *	@returns 
 */
#define WAAPI_MID_GET_ACTIVE_USER_INFO 6

/**
 *	@brief Will retrieve the status of Windows Security Center AV products.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_GET_WSC_AV_PRODUCTS 7

/**
 *	@brief Will detect all the fields and the subkeys of a registry path
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_ENUMERATE_REGISTRY 110001

/**
 *	@brief Will retrieve the contents of a file specified by its path.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_GET_FILE_CONTENTS 110002

/**
 *	@brief Will detect all the services of the curent machine
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_ENUMERATE_SERVICES 110003

/**
 *	@brief Will run a specific cmd command on a machine
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_EXECUTE_COMMAND 110004

/**
 *	@brief Will retrieve the version of a file specified by its path.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_GET_FILE_VERSION 110005

/**
 *	@brief Will run a specific WMI query on the machine.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_EXECUTE_WMI_QUERY 110006

/**
 *	@brief Will retrieve the contents of a directory given a recursive depth.
 *	@ingroup WAAPI_component_detect
 *	@returns An @ref waapi_errorcodes 'error code'
 */
#define WAAPI_MID_ENUMERATE_DIRECTORY 110007
