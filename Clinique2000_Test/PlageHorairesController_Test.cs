using Clinique2000_Models.Models;
using Clinique2000_Services.Services.IServices;
using Clinique2000_Web.Areas.Patient.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinique2000_Test
{
    

    public class PlageHorairesController_Test
    {

        private Mock<ICliniqueService> _cliniqueService_Mock;
        private Mock<IPlageHoraireService> _plageHoraireService_Mock;
        private Mock<IUserService> _userService_Mock;
        private Mock<IFileDAttenteService> _fileDAttenteService_Mock;
        private PlageHorairesController _plageHorairesController;

        public PlageHorairesController_Test()
        {
            _cliniqueService_Mock = new Mock<ICliniqueService>();
            _plageHoraireService_Mock = new Mock<IPlageHoraireService>();
            _userService_Mock = new Mock<IUserService>();
            _fileDAttenteService_Mock = new Mock<IFileDAttenteService>();
            _plageHorairesController = new PlageHorairesController(_plageHoraireService_Mock.Object,_fileDAttenteService_Mock.Object,_userService_Mock.Object, _cliniqueService_Mock.Object);
        }

        /// <summary>
        /// Test plageHorairesController.Index(Id) quand il n'ya pas des file d'attente pour la clinique id=5 
        /// </summary>
        [Fact]
        public async void Index_GetAllByFileDAttenteId()
        {
            var result = await _plageHorairesController.Index(5);

            _plageHoraireService_Mock.Verify(x => x.GetAllByFileDAttenteId(5), Times.Never);
             
            Assert.IsAssignableFrom<ViewResult>(result);

            ViewResult viewResult = result as ViewResult;
            Assert.Equal("Index", viewResult.ViewName);
        }
        /// <summary>
        ///
        /// </summary>
        [Fact]
        public async void Service_ObtenirFileDAttenteInscriptionsActives()
        {
            var clinique_One = new Clinique()
            {
                Id = 12,
                Nom = "CliniqueTEST"
            };

            var file_One = new FileDAttente()
            {
                Id = 1,
                Clinique = clinique_One,
            };

            _cliniqueService_Mock.Setup(x => x.ObtenirFileDAttenteInscriptionsActives(clinique_One)).Returns(new FileDAttente());

            var fileCourante = _plageHorairesController.Index(clinique_One.Id);

            Assert.Equal(1, fileCourante.Id);
        }

    }
}
