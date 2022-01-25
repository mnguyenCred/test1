using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.Rating;
using Factories;
namespace Services
{
    public class RatingServices
    {
        public static string thisClassName = "RatingServices";
        public static List<AppEntity> GetAll()
        {
            var allRatings = RatingManager.GetAll().OrderByDescending( m => m.CodedNotation.ToLower() == "all" ).ToList();

            return allRatings; ;
        }
        public static List<AppEntity> GetAllActiveRatings()
        {
            var allRatings = RatingManager.GetAll( true ).OrderByDescending( m => m.CodedNotation.ToLower() == "all" ).ToList();

            return allRatings; ;
        }
    }
}
