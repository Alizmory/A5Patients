using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using A1Patients.Models;
using Microsoft.AspNetCore.Http;

namespace A1Patients.Controllers
{
    public class PatientTreatmentController : Controller
    {
        private readonly PatientsContext _context;

        public PatientTreatmentController(PatientsContext context)
        {
            _context = context;
        }

        // GET: PatientTreatment
        public async Task<IActionResult> Index(string patientDiagnosisId)//LC
        {
            if (!string.IsNullOrEmpty(patientDiagnosisId))
            {
                Response.Cookies.Append("patientDiagnosisId", patientDiagnosisId);
                HttpContext.Session.SetString("patientDiagnosisId", patientDiagnosisId);
            }
            else if (Request.Query["patientDiagnosisId"].Any())
            {
                patientDiagnosisId = Request.Query["patientDiagnosisId"].ToString();
                Response.Cookies.Append("patientDiagnosisId", patientDiagnosisId);
                HttpContext.Session.SetString("patientDiagnosisId", patientDiagnosisId);
            }
            else if (Request.Cookies["patientDiagnosisId"] != null)
            {
                patientDiagnosisId = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                patientDiagnosisId = HttpContext.Session.GetString("patientDiagnosisId");
            }
            else
            {
                TempData["message"] = "Please select Patient Diagnosis";
                return RedirectToAction("Index", "PatientDiagnosis");
            }

            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(patientDiagnosisId)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();

            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            var patientsContext = _context.PatientTreatment.Include(p => p.PatientDiagnosis).Include(p => p.Treatment)
                                    .Where(d => d.PatientDiagnosisId == Convert.ToInt32(patientDiagnosisId))
                                    .OrderByDescending(t => t.DatePrescribed);

            return View(await patientsContext.ToListAsync());
        }

        // GET: PatientTreatment/Details/5
        public async Task<IActionResult> Details(int? id)//LC
        {
            string diagnosedID = string.Empty;

            if (Request.Cookies["patientDiagnosisId"] != null)
            {
                diagnosedID = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                diagnosedID = HttpContext.Session.GetString("patientDiagnosisId");
            }

            // Get the patient Full Name
            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(diagnosedID)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();

            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            if (id == null)
            {
                return NotFound();
            }
            var patientTreatment = await _context.PatientTreatment
                .Include(p => p.PatientDiagnosis)
                .Include(p => p.Treatment)
                .FirstOrDefaultAsync(m => m.PatientTreatmentId == id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            return View(patientTreatment);
        }

        // GET: PatientTreatment/Create
        public IActionResult Create()//LC
        {
            string diagnosedID = string.Empty;

            if (Request.Cookies["patientDiagnosisId"] != null)
            {
                diagnosedID = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                diagnosedID = HttpContext.Session.GetString("patientDiagnosisId");
            }
            // Get the patient Full Name
            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(diagnosedID)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();
            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId");
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(t => t.DiagnosisId == diagnosis.DiagnosisId), "TreatmentId", "Name");

            // date and time
            ViewBag.DatePrescribed = DateTime.Now.ToString("dd MMMM yyyy HH:mm");

            return View();
        }

        // POST: PatientTreatment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PatientTreatmentId,TreatmentId,DatePrescribed,Comments,PatientDiagnosisId")] PatientTreatment patientTreatment)
        {
            string diagnosedID = string.Empty;

            if (Request.Cookies["patientDiagnosisId"] != null)
            {
                diagnosedID = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                diagnosedID = HttpContext.Session.GetString("patientDiagnosisId");
            }

            // Get the patient Full Name
            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(diagnosedID)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();

            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            if (ModelState.IsValid)
            {
                _context.Add(patientTreatment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);//LC
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(t => t.DiagnosisId == diagnosis.DiagnosisId), "TreatmentId", "Name");
            
            ViewBag.DatePrescribed = DateTime.Now.ToString("dd MMMM yyyy HH:mm");
            ViewBag.PatientDiagnosisId = Convert.ToInt32(diagnosedID);

            return View(patientTreatment);
        }

        // GET: PatientTreatment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            string diagnosedID = string.Empty;//LC

            if (Request.Cookies["patientDiagnosisId"] != null)
            {
                diagnosedID = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                diagnosedID = HttpContext.Session.GetString("patientDiagnosisId");
            }

            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(diagnosedID)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();

            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            if (id == null)
            {
                return NotFound();
            }

            var patientTreatment = await _context.PatientTreatment.FindAsync(id);
            if (patientTreatment == null)
            {
                return NotFound();
            }
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(t => t.DiagnosisId == diagnosis.DiagnosisId), "TreatmentId", "Name", patientTreatment.TreatmentId);

            ViewBag.DatePrescribed = patientTreatment.DatePrescribed.ToString(String.Format("dd MMMM yyyy HH:mm"));

            return View(patientTreatment);
        }

        // POST: PatientTreatment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientTreatmentId,TreatmentId,DatePrescribed,Comments,PatientDiagnosisId")] PatientTreatment patientTreatment)
        {
            string diagnosedID = string.Empty;

            if (Request.Cookies["patientDiagnosisId"] != null)
            {
                diagnosedID = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                diagnosedID = HttpContext.Session.GetString("patientDiagnosisId");
            }

            // Get the patient Full Name
            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(diagnosedID)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            // Get the diagnosis name
            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();

            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            if (id != patientTreatment.PatientTreatmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientTreatment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientTreatmentExists(patientTreatment.PatientTreatmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["PatientDiagnosisId"] = new SelectList(_context.PatientDiagnosis, "PatientDiagnosisId", "PatientDiagnosisId", patientTreatment.PatientDiagnosisId);
            ViewData["TreatmentId"] = new SelectList(_context.Treatment.Where(t => t.DiagnosisId == diagnosis.DiagnosisId), "TreatmentId", "Name", patientTreatment.TreatmentId);
            return View(patientTreatment);
        }

        // GET: GOPatientTreatment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            string diagnosedID = string.Empty;

            if (Request.Cookies["patientDiagnosisId"] != null)
            {
                diagnosedID = Request.Cookies["patientDiagnosisId"].ToString();
            }
            else if (HttpContext.Session.GetString("patientDiagnosisId") != null)
            {
                diagnosedID = HttpContext.Session.GetString("patientDiagnosisId");
            }

            // Get the patient Full Name
            var diagnosis = _context.PatientDiagnosis.Where(d => d.PatientDiagnosisId == Convert.ToInt32(diagnosedID)).FirstOrDefault();
            var patient = _context.Patient.Where(p => p.PatientId == diagnosis.PatientId).FirstOrDefault();

            // Get the diagnosis name
            var diagnosisName = _context.Diagnosis.Where(n => n.DiagnosisId == diagnosis.DiagnosisId).FirstOrDefault();

            ViewData["FullName"] = patient.LastName + ", " + patient.FirstName;
            ViewData["DiagnosisName"] = diagnosisName.Name;

            if (id == null)
            {
                return NotFound();
            }

            var patientTreatment = await _context.PatientTreatment
                .Include(p => p.PatientDiagnosis)
                .Include(p => p.Treatment)
                .FirstOrDefaultAsync(m => m.PatientTreatmentId == id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            return View(patientTreatment);
        }

        // POST: GOPatientTreatment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patientTreatment = await _context.PatientTreatment.FindAsync(id);
            _context.PatientTreatment.Remove(patientTreatment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientTreatmentExists(int id)
        {
            return _context.PatientTreatment.Any(e => e.PatientTreatmentId == id);
        }
    }
}