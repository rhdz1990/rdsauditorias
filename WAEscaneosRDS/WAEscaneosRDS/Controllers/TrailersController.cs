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
            int draw;
            int start;
            int length;

            if (!int.TryParse(Request["draw"], out draw))
                draw = 1;

            if (!int.TryParse(Request["start"], out start) || start < 0)
                start = 0;

            if (!int.TryParse(Request["length"], out length) || length <= 0)
                length = 10;

            string searchValue = Request["search[value]"];

            var query = db.Tags
                .Where(tag => tag.Trailers_Id == trailerId)
                .SelectMany(tag => tag.Lpns.Select(lpn => new
                {
                    LPN = lpn.CodigoLpn,
                    Tag = tag.TagNumber,
                    FechaEscaneo = lpn.FechaEscaneo
                }));

            int recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(x =>
                    x.LPN.Contains(searchValue) ||
                    x.Tag.Contains(searchValue));
            }

            int recordsFiltered = query.Count();

            var page = query
                .OrderByDescending(x => x.FechaEscaneo)
                .Skip(start)
                .Take(length)
                .ToList()
                .Select(l => new
                {
                    LPN = l.LPN,
                    Tag = l.Tag,
                    FechaEscaneo = l.FechaEscaneo.HasValue
                        ? l.FechaEscaneo.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null
                })
                .ToList();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = page
            }, JsonRequestBehavior.AllowGet);

        }
    }
}
