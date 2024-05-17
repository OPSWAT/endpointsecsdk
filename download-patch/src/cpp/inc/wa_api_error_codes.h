/****** THIS FILE IS AUTOMATICALLY GENERATED. DO NOT EDIT OR YOUR CHANGES WILL BE OVERWRITTEN! ******/
#pragma once
/**	@file wa_api_error_codes.h
 *	@brief Defines success and error codes returned from API calls
 *	@defgroup waapi_errorcodes Error codes
 */

#include "wa_api_data_types.h"

/**
 *	@def WAAPI_SUCCESS(code)
 *	@brief Checks an error @a code for success. Equates to true if successful error code, false otherwise
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_SUCCESS(code) (((wa_int)(code))>=0)

/**
 *	@def WAAPI_FAILED(code)
 *	@brief Checks an error @a code for failure. Equates to true if failure error code, false otherwise
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_FAILED(code) (((wa_int)(code))<0)

/**
 *	@def WAAPI_SUCCESS_SPECIAL(code)
 *	@brief Checks an error @a code for a special success case. Equates to true if the error code is a method specific success code, false otherwise.
 *		   NOTE: this will return false for other normal success cases, such as WAAPI_OK.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_SUCCESS_SPECIAL(code) (((wa_int)(code))>=1000)

/**
 *	@brief Defines a success.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK 0

/**
 *	@brief Defines a success and value of TRUE.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_TRUE 1

/**
 *	@brief Defines a success and value of FALSE.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_FALSE 2

/**
 *	@brief Defines a partial success (for multiple operations, some succeeded and some failed).
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_PARTIAL_SUCCESS 3

/**
 *	@brief Defines a successful call, but requested resource wasn't found in local cache and must be pulled from network.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OPERATION_INPROGRESS 4

/**
 *	@brief Defines a successful call, but the data being used in the call is considered old by the api.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_CACHE_DATA_STALE 5

/**
 *	@brief Defines a successful call where not all results have been retreived. This may be returned from an asynchronous call where additional results are still being generated.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_MORE_RESULTS 6

/**
 *	@brief Defines a successful call where not all desired effects have been achieved. This may be returned when a reference has been released but the operation did not complete due to still-positive reference count.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_IN_USE 7

/**
 *	@brief Defines a successful call where the database (libwaresource) version is older than core version (libwaapi).
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_OUTDATED_DATABASE 8

/**
 *	@brief For method GetActiveUserInfo only. Defines a successful call where a failure occured when trying to get the password protection state but username and domain are successfully retrieved. 'protected' field will be set to false (default value), but should not be taken into consideration.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_FAILED_TO_GET_PASSWORD_PROTECTION 9

/**
 *	@brief Defines a successful call where the device is required to reboot for the functionality to be operational.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_RESTART_TO_ENABLE 10

/**
 *	@brief Defines a successful call, but no connected keyboards were detected for the Anti Keylogger feature.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_NO_KEYBOARD_DETECTED 11

/**
 *	@brief Defines a successful call, but the request is not completed because user request to cancel.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_CANCELED 12

/**
 *	@brief Defines a successful call, but the request is not completed because SDK lost control of the helper process. The helper reported this result before it was lost.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_RESULT_BEFORE_LOST_CONTROL 13

/**
 *	@brief Defines a success but validation failed
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_VALIDATION_FAILED 14

/**
 *	@brief Defines a success but cannot perform validation
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_CANNOT_VALIDATE 15

/**
 *	@brief Define a success, but validation is not supported for the product
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_VALIDATION_NOT_SUPPORTED 16

/**
 *	@brief HTTP ok success code.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_OK 200

/**
 *	@brief HTTP created success code.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_CREATED 201

/**
 *	@brief HTTP non authoritative success code.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_NON_AUTHORITATIVE 203

/**
 *	@brief HTTP no content success code.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_NO_CONTENT 204

/**
 *	@brief HTTP reset content success code.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_RESET_CONTENT 205

/**
 *	@brief HTTP partial content success code.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_PARTIAL_CONTENT 206

/**
 *	@brief Defines data is live.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_DATA_LIVE 1000

/**
 *	@brief Defines data is cached.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_DATA_CACHED 1001

/**
 *	@brief Defines data is expired.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_DATA_EXPIRED 1002

/**
 *	@brief Defines product installation requires restart.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_INSTALLATION_NEED_RESTART 1003

/**
 *	@brief Defines product installation requires restart after sucessfully install partially.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_PARTIAL_INSTALLATION_NEED_RESTART 1004

/**
 *	@brief Defines a successful call, but that a product has never, or does not report that it has ever, run a Backup.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_NO_BACKUP_REPORTED 2000

/**
 *	@brief Defines a successful call, but that a product's user interface may not reflect any changes.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_NOT_SHOWN_IN_GUI 5000

/**
 *	@brief Defines a successful call, but that a product has never, or does not report that it has ever, run a scan.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_NO_SCAN_REPORTED 5001

/**
 *	@brief Defines a successful call, but the product cannot perform the operation because of an expired license.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_PRODUCT_LICENSE_EXPIRED 5002

/**
 *	@brief Defines a successful call, but the product may not be able to perform the operation without some interaction from the user in the product UI.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_USER_INTERACTION 5003

/**
 *	@brief Defines a successful call, but the product may open a user interface in order to perform the operation.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_GUI_DISPLAYED 5004

/**
 *	@brief Defines a successful call, file contents was copied but NTFS name streams couldn't be copied.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_ALTERNATE_STREAMS_NOT_COPIED 5005

/**
 *	@brief Defines a successful call for circumstance the user does not set yet.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_OK_USER_DOES_NOT_SET 5006

/**
 *	@brief Defines a general failure.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_GENERAL -1

/**
 *	@brief Defines an error when the license file is missing.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_LICENSE_MISSING -2

/**
 *	@brief Defines an error when the license provided does not match the license file.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_LICENSE_MISMATCH -3

/**
 *	@brief Defines an error when the license key has expired, not be to be confused with component licenses.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_LICENSE_EXPIRED -4

/**
 *	@brief Defines an error when the API has not been initialized, yet a call requiring initialization was invoked.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NOT_INITIALIZED -5

/**
 *	@brief Defines an error when initialization is called when the API is already initialized.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_ALREADY_INITIALIZED -6

/**
 *	@brief Defines an error when a call is made to a component that is not licensed.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COMPONENT_NOT_LICENSED -7

/**
 *	@brief Defines an error when a call is made to a component whose license has expired.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COMPONENT_LICENSE_EXPIRED -8

/**
 *	@brief Defines an error when a call is made to a component that is licensed, but is not deployed.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COMPONENT_NOT_DEPLOYED -9

/**
 *	@brief Defines an error when the component module from disk has been tampered with, therefore unloaded by the api.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COMPONENT_TAMPERED -10

/**
 *	@brief Defines an error when a method call was made on a component that does not support it.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COMPONENT_METHOD_NOT_SUPPORTED -11

/**
 *	@brief Defines an error when a method call was made on a component that does not implement it.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COMPONENT_METHOD_NOT_IMPLEMENTED -12

/**
 *	@brief Defines an error when the database is missing.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_DATABASE_MISSING -13

/**
 *	@brief Defines an error when the database exists but is corrupted.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_DATABASE_CORRUPTED -14

/**
 *	@brief Defines an error when a lookup in the database is missing. Perhaps the database is out of date?
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_DATABASE_ENTRY_MISSING -15

/**
 *	@brief Defines an error when the database is queried when it has yet to be initialized.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_DATABASE_NOT_INITIALIZED -16

/**
 *	@brief Defines an error where there is no connection when one is expected.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NO_CONNECTION -17

/**
 *	@brief Defines an error when the provided computer name or IP to a remote call is not found in the network.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_IP_NOT_FOUND -18

/**
 *	@brief Defines an error when the provided user, password, or domain information is invalid.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_CREDENTIALS -19

/**
 *	@brief Defines an error when the input arguments provided do not match the methods documented input requirements. Not to be confused with @ref WAAPI_ERROR_INVALID_JSON
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_INPUT_ARGS -20

/**
 *	@brief Defines an error when the input arguments provided are not in proper @JSON format. Not to be confused with @ref WAAPI_ERROR_INVALID_INPUT_ARGS
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_JSON -21

/**
 *	@brief Defines an error when the access is denied for the invocation call requested.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_ACCESS_DENIED -22

/**
 *	@brief Defines an error when the system or product is in an invalid state to perform the invocation call requested.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_STATE -23

/**
 *	@brief Defines an error when a product is required to be running in order to perform the invocation call requested, yet the product is no longer running.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NOT_RUNNING -24

/**
 *	@brief Defines an error when working with Component Object Model (COM) on the Windows platform.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_COM -25

/**
 *	@brief Defines an error when a request is dispatched and an exception is thrown.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_DISPATCH_EXCEPTION -26

/**
 *	@brief Defines an error in a native system API.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NATIVE_API -27

/**
 *	@brief Defines an error when an object is not found. This can be objects held internally, or objects on the endpoint, such as files, backup times, etc.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NOT_FOUND -28

/**
 *	@brief Defines an error when an object is not found in the Windows Registry. Windows only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_REGISTRY_NOT_FOUND -29

/**
 *	@brief Defines an error when an operating is made to the Windows Registry and the operation is denied access. Windows only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_REGISTRY_ACCESS_DENIED -30

/**
 *	@brief Defines an error when a query operating is made to the Windows Registry and the data type read is not currently supported by the API. Windows only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_REGISTRY_READ_TYPE_NOT_SUPPORTED -31

/**
 *	@brief Defines an error that occurs in the Windows Registry but cannot be specified by a more specific registry error code. Windows only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_REGISTRY_GENERAL -32

/**
 *	@brief Defines an error that occurs with our encryption/decryption security module.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_CRYPTO -33

/**
 *	@brief Defines an error when a cache entry is missing.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NO_CACHE_ENTRY -34

/**
 *	@brief Defines an error when a cache entry exists, but it is out of date.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_CACHE_OUT_OF_DATE -35

/**
 *	@brief Defines an error when the configuration value for a valid key is of the incorrect type.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_CONFIG_VALUE -36

/**
 *	@brief Defines an error when the configuration key is not a valid configuration option.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_CONFIG_KEY -37

/**
 *	@brief Defines an error when an expression is in an invalid format.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_SYNTAX -38

/**
 *	@brief Defines an error when a dynamic function call is invalid.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NOT_A_FUNCTION -39

/**
 *	@brief Defines a timeout for uninstallation.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_UNINSTALL_TIMEOUT -40

/**
 *	@brief Defines an error internal to the scripting engine.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_SCRIPTING_ENGINE -41

/**
 *	@brief Defines a general error within a script.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_SCRIPTING_GENERAL -42

/**
 *	@brief Defines a general error during a WMI call.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_WMI -43

/**
 *	@brief Defines an error that occurs when an attempt to compile a script.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_SCRIPT_COMPILE_FAILURE -44

/**
 *	@brief Defines an error when an expression is found to be invalid or corrupt.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_MALFORMED_EXPRESSION -45

/**
 *	@brief Defines an error in the local caching mechanism.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_LOCAL_CACHE -46

/**
 *	@brief Defines an error when a digital signature failed checksum verification.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_SIGNATURE -47

/**
 *	@brief Defines an error when a hash value does not match the expected value.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_HASH_MISMATCH -48

/**
 *	@brief Defines an error when an internal thread failed to respond in a timely manner.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_THREAD_TIMEOUT -49

/**
 *	@brief Defines an error when no active user session is found.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_NO_ACTIVE_USER_SESSION -50

/**
 *	@brief Defines an error when the mockup json is not properly formatted or missing required information. Only returned when using WAAPI_CONFIG_MOCKUP_MODE setup config option.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_MOCKUP -51

/**
 *	@brief Defines an error when there are too many sub-queries in a bulk query.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_TOO_MANY_QUERIES -52

/**
 *	@brief Defines an error when a Bundle resource cannot be found. Mac only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_BUNDLE_NOT_FOUND -53

/**
 *	@brief Defines an error when the API does not have necessary permissions to access a necessary bundle resource. Mac only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_BUNDLE_ACCESS_DENIED -54

/**
 *	@brief Defines an error when an operation on a bundle resource fails. Mac only.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_BUNDLE_GENERAL -55

/**
 *	@brief Defines an error where the requested call or operation is currently running and concurrent operations are not supported.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_OPERATION_IN_PROGRESS -56

/**
 *	@brief Defines an error when an operation did not complete because a resource is being used.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_IN_USE -57

/**
 *	@brief Defines an error when the output arguments provided do not match the methods documented output requirements. Not to be confused with @ref WAAPI_ERROR_INVALID_JSON
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_OUTPUT_ARGS -58

/**
 *	@brief Defines an error when the license provided is not valid for requested mode of operation.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_LICENSE_INVALID -59

/**
 *	@brief Defines an error when a connection to wa_3rd_party_host_32.exe/wa_3rd_party_host_64.exe cannot be established.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_3RD_PARTY_HOST_CONNECTION_NOT_ESTABLISHED -60

/**
 *	@brief This is not an actual error. Will print additional information for the main error. Will not be retuned from an API call but will be available in the method output in case of an error.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INFORMATION -61

/**
 *	@brief Defines an error when the access is denied for the invocation call requested, due to tamper protection of the AV product being active.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_TAMPER_PROTECTION_ON -62

/**
 *	@brief (macOS only) Defines an error when an invalid value is provided for 'instance_id' key, when a signature has multiple instances installed on the same machine.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_INVALID_INSTANCE_ID -63

/**
 *	@brief Defines an error when the operation fails because it can not bypass password protection.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_PASSWORD_REQUIRED -64

/**
 *	@brief Defines an error when the operation fails because it can not bypass captcha protection.
 *	@ingroup waapi_errorcodes
 */
#define WAAPI_ERROR_CAPTCHA_REQUIRED -65

/**
 *	@brief Request returned an http 300 multiple choices notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_MULTIPLE_CHOICES -300

/**
 *	@brief Request returned an http 301 moved permanently notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_MOVED_PERMANENTLY -301

/**
 *	@brief Request returned an http 302 found notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_FOUND -302

/**
 *	@brief Request returned an http 303 see other notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_SEE_OTHER -303

/**
 *	@brief Request returned an http 304 not modified notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_NOT_MODIFIED -304

/**
 *	@brief Request returned an http 305 use proxy notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_USE_PROXY -305

/**
 *	@brief Request returned an http 307 temporary redirect notification.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_TEMPORARY_REDIRECT -307

/**
 *	@brief Request returned an http 400 bad request error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_BAD_REQUEST -400

/**
 *	@brief Request returned an http 401 unauthorized error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_UNAUTHORIZED -401

/**
 *	@brief Request returned an http 403 forbidden error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_FORBIDDEN -403

/**
 *	@brief Request returned an http 404 not found error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_NOT_FOUND -404

/**
 *	@brief Request returned an http 405 method not allowed error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_METHOD_NOT_ALLOWED -405

/**
 *	@brief Request returned an http 406 not acceptable error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_NOT_ACCEPTABLE -406

/**
 *	@brief Request returned an http 407 proxy authentication required error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_PROXY_AUTHENTICATION_REQUIRED -407

/**
 *	@brief Request returned an http 408 request timeout error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_REQUEST_TIMEOUT -408

/**
 *	@brief Request returned an http 409 conflict.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_CONFLICT -409

/**
 *	@brief Request returned an http 410 gone error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_GONE -410

/**
 *	@brief Request returned an http 411 length required error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_LENGTH_REQUIRED -411

/**
 *	@brief Request returned an http 412 precondition failed error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_PRECONDITION_FAILED -412

/**
 *	@brief Request returned an http 413 request entity too large error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_REQUEST_ENTITY_TOO_LARGE -413

/**
 *	@brief Request returned an http 414 request uri too long error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_REQUEST_URI_TOO_LONG -414

/**
 *	@brief Request returned an http 415 unsupported media type error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_UNSUPPORTED_MEDIA_TYPE -415

/**
 *	@brief Request returned an http 416 range not satisfiable error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_REQUESTED_RANGE_NOT_SATISFIABLE -416

/**
 *	@brief Request returned an http 417 expectation failed error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_EXPECTATION_FAILED -417

/**
 *	@brief Request returned an http 500 internal server error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_INTERNAL_SERVER_ERROR -500

/**
 *	@brief Request returned an http 501 not implemented error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_NOT_IMPLEMENTED -501

/**
 *	@brief Request returned an http 502 bad gateway error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_BAD_GATEWAY -502

/**
 *	@brief Request returned an http 503 service unavailable error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_SERVICE_UNAVAILABLE -503

/**
 *	@brief Request returned an http 504 gateway timeout error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_GATEWAY_TIMEOUT -504

/**
 *	@brief Request returned an http 505 version not supported error.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_VERSION_NOT_SUPPORTED -505

/**
 *	@brief Request failed because the version of the MetaDefender Endpoint Security SDK is too old. Please use a newer version.
 *	@ingroup waapi_errorcodes
 */
#define HTTP_INVALID_VERSION_STAMP -506

/**
 *	@brief Defines an error when the MetaDefender Endpoint Security SDK COM object is not available.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_OESIS_COM_NOT_AVAILABLE -1000

/**
 *	@brief Defines an error when the MetaDefender Endpoint Security SDK On Demand object is not available.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_OOD_NOT_AVAILABLE -1001

/**
 *	@brief Defines an error when there is problem initialize COM lib.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_INITIALIZE_COM -1002

/**
 *	@brief Defines an error when there is problem activating MetaDefender Endpoint Security SDK On Demand.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_ACTIVATE_OOD -1003

/**
 *	@brief Defines an error when OOD object is not initialized.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_OOD_NOT_INITIALIZED -1004

/**
 *	@brief Defines an error when queried product can't be found.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_PRODUCT_NOT_FOUND -1005

/**
 *	@brief Defines an error when queried product interface isn't supported.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_INTERFACE_NOT_SUPPORTED -1006

/**
 *	@brief Defines an error when queried product cannot update.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_CANNOT_UPDATE_PRODUCT -1007

/**
 *	@brief Defines an error when queried product cannot locate cache.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_CANNOT_FIND_CACHE_FILE -1008

/**
 *	@brief Defines an error when queried call isn't cached.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_QUERY_ISNT_CACHED -1009

/**
 *	@brief Defines an error when caching isn't enabled.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_CACHING_IS_DISABLED -1010

/**
 *	@brief Defines an error when definition state cannot be retrieved.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_CANNOT_GET_DEF_STATE -1011

/**
 *	@brief Defines an error when GEARS cannot detect products.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_CANNOT_DETECT_PRODUCTS -1012

/**
 *	@brief Defines an error when GEARS cannot be initialized.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_GEARS_CANNOT_INITIALIZE -1013

/**
 *	@brief Defines an error when an operation did not complete because it requires a newer version of server.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_LEGACY_SERVER -1014

/**
 *	@brief Defines an error when an operation did not complete because server couldn't be reached.
 *	@ingroup waapi_errorcodes
 */
#define VMOD_ERROR_NOROUTETOSERVER -1015

/**
 *	@brief Defines an error when there is a mismatch in output format between internal components.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_INTERNAL_FORMAT_MISMATCH -1016

/**
 *	@brief Defines an error when UpdateVerify is not initialized.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_UPDATEVERIFY_NOT_INITIALIZED -1017

/**
 *	@brief Defines an error when MetaDefender Endpoint Security SDK 4V is not initialized.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_4V_NOT_INITIALIZED -1018

/**
 *	@brief Defines an error when VMod Source is not initialized.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_OFFLINEVMOD_NOT_INITIALIZED -1019

/**
 *	@brief Defines an error when MetaDefender Endpoint Security SDK 4V cannot get product's version.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_CANNOT_GET_VERSION -1020

/**
 *	@brief Defines an error when the procedure to retrieve system updates is timeout.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_GET_SYSTEM_PATCH_TIMEOUT -1021

/**
 *	@brief Defines an error when MetaDefender Endpoint Security SDK 4V cannot retrieve OS info.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_GEARS_CANNOT_GET_OS_INFO -1022

/**
 *	@brief Defines an error when the vulnerability source in input is invalid.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_INVALID_VULN_SOURCE -1023

/**
 *	@brief Defines an error when a product is not supported.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_PRODUCT_NOT_SUPPORTED -1026

/**
 *	@brief Defines an error when a language is not supported.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_LANGUAGE_NOT_SUPPORTED -1027

/**
 *	@brief Defines an error when an architecture is not supported.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_ARCHITECTURE_NOT_SUPPORTED -1028

/**
 *	@brief Defines an error when the file type is not supported.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_NOT_RECOGNIZED_FILE -1029

/**
 *	@brief Cannot close all blocking processes.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_CANNOT_TERMINATE_PRODUCT -1030

/**
 *	@brief Cannot retrieve list of blocking processes.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_CANNOT_GET_PROCESSES -1031

/**
 *	@brief Existing product needs to be uninstalled before installation.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_NEED_UNINSTALL_PRODUCT_FIRST -1033

/**
 *	@brief Installation failed.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_INSTALLATION_FAILED -1034

/**
 *	@brief File/installer extraction failed.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_EXTRACT_FAILED -1036

/**
 *	@brief Defines that there isn't enough memory.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_OUT_OF_MEMORY -1037

/**
 *	@brief Defines an error when the type of data/content is not supported.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_DATA_TYPE_NOT_SUPPORTED -1038

/**
 *	@brief Defines an error when the queried index is out of array range.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_OUT_OF_RANGE -1039

/**
 *	@brief Defines an error when the online database prefix is not set or invalid
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_INVALID_ONLINE_DB_URI -1040

/**
 *	@brief Defines an error when the operation fails because another MSI installation process is running.
 *	@ingroup waapi_errorcodes
 */
#define WA_VMOD_ERROR_ANOTHER_MSI_INSTALLATION_INPROGRESS -1041