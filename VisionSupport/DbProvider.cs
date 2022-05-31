using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace VisionSupport
{
    public class DbProvider
    {
        public static string DbFolderPath { get; set; } = "Database";

        // Add a new record to database
        public static void AddRecord<T>(string path, string tablename, T record)
        {
            //Check DatabaseFolder Existing
            string checkfolder = Path.GetDirectoryName(path);
            if(!Directory.Exists(checkfolder)) Directory.CreateDirectory(checkfolder);

            bool _hastablename = true;
            if(string.IsNullOrEmpty(tablename)) _hastablename= false;
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path is null");
            if (record == null) throw new ArgumentNullException(nameof(record) + " is null");
            using(LiteDatabase db = new LiteDatabase(DbFolderPath))
            {
                ILiteCollection<T> collection;
                if(_hastablename) collection = db.GetCollection<T>(tablename);
                else collection = db.GetCollection<T>();

                collection.Insert(record);
            }
        }
        // Add a unique new record to database
        public static void AddRecord<T>(string path, string tablename, T record,string key)
        {
            //Check DatabaseFolder Existing
            string checkfolder = Path.GetDirectoryName(path);
            if (!Directory.Exists(checkfolder)) Directory.CreateDirectory(checkfolder);

            // Check key
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key is null");
            //flag find property existion
            bool found = false;

            TypeInfo typeinfo = typeof(T).GetTypeInfo();
            PropertyInfo[] properties = typeof(T).GetType().GetProperties();
            PropertyInfo keyproperty = null;
            foreach(PropertyInfo property in properties)
            {
                if(property.Name == key)
                {
                    found = true;
                    keyproperty = property;
                    break;
                }
            }

            if (!found) throw new ArgumentOutOfRangeException(nameof(key) + " is not exist");

            bool _hastablename = true;
            if (string.IsNullOrEmpty(tablename)) _hastablename = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path is null");
            if (record == null) throw new ArgumentNullException(nameof(record) + " is null");
            using (LiteDatabase db = new LiteDatabase(DbFolderPath))
            {
                ILiteCollection<T> collection;
                if (_hastablename) collection = db.GetCollection<T>(tablename);
                else collection = db.GetCollection<T>();
                found = false;
                List<T> records = collection.FindAll().ToList();
                foreach (T subrecord in records)
                {
                    object subrecordvalue = keyproperty.GetValue(subrecord);
                    object recordvalue = keyproperty.GetValue(record);

                    if (subrecordvalue == null ||recordvalue == null)
                    {
                        throw new ArgumentException("value is null");
                    }
                    if (keyproperty.GetValue(recordvalue).Equals(subrecordvalue))
                    {
                        found = true;
                        break;
                    }
                }
                if (found) throw new ArgumentException(nameof(record) + " is exist");
                collection.Insert(record);
            }
        }
        public static void RemoveAll<T>(string path, string tablename)
        {
            //Check DatabaseFolder Existing
            string checkfolder = Path.GetDirectoryName(path);
            if (!Directory.Exists(checkfolder)) Directory.CreateDirectory(checkfolder);

            bool _hastablename = true;
            if (string.IsNullOrEmpty(tablename)) _hastablename = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path is null");
            using (LiteDatabase db = new LiteDatabase(DbFolderPath))
            {
                ILiteCollection<T> collection;
                if (_hastablename) collection = db.GetCollection<T>(tablename);
                else collection = db.GetCollection<T>();
                collection.DeleteAll();
            }
        }
        public static void Remove<T>(string path, string tablename, int id)
        {
            //Check DatabaseFolder Existing
            string checkfolder = Path.GetDirectoryName(path);
            if (!Directory.Exists(checkfolder)) Directory.CreateDirectory(checkfolder);

            bool _hastablename = true;
            if (string.IsNullOrEmpty(tablename)) _hastablename = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path is null");
            using (LiteDatabase db = new LiteDatabase(DbFolderPath))
            {
                ILiteCollection<T> collection;
                if (_hastablename) collection = db.GetCollection<T>(tablename);
                else collection = db.GetCollection<T>();
                if (collection.Delete(id))
                {
                    throw new ArgumentOutOfRangeException($"{id} is not exist");
                }
            }
        }
        public static void Remove<T>(string path, string tablename, T record, string propertyname)
        {
            //Check DatabaseFolder Existing
            string checkfolder = Path.GetDirectoryName(path);
            if (!Directory.Exists(checkfolder)) Directory.CreateDirectory(checkfolder);

            // Check property
            if (string.IsNullOrEmpty(propertyname)) throw new ArgumentNullException("property is null");
            //flag find property existion
            bool found = false;

            TypeInfo typeinfo = typeof(T).GetTypeInfo();
            PropertyInfo[] properties = typeof(T).GetType().GetProperties();
            PropertyInfo keyproperty = null;
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == propertyname)
                {
                    found = true;
                    keyproperty = property;
                    break;
                }
            }

            if (!found) throw new ArgumentOutOfRangeException(nameof(propertyname) + " is not exist");

            bool _hastablename = true;
            if (string.IsNullOrEmpty(tablename)) _hastablename = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path is null");
            if (record == null) throw new ArgumentNullException(nameof(record) + " is null");
            using (LiteDatabase db = new LiteDatabase(DbFolderPath))
            {
                ILiteCollection<T> collection;
                if (_hastablename) collection = db.GetCollection<T>(tablename);
                else collection = db.GetCollection<T>();
                // find remove object
                T remover = collection.FindOne(x => keyproperty.GetValue(x).Equals(keyproperty.GetValue(record)));
                if (remover == null) throw new ArgumentOutOfRangeException(nameof(record) + " is not exist");
                PropertyInfo removerinfo = typeof(T).GetProperty("Id");
                int removerid = (int)removerinfo.GetValue(remover);
                if (collection.Delete(removerid)) throw new ArgumentException(nameof(record) + " is not exist");
            }
        }
        public static void Edit<T>(string path, T changedrecord, string keypropertyname, string tablename = "")
        {
            //Check DatabaseFolder Existing
            string checkfolder = Path.GetDirectoryName(path);
            if (!Directory.Exists(checkfolder)) Directory.CreateDirectory(checkfolder);

            // Check property
            if (string.IsNullOrEmpty(keypropertyname)) throw new ArgumentNullException("property is null");
            //flag find property existion
            bool found = false;

            TypeInfo typeinfo = typeof(T).GetTypeInfo();
            PropertyInfo[] properties = typeof(T).GetType().GetProperties();
            PropertyInfo keyproperty = null;
            foreach (PropertyInfo property in properties)
            {
                if (property.Name == keypropertyname)
                {
                    found = true;
                    keyproperty = property;
                    break;
                }
            }

            if (!found) throw new ArgumentOutOfRangeException(nameof(keypropertyname) + " is not exist");

            bool _hastablename = true;
            if (string.IsNullOrEmpty(tablename)) _hastablename = false;
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path is null");
            if (changedrecord == null) throw new ArgumentNullException(nameof(changedrecord) + " is null");
            using (LiteDatabase db = new LiteDatabase(DbFolderPath))
            {
                ILiteCollection<T> collection;
                if (_hastablename) collection = db.GetCollection<T>(tablename);
                else collection = db.GetCollection<T>();
                // find remove object
                T editer = collection.FindOne(x => keyproperty.GetValue(x).Equals(keyproperty.GetValue(changedrecord)));
                if (editer == null) throw new ArgumentOutOfRangeException(nameof(changedrecord) + " is not exist");
                PropertyInfo editerinfo = typeof(T).GetProperty("Id");
                int editerid = (int)editerinfo.GetValue(editer);
                editerinfo.SetValue(changedrecord, editerid);
                if (collection.Update(editerid,changedrecord)) throw new ArgumentException(nameof(changedrecord) + " is not exist");
            }
        }
    }
}