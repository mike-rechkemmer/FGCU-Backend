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

namespace OUC {

	public static class AcademicWeb {

		public static string getProgramsByCategory(string categoryid) {

			string strHTMLDecodedCategoryID = System.Web.HttpUtility.HtmlDecode(categoryid.Replace("&nbsp;", "+"));//For some reason when decoding whitespace (&nbsp;) the string was treated differently, so this.
			string strURLEncodedCategoryID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedCategoryID).Replace("%2b","+");//Notice this is not the FULL url so is ok to do the replace

			string _url = "https://www2.fgcu.edu/datafeeds/programcategories.xml.asp?category=" + strURLEncodedCategoryID;

			string _xpath = "//root/data/category/program";
			string response = "";


			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);


				//response += "<button class=\"printBtn programdataprintbutton\" onclick=\"javascript:window.print()\">" + "Print Details" + "</button>";
				response += "<div class=\"container-flex\">";

				//display items 
				foreach (var i in rss.items)
				{		
					response += "<h4>" + i.Element("programname").Value + "</h4>";

					response +=  "<p>";
					response += "<a href=\"" + i.Element("programurl").Value + "\">Program Details</a>";
					//if( i.Element("programurl").Value.Contains("aspx") )
					//	response += "<a href=\"" + i.Element("programurl").Value + "#CurriculumMap\">Curriculum Map</a> | ";
					//if( i.Element("programurl").Value.Contains("www2") )
					//	response += "<a href=\"" + i.Element("programurl").Value.Substring(0, i.Element("programurl").Value.LastIndexOf('/')) + "/curriculummap.html\">Curriculum Map</a> | ";
					response += " | <a href=\"" + i.Element("learningoutcome").Value + "\">Learning Outcomes</a>";
					
					/* 01 July 2019 - Joe Stanis
					 * Per Jeff, workaround to conditionally display the Interdisciplinary Entrepreneurship Studies (B.A.)
					 * link as "Visit School Site" and using alternate, school-specific URL
					 */
					if (i.Element("bannerid").Value == "BS-INTENT") {
						string strURLInterdisciplinaryEntrepreneurshipStudiesBA = "https://www.fgcu.edu/soe/";
						response += " | <a href=\"" + strURLInterdisciplinaryEntrepreneurshipStudiesBA + "\">Visit School Site</a>";
					}
					else {
						response += " | <a href=\"" + i.Element("collegeurl").Value + "\">Visit College Site</a>";
					}
					
					//if( i.Element("programurl").Value.Contains("aspx") )
					//	response += " | <a href=\"" + i.Element("programurl").Value + "#LearningOutcomes\">Learning Outcomes</a>";
					//else if( i.Element("programurl").Value.Contains("asp") )
					//	response += " | <a href=\"" + i.Element("programurl").Value.Substring(0, i.Element("programurl").Value.LastIndexOf('/')) + "/ALC.html\">Learning Outcomes</a>";

					response +=  "</p>";


				}
				response += "</div>";


			}
			catch (Exception e)
			{
				response = "<div>The list of programs is temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}

		/*
			 * BEGIN method: getMinorDegree 
			 */
		public static string getMinorDegree(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=admissionrequirements" +
				"&selectfield=DegreeRequirements" +
				"&selectfield=SemesterHours" +
				"&selectfield=AdditionalGradRequirements" +
				"&selectfield=TransferNotes" +
				"&selectfield=College" +
				"&selectfield=ProgramName" +
				"&selectfield=Concentration" +
				"&selectfield=Division" +
				"&selectfield=Degree" +
				"&selectfield=AcademicYear" +
				"&selectfield=ID" +
				"&selectfield=ProgressRewuirements" +
				"&selectfield=ProgressionAndAdditionalGraduationRequirements" +
				"&selectfield=FutureReqs" +
				"&selectfield=ProgramDescription" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue=" + strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getMinorDegree() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				// CSS override to switch Print Button from absolute positioning to float:
				/*response += "<style type=\"text/css\">" + 
					".main-content article button.printBtn.programdataprintbutton {" +
					"position: static !important;" +
					"float: right;" +
					"}" + 
					"</style>";*/

				//response += "<button class=\"printBtn programdataprintbutton\" onclick=\"javascript:window.print()\">" + "Print Details" + "</button>";
				response += "<section class=\"minorstext-section\">" + 
					"<div class=\"container\">";

				//display items 
				foreach (var i in rss.items)
				{		
					response += "<p>" + "<em>" + "<strong>" + i.Element("programdescription").Value + "</strong>" + "</em>" + "</p>";

					response += "<p class=\"statement-text\">" + "Admission Requirements" + "</p>";//General Requirements
					response += "<p>" + i.Element("admissionrequirements").Value + "</p>";

					if (i.Element("progressionandadditionalgraduationrequirements").Value != "") {
						response += "<p class=\"statement-text\">" + "Additional Graduation Requirements" + "</p>";
						response += "<p>" + i.Element("progressionandadditionalgraduationrequirements").Value + "</p>";
					}

					response += "<p class=\"statement-text\">" + "Program Requirements" + "</p>";//Specific Information
					response += "<p>" + i.Element("degreerequirements").Value + "</p>";
					response += "<p>" + i.Element("additionalgradrequirements").Value + "</p>";



					response += "<i>" + "Catalog Year: " + i.Element("academicyear").Value + "</i>";

					response += "<p>" + "<strong>" + "Total Semester Hours Required: " + i.Element("semesterhours").Value + " Hours" + "</strong>" + "</p>";

					if(i.Element("transfernotes").Value != ""){ 
						response += "<hr>" + "<p class=\"statement-text\">" + "Transfer Notes" + "</p>";
						response += "<p>" + i.Element("transfernotes").Value + "</p>";
					}


				}
				response += "</div>" + 
					"</section>";

			}
			catch (Exception e)
			{
				response = "<div>The requirements are temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}
		/*
			 * END method: getMinorDegree
			 */

		/*
			 * BEGIN method: getMajorDegree 
			 */
		public static string getMajorDegree(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=DegreeRequirements" +
				"&selectfield=SemesterHours" +
				"&selectfield=AdditionalGradRequirements" +
				"&selectfield=TransferNotes" +
				"&selectfield=College" +
				"&selectfield=ProgramName" +
				"&selectfield=Concentration" +
				"&selectfield=Division" +
				"&selectfield=Degree" +
				"&selectfield=AcademicYear" +
				"&selectfield=ID" +
				"&selectfield=ProgressRewuirements" +
				"&selectfield=ProgressionAndAdditionalGraduationRequirements" +
				"&selectfield=FutureReqs" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue="+strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getMajorDegree() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				response += "<section class=\"text-section\">" + 
					"<div class=\"container\">";

				//display items 
				foreach (var i in rss.items) {

					response += "<p class=\"statement-text\">" + "Program Requirements for the " + i.Element("academicyear").Value + " Catalog Year" + "</p>";

					response += "<p>" + 
						"The General Education Program Website is located at <a href=\"https://www.fgcu.edu/academics/undergraduatestudies/generaleducation/\" target=\"_blank\">https://www.fgcu.edu/academics/undergraduatestudies/generaleducation/</a>" + 
						"</p>";

					if ((i.Element("progressrewuirements").Value != "") || (i.Element("progressionandadditionalgraduationrequirements").Value != "")) {
						response += "<p class=\"statement-text\">" + "Program Progression and Additional Graduation Requirements" + "</p>";
						if (i.Element("progressrewuirements").Value != "") {
							response += "<p>" + i.Element("progressrewuirements").Value + "</p>";
						}
						if (i.Element("progressionandadditionalgraduationrequirements").Value != "") {
							response += "<p>" + i.Element("progressionandadditionalgraduationrequirements").Value + "</p>";
						}
					}

					response += "<p class=\"statement-text\">" + "Program Requirements" + "</p>";
					response += "<p>" + i.Element("degreerequirements").Value + "</p>";

					response += "<p>" + "<strong>" + "<span style=\"text-transform: uppercase\">Total Credits Required</span>: " + i.Element("semesterhours").Value + "</strong>" + "</p>";

					if (i.Element("futurereqs").Value != "") {
						response += "<p>" + i.Element("futurereqs").Value + "</p>";	
					}

					if (i.Element("additionalgradrequirements").Value != "") {
						response += "<p class=\"statement-text\">" + "Additional Graduation Requirements" + "</p>";
						response += "<p>" + i.Element("additionalgradrequirements").Value + "</p>";
					}

					if (i.Element("transfernotes").Value != "") {
						response += "<p class=\"statement-text\">" + "Transfer Notes and Acceptable Substitutes" + "</p>";
						response += "<p>" + i.Element("transfernotes").Value + "</p>";
					}

				}
				response += "</div>" + 
					"</section>";

			} catch (Exception e) {
				response = "<div>The program requirements for this major are temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}
		/*
			 * END method: getMajorDegree
			 */

		/*
			 * BEGIN method: getProgramConcentrations
			 */
		public static string getProgramConcentrations(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=Degree" +
				"&selectfield=ProgramName" +
				"&selectfield=Concentration" +
				"&selectfield=College" +
				"&selectfield=SemesterHours" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue="+strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getProgramConcentrations() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				// CSS override to switch Print Button from absolute positioning to float:
				/*response += "<style type=\"text/css\">" + 
					".main-content article button.printBtn.programdataprintbutton {" +
					"position: static !important;" +
					"float: right;" +
					"}" + 
					"</style>";*/

				response += "<section class=\"text-section\">" + 
					"<div class=\"container\">";

				//display items 
				foreach (var i in rss.items) {
					if (i.Element("concentration").Value != "") {

						response += "<p class=\"statement-text\">" + "Concentrations" + "</p>";

						int idx = 0;
						string[] arrSplitConcentrations = null;
						string strUnsplitConcentrations = i.Element("concentration").Value;
						char[] arrSeparators = {'|'};

						arrSplitConcentrations = strUnsplitConcentrations.Split(arrSeparators);

						if (arrSplitConcentrations.Length > 0) {
							response += "<ul>";
							for (idx = 0; idx < arrSplitConcentrations.Length; idx++) {
								response += "<li>" + arrSplitConcentrations[idx] + "</li>";
							}
							response += "</ul>";
						}



					}

				}
				response += "</div>" + 
					"</section>";

			} catch (Exception e) {
				response = "<div>The list of program concentrations is temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}
		/*
			 * END method: getProgramConcentrations
			 */

		/*
			 * BEGIN method: getProgramAdmissionRequirements
			 */
		public static string getProgramAdmissionRequirements(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=AdmissionRequirements" +
				"&selectfield=AcademicYear" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue="+strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getProgramAdmissionRequirements() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				response += "<section class=\"text-section\">" + 
					"<div class=\"container\">";

				//display items 
				foreach (var i in rss.items) {

					response += "<p class=\"statement-text\">" + "Admissions Information for the " + i.Element("academicyear").Value + " Catalog Year" + "</p>";

					response += "<p>" + "If you are not yet an FGCU student, visit the <a href=\"https://www2.fgcu.edu/admissions.asp\" target=\"_blank\">Admissions Office Website</a>" + "</p>";
					response += "<p>" + "If you are a newly-admitted FGCU student please be sure to <a href=\"http://studentservices.fgcu.edu/NewStudentPrograms/orientationregistration.html\" target=\"_blank\">sign up for an Eagle View Orientation session</a>. As part of this session you will be meeting with an academic advisor who will assist you in registering for classes for your major." + "</p>";

					if (i.Element("admissionrequirements").Value != "") {
						response += "<p class=\"statement-text\">" + "Program Admission Requirements" + "</p>";
						response += "<p>" + i.Element("admissionrequirements").Value + "</p>";
					}

					response += "<p class=\"statement-text\">" + "Admission Deadlines" + "</p>";
					response += "<p>" + "<a href=\"https://www2.fgcu.edu/admissions.asp\" target=\"_blank\">Visit the Admissions Office Web site</a> for information on admission deadlines of the university." + "</p>";

				}
				response += "</div>" + 
					"</section>";

			} catch (Exception e) {
				response = "<div>The information on program admission requirements is temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}
		/*
			 * END method: getProgramAdmissionRequirements
			 */


		/*
			 * BEGIN method: getGraduateProgramAccreditation
			 */
		public static string getGraduateProgramAccreditation(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=accredidations" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue=" + strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getGraduateProgramAccreditation() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				response += "<section class=\"text-section\">" + 
					"<div class=\"container\">";

				// display items 
				foreach (var i in rss.items)
				{
					/* if response is not empty */
					if (i.Element("accredidations").Value != "") {
						
						response += "<p class=\"statement-text\">" + "Institutional Accreditation" + "</p>";
						
						/* if response does not contain accredidation, add it, otherwise do nothing */
						if (!(i.Element("accredidations").Value.Contains("www.fgcu.edu/accreditation"))) {
							// fgcu-team mike moved out of the loop Institutional Accreditation (some programs where were no items so it wasn't showing at all) 
							response += "<p>" + 
							"Information on FGCU institutional accreditation is available at <a href=\"{{f:18143776}}\" target=\"blank\">www.fgcu.edu/accreditation</a>" + 
							"</p>";
						}
						response += "<p>" + i.Element("accredidations").Value + "</p>";
					}
					/* if response is empty */
					else {
						response += "<p class=\"statement-text\">" + "Institutional Accreditation" + "</p>";
						response += "<p>" + 
							"Information on FGCU institutional accreditation is available at <a href=\"{{f:18143776}}\" target=\"blank\">www.fgcu.edu/accreditation</a>" + 
							"</p>";
					}
				}
				response += "</div>" + 
					"</section>";
			}
			catch (Exception e)
			{
				response = "<div>The graduate accreditation data is temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}
		/*
			 * END method: getGraduateProgramAccreditation
			 */


		/*
			 * BEGIN method: getGraduateProgramAdmissionsInformation
			 */
		public static string getGraduateProgramAdmissionsInformation(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=admissionrequirements" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue=" + strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getGraduateProgramAdmissionsInformation() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				response += "<section class=\"text-section\">" + 
					"<div class=\"container\">";
					
				response += "<p class=\"statement-text\">" + "Getting Started" + "</p>";
				response += "<p>" + 
						"Applicants must submit an application online at <a href=\"https://apply.fgcu.edu\" target=\"_blank\">apply.fgcu.edu</a>. Please contact Graduate Admissions at 239-745-GRAD or <a href=\"mailto:graduate@fgcu.edu\">graduate@fgcu.edu</a> for additional information about the application process. Satisfaction of minimum University and program requirements does not guarantee admission to a graduate program. Applicants should check with the program for the most up to date admission requirements prior to commencing the application process." +
						//"Please visit the Office of Graduate Studies website <a href=\"http://www.fgcu.edu/graduate\" target=\"_blank\">www.fgcu.edu/graduate</a> or contact Graduate Studies at 239-590-7988 or <a href=\"mailto:graduate@fgcu.edu\">graduate@fgcu.edu</a> for an application for admission and additional information about the application process. Satisfaction of minimum University and program requirements does not guarantee admission to a graduate program. Applicants should check with the program for the most up to date admission requirements prior to commencing the application process." + 
						"</p>";
				
				//display items 
				foreach (var i in rss.items)
				{


					if (i.Element("admissionrequirements").Value != "") {
						response += "<p>" + i.Element("admissionrequirements").Value + "</p>";
					}
				}
				response += "</div>" + 
					"</section>";

			}
			catch (Exception e)
			{
				response = "<div>The graduate admissions information data is temporarily unavailable. Please try again later.</div>";
				
			}

			return response;
		}
		/*
			 * END method: getGraduateProgramAdmissionsInformation
			 */


		/*
			 * BEGIN method: getGraduateProgramRequirements 
			 */
		public static string getGraduateProgramRequirements(string bannerid) {

			string strHTMLDecodedBannerID = System.Web.HttpUtility.HtmlDecode(bannerid);
			string strURLEncodedBannerID = System.Web.HttpUtility.UrlEncode(strHTMLDecodedBannerID);

			string _url = "https://www2.fgcu.edu/datafeeds/catalogprogramrequirements.xml.asp?1=1" +
				"&trimjunkhtml=1" +
				"&stripinlinestyles=1" +
				"&selectfield=DegreeRequirements" +
				"&selectfield=SemesterHours" +
				"&selectfield=AdditionalGradRequirements" +
				"&selectfield=TransferNotes" +
				"&selectfield=College" +
				"&selectfield=ProgramName" +
				"&selectfield=Concentration" +
				"&selectfield=Division" +
				"&selectfield=Degree" +
				"&selectfield=AcademicYear" +
				"&selectfield=ID" +
				"&selectfield=ProgressRewuirements" +
				"&selectfield=ProgressionAndAdditionalGraduationRequirements" +
				"&selectfield=FutureReqs" +
				"&wherefield=BannerID&whereoperator=eq&wherevalue=" + strURLEncodedBannerID;

			string _xpath = "//root/data/item";
			string response = "";

			/* DEBUG
				response += "<!-- getGraduateProgramRequirements() Feed '"+_url+"' -->";
				//*/

			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				response += "<section class=\"text-section\">" + 
					"<div class=\"container\">";

				//display items 
				foreach (var i in rss.items) {

					response += "<p class=\"statement-text\">" + "Program Requirements for the " + i.Element("academicyear").Value + " Catalog Year" + "</p>";

					if (i.Element("progressionandadditionalgraduationrequirements").Value != "") {
						response += "<p class=\"statement-text\">" + "Program Progression and Additional Graduation Requirements" + "</p>";
						response += "<p>" + i.Element("progressionandadditionalgraduationrequirements").Value + "</p>";
					}

					response += "<p class=\"statement-text\">" + "Program Requirements" + "</p>";
					response += "<p>" + i.Element("degreerequirements").Value + "</p>";

					response += "<p>" + "<strong>" + "<span style=\"text-transform: uppercase\">Total Credits Required</span>: " + i.Element("semesterhours").Value + " HRS</strong>" + "</p>";

					if (i.Element("futurereqs").Value != "") {
						response += "<p>" + i.Element("futurereqs").Value + "</p>";	
					}

					if (i.Element("additionalgradrequirements").Value != "") {
						response += "<p class=\"statement-text\">" + "Additional Graduation Requirements" + "</p>";
						response += "<p>" + i.Element("additionalgradrequirements").Value + "</p>";
					}

					if (i.Element("transfernotes").Value != "") {
						response += "<p class=\"statement-text\">" + "Transfer Notes and Acceptable Substitutes" + "</p>";
						response += "<p>" + i.Element("transfernotes").Value + "</p>";
					}

					if (i.Element("progressrewuirements").Value != "") {
						response += "<p class=\"statement-text\">" + "Progression Requirements" + "</p>";
						response += "<p>" + i.Element("progressrewuirements").Value + "</p>";
					}

				}
				response += "</div>" + 
					"</section>";

			} catch (Exception e) {
				response = "<div>The graduate program requirements are temporarily unavailable. Please try again later.</div>";
				response += e.ToString();
			}

			return response;
		}
		/*
		* END method: getGraduateProgramRequirements
		*/

		
		/*
		* BEGIN method: getCatalogFrontMatterInfo 
		*/

		public static string getCatalogFrontMatterInfo(string frontmattercatalogtype = "", string academicyear = "", string fmid= "") {

			string strURLEncodedAcademicYear = "";
			string strURLEncodedfmid = "";
			
			//fgcuteam-mike Select type
			if ((HttpContext.Current.Request.QueryString["catalogtype"] != "") && (HttpContext.Current.Request.QueryString["catalogtype"] != null)) {
					frontmattercatalogtype = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["catalogtype"]);
			}else{
					if(String.IsNullOrWhiteSpace(frontmattercatalogtype)) frontmattercatalogtype = "current";//fgcuteam-mike
			}
		
			if ((HttpContext.Current.Request.QueryString["academicyear"] != "") && (HttpContext.Current.Request.QueryString["academicyear"] != null)) {
				strURLEncodedAcademicYear = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["academicyear"]);
				//frontmattercatalogtype = "archive";//fgcuteam-mike if year select archive automatically
			}
			else {
				strURLEncodedAcademicYear = System.Web.HttpUtility.UrlEncode(academicyear);
				/*//fgcuteam-mike Select current or working:
				if ((HttpContext.Current.Request.QueryString["type"] != "") && (HttpContext.Current.Request.QueryString["type"] != null)) {
					frontmattercatalogtype = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["type"]);
				}else{
					frontmattercatalogtype = "current";
				}*/
					
			}
			
			if ((HttpContext.Current.Request.QueryString["fmid"] != "") && (HttpContext.Current.Request.QueryString["fmid"] != null)) {
				strURLEncodedfmid = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["fmid"]);
			}
			else {
				strURLEncodedfmid = System.Web.HttpUtility.UrlEncode(fmid);
			}
			
			string _url = "https://www2.fgcu.edu/datafeeds/catalogfrontmatter.xml.asp?1=1" +		
				"&wherefield=FMID" +
				"&whereoperator=eq" +
				"&wherevalue="+ strURLEncodedfmid;

			string strTableName = "";
			switch (frontmattercatalogtype.ToLower()) {
				case "archives":
				case "archive":
				strTableName = "FM_Archives";
				break;

				case "prerelease":
				case "pre-release":
				strTableName = "FM_Working";

				_url += "&wherefield=Approved" +
					"&whereoperator=eq" +
					"&wherevalue=T";

				break;

				case "working":
				strTableName = "FM_Working";

				//fgcuteam-mike disabled condition
				/*_url += "&wherefield=Approved" +
					"&whereoperator=ne" +
					"&wherevalue=T";*/

				break;

				case "current":
				default:
				strTableName = "FM_CurrentCat";
				break;
			}

			if (strURLEncodedAcademicYear != "") {
				_url += "&wherefield=AcademicYear" +
					"&whereoperator=eq" +
					"&wherevalue=" + strURLEncodedAcademicYear;
			}

			_url +=	"&orderby=AcademicYear" +
				"&orderdir=DESC" +
				"&orderby=FMTitle" +
				"&orderdir=ASC" +
				"&table=" + strTableName;

			string _xpath = "//root/data/item";

			string response = "";
		
			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath);

				// Create the page title:
				string strFrontMatterTitle = "";
				strFrontMatterTitle = rss.items[0].Element("fmid").Value;
				if ((strURLEncodedAcademicYear != "") && (strURLEncodedAcademicYear != null)) {
					strFrontMatterTitle += " " + strURLEncodedAcademicYear + " Catalog Year";
				}
				
				if(fmid != "Home Page"){
				response += "<section>" +
					"<div class=\"container-flex\">" +
					"<p class=\"h2\">"+strFrontMatterTitle+"</p>" +
					"</div>" +
					"</section>";
				}else{
					response +="<div><p>&nbsp;</p></div>";
				}
				
				if(rss.items.Count > 1){//fgcuteam-mike

					response += "<div class=\"client-stackable container-flex accordion-wrapper wrapper-catalog-front-matter-content\">" +
						"<div class=\"accordion-controls all-collapsed\">" +
						"<button class=\"open\">Expand All</button>" +
						"<button class=\"close\">Collapse All</button>" +
						"</div>" +
						"<ul>";

					//display items 
					foreach (var i in rss.items) {

						response += "<li>"+
							"<p class=\"accordion-title\">"+i.Element("fmtitle").Value+"</p>" +
							"<a class=\"toggle-accordion\" aria-expanded=\"false\" href=\"#"+i.Element("id").Value+"\"><span>Toggle More Info</span></a>" +
							"<div id=\""+i.Element("id").Value+"\" class=\"more-info\" aria-hidden=\"true\">" +
							"<div class=\"container-print\" ><a href=\"https://www2.fgcu.edu/_includes/FGCUcatalog/printinfo.asp?cat="+strTableName+"&ID="+i.Element("id").Value+"\" target=\"_blank\" title=\"Print\" aria-label=\"Print\" ><span class=\"icon-print\"></span></a></div>" +
							i.Element("fmcontent").Value +
							"</div>" +
							"</li>";

					}

					response += "</div>" +
						"</ul>";
				}else{
					response += "<div class=\"client-stackable container-flex\">" +
									//"<p class=\"h5\">"+rss.items[0].Element("fmtitle").Value+"</p>" +
									"<div>" + 
									"<div class=\"container-print\" ><a href=\"https://www2.fgcu.edu/_includes/FGCUcatalog/printinfo.asp?cat="+strTableName+"&ID="+rss.items[0].Element("id").Value+"\" target=\"_blank\" title=\"Print\" aria-label=\"Print\"><span class=\"icon-print\"></span></a></div>" +
									rss.items[0].Element("fmcontent").Value + 
									"</div>" +
								"</div>";
				}

			} catch (Exception e) {
				response = "<div class=\"container-flex\">This catalog content is not available for this academic year.</div>";
				/* DEBUG
				response += "<p>getCatalogFrontMatterInfo() Feed '"+_url+"'</p>";
				response += "<p>strTableName '"+strTableName+"'</p>";
				response += "<p>academicyear '"+academicyear+"'</p>";
				response += "<p>strURLEncodedAcademicYear '"+strURLEncodedAcademicYear+"'</p>";
				response += e.ToString();
				//*/
			
			}

			return response;

		}

		/*
		* END method: getCatalogFrontMatterInfo
		*/
		
		/*
		* fgcuteam-mike
		*/
		public static string getCatalogFrontMatters(string frontmattercatalogtype = "", string academicyear = "", string fmtype = "") {
			string strURLEncodedAcademicYear = "";
			
			
			//fgcuteam-mike Select type
			if ((HttpContext.Current.Request.QueryString["catalogtype"] != "") && (HttpContext.Current.Request.QueryString["catalogtype"] != null)) {
					frontmattercatalogtype = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["catalogtype"]);
			}else{
				if(String.IsNullOrWhiteSpace(frontmattercatalogtype)){	
					if(HttpContext.Current.Request.Url.AbsolutePath.ToLower().Contains("/working/")) 
						frontmattercatalogtype = "working";
					else
						frontmattercatalogtype = "current";//fgcuteam-mike
				}
			}
		
			if ((HttpContext.Current.Request.QueryString["academicyear"] != "") && (HttpContext.Current.Request.QueryString["academicyear"] != null)) {
				strURLEncodedAcademicYear = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["academicyear"]);
				//frontmattercatalogtype = "archive";//fgcuteam-mike if year select archive automatically
			}
			else {
				strURLEncodedAcademicYear = System.Web.HttpUtility.UrlEncode(academicyear);
			}
			
			
			string _url = "https://www2.fgcu.edu/datafeeds/catalogfrontmatter.xml.asp?1=1" +	
				"&distinct=1" +
				"&selectfield=fmid&selectfield=type&selectfield=academicyear" +
				"&wherefield=Type" +
				"&whereoperator=eq" +
				"&wherevalue="+ fmtype;

			string strTableName = "";
			switch (frontmattercatalogtype.ToLower()) {
				case "archives":
				case "archive":
				strTableName = "FM_Archives";
				break;

				case "prerelease":
				case "pre-release":
				strTableName = "FM_Working";

				_url += "&wherefield=Approved" +
					"&whereoperator=eq" +
					"&wherevalue=T";

				break;

				case "working":
				strTableName = "FM_Working";

				//fgcuteam-mike disabled condition
				/*_url += "&wherefield=Approved" +
					"&whereoperator=ne" +
					"&wherevalue=T";*/

				break;

				case "current":
				default:
				strTableName = "FM_CurrentCat";
				break;
			}

			if (strURLEncodedAcademicYear != "") {
				_url += "&wherefield=AcademicYear" +
					"&whereoperator=eq" +
					"&wherevalue=" + strURLEncodedAcademicYear;
			}

			_url +=	"&distinct=1";
				
			_url +=	"&orderby=AcademicYear" +
				"&orderdir=DESC" +
				"&orderby=FMID" +
				"&orderdir=ASC" +
				"&table=" + strTableName;	
			

			string _xpath = "//root/data/item";

			string response = "";			
			
			try {
				string[] years = strURLEncodedAcademicYear.Split('-');
				int startYear = 0;

				if (years.Length >= 1){					
					Int32.TryParse(years[0], out startYear);
				}

				OuMediaRss rss = new OuMediaRss(_url, _xpath);
				
				if(rss.items.Count > 1){
					int itemsTotal = rss.items.Count;
					int columnTotal = itemsTotal/2;
					int colCounter = 0;
					string basePage = "";
					
					response += "<div class=\"container-flex row-alt\">";
					//display items 
					foreach (var i in rss.items) {

						if(colCounter == 0 ){
							response += "<div class=\"col-sm\"><ul>";
							if(fmtype=="A") response += "<li><a href=\"/academics/academiccalendar/\">Academic Calendar</a></li>";
						}
						if(colCounter == columnTotal) response += "</ul></div><div class=\"col-sm\"><ul>";
						if(fmtype=="A") basePage = "generalinformation";
						if(fmtype=="U") basePage = "undergraduateinformation";
						if(fmtype=="G") basePage = "graduateinformation";
						if (i.Element("fmid").Value != "Home Page"
						&& !( fmtype=="A" &&  i.Element("fmid").Value.Contains("Course Information") )
						&& !( (startYear > 2011||startYear==0) && fmtype=="U" && (i.Element("fmid").Value.Contains("Goal") || i.Element("fmid").Value.Contains("Outcome")) ) 
						&& !( (startYear > 2011||startYear==0) && fmtype=="G" && (i.Element("fmid").Value.Contains("Goal") || i.Element("fmid").Value.Contains("Outcome")) )   )	{//fgcuteam-mike: Remove Learning outcome as requested by Catalog Team 1/25/2021
							response += "<li>";
							response += "<a href=\""+basePage+"?fmid="+System.Web.HttpUtility.UrlEncode(i.Element("fmid").Value);
							if(frontmattercatalogtype != "current" && frontmattercatalogtype != "working") response += "&AcademicYear="+strURLEncodedAcademicYear;
							response += "\">"+i.Element("fmid").Value+"</a>";
							response += "</li>";
						}
						//if(colCounter == 0) response += "</ul>";
						colCounter++;
						if(colCounter == itemsTotal) response += "</ul></div>";
					}
					response += "</div>";
				}
			} catch (Exception e) {
				response = "<div>This catalog content is not available for this academic year.</div>";
				//response += "<p> Feed '"+_url+"'</p>";
				//response += e.ToString();
			}
			return response;

		}

		
		/*
		* BEGIN method: getCatalogAcademicPrograms
		*/

		public static string getCatalogAcademicPrograms(string table = "", string academicyear = "") {

			string strURLEncodedAcademicYear = "";
			
			if (HttpContext.Current.Request.QueryString["academicyear"] != null) {
				if (HttpContext.Current.Request.QueryString["academicyear"] != "") {
					strURLEncodedAcademicYear = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["academicyear"]);
				}
			}

			if (strURLEncodedAcademicYear == "" && !String.IsNullOrWhiteSpace(academicyear)) {
				strURLEncodedAcademicYear = System.Web.HttpUtility.UrlEncode(academicyear);
			}

			string strTable = table;
			if (HttpContext.Current.Request.QueryString["table"] != null) {
				if (HttpContext.Current.Request.QueryString["table"] != "") {
					strTable = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["table"]);
				}
			}

			string strProgramType = "";
			if (HttpContext.Current.Request.QueryString["programtype"] != null) {
				if (HttpContext.Current.Request.QueryString["programtype"] != "") {
					strProgramType = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["programtype"]);
				}
			}

			string strCollege = "";
			if ((HttpContext.Current.Request.QueryString["college"] != "") && (HttpContext.Current.Request.QueryString["college"] != null)) {
				strCollege = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["college"]);
			}
			
			string strID = "";
			if ((HttpContext.Current.Request.QueryString["id"] != "") && (HttpContext.Current.Request.QueryString["id"] != null)) {
				strID = System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString["id"]);
			}

			string _url = "https://www2.fgcu.edu/datafeeds/catalogacademicprograms.xml.asp?1=1";
			if (strID == "") {//fgcuteam-mike
				_url += "&selectfield=id" +
				"&selectfield=admissionrequirements" +
				"&selectfield=programdegree" +
				"&selectfield=programtype" +
				"&selectfield=concentration" +
				"&selectfield=accredidations" +
				"&selectfield=college";
			}

			string strTableName = "";
			switch (strTable.ToLower()) {
				case "archives":
				case "archive":
				strTableName = "Archives";
				break;

				case "prerelease":
				case "pre-release":
				strTableName = "Working";

				//_url += "&wherefield=pStatus" +
				//	"&whereoperator=ne" +
				//	"&wherevalue=Not%20Reviewed%20Yet";

				break;

				case "working":
				strTableName = "Working";

				//_url += "&wherefield=pStatus" +
				//	"&whereoperator=eq" +
				//	"&wherevalue=Not%20Reviewed%20Yet";

				break;

				case "current":
				default:
				strTableName = "CurrentCat";
				break;
			}

			if (strURLEncodedAcademicYear != "" && strID == "") { //fgcuteam-mike add strID condition
				_url += "&wherefield=AcademicYear" +
					"&whereoperator=eq" +
					"&wherevalue=" + strURLEncodedAcademicYear;
			}
			
			if (strProgramType != "") {
				_url += "&wherefield=ProgramType" +
					"&whereoperator=eq" +
					"&wherevalue=" + strProgramType;
			}
			
			if (strCollege != "") {
				_url += "&wherefield=College" +
					"&whereoperator=eq" +
					"&wherevalue=" + strCollege;
			}
			
			if (strID != "") {
				_url += "&wherefield=ID" +
					"&whereoperator=eq" +
					"&wherevalue=" + strID;
			}			

			_url +=	"&orderby=AcademicYear" +
				"&orderdir=DESC" +
				"&orderby=ProgramDegree" +
				"&orderdir=ASC" +
				"&table=" + strTableName;

			string _xpath = "//root/data/item";
			
			string urlAjaxLoaderImage = "https://cdnjs.cloudflare.com/ajax/libs/galleriffic/2.0.1/css/loader.gif";

			string response = "";
			
			/* DEBUG
			response += "<p>getCatalogAcademicPrograms() Feed '"+_url+"'</p>";
			response += "<p>strTableName '"+strTableName+"'</p>";
			response += "<p>academicyear '"+academicyear+"'</p>";
			response += "<p>strURLEncodedAcademicYear '"+strURLEncodedAcademicYear+"'</p>";
			//*/
			
			try {

				OuMediaRss rss = new OuMediaRss(_url, _xpath, 15000);

				// Create the page title:
				string strProgramListTitle = "";

				if (strCollege != "") {
					string strCollegeDecoded = System.Web.HttpUtility.UrlDecode(strCollege);
					strProgramListTitle += "<span class=\"pagetitleprogramcollege h3\">" + strCollegeDecoded + " </span>";

					response += "<script type=\"text/javascript\">" +
						"gstrCollege = '" + strCollegeDecoded.Trim() + "'" +
						"</script>";
				}

				if (strProgramType != "") {
					
					response += "<script type=\"text/javascript\">" +
						"gstrProgramType = '" + strProgramType.Trim().ToUpper() + "'" +
						"</script>";
					
					string strProgramLevel = "";
					switch (strProgramType.ToLower()) {
						case "u":
						strProgramLevel += "Undergraduate Programs ";
						break;

						case "g":
						strProgramLevel += "Graduate Programs ";
						break;

						case "m":
						strProgramLevel += "Minors ";
						break;

						case "c":
						strProgramLevel += "Certificates ";
						break;

						default:
						break;
					}
					
					if (strProgramLevel != "") {
						strProgramListTitle += "<span class=\"pagetitleprogramtype h3\">" + strProgramLevel + " </span>";
					}
					
				}
				
				
				if ((strURLEncodedAcademicYear != "") && (strURLEncodedAcademicYear != null)) {
					strProgramListTitle += "<span class=\" h3\">" + strURLEncodedAcademicYear + " Catalog Year</span>";
				}
				

				response += "<script type=\"text/javascript\">" +
					"gstrTable = '" + strTableName.Trim() + "'" +
					"</script>";



				/*response += "<div class=\"container-flex accordion-wrapper wrapper-catalog-academic-programs-content wrapperacademicprograms \">" +
					"<ul>";

				//display items 
				foreach (var i in rss.items) {

					response += "<li class=\"ishidden\">"+
						"<p class=\"accordion-title\">"+i.Element("programdegree").Value+"</p>" +
						"<a class=\"toggle-accordion\" aria-expanded=\"false\" " +
						" data-id=\"" + i.Element("id").Value + "\" " +
						" data-programtype=\"" + i.Element("programtype").Value + "\" " +
						" data-college=\"" + i.Element("college").Value + "\" " +
						" ajaxloaded=\"0\" " +
						" href=\"#"+i.Element("id").Value+"\"><span>Toggle More Info</span></a>" +
						"<div class=\"more-info\" aria-hidden=\"true\">" +
						"<div class=\"academicprogramajaxloader ishidden \"><img src=\"" + urlAjaxLoaderImage + "\" /></div>" +
						"</div>" +
						"</li>";

				}

				response += "</div>" +
					"</ul>";*/
				
				//fgcuteam-mike
				string baseQuery = ( HttpContext.Current.Request.Url.Query.Contains("?") ) ? HttpContext.Current.Request.Url.Query+"&" : HttpContext.Current.Request.Url.Query;//HttpContext.Current.Request.ServerVariables["QUERY_STRING"]
				string collegeQuery = "?";
				string programtypeQuery = "?";
				foreach (String key in HttpContext.Current.Request.QueryString.AllKeys)
				{
    				if(key != "" && key !=null){
						if(key != "college") collegeQuery += key+"="+System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString[key])+"&";
						if(key != "programtype") programtypeQuery += key+"="+System.Web.HttpUtility.UrlEncode(HttpContext.Current.Request.QueryString[key])+"&";
					}
				}
				
				if (strID != "") {
					/*response += "<section class=\"wrapperacademicprograms\">" +
					"<div class=\"container-flex\">" +
					"<p class=\"h4\">"+strProgramListTitle+"</p>" +
					"</div>" +
					"</section>";*/
					
					response += "<div class=\"container-flex\"><a class=\"text-caption\" target=\"_blank\" href=\"https://www2.fgcu.edu/_includes/FGCUCatalog/printprogram.asp?cat=" + strTableName.Trim() +"&ID=" + strID + "\"><span class=\"icon-print\"></span>Print Program Details</a></div>";
					
					response += "<div class=\"container-flex wrapperacademicprograms\">";
					if ( rss.items[0].Element("programdegree").Value != "" ) {
						response += "<div class=\"h3\">" + rss.items[0].Element("programdegree").Value + " " + rss.items[0].Element("academicyear").Value + " Catalog Year" + "</div>";
					}
					if ( rss.items[0].Element("college").Value != "" ) {
						response += "<div class=\"programcollege\">" + rss.items[0].Element("college").Value + "</div>";
					}
					if ( rss.items[0].Element("division").Value != "" ) {
						response += "<div class=\"programdivision\">" + rss.items[0].Element("division").Value + "</div>";
					}
					var arrProgramContactInfo = new List<string>();
					if ( rss.items[0].Element("webaddress").Value != "" ) {
						arrProgramContactInfo.Add("Website: <a href=\"" + rss.items[0].Element("webaddress").Value + "\">" + rss.items[0].Element("webaddress").Value + "</a>");
					}
					if ( rss.items[0].Element("phone").Value != "" ) {
						arrProgramContactInfo.Add(rss.items[0].Element("phone").Value);
					}
					if(arrProgramContactInfo.Count > 0){
						response += "<div class=\"programcontactinfo\">" + String.Join("<br/>", arrProgramContactInfo.ToArray()) + "</div>";
					}
					if ( rss.items[0].Element("concentration").Value != "" ) {
						var arrConcentrations = rss.items[0].Element("concentration").Value.Split('|').ToList();
						response += "<fieldset class=\"programconcentrations\"><legend>Concentrations</legend>";
						response += "<ul>";
						foreach (var i in arrConcentrations) {
							response += "<li>" + i + "</li>";
						}
						response += "</ul>";
						response += "</fieldset>";
					}
					
					//response += "<div class=\"clearfloat\"></div>";
					if ( rss.items[0].Element("programdescription").Value != "" ) {
						response += "<div class=\"programprogramdescription\">" + rss.items[0].Element("programdescription").Value + "</div>";
					}
					if ( rss.items[0].Element("accredidations").Value != "" ) {
						response += "<div class=\"sectiontitle\">" + "Program Accreditation" + "</div>";
						response += "<div class=\"programprogramaccredidations\">" + rss.items[0].Element("accredidations").Value + "</div>";
					}					
					string parLabel = "";
					if (rss.items[0].Element("admissionrequirements").Value != "") {
						parLabel = (rss.items[0].Element("programtype").Value == "M")? "Admission Requirements" : "Program Admission Requirements";
						response += "<div class=\"sectiontitle\">" + parLabel + "</div>";
						response += "<div class=\"admissionrequirements\">" + rss.items[0].Element("admissionrequirements").Value + "</div>";
					}				
					if ( rss.items[0].Element("progressionandadditionalgraduationrequirements").Value != "" ) {
						response += "<div class=\"sectiontitle\">" + "Progression and Additional Graduation Requirements" + "</div>";
						response += "<div class=\"progressionandadditionalgraduationrequirements\">" + rss.items[0].Element("progressionandadditionalgraduationrequirements").Value + "</div>";
					}
					if ( rss.items[0].Element("degreerequirements").Value != "" ) {
						response += "<div class=\"sectiontitle\">" + "Program Requirements" + "</div>";
						response += "<div class=\"programdegreerequirements\">" + rss.items[0].Element("degreerequirements").Value + "</div>";
					}
					if ( rss.items[0].Element("semesterhours").Value != "" ) {
						response += "<div class=\"programsemesterhours\">Total Credits Required: " + rss.items[0].Element("semesterhours").Value + "</div>";
					}							
						
					response += "</div>";
				}else{
					string highlightCollege;
					response += "<div class=\"container-flex\">";
					response += "<p class=\"h5\">Filter Programs by Degree or College</p>";
					response += "<div class=\"row\">";
					response += 	"<div class=\"col-md-2\">Degree:</div>";
					response += 	"<div class=\"col-md-22\">";
					response += 		(HttpContext.Current.Request.QueryString["programtype"] == null)?"<span>All</span>" : "<a style=\"text-decoration: underline;\" href=\"" +programtypeQuery +"\">All</a>";
					response += 		(HttpContext.Current.Request.QueryString["programtype"] == "u")?" | <span>Undergraduate</span>" : " | <a style=\"text-decoration: underline;\" href=\"" +programtypeQuery +"programtype=u\">Undergraduate</a>";
					response += 		(HttpContext.Current.Request.QueryString["programtype"] == "g")?" | <span>Graduate</span>" : " | <a style=\"text-decoration: underline;\" href=\"" +programtypeQuery +"programtype=g\">Graduate</a>";
					response += 		(HttpContext.Current.Request.QueryString["programtype"] == "m")?" | <span>Minor</span>" : " | <a style=\"text-decoration: underline;\" href=\"" +programtypeQuery +"programtype=m\">Minor</a>";
					response += 		(HttpContext.Current.Request.QueryString["programtype"] == "c")?" | <span>Certificate</span>" : " | <a style=\"text-decoration: underline;\" href=\"" +programtypeQuery +"programtype=c\">Certificate</a>";
					response += 	"</div>";
					response += "</div>";
					response += "<div class=\"row\">";
					response += 	"<div class=\"col-md-2\">College:</div>";
					response += 	"<div class=\"col-md-22\">";
						response += 		(HttpContext.Current.Request.QueryString["college"] == null)?"<span>All</span>" : "<a style=\"text-decoration: underline;\" href=\"" +collegeQuery +"\">All</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "College of Arts and Sciences")?" | <span>College of Arts and Sciences</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college=College of Arts and Sciences\">College of Arts and Sciences</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "College of Education")?" | <span>College of Education</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college=College of Education\">College of Education</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "Lutgert College of Business")?" | <span>Lutgert College of Business</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college=Lutgert College of Business\">Lutgert College of Business</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "Marieb College of Health & Human Services")?" | <span>Marieb College of Health &amp; Human Services</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college="+System.Web.HttpUtility.UrlEncode("Marieb College of Health & Human Services")+"\">Marieb College of Health &amp; Human Services</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "Daveler & Kauanui School of Entrepreneurship")?" | <span>Daveler &amp; Kauanui School of Entrepreneurship</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college="+System.Web.HttpUtility.UrlEncode("Daveler & Kauanui School of Entrepreneurship")+"\">Daveler &amp; Kauanui School of Entrepreneurship</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "U.A. Whitaker College of Engineering")?" | <span>U.A. Whitaker College of Engineering</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college=U.A. Whitaker College of Engineering\">U.A. Whitaker College of Engineering</a>";
						response += 		(HttpContext.Current.Request.QueryString["college"] == "The Water School")?" | <span>The Water School</span>" : " | <a style=\"text-decoration: underline;\" href=\""+collegeQuery+"college=The Water School\">The Water School</a>";					
		
						/*highlightCollege = (HttpContext.Current.Request.QueryString["college"] == null) ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"\">All</a>";					
						highlightCollege = (HttpContext.Current.Request.QueryString["college"] == "College of Arts and Sciences") ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"college=College of Arts and Sciences\">College of Arts and Sciences</a>";
						highlightCollege = (HttpContext.Current.Request.QueryString["college"] == "College of Education") ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"college=College of Education\">College of Education</a>";
						highlightCollege = (HttpContext.Current.Request.QueryString["college"] == "Lutgert College of Business") ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"college=Lutgert College of Business\">Lutgert College of Business</a>";
						highlightCollege = (HttpContext.Current.Request.QueryString["college"] == "Marieb College of Health & Human Services") ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"college="+System.Web.HttpUtility.UrlEncode("Marieb College of Health & Human Services")+"\">Marieb College of Health &amp; Human Services</a>";
						highlightCollege = (HttpContext.Current.Request.QueryString["college"] == "School of Entrepreneurship") ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"college=School of Entrepreneurship\">School of Entrepreneurship</a>";
						highlightCollege = (HttpContext.Current.Request.QueryString["college"] == "U.A. Whitaker College of Engineering") ? "" : "text-decoration: underline;";
						response += 	"<a style=\" "+highlightCollege+" \" href=\""+collegeQuery+"college=U.A. Whitaker College of Engineering\">U.A. Whitaker College of Engineering</a>";*/						
					response += 	"</div>";
					response += "</div>";
					
					if(strProgramListTitle=="") strProgramListTitle = "<span class=\"h3\">All Academic Programs</span>";
					response += "<section class=\"wrapperacademicprograms\">" +
								"<div class=\"container-flex\">" +
								"<br /><div class=\"\">"+strProgramListTitle+"</div>";
								//if( HttpContext.Current.Request.QueryString["programtype"] == null || HttpContext.Current.Request.QueryString["programtype"] == "u" || HttpContext.Current.Request.QueryString["programtype"] == "g")  response += "<div>(Concentrations below degree)</div>";
					response += "</div>" +
					"</section>";
					
					if(rss.items.Count > 0){
						response += "<ul>";
						foreach (var i in rss.items) {
							response += "<li><a href=\""+"?id="+i.Element("id").Value+" \">"+i.Element("programdegree").Value+"</a></li>";
							
							if ( i.Element("concentration").Value != "" ) {
								var arrayConcentrations = i.Element("concentration").Value.Split('|').ToList();
								response += "<ul>";
								foreach (var c in arrayConcentrations) {
									response += "<li>" + c + "</li>";
								}
								response += "</ul>";
							}							
						}
						response += "</ul>";
					}else{
						response += "No programs found.";					
					}
					response +="</div>";
					

				}
				
			} catch (Exception e) {
				response = "<div>The catalog content is not available for this academic year.</div>";
				/* DEBUG
				response += "<p>getCatalogAcademicPrograms() Feed '"+_url+"'</p>";
				response += "<p>strTableName '"+strTableName+"'</p>";
				response += "<p>academicyear '"+academicyear+"'</p>";
				response += "<p>strURLEncodedAcademicYear '"+strURLEncodedAcademicYear+"'</p>";
				response += e.ToString();
				//*/

			}

			return response;

		}

		/*
		 * END method: getCatalogAcademicPrograms
		 */



	}
}