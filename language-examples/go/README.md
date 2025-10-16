In order to get started with this script you will need to download and install the following pre-requistes

For Windows
1.  Install GO
	a. Navigate to https://go.dev/doc/install
	b. Install the correct version of go for the architecture
		Example for x64 use go1.24.5.windows-amd64.msi - Note the version may change
2.  Install gcc
	a. Navigate to https://github.com/niXman/mingw-builds-binaries/releases/tag/15.1.0-rt_v12-rev0
		Download the correct version.  X64 should be x86_64-15.1.0-release-posix-seh-ucrt-rt_v12-rev0.7z
	b. Extract the files to a path in the system.  Example c:\gcc
	c. Once extracted find the path to gcc and add it as an environment variable
	   Example:  C:\gcc\mingw64\bin
		

3.  Copy the license files to the build directory
	%extracted_root%\build - add license.cfg and pass_key.txt

4.  Build with Powershell
	a. Open Powershell
	b. Navigate to the extracted root
	c. Run build.ps1
	
5.  Run the sample code
	a. Navigate to the build directory %extracted_root%\build
	b. run go-sample.exe

Note: sample code is in %extracted_root%\src\main.go



For Mac
 -- Still in Progress