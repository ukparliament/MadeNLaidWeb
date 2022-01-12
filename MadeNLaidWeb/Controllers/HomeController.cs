using MadeNLaidWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Text;
using System.IO;

namespace MadeNLaidWeb.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Route("/")]
        [HttpGet]
        public IActionResult Index()
        {
            var statutoryInstruments = GetInstruments();
            return View(statutoryInstruments);
        }

        [Route("/Instrument/{id}")]
        [HttpGet]
        public IActionResult Instrument(string id)
        {
            StatutoryInstrument statutoryInstrument = GetInstrument(id);
            return View(statutoryInstrument);
        }

        [ResponseCache(Duration = 1200)]
        [HttpGet]
        [Route("/Rss")]
        public IActionResult Rss()
        {
            var feed = new SISyndicationFeed();

            feed.Copyright = new TextSyndicationContent("https://www.parliament.uk/site-information/copyright-parliament/open-parliament-licence/");
            feed.Title = new TextSyndicationContent("Made Statutory Instruments laid before the UK Parliament");
            feed.Description = new TextSyndicationContent("Updates whenever a negative or affirmative Statutory Instrument is laid before a House in the UK Parliament");
            feed.Language = "en-uk";

            XmlDocument doc = new XmlDocument();
            XmlElement feedElement = doc.CreateElement("link");
            feedElement.InnerText = "https://api.parliament.uk/made-n-laid";
            feed.ElementExtensions.Add(feedElement);

            XmlElement feedElement1 = doc.CreateElement("managingEditor");
            feedElement1.InnerText = "somervillea@parliament.uk (Anya Somerville)";
            feed.ElementExtensions.Add(feedElement1);

            XmlElement feedElement2 = doc.CreateElement("pubDate");
            feedElement2.InnerText = DateTime.Now.ToString();
            feed.ElementExtensions.Add(feedElement2);

            var items = new List<SyndicationItem>();
            var statutoryInstruments = GetInstruments();
            foreach (var si in statutoryInstruments.Where(x=>x.IsTweeted))
            {
                var item = new SyndicationItem();
                item.Title = new TextSyndicationContent(si.Name);
                item.Content = new TextSyndicationContent(si.Description);
                item.PublishDate = si.LaidDate;
                XmlElement feedElement3 = doc.CreateElement("guid");
                feedElement3.InnerText = si.Url;
                item.ElementExtensions.Add(feedElement3);

                XmlElement feedElement4 = doc.CreateElement("link");
                feedElement4.InnerText = si.Url;
                item.ElementExtensions.Add(feedElement4);
                items.Add(item);
            }

            feed.Items = items;
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };
            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, settings))
                {
                    var rssFormatter = new Rss20FeedFormatter(feed, false);
                    rssFormatter.WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
            }
        }

        StatutoryInstrument GetInstrument(string id1)
        {
            var id = "https://id.parliament.uk/" + id1;
            string connectionString = _configuration["MadeNLaidSqlServer"];

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using (SqlCommand cmd = new SqlCommand("Read from database", connection))
            {
                String sql = $@"SELECT 
	                                  [StatutoryInstrumentName]
                                      ,[ProcedureName]
                                      ,[LayingBodyName]
                                      ,[LaidDate]
                                      ,[MadeDate]
                                      ,[StatutoryInstrumentUri]
                                      ,[WorkPackageUri]
                                      ,[TnaUri]
                                      ,[IsTweeted]
                                FROM [dbo].[MadeNLaidStatutoryInstrument]
                                WHERE [StatutoryInstrumentUri] = '{id}'";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var si = new StatutoryInstrument();
                            si.Name = reader.GetString(0);
                            si.ProcedureName = reader.GetString(1);
                            si.LayingBodyName = reader.GetString(2);
                            si.LaidDate = reader.GetDateTimeOffset(3);
                            si.MadeDate = reader.GetDateTimeOffset(4);
                            si.Id = reader.GetString(5);
                            si.WorkPackageId = reader.GetString(6);
                            si.Link = reader.GetString(7);
                            si.IsTweeted = reader.GetBoolean(8);
                            connection.Close();
                            return si;
                        }
                    }
                }
            }

            connection.Close();
            return null;
        }

        IEnumerable<StatutoryInstrument> GetInstruments()
        {
            string connectionString = _configuration["MadeNLaidSqlServer"];

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            List<StatutoryInstrument> statutoryInstruments = new List<StatutoryInstrument>();
            using (SqlCommand cmd = new SqlCommand("Read from database", connection))
            {
                String sql = @"SELECT 
	                                  [StatutoryInstrumentName]
                                      ,[ProcedureName]
                                      ,[LayingBodyName]
                                      ,[LaidDate]
                                      ,[MadeDate]
                                      ,[StatutoryInstrumentUri]
                                      ,[WorkPackageUri]
                                      ,[TnaUri]
                                      ,[IsTweeted]
                                FROM [dbo].[MadeNLaidStatutoryInstrument]";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var si = new StatutoryInstrument();
                            si.Name = reader.GetString(0);
                            si.ProcedureName = reader.GetString(1);
                            si.LayingBodyName = reader.GetString(2);
                            si.LaidDate = reader.GetDateTimeOffset(3);
                            si.MadeDate = reader.GetDateTimeOffset(4);
                            si.Id = reader.GetString(5);
                            si.WorkPackageId = reader.GetString(6);
                            si.Link = reader.GetString(7);
                            si.IsTweeted = reader.GetBoolean(8);
                            statutoryInstruments.Add(si);
                        }
                    }
                }
            }

            connection.Close();
            return statutoryInstruments.OrderByDescending(x => x.LaidDate);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
