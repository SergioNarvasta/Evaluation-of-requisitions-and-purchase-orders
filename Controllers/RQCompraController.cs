﻿
using HDProjectWeb.Models;
using HDProjectWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HDProjectWeb.Controllers
{
    [Authorize]
    public class RQCompraController :Controller
    {
        private readonly IRepositorioRQCompra repositorioRQCompra;
        private readonly IServicioEstandar servicioEstandar;
        private readonly IServicioUsuario servicioUsuario;

        public RQCompraController(IRepositorioRQCompra repositorioRQCompra,IServicioEstandar servicioEstandar, IServicioUsuario servicioUsuario) 
        {
            this.repositorioRQCompra = repositorioRQCompra;
            this.servicioEstandar     = servicioEstandar;
            this.servicioUsuario = servicioUsuario;
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            RQCompra crear = new();
            var periodo = servicioEstandar.ObtenerPeriodo();
            var date = DateTime.Now;
            ViewBag.periodo = periodo;
            string coduser = servicioUsuario.ObtenerCodUsuario();
            crear.S10_codepk = await servicioUsuario.ObtenerEpkUsuario(coduser);
            crear.S10_nomusu = await servicioUsuario.ObtenerNombreUsuario(coduser);
            crear.Rco_fec_registro = date;
            //crear.S10_usuario = codaux;
            //crear.S10_nomusu = servicioUsuario.ObtenerNombreUsuario(codaux);
            ViewBag.estado = "1";
           
            return View(crear);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(RQCompra rQCompra)
        {
            /*if(!ModelState.IsValid)
            {
                return View(rQCompra); 
            }     */
            rQCompra.Cia_codcia = servicioEstandar.Compañia();
            rQCompra.Suc_codsuc = servicioEstandar.Sucursal();
            rQCompra.Tin_codtin = servicioEstandar.TipoInventario();
            rQCompra.Rco_codusu = servicioUsuario.ObtenerCodUsuario();
            rQCompra.Rco_codepk = await servicioEstandar.GeneraRco_Codepk();
            await repositorioRQCompra.Crear(rQCompra);

           /*oreach(DetalleReq detalleReq in rQCompra.ListaDetalles )
            {
               //await servicioDetalle.Crear(DetalleReq);
            }*/
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string periodo,string busqueda,string estado,string orden) 
        {
            if(periodo is not null)
            {
               await servicioEstandar.ActualizaPeriodo(periodo);
            }
            if (orden is not null)
            {
               await servicioEstandar.ActualizaOrden(orden);
            }
            orden =await servicioEstandar.ObtenerOrden();
            periodo = await servicioEstandar.ObtenerPeriodo();
            ViewBag.periodo = periodo.Remove(4, 2) + "-" + periodo.Remove(0, 4);
            PaginacionViewModel paginacionViewModel = new();
            string codUser = servicioUsuario.ObtenerCodUsuario();
            int epkUser = await servicioUsuario.ObtenerEpkUsuario(codUser);
            string estado1, estado2;
            if (estado == "2")
            {
                estado1 = "1"; estado2 = "0";
            }
            else
            {
                estado1 = estado; estado2 = estado;
            }
            if (busqueda is not null)
            {               
                var bus_rQCompra = await repositorioRQCompra.BusquedaMultiple(periodo, paginacionViewModel, epkUser, busqueda, estado1,estado2);
                var bus_totalRegistros = await repositorioRQCompra.ContarRegistrosBusqueda(periodo, epkUser, busqueda, estado1, estado2);
                var respuesta = new PaginacionRespuesta<RQCompraCab>
                {
                    Elementos = bus_rQCompra,
                    Pagina = paginacionViewModel.Pagina,
                    RecordsporPagina = paginacionViewModel.RecordsPorPagina,
                    CantidadRegistros = bus_totalRegistros,
                    BaseURL = Url.Action()
                };
                return View(respuesta);
            }
            else
            {
                var rQCompra = await repositorioRQCompra.Obtener(periodo, paginacionViewModel, epkUser, orden, estado1, estado2);
                var totalRegistros = await repositorioRQCompra.ContarRegistros(periodo, epkUser, estado1, estado2);
                var respuesta = new PaginacionRespuesta<RQCompraCab>
                {
                    Elementos = rQCompra,
                    Pagina = paginacionViewModel.Pagina,
                    RecordsporPagina = paginacionViewModel.RecordsPorPagina,
                    CantidadRegistros = totalRegistros,
                    BaseURL = Url.Action()
                };
                return View(respuesta);
            }                        
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(PaginacionViewModel paginacionViewModel)
        {
            string estado1, estado2,estado="2";
            if (estado == "2")
            {
                estado1 = "1"; estado2 = "0";
            }
            else
            {
                estado1 = estado; estado2 = estado;
            }
            string coduser = servicioUsuario.ObtenerCodUsuario();
            string orden = await servicioEstandar.ObtenerOrden();
            int epkUser = await servicioUsuario.ObtenerEpkUsuario(coduser);
            string periodo = await servicioEstandar.ObtenerPeriodo();
            ViewBag.periodo =  periodo.Remove(4,2)+"-"+periodo.Remove(0,4);         
            var rQCompra   = await repositorioRQCompra.Obtener(periodo,paginacionViewModel,epkUser, orden,  estado1, estado2);
            var totalRegistros = await repositorioRQCompra.ContarRegistros(periodo, epkUser, estado1, estado2);
            if (totalRegistros==0)
            {
                ViewBag.registros = "0";
            }
            var respuesta = new PaginacionRespuesta<RQCompraCab>
            {
                Elementos = rQCompra,
                Pagina = paginacionViewModel.Pagina,
                RecordsporPagina = paginacionViewModel.RecordsPorPagina,
                CantidadRegistros = totalRegistros,
                BaseURL = Url.Action()
            };
            return View(respuesta);
        }
      
        [HttpGet]
        public async Task<IActionResult> Editar(string Rco_Numero)
        {
            var rQCompra = await repositorioRQCompra.ObtenerporCodigo(Rco_Numero);
            if(rQCompra is null)
            {
                return RedirectToAction("NoEncontrado","Home");   
            }
            var periodo = servicioEstandar.ObtenerPeriodo();
            ViewBag.periodo = periodo;
            ViewBag.Rco_numero = Rco_Numero;
            return View(rQCompra);
        }

        [HttpPost]
        public  async Task<IActionResult> Editar(RQCompra rQCompraEd)
        {
           //var usuarioid=servicioEstandar.ObtenerPeriodo(); //ObtenerUsuarioId
           await repositorioRQCompra.Actualizar(rQCompraEd);
           return RedirectToAction("Index");
        }
        [Authorize]
        [HttpGet]
        public async Task <IActionResult> Evaluacion(int epk)
        {
            var ReqCompra = await repositorioRQCompra.ObtenerporEpk(epk);
            if (ReqCompra is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            string cia, suc;
            cia = servicioEstandar.Compañia();
            suc = servicioEstandar.Sucursal();
            ViewBag.cia = cia;
            ViewBag.suc = suc;
            ViewBag.usu = servicioUsuario.ObtenerCodUsuario();
            ViewBag.epk = ReqCompra.Rco_codepk;
            ViewBag.num = ReqCompra.Rco_numrco;
            ViewBag.url = Url.Action();
            return View(ReqCompra);
        }
        [HttpPost]
        public async Task<IActionResult> Aprobar(string cia, string suc, string epk, string usu,string num)
        {
            int cia_codcia, suc_codsuc, rco_codepk, uap_codepk;
            cia_codcia = int.Parse(cia);
            suc_codsuc = int.Parse(suc);
            rco_codepk = int.Parse(epk);
            uap_codepk = await servicioUsuario.ObtenerEpkUsuario(usu);
            var result = await repositorioRQCompra.AprobarReq(cia_codcia, suc_codsuc, rco_codepk, uap_codepk);
            string message = "se aprobo con exito la Requisicion de Compra ";
            if (result < 0)
            {
                message = "Ocurrio un error al intentar aprobar la Requisicion de Compra ";
            }
            ViewBag.message = message;
            ViewBag.usu = usu;
            ViewBag.num = num;
            ViewBag.result = result;
            return View("ResultAprob");
        }
        [HttpPost]
        public async Task<IActionResult> Rechazar(string cia, string suc, string epk, string usu, string num, string mot)
        {
            int cia_codcia, suc_codsuc, rco_codepk, uap_codepk;
            cia_codcia = int.Parse(cia);
            suc_codsuc = int.Parse(suc);
            rco_codepk = int.Parse(epk);
            uap_codepk = await servicioUsuario.ObtenerEpkUsuario(usu);
            var result = await repositorioRQCompra.RechazaReq(cia_codcia, suc_codsuc, rco_codepk, uap_codepk, mot);
            string message = "Se rechazo con exito la Requisicion de Compra ";
            if (result < 0)
            {
                message = "Ocurrio un error al intentar rechazar la Requisicion de Compra ";
            }
            ViewBag.message = message;
            ViewBag.usu = usu;
            ViewBag.num = num;
            ViewBag.result = result;
            return View("ResultAprob");
        }
        [HttpPost]
        public async Task<IActionResult> Devolver(string cia, string suc, string epk, string usu, string num)
        {
            int cia_codcia, suc_codsuc, rco_codepk, uap_codepk;
            cia_codcia = int.Parse(cia);
            suc_codsuc = int.Parse(suc);
            rco_codepk = int.Parse(epk);
            uap_codepk = await servicioUsuario.ObtenerEpkUsuario(usu);

            var result = await repositorioRQCompra.DevuelveReq(cia_codcia, suc_codsuc, rco_codepk, uap_codepk);
            string message = "Se devolvio con exito la Requisicion de Compra ";
            if (result < 0)
            {
                message = "Ocurrio un error: no se devolvio la Requisicion de Compra ";
            }
            ViewBag.message = message;
            ViewBag.usu = usu;
            ViewBag.num = num;
            ViewBag.result = result;
            return View("ResultAprob");
        }
    }
}
