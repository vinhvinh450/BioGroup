using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BioGroupAPI.Data;
using Npgsql;

var sqlConnStr = "Server=localhost;Database=BioGroupDB;User Id=sa;Password=123;TrustServerCertificate=True;";
var pgConnStr  = "Host=dpg-d7s55amgvqtc73bs2h7g-a.oregon-postgres.render.com;Database=biogroup_db;Username=biogroup_db_user;Password=gKc86k7wfPnMi1AahVps6sovFebmm2xh;SSL Mode=Require;Trust Server Certificate=true";

// ── Bước 1: Tạo schema PostgreSQL qua EF Core ──────────────
Console.WriteLine("🔧 Tạo schema PostgreSQL...");
var options = new DbContextOptionsBuilder<BioGroupContext>()
    .UseNpgsql(pgConnStr)
    .Options;
using (var ctx = new BioGroupContext(options))
{
    ctx.Database.EnsureCreated();
    Console.WriteLine("✅ Schema OK\n");
}

// ── Bước 2: Copy data từ SQL Server ────────────────────────
await using var sqlConn = new SqlConnection(sqlConnStr);
await using var pgConn  = new NpgsqlConnection(pgConnStr);

Console.WriteLine("Kết nối SQL Server...");
await sqlConn.OpenAsync();
Console.WriteLine("✅ SQL Server OK");

Console.WriteLine("Kết nối PostgreSQL...");
await pgConn.OpenAsync();
Console.WriteLine("✅ PostgreSQL OK\n");

// Xóa seed data cũ (theo thứ tự ngược FK)
Console.WriteLine("🗑 Xóa dữ liệu cũ trong PostgreSQL...");
var deleteSql = new[]
{
    "DELETE FROM \"OrderTrackings\"", "DELETE FROM \"Payments\"",
    "DELETE FROM \"OrderItems\"",     "DELETE FROM \"Orders\"",
    "DELETE FROM \"DistributorProducts\"", "DELETE FROM \"DistributorProfiles\"",
    "DELETE FROM \"ProductImages\"",  "DELETE FROM \"Users\"", "DELETE FROM \"Products\""
};
foreach (var sql in deleteSql)
{
    await using var cmd = new NpgsqlCommand(sql, pgConn);
    try { await cmd.ExecuteNonQueryAsync(); } catch { }
}
Console.WriteLine("✅ Đã xóa dữ liệu cũ\n");

var tables = new[]
{
    "Products", "ProductImages", "Users",
    "DistributorProfiles", "DistributorProducts",
    "Orders", "OrderItems", "Payments", "OrderTrackings"
};

int totalRows = 0;

foreach (var table in tables)
{
    Console.WriteLine($"📋 Migrate bảng: {table}");
    try
    {
        await using var readCmd = new SqlCommand($"SELECT * FROM [{table}]", sqlConn);
        await using var reader  = await readCmd.ExecuteReaderAsync();

        var schema  = reader.GetColumnSchema();
        var columns = schema.Select(c => c.ColumnName).ToList();
        if (columns.Count == 0) { Console.WriteLine("  (trống)\n"); continue; }

        var colList   = string.Join(", ", columns.Select(c => $"\"{c}\""));
        var paramList = string.Join(", ", columns.Select((_, i) => $"@p{i}"));
        var insertSql = $"INSERT INTO \"{table}\" ({colList}) VALUES ({paramList}) ON CONFLICT DO NOTHING";

        int count = 0, skip = 0;
        while (await reader.ReadAsync())
        {
            await using var insCmd = new NpgsqlCommand(insertSql, pgConn);
            for (int i = 0; i < columns.Count; i++)
            {
                var val = reader.GetValue(i);
                insCmd.Parameters.AddWithValue($"@p{i}", val is DBNull ? DBNull.Value : val);
            }
            try { await insCmd.ExecuteNonQueryAsync(); count++; }
            catch (Exception ex)
            {
                if (skip == 0) Console.WriteLine($"  ⚠ {ex.Message.Split('\n')[0]}");
                skip++;
            }
        }

        // Reset sequence
        try
        {
            await using var seqCmd = new NpgsqlCommand(
                $"SELECT setval(pg_get_serial_sequence('\"{table}\"', 'Id'), COALESCE(MAX(\"Id\"), 1)) FROM \"{table}\"", pgConn);
            await seqCmd.ExecuteScalarAsync();
        }
        catch { }

        totalRows += count;
        Console.WriteLine($"  ✅ {count} rows OK, {skip} bỏ qua\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ❌ Lỗi bảng {table}: {ex.Message.Split('\n')[0]}\n");
    }
}

Console.WriteLine($"✅ Hoàn tất! Tổng {totalRows} rows đã migrate.");
