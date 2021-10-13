using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using Rocky_Utility.BrainTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<ApplicationUser> _repository;
        private readonly IInquiryHeaderRepository _inquiryHeaderRepository;
        private readonly IInquiryDetailRepository _inquiryDetailRepository;
        private readonly IEmailSender _emailSender;
        private readonly IBrainTreeGate _brainTreeGate;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IProductRepository productRepository, IRepository<ApplicationUser> repository,
            IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IInquiryHeaderRepository inquiryHeaderRepository,
            IInquiryDetailRepository inquiryDetailRepository, IBrainTreeGate brainTreeGate)
        {
            _productRepository = productRepository;
            _repository = repository;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _inquiryHeaderRepository = inquiryHeaderRepository;
            _inquiryDetailRepository = inquiryDetailRepository;
            _brainTreeGate = brainTreeGate;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new();
            if (HttpContext != null && HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart) != null && HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart).Count > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart);
            }

            List<int> productInCart = shoppingCartList.Select(p => p.ProductId).ToList();
            IEnumerable<Product> productList = _productRepository.GetAll().Where(p => productInCart.Contains(p.Id)).ToList();
            IList<Product> productListFinal = new List<Product>();

            foreach (var product in shoppingCartList)
            {
                Product productTemp = productList.FirstOrDefault(p => p.Id == product.ProductId);
                productTemp.TempSqFt = product.SqFt;
                productListFinal.Add(productTemp);
            }
            return View(productList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost()
        {
            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //var userId = User.FindFirst(ClaimTypes.NameIdentifier);

            List<ShoppingCart> shoppingCartList = new();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart) != null
                && HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart).Count > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart);
            }

            List<int> productInCart = shoppingCartList.Select(p => p.ProductId).ToList();
            IEnumerable<Product> productList = _productRepository.GetAll().Where(p => productInCart.Contains(p.Id)).ToList();


            //credit card
            var gateway = _brainTreeGate.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _repository.GetAlll).FirstOrDefault(ap => ap.Id == claim.Value),
                ProductList = productList.ToList()
            };
            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection keyValuePairs, ProductUserVM ProductUserVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //var pathToTemplate = "F:\\Rocky\\Rocky\\wwwroot\\templates\\Inquiry.html";

            //var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString() + "templates" +
            //    Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

            var pathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar
                                                                 + "templates" + Path.DirectorySeparatorChar +
                                                                 "Inquiry.html";

            const string subject = "New Inquiry";
            var HtmlBody = "";
            using (StreamReader sr = System.IO.File.OpenText(pathToTemplate))
            {
                HtmlBody = sr.ReadToEnd();
            }
            //Name: { 0}
            //Email: { 1}
            //Phone: { 2}
            //Products: {3}

            StringBuilder productListSB = new();
            foreach (var prod in ProductUserVM.ProductList)
            {
                productListSB.Append($" - Name: { prod.Name} <span style='font-size:14px;'> (ID: {prod.Id})</span><br />");
            }

            string messageBody = string.Format(HtmlBody,
                ProductUserVM.ApplicationUser.FullName,
                ProductUserVM.ApplicationUser.Email,
                ProductUserVM.ApplicationUser.PhoneNumber,
                productListSB.ToString());


            await _emailSender.SendEmailAsync(WebConstants.EmailAdmin, subject, messageBody);

            InquiryHeader inquiryHeader = new()
            {
                ApplicationUserId = claim.Value,
                FullName = ProductUserVM.ApplicationUser.FullName,
                Email = ProductUserVM.ApplicationUser.Email,
                PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                InquiryDate = System.DateTime.Now,
            };
            _inquiryHeaderRepository.Add(inquiryHeader);
            _inquiryHeaderRepository.SaveChanges();

            foreach (var prod in ProductUserVM.ProductList)
            {
                InquiryDetail inquiryDetail = new InquiryDetail
                {
                    InquiryHeaderId = inquiryHeader.Id,
                    ProductId = prod.Id,
                };
                _inquiryDetailRepository.Add(inquiryDetail);
            }
            _inquiryDetailRepository.SaveChanges();

            string nonceFromTheClient = keyValuePairs["payment_method_nonce"];

            var orderTotal = 0.0;
            foreach (Product prod in ProductUserVM.ProductList)
            {
                orderTotal += prod.Price * prod.TempSqFt;
            }
            var request = new TransactionRequest
            {
                Amount = Convert.ToDecimal(orderTotal),
                PaymentMethodNonce = nonceFromTheClient,
                OrderId = ProductUserVM.ApplicationUser.Id,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };
            var gateway = _brainTreeGate.GetGateway();
            Result<Transaction> result = gateway.Transaction.Sale(request);
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(ProductUserVM ProductUserVM)
        {
            HttpContext.Session.Clear();
            return View();
        }

        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart) != null
                && HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart).Count > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WebConstants.SessionCart);
            }
            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(p => p.ProductId == id));
            HttpContext.Session.Set(WebConstants.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Index), new List<ShoppingCart>());
        }

        [HttpPost]
        public IActionResult UpdateCart(IEnumerable<Product> products)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (var prod in products)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WebConstants.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }
    }
}
