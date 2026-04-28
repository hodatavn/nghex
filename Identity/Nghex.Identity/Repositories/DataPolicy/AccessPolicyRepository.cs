using System.Data;
using Dapper;
using Nghex.Data;
using Nghex.Identity.Persistence.Entities.DataPolicy;
using Nghex.Identity.Repositories.DataPolicy.Interfaces;

namespace Nghex.Identity.Repositories.DataPolicy;

public class AccessPolicyRepository(IDatabaseExecutor databaseExecutor) : IAccessPolicyRepository
{
    public Task<IEnumerable<(string policyType, string policyMode, string? policyCode)>> GetAllPoliciesAsync(long accountId)
    {
        const string sql = @"
            SELECT ap.Policy_Type, ap.AP_Mode, apd.AP_Code
            FROM SYS_ACCESS_POLICY ap
            LEFT JOIN sys_access_policy_detail apd
                ON apd.Account_Id = ap.Account_Id AND apd.Policy_Type = ap.Policy_Type
            WHERE ap.Account_Id = :AccountId";

        var p = new DynamicParameters();
        p.Add("AccountId", accountId, DbType.Int64);
        return databaseExecutor.ExecuteQueryMultipleAsync<(string, string, string?)>(sql, p);
    }

    public Task<IEnumerable<AccessPolicyEntity>> GetByAccountIdAsync(long accountId)
    {
        const string sql = @"
            SELECT Account_Id, Policy_Type, AP_Mode, Created_At, Updated_At
            FROM SYS_ACCESS_POLICY
            WHERE Account_Id = :AccountId
            ORDER BY Policy_Type";

        var p = new DynamicParameters();
        p.Add("AccountId", accountId, DbType.Int64);
        return databaseExecutor.ExecuteQueryMultipleAsync<AccessPolicyEntity>(sql, p);
    }

    public Task<IEnumerable<AccessPolicyDetailEntity>> GetDetailsByAccountIdAsync(long accountId)
    {
        const string sql = @"
            SELECT Account_Id, Policy_Type, AP_Code
            FROM SYS_ACCESS_POLICY_DETAIL
            WHERE Account_Id = :AccountId
            ORDER BY Policy_Type, AP_Code";

        var p = new DynamicParameters();
        p.Add("AccountId", accountId, DbType.Int64);
        return databaseExecutor.ExecuteQueryMultipleAsync<AccessPolicyDetailEntity>(sql, p);
    }

    public async Task UpsertAsync(long accountId, string policyType, string policyMode, string updatedBy)
    {
        const string sql = @"
            MERGE INTO SYS_ACCESS_POLICY ap
            USING (SELECT :AccountId AS Account_Id, :PolicyType AS Policy_Type FROM dual) src
            ON (ap.Account_Id = src.Account_Id AND ap.Policy_Type = src.Policy_Type)
            WHEN MATCHED THEN
                UPDATE SET AP_Mode = :PolicyMode, Updated_At = SYSDATE
            WHEN NOT MATCHED THEN
                INSERT (Account_Id, Policy_Type, AP_Mode, Created_At, Updated_At)
                VALUES (:AccountId, :PolicyType, :PolicyMode, SYSDATE, SYSDATE)";

        var p = new DynamicParameters();
        p.Add("AccountId", accountId, DbType.Int64);
        p.Add("PolicyType", policyType, DbType.String);
        p.Add("PolicyMode", policyMode, DbType.String);
        await databaseExecutor.ExecuteNonQueryAsync(sql, p);
    }

    public Task ReplaceDetailsAsync(long accountId, string policyType, IEnumerable<string> policyCodes)
    {
        var codes = policyCodes.ToList();
        return databaseExecutor.ExecuteInTransactionAsync(async tx =>
        {
            const string deleteSql = @"
                DELETE FROM SYS_ACCESS_POLICY_DETAIL
                WHERE Account_Id = :AccountId AND Policy_Type = :PolicyType";

            var dp = new DynamicParameters();
            dp.Add("AccountId", accountId, DbType.Int64);
            dp.Add("PolicyType", policyType, DbType.String);
            await databaseExecutor.ExecuteNonQueryInTransactionAsync(tx, deleteSql, dp);

            foreach (var code in codes)
            {
                const string insertSql = @"
                    INSERT INTO SYS_ACCESS_POLICY_DETAIL (Account_Id, Policy_Type, AP_Code)
                    VALUES (:AccountId, :PolicyType, :APCode)";

                var ip = new DynamicParameters();
                ip.Add("AccountId", accountId, DbType.Int64);
                ip.Add("PolicyType", policyType, DbType.String);
                ip.Add("APCode", code, DbType.String);
                await databaseExecutor.ExecuteNonQueryInTransactionAsync(tx, insertSql, ip);
            }
        });
    }

    public Task DeleteByTypeAsync(long accountId, string policyType)
    {
        return databaseExecutor.ExecuteInTransactionAsync(async tx =>
        {
            var p = new DynamicParameters();
            p.Add("AccountId", accountId, DbType.Int64);
            p.Add("PolicyType", policyType, DbType.String);

            await databaseExecutor.ExecuteNonQueryInTransactionAsync(tx,
                "DELETE FROM SYS_ACCESS_POLICY_DETAIL WHERE Account_Id = :AccountId AND Policy_Type = :PolicyType", p);
            await databaseExecutor.ExecuteNonQueryInTransactionAsync(tx,
                "DELETE FROM SYS_ACCESS_POLICY WHERE Account_Id = :AccountId AND Policy_Type = :PolicyType", p);
        });
    }

    public Task DeleteAllAsync(long accountId)
    {
        return databaseExecutor.ExecuteInTransactionAsync(async tx =>
        {
            var p = new DynamicParameters();
            p.Add("AccountId", accountId, DbType.Int64);

            await databaseExecutor.ExecuteNonQueryInTransactionAsync(tx,
                "DELETE FROM SYS_ACCESS_POLICY_DETAIL WHERE Account_Id = :AccountId", p);
            await databaseExecutor.ExecuteNonQueryInTransactionAsync(tx,
                "DELETE FROM SYS_ACCESS_POLICY WHERE Account_Id = :AccountId", p);
        });
    }
}
