﻿using aspnet02_boardapp.Data;
using aspnet03_portfolioWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace aspnet03_portfolioWebApp.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _db;

        // 파일 업로드용 웹환경을 위한 객체
        private readonly IWebHostEnvironment _environment;

        public PortfolioController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var list = _db.Portfolios.ToList();  // SELECT *
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {   // PortfolioModel(X) -> TempPortfolioModel 선택(O)
            return View();
        }

        [HttpPost]
        public IActionResult Create(TempPortfolioModel temp)
        {
            // 파일 업로드, Temp -> Model db저장
            if (ModelState.IsValid)
            {
                // 파일 업로드되면 새로운 이미지 파일명을 받음
                string upFileName = UploadImageFile(temp);

                // TempPortfolioModel -> PortfolioModel 변경
                var portfolio = new PortfolioModel()
                {
                    Division = temp.Division,
                    Title = temp.Title,
                    Description = temp.Description,
                    Url = temp.Url,
                    FileName = upFileName  // 핵심!
                };

                _db.Portfolios.Add(portfolio);
                _db.SaveChanges();

                TempData["succeed"] = "포트폴리오 저장완료!";
                return RedirectToAction("Index", "Portfolio");                 
            }
            return View();
        }

        // Routing 이나 GET/POST랑 관계없음.
        private string UploadImageFile(TempPortfolioModel temp)
        {
            var resultFileName = "";

            try
            {
                if (temp.PortfolioImage != null)
                {
                    string uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");    // wwwroot밑에 upload라는 폴더 존재
                    resultFileName = Guid.NewGuid() + "_" + temp.PortfolioImage.FileName;  // 중복된 이미지 파일명 제거
                    string filePath = Path.Combine(uploadFolder, resultFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        temp.PortfolioImage.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }

            return resultFileName;
        }
    }
}
