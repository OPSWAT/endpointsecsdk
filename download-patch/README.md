# OPSWAT MetaDefender SDK Patch Download Sample


### Explanation of Workflow
This sample provides code that demonstrates using the SDK's ability to download the content needed to install a specific application.

The sample requires a signatureID associated with the application that should be downloaded.  For example signatureID of **3521** is for **Foxit Reader**.  

The workflow is to call the SDK to get the URL and Hash.  The sample then will use the URL to download the content.  After a successful download the sample will genenerate a hash from the downloaded file and compare that hash with the expected hash returned from the SDK.  

### Running the Sample
Run the command **\bin\download-patch.exe 3521** to see the sample run.

### Compiling the Scanner
This application was developed using Visual Studio Community Edition.  That can be downloaded with winget here with this command 

**"winget install --id=Microsoft.VisualStudio.2022.Community  -e"**  

Or download the installer here:

**https://visualstudio.microsoft.com/vs/community/**

Once Visual Studio is installed open the project /src/download-patch/download-patch.sln.  Compile the project and run.

The directory structure is as follows

/bin - Contains the release version of this sample and the files used for initial developement.  The library files and dat files could be out of date by the time you try this.

**Build files**<br>
**src/cpp** - Contains the build files and code files for the sample 

**Postbuild files - These files are copied to the output directory**<br>
**src/cpp/license** - Contains the license files available in your POC directory.<br>
**src/cpp/dat** - Data files needed.  These should be updated for the latest patch info. <br>
**src/cpp/dll** - SDK files needed to download the patches.  These should be updated.<br>

**Nuget Packages - Should automatically downlaod to the packages directory**<br>
**Libcurl** - Used for downloading the file<br>
**OpenSSL** - Used to generate the sha256 hash.<br>


### Important Notes!!! 

1.  This sample is using Offline mode of the SDK.  That means that there are 2 files that need to be delivered to the endpoint to run this.  That is patch.dat and ap_checksum.dat.
1. This works for all applications that have a URL to content that can be downloaded.  This will **NOT WORK** for applicaitions that are orchestrated by the SDK.  The main products that fit this category are Office 365, Office 2021, Office 2019 and Office 2016.
1. In order to retrieve the checksums ap_checksum.dat is required to be on the endpoint.  This file is not needed to simply download the patches.

