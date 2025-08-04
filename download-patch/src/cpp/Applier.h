#include <string>
using namespace std;

#define ERROR_DOWNLOAD -1
#define ERROR_HASH -2


int LookupAndDownloadPatch(int signatureId, int patchId, wstring token);

