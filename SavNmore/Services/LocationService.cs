using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using savnmore.Models;

namespace savnmore.Services
{
    public class LocationService
    {
        public const string Zipcode = "zip";
        public const string DefaultZipcode = "08879";
        /// <summary>
        /// returns the last entered zip or the default
        /// </summary>
        /// <returns></returns>
        public static string GetUserZip()
        {
            if (HttpContext.Current.Session[Zipcode] == null)
            {
                HttpContext.Current.Session[Zipcode] = DefaultZipcode ;
            }
            return HttpContext.Current.Session[Zipcode] as string;
        }
        /// <summary>
        /// saves the new zipcode
        /// </summary>
        /// <param name="newZip"></param>
        public static  void SaveZip(string newZip)
        {
            if(!string.IsNullOrEmpty(newZip))
            {
            HttpContext.Current.Session[Zipcode] = newZip;
            }
        }
        public static List<SelectListItem> GetMiles()
        {
            List<SelectListItem> miles = new List<SelectListItem>
                                             {
                                                 new SelectListItem
                                                     {
                                                         Value = "5",
                                                         Text = "5 Miles"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = "10",
                                                         Text = "10 Miles"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = "15",
                                                         Text = "15 Miles"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = "20",
                                                         Text = "20 Miles"
                                                     },
                                                 new SelectListItem
                                                     {
                                                         Value = "25",
                                                         Text = "25 Miles"
                                                     }
                                             };

            return miles;
        }

        public const double MilesToDegree = 69.17;
        private readonly savnmoreEntities _db = new savnmoreEntities();
        public MaxDistance FindZipCodes(MaxDistance fromZip)
        {
            fromZip.ZipCodes = new List<ZipCodeEntry>();
            //Get the Lat/Long of the zipcode

            var latlong = GetLatLong(fromZip.ZipCode);
            fromZip.Latitude = latlong.Latitude;
            fromZip.Longitude = latlong.Longitude;
            //Determine the maxes (69.17 is the # of miles/degree)
            fromZip.MaxLatitude = fromZip.Latitude + (fromZip.Miles / MilesToDegree);
            fromZip.MinLatitude = fromZip.Latitude- (fromZip.MaxLatitude - fromZip.Latitude);
            fromZip.MaxLongitude = fromZip.Longitude +
                                   fromZip.Miles/(Math.Cos(fromZip.MinLatitude*Math.PI/180)*MilesToDegree);
            fromZip.MinLongitude = fromZip.Longitude - (fromZip.MaxLongitude - fromZip.Longitude);
            //now find the zip codes with these values

            var closezips = from z in _db.ZipCodes
                            where z.Longitude >= fromZip.MinLongitude && z.Longitude <= fromZip.MaxLongitude
                            && z.Latitude >= fromZip.MinLatitude && z.Latitude <= fromZip.MaxLatitude
                                select z;

            fromZip.ZipCodes.AddRange(closezips.ToList());
              
            return fromZip;
        }
        public ZipCodeEntry GetLatLong(string zip)
        {
            try
            {
                return _db.ZipCodes.Single(t => t.ZipCode == zip);
            }
            catch 
            {
                return new ZipCodeEntry();
            }
        }
        public List<int> FindStores(List<string> fromZips)
        {
            var storeIds = from s in _db.Stores where fromZips.Contains(s.Address.Zip) select s.Id;
            return storeIds.ToList();
        }
        public List<Store> GetAllStores(List<string> fromZips)
        {
            var stores = from s in _db.Stores where fromZips.Contains(s.Address.Zip) select s;
            return stores.ToList();
        }
        public List<Store> FindStores(MaxDistance maxDistance)
        {
            if(maxDistance.ZipCodes == null || !maxDistance.ZipCodes.Any())
            maxDistance = FindZipCodes(maxDistance);
            var zips = maxDistance.ZipCodes.Select(t => t.ZipCode).ToList();
            var stores = GetAllStores(zips);
            return stores;
        }
        /// <summary>
        /// finds all stores for the specified max distance zip code and miles
        /// </summary>
        /// <param name="fromZip"></param>
        /// <returns></returns>
        public MaxDistance GetAllStores(MaxDistance fromZip)
        {
            fromZip = FindZipCodes(fromZip);
            fromZip.CloseStores = GetAllStores(fromZip.ZipCodes.Select(t => t.ZipCode).ToList());
            return fromZip;
        }
    }
    /*CREATE FUNCTION dbo.RadiusFunc
2
3(
4	@ZipCode nchar(5),
5	@Miles decimal(18, 9)
6)
7
8RETURNS
9	@MaxLongLats TABLE
10	(
11		Latitude decimal(10,8),
12		Longitude decimal(11,8),
13		MaxLatitude decimal(10,8),
14		MinLatitude decimal(10,8),
15		MaxLongitude decimal(11,8),
16		MinLongitude decimal(11,8)
17	)
18AS
19
20BEGIN
21	--Declare variables
22	DECLARE @Latitude decimal(10,8), @Longitude decimal(11,8)
23	DECLARE @MaxLatitude decimal(10, 8), @MinLatitude decimal(10, 8)
24	DECLARE @MaxLongitude decimal(11, 8), @MinLongitude decimal(11, 8)
25
26	--Get the Lat/Long of the zipcode
27	SELECT @Latitude = Latitude, @Longitude = Longitude
28	FROM dbo.ZipCodes
29	WHERE ZipCode = @ZipCode
30
31	--Zipcode not found?
32	IF @@ROWCOUNT = 0
33		RETURN 
34
35	--Determine the maxes (69.17 is the # of miles/degree)
36	SET @MaxLatitude = @Latitude + @Miles / 69.17
37	SET @MinLatitude = @Latitude - (@MaxLatitude - @Latitude)
38	SET @MaxLongitude = @Longitude + @Miles / (COS(@MinLatitude * PI() / 180) * 69.17)
39	SET @MinLongitude = @Longitude - (@MaxLongitude - @Longitude)
40
41	--Insert data into return table
42	INSERT INTO @MaxLongLats
43		(Latitude, Longitude, MaxLatitude, MinLatitude, MaxLongitude, MinLongitude)
44	SELECT @Latitude, @Longitude, @MaxLatitude, @MinLatitude, @MaxLongitude, @MinLongitude
45	RETURN
46END
     */
}