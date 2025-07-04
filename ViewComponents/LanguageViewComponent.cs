using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MovieTicketBooking.ViewComponents
{
    public class LanguageViewComponent : ViewComponent
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LanguageViewComponent(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }

    public class SharedResource
    {
        // This class is used to locate the resource file
    }
} 