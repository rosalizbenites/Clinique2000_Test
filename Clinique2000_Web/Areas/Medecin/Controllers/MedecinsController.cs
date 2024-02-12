using Clinique2000_Models.Models;
using Clinique2000_Models.ViewModels;
using Clinique2000_Services.Services.IServices;
using Clinique2000_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;

namespace Clinique2000_Web.Areas.Medecin.Controllers
{

    [Area("Medecin")]
    [Authorize(Roles = Constants.MedecinRole)]
    public class MedecinsController : Controller
    {


        private readonly IFileDAttenteService _fileDAttenteService;
        private readonly IPlageHoraireService _plageHoraireService;
        private readonly IUserService _userService;
        private readonly ICliniqueService _cliniqueService;


        public MedecinsController(IPlageHoraireService plageHoraireService, IFileDAttenteService fileDAttenteService, IUserService userService, ICliniqueService cliniqueService)
        {
            _fileDAttenteService = fileDAttenteService;
            _plageHoraireService = plageHoraireService;
            _userService = userService;
            _cliniqueService = cliniqueService;
        }

        public async Task<IActionResult> GererConsultations(int FileDAttenteid)
        {

            User user = await _userService.ObtenirUtilisateurCourantAsync();

            

            var listplagesMedecin = await _plageHoraireService.VerificationListConsultationMedecin(FileDAttenteid, user.Id);

            if (listplagesMedecin.Count == 0)
                return View(null);

            else
            {
                var plageHoraireCourante = await _plageHoraireService.GetByIdAsync(listplagesMedecin[0].Id);
                plageHoraireCourante.DebutReel = DateTime.Now.TimeOfDay;
                await _plageHoraireService.EditAsync(plageHoraireCourante);

                ConsultationVM Consultation = new ConsultationVM
                {
                    Nom = listplagesMedecin[0].Patient.Nom,
                    Prenom = listplagesMedecin[0].Patient.Prenom,
                    NumeroAssuranceMaladie = listplagesMedecin[0].Patient.NumeroAssuranceMaladie,
                    HeurePrevueRV = listplagesMedecin[0].Debut,
                    PlageHoraireId = listplagesMedecin[0].Id,
                    FileDAttenteId = listplagesMedecin[0].FileDAttenteId
                };

                               
                if (listplagesMedecin.Count == 1)
                    Consultation.EstDerniereConsultation = true;

                return View(Consultation);
            } 

            
        }

       
        // GET: MedecinsController/Details/5
        public async Task<IActionResult> GererEtatConsultations(int PlageHoraireId)
        {
            var plageHoraireCourante = await _plageHoraireService.GetByIdAsync(PlageHoraireId);

            plageHoraireCourante.FinReelle = DateTime.Now.TimeOfDay;
            plageHoraireCourante.EstReservee = false;
            await _plageHoraireService.EditAsync(plageHoraireCourante);

            return RedirectToAction("GererConsultations", new { FileDAttenteId = plageHoraireCourante.FileDAttenteId });
        }

        // GET: MedecinsController/Create
        public async Task<IActionResult> GererTerminerConsultations(int PlageHoraireId)
        {

            var plageHoraireCourante = await _plageHoraireService.GetByIdAsync(PlageHoraireId);

            plageHoraireCourante.FinReelle = DateTime.Now.TimeOfDay;
            plageHoraireCourante.EstReservee = false;

            await _plageHoraireService.EditAsync(plageHoraireCourante);

            ConsultationVM Consultation = new ConsultationVM
            {
                HeurePrevueRV = plageHoraireCourante.Debut,   
            };

            return View("ConsultationTerminee",Consultation);
        }


        public async Task<IActionResult> voirConsultations()
        {

            User user = await _userService.ObtenirUtilisateurCourantAsync();
            if (user != null && user.CliniqueId != null)
            {
                var cliniqueCourante = await _cliniqueService.GetByIdAsync((int)user.CliniqueId);
                var fileDAttenteCourante = _cliniqueService.ObtenirFileDAttenteInscriptionsActives(cliniqueCourante);

                return RedirectToAction("GererConsultations", new { FileDAttenteId = fileDAttenteCourante.Id });
            }

            return NotFound();
        }

        

        

    }
}
