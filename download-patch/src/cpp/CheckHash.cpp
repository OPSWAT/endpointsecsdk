#include "openssl\sha.h"
#include <stdio.h>
#include <corecrt_malloc.h>
#include <string.h>
#include <string>
#include "Utils.h"
using namespace std;


void sha256_hash_string(unsigned char hash[SHA256_DIGEST_LENGTH], char* outputBuffer)
{
    int i = 0;

    for (i = 0; i < SHA256_DIGEST_LENGTH; i++)
    {
        sprintf_s(outputBuffer + (i * 2),70-(i*2), "%02x", hash[i]);
    }
}

// Buffer size needed is 65
int sha256_file(const char* path, char* outputBuffer)
{
    FILE* file;
    int result = fopen_s(&file, path, "rb");
    if (result != 0) return -534;

    unsigned char hash[SHA256_DIGEST_LENGTH];
    SHA256_CTX sha256;
    SHA256_Init(&sha256);
    const size_t bufSize = 32768;
    const size_t elementSize = 1;

    unsigned char* buffer = (unsigned char*)malloc(bufSize);
    size_t bytesRead = 0;
    if (!buffer) return ENOMEM;
    while (bytesRead = fread(buffer, elementSize, bufSize, file))
    {
        SHA256_Update(&sha256, buffer, bytesRead);
    }
    SHA256_Final(hash, &sha256);

    sha256_hash_string(hash, outputBuffer);
    fclose(file);
    free(buffer);
    return 0;
}


bool ValidateHash256(wstring sourceFile, wstring expectedHash)
{
    bool result = false;
    char calculatedHash[70] = { 0 };
    string sourceFileUTF8 = utf8_encode(sourceFile);
    string expectedHashUTF8 = utf8_encode(expectedHash);

    
    if (sha256_file(sourceFileUTF8.c_str(), calculatedHash) == 0)
    {
        if (_strcmpi(calculatedHash, expectedHashUTF8.c_str()) == 0)
        {
            result = true;
        }
    }

    return result;
}