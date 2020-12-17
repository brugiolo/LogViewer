using LogViewer.Site.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer.Site.Controllers
{
    public class RequestLogController : Controller
    {
        private readonly string _defaultExtension = ".TXT";
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public RequestLogController(IConfiguration configuration)
        {
            _configuration = configuration;
            _baseUrl = configuration.GetValue<string>("MySettings:BaseUrl");
            _httpClient = new HttpClient();
        }

        public async Task<ActionResult> Index(string text)
        {
            var logs = new List<RequestLogViewModel>();
            var response = await _httpClient.GetAsync(_baseUrl + "list/?text=" + text);
            if (response.IsSuccessStatusCode)
            {
                logs = await response.Content.ReadAsAsync<List<RequestLogViewModel>>();
            }

            return View(logs ?? new List<RequestLogViewModel>());
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + id);
            if (response.IsSuccessStatusCode)
            {
                var log = await response.Content.ReadAsAsync<RequestLogViewModel>();
                return View(log);
            }

            return View();
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Import(IFormFile iFormFile)
        {
            var fileExtension = Path.GetExtension(iFormFile.FileName).ToUpper();
            if (fileExtension != _defaultExtension)
            {
                ViewBag.IsSuccess = false;
                ViewBag.Message = "Incorrect file extension. Check the file and try again.";
                return View();
            }

            byte[] data;
            using (var br = new BinaryReader(iFormFile.OpenReadStream()))
            {
                data = br.ReadBytes((int)iFormFile.OpenReadStream().Length);
            }

            var bytes = new ByteArrayContent(data);
            var multiContent = new MultipartFormDataContent();
            multiContent.Add(bytes, "file", iFormFile.FileName);

            var result = _httpClient.PostAsync(_baseUrl + "InsertFromFile", multiContent).Result;

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
                return RedirectToAction("Index", "RequestLog");
            else
                return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RequestLogViewModel requestLogViewModel)
        {
            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(requestLogViewModel), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_baseUrl, httpContent);
                
                return View(nameof(Index));
            }
            catch
            {
                return View(nameof(Index));
            }
        }

        public async Task<ActionResult> Edit(Guid id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + id);
            if (response.IsSuccessStatusCode)
            {
                var log = await response.Content.ReadAsAsync<RequestLogViewModel>();
                return View(log);
            }

            return View(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, RequestLogViewModel requestLogViewModel)
        {
            try
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(requestLogViewModel), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(_baseUrl + id, httpContent);
                if (response.IsSuccessStatusCode)
                {
                    var log = await response.Content.ReadAsAsync<RequestLogViewModel>();
                    return View(log);
                }

                return RedirectToAction(nameof(Details));
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Delete(Guid id)
        {
            var response = await _httpClient.GetAsync(_baseUrl + id);
            if (response.IsSuccessStatusCode)
            {
                var log = await response.Content.ReadAsAsync<RequestLogViewModel>();
                return View(log);
            }

            return View(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(Guid id, RequestLogViewModel requestLogViewModel)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(_baseUrl + id.ToString());

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(nameof(Index));
            }
        }
    }
}
