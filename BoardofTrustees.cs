using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using OUC;

namespace OUC
{
		public static class BoardofTrustees
		{
			
			/* BEGIN method: BOTmembers */
			public static string BOTmembers()
			{
				string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-members.xml.asp";
				string _xpath = "root/data/item";
				string response = "";

				try
				{

					OuMediaRss rss = new OuMediaRss(_url, _xpath);

					response += "<div class=\"container-flex row-alt\">";
					//display items 
					foreach (var i in rss.items)
					{		
                   		 response += "<div class=\"col-sm-12\">" + "<p style=\"text-align:center;\" >" + "<strong>" + i.Element("memberprefix").Value + " " + i.Element("memberfirstname").Value + " " + i.Element("memberlastname").Value + "<br>" + "</strong >" + i.Element("memberemployer").Value + "<br>" + i.Element("memberaddress").Value + "<br>" + i.Element("membercity").Value + " " + i.Element("memberstate").Value + i.Element("memberzip").Value + "</p>" + "</div>";
					}
					response += "</div>";

				}
				catch (Exception e)
				{
					response = "Members list cannot be displayed at this time. Please try again later.";
					response = e.ToString();
				}

				return response;
			}
			/* END method: BOTmembers */
			
			
			/* BEGIN method: BOT NextMeet */
			public static string NextMeet()
			{
				DateTime today = DateTime.Now.AddDays(-1);
				DateTime upcomingDay = today.AddDays(6000);

				//string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-meetings-MOD-extractedmeetinglocation-agendafilelinks.xml.asp?wherefield=meetingdate1&whereoperator=gteq&wherevalue="+today+"&orderby=meetingdate1&orderdir=asc";
				string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-upcoming-MOD-meetings-sort-by-date-and-time.xml.asp";//New hardcoded due to sorting needed for date and time string to time convevert SQL
				string _xpath = "root/data/item";
				string response = "";

			try
            {

                OuMediaRss rss = new OuMediaRss(_url, _xpath);

                //display items 
                foreach (var i in rss.items)
                {
					DateTime DayofWeek = DateTime.Parse(i.Element("meetingdate1").Value);
					//response += DayofWeek.ToString("dddd MMMM dd yyyy");
					
					response += "<p style=\"text-align: center;\">" + "<span style=\"background-color: #ccffff;font-size:  1.2em;\">" + DayofWeek.ToString("dddd" + ", " + "MMMM dd" + ", " + "yyyy") + "</span>" + "</p>"; //Upcoming Date Highlighted
					
					response += "<hr style=\"height: 1px;\">";
					
					//Table Below
					response += "<div class=\"container-flex stackable-table-wrap\">" 
						+ "<table class=\"ou-stackable-columns-table\">" + "<thead>" + "<tbody>" 
						
						//TIME
							+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "Time" + "</b>" + "</td>" 
							+ "<td class=\"with-borders align-center\">" + "<span>" + DayofWeek.ToString("dddd" + ", " + "MMMM dd" + ", " + "yyyy") + ":" + "</span>" + "<br>" + "<span>" + i.Element("meetingtime1start").Value + " - " + i.Element("meetingtime1end").Value + "</span>" + "</td>"
							
							+ "</tr>"      
						//DETAILS
							+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "Details" + "</b>" + "</td>" 
							+ "<td class=\"with-borders align-center\">" + "<p style=\"white-space: pre-line;\">" + i.Element("meetinglocation").Value + "</p>" + "</td>"
							
							+ "</tr>"
						//Public Virtual Access //fgcuteam-mike added 
						//+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "Public Virtual Access" + "</b>" + "</td>" 
						//+ "<td class=\"with-borders align-center\">" + "<p style=\"white-space: pre-line;\"><a href=\"/boardcast\">FGCU BOT Board Cast</a></p>" + "</td>"
						//+ "</tr>"							
                        //AGENDA
							+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "Agenda" + "</b>" + "</td>" 
							+ "<td class=\"with-borders align-center\">" + "<span>" 
							//+ "<a href=" + "https://www.fgcu.edu/Trustees/AgendaFile/" + Uri.EscapeUriString(i.Element("agendadirectory").Value) + "/" + Uri.EscapeUriString(i.Element("agendafile").Value) + " target=\"_blank\">" + i.Element("agendafiledesc").Value + "</a>";
							
							+ i.Element("agendafilehtmllinks").Value;
					
							response += "<span class=\"asterick\">" 
							+ "* In order to view this file, you must have Adobe Acrobat Reader software which is free from " + "<a href=\"http://www.adobe.com/products/acrobat/readstep2.html\" target=\"_blank\">" + "Adobe Systems, Inc." + "</a>" 
							+ "</span>" + "</td>"
							+ "</tr>"    
						//TEMP REQUEST - Virtual Access - fgcuteam-mike
						+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "Virtual Access" + "</b>" + "</td>" 
						+ "<td class=\"with-borders align-center\">" + "<a href=\"https://www.fgcu.edu/about/leadership/fgcuboardoftrustees/boardcast.aspx\">" + "FGCU BOT Board Cast" + "</a>" + "</td>"
						+ "</tr>"
						//ADDITIONALINFO
							+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "For Additional Information" + "</b>" + "</td>" 
							+ "<td class=\"with-borders align-center\">" + "<span>" + "Call Florida Gulf Coast University: " + "<br>" + "239-590-1065" + "</span>" + "</td>"
							+ "</tr>"
						//VISITORPARKING
							+ "<tr class=\"with-borders\">" + "<td class=\"with-borders align-center\">" + "<b>" + "Visitor Parking" + "</b>" + "</td>" 
							+ "<td class=\"with-borders align-center\">" + "<span>" + "Meetings of the Florida Gulf Coast University Board of Trustees are open to the public. Persons attending meetings should stop at the Information Booth located on the campus entrance road to obtain a visitor parking permit. Parking permits are required of all vehicles on campus." + "</span>" + "</td>"
//							+ "<td class=\"with-borders align-center\">" + "<span>" + "N/A" + "</span>" + "</td>"								
							+ "</tr>"
						//Table Ending
						+ "</tbody>" + "</table>" + "</div>";
					
						break;
                }

            }
            catch (Exception e)
            {
                response = "There are no Upcoming Dates listed at this time.";
                response = e.ToString();
            }
			
            return response;
			}
			/* END method: BOT NextMeet */
			
      	        /* BEGIN method: Agenda */
        public static string Agenda()
        {
			string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-meetings-MOD-extractedmeetinglocation-agendafilelinks.xml.asp?wherevalue=meetingid&orderby=meetingdate1&orderdir=desc";
            string _xpath = "root/data/item";
            string response = "";
            
			try
            {
                OuMediaRss rss = new OuMediaRss(_url, _xpath, 30000);
				
				response += "<div class=\"container-flex stackable-table-wrap\">";
				
				response += "<table id=\"agendasAndMaterials\" class=\"ou-stackable-columns-table\" border=\"0\" width=\"100%\" cellspacing=\"4\" cellpadding=\"0\">";

				response += "<thead>" + "<tr>";
				response += "<th class=\"rows\" width=\"15%\">" + "Meeting Date" + "</th>";     
				response += "<th class=\"rows\">" + "Committee Name" + "</th>";
				response += "<th class=\"rows\">" + "File" + "</th>";
				response += "</tr>" + "</thread>";

				response += "<tbody>";
				
                //display items 
                foreach (var i in rss.items)
                {		
                    
					
					
					response += "<tr class=\"with-borders\">";
					
							//Meeting Date
							response += "<td class=\"with-borders align-center\">" + i.Element("meetingdate1").Value + "</td>";

							//Comittee Name
							response += "<td class=\"with-borders align-center\">" + i.Element("extractedmeetinglocation").Value + "</td>";
							
							//File

							response += "<td class=\"with-borders align-center\">";
							
							if(i.Element("agendafilehtmllinks").Value == ""){ 
								response += "No Agenda Items available at this time.";
							}
					
							response += i.Element("agendafilehtmllinks").Value;
								
							response += "</td>";
					
					response += "</td>";
                }
				response += "<tbody>";
				response += "</table>";
				response += "</div>";
            }
            catch (Exception e)
            {
                response = "Members list cannot be displayed at this time. Please try again later.";
                response = e.ToString();
            }
			
            return response;
        }
        /* END method: Agenda */
			
		/* BEGIN method: MeetingSchedule */
        public static string MeetingSchedule()
        {
			
            DateTime today = DateTime.Now;
			DateTime upcomingDay = today.AddDays(6000);

			// string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-meetings.xml.asp?wherefield=meetingdate1&whereoperator=gteq&wherevalue="+today+"&orderby=meetingdate1&orderdir=asc";
			string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-upcoming-meetings-sort-by-date-and-time.xml.asp";
            string _xpath = "root/data/item";
            string response = "";
            
			try
            {

                OuMediaRss rss = new OuMediaRss(_url, _xpath);
				
				response += "<div class=\"container-flex stackable-table-wrap\">";
				response += "<table class=\"ou-stackable-columns-table\" border=\"0\" width=\"100%\" cellspacing=\"0\" cellpadding=\"2\">";
				response += "<tbody>";


                //display items 
                foreach (var i in rss.items)
                {		
					DateTime DayofWeek = DateTime.Parse(i.Element("meetingdate1").Value);
					//Date & Time
                   // response += "<strong>" + i.Element("meetingdate1").Value + "</strong>" + "<br>" + DayofWeek.ToString("dddd" + ", " + "MMMM dd" + ", " + "yyyy") + ":" + "<br>" + i.Element("meetingtime1start").Value + " - " + i.Element("meetingtime1end").Value;
					//string dynamicOrHardCoded = (i.Element("meetingdate1").Value == "10/29/2020") ? "TBA" : i.Element("meetingtime1start").Value + " - " + i.Element("meetingtime1end").Value;
					string dynamicOrHardCoded = i.Element("meetingtime1start").Value + " - " + i.Element("meetingtime1end").Value;
					
					response += "<tr class=\"with-borders\">"
                               + "<td class=\"with-borders align-center\">" + "<strong>" + "Date &amp; Time" + "</strong>" + "</td>"
								+ "<td class=\"with-borders align-center\">"
								+ "<strong>" + i.Element("meetingdate1").Value + "</strong>" + "<br>" + DayofWeek.ToString("dddd" + ", " + "MMMM dd" + ", " + "yyyy") + ":" + "<br>" + dynamicOrHardCoded
								+ "</td>"
								+ "</tr>";
                                 
					
					//Location
						response += "<tr class=\"with-borders\">"
                               + "<td class=\"with-borders align-center\">" + "<strong>" + "Details" + "</strong>" + "</td>"
								+ "<td class=\"with-borders align-center\">"
								+ "<p style=\"white-space: pre-line;\">" + i.Element("meetinglocation").Value + "</p>"
								+ "</td>"
								+ "</tr>";
                }
				
				response += "</table>";
				response += "</tbody>";
				response += "</div>";

            }
            catch (Exception e)
            {
                response = "Members list cannot be displayed at this time. Please try again later.";
                response = e.ToString();
            }
			
            return response;
        }
        /* END method: MeetingSchedule */
			
		     /* BEGIN method: Minutes */
        public static string Minutes()
        {

			string _url = "https://www2.fgcu.edu/datafeeds/boardoftrustees-meetings-MOD-extracted-minutes-meetings.xml.asp?wherevalue=meetingid&orderby=meetingdate1&orderdir=desc";
            string _xpath = "root/data/item";
            string response = "";
			
			string committeeWord;
			string yearWord;
			string previousYearWord = "";
            
			try
            {
                OuMediaRss rss = new OuMediaRss(_url, _xpath, 6000);

				var list = new List<string>();
				
				response += "<div class=\"container-flex row-alt\">";

                //display items 
                foreach (var i in rss.items)
                {		
					committeeWord = i.Element("committeename").Value;
					
					if (!list.Contains(committeeWord))
						{
							if(committeeWord != ""){	
									list.Add(committeeWord);
									//response += "<p>" + "<strong>" + committeeWord + " was added - Approved!" + "</strong>" + "</p>";
							}	
						}
                }
				
				     foreach (var c in list)
					{
						 if(c == "FGCU Board of Trustees"){ response += "<div class=\"col-sm\">"; }
						 if(c == "Audit and Compliance Committee"){ response += "<div class=\"col-sm\">"; }
						 
						 
						response += "<p class=\"committeesection statement-text\">" + c + "</p>";
						 
						 response += "<div class=\"container-flex accordion-wrapper simple plain push\">";
						 
						  response += "<ul>";
						 
						 previousYearWord = "0000"; //Resets value- So if it's same year, but different committee, it won't break
						 
						 foreach (var i in rss.items)
						{		
							if ((i.Element("committeename").Value) == c){
								DateTime DayofWeek = DateTime.Parse(i.Element("meetingdate1").Value);
								yearWord = DayofWeek.ToString("yyyy");
								
									if (yearWord != previousYearWord)
										{
										
											response += "<li>";
											
											response += "<p class=\"accordion-title\">" + DayofWeek.ToString("yyyy") + "</p>";

											response += "<a class=\"toggle-accordion-list\" aria-expanded=\"false\" href=" + "#" + DayofWeek.ToString("yyyy") + ">" + "<span>" + "Toggle More Info" + "</span>" + "</a>";
										
											response += "<div id=\"" + DayofWeek.ToString("yyyy") + "\" class=\"more-info\" aria-hidden=\"true\" style=\"display: none;\">";  
										
											previousYearWord = yearWord;
										}
								
										if (yearWord == previousYearWord)
										{
											if (i.Element("minutes").Value != ""){
												response +=	"<p>" 
															+ "<a href=" + "https://www2.fgcu.edu/Trustees/Minutes/" + Uri.EscapeUriString(i.Element("minutesdirectory").Value) + "/" + Uri.EscapeUriString(i.Element("minutes").Value) + " target=\"_blank\">" + DayofWeek.ToString("MMMM dd" + ", " + "yyyy") + "</a>" 
															+ "</p>";
											}
											
											if (i.Element("minutes").Value == ""){
																	if (i.Element("minutesdesc").Value == ""){
																		response +=	"<p>" 
																					+ DayofWeek.ToString("MMMM dd" + ", " + "yyyy") 
																					+ "</p>";
																	}

													if (i.Element("minutesdesc").Value != ""){
																	response +=	"<p>" 
																	+ DayofWeek.ToString("MMMM dd" + ", " + "yyyy") 
																	+ i.Element("minutesdesc").Value 
																	+ "</p>";
													}
											}
										}
								}
						 }
						 				response += "</div>";	
							 		response += "</li>";
								response += "</ul>";
						 
						 if(c == "FGCU Board of Trustees"){ response += "</div>" + "</div>"; } //CHANGE THIS IF YOU WANT LEFT COLUMN TO BE LONGER, THIS IS WHERE COLUMN 1 ENDS
					}
				 response += "</div>";	
            }
			
            catch (Exception e)
            {
                response = "Minutes list cannot be displayed at this time. Please try again later.";
                response = e.ToString();
            }
			
			response += "</div>";
			response += "</div>";
			response += "</div>";
			
            return response;
        }
        /* END method: Minutes */
			
		}
}