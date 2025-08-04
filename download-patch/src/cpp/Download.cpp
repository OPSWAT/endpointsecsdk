
#define _CRT_SECURE_NO_WARNINGS

#include "curl.h"
#include <cstdio>
#include <stdlib.h>
#include <string>
#include "Utils.h"
#include <sys/utime.h>
#include <time.h>

using namespace std;


//
// Note this function can be used for tracking the download or cancelling the download
//
static size_t write_data(void* ptr, size_t size, size_t nmemb, void* stream) {
    size_t written = fwrite(ptr, size, nmemb, static_cast<FILE*>(stream));
    return written;
} /* write_data */


int DownloadFile(wstring url, wstring destination )
{
    int result = -1;

    wcout << L"Downloading from URL:  \n";
    wcout << url << "\n";
    wcout << "\n";
    wcout << L"Writing to file:  \n";
    wcout << destination << "\n";

    // Download the file using curl library into DownloadCURL folder
    if (CURL* curl = curl_easy_init()) {
        
        string destUTF8 = utf8_encode(destination);
        string urlUTF8 = utf8_encode(url);
        
        if (FILE* fp = fopen(destUTF8.c_str(), "wb")) {
            curl_easy_setopt(curl, CURLOPT_URL, urlUTF8.c_str());
            curl_easy_setopt(curl, CURLOPT_FAILONERROR, 1);
            curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_data);
            curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);
            curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1L); // follow redirects
            curl_easy_setopt(curl, CURLOPT_HTTPPROXYTUNNEL, 1L); // corp. proxies etc.
            curl_easy_setopt(curl, CURLOPT_MAX_RECV_SPEED_LARGE, (curl_off_t)10*1000*1000); // This sets throttling at 10 MB Bytes per second
            curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, false); // Note this disables the SSL Head check.  We do the hash check to validate
            curl_easy_setopt(curl, CURLOPT_FILETIME, 1L);


            /* Perform the request, res will get the return code */
            CURLcode res = curl_easy_perform(curl);

            // Close the file
            fclose(fp);


            // On success update the time stamp
            if (!res)
            {
                result = 0;

                // Update the Time stamp to match the online version
                long filetime;
                res = curl_easy_getinfo(curl, CURLINFO_FILETIME, &filetime);
                if ((CURLE_OK == res) && (filetime >= 0)) {
                    time_t file_time = (time_t)filetime;

                    struct _utimbuf ut;
                    ut.modtime = file_time;
                    ut.actime = file_time;
                    _utime(destUTF8.c_str(), &ut);
                }
            }
            else
            {
                result = res;
            }

        }
        curl_easy_cleanup(curl);
    } 

    return result;

} 