rmdir /S /Q Archives\prontocms
del Archives\ProntoCms-source.zip

svn export Source Archives/prontocms
cd Archives
"C:\Program Files\7-Zip\7z.exe" a -r -mx9 ProntoCms-Source.zip prontocms
cd ..