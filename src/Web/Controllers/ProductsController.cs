﻿using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Depot.Web.Extensions;
using Depot.Web.Filters;
using Depot.Web.Helpers;
using Depot.Web.Models;
using Raven.Client;

namespace Depot.Web.Controllers
{
    [RavenDocumentSession]
    public class ProductsController: AutoMapperController
    {
        private readonly IDocumentSession _session;

        public ProductsController(IDocumentSession session)
        {
            _session = session;
        }

        public ActionResult Index()
        {
            var products = _session.Query<Product>().ToArray();
            return AutoMapView<ProductListModel[]>(products, View());
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult New(ProductEditModel productEditModel)
        {
            if (!ModelState.IsValid)
            {
                return View(productEditModel);
            }

            var product = Mapper.Map<ProductEditModel, Product>(productEditModel);
            _session.Store(product);

            return RedirectToAction("Index").WithFlash(FlashMessageType.Notice, "Product was successfully created.");
        }

        public ActionResult Edit(int id)
        {
            var product = _session.Load<Product>(id);
            return AutoMapView<ProductEditModel>(product, View());
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(ProductEditModel productEditModel)
        {
            if (!ModelState.IsValid)
            {
                return AutoMapView<ProductEditModel>(productEditModel, View());
            }

            var product = _session.Load<Product>(productEditModel.Id);
            product.Description = productEditModel.Description;
            product.ImageUrl = productEditModel.ImageUrl;
            product.Price = productEditModel.Price;
            product.Title = productEditModel.Title;

            return RedirectToAction("Show", new {id = productEditModel.Id}).WithFlash(FlashMessageType.Notice, "Product was successfully updated.");
        }

        public ActionResult Show(int id)
        {
            var product = _session.Load<Product>(id);
            return AutoMapView<ProductShowModel>(product, View());
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var product = _session.Load<Product>(id);
            _session.Delete(product);
            return RedirectToAction("Index").WithFlash(FlashMessageType.Notice, "Product was successfully deleted.");
        }

        public ActionResult ValidateUniqueTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var isUniqueTitle = !_session.Query<Product>().Any(p => p.Title == title.Trim());
                return Json(isUniqueTitle, JsonRequestBehavior.AllowGet);
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}