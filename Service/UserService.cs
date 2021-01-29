
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Models;
using Security;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Helpers;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using System;
using Microsoft.IdentityModel.Tokens;

namespace Services
{
    public class UserService : IUserService
    {
        private readonly AlphaShopDbContext alphaShopDbContext;

        private readonly AppSettings appSettings;

        public UserService(AlphaShopDbContext alphaShopDbContext, IOptions<AppSettings> appSettings)
        {
            this.alphaShopDbContext = alphaShopDbContext;
            this.appSettings = appSettings.Value;
        }

        public async Task<Utenti> GetUser(string UserId)
        {
            return await this.alphaShopDbContext.Utenti
                .Where(c => c.UserId  == UserId)
                .Include(r => r.Profili)
                //.Where(c => c.UserId  == username && c.Password == password)
                //.Where(c => c.Password == password) 
                .FirstOrDefaultAsync();
        }

        public Utenti CheckUser(string UserId)
        {
            return this.alphaShopDbContext.Utenti
                .AsNoTracking()
                .Where(c => c.UserId  == UserId)
                .Include(r => r.Profili)
                .FirstOrDefault();
        }

        public Utenti CheckUserByFid(string CodFid)
        {
            return this.alphaShopDbContext.Utenti
                .AsNoTracking()
                .Where(c => c.CodFidelity  == CodFid)
                .FirstOrDefault();
        }        

        public async Task<ICollection<Utenti>> GetAll()
        {
            return await this.alphaShopDbContext.Utenti
                .Include(r => r.Profili)
                .OrderBy(c => c.UserId)
                .ToListAsync();
        }

        public  bool Authenticate(string username, string password)
        {
            bool retVal = false;

            PasswordHasher Hasher = new PasswordHasher();

            Utenti utente = this.alphaShopDbContext.Utenti
                .Include(r => r.Profili)
                .Where(c => c.UserId  == username)
                .FirstOrDefault();

            if (utente != null)
            {
                string EncryptPwd = utente.Password;
                
                retVal = Hasher.Check(EncryptPwd, password).Verified;
            }
            
            return retVal; 
        }

        public bool InsUtente(Utenti utente)
        {
            this.alphaShopDbContext.Add(utente);
            return Salva();
        }

        public bool UpdUtente(Utenti utente)
        {
            this.alphaShopDbContext.Update(utente);
            return Salva();
        }

        public bool DelUtente(Utenti utente)
        {
            this.alphaShopDbContext.Remove(utente);
            return Salva();
        }

        public bool Salva()
        {
            
            var saved = this.alphaShopDbContext.SaveChanges();
            
            return saved >= 0 ? true : false; 
        }

        public string GetToken(string username)
        {
            Utenti utente = this.CheckUser(username);

            //Creazione del Token Jwt
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.appSettings.Secret);

            ICollection<Profili> userRoles = utente.Profili;
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, utente.UserId));

            foreach (var Role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, Role.Tipo));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Validità del Token
                Expires = DateTime.UtcNow.AddDays(this.appSettings.Expiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
