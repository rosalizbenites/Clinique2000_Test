using Clinique2000_DataAccess.Data;
using Clinique2000_Models.Models;
using Clinique2000_Models.ViewModels;
using Clinique2000_Services.Services;
using Clinique2000_Services.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Clinique2000_Web.Areas.Salle.Controllers
{
    [Area("Salle")]
    [AllowAnonymous]
    public class SalleDAttentesController : Controller
    {
        private readonly Clinique2000DbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ICliniqueService _cliniqueService;

        public SalleDAttentesController(Clinique2000DbContext context, UserManager<User> userManager, ICliniqueService cliniqueService)
        {
            _context = context;
            _userManager = userManager;
            _cliniqueService = cliniqueService;
        }

        // GET: Patients
        public async Task<IActionResult> ChoisirClinique()
        {
            List<Clinique> cliniques = await _context.Cliniques.Include(f => f.FilesDAttente).ToListAsync();
            cliniques = _cliniqueService.ObtenirListeCliniquesOuvertes(cliniques);
            return View(cliniques);
        }

        // GET: SalleDattente/SalleAttente/5
        public async Task<IActionResult> SalleAttente(int id)
        {
            if (id == null)
            {
                return NotFound();
            }


            //FileDAttente? fileDAttente = await _context
            //                                .FilesDAttente
            //                                    .Include(f => f.PlagesHoraires.Where(ph => ph.EstReservee == true))
            //                                        .FirstOrDefaultAsync(f => f.Id == id);

            Clinique clinique = await _cliniqueService.GetByIdAsync(id);

            FileDAttente? fileDAttente = _cliniqueService.ObtenirFileDAttenteInscriptionsActives(clinique);

            var SalleDattente = fileDAttente.PlagesHoraires.Where(ph => ph.EstReservee == true)
                .Select(ph => new PatientViewModel(ph))
                .OrderBy(ph => ph.HeurePlageHoraire)
                .ThenBy(ph => ph.Prenom)
                .ThenBy(ph => ph.NomAbbrege).ToList();

            if (SalleDattente.Count == 0)
            {
                ViewBag.Message = "Aucun patient n'a pris de rendez-vous";
            }

            return View(SalleDattente);
        }
    }
}
