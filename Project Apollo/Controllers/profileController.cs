﻿using Project_Apollo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_Apollo.Controllers {
	public class profileController : Controller {
		DBase db = new DBase();

		// GET: profile
		public ActionResult Index() {
			//Session["id"] = 1; TESTING ONLY
			User user = db.userTable.Find((int)Session["id"]);

			//TESTING ONLY
			Session["userRole"] = (int)user.UserRole;
			Session["userPhoneNumber"] = user.Mobile;
			Session["userEmail"] = user.Email;
			Session["userDescription"] = user.Description;

			//Choosing layout depends on user role
			if ((int)Session["userRole"] == 0 || (int)Session["userRole"] == 2 || (int)Session["userRole"] == 3) {
				//If the user is admin, customer or project manager
				ViewBag.showNav = false;
			} else {
				//If the user is team leader or jenior engineer
				ViewBag.showNav = true;
			}

			//Loading tabs depends on user role
			switch ((int)Session["userRole"]) {
				case 0:
					ViewBag.tabs = new string[2] {"Projects", "User Managment"};
					ViewBag.tabAttr = new string[2] {"projects" , "user-management" };
					break;
                case 1:
                    ViewBag.tabs = new string[2] { "Projects", "Accept requests" };
					ViewBag.tabAttr = new string[2] { "projects", "accept-request" };
					break;
                case 2:
                    ViewBag.tabs = new string[3] { "Projects", "Send request", "Accept requests" };
					ViewBag.tabAttr = new string[3] { "projects", "send-request", "accept-request" };
					break;
				case 3:
					ViewBag.tabs = new string[] { "Projects", "Accept request" };
					ViewBag.tabAttr = new string[2] { "projects", "accept-request" };
					break;
				case 4:
					ViewBag.tabs = new string[] { "Projects", "Accept request" };
					ViewBag.tabAttr = new string[2] { "projects", "accept-request" };
					break;
			}			
			

			if (user.Photo == null) {
				Session["userPhoto"] = "/Public/assets/images/default-user.jpg";
			} else {
				var img = ImageConverter.convertPhotoToString(user.Photo);
				Session["userPhoto"] = img;
			}

			Session["userName"] = user.name;
			return View(this.loadAssignedProjects((int)Session["id"]));
		}


		public void declineProject(int projectID) {
			var Result = new HomeController().deleteProject(projectID);
		}
		public List<User> getTeamLeaders() {
			return db.userTable.Where(u => (int)u.UserRole == 3).ToList();
		}
		public List<User> getJuniorEngineers() {
			return db.userTable.Where(u => (int)u.UserRole == 4).ToList();
		}
		public void approveProject(int projectId) {
			Project p = db.ProjectTable.Find(projectId);
			p.status = (status)0;
			db.SaveChanges();
		}
		public void removeMember(int userId = 1, int projectId = 6) {
			Project p = db.ProjectTable.Find(projectId);
			User u = db.userTable.Find(userId);
			p.workers.Remove(u);
			db.SaveChanges();
		}

		public object loadPendingProjects() {
			int status = 3;
			List<Project> projects = db.ProjectTable.Where(x => ((int)x.status) == status).ToList();
			return View(projects);
		}
        public void deleteUser(int id)
        {
            User u = db.userTable.Find(id);
            if (u.UserRole == (userRole)1) // customer
            {
                foreach (Project p in u.ownProject.ToList())
                {
                    db.ProjectTable.Remove(p);
                }
            }
            else if (u.UserRole == (userRole)2)//projectManager
            {
                foreach (Project p in u.manageProject.ToList())
                {
                    db.Entry(p).Reference("projectManager").CurrentValue = null;
                }
            }
            else if (u.UserRole == (userRole)3)//teamLeader
            {
                foreach (Project p in u.leadProject.ToList())
                {
                    db.Entry(p).Reference("teamLeader").CurrentValue = null;
                }
            }
            else if (u.UserRole == (userRole)3)//juniorEngineer
            {

            }
            foreach (Qualifications q in u.Qualifications.ToList())
            {
                db.qualificationsTable.Remove(q);
            }
            db.SaveChanges();
        }
        public void requestTeamLeader(int projectid = 2, int teamLeaderId = 1)
        {
            db.RequestsTable.Add(new Requests
            {
                project = db.ProjectTable.Find(projectid),
                sender = db.userTable.Find((int)Session["id"]),
                reciever = db.userTable.Find(teamLeaderId),
                requestType = (request)0
            });
            db.SaveChanges();
        }
        public void requestJuniorEngineer(int projectid = 2, int JuniorId = 1)
        {
            db.RequestsTable.Add(new Requests
            {
                project = db.ProjectTable.Find(projectid),
                sender = db.userTable.Find((int)Session["id"]),
                reciever = db.userTable.Find(JuniorId),
                requestType = (request)1
            });
            db.SaveChanges();
        }
        public object loadAssignedProjects(int userId)
        {
            var arr = db.ProjectTable.Where(x => ((int)x.status) == 1 && (x.projectManager.ID == userId || x.teamLeader.ID == userId || x.workers.Any(ss=>ss.ID == userId)||x.customer.ID == userId)).ToList();
            return arr;
        }
    }

}
