// 
//  Main.cs
//  
//  Copyright (c) 2011 Washington State Department of Transportation
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace WSFRouteSchedules
{
	public class FerriesRoute
	{
		public int RouteID {get;set;}
		public string Description {get;set;}
		public List<Dates> Date = new List<Dates>();

		public override string ToString ()
		{
			return string.Format("[FerriesRoute: RouteID={0}, Description={1}, Date={2}]",
			                     RouteID, Description, Date);
		}
	}

	public class Dates
	{
		public DateTime Date {get;set;}
		public List<FerriesTerminal> Sailings = new List<FerriesTerminal>();
		
		public override string ToString ()
		{
			 return string.Format("[Dates: Date={0}, Sailings={1}]", Date, Sailings);
		}
	}
	
	public class FerriesTerminal
	{
		public int DepartingTerminalID {get;set;}
		public string DepartingTerminalName {get;set;}
		public int ArrivingTerminalID {get;set;}
		public string ArrivingTerminalName {get;set;}
		public List<string> Annotations = new List<string>();
		public List<FerriesScheduleTimes> Times = new List<FerriesScheduleTimes>();
	}
	
	public class FerriesScheduleTimes
	{
		public DateTime DepartingTime {get;set;}
		public List<int> AnnotationIndexes = new List<int>();		
	}	
	
	public class CacheFlushDate
	{
		public DateTime CacheDate {get;set;}
		
		public override string ToString ()
		{
			return string.Format("[CacheFlushDate: FlushDate={0}]", CacheDate);
		}
	}	
	
	class MainClass
	{		
		public static void Main (string[] args)
		{
			List<FerriesRoute> _items = new List<FerriesRoute>();
			List<CacheFlushDate> _cacheFlushDate = new List<CacheFlushDate>();
			var serializer = new JsonSerializer();
			string cachedDateTime = "";

			// The Traveler Information Application Programming Interface is designed to provide third parties
			// with a single gateway to all of WSDOT's traveler information data. To use the WSDL services you
			// must be assigned an Access Code. You can obtain an Access Code by simply providing your email
			// address at the Traveler Information API site, http://www.wsdot.wa.gov/traffic/api/

			string API_ACCESS_CODE = "INSERT_YOUR_API_ACCESS_CODE_HERE";
			
			b2b.wsdot.wa.gov.WSFSchedule obj = new b2b.wsdot.wa.gov.WSFSchedule();

			try
			{
				using (var re = File.OpenText(@"WSFCacheFlushDateSchedules.js"))
				using (var reader = new JsonTextReader(re))
				{
					var entries = serializer.Deserialize<CacheFlushDate[]>(reader);
					cachedDateTime = entries[0].CacheDate.ToString();
				}
			}
			catch
			{
				Console.WriteLine("Error opening WSFCacheFlushDateSchedules.js file");
			}
			
			// Most web methods in this service are cached. Implementing caching in your program is recommended
			// in order to not query the web service when not necessary. GetCacheFlushDate() will tell you the
			// date and time that the cache was last flushed.
			System.DateTime dateTime = (System.DateTime) obj.GetCacheFlushDate();
			_cacheFlushDate.Add(new CacheFlushDate() {
				CacheDate = dateTime
			});
			
			// Write the most recent cache flush date and time to a JSON file.
			string jsonCacheDate = JsonConvert.SerializeObject(_cacheFlushDate);
			File.WriteAllText(@"WSFCacheFlushDateSchedules.js", jsonCacheDate);			
			
			// Compare the cache dates. If dates are equal then we already have the most current data.
			if (!cachedDateTime.Equals(dateTime.ToString()))
			{	
				// All method calls require the APIAccessHeader to pass a valid APIAccessCode.
				b2b.wsdot.wa.gov.APIAccessHeader apiAccessHeader = new b2b.wsdot.wa.gov.APIAccessHeader();
				apiAccessHeader.APIAccessCode = API_ACCESS_CODE;
				obj.APIAccessHeaderValue = apiAccessHeader;			
				b2b.wsdot.wa.gov.TripDateMsg tripDateMsg = new b2b.wsdot.wa.gov.TripDateMsg();
				b2b.wsdot.wa.gov.RouteMsg routeMsg = new b2b.wsdot.wa.gov.RouteMsg();
				
				tripDateMsg.TripDate = DateTime.Today;

				// Provide all available routes for a particular date.
				b2b.wsdot.wa.gov.RouteBriefResponse[] routes = obj.GetAllRoutes(tripDateMsg);
				
				for (int i=0; i<routes.Length; i++)
				{
					_items.Add(new FerriesRoute() {
						RouteID = routes[i].RouteID,
						Description = routes[i].Description
					});
					
					DateTime today = DateTime.Today;
					
					for (int j=0; j<7; j++)
					{
						_items[i].Date.Add(new Dates() {
							Date = today
						});
						
						routeMsg.RouteID = routes[i].RouteID;
						routeMsg.TripDate = today;
						
						// Retrieve sailing times associated with a specific route for a particular date.
						b2b.wsdot.wa.gov.SchedResponse scheduleByRouteResults = obj.GetScheduleByRoute(routeMsg);
						
						for (int k=0; k<scheduleByRouteResults.TerminalCombos.Length; k++)
						{
							_items[i].Date[j].Sailings.Add(new FerriesTerminal() {
								DepartingTerminalID = scheduleByRouteResults.TerminalCombos[k].DepartingTerminalID,
								DepartingTerminalName = scheduleByRouteResults.TerminalCombos[k].DepartingTerminalName,
								ArrivingTerminalID = scheduleByRouteResults.TerminalCombos[k].ArrivingTerminalID,
								ArrivingTerminalName = scheduleByRouteResults.TerminalCombos[k].ArrivingTerminalName
							});
							
							b2b.wsdot.wa.gov.TerminalComboMsg terminalComboMsg = new b2b.wsdot.wa.gov.TerminalComboMsg();
							terminalComboMsg.TripDate = today;
							terminalComboMsg.DepartingTerminalID = scheduleByRouteResults.TerminalCombos[k].DepartingTerminalID;
							terminalComboMsg.ArrivingTerminalID = scheduleByRouteResults.TerminalCombos[k].ArrivingTerminalID;
							
							// Retrieve sailing times associated with a specific departing / arriving terminal combination for a particular date.
							b2b.wsdot.wa.gov.SchedResponse scheduleByTerminalComboResults = obj.GetScheduleByTerminalCombo(terminalComboMsg);
							
							for (int l=0; l<scheduleByTerminalComboResults.TerminalCombos.Length; l++)
							{
								for (int m=0; m<scheduleByTerminalComboResults.TerminalCombos[l].Annotations.Length; m++)
								{
									_items[i].Date[j].Sailings[k].Annotations.Add(scheduleByTerminalComboResults.TerminalCombos[l].Annotations[m]);
								}
								
								for (int n=0; n<scheduleByTerminalComboResults.TerminalCombos[l].Times.Length; n++)
								{
									_items[i].Date[j].Sailings[k].Times.Add(new FerriesScheduleTimes() {
										DepartingTime = scheduleByTerminalComboResults.TerminalCombos[l].Times[n].DepartingTime
									});							
	
									for (int o=0; o<scheduleByTerminalComboResults.TerminalCombos[l].Times[n].AnnotationIndexes.Length; o++)
									{
										_items[i].Date[j].Sailings[k].Times[n].AnnotationIndexes.Add(scheduleByTerminalComboResults.TerminalCombos[l].Times[n].AnnotationIndexes[o]);
									}
								}
							}
						}
						
						today = today.AddDays(1);
					}
					
				}
				
				// Serialize the data object to a JSON file.
				string json = JsonConvert.SerializeObject(_items);
				File.WriteAllText(@"WSFRouteSchedules.js", json);
				
				FileStream sourceFile = File.OpenRead(@"WSFRouteSchedules.js");
	        	FileStream destFile = File.Create(@"WSFRouteSchedules.js.gz");
				
				// Now compress the JSON file so it is smaller to download.
				// Uncompressed the file is about 459 KB, compressed it drops to 25 KB. Nice.
				GZipStream compStream = new GZipStream(destFile, CompressionMode.Compress);
				
		        try
		        {
		            int theByte = sourceFile.ReadByte();
		            while (theByte != -1)
		            {
		                compStream.WriteByte((byte)theByte);
		                theByte = sourceFile.ReadByte();
		            }
		        }
		        finally
		        {
		            compStream.Dispose();
		        }
			}
		}
	}
}
