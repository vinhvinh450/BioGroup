# ============================================================
#  migrate-to-postgres.ps1
#  Di chuyển dữ liệu từ SQL Server local → PostgreSQL (Render)
#  Cách dùng: .\migrate-to-postgres.ps1 -PgUrl "postgresql://..."
# ============================================================
param(
    [Parameter(Mandatory=$true)]
    [string]$PgUrl   # External Database URL từ Render
)

# ── Load Npgsql từ NuGet cache ─────────────────────────────
$npgsqlDll = Get-ChildItem "$env:USERPROFILE\.nuget\packages\npgsql" -Recurse -Filter "Npgsql.dll" |
    Where-Object { $_.FullName -match "net8.0[^\w]" -and $_.FullName -notmatch "ref\\" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName

if (-not $npgsqlDll) {
    Write-Error "Không tìm thấy Npgsql.dll. Hãy chạy 'dotnet restore' trong BioGroupAPI trước."
    exit 1
}
Add-Type -Path $npgsqlDll
Write-Host "✅ Đã load Npgsql: $npgsqlDll" -ForegroundColor Green

# ── Kết nối SQL Server ─────────────────────────────────────
$sqlConnStr = "Server=localhost;Database=BioGroupDB;User Id=sa;Password=123;TrustServerCertificate=True;"
$sqlConn = New-Object System.Data.SqlClient.SqlConnection($sqlConnStr)
try {
    $sqlConn.Open()
    Write-Host "✅ Kết nối SQL Server thành công" -ForegroundColor Green
} catch {
    Write-Error "❌ Không kết nối được SQL Server: $_"
    exit 1
}

# ── Kết nối PostgreSQL ─────────────────────────────────────
$pgConn = New-Object Npgsql.NpgsqlConnection($PgUrl)
try {
    $pgConn.Open()
    Write-Host "✅ Kết nối PostgreSQL thành công" -ForegroundColor Green
} catch {
    Write-Error "❌ Không kết nối được PostgreSQL: $_"
    $sqlConn.Close()
    exit 1
}

# ── Helper: đọc bảng SQL Server ────────────────────────────
function Read-SqlTable($conn, $tableName) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT * FROM [$tableName]"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $adapter.Fill($dt) | Out-Null
    return $dt
}

# ── Helper: insert 1 row vào PostgreSQL ────────────────────
function Insert-PgRow($pgConn, $tableName, $row, $columns) {
    $colList  = ($columns | ForEach-Object { "`"$_`"" }) -join ", "
    $paramList = ($columns | ForEach-Object -Begin {$i=0} -Process { $i++; "@p$i" }) -join ", "

    $cmd = $pgConn.CreateCommand()
    $cmd.CommandText = "INSERT INTO `"$tableName`" ($colList) VALUES ($paramList) ON CONFLICT DO NOTHING"

    $i = 0
    foreach ($col in $columns) {
        $i++
        $val = $row[$col]
        if ($val -is [DBNull]) {
            $cmd.Parameters.AddWithValue("@p$i", [DBNull]::Value) | Out-Null
        } else {
            $cmd.Parameters.AddWithValue("@p$i", $val) | Out-Null
        }
    }
    try {
        $cmd.ExecuteNonQuery() | Out-Null
    } catch {
        Write-Warning "  ⚠ Bỏ qua row lỗi [$tableName]: $($_.Exception.Message.Split("`n")[0])"
    }
}

# ── Helper: reset sequence (auto-increment) ─────────────────
function Reset-PgSequence($pgConn, $tableName) {
    $cmd = $pgConn.CreateCommand()
    $cmd.CommandText = "SELECT setval(pg_get_serial_sequence('`"$tableName`"', 'Id'), COALESCE(MAX(`"Id`"), 1)) FROM `"$tableName`""
    try { $cmd.ExecuteNonQuery() | Out-Null } catch {}
}

# ── Danh sách bảng theo thứ tự FK ──────────────────────────
$tables = @(
    "Products",
    "ProductImages",
    "Users",
    "DistributorProfiles",
    "DistributorProducts",
    "Orders",
    "OrderItems",
    "Payments",
    "OrderTrackings"
)

# ── Migrate từng bảng ──────────────────────────────────────
$totalRows = 0
foreach ($table in $tables) {
    Write-Host "`n📋 Đang migrate bảng: $table" -ForegroundColor Cyan

    try {
        $dt = Read-SqlTable $sqlConn $table
        $count = $dt.Rows.Count
        Write-Host "   Tìm thấy $count rows"

        if ($count -eq 0) {
            Write-Host "   (Bỏ qua — không có dữ liệu)" -ForegroundColor Gray
            continue
        }

        $columns = $dt.Columns | ForEach-Object { $_.ColumnName }
        $done = 0

        foreach ($row in $dt.Rows) {
            Insert-PgRow $pgConn $table $row $columns
            $done++
            if ($done % 50 -eq 0) { Write-Host "   ... $done/$count" }
        }

        Reset-PgSequence $pgConn $table
        Write-Host "   ✅ Đã migrate $done rows" -ForegroundColor Green
        $totalRows += $done
    } catch {
        Write-Warning "   ⚠ Lỗi bảng $table : $_"
    }
}

# ── Đóng kết nối ───────────────────────────────────────────
$sqlConn.Close()
$pgConn.Close()

Write-Host "`n✅ Hoàn tất! Tổng cộng $totalRows rows đã migrate." -ForegroundColor Green
