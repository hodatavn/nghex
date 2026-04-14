using System.Data;
using Dapper;
using Nghex.Base.Entities;
using Nghex.Identity.Persistence.Entities;
using Nghex.Base.Repositories;
using Nghex.Identity.Repositories.Interfaces;
using Nghex.Logging.Interfaces;
using Nghex.Data;

namespace Nghex.Identity.Repositories
{
    /// <summary>
    /// JWT Token Repository implementation
    /// </summary>
    public class JwtTokenRepository : IJwtTokenRepository
    {
        private readonly IDatabaseExecutor _databaseExecutor;
        private readonly ILoggingService _loggingService;

        public JwtTokenRepository(IDatabaseExecutor databaseExecutor, ILoggingService loggingService)
        {
            _databaseExecutor = databaseExecutor ?? throw new ArgumentNullException(nameof(databaseExecutor));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public async Task<long> AddAsync(JwtTokenEntity jwtToken)
        {
            try
            {
                const string query = @"
                    INSERT INTO sys_jwt_tokens (
                        Account_Id, Token_Id, Refresh_Token, Expires_At, Refresh_Expires_At,
                        Is_Revoked, IP_Address, User_Agent, Created_At, Created_By
                    ) VALUES (
                        :Account_Id, :Token_Id, :Refresh_Token, 
                        :Expires_At,
                        :Refresh_Expires_At,
                        0, :IP_Address, :User_Agent, SYSDATE, :Created_By
                    ) RETURNING Id INTO :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Account_Id", jwtToken.AccountId, DbType.Int64);
                parameters.Add("Token_Id", jwtToken.TokenId, DbType.String);
                parameters.Add("Refresh_Token", jwtToken.RefreshToken, DbType.String);
                // Store all timestamps in UTC
                parameters.Add("Expires_At", jwtToken.ExpiresAt, DbType.DateTime);
                parameters.Add("Refresh_Expires_At", jwtToken.RefreshExpiresAt, DbType.DateTime);
                parameters.Add("IP_Address", jwtToken.IpAddress ?? "", DbType.String);
                parameters.Add("User_Agent", jwtToken.UserAgent ?? "", DbType.String);
                parameters.Add("Created_By", jwtToken.CreatedBy ?? "", DbType.String);

                return await _databaseExecutor.ExecuteInsertWithReturnIdAsync(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error adding JWT token",
                    ex,
                    source: "JwtTokenRepository.AddAsync",
                    module: "JWT",
                    action: "AddToken",
                    details: new { TokenId = jwtToken.TokenId, AccountId = jwtToken.AccountId }
                );
                throw;
            }
        }

        public async Task<JwtTokenEntity?> GetByTokenIdAsync(string tokenId)
        {
            try
            {
                const string query = @"
                    SELECT Id, Account_Id, Token_Id, Refresh_Token, 
                        Expires_At, 
                        Refresh_Expires_At,
                        Is_Revoked, 
                        Revoked_At, 
                        IP_Address, User_Agent
                    FROM sys_jwt_tokens
                    WHERE Token_Id = :Token_Id AND Is_Revoked = 0";

                var parameters = new DynamicParameters();
                parameters.Add("Token_Id", tokenId, DbType.String);

                return await _databaseExecutor.ExecuteQuerySingleAsync<JwtTokenEntity>(query, parameters);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting JWT token by token ID: {tokenId}",
                    ex,
                    source: "JwtTokenRepository.GetByTokenIdAsync",
                    module: "JWT",
                    action: "GetToken"
                );
                throw;
            }
        }

        public async Task<JwtTokenEntity?> GetByRefreshTokenAsync(string refreshToken)
        {
            try
            {
                const string query = @"
                    SELECT Id, Account_Id, Token_Id, Refresh_Token, 
                        Expires_At, 
                        Refresh_Expires_At,
                        Is_Revoked, 
                        Revoked_At, 
                        IP_Address, User_Agent
                    FROM sys_jwt_tokens
                    WHERE Refresh_Token = :Refresh_Token AND Is_Revoked = 0";

                var parameters = new DynamicParameters();
                parameters.Add("Refresh_Token", refreshToken, DbType.String);

                var result = await _databaseExecutor.ExecuteQuerySingleAsync<JwtTokenEntity>(query, parameters);
                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting JWT token by refresh token",
                    ex,
                    source: "JwtTokenRepository.GetByRefreshTokenAsync",
                    module: "JWT",
                    action: "GetToken"
                );
                throw;
            }
        }

        public async Task<List<JwtTokenEntity>> GetByAccountIdAsync(long accountId)
        {
            try
            {
                const string query = @"
                    SELECT Id, Account_Id, Token_Id, Refresh_Token, 
                        Expires_At, 
                        Refresh_Expires_At,
                        Is_Revoked, 
                        Revoked_At, 
                        IP_Address, User_Agent
                    FROM sys_jwt_tokens
                    WHERE Account_Id = :Account_Id AND Is_Revoked = 0";

                var parameters = new DynamicParameters();
                parameters.Add("Account_Id", accountId, DbType.Int64);

                var results = await _databaseExecutor.ExecuteQueryMultipleAsync<JwtTokenEntity>(query, parameters);
                return results.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error getting JWT tokens by account ID: {accountId}",
                    ex,
                    source: "JwtTokenRepository.GetByAccountIdAsync",
                    module: "JWT",
                    action: "GetTokens"
                );
                throw;
            }
        }

        public async Task<bool> UpdateAsync(JwtTokenEntity jwtToken)
        {
            try
            {
                const string query = @"
                    UPDATE sys_jwt_tokens SET
                        Account_Id = :Account_Id,
                        Token_Id = :Token_Id,
                        Refresh_Token = :Refresh_Token,
                        Expires_At = :Expires_At,
                        Refresh_Expires_At = :Refresh_Expires_At,
                        Is_Revoked = :Is_Revoked,
                        Revoked_At = :Revoked_At,
                        IP_Address = :IP_Address,
                        User_Agent = :User_Agent,
                        Updated_At = SYSDATE,
                        Updated_By = :Updated_By
                    WHERE Id = :Id";

                var parameters = new DynamicParameters();
                parameters.Add("Id", jwtToken.Id);
                parameters.Add("Account_Id", jwtToken.AccountId, DbType.Int64);
                parameters.Add("Token_Id", jwtToken.TokenId, DbType.String);
                parameters.Add("Refresh_Token", jwtToken.RefreshToken, DbType.String);
                // Store all timestamps in UTC
                parameters.Add("Expires_At", jwtToken.ExpiresAt, DbType.DateTime);
                parameters.Add("Refresh_Expires_At", jwtToken.RefreshExpiresAt, DbType.DateTime);
                parameters.Add("Is_Revoked", jwtToken.IsRevoked ? 1 : 0);
                parameters.Add("Revoked_At", jwtToken.RevokedAt, DbType.DateTime);
                parameters.Add("IP_Address", jwtToken.IpAddress ?? "", DbType.String);
                parameters.Add("User_Agent", jwtToken.UserAgent ?? "", DbType.String);
                parameters.Add("Updated_By", jwtToken.UpdatedBy ?? "", DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error updating JWT token: {jwtToken.Id}",
                    ex,
                    source: "JwtTokenRepository.UpdateAsync",
                    module: "JWT",
                    action: "UpdateToken"
                );
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string tokenId)
        {
            try
            {
                const string query = @"
                    UPDATE sys_jwt_tokens SET
                        Is_Revoked = 1,
                        Revoked_At = SYSDATE,
                        Updated_At = SYSDATE,
                        Updated_By = 'system'
                    WHERE Token_Id = :Token_Id";

                var parameters = new DynamicParameters();
                parameters.Add("Token_Id", tokenId, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error revoking JWT token: {tokenId}",
                    ex,
                    source: "JwtTokenRepository.RevokeTokenAsync",
                    module: "JWT",
                    action: "RevokeToken"
                );
                throw;
            }
        }

        public async Task<bool> RevokeByRefreshTokenAsync(string refreshToken)
        {
            try
            {
                const string query = @"
                    UPDATE sys_jwt_tokens SET
                        Is_Revoked = 1,
                        Revoked_At = SYSDATE,
                        Updated_At = SYSDATE,
                        Updated_By = 'system'
                    WHERE Refresh_Token = :Refresh_Token AND Is_Revoked = 0";

                var parameters = new DynamicParameters();
                parameters.Add("Refresh_Token", refreshToken, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    "Error revoking JWT token by refresh token",
                    ex,
                    source: "JwtTokenRepository.RevokeByRefreshTokenAsync",
                    module: "JWT",
                    action: "RevokeByRefreshToken"
                );
                throw;
            }
        }

        public async Task<bool> TryRevokeTokenAsync(string tokenId)
        {
            try
            {
                const string query = @"
                    UPDATE sys_jwt_tokens SET
                        Is_Revoked = 1,
                        Revoked_At = SYSDATE,
                        Updated_At = SYSDATE,
                        Updated_By = 'system'
                    WHERE Token_Id = :Token_Id AND Is_Revoked = 0";

                var parameters = new DynamicParameters();
                parameters.Add("Token_Id", tokenId, DbType.String);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error try-revoking JWT token: {tokenId}",
                    ex,
                    source: "JwtTokenRepository.TryRevokeTokenAsync",
                    module: "JWT",
                    action: "TryRevokeToken"
                );
                throw;
            }
        }

        public async Task<bool> RevokeAllTokensForAccountAsync(long accountId)
        {
            try
            {
                const string query = @"
                    UPDATE sys_jwt_tokens SET
                        Is_Revoked = 1,
                        Revoked_At = SYSDATE,
                        Updated_At = SYSDATE,
                        Updated_By = 'system'
                    WHERE Account_Id = :Account_Id AND Is_Revoked = 0";

                var parameters = new DynamicParameters();
                parameters.Add("Account_Id", accountId, DbType.Int64);

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error revoking all JWT tokens for account: {accountId}",
                    ex,
                    source: "JwtTokenRepository.RevokeAllTokensForAccountAsync",
                    module: "JWT",
                    action: "RevokeAllTokens"
                );
                throw;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync()
        {
            try
            {
                const string query = @"
                    DELETE FROM sys_jwt_tokens
                    WHERE Expires_At < SYSDATE OR Refresh_Expires_At < SYSDATE";

                var rowsAffected = await _databaseExecutor.ExecuteNonQueryAsync(query);
                return rowsAffected;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error cleaning up expired JWT tokens",
                    ex,
                    source: "JwtTokenRepository.CleanupExpiredTokensAsync",
                    module: "JWT",
                    action: "CleanupTokens"
                );
                throw;
            }
        }

        public async Task<bool> IsTokenValidAsync(string tokenId)
        {
            try
            {
                const string query = @"
                    SELECT COUNT(1)
                    FROM sys_jwt_tokens
                    WHERE Token_Id = :Token_Id AND Is_Revoked = 0 AND Expires_At > SYSDATE";

                var parameters = new DynamicParameters();
                parameters.Add("Token_Id", tokenId, DbType.String);

                var count = await _databaseExecutor.ExecuteScalarAsync<long>(query, parameters);
                return count > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync(
                    $"Error validating JWT token: {tokenId}",
                    ex,
                    source: "JwtTokenRepository.IsTokenValidAsync",
                    module: "JWT",
                    action: "ValidateToken"
                );
                throw;
            }
        }
    }
}
