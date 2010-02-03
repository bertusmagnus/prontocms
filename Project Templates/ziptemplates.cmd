if exist ProntoCMS-CSharp.zip del ProntoCMS-CSharp.zip

pushd .
cd CSharp

..\..\7zip\7za.exe a -tzip ..\ProntoCMS-CSharp.zip __TemplateIcon.ico MyTemplate.vstemplate

cd ProntoCmsWebApplication
..\..\..\7zip\7za.exe a -tzip ..\..\ProntoCMS-CSharp.zip Web.config Global.asax Default.aspx ProntoWebApplication.csproj
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip admin\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip App_Data\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip Libraries\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip Properties\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip themes\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip templates\

cd ..

cd VB
..\..\7zip\7za.exe a -tzip ..\ProntoCMS-VB.zip __TemplateIcon.ico MyTemplate.vstemplate

cd ProntoCmsWebApplication
..\..\..\7zip\7za.exe a -tzip ..\..\ProntoCMS-VB.zip Web.config Global.asax Default.aspx ProntoWebApplication.vbproj
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip admin\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip App_Data\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip Libraries\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip Properties\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip themes\
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip templates\


popd