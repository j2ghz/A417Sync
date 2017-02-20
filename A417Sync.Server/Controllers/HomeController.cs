namespace A417Sync.Server.Controllers
{
    using A417Sync.Server.Services;
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        public HomeController(RepoProvider repoProvider)
        {
            this.repoProvider = repoProvider;
        }
        private readonly RepoProvider repoProvider;
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Modpacks()
        {
            return View(repoProvider.Load());
        }
    }
}