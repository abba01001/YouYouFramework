using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TCPServer.Core;

namespace TCPServer.Utils
{
    public class JwtHelper
    {
        public static string GenerateToken(string username, string role)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KeyUtils.GetTokenKey(Token.TokenKey)));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "your-issuer",
                audience: "your-audience",
                claims: claims,
                expires: DateTime.Now.AddHours(24), // 设置过期时间
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler(); // 创建 Token 解析器
            var key = Encoding.UTF8.GetBytes(KeyUtils.GetTokenKey(Token.TokenKey)); // 解析密钥

            try
            {
                // 设置 Token 验证参数
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // 验证发行者
                    ValidIssuer = "your-issuer", // 发行者
                    ValidateAudience = true, // 验证受众
                    ValidAudience = "your-audience", // 受众
                    ValidateLifetime = true, // 验证过期时间
                    IssuerSigningKey = new SymmetricSecurityKey(key), // 使用密钥验证签名
                    ClockSkew = TimeSpan.Zero  // 防止时间差异
                };

                // 验证 Token 并获取用户信息
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal; // 返回解密后的 ClaimsPrincipal
            }
            catch (Exception ex)
            {
                // 如果 Token 验证失败，返回 null
                return null;
            }
        }


        public static bool CheckUserPermission(string token, string requiredRole)
        {
            var principal = ValidateToken(token);
            if (principal == null)
            {
                return false; // Token 无效
            }

            var roleClaim = principal.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value == requiredRole; // 比较角色
        }
    }
}
