namespace UGFramework.Editor
{
    using UGFramework.Core;
    using LitJson;
    using UnityEditor;
    using UnityEngine;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System;
    using System.Collections;
    using SqlCipher4Unity3D;
    using ExcelDataReader;
    using System.Data;

    public class DaoUtility
    {
        //---------- Following defination should be customed according to project. ----------
        public static readonly string ConfigFilePath = Application.dataPath + "/export_db_config.json";
        public static readonly string DbExportPath = Application.dataPath + "/StreamingAssets/db";
        public static readonly string ExcelFilePath = Application.dataPath + "/../../../Design";
        public static readonly string CodeGeneratePath = Application.dataPath + "/Patch/Daos";
        public static readonly string DbPassword = ""; //"@(7$$5)1";
        //---------- Above defination should be customed according to project. ----------

        public class ExcelTable
        {
            private DataTable m_Data = null;
            private string m_TableName = string.Empty;
            private bool m_IsValid = false;
            private int m_PrimaryKeyIndex = -1;
            private List<string> m_FieldNames = new List<string>();
            private List<string> m_FieldTypes = new List<string>();

            private string m_CreateTableSqlCmd = string.Empty;
            private List<string> m_UpdateTableSqlCmdList = new List<string>();

            public bool IsValid
            {
                get
                {
                    return m_IsValid;
                }
            }

            public string TableName
            {
                get
                {
                    return m_TableName;
                }
            }

            public DataTable Data
            {
                get
                {
                    return m_Data;
                }
            }

            public string CreateTableSqlCommand
            {
                get
                {
                    return m_CreateTableSqlCmd;
                }
            }

            public List<string> UpdateTableSqlCmdList
            {
                get
                {
                    return m_UpdateTableSqlCmdList;
                }
            }

            public ExcelTable(string TableName, DataTable TableData)
            {
                if (string.IsNullOrEmpty(TableName) || TableData == null)
                {
                    return;
                }

                int _Columns = TableData.Columns.Count;
                int _Rows = TableData.Rows.Count;

                if (_Columns <= 0 || _Rows < 2)
                {
                    Debug.LogError("[Export Data] Columns is 0 or rows is less than 2 in [" + TableName + "]");
                    return;
                }

                m_TableName = TableName;
                m_Data = TableData;

                for (int i = 0; i < _Columns; ++ i)
                {
                    string _FieldName = TableData.Rows[1][i].ToString();

                    if (string.IsNullOrEmpty(_FieldName))
                    {
                        Debug.LogError("[Export Data] Field name is null or empty in column " + i + "[" + TableName + "]");
                        return;
                    }

                    m_FieldNames.Add(_FieldName);

                    string _FieldType = TableData.Rows[0][i].ToString();

                    if (string.IsNullOrEmpty(_FieldType))
                    {
                        Debug.LogError("[Export Data] Field type is null or empty in column " + i + "[" + TableName + "]");
                        return;
                    }

                    _FieldType = _FieldType.ToLower().Trim();

                    if (_FieldType.Contains("primary key"))
                    {
                        if (m_PrimaryKeyIndex != -1)
                        {
                            Debug.LogError("[Export Data] More than one primary key is detected.");
                            return;
                        }

                        m_PrimaryKeyIndex = i;

                        _FieldType = _FieldType.Replace("primary key", "");
                        _FieldType = _FieldType.Replace(",", "");
                        _FieldType = _FieldType.Trim();
                    }

                    _FieldType = _FieldType.ToUpper();
                    m_FieldTypes.Add(_FieldType);
                }

                if (m_PrimaryKeyIndex == -1)
                {
                    Debug.LogError("[Export Data] No primary key is detected.");
                    return;
                }

                m_CreateTableSqlCmd = GenerateCreateTableSqlString();

                m_IsValid = true;
            }

            protected string GenerateCreateTableSqlString()
            {
                string _Ret = string.Empty;

                _Ret += "CREATE TABLE '"; // CREATE TABLE '
                _Ret += m_TableName; // CREATE TABLE '[Table Name]
                _Ret += "' ("; // CREATE TABLE '[Table Name]' (
                _Ret += "'" + m_FieldNames[m_PrimaryKeyIndex] + "' " + m_FieldTypes[m_PrimaryKeyIndex] + " PRIMARY KEY NOT NULL";

                for (int i = 0; i < m_Data.Columns.Count; ++i)
                {
                    if (i == m_PrimaryKeyIndex)
                    {
                        continue;
                    }

                    _Ret += ", '" + m_FieldNames[i] + "' " + m_FieldTypes[i] + " NOT NULL";
                }

                _Ret += ")"; // CREATE TABLE '[Table Name]' ()

                return _Ret;
            }

            public void GenerateInsertDataSqlString(int RowIndex, out string Command, out List<object> ParamList)
            {
                Command = string.Empty;
                ParamList = null;
                
                if (m_Data.Columns.Count <= 0 || RowIndex < 0 || RowIndex > m_Data.Rows.Count)
                {
                    return;
                }

                ParamList = new List<object>();

                Command += "INSERT INTO '";
                Command += m_TableName;
                Command += "' (";

                string _FieldNames = string.Empty;
                string _FieldValuePlaceholder = string.Empty;

                for (int i = 0; i < m_FieldNames.Count; ++i)
                {
                    _FieldNames += (i == 0) ? (m_FieldNames[i]) : (", " + m_FieldNames[i]);
                    _FieldValuePlaceholder += (i == 0) ? "?" : ", ?";

                    if (i < m_FieldTypes.Count)
                    {
                        if (m_FieldTypes[i] == "INTEGER")
                        {
                            ParamList.Add(Convert.ToInt64(m_Data.Rows[RowIndex][i].ToString()));
                        }
                        else if (m_FieldTypes[i] == "TEXT")
                        {
                            ParamList.Add(m_Data.Rows[RowIndex][i].ToString());
                        }
                        else if (m_FieldTypes[i] == "REAL")
                        {
                            ParamList.Add(Convert.ToSingle(m_Data.Rows[RowIndex][i].ToString()));
                        }
                        else
                        {
                            ParamList.Add(null);
                        }
                    }
                }

                Command += _FieldNames;
                Command += ") VALUES (";
                Command += _FieldValuePlaceholder;
                Command += ")";
            }

            public void GenerateDaoAccessCode(string Path, string DbName)
            {
                if (string.IsNullOrEmpty(Path))
                {
                    return;
                }

                string _DaoSrc = string.Empty;
                _DaoSrc += "namespace ProjectX.Daos\n";
                _DaoSrc += "{\n";
                _DaoSrc += "\tusing SQLite.Attribute;\n";
                _DaoSrc += "\tusing UGFramework.Core;\n";
                _DaoSrc += "\tusing UnityEngine.Scripting;\n\n";
                _DaoSrc += "\t[Preserve]\n";
                _DaoSrc += "\tpublic class " + m_TableName + ": DaoVO\n";
                _DaoSrc += "\t{\n";
                _DaoSrc += "\t\t[PrimaryKey]\n";
                _DaoSrc += "\t\tpublic " + GetFieldCodeTypeByIndex(m_PrimaryKeyIndex) + " " + m_FieldNames[m_PrimaryKeyIndex] + "{ get; set; }\n";
                for (int i = 0; i < m_FieldNames.Count; ++i)
                {
                    if (i == m_PrimaryKeyIndex)
                    {
                        continue;
                    }

                    _DaoSrc += "\t\tpublic " + GetFieldCodeTypeByIndex(i) + " " + m_FieldNames[i] + "{ get; set; }\n";
                }
                _DaoSrc += "\t}\n";
                _DaoSrc += "}\n";

                FileStream _DaoStream = null;
                StreamWriter _DaoWriter = null;

                try
                {
                    _DaoStream = new FileStream(Path + "/" + m_TableName + ".cs", FileMode.Create);

                    if (_DaoStream != null)
                    {
                        _DaoWriter = new StreamWriter(_DaoStream);

                        if (_DaoWriter != null)
                        {
                            _DaoWriter.Write(_DaoSrc);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                finally
                {
                    if (_DaoWriter != null)
                    {
                        _DaoWriter.Close();
                        _DaoWriter.Dispose();
                        _DaoWriter = null;
                    }

                    if (_DaoStream != null)
                    {
                        _DaoStream.Close();
                        _DaoStream.Dispose();
                        _DaoStream = null;
                    }
                }

                string _DaoAccessSrc = string.Empty;
                string _CacheValueName = "m_DataCache";
                string _DaoIndexName = m_TableName + "Id";
                _DaoAccessSrc += "namespace ProjectX.Daos\n";
                _DaoAccessSrc += "{\n";
                _DaoAccessSrc += "\tusing System.Collections.Generic;\n";
                _DaoAccessSrc += "\tusing UGFramework.Core;\n\n";
                _DaoAccessSrc += "\tpublic class " + m_TableName + "Dao: Dao\n";
                _DaoAccessSrc += "\t{\n";
                _DaoAccessSrc += "\t\tprotected Dictionary<" + GetFieldCodeTypeByIndex(m_PrimaryKeyIndex) + ", " + m_TableName + "> " + _CacheValueName + " = null;\n\n";
                _DaoAccessSrc += "\t\tpublic " + m_TableName + "Dao(string DbPath): base(DbPath)\n";
                _DaoAccessSrc += "\t\t{\n";
                _DaoAccessSrc += "\t\t\t" + _CacheValueName + " = new Dictionary<" + GetFieldCodeTypeByIndex(m_PrimaryKeyIndex) + ", " + m_TableName + ">();\n";
                _DaoAccessSrc += "\t\t}\n";
                _DaoAccessSrc += "\t\t~" + m_TableName + "Dao()\n";
                _DaoAccessSrc += "\t\t{\n";
                _DaoAccessSrc += "\t\t\tif (" + _CacheValueName + " != null)\n";
                _DaoAccessSrc += "\t\t\t{\n";
                _DaoAccessSrc += "\t\t\t\t" + _CacheValueName + ".Clear();\n";
                _DaoAccessSrc += "\t\t\t\t" + _CacheValueName + " = null;\n";
                _DaoAccessSrc += "\t\t\t}\n";
                _DaoAccessSrc += "\t\t}\n";
                _DaoAccessSrc += "\t\tpublic " + m_TableName + " Get" + m_TableName + "ByIndex(" + GetFieldCodeTypeByIndex(m_PrimaryKeyIndex) + " " + _DaoIndexName + ")\n";
                _DaoAccessSrc += "\t\t{\n";
                _DaoAccessSrc += "\t\t\tif (" + _CacheValueName + ".ContainsKey(" + _DaoIndexName + "))\n";
                _DaoAccessSrc += "\t\t\t{\n";
                _DaoAccessSrc += "\t\t\t\treturn " + _CacheValueName + "[" + _DaoIndexName + "];\n";
                _DaoAccessSrc += "\t\t\t}\n";
                _DaoAccessSrc += "\t\t\telse\n";
                _DaoAccessSrc += "\t\t\t{\n";
                _DaoAccessSrc += "\t\t\t\treturn CacheData(" + _DaoIndexName + ");\n";
                _DaoAccessSrc += "\t\t\t}\n";
                _DaoAccessSrc += "\t\t}\n";
                _DaoAccessSrc += "\t\tprotected " + m_TableName + " CacheData(" + GetFieldCodeTypeByIndex(m_PrimaryKeyIndex) + " " + _DaoIndexName + ")\n";
                _DaoAccessSrc += "\t\t{\n";
                _DaoAccessSrc += "\t\t\tif (" + _CacheValueName + ".ContainsKey(" + _DaoIndexName + "))\n";
                _DaoAccessSrc += "\t\t\t{\n";
                _DaoAccessSrc += "\t\t\t\treturn " + _CacheValueName + "[" + _DaoIndexName + "];\n";
                _DaoAccessSrc += "\t\t\t}\n";
                _DaoAccessSrc += "\t\t\t" + m_TableName + " _Ret = null;\n";
                _DaoAccessSrc += "\t\t\tSqlCipher4Unity3D.SQLiteConnection _connection = new SqlCipher4Unity3D.SQLiteConnection(m_DbPath, \"" + DaoUtility.DbPassword +"\");\n";
                _DaoAccessSrc += "\t\t\tif (_connection == null)\n";
                _DaoAccessSrc += "\t\t\t{\n";
                _DaoAccessSrc += "\t\t\t\treturn null;\n";
                _DaoAccessSrc += "\t\t\t}\n";
                _DaoAccessSrc += "\t\t\tIEnumerable<" + m_TableName + "> _RetSet = _connection.Table<" + m_TableName + ">().Where(x => x." + m_FieldNames[m_PrimaryKeyIndex] + " == " + _DaoIndexName + ");\n";
                _DaoAccessSrc += "\t\t\tforeach(var _Iterator in _RetSet)\n";
                _DaoAccessSrc += "\t\t\t{\n";
                _DaoAccessSrc += "\t\t\t\t_Ret = _Iterator;\n";
                _DaoAccessSrc += "\t\t\t\t" + _CacheValueName + ".Add(" + _DaoIndexName + ", _Ret);\n";
                _DaoAccessSrc += "\t\t\t\tbreak;\n";
                _DaoAccessSrc += "\t\t\t}\n";
                _DaoAccessSrc += "\t\t\t_connection.Close();\n";
                _DaoAccessSrc += "\t\t\t_connection.Dispose();\n";
                _DaoAccessSrc += "\t\t\t_connection = null;\n";
                _DaoAccessSrc += "\t\t\treturn _Ret;\n";
                _DaoAccessSrc += "\t\t}\n";
                _DaoAccessSrc += "\t}\n";
                _DaoAccessSrc += "}\n";

                try
                {
                    _DaoStream = new FileStream(Path + "/" + m_TableName + "Dao.cs", FileMode.Create);

                    if (_DaoStream != null)
                    {
                        _DaoWriter = new StreamWriter(_DaoStream);

                        if (_DaoWriter != null)
                        {
                            _DaoWriter.Write(_DaoAccessSrc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                finally
                {
                    if (_DaoWriter != null)
                    {
                        _DaoWriter.Close();
                        _DaoWriter.Dispose();
                        _DaoWriter = null;
                    }

                    if (_DaoStream != null)
                    {
                        _DaoStream.Close();
                        _DaoStream.Dispose();
                        _DaoStream = null;
                    }
                }
            }

            protected string GetFieldCodeTypeByIndex(int Index)
            {
                if (Index < 0 || Index > m_FieldTypes.Count)
                {
                    return string.Empty;
                }

                string _Ret = string.Empty;

                switch (m_FieldTypes[Index])
                {
                    case "INTEGER":
                        _Ret = "int";
                        break;
                    case "REAL":
                        _Ret = "float";
                        break;
                    case "TEXT":
                        _Ret = "string";
                        break;
                }

                return _Ret;
            }
        }

        [MenuItem("UGFramework/Config Data/Create Export Config")]
        public static void ConfigData_CreateExportConfig()
        {
            File.Delete(ConfigFilePath);

            JsonData _Content = new JsonData();
            _Content["[DB Name 1]"] = new JsonData();
            _Content["[DB Name 1]"]["[Excel Table Name 1]"] = "Excel File Name 1";
            _Content["[DB Name 1]"]["[Excel Table Name 2]"] = "Excel File Name 2";
            _Content["[DB Name 2]"] = new JsonData();
            _Content["[DB Name 2]"]["[Excel Table Name 3]"] = "Excel File Name 3";
            _Content["[DB Name 2]"]["[Excel Table Name 4]"] = "Excel File Name 4";

            StringBuilder _Builder = new StringBuilder();

            JsonWriter _Writer = new JsonWriter(_Builder);
            _Writer.PrettyPrint = true;
            _Writer.IndentValue = 2;

            JsonMapper.ToJson(_Content, _Writer);
                        
            File.WriteAllText(ConfigFilePath, _Builder.ToString());

            AssetDatabase.Refresh();
        }

        [MenuItem("UGFramework/Config Data/Export Config Data")]
        public static void ConfigData_ExportConfigData()
        {
            Debug.Log("======> Valid Excel File Path <======");

            if (!Directory.Exists(ExcelFilePath))
            {
                Debug.LogError("[Export Data] Excel data source doesn't exist.");
                return;
            }

            Debug.Log("======> Remove Previous Export Data <======");

            if (Directory.Exists(DbExportPath))
            {
                SystemUtility.DeleteDirectory(DbExportPath);
            }

            Directory.CreateDirectory(DbExportPath);

            Debug.Log("======> Remove Previous Generated Code");

            ConfigData_RemoveAutoGenCode();

            Debug.Log("======> Load Export Config <======");

            FileInfo _ConfigFileInfo = new FileInfo(ConfigFilePath);

            if (_ConfigFileInfo == null || !_ConfigFileInfo.Exists)
            {
                Debug.LogError("[Export Data] Fail to read config file.");
                return;
            }

            Dictionary<string, Dictionary<string, string>> _ConfigSetting = LoadExportConfig();

            if (_ConfigSetting == null || _ConfigSetting.Count <= 0)
            {                
                return;
            }

            Debug.Log("======> Export Data <======");

            ExportData(_ConfigSetting);

            Debug.Log("======> Finish Excel Data Export <======");

            AssetDatabase.Refresh();
        }

        [MenuItem("UGFramework/Config Data/Remove Auto Generated Code")]
        public static void ConfigData_RemoveAutoGenCode()
        {
            if (Directory.Exists(CodeGeneratePath))
            {
                SystemUtility.DeleteDirectory(CodeGeneratePath);
            }

            Directory.CreateDirectory(CodeGeneratePath);

            AssetDatabase.Refresh();
        }

        protected static void ExportData(Dictionary<string, Dictionary<string, string>> ConfigSetting)
        {
            if (ConfigSetting == null)
            {
                return;
            }

            foreach (var _ConfigSettingIterator in ConfigSetting)
            {
                string _ExportDbName = _ConfigSettingIterator.Key;
                string _ExportDbPath = DbExportPath + "/" + _ExportDbName;

                SQLiteConnection _DbConnection = new SQLiteConnection(_ExportDbPath, DbPassword);

                if (_DbConnection == null)
                {
                    Debug.LogError("[Export Data] Cannot create " + _ExportDbName + ".");
                    continue;
                }

                Dictionary<string, string> _ExportSetting = _ConfigSettingIterator.Value;

                if (_ExportSetting == null)
                {
                    continue;
                }

                foreach (var _ExportSettingIterator in _ExportSetting)
                {
                    string _ExcelTableName = _ExportSettingIterator.Key;
                    string _ExcelFileName = _ExportSettingIterator.Value;

                    string _ExcelFilePath = ExcelFilePath + "/" + _ExcelFileName;

                    if (!File.Exists(_ExcelFilePath))
                    {
                        Debug.LogError("[Export Data] " + _ExcelFileName + " doesn't exist.");
                        continue;
                    }

                    FileStream _FileStream = null;

                    try
                    {
                        _FileStream = File.Open(_ExcelFilePath, FileMode.Open, FileAccess.Read);
                    }
                    catch(Exception ex)
                    {
                        Debug.Log(ex.Message);
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    if (_FileStream == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    IExcelDataReader _Reader = ExcelReaderFactory.CreateReader(_FileStream);

                    if (_Reader == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    DataSet _DataSet = _Reader.AsDataSet();

                    if (_DataSet == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    DataTable _TableData = _DataSet.Tables[_ExcelTableName];

                    if (_TableData == null)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + ".");
                        continue;
                    }

                    int _Columns = _TableData.Columns.Count;
                    int _Rows = _TableData.Rows.Count;

                    if (_Rows < 2 || _Columns <= 0)
                    {
                        Debug.LogError("[Export Data] Rows are less than two or columns are less than one.");
                        continue;
                    }

                    ExcelTable _ExcelTableObject = new ExcelTable(_ExcelTableName, _TableData);

                    if (!_ExcelTableObject.IsValid)
                    {
                        Debug.LogError("[Export Data] Cannot read " + _ExcelFileName + "|" + _ExcelTableName + ".");
                        continue;
                    }

                    string _CreateTableCommand = _ExcelTableObject.CreateTableSqlCommand;

                    try
                    {
                        _DbConnection.Execute(_CreateTableCommand);
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        continue;
                    }

                    SQLiteCommand _InsertCommand = null;
                    string _InsertSqlCommand = string.Empty;
                    List<object> _ParamList = null;

                    if (_Rows >= 2)
                    {
                        _DbConnection.BeginTransaction();

                        for (int i = 2; i < _Rows; ++i)
                        {
                            try
                            {
                                _ExcelTableObject.GenerateInsertDataSqlString(i, out _InsertSqlCommand, out _ParamList);

                                if (!string.IsNullOrEmpty(_InsertSqlCommand) && _ParamList != null)
                                {
                                    _InsertCommand = _DbConnection.CreateCommand(_InsertSqlCommand, _ParamList.ToArray());
                                }

                                if (_InsertCommand != null)
                                {
                                    _InsertCommand.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError(ex.Message);
                            }
                        }

                        _DbConnection.Commit();

                        _ExcelTableObject.GenerateDaoAccessCode(CodeGeneratePath, _ExportDbName);

                        _InsertCommand = null;
                        _InsertSqlCommand = string.Empty;
                        _ParamList = null;
                    }
                }

                _DbConnection.Close();
                _DbConnection.Dispose();
                _DbConnection = null;
            }
        }

        protected static Dictionary<string, Dictionary<string, string>> LoadExportConfig()
        {
            string _ConfigContent = string.Empty;

            List<string> _ValidList = new List<string>();

            try
            {
                _ConfigContent = File.ReadAllText(ConfigFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                Debug.LogError("[Export Data] Fail to read config file.");
                return null;
            }

            JsonData _ConfigContentObject = JsonMapper.ToObject(_ConfigContent);

            if (_ConfigContentObject == null || !_ConfigContentObject.IsObject)
            {
                Debug.LogError("[Export Data] Fail to parse config file.");
                return null;
            }

            IDictionary _ConfigContentDict = _ConfigContentObject as IDictionary;

            if (_ConfigContentDict == null)
            {
                Debug.LogError("[Export Data] Fail to parse config file.");
                return null;
            }

            Dictionary<string, Dictionary<string, string>> _Ret = new Dictionary<string, Dictionary<string, string>>();

            foreach (string _ExportDbName in _ConfigContentDict.Keys)
            {
                if (string.IsNullOrEmpty(_ExportDbName))
                {
                    Debug.LogError("[Export Data] Export db name is null.");
                    return null;
                }

                if (!_Ret.ContainsKey(_ExportDbName))
                {
                    _Ret.Add(_ExportDbName, new Dictionary<string, string>());
                }

                JsonData _DbConfigObject = _ConfigContentObject[_ExportDbName];

                if (_DbConfigObject == null || !_DbConfigObject.IsObject)
                {
                    Debug.LogError("[Export Data] Fail to parse config file. [" + _ExportDbName + "]");
                    return null;
                }

                IDictionary _DbConfigDict = _DbConfigObject as IDictionary;

                if (_DbConfigDict == null)
                {
                    Debug.LogError("[Export Data] Fail to parse config file. [" + _ExportDbName + "]");
                    return null;
                }

                foreach (string _ExcelTableName in _DbConfigDict.Keys)
                {
                    if (string.IsNullOrEmpty(_ExcelTableName))
                    {
                        Debug.LogError("[Export Data] " + _ExportDbName + " refers to a null or empty excel table name.");
                        return null;
                    }

                    if (_ValidList.Contains(_ExcelTableName))
                    {
                        Debug.LogError("[Export Data] " + _ExportDbName + " - " + _ExcelTableName + " is already exported. Skipped.");
                        continue;
                    }

                    JsonData _ExcelFileNameObject = _DbConfigObject[_ExcelTableName];

                    if (_ExcelFileNameObject == null || !_ExcelFileNameObject.IsString)
                    {
                        Debug.LogError("[Export Data] Fail to parse config file. [" + _ExportDbName + " - " + _ExcelTableName + "]");
                        return null;
                    }

                    string _ExcelFileName = _ExcelFileNameObject.ToString();

                    if (string.IsNullOrEmpty(_ExcelFileName))
                    {
                        Debug.LogError("[Export Data] " + _ExportDbName + " - " + _ExcelTableName + " refers to a null or empty excel file name.");
                        return null;
                    }

                    _Ret[_ExportDbName].Add(_ExcelTableName, _ExcelFileName);
                }
            }

            return _Ret;
        } 

    }
}
 