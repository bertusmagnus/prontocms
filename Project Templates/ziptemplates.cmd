if exist ProntoCMS-CSharp.zip del ProntoCMS-CSharp.zip

pushd .
cd CSharp

..\..\7zip\7za.exe a -tzip ..\ProntoCMS-CSharp.zip __TemplateIcon.ico MyTemplate.vstemplate

cd ProntoCmsWebApplication
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-CSharp.zip Web.config Global.asax Default.aspx ProntoCmsWebApplication.csproj admin\ App_Data\ Libraries\ Properties\ themes\ templates\

cd ..\..

cd VB
..\..\7zip\7za.exe a -tzip ..\ProntoCMS-VB.zip __TemplateIcon.ico MyTemplate.vstemplate

cd ProntoCmsWebApplication
..\..\..\7zip\7za.exe a -tzip -xr!?svn\ ..\..\ProntoCMS-VB.zip Web.config Global.asax Default.aspx ProntoCmsWebApplication.vbproj admin\ App_Data\ Libraries\ "My Project\" themes\ templates\


popd