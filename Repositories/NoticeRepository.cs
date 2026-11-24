using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface INoticeRepository
    {
        Task<List<Notice>> GetPublicNoticesAsync(int? categoryId = null, int limit = 20);
        Task<List<Notice>> GetAllNoticesAsync();
        Task<Notice?> GetNoticeByIdAsync(int noticeId);
        Task<int> CreateNoticeAsync(Notice notice);
        Task<bool> UpdateNoticeAsync(Notice notice);
        Task<bool> DeleteNoticeAsync(int noticeId);
        Task IncrementViewCountAsync(int noticeId);
        Task<List<NoticeCategory>> GetCategoriesAsync();
        Task<int> CreateCategoryAsync(NoticeCategory category);
    }

    public class NoticeRepository : INoticeRepository
    {
        private readonly DatabaseHelper _db;

        public NoticeRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<List<Notice>> GetPublicNoticesAsync(int? categoryId = null, int limit = 20)
        {
            var sql = $@"
                SELECT TOP ({limit}) n.NoticeId, n.CategoryId, nc.CategoryName, nc.BadgeClass,
                       n.Title, n.Slug, n.Description, n.PublishDate, n.ExpiryDate,
                       n.AttachmentUrl, n.AttachmentFileName, n.IsPinned, n.IsActive,
                       n.ViewCount, n.CreatedDate
                FROM dbo.Notices n
                JOIN dbo.NoticeCategories nc ON n.CategoryId = nc.CategoryId
                WHERE n.IsActive = 1
                  AND (n.ExpiryDate IS NULL OR n.ExpiryDate >= CAST(GETDATE() AS DATE))";

            var parameters = new List<SqlParameter>();
            if (categoryId.HasValue)
            {
                sql += " AND n.CategoryId = @CategoryId";
                parameters.Add(new SqlParameter("@CategoryId", categoryId.Value));
            }

            sql += " ORDER BY n.IsPinned DESC, n.PublishDate DESC";

            return await _db.ExecuteReaderListAsync(sql, MapNotice, CommandType.Text, parameters.ToArray());
        }

        public async Task<List<Notice>> GetAllNoticesAsync()
        {
            const string sql = @"
                SELECT n.NoticeId, n.CategoryId, nc.CategoryName, nc.BadgeClass,
                       n.Title, n.Slug, n.Description, n.PublishDate, n.ExpiryDate,
                       n.AttachmentUrl, n.AttachmentFileName, n.IsPinned, n.IsActive,
                       n.ViewCount, n.CreatedDate
                FROM dbo.Notices n
                JOIN dbo.NoticeCategories nc ON n.CategoryId = nc.CategoryId
                ORDER BY n.PublishDate DESC";

            return await _db.ExecuteReaderListAsync(sql, MapNotice);
        }

        public async Task<Notice?> GetNoticeByIdAsync(int noticeId)
        {
            const string sql = @"
                SELECT n.NoticeId, n.CategoryId, nc.CategoryName, nc.BadgeClass,
                       n.Title, n.Slug, n.Description, n.PublishDate, n.ExpiryDate,
                       n.AttachmentUrl, n.AttachmentFileName, n.IsPinned, n.IsActive,
                       n.ViewCount, n.CreatedDate
                FROM dbo.Notices n
                JOIN dbo.NoticeCategories nc ON n.CategoryId = nc.CategoryId
                WHERE n.NoticeId = @Id";

            return await _db.ExecuteReaderSingleAsync(sql, MapNotice, CommandType.Text, new[] { new SqlParameter("@Id", noticeId) });
        }

        public async Task<int> CreateNoticeAsync(Notice notice)
        {
            const string sql = @"
                INSERT INTO dbo.Notices (CategoryId, Title, Slug, Description, PublishDate, ExpiryDate, AttachmentUrl, AttachmentFileName, IsPinned, IsActive, ViewCount, CreatedDate)
                VALUES (@CatId, @Title, @Slug, @Desc, @PubDate, @ExpDate, @AttUrl, @AttName, @IsPinned, @IsActive, 0, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var slug = notice.Title.ToLowerInvariant().Replace(" ", "-").Replace("/", "-");

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@CatId", notice.CategoryId),
                new SqlParameter("@Title", notice.Title),
                new SqlParameter("@Slug", slug),
                new SqlParameter("@Desc", notice.Description),
                new SqlParameter("@PubDate", notice.PublishDate),
                new SqlParameter("@ExpDate", (object?)notice.ExpiryDate ?? DBNull.Value),
                new SqlParameter("@AttUrl", (object?)notice.AttachmentUrl ?? DBNull.Value),
                new SqlParameter("@AttName", (object?)notice.AttachmentFileName ?? DBNull.Value),
                new SqlParameter("@IsPinned", notice.IsPinned),
                new SqlParameter("@IsActive", notice.IsActive)
            });

            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateNoticeAsync(Notice notice)
        {
            const string sql = @"
                UPDATE dbo.Notices
                SET CategoryId = @CatId, Title = @Title, Description = @Desc,
                    PublishDate = @PubDate, ExpiryDate = @ExpDate,
                    AttachmentUrl = ISNULL(@AttUrl, AttachmentUrl),
                    AttachmentFileName = ISNULL(@AttName, AttachmentFileName),
                    IsPinned = @IsPinned, IsActive = @IsActive, UpdatedDate = GETDATE()
                WHERE NoticeId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@CatId", notice.CategoryId),
                new SqlParameter("@Title", notice.Title),
                new SqlParameter("@Desc", notice.Description),
                new SqlParameter("@PubDate", notice.PublishDate),
                new SqlParameter("@ExpDate", (object?)notice.ExpiryDate ?? DBNull.Value),
                new SqlParameter("@AttUrl", (object?)notice.AttachmentUrl ?? DBNull.Value),
                new SqlParameter("@AttName", (object?)notice.AttachmentFileName ?? DBNull.Value),
                new SqlParameter("@IsPinned", notice.IsPinned),
                new SqlParameter("@IsActive", notice.IsActive),
                new SqlParameter("@Id", notice.NoticeId)
            });

            return rows > 0;
        }

        public async Task<bool> DeleteNoticeAsync(int noticeId)
        {
            const string sql = "UPDATE dbo.Notices SET IsActive = 0, UpdatedDate = GETDATE() WHERE NoticeId = @Id";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", noticeId) });
            return rows > 0;
        }

        public async Task IncrementViewCountAsync(int noticeId)
        {
            const string sql = "UPDATE dbo.Notices SET ViewCount = ViewCount + 1 WHERE NoticeId = @Id";
            await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", noticeId) });
        }

        public async Task<List<NoticeCategory>> GetCategoriesAsync()
        {
            const string sql = @"
                SELECT nc.CategoryId, nc.CategoryName, nc.BadgeClass, nc.IsActive,
                       (SELECT COUNT(*) FROM dbo.Notices n WHERE n.CategoryId = nc.CategoryId AND n.IsActive = 1) AS NoticeCount
                FROM dbo.NoticeCategories nc
                WHERE nc.IsActive = 1
                ORDER BY nc.CategoryName";

            return await _db.ExecuteReaderListAsync(sql, r => new NoticeCategory
            {
                CategoryId = r.GetInt32(r.GetOrdinal("CategoryId")),
                CategoryName = r.GetString(r.GetOrdinal("CategoryName")),
                BadgeClass = r.GetString(r.GetOrdinal("BadgeClass")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                NoticeCount = r.GetInt32(r.GetOrdinal("NoticeCount"))
            });
        }

        public async Task<int> CreateCategoryAsync(NoticeCategory category)
        {
            const string sql = @"
                INSERT INTO dbo.NoticeCategories (CategoryName, BadgeClass, IsActive, CreatedDate)
                VALUES (@Name, @Badge, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var res = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Name", category.CategoryName),
                new SqlParameter("@Badge", category.BadgeClass),
                new SqlParameter("@IsActive", category.IsActive)
            });

            return Convert.ToInt32(res);
        }

        private static Notice MapNotice(SqlDataReader r)
        {
            return new Notice
            {
                NoticeId = r.GetInt32(r.GetOrdinal("NoticeId")),
                CategoryId = r.GetInt32(r.GetOrdinal("CategoryId")),
                CategoryName = r.IsDBNull(r.GetOrdinal("CategoryName")) ? null : r.GetString(r.GetOrdinal("CategoryName")),
                BadgeClass = r.IsDBNull(r.GetOrdinal("BadgeClass")) ? null : r.GetString(r.GetOrdinal("BadgeClass")),
                Title = r.GetString(r.GetOrdinal("Title")),
                Slug = r.IsDBNull(r.GetOrdinal("Slug")) ? null : r.GetString(r.GetOrdinal("Slug")),
                Description = r.GetString(r.GetOrdinal("Description")),
                PublishDate = r.GetDateTime(r.GetOrdinal("PublishDate")),
                ExpiryDate = r.IsDBNull(r.GetOrdinal("ExpiryDate")) ? null : r.GetDateTime(r.GetOrdinal("ExpiryDate")),
                AttachmentUrl = r.IsDBNull(r.GetOrdinal("AttachmentUrl")) ? null : r.GetString(r.GetOrdinal("AttachmentUrl")),
                AttachmentFileName = r.IsDBNull(r.GetOrdinal("AttachmentFileName")) ? null : r.GetString(r.GetOrdinal("AttachmentFileName")),
                IsPinned = r.GetBoolean(r.GetOrdinal("IsPinned")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                ViewCount = r.GetInt32(r.GetOrdinal("ViewCount")),
                CreatedDate = r.GetDateTime(r.GetOrdinal("CreatedDate"))
            };
        }
    }
}
