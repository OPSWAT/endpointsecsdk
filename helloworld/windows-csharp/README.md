# OESIS Framework - Getting Started Samples


### Compiling the Samples
This application was developed using Visual Studio Community Edition.  That can be downloaded with winget here with this command 

#### "winget install --id=Microsoft.VisualStudio.2022.Community  -e"  

Or download the installer here:

#### https://visualstudio.microsoft.com/vs/community/

Once Visual Studio is installed open the solution /src/HelloWorld/HelloWorld.sln.  

Make sure to copy the download_token.txt, license.cfg and pass_key.txt file into the /src/HelloWorld/license directory.

Compile the Compliance project.

#### Notes:

The SDK downloader at the repo root must be run before building — run `sdk-downloader\windows-csharp\bin\SDKDownloader.exe` (or `python sdk-downloader\script\src\main.py`), which populates `OPSWAT-SDK\`. Each project's post-build step then runs `copysdkfiles.ps1` to stage the SDK from `OPSWAT-SDK\` into the build output. If the SDK files are missing, the build stops with an error telling you to run the downloader first.


### Running the Samples
Each sample for the different use cases is included in the HelloWorld solution.  Currently it contains the following samples.

#### Compliance - Firewall Running
This sample will detect the existing Firewall Products installed on the Endpoint.  It will then make a call using the SDK to check to see if the firewall is running and print the status for each firewall product discovered.  

Program.cs - Contains the main sample code.  Start with the main function.\
OESISAdapter.cs - Provides the .Net Runtime mappings to the Native Code\
XStringMarshaler.cs - Provides the needed string mappings between .Net and Native Code\
Product.cs - Plain object used to map product information\

#### InlineLicense - Loads the license into memory
This sample demonstrates how to load the license.cfg straight from the code.    

Program.cs - Contains the main sample code.  Start with the main function.\
OESISAdapter.cs - Provides the .Net Runtime mappings to the Native Code\
XStringMarshaler.cs - Provides the needed string mappings between .Net and Native Code\


#### Vulnerability - Scans the system for 3rd Party Vulnerabilitys
This sample will detect all the products on the system.  Call the GetProductVulnerability method to get the results of any vulnerabilities.  It prints the counts of any vulnerabilities and the more detailed response is written in a results file.    

Program.cs - Contains the main sample code.  Start with the main function.\
OESISAdapter.cs - Provides the .Net Runtime mappings to the Native Code\
XStringMarshaler.cs - Provides the needed string mappings between .Net and Native Code\
Product.cs - Plain object used to map product information\

#### Patch - An example of the workflow to install a patch
This sample will install Firefox.  Will patch current version or install a fresh install of FireFox. Calls GetLatestInstaller and InstallFromFiles.  Will download the file using Windows download methodes and does a Checksum Validation.    

Program.cs - Contains the main sample code.  Start with the main function.\
OESISAdapter.cs - Provides the .Net Runtime mappings to the Native Code\
XStringMarshaler.cs - Provides the needed string mappings between .Net and Native Code\
InstallDetails.cs - Plain object used to map the URL and other information needed to download and install the content\
HttpClientUtils.cs - Interfaces with the Native HTTP Download code and the Checksum validatiors\

#### More Coming soon 

