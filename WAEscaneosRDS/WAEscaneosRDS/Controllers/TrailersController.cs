using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WAEscaneosRDS.Models;

namespace WAEscaneosRDS.Controllers
{
    public class TrailersController : Controller
    {
        db_a3f19c_administracionEntities db = new db_a3f19c_administracionEntities();
        
        // GET: Trailers
        public ActionResult Index()
        {
            return View();
        }

        // GET: Trailers/ObtenerTrailers
        public JsonResult ObtenerTrailers()
        {
            var trailers = db.Trailers
                .Select(t => new {
                    t.Id,
                    t.Nombre,                    
                })
                .OrderByDescending(t => t.Id)
                .ToList();

            return Json(new { data = trailers }, JsonRequestBehavior.AllowGet);
        }

        // Vista detalle LPNs por trailer
        public ActionResult DetalleTrailer(int trailerId)
        {
            ViewBag.TrailerId = trailerId;
            return View();
        }

        // JSON LPNs por trailer
        public JsonResult ObtenerLPNsPorTrailer(int trailerId)
        {
            // 1️⃣ Traemos los datos a memoria
            var lpnsEnMemoria = db.Tags
                .Where(tag => tag.Trailers_Id == trailerId)
                .SelectMany(tag => tag.Lpns.Select(lpn => new {
                    LPN = lpn.CodigoLpn,
                    Tag = tag.TagNumber,
                    FechaEscaneo = lpn.FechaEscaneo
                }))
                .ToList(); // <-- aquí EF trae los datos, ahora podemos usar ToString

            // 2️⃣ Convertimos las fechas a ISO
            var lpnsISO = lpnsEnMemoria
                .Select(l => new {
                    LPN = l.LPN,
                    Tag = l.Tag,
                    FechaEscaneo = l.FechaEscaneo.HasValue
                                    ? l.FechaEscaneo.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                                    : null
                })
                .OrderByDescending(l => l.FechaEscaneo) // Orden descendente
                .ToList();

            // 3️⃣ Devolvemos formato compatible con DataTables
            return Json(new { data = lpnsISO }, JsonRequestBehavior.AllowGet);

        }
    }
}