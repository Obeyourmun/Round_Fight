@echo off
XlsxToLua.exe TestExcel ExportLua ClientVirtual lang.txt -columnInfo -allowedNullNumber -printEmptyStringWhenLangNotMatching
set errorLevel = %errorlevel%
if errorLevel == 0 (
	@echo �����ɹ�
) else (
	@echo ����ʧ��
)
pause