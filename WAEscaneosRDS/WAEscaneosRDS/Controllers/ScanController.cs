using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WAEscaneosRDS.Models;

namespace WAEscaneosRDS.Controllers
{
    public class ScanController : Controller
    {
        private db_a3f19c_administracionEntities db = new db_a3f19c_administracionEntities();

        // GET: Scan
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> LoginScanner(string codigoUsuario)
        {
            var usuario = await db.Usuarios
                .FirstOrDefaultAsync(x => x.CodigoUsuario == codigoUsuario);

            if (usuario == null)
                return Json(new { ok = false });

            Session["UsuarioId"] = usuario.Id;
            Session["UsuarioNombre"] = usuario.Nombre;

            return Json(new { ok = true, id = usuario.Id, nombre = usuario.Nombre });
        }

        [HttpPost]
        public async Task<JsonResult> CerrarTag(int tagId)
        {
            var tag = await db.Tags.FindAsync(tagId);

            if (tag == null)
                return Json(new { ok = false });

            tag.TagStates_Id = 2; // estado cerrado

            await db.SaveChangesAsync();

            return Json(new { ok = true });
        }

        [HttpPost]
        public async Task<JsonResult> ValidarTrailer(string nombre)
        {
            var trailer = await db.Trailers
                .FirstOrDefaultAsync(x => x.Nombre == nombre);

            if (trailer == null)
                return Json(new { existe = false });

            return Json(new { existe = true, id = trailer.Id });
        }

        [HttpPost]
        public async Task<JsonResult> CrearTrailer(string nombre)
        {
            var trailer = new Trailers
            {
                Nombre = nombre
            };

            db.Trailers.Add(trailer);

            await db.SaveChangesAsync();

            return Json(new { ok = true, id = trailer.Id });
        }

        [HttpPost]
        public async Task<JsonResult> CrearTag(string tagNumber, int trailerId)
        {
            bool existe = await db.Tags
                .AnyAsync(x => x.TagNumber == tagNumber);

            if (existe)
                return Json(new { ok = false });

            var tag = new Tags
            {
                TagNumber = tagNumber,
                Trailers_Id = trailerId,
                FechaCreacion = DateTime.Now,
                TagStates_Id = 1
            };

            db.Tags.Add(tag);

            await db.SaveChangesAsync();

            return Json(new { ok = true, tagId = tag.Id });
        }

        [HttpPost]
        public async Task<JsonResult> BuscarTag(string tagNumber)
        {
            var tag = await db.Tags
                .Include(t => t.Lpns)
                .FirstOrDefaultAsync(t => t.TagNumber == tagNumber);

            if (tag == null)
                return Json(new { existe = false });

            var lpns = tag.Lpns
                .OrderByDescending(l => l.FechaEscaneo)
                .Select(l => l.CodigoLpn)
                .ToList();

            return Json(new
            {
                existe = true,
                tagId = tag.Id,
                lpns = lpns
            });
        }

        [HttpPost]
        public async Task<JsonResult> EscanearLpn(int tagId, string codigo)
        {
            // obtener usuario actual
            int usuarioId = (int)Session["UsuarioId"];

            bool duplicado = await db.Lpns
                .AnyAsync(x => x.CodigoLpn == codigo);

            if (duplicado)
                return Json(new { ok = false, mensaje = "LPN duplicado" });

            var lpn = new Lpns
            {
                Tags_Id = tagId,
                CodigoLpn = codigo,
                FechaEscaneo = DateTime.Now,
                Usuarios_Id = usuarioId
            };

            db.Lpns.Add(lpn);

            await db.SaveChangesAsync();

            return Json(new { ok = true, codigo = codigo });
        }

    }
}