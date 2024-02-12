using Clinique2000_Models.Models;
using Clinique2000_Models.ViewModels;
using Clinique2000_Services.Services.IServices;
using Clinique2000_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Clinique2000_Web.Areas.Patient.Controllers
{

    [Area("Patient")]
    //[Authorize(Roles = Constants.PatientRole)]
    public class PlageHorairesController : Controller
    {
        private readonly IFileDAttenteService _fileDAttenteService;
        private readonly IPlageHoraireService _plageHoraireService;
        private readonly IUserService _userService;
        private readonly ICliniqueService _cliniqueService;

        public PlageHorairesController(IPlageHoraireService plageHoraireService, IFileDAttenteService fileDAttenteService,IUserService userService, ICliniqueService cliniqueService)
        {
            _fileDAttenteService = fileDAttenteService;
            _plageHoraireService = plageHoraireService;
            _userService = userService;
            _cliniqueService = cliniqueService;
        }


        // GET: PlageHoraireController
        public async Task<IActionResult> Index(int CliniqueId)
        {
            Clinique objClinique = await _cliniqueService.GetByIdAsync(CliniqueId);
            int FileDAttenteId = 0;
            var fileCourante = _cliniqueService.ObtenirFileDAttenteInscriptionsActives(objClinique);
            if (fileCourante != null)
            {
                FileDAttenteId = fileCourante.Id;

                TimeSpan heureActuelle = DateTime.Now.TimeOfDay;

                List<PlageHoraire> PlageHoraireList = new List<PlageHoraire>(await _plageHoraireService.GetAllByFileDAttenteId(FileDAttenteId));


                if (fileCourante.DateHeureOuverture.Date == DateTime.Now.Date)
                {
                    // Montrer plages horaires disponibles ET plages horaires du présent et du futur
                    for (int x = PlageHoraireList.Count - 1; x >= 0; x--)
                    {
                        if (PlageHoraireList[x].Debut <= heureActuelle)
                        {
                            PlageHoraireList.RemoveAt(x);
                        }

                    }
                }

                // Montrer tous plages horaires disponibles

                return View(PlageHoraireList);
            }

            return View("Index",null);

        }

        // GET: PlageHoraireController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }
        [AllowAnonymous]
        public async Task<IActionResult> UpdateReserveStatus(int id)
        {
            User user = await _userService.ObtenirUtilisateurCourantAsync();
            var plageHoraire = await _plageHoraireService.GetByIdAsync(id);
            PlageHoraire verifieRDV = (PlageHoraire) await _plageHoraireService.VerifierPlageHoraireReserve(user.Id);

            if (verifieRDV == null)
            {
                if (plageHoraire == null)
                {
                    return NotFound();
                }

                plageHoraire.EstReservee = true;
                plageHoraire.PatientId = user.Id;
                await _plageHoraireService.EditAsync(plageHoraire);

                return View("ConfirmationRDV", await RendezVousAssigne(id));
            }
            
            return View("RDVFixe",await RendezVousAssigne(verifieRDV.Id));
        }

        public async Task<ConfirmationRDVVM> RendezVousAssigne(int id)
        {
            User user = await _userService.ObtenirUtilisateurCourantAsync();
            var plageHoraire = await _plageHoraireService.GetByIdAsync(id);
           
            var confirmationViewModel = new ConfirmationRDVVM
            {
                NomPatient = user.Prenom + " " + user.Nom,
                DateRendezVous = plageHoraire.FileDAttente.DateHeureOuverture,
                HeureRendezVous = plageHoraire.Debut.ToString(@"hh\:mm"),
                NomClinique = plageHoraire.FileDAttente.Clinique.Nom,
                AdresseClinique = plageHoraire.FileDAttente.Clinique.Adresse,
                TelephoneClinique = plageHoraire.FileDAttente.Clinique.Telephone
            };

            return confirmationViewModel;
        }



        // GET: PlageHoraireController/Create
        public async Task<ActionResult> Create(int fileDAttenteId)
        {
            if (fileDAttenteId <= 0)
                return NotFound();
            var fileCourante = await _fileDAttenteService.GetByIdAsync(fileDAttenteId);
            if (fileCourante == null)
                return NotFound();
            else
            {

                TimeSpan heureDebut = fileCourante.DateHeureOuverture.TimeOfDay;
                TimeSpan dureeTotalFileDAttente = fileCourante.DateHeureFermeture.TimeOfDay - fileCourante.DateHeureOuverture.TimeOfDay;
                int nombreMedecins = fileCourante.NombreMedecins;
                int dureeConsultation = fileCourante.Clinique.DureeParConsultation;
                int totalMinutes = (int)dureeTotalFileDAttente.TotalMinutes;
                int nombrePlageshoraires = totalMinutes / dureeConsultation;

                //
                for (int x = 0; x < nombrePlageshoraires; x++)
                {
                    int dureeTemp = x * dureeConsultation;
                    TimeSpan debutTemp = heureDebut.Add(TimeSpan.FromMinutes(dureeTemp));
                    for (int y = 0; y < nombreMedecins; y++)
                    {
                        PlageHoraire plagehoraire = new PlageHoraire()
                        {
                            Debut = debutTemp,
                            FileDAttenteId = fileCourante.Id,
                            NumeroMedecin = y,
                        };
                        await _plageHoraireService.CreateAsync(plagehoraire);
                    }
                }

                return RedirectToAction("Index", fileDAttenteId);

            }
        }

    }
}
