
WINFORM - Any Cpu
------------------------------------------------------------------------------------------

When using the files in this directory, copy PEGRPE.DLL from the included 
DotNetAnyCpu/x64 folder into system32 on your 64 bit development system.  
Our setup.exe does not install into the native 64 bit system director so you 
must copy this file manually.


This assembly can simplify development of projects that need to support native 64 bit 
with only one AsAny AnyCpu based exe.  Eliminating a separate build though potentially 
adding confusion if a user simply copies your final app from a 32 to 64 bit system without 
running your install. 

Optionally, the easiest distribution that will run on both 32 bit and 64 bit is to 
use our 32 bit .Net controls found in the DotNet20 director.  Compile your .NET app 
with x86 CPU setting and your EXE will run on 64 bit Windows via WOW64. 
This is the recommend method of supporting Winx64 execution as one 32 bit exe/install 
will run on both 32 and 64 bit systems and use PEGRP32E.DLL.  A user can simply copy
your app from 32 to 64 bit systems without concern or need of an install. 

The .Net Winform Assembly in this directory is built with AsAny/AnyCpu target cpu and 
hard wired to a native DLL named PEGRPE.DLL.  When distributing, you will
need to conditionally install the 32 or 64 bit version of PEGRPE.DLL found in either
the DotNetAnyCPu/x32 or DotNetAnyCpu/x64 sub directory.  

While developing you can switch the target cpu from x86 to x64 simplifying testing 
both targets. For distribution the purpose of this assembly is to allow your exe 
to be built with cpu setting AsAny/AnyCpu. 

When distributing, PEGRPE.DLL can be in your app's local directory along with this
Gigasoft.ProEssentials.DLL.  Your setup/install process must install the correct 
version of PEGRPE.DLL based on the native operating system 32 or 64 bit condition. 

Another option for native 64 bit support is found in our directory
ProEssentials7/x64/DotNet64 that uses PEGRP64E.DLL. 






