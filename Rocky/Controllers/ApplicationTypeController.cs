using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize(Roles = WebConstants.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        private readonly IApplicationTypeRepository _ApplicationTypeRepository;

        public ApplicationTypeController(IApplicationTypeRepository ApplicationTypeRepository)
        {
            _ApplicationTypeRepository = ApplicationTypeRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<ApplicationType> objList = _ApplicationTypeRepository.GetAll();
            return View(objList);
        }

        public IActionResult AddOrUpdate(int? id)
        {
            if (id == null || id == 0)
            {
                return View();
            }
            var applicationType = _ApplicationTypeRepository.Find(id.GetValueOrDefault());
            if (applicationType == null)
            {
                return NotFound();
            }
            return View(applicationType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(ApplicationType applicationType)
        {
            if (ModelState.IsValid)
            {
                if (applicationType.Id == 0)
                {
                    _ApplicationTypeRepository.Add(applicationType);
                }
                else
                {
                    _ApplicationTypeRepository.Update(applicationType);
                }
                _ApplicationTypeRepository.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(applicationType);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var applicationType = _ApplicationTypeRepository.Find(id.GetValueOrDefault());
            if (applicationType == null)
            {
                return NotFound();
            }
            return View(applicationType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSave(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var applicationType = _ApplicationTypeRepository.Find(id.GetValueOrDefault());
            if (applicationType == null)
            {
                return NotFound();
            }
            _ApplicationTypeRepository.Remove(applicationType);
            _ApplicationTypeRepository.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
