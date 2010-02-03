if exist VB\ProntoCmsWebApplication\Libraries rmdir VB\ProntoCmsWebApplication\Libraries
if exist VB\ProntoCmsWebApplication\admin rmdir VB\ProntoCmsWebApplication\admin

mklink /J VB\ProntoCmsWebApplication\Libraries ..\Source\Pronto\bin\Release
mklink /J VB\ProntoCmsWebApplication\admin ..\Source\ClientAdmin

if exist CSharp\ProntoCmsWebApplication\Libraries rmdir CSharp\ProntoCmsWebApplication\Libraries
if exist CSharp\ProntoCmsWebApplication\admin rmdir CSharp\ProntoCmsWebApplication\admin

mklink /J CSharp\ProntoCmsWebApplication\Libraries ..\Source\Pronto\bin\Release
mklink /J CSharp\ProntoCmsWebApplication\admin ..\Source\ClientAdmin