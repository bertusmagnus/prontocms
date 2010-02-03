if exist VB\ProntoWebApplication\Libraries rmdir VB\ProntoWebApplication\Libraries
if exist VB\ProntoWebApplication\admin rmdir VB\ProntoWebApplication\admin

mklink /J VB\ProntoWebApplication\Libraries ..\Source\Pronto\bin\Release
mklink /J VB\ProntoWebApplication\admin ..\Source\ClientAdmin