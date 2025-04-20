using System.Collections.Generic;
using Eventurely.Web.Models; // Assuming Event is defined here

namespace Eventurely.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Event> FeaturedEvents { get; set; } = new List<Event>();
    }
}
