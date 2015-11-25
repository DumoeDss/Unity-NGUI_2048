using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;

public class DbAccess  {
    
    private SqliteConnection DbConnection;
    private SqliteCommand DbCommand;
    private SqliteDataReader DbReader;

    public DbAccess()
    {

    }

    public DbAccess(string conn)
    {
        ConnectDb(conn);
    }

    //建立连接
    public void ConnectDb(string DbName)
    {
        try
        {
            DbConnection = new SqliteConnection(DbName);
            DbConnection.Open();
            //Debug.Log("Connect Success!");
        } catch(System.Exception e)
        {
            Debug.Log(e.ToString());
        }
        
    }

    //断开连接
    public void DisConnectDb()
    {
        if (DbCommand != null)
        {
            DbCommand.Dispose();
        }
        DbCommand = null;

        if (DbReader != null)
        {
            DbReader.Dispose();
        }
        DbReader = null;

        if (DbConnection != null)
        {
            DbConnection.Close();
        }
        DbConnection = null;
        //Debug.Log("Disconnected FROM db.");
    }

    //判断表是否存在
    public bool IsTableExist(string table_name)
    {
        if (table_name == null)
        {
            return false;
        }

        DbCommand = DbConnection.CreateCommand();
        DbCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE TYPE='table' AND NAME='"+ table_name+"'";
        //DbReader = DbCommand.ExecuteReader();
        if (0 == System.Convert.ToInt32(DbCommand.ExecuteScalar()))
            return false;
        else return true;

    }

    public void DataCount(string table_name,out int count)
    {
        count=0;
        DbCommand = DbConnection.CreateCommand();
        DbCommand.CommandText = "SELECT COUNT(*) FROM " + table_name;
        count = System.Convert.ToInt32(DbCommand.ExecuteScalar());
    }

    //ExecuteQuery(sql)
    private SqliteDataReader ExecuteQuery(string sql)
    {
        DbCommand = DbConnection.CreateCommand();
        DbCommand.CommandText = sql;
        try { 
        DbReader = DbCommand.ExecuteReader();
            //Debug.Log("ExecuteQuery"+sql+" Success!");
            return DbReader;         
        } catch(SqliteException e)
        {
            Debug.Log(e.ToString());
            throw new SqliteException("ExecuteQuery"+sql+" Failed!");
        }
        
    }

    

    //CREATE TABLE table_name(列名称1 数据类型,列名称2 数据类型,列名称3 数据类型,....)
    public SqliteDataReader CreateTable(string table_name,string[] fileds,string[] filedstype)
    {
        if (fileds.Length != filedstype.Length)
        {
            throw new SqliteException("fileds.Length != values.Length");
        }
        //int PRIMARY KEY IDENTITY
        string sql = "CREATE TABLE " + table_name + " (" + fileds[0] + " " + filedstype[0];
        for (int i = 1; i < fileds.Length; ++i)
        {
            sql += ", " + fileds[i] + " " + filedstype[i];
        }
        sql += ");";


        return ExecuteQuery(sql);
    }

    //查
    //SELECT * FROM table_name
    public SqliteDataReader SelectTable(string table_name)
    {
        string sql = "SELECT * FROM " + table_name;
        return ExecuteQuery(sql);
    }
    //SELECT item FROM table_name
    public SqliteDataReader SelectTable(string table_name, string item)
    {
        string sql = "SELECT " + item + " FROM " + table_name;
        return ExecuteQuery(sql);
    }
    public SqliteDataReader SelectTable(string table_name, string keyfiled, string keyvalue)
    {
        string sql = "SELECT * FROM " + table_name + " WHERE " + keyfiled + " = '" + keyvalue + "'";
        return ExecuteQuery(sql);
    }
    public SqliteDataReader SelectTable(string table_name, string keyfiled, int keyvalue)
    {
        string sql = "SELECT * FROM " + table_name + " WHERE " + keyfiled + " = " + keyvalue;
        return ExecuteQuery(sql);
    }
    //SELECT item FROM table_name WHERE (列名称=列数据)
    public SqliteDataReader SelectTable(string table_name, string item, string keyfiled, string keyvalue)
    {
        string sql = "SELECT " + item + " FROM " + table_name + " WHERE " + keyfiled + " = '" + keyvalue+"'";
        return ExecuteQuery(sql);
    }

    //增
    //INSERT INTO table_name VALUES (值1, 值2,....)
    public SqliteDataReader InsertTable(string table_name,string[] values)
    {
        string sql = "INSERT INTO TABLE VALUES('" + values[0]+"'";
        for(int i = 1; i < values.Length; ++i)
        {
            sql += ",'" + values[i]+"'";
        }
        sql += ")";
        return ExecuteQuery(sql);
    }
    //INSERT INTO table_name (列1, 列2,...) VALUES (值1, 值2,....)
    public SqliteDataReader InsertTable(string table_name, string[] fileds, string[] values)
    {
        if (fileds.Length != values.Length)
        {
            throw new SqliteException("fileds.Length != values.Length");
        }
        string sql = "INSERT INTO "+ table_name +" ( "+ fileds[0];
        for (int i = 1; i < fileds.Length; ++i)
        {
            sql += ", " + fileds[i];
        }
        sql += ") VALUES ('" + values[0]+"'";
        for (int i = 1; i < values.Length; ++i)
        {
            sql += ", '" + values[i] + "'";
        }
        sql += ")";
        return ExecuteQuery(sql);
    }

    //删
    //DELETE table_name WHERE
    public SqliteDataReader DeleteTable(string table_name,string[] fileds,string[] values)
    {
        if (fileds.Length != values.Length)
        {
            throw new SqliteException("fileds.Length != values.Length");
        }
        string sql = "DELETE FROM " + table_name + " WHERE " + fileds[0] + " = '" + values[0]+"'";
        for(int i = 1; i < fileds.Length; ++i)
        {
            sql += " OR " + fileds[i] + "='" + values[i]+"'";
        }
        return ExecuteQuery(sql);
    }
    //DELETE table
    public SqliteDataReader DeleteContents(string table_name)

    {

        string sql = "DELETE FROM " + table_name;

        return ExecuteQuery(sql);

    }

    //改
    //UPDATE table_name SET filed1 = value1,filed2 = value2
    public SqliteDataReader UpdateTable(string table_name, string[] fileds, string[] values)
    {
        if (fileds.Length != values.Length)
        {
            throw new SqliteException("fileds.Length != values.Length");
        }
        string sql = "UPDATE " + table_name + " SET " + fileds[0] + " = '" + values[0] + "'";
        for (int i = 1; i < fileds.Length; ++i)
        {
            sql += ", " + fileds[i] + " = '" + values[i]+"'";
        }
        return ExecuteQuery(sql);
    }
    public SqliteDataReader UpdateTable(string table_name, string filed, string value)
    {
        string sql = "UPDATE " + table_name + " SET " + filed + " = '" + value+"'";
        return ExecuteQuery(sql);
    }
    public SqliteDataReader UpdateTable(string table_name, string filed, string value, string keyfiled, string keyvalue)
    {
        string sql = "UPDATE " + table_name + " SET " + filed + " = '" + value + "' WHERE " + keyfiled + " = '" + keyvalue+"'";
        return ExecuteQuery(sql);
    }
    //UPDATE table_name SET filed1 = value1,filed2 = value2 WHERE keyfiled = keyvalue;
    public SqliteDataReader UpdateTable(string table_name,string[] fileds,string[] values,string keyfiled,string keyvalue)
    {
        if (fileds.Length != values.Length)
        {
            throw new SqliteException("fileds.Length != values.Length");
        }
        string sql = "UPDATE " + table_name + " SET " + fileds[0] + " = '" + values[0] + "'";
        for(int i = 1; i < fileds.Length; ++i)
        {
            sql += " , " + fileds[i] + " = '" + values[i]+"'";
        }
        sql += " WHERE " + keyfiled + " = '" + keyvalue+"'";
        return ExecuteQuery(sql);
    }











}
