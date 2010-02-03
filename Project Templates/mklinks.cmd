if exist VB\ProntoWebApplication\Libraries rmdir VB\ProntoWebApplication\Libraries
if exist VB\ProntoWebApplication\admin rmdir VB\ProntoWebApplication\admin

mklink /J VB\ProntoWebApplication\Libraries ..\Source\Pronto\bin\Release
mklink /J VB\ProntoWebApplication\admin ..\Source\ClientAdmin

if exist CSharp\ProntoWebApplication\Libraries rmdir CSharp\ProntoWebApplication\Libraries
if exist CSharp\ProntoWebApplication\admin rmdir CSharp\ProntoWebApplication\admin

mklink /J CSharp\ProntoWebApplication\Libraries ..\Source\Pronto\bin\Release
mklink /J CSharp\ProntoWebApplication\admin ..\Source\ClientAdmin