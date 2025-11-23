using BLL.Service.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Shared.Models;
using Shared.Models.Pagination;
using System.Data;

namespace BLL.Service.Implementation
{
    public class OptionRepository : IOptionRepository
    {
        private readonly string _connectionString;

        public OptionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public async Task<List<OptionSetDto>> GetOptionSetsAsync()
        {
            var list = new List<OptionSetDto>();
            await using var conn = GetConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("SELECT OptionId, Name FROM Options ORDER BY OptionId", conn);

            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new OptionSetDto
                {
                    OptionId = rdr.GetInt32(0),
                    Name = rdr.GetString(1)
                });
            }

            return list;
        }

        public async Task<PagedResult<OptionSetDto>> GetPagedOptionSetsAsync(DataTableRequest req)
        {
            var result = new PagedResult<OptionSetDto>();

            await using var conn = GetConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_OptionSets_Paged", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Skip", req.Skip);
            cmd.Parameters.AddWithValue("@PageSize", req.PageSize);
            cmd.Parameters.AddWithValue("@Search", string.IsNullOrWhiteSpace(req.Search) ? DBNull.Value : req.Search);
            cmd.Parameters.AddWithValue("@SortColumn", req.SortColumn ?? "OptionId");
            cmd.Parameters.AddWithValue("@SortDirection", req.SortDirection ?? "DESC");

            await using var rdr = await cmd.ExecuteReaderAsync();

            if (await rdr.ReadAsync())
                result.FilteredCount = rdr.GetInt32(0);

            result.TotalCount = result.FilteredCount;

            if (await rdr.NextResultAsync())
            {
                while (await rdr.ReadAsync())
                {
                    result.Data.Add(new OptionSetDto
                    {
                        OptionId = rdr.GetInt32(0),
                        Name = rdr.GetString(1)
                    });
                }
            }

            return result;
        }

        public async Task<List<OptionDto>> GetOptionValuesAsync(int optionId)
        {
            var list = new List<OptionDto>();
            await using var conn = GetConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("sp_GetOptionValues", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@OptionId", optionId);

            await using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new OptionDto
                {
                    OptionValueId = rdr.GetInt32(0),
                    OptionId = rdr.GetInt32(1),
                    Value = rdr.GetString(2)
                });
            }

            return list;
        }

        public async Task CreateOptionSetAsync(string name)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                // Check existing
                await using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Options WHERE Name=@n", conn, (SqlTransaction)tran)
                {
                    CommandType = CommandType.Text
                })
                {
                    checkCmd.Parameters.AddWithValue("@n", name);
                    var count = (int)await checkCmd.ExecuteScalarAsync();
                    if (count > 0) throw new Exception("Option Set name already exists.");
                }

                await using var cmd = new SqlCommand("INSERT INTO Options (Name) VALUES (@n)", conn, (SqlTransaction)tran);
                cmd.Parameters.AddWithValue("@n", name);

                await cmd.ExecuteNonQueryAsync();
                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<OptionSetDto?> GetOptionSetAsync(int id)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            await using var cmd = new SqlCommand("SELECT OptionId, Name FROM Options WHERE OptionId=@i", conn);
            cmd.Parameters.AddWithValue("@i", id);

            await using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
                return new OptionSetDto
                {
                    OptionId = rdr.GetInt32(0),
                    Name = rdr.GetString(1)
                };

            return null;
        }

        public async Task<bool> UpdateOptionSetAsync(int id, string name)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                // Check duplicate
                await using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Options WHERE Name=@n AND OptionId<>@i", conn, (SqlTransaction)tran))
                {
                    checkCmd.Parameters.AddWithValue("@n", name);
                    checkCmd.Parameters.AddWithValue("@i", id);
                    var count = (int)await checkCmd.ExecuteScalarAsync();
                    if (count > 0) throw new Exception("Option Set name already exists.");
                }

                await using var cmd = new SqlCommand("UPDATE Options SET Name=@n WHERE OptionId=@i", conn, (SqlTransaction)tran);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@i", id);

                var rows = await cmd.ExecuteNonQueryAsync();
                await tran.CommitAsync();

                return rows > 0;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteOptionSetAsync(int id)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                await using (var cmd = new SqlCommand("DELETE OptionValues WHERE OptionId=@i", conn, (SqlTransaction)tran))
                {
                    cmd.Parameters.AddWithValue("@i", id);
                    await cmd.ExecuteNonQueryAsync();
                }

                await using var cmd2 = new SqlCommand("DELETE Options WHERE OptionId=@i", conn, (SqlTransaction)tran);
                cmd2.Parameters.AddWithValue("@i", id);

                var rows = await cmd2.ExecuteNonQueryAsync();
                await tran.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> AddOptionValueAsync(int setId, string value)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                await using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM OptionValues WHERE OptionId=@o AND [Value]=@v", conn, (SqlTransaction)tran))
                {
                    checkCmd.Parameters.AddWithValue("@o", setId);
                    checkCmd.Parameters.AddWithValue("@v", value);
                    var count = (int)await checkCmd.ExecuteScalarAsync();
                    if (count > 0) throw new Exception("This value already exists for the selected option set.");
                }

                await using var cmd = new SqlCommand("INSERT INTO OptionValues (OptionId,[Value]) VALUES (@o,@v)", conn, (SqlTransaction)tran);
                cmd.Parameters.AddWithValue("@o", setId);
                cmd.Parameters.AddWithValue("@v", value);

                var rows = await cmd.ExecuteNonQueryAsync();
                await tran.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOptionValueAsync(int id, string value, int setId)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                await using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM OptionValues WHERE OptionId=@o AND [Value]=@v AND OptionValueId<>@i", conn, (SqlTransaction)tran))
                {
                    checkCmd.Parameters.AddWithValue("@o", setId);
                    checkCmd.Parameters.AddWithValue("@v", value);
                    checkCmd.Parameters.AddWithValue("@i", id);

                    var count = (int)await checkCmd.ExecuteScalarAsync();
                    if (count > 0) throw new Exception("This value already exists for the selected option set.");
                }

                await using var cmd = new SqlCommand("UPDATE OptionValues SET [Value]=@v WHERE OptionValueId=@i", conn, (SqlTransaction)tran);
                cmd.Parameters.AddWithValue("@v", value);
                cmd.Parameters.AddWithValue("@i", id);

                var rows = await cmd.ExecuteNonQueryAsync();
                await tran.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteOptionValueAsync(int id)
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();

            try
            {
                await using var cmd = new SqlCommand("DELETE OptionValues WHERE OptionValueId=@i", conn, (SqlTransaction)tran);
                cmd.Parameters.AddWithValue("@i", id);

                var rows = await cmd.ExecuteNonQueryAsync();
                await tran.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }
    }
}
