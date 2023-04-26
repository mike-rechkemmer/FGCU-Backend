using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Specialized;
using System.Data;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using System.Collections.Generic;
using OUC;

/// <summary>
///
/// Usage: 
/// 
/// </summary> 

namespace OUC {
	public class GetProfiles {
		
	    class EmployeeInfo
	{
		public string id;
		public string firstname;
		public string lastname;		
		public string title;
		public string degree;
		public string focus;
		public string photo;
		public string email;
		public string emailText;
		public string building;
		public string room;
		public string department1_id;
		public string department2_id;
		public string department1_name;
		public string department2_name;							
		public string phone;
		public string sex;
		public string faculty_staff;
		public string summary;
	}     
	
	public static string getProfileInfoFormat(string emailPrefix, string field){
		string pf = "";
		string dataFieldInfo = "";
		if(field == "location"){
			dataFieldInfo = getProfileInfo(emailPrefix,  "stvbldg_desc") + " " + getProfileInfo(emailPrefix,  "spraddr_street_line2");
			if(dataFieldInfo != " ") pf = "<span class=\"icon-location\"></span>" + " <span>" + dataFieldInfo + "</span>";
		}
		if(field == "phone"){
			dataFieldInfo = getProfileInfo(emailPrefix,  "sprtele_phone_number");
			if(dataFieldInfo != "") pf = "<span class=\"icon-phone\"></span>" + " <a href=\"tel:"+ dataFieldInfo + "\">" + dataFieldInfo + "</a>";
		}
		if(field == "email"){
			dataFieldInfo = getProfileInfo(emailPrefix,  "email_address");
			if(dataFieldInfo != "") pf = "<span class=\"icon-mail-close\"></span>" + " <a href=\"mailto:"+ dataFieldInfo + "\">" + dataFieldInfo + "</a>";
		}			
		return pf;
	}
		
	
	public static string getProfileInfo(string emailPrefix, string dataField){
			string response = "";
			string directoryFilePath = HttpContext.Current.Server.MapPath("/directory")+"/_directorydata.xml";
			string cacheKey = directoryFilePath; //"profile"+emailPrefix;
			string directoryXML = null;
		
			//For legacy compatibility only to other directory file.
			if(dataField == "nbrjobs_desc") dataField = "title";
			if(dataField == "email_address") dataField = "email-address";
			if(dataField == "stvbldg_desc") dataField = "building";
			if(dataField == "spraddr_street_line2") dataField = "room";
			if(dataField == "ftvorgn_orgn_code") dataField = "department1-id";
			if(dataField == "organization_level_2") dataField = "department2-id";
			if(dataField == "ftvorgn_title") dataField = "department1-name";
			if(dataField == "organization_desc_2") dataField = "department2-name";
			if(dataField == "sprtele_phone_number") dataField = "phone";
			if(dataField == "spbpers_sex") dataField = "sex";
			if(dataField == "faculty_staff_ind") dataField = "faculty-staff";
		
			
			try {
				XmlNode profile;
				XmlDocument xdcDocument = new XmlDocument();
				directoryXML = (System.Web.HttpRuntime.Cache[cacheKey]as string) ;
				System.Web.Caching.CacheDependency myDependency = new System.Web.Caching.CacheDependency(directoryFilePath,  DateTime.Now);
				//if( directoryXML == null || myDependency.HasChanged){
					directoryXML = System.IO.File.ReadAllText(directoryFilePath);
				//	System.Web.HttpRuntime.Cache.Insert(cacheKey, directoryXML, myDependency);					
				//}
				xdcDocument.LoadXml(directoryXML);
				XmlNode xelRoot = xdcDocument.DocumentElement;
				IEnumerator ienum = xelRoot.GetEnumerator();
				while (ienum.MoveNext()) 
				{     
					profile = (XmlNode) ienum.Current;				  					
					if(emailPrefix == profile["email-prefix"].InnerText ) {						
						if(dataField == "avatar"){
							if(File.Exists(HttpContext.Current.Server.MapPath("/directory/_images")+"/"+emailPrefix+".jpg") == true) return "/directory/_images/"+emailPrefix+".jpg";
							else
								return (profile["sex"].InnerText == "F") ? "/_resources/images/faculty-staff-female-avatar-200x200.jpg" : "/_resources/images/faculty-staff-male-avatar-200x200.jpg";	
						}else{
							return profile[dataField].InnerText;
						}
					}
				}				
			}
			catch (Exception e)
			{				
				response = "Faculty Staff Directory Profile Error."+ e.Message ;
			}
		return response;
	}
		
		public static string displaySnippetListing(string deptFilter, string userList="", string removeList = "", string layout = "thumbnail", bool azindex = false, string facultystaff = "all", string removeFields = "", bool manualsort = false) { 
			string response = "";
			string listPersons = "";
			int personsCount = 0;
			if(deptFilter == "" && userList == "") return "Please input a Department Id or a comma separated User List";
			string[] deptFilterList = deptFilter.Split(',');
			userList = Regex.Replace(userList, @"\s+", "");
			
			try {
				
				var _allEmployeeInfo = new List<EmployeeInfo>();
				
				XmlDocument xdcDocument = new XmlDocument();
				//string directoryXML = HttpContext.Current.Server.MapPath("/directorydata")+"/directory.xml";
				string directoryXML = HttpContext.Current.Server.MapPath("/directory")+"/_directorydata.xml";
				xdcDocument.Load(@directoryXML);			
				XmlNode xelRoot = xdcDocument.DocumentElement;
				
				
				var xDoc = XDocument.Parse( xdcDocument.InnerXml);
								
				IEnumerator ienum = xelRoot.GetEnumerator();
				XmlNode profile;
				while (ienum.MoveNext()) 
				{     
					profile = (XmlNode) ienum.Current;				  
					string dept1 = "("+profile["department1-id"].InnerText+")";
					string dept2 = "("+profile["department2-id"].InnerText+")";
					string deptFilterP = "";
					string usrEmail = profile["email-address"].InnerText;
					foreach (string deptX in deptFilterList) deptFilterP += "("+ deptX.Trim() +")";
					
					if( removeList != "" && deptFilter != ""){
						string[] removeUsers = removeList.Replace(" ", "").TrimEnd(',').Replace("@fgcu.edu", "").Split(',');
						if (  removeUsers.Contains(usrEmail.Split('@')[0]) ) continue;
					}
					
					if( userList == "" ){ 
						if(deptFilterP.Contains(dept1)==false && deptFilterP.Contains(dept2)==false )  continue;
					}else{
						string[] users = userList.Replace(" ", "").TrimEnd(',').Replace("@fgcu.edu", "").Split(',');
						if(deptFilter == ""){
							if ( ! users.Contains(usrEmail.Split('@')[0]) ) continue;
						}else{
							if ( 
								(! users.Contains(usrEmail.Split('@')[0]) )
								&& (deptFilterP.Contains(dept1)==false && deptFilterP.Contains(dept2)==false) 
							   ) continue;
						}
					}					
					string id = profile["id"].InnerText;
					string firstname = (profile["preffirstname"].InnerText.Trim() == "") ? profile["firstname"].InnerText : profile["preffirstname"].InnerText;
					string lastname = profile["lastname"].InnerText;					
					string title = profile["title"].InnerText;
					string degree = profile["degree"].InnerText;
					string focus = profile["focus"].InnerText;
					string photo = profile["photo"].InnerText;
					string email = profile["email-address"].InnerText;
					string emailText = email.Split('@')[0];
					string building = profile["building"].InnerText;
					string room = profile["room"].InnerText;
					string department1_id = profile["department1-id"].InnerText;
					string department2_id = profile["department2-id"].InnerText;
					string department1_name = profile["department2-name"].InnerText;
					string department2_name = profile["department2-name"].InnerText;										
					string phone = profile["phone"].InnerText;
					string sex = profile["sex"].InnerText;
					string faculty_staff = profile["faculty-staff"].InnerText;
					string summary = profile["summary"].InnerText;

					
					if(userList != ""){
						string[] users2 = userList.Replace(" ", "").TrimEnd(',').Replace("@fgcu.edu", "").Split(',');
						if(users2.Contains(emailText) == false && facultystaff != "all" && faculty_staff.ToLower() != facultystaff.ToLower() ) continue;
					}else{
						if( facultystaff != "all" && faculty_staff.ToLower() != facultystaff.ToLower() ) continue;
					}
					
					_allEmployeeInfo.Add(new EmployeeInfo() {id=id, firstname=firstname,  lastname=lastname,  title=title,  degree=degree, focus=focus, photo=photo, email=email,  emailText=emailText,  building=building,  room=room,  department1_id=department1_id,  department2_id=department2_id,  department1_name=department1_name,  department2_name=department2_name, phone=phone, sex=sex, faculty_staff=faculty_staff, summary=summary });
					
				}
				IEnumerable<object> o_allEmployeeInfo = null;
				if(manualsort == true && userList != ""){
					string[] newOrder = userList.Replace(" ", "").TrimEnd(',').Replace("@fgcu.edu", "").Split(',');
					List<string> theseIds = _allEmployeeInfo.Select(item => item.emailText).ToList();
					List<string> newOrderList = new List<string>(newOrder);
					List<string> newOrderList2 = new List<string>();
					foreach (string userName in newOrderList)
					{
						if(theseIds.Contains(userName)) newOrderList2.Add(userName);
					}
					string[] newOrder2 = newOrderList2.ToArray();
					
					
					o_allEmployeeInfo = newOrder2.Select(o => _allEmployeeInfo.First(t => t.emailText == o)).ToList();
				}else{//force sorting alphabetically by firstname(in case directory order is wrong)
					
						o_allEmployeeInfo  = _allEmployeeInfo.OrderBy(x => x.lastname).ThenBy(x => x.firstname).ToList(); 
					
				}
				var n_allEmployeeInfo = (o_allEmployeeInfo != null) ? o_allEmployeeInfo : _allEmployeeInfo;
				
				foreach (EmployeeInfo empInfo in n_allEmployeeInfo)
        		{
         			string id = empInfo.id;
					string firstname = empInfo.firstname;
					string lastname = empInfo.lastname;					
					string title = empInfo.title;
					string degree = empInfo.degree;
					string focus = empInfo.focus;
					string photo = empInfo.photo;
					string email = empInfo.email;
					string emailText = empInfo.emailText;
					string building = empInfo.building;
					string room = empInfo.room;
					string department1_id = empInfo.department1_id;
					string department2_id = empInfo.department2_id;
					string department1_name = empInfo.department1_name;
					string department2_name = empInfo.department2_name;										
					string phone = empInfo.phone;
					string sex = empInfo.sex;
					string faculty_staff = empInfo.faculty_staff;
					string summary = empInfo.summary;
					
					//string degree = "";
					bool userExists = false;
					
					//Set a default picture based on gender. If a .xml profile is found try to get the img value from there, otherwise try to get it from a {emailprefix}.jpg file if it exists.
					string picture = (sex == "F") ? "/_resources/images/faculty-staff-female-avatar-200x200.jpg" : "/_resources/images/faculty-staff-male-avatar-200x200.jpg";		
					if(File.Exists(HttpContext.Current.Server.MapPath("/directory")+"/"+emailText+".xml") == true)	userExists = true;

					if(picture == "/_resources/images/faculty-staff-female-avatar-200x200.jpg" || picture == "/_resources/images/faculty-staff-male-avatar-200x200.jpg" ){
						if(File.Exists(HttpContext.Current.Server.MapPath("/directory/_images")+"/"+emailText+".jpg") == true) picture = "/directory/_images/"+emailText+".jpg";
					}
					if(degree.Replace(" ", "") != "" ) degree = " ("+ degree+")";
					
					string 					colWidth = "col-xs-12";
					string 					contentWidth = "col-md-14";
					string 					picWidth = "col-md-10";
					if(layout == "4column") {colWidth = "col-xs-6";  contentWidth = "col-md-24"; picWidth = "col-md-24";}
					if(layout == "3column") {colWidth = "col-xs-8";  contentWidth = "col-md-24"; picWidth = "col-md-24";}
					if(layout == "2column") {colWidth = "col-xs-12"; contentWidth = "col-md-14"; picWidth = "col-md-10";}//thumbnail
					if(layout == "text") 	{colWidth = "col-xs-12"; contentWidth = ""; 		 picWidth = "col-md-10";}
               		
					if(layout == "1column"){
						listPersons += "<div class=\"col-xs-24 facultystaffcontainer\">";
							listPersons += "<div class=\"profileheader\">";
								if(userExists==true)listPersons += "<div class=\"profiletitle\"><span class=\"h3\"><strong>"+lastname +", "+firstname + degree + "</strong></span><span class=\"profilefull\"><a href=\"/directory/" +emailText+".aspx?list=1"+ "\">Full Profile</a></span></div>";
								else
													listPersons += "<div class=\"profiletitle\"><span class=\"h3\"><strong>"+lastname +", "+firstname + degree + "</strong></span></div>";
							
							listPersons +=	"<div class=\"profilesubtitle\">";
							if(!removeFields.Contains("notitle"))listPersons += "<span><strong>"+title+"</strong></span>";
							if(!removeFields.Contains("notitle") && !removeFields.Contains("nofocus") && title != "" && focus != "")listPersons += ", ";	
							if(!removeFields.Contains("nofocus"))listPersons += "<span>"+focus+"</span>";	
							listPersons += "</div>";
							listPersons += "</div>";
						
							listPersons += "<div class=\"col-sm-6\"><span>";
							if(userExists==true)listPersons += "<a href=\"/directory/" +emailText+".aspx?list=1"+ "\">";
							listPersons += 						"<img src=\"" + picture +"\" alt=\"Faculty Staff Default Avatar\" width=\"200\" height=\"200\" />";
							if(userExists==true)listPersons += "</a>";
							listPersons += "</span></div>";
							listPersons += "<div class=\"col-sm-18\">";
							listPersons += 			"<div class=\"profiledescription\">"+summary+"</div>";					
							listPersons += 			"<div class=\"profilecontacts\">";										
							if((building.Length > 1 || room.Length > 1) && removeFields.Contains("nooffice") == false) listPersons += "<span class=\"footerprofile1col\"><span class=\"icon-location\"></span>" + building + " " + room +"</span>";
							if(phone.Length > 1 && removeFields.Contains("nophone") == false) listPersons += 		      	"<span class=\"footerprofile1col\"><span class=\"icon-phone\"></span><a href=\"tel:"+ phone +"\">"+ phone +"</a></span>";
							if(!removeFields.Contains("noemail"))   listPersons += 		      	"<span class=\"footerprofile1col\"><span class=\"icon-mail-close\"></span><a href=\"mailto:"+email+"\">" +email+"</a></span>";						
							listPersons += 		    "</div>";
							listPersons += "</div>";						
						listPersons += "</div>";						
					}else if(layout == "table"){
						listPersons += "<tr class=\"with-borders\">";
						if(userExists==true)listPersons += "<td class=\"with-borders align-center\" data-header=\"Name\"><a href=\"/directory/" +emailText+".aspx?list=1"+ "\">"+lastname +", "+firstname + degree + "</a></td>";
						else
											listPersons += "<td class=\"with-borders align-center\" data-header=\"Name\">"+lastname +", "+firstname + degree + "</td>";
						listPersons += 		"<td class=\"with-borders align-center\" data-header=\"Department\">"+department1_name+"</td>";
						listPersons += 		"<td class=\"with-borders align-center\" data-header=\"Email\"><a href=\"mailto:"+email+"\">" +email+"</a></td>";
						listPersons += 		"<td class=\"with-borders align-center\" data-header=\"Phone\"><a href=\"tel:"+ phone +"\">"+ phone +"</a></td>";
						listPersons +=	"</tr>";
					}else{
						listPersons += "<div class=\""+colWidth+" facultystaffcontainer\">";
						if(layout != "text") {
							listPersons += "<div class=\"" +picWidth+"\"><span>";
							if(userExists==true)listPersons += "<a href=\"/directory/" +emailText+".aspx?list=1"+ "\">";
												listPersons += "<img src=\"" + picture +"\" alt=\"Faculty Staff Default Avatar\" width=\"200\" height=\"200\" />";
							if(userExists==true)listPersons += "</a>";
							listPersons += "</span></div>";
						}
						listPersons += 		"<div class=\""+ contentWidth +"\">";
						listPersons += 			"<div>";					
						if(userExists==true)listPersons += "<div><span><strong><a href=\"/directory/" +emailText+".aspx?list=1"+ "\">"+lastname +", "+firstname + degree + "</a></strong></span></div>";
						else
											listPersons += "<div><span><strong>"+lastname +", "+firstname + degree + "</strong></span></div>";
						if(!removeFields.Contains("notitle"))listPersons += 		      	"<div><span>"+title+"</span></div>";
						if((building.Length > 1 || room.Length > 1) && removeFields.Contains("nooffice") == false) listPersons += 		      	"<div><span class=\"icon-location\"></span> <span class=\"officelocation\">" + building + " " + room +"</span></div>";
						if(phone.Length > 1 && removeFields.Contains("nophone") == false) listPersons += 		      	"<div><span class=\"icon-phone\"></span> <a href=\"tel:"+ phone +"\">"+ phone +"</a></div>";
						if(!removeFields.Contains("noemail"))   listPersons += 		      	"<div><span class=\"icon-mail-close\"></span> <a href=\"mailto:"+email+"\">" +email+"</a></div>";
						if(focus.Length > 1 && removeFields.Contains("nofocus")==false)listPersons += "<div><span class=\"icon-bookmark-checkmark\"></span> <span>"+focus+"</span></div>";	
						listPersons += 			"</div>";
						listPersons += 		"</div>";
						listPersons += "</div>";
					}
					personsCount++;
            
					
				}
				
			}
			catch (Exception e)
			{				
				response = "Faculty Staff Directory List Error."+e.Message ;
			}
			if(azindex==true) {
				response = "<div class=\"az-index\" data-default-letter=\"\" data-init-letter=\"\"><ul>"+ listPersons + "</ul></div>";
			}else if(layout == "table") {
				response = "<div class=\"stackable-table-wrap\"><table  data-xsl-params=\"header,center\"><thead><tr><th>Name</th><th>Department</th><th>Email</th><th>Phone</th></tr>                  </thead><tbody>" + listPersons + "</tbody></table></div>";
			}else{
				response = listPersons;
			}
			return response;
		}
		
		public static string displayListing(string profileXML, string deptXML, string indexXML, string deptFilter) { 
			string response = "";			 						
			try {
				XmlDocument xdcDocument = new XmlDocument();
				xdcDocument.Load(@profileXML);
				XmlElement xelRoot = xdcDocument.DocumentElement;
				XmlNodeList profileNodes = xelRoot.SelectNodes("/directorydataset/SXVHDIR");
				
				XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object			
				xmlDoc.Load(profileXML); // Load the XML document from the specified file						
				XmlNodeList profiles = xmlDoc.GetElementsByTagName("/directorydataset");																														
					foreach (XmlNode profile in profileNodes) {					
					string firstName = profile["spriden_first_name"].InnerText;
					string lastName = profile["spriden_last_name"].InnerText;					
					string jobTitle = profile["nbrjobs_desc"].InnerText;
					string college = "";
					string deptID = profile["ftvorgn_orgn_code"].InnerText;
					string office = "";
					string email = profile["email_address"].InnerText;
					string emailText = email.Split('@')[0];
					string phone = "";
					response += "<tr>";
					response += "	<td>" + firstName + " " + lastName + "</td>";				
					response += "	<td>" + jobTitle + "</td>";				
					response += "	<td>" + college + "</td>";				
					response += "	<td>" + getDepartName(deptID, deptXML) + "</td>";				
					response += "	<td>" + office + "</td>";				
					response += "	<td><a href=\"mailto:" + email +  "\">" + emailText + "</a></td>";		
					response += "	<td>" + phone + "</td>";		
					response += "</tr>";										
				}					
			}
			catch (Exception e)
			{
				response = "Faculty profiles cannot be displayed at this time. Please try again later.";
			}
			
			return response;											
		}	
		
		public static string getDepartName(string deptID, string deptXML) { 
			string deptName = "";
			XmlDocument xdcDocument = new XmlDocument();
			xdcDocument.Load(@deptXML);
			XmlElement xelRoot = xdcDocument.DocumentElement;
			XmlNodeList deptNodes = xelRoot.SelectNodes("/Departments/DEPTS");
			foreach (XmlNode dept in deptNodes) {					
				string currentID = dept["ftvorgn_orgn_code"].InnerText;
				string currentDeptName = dept["ftvorgn_title"].InnerText;
				bool deptCompare = String.Equals(currentID, deptID);
				if (deptCompare == true) {
					deptName += currentDeptName;
					break;
				}				
			}
			return deptName;
		}	
	}		
}
