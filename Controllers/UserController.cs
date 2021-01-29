using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;
using Services;
using Microsoft.AspNetCore.Mvc;
using Security;

namespace GestUser.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly IUserService userRepository;

        public UserController(IUserService userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpPost("auth")]
        [ProducesResponseType(400,Type = typeof(InfoMsg))]
        public IActionResult Authenticate([FromBody]Utenti userParam)
        {
            string tokenJWT = "";

            bool IsOk = userRepository.Authenticate(userParam.UserId, userParam.Password);

            if (!IsOk)
            {
                return BadRequest(new InfoMsg(DateTime.Today, string.Format($"User Id e/o Password non corrette!")));
            }
            else
            {
                tokenJWT = userRepository.GetToken(userParam.UserId);
            }

            return Ok(new { token = tokenJWT });

        }

        [HttpGet("all")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(Utenti))]
        public async Task<IActionResult> GetAllUser()
        {
            var utenti = await this.userRepository.GetAll();

            if (utenti.Count == 0)
            {
                return NotFound(string.Format("Non è stato trovato alcun utente!"));
            }

            return Ok(utenti);
        }

        [HttpGet("cerca/{userid}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(Utenti))]
        public async Task<IActionResult> GetUser(String userid)
        {
            var utente = await this.userRepository.GetUser(userid);

            if (utente == null)
            {
                return NotFound(new InfoMsg(DateTime.Today, string.Format($"Non è stato trovato l'utente {userid}!")));
            }

            return Ok(utente);
        }

        [HttpPost("inserisci")]
        [ProducesResponseType(201, Type = typeof(Utenti))]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public IActionResult SaveUtente([FromBody] Utenti utente)
        {
            if (utente == null)
            {
                return BadRequest(new InfoMsg(DateTime.Today, "E' necessario inserire i dati dell'utente"));
            }

            //Contolliamo se l'utente è presente
            var isPresent = userRepository.CheckUser(utente.UserId);

            if (isPresent != null)
            {
                return StatusCode(422, new InfoMsg(DateTime.Today, $"Utente {utente.UserId} presente in uso! Impossibile inserire!"));
            }
            
            //Contolliamo se la fidelity è presente
            var isCodFid = userRepository.CheckUserByFid(utente.CodFidelity);

            if (isCodFid != null)
            {
                return StatusCode(422, new InfoMsg(DateTime.Today, $"CodFid {utente.CodFidelity} in uso! Impossibile inserire!"));
            }

            //Verifichiamo che i dati siano corretti
            if (!ModelState.IsValid)
            {
                string ErrVal = "";

                foreach (var modelState in ModelState.Values) 
                {
                    foreach (var modelError in modelState.Errors) 
                    {
                        ErrVal += modelError.ErrorMessage + " - "; 
                    }
                }

                return BadRequest(new InfoMsg(DateTime.Today, ErrVal));
            }

            PasswordHasher Hasher = new PasswordHasher();

            //Crptiamo la Password
            utente.Password = Hasher.Hash(utente.Password);

            //verifichiamo che i dati siano stati regolarmente inseriti nel database
            if (!userRepository.InsUtente(utente))
            {
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Ci sono stati problemi nell'inserimento dell'Utente {utente.UserId}."));
            }

            return Ok(new InfoMsg(DateTime.Today, $"Inserimento Utente {utente.UserId} eseguito con successo!"));
        }

        [HttpDelete("elimina/{userid}")]
        [ProducesResponseType(201, Type = typeof(InfoMsg))]
        [ProducesResponseType(400, Type = typeof(InfoMsg))]
        [ProducesResponseType(422, Type = typeof(InfoMsg))]
        [ProducesResponseType(500)]
        public IActionResult DeleteUser(string UserId)
        {
            if (UserId == "")
            {
                return BadRequest(new InfoMsg(DateTime.Today, $"E' necessario inserire la userid dell'utente!"));
            }

            //Contolliamo se l'articolo è presente (Usare il metodo senza Traking)
            var user = userRepository.CheckUser(UserId);

            if (user == null)
            {
                return StatusCode(422, new InfoMsg(DateTime.Today, $"Utente {UserId} NON presente in anagrafica! Impossibile Eliminare!"));
            }

            //verifichiamo che i dati siano stati regolarmente eliminati dal database
            if (!userRepository.DelUtente(user))
            {
                //ModelState.AddModelError("", $"Ci sono stati problemi nella eliminazione dell'Articolo {articolo.CodArt}.  ");
                return StatusCode(500, new InfoMsg(DateTime.Today, $"Ci sono stati problemi nella eliminazione dell'utente {user.UserId}.  "));
            }

            return Ok(new InfoMsg(DateTime.Today, $"Eliminazione utente {user.UserId} eseguita con successo!"));

        }

    
    }
}
