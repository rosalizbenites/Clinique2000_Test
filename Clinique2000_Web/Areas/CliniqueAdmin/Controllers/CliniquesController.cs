using Clinique2000_DataAccess.Data;
using Clinique2000_Models.Models;
using Clinique2000_Services.Services;
using Clinique2000_Services.Services.IServices;
using Clinique2000_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Clinique2000_Web.Areas.CliniqueAdmin.Controllers
{
    [Area("CliniqueAdmin")]
    //[Authorize(Roles = Constants.CliniqueAdminRole)]
    public class CliniquesController : Controller
    {
        private readonly Clinique2000DbContext _context;
        private readonly IAzureMapsService _azureMapsService;
        private readonly ContexteService _contexteService;
        private readonly IUserService _userService;
        private readonly ICliniqueService _cliniqueService;

        public CliniquesController(Clinique2000DbContext context, IAzureMapsService azureMapsService, IUserService userService, ContexteService contexteService, ICliniqueService cliniqueService)
        {
            _context = context;
            _azureMapsService = azureMapsService;
            _userService = userService;
            _contexteService = contexteService;
            _cliniqueService = cliniqueService;
        }

        public async Task<IActionResult> GestionCliniques()
        {
            User user = await _userService.ObtenirUtilisateurCourantAsync();

            if (user != null)
            {
                var cliniques = await _context.Cliniques.Where(c => c.CliniqueAdminId == user.Id).ToListAsync();

                if (cliniques.Count == 0)
                {
                    ViewBag.Message = "Vous n'êtes administrateur d'aucune clinique";
                }

                return View(cliniques);
            }

            else
            {
                return Problem("Entity set 'Clinique2000DbContext.Cliniques' is null.");
            }
        }

        
        public async Task<IActionResult> GestionFiles(int id)
        { 
            Clinique? cliniqueCourante = await _cliniqueService.GetByIdAsync(id);
            _contexteService.CliniqueCouranteId = id;

            if (cliniqueCourante.FilesDAttente.Count == 0)
            {
                ViewBag.Message = "Aucune file d'attente n'existe pour la clinique sélectionné";
            }

            return View(cliniqueCourante);
        }

        
        // GET: Cliniques/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Cliniques == null)
            {
                return NotFound();
            }

            var clinique = await _context.Cliniques
                .FirstOrDefaultAsync(m => m.Id == id);
            if (clinique == null)
            {
                return NotFound();
            }

            return View(clinique);
        }

        // GET: Cliniques/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliniques/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            TempData[Constants.Error] = $"Clinique {clinique.Nom} n'a pas été ajoutée.";
            return View(clinique);
        }

        // GET: Cliniques/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Cliniques == null)
            {
                return NotFound();
            }

            var clinique = await _context.Cliniques.FindAsync(id);

            if (clinique == null)
            {
                return NotFound();
            }
            return View(clinique);
        }

        // POST: Cliniques/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Clinique clinique)
        {
            clinique.CliniqueAdminId = _userService.ObtenirUtilisateurCourantAsync().Result.Id;

            if (id != clinique.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clinique);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CliniqueExists(clinique.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("GestionCliniques", "Cliniques");
            }
            return View(clinique);
        }

        private bool CliniqueExists(int id)
        {
            return (_context.Cliniques?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private string SupprimerCaracteresNonNumeriques(string input)
        {
            return new string(input.Where(char.IsDigit).ToArray());
        }
    }
}
