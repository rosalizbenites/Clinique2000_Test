using Clinique2000_DataAccess.Data;
using Clinique2000_Models.Models;
using Clinique2000_Models.ViewModels;
using Clinique2000_Services.Services;
using Clinique2000_Services.Services.IServices;
using Clinique2000_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinique2000_Web.Areas.Patient.Controllers
{
    
    
    [Area("Utilisateur")]
    
    public class CliniquesController : Controller
    {
        private readonly Clinique2000DbContext _context;
        private readonly IAzureMapsService _azureMapsService;
        private readonly ICliniqueService _cliniqueService;
        private readonly IUserService _userService;
        private readonly IPlageHoraireService _plageHoraireService;

        public CliniquesController(Clinique2000DbContext context, IAzureMapsService azureMapsService, ICliniqueService cliniqueService, IUserService userService, IPlageHoraireService plageHoraireService)
        {
            _context = context;
            _azureMapsService = azureMapsService;
            _cliniqueService = cliniqueService;
            _userService = userService;
            _plageHoraireService = plageHoraireService;
        }

        //[Authorize(Roles = Constants.PatientRole)]
        // GET: Cliniques		
        public async Task<IActionResult> Recherche()
        {
            User userCourant = await _userService.ObtenirUtilisateurCourantAsync();

            bool estPatient = await _userService.ValiderRoleAsync(userCourant.Id, Constants.PatientRole);

            if (!estPatient)
            {
                RedirectToPage("/Account/Manage/Index", new { area = "Identity" });
            }



            RechercheCliniquesVM vm = new RechercheCliniquesVM();
            PlageHoraire verifieRDV = (PlageHoraire)await _plageHoraireService.VerifierPlageHoraireReserve(userCourant.Id);

            if (userCourant != null)
            {
                if (verifieRDV != null)
                {
                    var confirmationViewModel = new ConfirmationRDVVM
                    {
                        NomPatient = userCourant.Prenom + " " + userCourant.Nom,
                        DateRendezVous = verifieRDV.FileDAttente.DateHeureOuverture,
                        HeureRendezVous = verifieRDV.Debut.ToString(@"hh\:mm"),
                        NomClinique = verifieRDV.FileDAttente.Clinique.Nom,
                        AdresseClinique = verifieRDV.FileDAttente.Clinique.Adresse,
                        TelephoneClinique = verifieRDV.FileDAttente.Clinique.Telephone
                    };                   
                    return View("RDVFixe", confirmationViewModel);
                }
                else
                {
                    List<Clinique> cliniques = await _context.Cliniques.ToListAsync();
				    List<double> distances = new List<double>();
				    List<TimeSpan?> heuresProchainesPlagesHorairesDisponibles = new List<TimeSpan?>();
				    List<ResultatsCliniquesVM> resultats = new List<ResultatsCliniquesVM>();

				    cliniques = _cliniqueService.TrierCliniques(userCourant, cliniques);
				    distances = _azureMapsService.ObtenirDistances(userCourant, cliniques);
				    heuresProchainesPlagesHorairesDisponibles = _cliniqueService.ObtenirListeHeuresProchainesPlagesHorairesDisponible(cliniques);
                    
                    if (cliniques.Count != 0)
				    {
					    for (int i = 0; i < cliniques.Count; i++)
                        {
						    resultats.Add(new ResultatsCliniquesVM
						    {
							    Clinique = cliniques[i],
							    Distance = distances[i],
							    HeureProchainePlagesHoraireDisponible = heuresProchainesPlagesHorairesDisponibles[i]
						    });
                        }
				    }

				    vm.Resultats = resultats;
				    vm.LatitudeUtilisateur = userCourant.Latitude.ToString()?.Replace(',', '.');
				    vm.LongitudeUtilisateur = userCourant.Longitude.ToString()?.Replace(',', '.');

				    // TODO : Tester la recherche avec carte dans le cas où la recherche ne retourne aucun résultat (liste vide)
				    return View(vm);
                }
			}
            vm.Erreur = "Aucun utilisateur n'est connecté.";
            return View(vm);
		}

        // GET: Cliniques/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            if (id == null || _context.Cliniques == null)
            {
                return NotFound();
            }

            var clinique = await _cliniqueService.GetByIdAsync(id);

            CliniqueFileDAttenteVM vm = _cliniqueService.ObtenirCliniqueFileDAttente(clinique);

            if (clinique == null)
            {
                return NotFound();
            }

            return View(vm);
        }

        // GET: Cliniques/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliniques/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Clinique clinique)
        {
            clinique.Latitude = _azureMapsService.ObtenirCoordonnees(clinique.CodePostal).Latitude;
            clinique.Longitude = _azureMapsService.ObtenirCoordonnees(clinique.CodePostal).Longitude;

            if (ModelState.IsValid)
            {

                clinique.Telephone = SupprimerCaracteresNonNumeriques(clinique.Telephone);
                _context.Add(clinique);
                await _context.SaveChangesAsync();
                int lastClinicId = clinique.Id;
                TempData[Constants.Success] = $"Clinique {clinique.Nom} ajoutée";
                return RedirectToAction("Details", new { id = lastClinicId });
            }

            TempData[Constants.Error] = $"Clinique {clinique.Nom} n'ai pas été ajoutée.";
            return View(clinique);
        }

        private string SupprimerCaracteresNonNumeriques(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }

    }
}
