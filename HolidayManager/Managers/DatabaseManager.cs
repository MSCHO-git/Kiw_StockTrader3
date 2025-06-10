using System;
using System.Data.SQLite;
using System.IO;
using Dapper;
using System.Collections.Generic;

namespace HolidayManager.Managers
{
    public static class DatabaseManager
    {
        private static string _connectionString;

        static DatabaseManager()
        {
            InitializeDatabase();
        }

        private static void InitializeDatabase()
        {
            try
            {
                // Data 폴더 경로 설정
                string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

                // Data 폴더가 없으면 생성
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                // 데이터베이스 파일 경로
                string dbPath = Path.Combine(dataFolder, "holidays.db");
                _connectionString = $"Data Source={dbPath};Version=3;";

                // 데이터베이스 파일이 없으면 생성 및 테이블 생성
                if (!File.Exists(dbPath))
                {
                    CreateDatabase();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"데이터베이스 초기화 실패: {ex.Message}");
            }
        }

        private static void CreateDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                // Holidays 테이블 생성
                string createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Holidays (
                        HolidayDate TEXT PRIMARY KEY,
                        HolidayName TEXT NOT NULL,
                        HolidayType TEXT,
                        Notes TEXT,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE INDEX IF NOT EXISTS idx_holidays_date ON Holidays(HolidayDate);
                ";

                connection.Execute(createTableSql);
                Console.WriteLine("데이터베이스 및 테이블 생성 완료");
            }
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public static int Execute(string sql, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.Execute(sql, parameters);
            }
        }

        public static T ExecuteScalar<T>(string sql, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.ExecuteScalar<T>(sql, parameters);
            }
        }

        public static IEnumerable<T> Query<T>(string sql, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.Query<T>(sql, parameters);
            }
        }

        public static T QueryFirstOrDefault<T>(string sql, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.QueryFirstOrDefault<T>(sql, parameters);
            }
        }
    }
}