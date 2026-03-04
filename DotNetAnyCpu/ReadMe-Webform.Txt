
WEBFORM - AnyCpu
-------------------------------------------------------------------------------------------------

This directory holds .NET WebForm assemblies compiled with AsAny target cpu. 

This allows for development with 32 bit Visual Studio/local server, and also allows 
placement on a 64 bit production server. 

WebForm deployment is distributing our DLLs within the /Bin directory of your web application. 

Starting with v7.1. Gigasoft.ProEssentialsWeb.DLL now expects to find 
PEGRPE.DLL. This is a new filename for ProEssentials. This file is a renamed version 
of either PEGRP32E.DLL or PEGRP64E.DLL. Our setup creates two sub-directories 
x86 and x64 within this directory. The setup also copies the 32 bit variation of PEGRPE.DLL 
into the Windows 32 bit system folder as the Visual Studio design time will expect to find 
the 32 bit version in design mode and running a web site through Visual Studio. 
Note 64 bit systems use syswow64 for 32 bit files, and system32 for 64 bit files. 

When deploying web site, place correct variation of PEGRPE.DLL 
within the Bin dir. Copy from either our x64 or x86 dir depending if your server 
is 64 or 32 bit.
 
Optionally: we added a potential custom config setting within Web.config. Note the 
ProEssentialsConfig declaration and proceeding section using item "nativedir". These are 
added to your web.config as needed if you prefer to control where our native DLL is found.

For example... 

<configuration>
  <configSections>
    <sectionname="ProEssentialsConfig"   type="Gigasoft.ProEssentials.ProEssentialsConfigSection" />
  </configSections>
  <ProEssentialsConfignativedir ="d:\mydir" />
  <system.web>
  <compilationdebug="true"targetFramework="4.0">
  <assemblies>
    <add assembly="System.Design, Version=4.0.0.0, Culture=neutral,PublicKeyToken=B03F5F7F11D50A3A"/> 
  </assemblies> 
  </compilation>
  </system.web>
</configuration>

In which case ProEssentials will look to load the necessary native DLL from d:\mydir.
 






