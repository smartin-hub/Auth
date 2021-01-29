using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace Services
{
     public interface IUserService
     {
     Task<Utenti> GetUser(string UserId);
     Utenti CheckUser(string UserId);
     Utenti CheckUserByFid(string CodFid);
     Task<ICollection<Utenti>> GetAll();

     bool InsUtente(Utenti utente);
     bool UpdUtente(Utenti utente);
     bool DelUtente(Utenti utente);
     bool Salva();

     bool Authenticate(string username, string password);
     string GetToken(string username);

     }
}