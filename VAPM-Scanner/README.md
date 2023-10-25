# OPSWAT Endpoint SDK Samples


### Running the Scanner
This implementation contains a pre-compiled executable in the "/bin" directory.  In order to run this application, it will require a license.cfg and pass_key.txt file.  Those can be obtained by contacting christopher.seiler@opswat.com.

Once the files are copied into the running directory, run: 

####"AcmeScanner.exe" as an Administrator  

It may take a few minutes to initially load as the application downloads the latest SDK and pulls down the latest Vulnerability  and Patch databases.

This contains 3 tabs that demonstrate different features of the application.  

1.  Offline Tab - This demonstrates loading the catalog files downloaded to an endpoint.  It will scan the system and has the capability of installing patches.
2.  Orchestration - This shows the API implementation with Windows Update.  Also allows patches to be installed.
3.  Catalog - This will load the details of the server-side JSON catalog and populates some of that data in an easy to see format.

### Compiling the Scanner
This application was developed using Visual Studio Community Edition.  That can be downloaded with winget here with this command 

####"winget install --id=Microsoft.VisualStudio.2022.Community  -e"  

Or download the installer here:

####https://visualstudio.microsoft.com/vs/community/

Once Visual Studio is installed open the project /src/AcmeScanner/AcmeScanner.sln.  Compile the project and run.

Notes:

VAPMAdapter is a layer that is used for interacting with the UI.  Most functionality and workflows that need to be moved to an existing application can be seen in this project.

Tasks Folder in VAPMAdpater is a good place to start understanding the different workflows.
