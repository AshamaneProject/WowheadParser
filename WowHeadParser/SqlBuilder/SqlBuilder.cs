/*
 * * Created by Traesh for AshamaneProject (https://github.com/AshamaneProject)
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Sql
{
    public enum SqlQueryType : byte
    {
        None,
        Update,
        InsertOrUpdate,
        Replace,
        Insert,
        InsertIgnore,
        DeleteInsert,
        Max,
    }

    public class SqlBuilder
    {
        /// <summary>
        /// Gets a sql query type
        /// </summary>
        public SqlQueryType QueryType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to allow null values
        /// </summary>
        public bool AllowNullValue { get; private set; }

        /// <summary>
        /// Gets a value indication whether to allow append header to insert and replace query
        /// </summary>
        public bool WriteWithoutHeader { get; private set; }

        private string _tableName = string.Empty;

        private string _keyName = string.Empty;

        private List<string> _fields = new List<string>(64);

        public List<SqlItem> _items = new List<SqlItem>(64);

        private StringBuilder _content = new StringBuilder(8196);

        /// <summary>
        /// Initial Sql builder
        /// </summary>
        /// <param name="tableName">Table name (like creature_template, creature etc.)</param>
        /// <param name="keyName">Key name (like entry, id, guid etc.)</param>
        public SqlBuilder(string tableName, string keyName, SqlQueryType type = SqlQueryType.InsertOrUpdate)
        {
            _tableName = tableName;
            _keyName = keyName;

            WriteWithoutHeader = false;// Settings.Default.WithoutHeader;
            AllowNullValue = true;// Settings.Default.AllowEmptyValues;
            QueryType = type;// (SqlQueryType)Settings.Default.QueryType;

            if (QueryType <= SqlQueryType.None || QueryType >= SqlQueryType.Max)
                throw new InvalidQueryTypeException(QueryType);
        }

        /// <summary>
        /// Initial Sql builder
        /// <param name="tableName">Table name (like creature_template, creature etc.)</param>
        /// </summary>
        public SqlBuilder(string tableName)
                : this(tableName, "entry")
        {
        }

        /// <summary>
        /// Append fields names
        /// </summary>
        /// <param name="args">fields name array</param>
        public void SetFieldsNames(params string[] args)
        {
            if (args == null)
                throw new ArgumentNullException();

            for (int i = 0; i < args.Length; ++i)
            {
                _fields.Add(args[i]);
            }
        }

        /// <summary>
        /// Append fields names
        /// </summary>
        /// <param name="args">fields name array</param>
        public void SetFieldsName(string format, params object[] args)
        {
            if (args == null || string.IsNullOrEmpty(format))
                throw new ArgumentNullException();

            string field = string.Format(format, args);
            _fields.Add(field);
        }

        /// <summary>
        /// Append key and fields value 
        /// </summary>
        /// <param name="key">key value</param>
        /// <param name="args">string fields values array</param>
        public void AppendFieldsValue(object key, params string[] args)
        {
            if (key == null || args == null)
                throw new ArgumentNullException();

            List<string> values = new List<string>(args.Length);
            for (int i = 0; i < args.Length; ++i)
            {
                values.Add(args[i]);
            }

            _items.Add(new SqlItem(key, values));
        }

        /// <summary>
        /// Append key and fields value 
        /// </summary>
        /// <param name="key">key value</param>
        /// <param name="args">object fields values array</param>
        public void AppendFieldsValue(object key, params object[] args)
        {
            if (key == null || args == null)
                throw new ArgumentNullException();

            List<string> values = new List<string>(args.Length);
            for (int i = 0; i < args.Length; ++i)
            {
                values.Add(args[i].ToString());
            }

            _items.Add(new SqlItem(key, values));
        }

        /// <summary>
        /// Append sql query
        /// </summary>
        /// <param name="query"></param>
        public void AppendSqlQuery(string query, params object[] args)
        {
            if (args == null || string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException();

            _content.AppendLine(string.Format(query, args));
        }

        public bool Empty
        {
            get { return _items.Count <= 0; }
        }

        /// <summary>
        /// Build sql query
        /// </summary>
        public override string ToString()
        {
            if (Empty)
                return string.Empty;

            _content.Clear();
            _content.Capacity = 2048 * _items.Count;

            switch (QueryType)
            {
                case SqlQueryType.Update:
                    return BuildUpdateQuery();
                case SqlQueryType.InsertOrUpdate:
                    return BuildInsertOrUpdateQuery();
                case SqlQueryType.Replace:
                case SqlQueryType.Insert:
                case SqlQueryType.InsertIgnore:
                case SqlQueryType.DeleteInsert:
                    return BuildReplaceInsertQuery();
                default:
                    return string.Empty;
            }
        }

        private string BuildUpdateQuery()
        {
            for (int i = 0; i < _items.Count; ++i)
            {
                bool notEmpty = false;

                SqlItem item = _items[i];

                StringBuilder contentInternal = new StringBuilder(1024);
                {
                    contentInternal.AppendFormat("UPDATE `{0}` SET ", _tableName);
                    for (int j = 0; j < item.Count; ++j)
                    {
                        if (!AllowNullValue && string.IsNullOrWhiteSpace(item[j]))
                            continue;

                        contentInternal.AppendFormat(NumberFormatInfo.InvariantInfo, "`{0}` = '{1}', ", _fields[j], item[j]);
                        notEmpty = true;
                    }
                    contentInternal.Remove(contentInternal.Length - 2, 2);
                    contentInternal.AppendFormat(" WHERE `{0}` = {1};", _keyName, item.Key).AppendLine();

                    if (notEmpty)
                        _content.Append(contentInternal.ToString());
                }
            }

            return _content.ToString();
        }

        private string BuildInsertOrUpdateQuery()
        {
            for (int i = 0; i < _items.Count; ++i)
            {
                bool notEmpty = false;

                SqlItem item = _items[i];

                StringBuilder tableNames    = new StringBuilder(1024);
                StringBuilder tableValues   = new StringBuilder(1024);
                StringBuilder tableUpdates  = new StringBuilder(1024);

                tableNames.AppendFormat("INSERT INTO `{0}` ({1}, ", _tableName, _keyName);
                tableValues.AppendFormat(") VALUES ({0}, ", item.Key);
                tableUpdates.AppendFormat(") ON DUPLICATE KEY UPDATE ");  
                for (int j = 0; j < item.Count; ++j)
                {
                    if (!AllowNullValue && string.IsNullOrWhiteSpace(item[j]))
                        continue;

                    tableNames.AppendFormat(NumberFormatInfo.InvariantInfo, "{0}, ", _fields[j]);
                    tableValues.AppendFormat(NumberFormatInfo.InvariantInfo, "'{0}', ", item[j].Replace("'", "''"));
                    tableUpdates.AppendFormat(NumberFormatInfo.InvariantInfo, "{0} = VALUES({1}), ", _fields[j], _fields[j]);
                    notEmpty = true;
                }

                if (notEmpty)
                {
                    tableNames.Remove(tableNames.Length - 2, 2);
                    tableValues.Remove(tableValues.Length - 2, 2);
                    tableUpdates.Remove(tableUpdates.Length - 2, 2);

                    tableNames.Append(tableValues);
                    tableNames.Append(tableUpdates);
                    tableNames.Append(";").AppendLine();

                    _content.Append(tableNames.ToString());
                }
            }

            return _content.ToString();
        }

        private string BuildReplaceInsertQuery()
        {
            if (QueryType == SqlQueryType.DeleteInsert)
            {
                List<object> alreadyDoneEntry = new List<object>();

                foreach (SqlItem item in _items)
                {
                    if (alreadyDoneEntry.Contains(item.Key))
                        continue;

                    _content.AppendFormat("DELETE FROM `{0}` WHERE `{1}` = '{2}';", _tableName, _keyName, item.Key).AppendLine();
                    alreadyDoneEntry.Add(item.Key);
                }
            }

            switch (QueryType)
            {
                case SqlQueryType.Insert:
                case SqlQueryType.DeleteInsert:
                    _content.AppendFormat("INSERT INTO `{0}`", _tableName);
                    break;
                case SqlQueryType.InsertIgnore:
                    _content.AppendFormat("INSERT IGNORE INTO `{0}`", _tableName);
                    break;
                case SqlQueryType.Replace:
                    _content.AppendFormat("REPLACE INTO `{0}`", _tableName);
                    break;
            }

            if (!WriteWithoutHeader)
            {
                _content.AppendFormat(" (`{0}`, ", _keyName);

                for (int i = 0; i < _fields.Count; ++i)
                    _content.AppendFormat("`{0}`, ", _fields[i]);

                _content.Remove(_content.Length - 2, 2);
                _content.Append(")");
            }
            _content.AppendLine(" VALUES");

            for (int i = 0; i < _items.Count; ++i)
            {
                SqlItem item = _items[i];

                _content.AppendFormat("('{0}', ", item.Key);
                for (int j = 0; j < item.Count; ++j)
                {
                    _content.AppendFormat(NumberFormatInfo.InvariantInfo, "'{0}', ", item[j]);
                }
                _content.Remove(_content.Length - 2, 2);
                _content.AppendFormat("){0}", i < _items.Count - 1 ? "," : ";").AppendLine();
            }

            return _content + Environment.NewLine;
        }
    }
}