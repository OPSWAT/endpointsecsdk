///////////////////////////////////////////////////////////////////////////////////////////////
///  Sample Code for HelloWorld
///  Reference Implementation using OESIS Framework
///  
///  Created by Chris Seiler
///  OPSWAT OEM Solutions Architect
///////////////////////////////////////////////////////////////////////////////////////////////
#include "Utils.h"


wstring Utils::ReadFileContentIntoWString(const wstring& filename)
{
#ifdef _WIN32
	wstring buffer;            // stores file contents
	FILE* file = _wfopen(filename.c_str(), L"rtS, ccs=UTF-8");
	HRESULT r = GetLastError();
	// Failed to open file
	if (file == NULL)
	{
		// ...handle some error...
		return buffer;
	}
	size_t filesize = GetSizeOfFile(filename);
	// Read entire file contents in to memory
	if (filesize > 0)
	{
		buffer.resize(filesize);
		size_t wchars_read = fread(&(buffer.front()), sizeof(wchar_t), filesize, file);
		buffer.resize(wchars_read);
		buffer.shrink_to_fit();
	}
	fclose(file);
	return buffer;

#else
	ifstream inputFile;
	string content = "";
	string tmp;
	string path = utf8_encode(filename);
	inputFile.open(path);
	while (inputFile >> tmp)
	{
		content += tmp;
	}
	inputFile.close();
	return utf8_decode(content);

#endif // 
}



#ifdef _WIN32
size_t Utils::GetSizeOfFile(const wstring& path)
{
	struct _stat fileinfo;
	_wstat(path.c_str(), &fileinfo);
	return fileinfo.st_size;
}
#endif // _WIN32


//
wstring Utils::GetCurrentFolderPath()
{
#ifdef _WIN32
	wchar_t buffer[MAX_PATH];
	GetModuleFileNameW(NULL, buffer, MAX_PATH);
	string::size_type pos = wstring(buffer).find_last_of(L"\\/");
	return wstring(buffer).substr(0, pos);
#else
	char* currentFolder = getcwd(NULL, 0);
	return utf8_decode(currentFolder);

#endif // _WIN32
}
