﻿using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 定义本工具中的常量以及供各个类之间读取的常量
/// </summary>
public class AppValues
{
    /// <summary>
    /// Excel文件中存放数据的工作簿Sheet名。除预设功能的特殊Sheet表之外，其余Sheet表可自定义内容，不会被本工具导出
    /// </summary>
    public const string EXCEL_DATA_SHEET_NAME = "data$";

    /// <summary>
    /// Excel文件中存放该表格配置的工作簿Sheet名
    /// </summary>
    public const string EXCEL_CONFIG_SHEET_NAME = "config$";

    /// <summary>
    /// Excel临时文件的文件名前缀
    /// </summary>
    public const string EXCEL_TEMP_FILE_FILE_NAME_START_STRING = "~$";

    /// <summary>
    /// 声明将Excel所在文件夹下属子文件夹中的Excel文件也进行导出的命令参数
    /// </summary>
    public const string EXPORT_INCLUDE_SUBFOLDER_PARAM_STRING = "-exportIncludeSubfolder";

    /// <summary>
    /// 声明将生成的文件按原Excel文件所在的目录结构进行存储的命令参数
    /// </summary>
    public const string EXPORT_KEEP_DIRECTORY_STRUCTURE_PARAM_STRING = "-exportKeepDirectoryStructure";

    /// <summary>
    /// 声明将表格导出到MySQL数据库的命令参数
    /// </summary>
    public const string EXPORT_MYSQL_PARAM_STRING = "-exportMySQL";

    /// <summary>
    /// 声明在生成的lua文件开头以注释形式展示列信息的命令参数
    /// </summary>
    public const string NEED_COLUMN_INFO_PARAM_STRING = "-columnInfo";

    /// <summary>
    /// 声明不进行表格检查的命令参数
    /// </summary>
    public const string UNCHECKED_PARAM_STRING = "-unchecked";

    /// <summary>
    /// 声明不导出lua文件的命令参数
    /// </summary>
    public const string NOT_EXPORT_LUA_PARAM_STRING = "-notExportLua";

    /// <summary>
    /// 声明不指定项目Client目录的命令参数
    /// </summary>
    public const string NO_CLIENT_PATH_PARAM_STRING = "-noClient";

    /// <summary>
    /// 声明不含有lang文件的命令参数
    /// </summary>
    public const string NO_LANG_PARAM_STRING = "-noLang";

    /// <summary>
    /// 声明当lang型数据key在lang文件中找不到对应值时，在lua文件输出字段值为空字符串的命令参数
    /// </summary>
    public const string LANG_NOT_MATCHING_PRINT_PARAM_STRING = "-printEmptyStringWhenLangNotMatching";

    /// <summary>
    /// 声明只将部分指定Excel表进行导出的命令参数
    /// </summary>
    public const string PART_EXPORT_PARAM_STRING = "-part";

    /// <summary>
    /// 声明忽略对指定Excel表进行导出的命令参数
    /// </summary>
    public const string EXCEPT_EXPORT_PARAM_STRING = "-except";

    /// <summary>
    /// 声明允许int、float型字段中存在空值的命令参数
    /// </summary>
    public const string ALLOWED_NULL_NUMBER_PARAM_STRING = "-allowedNullNumber";

    /// <summary>
    /// 声明额外导出为其他格式文件时，涉及所有文件
    /// </summary>
    public const string EXPORT_ALL_TO_EXTRA_FILE_PARAM_STRING = "$all";

    /// <summary>
    /// 声明将指定的Excel文件额外导出为csv文件
    /// </summary>
    public const string EXPORT_CSV_PARAM_STRING = "-exportCsv";

    /// <summary>
    /// 声明导出csv文件时的参数
    /// </summary>
    public const string EXPORT_CSV_PARAM_PARAM_STRING = "-exportCsvParam";

    /// <summary>
    /// 声明将指定的Excel文件额外导出为csv对应C#类文件
    /// </summary>
    public const string EXPORT_CS_CLASS_PARAM_STRING = "-exportCsClass";

    /// <summary>
    /// 声明导出与csv对应C#类文件的参数
    /// </summary>
    public const string EXPORT_CS_CLASS_PARAM_PARAM_STRING = "-exportCsClassParam";

    /// <summary>
    /// 声明将指定的Excel文件额外导出为csv对应Java类文件
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_STRING = "-exportJavaClass";

    /// <summary>
    /// 声明导出与csv对应Java类文件的参数
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_PARAM_STRING = "-exportJavaClassParam";

    /// <summary>
    /// 声明将指定的Excel文件额外导出为json文件
    /// </summary>
    public const string EXPORT_JSON_PARAM_STRING = "-exportJson";

    /// <summary>
    /// 声明导出json文件时的参数
    /// </summary>
    public const string EXPORT_JSON_PARAM_PARAM_STRING = "-exportJsonParam";

    /// <summary>
    /// 导出csv文件参数下属的具体参数，用于配置导出路径
    /// </summary>
    public const string EXPORT_CSV_PARAM_EXPORT_PATH_PARAM_STRING = "exportPath";

    /// <summary>
    /// 导出csv文件参数下属的具体参数，用于配置导出文件的扩展名
    /// </summary>
    public const string EXPORT_CSV_PARAM_EXTENSION_PARAM_STRING = "extension";

    /// <summary>
    /// 导出csv文件参数下属的具体参数，用于配置字段间的分隔符
    /// </summary>
    public const string EXPORT_CSV_PARAM_SPLIT_STRING_PARAM_STRING = "splitString";

    /// <summary>
    /// 导出csv文件参数下属的具体参数，用于配置是否在首行列举字段名称
    /// </summary>
    public const string EXPORT_CSV_PARAM_IS_EXPORT_COLUMN_NAME_PARAM_STRING = "isExportColumnName";

    /// <summary>
    /// 导出csv文件参数下属的具体参数，用于配置是否在其后列举字段数据类型
    /// </summary>
    public const string EXPORT_CSV_PARAM_IS_EXPORT_COLUMN_DATA_TYPE_PARAM_STRING = "isExportColumnDataType";

    /// <summary>
    /// 导出csv对应C#类文件参数下属的具体参数，用于配置导出路径
    /// </summary>
    public const string EXPORT_CS_CLASS_PARAM_EXPORT_PATH_PARAM_STRING = "exportPath";

    /// <summary>
    /// 导出csv对应C#类文件参数下属的具体参数，用于配置命名空间
    /// </summary>
    public const string EXPORT_CS_CLASS_PARAM_NAMESPACE_PARAM_STRING = "namespace";

    /// <summary>
    /// 导出csv对应C#类文件参数下属的具体参数，用于配置引用类库
    /// </summary>
    public const string EXPORT_CS_CLASS_PARAM_USING_PARAM_STRING = "using";

    /// <summary>
    /// 导出csv对应Java类文件参数下属的具体参数，用于配置导出路径
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_EXPORT_PATH_PARAM_STRING = "exportPath";

    /// <summary>
    /// 导出csv对应Java类文件参数下属的具体参数，用于配置包名
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_PACKAGE_PARAM_STRING = "package";

    /// <summary>
    /// 导出csv对应Java类文件参数下属的具体参数，用于配置引用类库
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_IMPORT_PARAM_STRING = "import";

    /// <summary>
    /// 导出csv对应Java类文件参数下属的具体参数，用于配置是否将日期型转为Date而不是Calendar类型
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_IS_USE_DATE_PARAM_STRING = "isUseDate";

    /// <summary>
    /// 导出csv对应Java类文件参数下属的具体参数，用于配置是否生成无参构造函数
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_IS_GENERATE_CONSTRUCTOR_WITHOUT_FIELDS_PARAM_STRING = "isGenerateConstructorWithoutFields";

    /// <summary>
    /// 导出csv对应Java类文件参数下属的具体参数，用于配置是否生成含全部参数的构造函数
    /// </summary>
    public const string EXPORT_JAVA_CLASS_PARAM_IS_GENERATE_CONSTRUCTOR_WITH_ALL_FIELDS_PARAM_STRING = "isGenerateConstructorWithAllFields";

    /// <summary>
    /// 对导出csv对应C#或Java类文件进行自动命名的参数
    /// </summary>
    public const string AUTO_NAME_CSV_CLASS_PARAM_STRING = "-autoNameCsvClassParam";

    /// <summary>
    /// 对导出csv对应C#或Java类文件自动命名参数下属的具体参数，用于配置在类名前增加的前缀
    /// </summary>
    public const string AUTO_NAME_CSV_CLASS_PARAM_CLASS_NAME_PREFIX_PARAM_STRING = "classNamePrefix";

    /// <summary>
    /// 对导出csv对应C#或Java类文件自动命名参数下属的具体参数，用于配置在类名前增加的后缀
    /// </summary>
    public const string AUTO_NAME_CSV_CLASS_PARAM_CLASS_NAME_POSTFIX_PARAM_STRING = "classNamePostfix";

    /// <summary>
    /// 导出json文件参数下属的具体参数，用于配置导出路径
    /// </summary>
    public const string EXPORT_JSON_PARAM_EXPORT_PATH_PARAM_STRING = "exportPath";

    /// <summary>
    /// 导出json文件参数下属的具体参数，用于配置导出文件的扩展名
    /// </summary>
    public const string EXPORT_JSON_PARAM_EXTENSION_PARAM_STRING = "extension";

    /// <summary>
    /// 导出json文件参数下属的具体参数，用于配置是否将生成的json字符串整理为带缩进格式的形式
    /// </summary>
    public const string EXPORT_JSON_PARAM_IS_FORMAT_PARAM_STRING = "isFormat";

    /// <summary>
    /// 导出json文件参数下属的具体参数，用于配置是否生成为各行数据对应的json object包含在一个json array的形式
    /// </summary>
    public const string EXPORT_JSON_PARAM_IS_EXPORT_JSON_ARRAY_FORMAT_PARAM_STRING = "isExportJsonArrayFormat";

    /// <summary>
    /// 导出json文件参数下属的具体参数，用于配置若生成包含在一个json object的形式，是否使每行字段信息对应的json object中包含主键列对应的键值对
    /// </summary>
    public const string EXPORT_JSON_PARAM_IS_MAP_INCLUDE_KEY_COLUMN_VALUE_PARAM_STRING = "isMapIncludeKeyColumnValue";

    /// <summary>
    /// 导出csv对应C#类文件的扩展名（不含点号）
    /// </summary>
    public static string EXPORT_CS_CLASS_FILE_EXTENSION = "cs";

    /// <summary>
    /// 导出csv对应Java类文件的扩展名（不含点号）
    /// </summary>
    public static string EXPORT_JAVA_CLASS_FILE_EXTENSION = "java";

    /// <summary>
    /// 配置文件（配置自定义的检查规则）的文件名
    /// </summary>
    public const string CONFIG_FILE_NAME = "config.txt";

    // 每张数据表前五行分别声明字段描述、字段变量名、字段数据类型、字段检查规则、导出到数据库中的字段名及类型（行编号从0开始）
    public const int DATA_FIELD_DESC_INDEX = 0;
    public const int DATA_FIELD_NAME_INDEX = 1;
    public const int DATA_FIELD_DATA_TYPE_INDEX = 2;
    public const int DATA_FIELD_CHECK_RULE_INDEX = 3;
    public const int DATA_FIELD_EXPORT_DATABASE_FIELD_INFO = 4;
    public const int DATA_FIELD_DATA_START_INDEX = 5;

    // 每张配置表中的一列为一个配置参数的声明，其中第一行声明参数名，其余行声明具体参数（行编号从0开始）
    public const int CONFIG_FIELD_DEFINE_INDEX = 0;
    public const int CONFIG_FIELD_PARAM_START_INDEX = 1;

    // 声明整表检查的配置参数名
    public const string CONFIG_NAME_CHECK_TABLE = "tableCheckRule";
    // 声明对某张表格设置特殊导出规则的配置参数名
    public const string CONFIG_NAME_EXPORT = "tableExportConfig";
    // 声明某张表格导出到数据库中的表名
    public const string CONFIG_NAME_EXPORT_DATABASE_TABLE_NAME = "exportDatabaseTableName";
    // 声明某张表格导出到数据库中的说明信息
    public const string CONFIG_NAME_EXPORT_DATABASE_TABLE_COMMENT = "exportDatabaseTableComment";
    // 声明某张表格导出到数据库中时string型字段中的空白单元格导出为数据库中的NULL
    public const string CONFIG_NAME_EXPORT_DATABASE_WRITE_NULL_FOR_EMPTY_STRING = "exportDatabaseWriteNullForEmptyString";
    // 声明某张表格导出为lua table时，是否将主键列的值作为table中的元素
    public const string CONFIG_NAME_ADD_KEY_TO_LUA_TABLE = "addKeyToLuaTable";
    // 声明某张表格导出csv对应C#或Java文件的类名
    public const string CONFIG_NAME_EXPORT_CSV_CLASS_NAME = "exportCsvClassName";

    // 声明对某张表格不进行默认导出的参数配置
    public const string CONFIG_PARAM_NOT_EXPORT_ORIGINAL_TABLE = "-notExportOriginalTable";

    // 以下为config配置文件中配置项的key名
    // MySQL连接字符串
    public const string APP_CONFIG_KEY_MYSQL_CONNECT_STRING = "connectMySQLString";
    // 创建MySQL数据库表格时额外指定的参数字符串
    public const string APP_CONFIG_KEY_CREATE_DATABASE_TABLE_EXTRA_PARAM = "createDatabaseTableExtraParam";
    // 未声明date型的输入格式时所采用的默认格式
    public const string APP_CONFIG_KEY_DEFAULT_DATE_INPUT_FORMAT = "defaultDateInputFormat";
    // 未声明date型导出至lua文件的格式时所采用的默认格式
    public const string APP_CONFIG_KEY_DEFAULT_DATE_TO_LUA_FORMAT = "defaultDateToLuaFormat";
    // 未声明date型导出至MySQL数据库的格式时所采用的默认格式
    public const string APP_CONFIG_KEY_DEFAULT_DATE_TO_DATABASE_FORMAT = "defaultDateToDatabaseFormat";
    // 未声明time型的输入格式时所采用的默认格式
    public const string APP_CONFIG_KEY_DEFAULT_TIME_INPUT_FORMAT = "defaultTimeInputFormat";
    // 未声明time型导出至lua文件的格式时所采用的默认格式
    public const string APP_CONFIG_KEY_DEFAULT_TIME_TO_LUA_FORMAT = "defaultTimeToLuaFormat";
    // 未声明time型导出至MySQL数据库的格式时所采用的默认格式
    public const string APP_CONFIG_KEY_DEFAULT_TIME_TO_DATABASE_FORMAT = "defaultTimeToDatabaseFormat";

    // 以下为TableInfo的ExtraParam所支持的key声明
    // date型的输入格式
    public const string TABLE_INFO_EXTRA_PARAM_KEY_DATE_INPUT_FORMAT = "dateInputFormat";
    // date型导出至lua文件的格式
    public const string TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_LUA_FORMAT = "dateToLuaFormat";
    // date型导出至MySQL数据库的格式
    public const string TABLE_INFO_EXTRA_PARAM_KEY_DATE_TO_DATABASE_FORMAT = "dateToDatabaseFormat";
    // time型的输入格式
    public const string TABLE_INFO_EXTRA_PARAM_KEY_TIME_INPUT_FORMAT = "timeInputFormat";
    // time型导出至lua文件的格式
    public const string TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_LUA_FORMAT = "timeToLuaFormat";
    // time型导出至MySQL数据库的格式
    public const string TABLE_INFO_EXTRA_PARAM_KEY_TIME_TO_DATABASE_FORMAT = "timeToDatabaseFormat";

    // 将MySQL中datetime、date型的默认格式作为本工具对date、time两种时间型进行检查并发现错误后的输出格式
    public const string APP_DEFAULT_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";
    public const string APP_DEFAULT_TIME_FORMAT = "HH:mm:ss";
    // 导出数据到MySQL中的date型字段的默认格式
    public const string APP_DEFAULT_ONLY_DATE_FORMAT = "yyyy-MM-dd";

    /// <summary>
    /// 本工具所在目录，不能用System.Environment.CurrentDirectory因为当本工具被其他程序调用时取得的CurrentDirectory将是调用者的路径
    /// </summary>
    public static string PROGRAM_FOLDER_PATH = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

    /// <summary>
    /// 以1970年1月1日作为计算距今秒数的参考时间，并且作为存储time型的DateTime变量的日期部分，此时间的DateTimeKind为Unspecified
    /// </summary>
    public static DateTime REFERENCE_DATE = new DateTime(1970, 1, 1, 0, 0, 0);

    /// <summary>
    /// 此时间的DateTimeKind为Local，用于转为时间戳时用当前时区表示。北京时间的时间戳0表示1970-01-01 08:00:00
    /// </summary>
    public static DateTime REFERENCE_DATE_LOCAL = TimeZone.CurrentTimeZone.ToLocalTime(REFERENCE_DATE);

    /// <summary>
    /// 未对某字段命名时，默认给予的字段名前缀
    /// </summary>
    public static string AUTO_FIELD_NAME_PREFIX = "未命名字段";

    /// <summary>
    /// 用户输入的要导出的Excel文件所在目录
    /// </summary>
    public static string ExcelFolderPath = null;

    /// <summary>
    /// 用户输入的要生成的lua文件存放路径
    /// </summary>
    public static string ExportLuaFilePath = null;

    /// <summary>
    /// 用户选择的是否要导出Excel文件夹下属子文件夹中的Excel文件（默认为不包含子文件夹）
    /// </summary>
    public static bool IsExportIncludeSubfolder = false;

    /// <summary>
    /// 用户选择的当导出Excel文件夹下属子文件夹中的Excel文件时，是否将生成的文件按原Excel文件所在的目录结构进行存储（默认为直接存储在同级目录下）
    /// </summary>
    public static bool IsExportKeepDirectoryStructure = false;

    /// <summary>
    /// 用户输入的国际化文件所在路径
    /// </summary>
    public static string LangFilePath = null;

    /// <summary>
    /// 用户输入的Client目录所在路径
    /// </summary>
    public static string ClientPath = null;

    /// <summary>
    /// 用户输入的是否需要检查表格（默认为检查）
    /// </summary>
    public static bool IsNeedCheck = true;

    /// <summary>
    /// 当lang型数据key在lang文件中找不到对应值时，是否在lua文件输出字段值为空字符（默认为输出nil）
    /// </summary>
    public static bool IsPrintEmptyStringWhenLangNotMatching = false;

    /// <summary>
    /// 用户输入的是否导出表格数据到MySQL数据库
    /// </summary>
    public static bool IsExportMySQL = false;

    /// <summary>
    /// 用户输入的是否需要在生成lua文件的最上方用注释形式显示列信息（默认为不需要）
    /// </summary>
    public static bool IsNeedColumnInfo = false;

    /// <summary>
    /// 用户输入的是否允许int、float型字段中存在空值
    /// </summary>
    public static bool IsAllowedNullNumber = false;

    /// <summary>
    /// 未声明date型的输入格式时所采用的默认格式
    /// </summary>
    public static string DefaultDateInputFormat = null;

    /// <summary>
    /// 未声明date型导出至lua文件的格式时所采用的默认格式
    /// </summary>
    public static string DefaultDateToLuaFormat = null;

    /// <summary>
    /// 未声明date型导出至MySQL数据库的格式时所采用的默认格式
    /// </summary>
    public static string DefaultDateToDatabaseFormat = null;

    /// <summary>
    /// 未声明time型的输入格式时所采用的默认格式
    /// </summary>
    public static string DefaultTimeInputFormat = null;

    /// <summary>
    /// 未声明time型导出至lua文件的格式时所采用的默认格式
    /// </summary>
    public static string DefaultTimeToLuaFormat = null;

    /// <summary>
    /// 未声明time型导出至MySQL数据库的格式时所采用的默认格式
    /// </summary>
    public static string DefaultTimeToDatabaseFormat = null;

    /// <summary>
    /// lang文件转为键值对形式（key：lang文件中的key名， value：对应的在指定语言下的翻译）
    /// </summary>
    public static Dictionary<string, string> LangData = new Dictionary<string, string>();

    /// <summary>
    /// config文件转为键值对形式（key：配置文件中的key名， value：对应的配置规则字符串）
    /// </summary>
    public static Dictionary<string, string> ConfigData = new Dictionary<string, string>();

    /// <summary>
    /// 存储每张Excel表格解析成的本工具所需的数据结构（key：表名）
    /// </summary>
    public static Dictionary<string, TableInfo> TableInfo = new Dictionary<string, TableInfo>();

    /// <summary>
    /// 存储本次要导出的Excel文件名对应的文件所在路径（key：表名， value：文件所在路径）
    /// </summary>
    public static Dictionary<string, string> ExportTableNameAndPath = new Dictionary<string, string>();

    /// <summary>
    /// 存储本次忽略导出的Excel文件名
    /// </summary>
    public static List<string> ExceptExportTableNames = new List<string>();

    /// <summary>
    /// 存储本次要额外导出为csv文件的Excel文件名
    /// </summary>
    public static List<string> ExportCsvTableNames = new List<string>();

    /// <summary>
    /// 导出csv文件的存储路径
    /// </summary>
    public static string ExportCsvPath = null;

    /// <summary>
    /// 导出csv文件的扩展名（不含点号），默认为csv
    /// </summary>
    public static string ExportCsvExtension = "csv";

    /// <summary>
    /// 导出csv文件中的字段分隔符，默认为英文逗号
    /// </summary>
    public static string ExportCsvSplitString = ",";

    /// <summary>
    /// 导出的csv文件中是否在首行列举字段名称，默认为是
    /// </summary>
    public static bool ExportCsvIsExportColumnName = true;

    /// <summary>
    /// 导出的csv文件中是否在其后列举字段数据类型，默认为是
    /// </summary>
    public static bool ExportCsvIsExportColumnDataType = true;

    /// <summary>
    /// 存储本次要额外导出为csv对应C#类文件的Excel文件名
    /// </summary>
    public static List<string> ExportCsClassTableNames = new List<string>();

    /// <summary>
    /// 导出csv对应C#类文件的存储路径
    /// </summary>
    public static string ExportCsClassPath = null;

    /// <summary>
    /// 导出csv对应C#类文件中的命名空间
    /// </summary>
    public static string ExportCsClassNamespace = null;

    /// <summary>
    /// 导出csv对应C#类文件中的引用类库
    /// </summary>
    public static List<string> ExportCsClassUsing = null;

    /// <summary>
    /// 存储本次要额外导出为csv对应Java类文件的Excel文件名
    /// </summary>
    public static List<string> ExportJavaClassTableNames = new List<string>();

    /// <summary>
    /// 导出csv对应Java类文件的存储路径
    /// </summary>
    public static string ExportJavaClassPath = null;

    /// <summary>
    /// 导出csv对应Java类文件的包名
    /// </summary>
    public static string ExportJavaClassPackage = null;

    /// <summary>
    /// 导出csv对应Java类文件中的引用类库
    /// </summary>
    public static List<string> ExportJavaClassImport = null;

    /// <summary>
    /// 导出csv对应Java类文件中，是否将时间型转为Date而不是Calendar，默认为true
    /// </summary>
    public static bool ExportJavaClassIsUseDate = true;

    /// <summary>
    /// 导出csv对应Java类文件中，是否生成无参构造函数，默认为false
    /// </summary>
    public static bool ExportJavaClassisGenerateConstructorWithoutFields = false;

    /// <summary>
    /// 导出csv对应Java类文件中，是否生成含全部参数的构造函数，默认为false
    /// </summary>
    public static bool ExportJavaClassIsGenerateConstructorWithAllFields = false;

    /// <summary>
    /// 导出csv对应C#或Java类的类名前缀
    /// </summary>
    public static string ExportCsvClassClassNamePrefix = null;

    /// <summary>
    /// 导出csv对应C#或Java类的类名后缀
    /// </summary>
    public static string ExportCsvClassClassNamePostfix = null;

    /// <summary>
    /// 存储本次要额外导出为json文件的Excel文件名
    /// </summary>
    public static List<string> ExportJsonTableNames = new List<string>();

    /// <summary>
    /// 导出json文件的存储路径
    /// </summary>
    public static string ExportJsonPath = null;

    /// <summary>
    /// 导出json文件的扩展名（不含点号），默认为txt
    /// </summary>
    public static string ExportJsonExtension = "txt";

    /// <summary>
    /// 导出的json文件中是否将json字符串整理为带缩进格式的形式，默认为否
    /// </summary>
    public static bool ExportJsonIsFormat = false;

    /// <summary>
    /// 导出的json文件是否生成为各行数据对应的json object包含在一个json array的形式，默认为是
    /// </summary>
    public static bool ExportJsonIsExportJsonArrayFormat = true;

    /// <summary>
    /// 导出的json文件，若生成包含在一个json object的形式，是否使每行字段信息对应的json object中包含主键列对应的键值对，默认为是
    /// </summary>
    public static bool ExportJsonIsExportJsonMapIncludeKeyColumnValue = true;

    /// <summary>
    /// 存储运行时打印的所有信息，在程序运行完毕后输出为txt文件，从而解决如果输出内容过多控制台无法显示全部信息的问题
    /// </summary>
    public static StringBuilder LogContent = new StringBuilder();
}
