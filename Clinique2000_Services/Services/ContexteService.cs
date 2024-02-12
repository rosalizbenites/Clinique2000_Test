using Clinique2000_Models.Models;
using Clinique2000_Services.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinique2000_Services.Services
{
    // Service servant à faire persister certaines données en cours d'exécution (dans un singleton)
    public class ContexteService
    {
        public int CliniqueCouranteId { get; set; }
    }
}
