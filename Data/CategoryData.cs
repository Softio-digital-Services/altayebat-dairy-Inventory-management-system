using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace InventorySystem.Data
{
    public class CategoryData
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public string CategoryImage { get; set; }
        public DateTime DateCreated { get; set; }

        public static List<CategoryData> GetAllCategories()
        {
            string sql = "SELECT * FROM categories ORDER BY category_name";
            return DatabaseHelper.ExecuteQuery(sql, MapFromReader);
        }

        private static CategoryData MapFromReader(SqliteDataReader reader)
        {
            var cat = new CategoryData
            {
                Id           = reader.GetInt32(reader.GetOrdinal("id")),
                CategoryName = reader.GetString(reader.GetOrdinal("category_name")),
                Description  = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description")),
                CategoryImage = reader.IsDBNull(reader.GetOrdinal("category_image")) ? "" : reader.GetString(reader.GetOrdinal("category_image"))
            };

            try
            {
                int ord = reader.GetOrdinal("date_created");
                cat.DateCreated = reader.IsDBNull(ord) ? DateTime.Now
                    : DateTime.Parse(reader.GetString(ord));
            }
            catch { cat.DateCreated = DateTime.Now; }

            return cat;
        }

        public static void AddCategory(string name, string description, string image = "")
        {
            string sql = "INSERT INTO categories (category_name, description, category_image, date_created) " +
                         "VALUES (@name, @desc, @img, datetime('now'))";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name", name),
                new SqliteParameter("@desc", description),
                new SqliteParameter("@img",  image));
        }

        public static void UpdateCategory(int id, string name, string description, string image)
        {
            string sql = "UPDATE categories SET category_name=@name, description=@desc, category_image=@img WHERE id=@id";
            DatabaseHelper.ExecuteNonQuery(sql,
                new SqliteParameter("@name", name),
                new SqliteParameter("@desc", description),
                new SqliteParameter("@img",  image),
                new SqliteParameter("@id",   id));
        }

        public static void DeleteCategory(int id)
        {
            string sql = "DELETE FROM categories WHERE id=@id";
            DatabaseHelper.ExecuteNonQuery(sql, new SqliteParameter("@id", id));
        }

        /// <summary>
        /// Returns the total count of all items (regardless of status) in a given category.
        /// </summary>
        public static int GetItemCount(string categoryName)
        {
            string sql = @"SELECT COUNT(*) FROM parts p
                           LEFT JOIN categories c ON p.category_id = c.id
                           WHERE p.date_deleted IS NULL AND c.category_name = @cat";
            return DatabaseHelper.ExecuteScalar<int>(sql, new SqliteParameter("@cat", categoryName));
        }

        /// <summary>
        /// Returns the total count of all items across all categories.
        /// </summary>
        public static int GetTotalItemCount()
        {
            string sql = "SELECT COUNT(*) FROM parts WHERE date_deleted IS NULL";
            return DatabaseHelper.ExecuteScalar<int>(sql);
        }
    }
}
