using Aephy.API.DBHelper;
using Aephy.API.Models;
using Aephy.Helper.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using System.Net.Mail;
using System.Reflection.Emit;

namespace Aephy.API.AlgorithumHelper;

public class FreelancerFinderHelper
{
    public async Task FindFreelancersAsync(AephyAppDbContext db, string? clientId, string? projectType, int solutionId, int IndustryId, int totalProjectManager, int totalExpert, int totalAssociate)
    {
        //=== Check Stage for header and detail data ===//
        var freelancerFindProcessHeader = db.FreelancerFindProcessHeader.Where(x => x.ClientId == clientId && x.ProjectType == projectType && x.IndustryId == IndustryId && x.SolutionId == solutionId).FirstOrDefault();
        var freelancerFindProcessDetail = db.FreelancerFindProcessDetails.ToList();
        if (freelancerFindProcessHeader?.IsTeamCompleted == false || freelancerFindProcessHeader?.IsTeamCompleted == null)
        {
            //=== Update Score baised on review for freelancer ===//
            await GetRankedWiseFreeLancerList(db, solutionId, IndustryId, clientId);
            try
            {
                //=== Find All users with details and merge it. ====//
                var users = await db.Users.ToArrayAsync();
                var details = await db.FreelancerDetails.ToArrayAsync();
                var mergedArray = users.Join(details, user => user.Id, detail => detail.UserId,
                    (user, detail) => new
                    {
                        UserId = user.Id,
                        StartHoursFinal = user.StartHoursFinal,
                        EndHoursFinal = user.EndHoursFinal,
                        FreelancerDetail = detail,
                        Email = user.Email
                    }).ToArray();


                //=== Find client details ====//
                var clientDetails = users.Where(x => x.Id == clientId).FirstOrDefault();
                if (clientDetails != null)
                {
                    //=== Lable for make loop when add hours in client time ===//
                    int runningStatus = 0;
                Label:
                    //=== Count for project wise freelancer requirements. ====//
                    int projectManager = 0, associate = 0, expert = 0;
                    int projectManagerOld = 0, associateOld = 0, expertOld = 0;
                    if (projectType == "small")
                    {
                        associate = 1;
                        expert = 1;
                        associateOld = 1;
                        expertOld = 1;
                    }
                    else if (projectType == "medium")
                    {
                        projectManager = 1;
                        associate = 1;
                        expert = 1;
                        projectManagerOld = 1;
                        associateOld = 1;
                        expertOld = 1;
                    }
                    else if (projectType == "large")
                    {
                        projectManager = 1;
                        associate = 2;
                        expert = 2;
                        projectManagerOld = 1;
                        associateOld = 2;
                        expertOld = 2;
                    }
                    else if (projectType == "custom")
                    {
                        projectManager = totalProjectManager;
                        associate = totalAssociate;
                        expert = totalExpert;
                        projectManagerOld = totalProjectManager;
                        associateOld = totalAssociate;
                        expertOld = totalExpert;
                    }

                    //=== Genrate freelancers list depend on client working time and exclude alrady approve project ===//
                    var approvedDetailIds = freelancerFindProcessDetail.Where(x => x.ApproveStatus > 0).Select(x => x.FreelancerId).ToList();
                    var freelancers = mergedArray.Where(x => x.StartHoursFinal.HasValue ? x.StartHoursFinal.Value.TimeOfDay == clientDetails.StartHours.Value.TimeOfDay : false &&
                                                       x.EndHoursFinal.HasValue ? x.EndHoursFinal.Value.TimeOfDay == clientDetails.EndHours.Value.TimeOfDay : false).ToArray();
                    if (approvedDetailIds.Any())
                    {
                        freelancers = freelancers.Where(x => !approvedDetailIds.Contains(x.UserId)).ToArray();
                    }

                    //=== Genrate Project Startdate and End Date Baised on Milestone ===//
                    int projectCompletedTime = db.SolutionMilestone.Where(x => x.IndustryId == IndustryId && x.SolutionId == solutionId && x.ProjectType == projectType).Select(x => x.Days).Sum();
                    DateTime startDate = DateTime.Now;
                    DateTime endDate = startDate.AddDays(projectCompletedTime);

                    //=== Find Solution and Industry details for email and Genrate Email body baised on html template ===//
                    var solutionTitle = await db.Solutions.Where(x => x.Id == solutionId).Select(x => x.Title).FirstOrDefaultAsync();
                    var industryName = await db.Industries.Where(x => x.Id == IndustryId).Select(x => x.IndustryName).FirstOrDefaultAsync();

                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string fileName = "NotificationTemplate.html";
                    string filePath = Path.Combine(currentDirectory.Replace("\\bin\\Debug\\net7.0", "\\AlgorithumHelper"), fileName);
                    string body = File.ReadAllText(filePath);
                    body = body.Replace("{{ project_name }}", solutionTitle);
                    body = body.Replace("{{ industry_name }}", industryName);
                    body = body.Replace("{{ duration }}", Convert.ToString(projectCompletedTime) + "Days");
                    body = body.Replace("{{ size }}", projectType);

                    //=== Find Freelancer which is alrady in leave baised on duration ===//
                    var excludeDateFreelancer = db.FreelancerExcludeDate.Where(exclude => exclude.ExcludeDate < startDate || exclude.ExcludeDate > endDate).ToList();
                    int maxLeaveDays = 4;
                    if (excludeDateFreelancer.Any())
                    {
                        //=== Find Freelancers leave count baised on exclude date data ===//
                        var invalidFreelancerIds = excludeDateFreelancer.GroupBy(exclude => exclude.FreelancerId)
                            .Where(group => group.Count() >= maxLeaveDays).Select(group => group.Key).ToList();

                        //=== Remove freelancer which is alrady more then 4 leave during given timeline ===//
                        if (invalidFreelancerIds.Any())
                        {
                            freelancers = freelancers.Where(x => !invalidFreelancerIds.Contains(x.UserId)).ToArray();
                        }
                    }

                    if (freelancers.Count() > 0)
                    {
                        //=== Find Algorithum Current stage and according stage find freelancer ===//

                        int algorithumStage = freelancerFindProcessHeader == null ? 0 : freelancerFindProcessHeader.CurrentAlgorithumStage;

                        if (algorithumStage == 1)
                        {
                            projectManager = projectManager * 3;
                            associate = associate * 3;
                            expert = expert * 3;
                        }
                        else if (algorithumStage == 2)
                        {
                            projectManager = projectManager * 5;
                            associate = associate * 5;
                            expert = expert * 5;
                        }
                        else if (algorithumStage == 3)
                        {
                            projectManager = projectManager * 10;
                            associate = associate * 10;
                            expert = expert * 10;
                        }
                        else if (algorithumStage > 0)
                        {
                            projectManager = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Project Manager").Count();
                            expert = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Expert").Count();
                            associate = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Associate").Count();
                        }

                        //=== Getting freelancers baised on ratting score ====//
                        var projectManagersArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Project Manager").ToArray();
                        var rankedProjectManager = projectManagersArray.GroupBy(b => b.FreelancerDetail.Score)
                            .OrderByDescending(g => g.Key)
                            .SelectMany((item, index) => item.Select(inner =>
                                new FreelancerDetailModel
                                {
                                    UserId = inner.UserId,
                                    Score = item.Key,
                                    Ranking = index + 1,
                                    Email = inner.Email
                                })).Take(projectManager).ToList();

                        var expertsArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Expert").ToArray();
                        var rankedExpertsArray = expertsArray.GroupBy(b => b.FreelancerDetail.Score)
                            .OrderByDescending(g => g.Key)
                            .SelectMany((item, index) => item.Select(inner =>
                                new FreelancerDetailModel
                                {
                                    UserId = inner.UserId,
                                    Score = item.Key,
                                    Ranking = index + 1,
                                    Email = inner.Email
                                })).Take(expert).ToList();

                        var associatesArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Associate").Take(associate).ToArray();
                        var rankedAssociatesArray = associatesArray.GroupBy(b => b.FreelancerDetail.Score)
                            .OrderByDescending(g => g.Key)
                            .SelectMany((item, index) => item.Select(inner =>
                                new FreelancerDetailModel
                                {
                                    UserId = inner.UserId,
                                    Score = item.Key,
                                    Ranking = index + 1,
                                    Email = inner.Email
                                })).Take(associate).ToList();

                        //=== Save and Update Header and Header Details ===//
                        if (freelancerFindProcessHeader == null)
                        {
                            //=== Save header with algorithum stage ====//
                            var header = new FreelancerFindProcessHeader
                            {
                                ClientId = clientId,
                                ProjectType = projectType,
                                SolutionId = solutionId,
                                IndustryId = IndustryId,
                                CurrentAlgorithumStage = 1,
                                TotalProjectManager = projectManager,
                                TotalAssociate = associate,
                                TotalExpert = expert,
                                StartDate = DateTime.Now,
                                ExecuteDate = DateTime.Now,
                                IsTeamCompleted = false,
                                CurrentStatus = 0
                            };

                            var headerModel = await db.FreelancerFindProcessHeader.AddAsync(header);
                            await db.SaveChangesAsync();

                            //=== Save header details with algorithum stage ====//
                            var detailModelList = new List<FreelancerFindProcessDetails>();
                            var emailIds = new List<string>();

                            foreach (var item in rankedProjectManager)
                            {
                                var pmodel = new FreelancerFindProcessDetails
                                {
                                    FreelancerFindProcessHeaderId = headerModel.Entity.Id,
                                    AlgorithumStage = 1,
                                    FreelancerType = "Project Manager",
                                    FreelancerId = item.UserId,
                                    ApproveStatus = 0,
                                    CreatedDate = DateTime.Now,
                                };
                                detailModelList.Add(pmodel);
                                emailIds.Add(item.Email);
                            }
                            foreach (var item in rankedExpertsArray)
                            {
                                var emodel = new FreelancerFindProcessDetails
                                {
                                    FreelancerFindProcessHeaderId = headerModel.Entity.Id,
                                    AlgorithumStage = 1,
                                    FreelancerType = "Expert",
                                    FreelancerId = item.UserId,
                                    ApproveStatus = 0,
                                    CreatedDate = DateTime.Now,
                                };
                                detailModelList.Add(emodel);
                                emailIds.Add(item.Email);
                            }
                            foreach (var item in rankedAssociatesArray)
                            {
                                var amodel = new FreelancerFindProcessDetails
                                {
                                    FreelancerFindProcessHeaderId = headerModel.Entity.Id,
                                    AlgorithumStage = 1,
                                    FreelancerType = "Associate",
                                    FreelancerId = item.UserId,
                                    ApproveStatus = 0,
                                    CreatedDate = DateTime.Now,
                                };
                                detailModelList.Add(amodel);
                                emailIds.Add(item.Email);
                            }

                            if (detailModelList.Any())
                            {
                                await db.FreelancerFindProcessDetails.AddRangeAsync(detailModelList);
                                await db.SaveChangesAsync();

                                //After save send mail to freelancer.
                                foreach (var item in emailIds)
                                {
                                    bool send = SendEmailHelper.SendEmail(item, "Project Invitation - Time Sensitive", body);
                                }
                            }
                        }
                        else
                        {
                            //=== Update header with algorithum stage ====//
                            var headerDetailData = freelancerFindProcessDetail.Where(x => x.FreelancerFindProcessHeaderId == freelancerFindProcessHeader.Id).ToList();

                            freelancerFindProcessHeader.CurrentAlgorithumStage = algorithumStage + 1;
                            freelancerFindProcessHeader.ExecuteDate = DateTime.Now;

                            var oldFreelancerInDetails = headerDetailData.Select(x => x.FreelancerId).ToList();
                            var detailModelList = new List<FreelancerFindProcessDetails>();
                            var emailIds = new List<string>();

                            //=== Remove freelancer which is alrady in detail section ===//
                            rankedProjectManager = rankedProjectManager.Where(rp => !oldFreelancerInDetails.Contains(rp.UserId)).Take(projectManager).ToList();
                            rankedExpertsArray = rankedExpertsArray.Where(rp => !oldFreelancerInDetails.Contains(rp.UserId)).Take(expert).ToList();
                            rankedAssociatesArray = rankedAssociatesArray.Where(rp => !oldFreelancerInDetails.Contains(rp.UserId)).Take(associate).ToList();

                            var headerModel = db.FreelancerFindProcessHeader.Update(freelancerFindProcessHeader);
                            await db.SaveChangesAsync();

                            //=== Save header details with algorithum stage ====//
                            int currentPCount = headerDetailData.Where(pc => pc.FreelancerType == "Project Manager" && pc.ApproveStatus == 1).Count();
                            int currentECount = headerDetailData.Where(pc => pc.FreelancerType == "Expert" && pc.ApproveStatus == 1).Count();
                            int currentACount = headerDetailData.Where(pc => pc.FreelancerType == "Associate" && pc.ApproveStatus == 1).Count();

                            if (currentPCount != projectManagerOld)
                            {
                                foreach (var item in rankedProjectManager)
                                {
                                    var pmodel = new FreelancerFindProcessDetails
                                    {
                                        FreelancerFindProcessHeaderId = headerModel.Entity.Id,
                                        AlgorithumStage = algorithumStage + 1,
                                        FreelancerType = "Project Manager",
                                        FreelancerId = item.UserId,
                                        ApproveStatus = 0,
                                        CreatedDate = DateTime.Now,
                                    };
                                    detailModelList.Add(pmodel);
                                    emailIds.Add(item.Email);
                                }
                            }

                            if (currentECount != expertOld)
                            {
                                foreach (var item in rankedExpertsArray)
                                {
                                    var emodel = new FreelancerFindProcessDetails
                                    {
                                        FreelancerFindProcessHeaderId = headerModel.Entity.Id,
                                        AlgorithumStage = algorithumStage + 1,
                                        FreelancerType = "Expert",
                                        FreelancerId = item.UserId,
                                        ApproveStatus = 0,
                                        CreatedDate = DateTime.Now,
                                    };
                                    detailModelList.Add(emodel);
                                    emailIds.Add(item.Email);
                                }
                            }

                            if (currentACount != associateOld)
                            {
                                foreach (var item in rankedAssociatesArray)
                                {
                                    var amodel = new FreelancerFindProcessDetails
                                    {
                                        FreelancerFindProcessHeaderId = headerModel.Entity.Id,
                                        AlgorithumStage = algorithumStage + 1,
                                        FreelancerType = "Associate",
                                        FreelancerId = item.UserId,
                                        ApproveStatus = 0,
                                        CreatedDate = DateTime.Now,
                                    };
                                    detailModelList.Add(amodel);
                                    emailIds.Add(item.Email);
                                }
                            }

                            if (detailModelList.Any())
                            {
                                await db.FreelancerFindProcessDetails.AddRangeAsync(detailModelList);
                                await db.SaveChangesAsync();

                                //After save send mail to freelancer.
                                foreach (var item in emailIds)
                                {
                                    bool send = SendEmailHelper.SendEmail(item, "Project Invitation - Time Sensitive", body);
                                }
                            }
                        }
                    }
                    else
                    {
                        clientDetails.StartHours.Value.AddHours(2);
                        clientDetails.EndHours.Value.AddHours(2);

                        runningStatus++;

                        if (runningStatus <= 5)
                        {
                            goto Label;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public async Task GetRankedWiseFreeLancerList(AephyAppDbContext db, int solutionId, int IndustryId, string? clientId)
    {
        //== Get RowData from Table ==//
        var rowData = await db.FeatureWiseRanking.Where(x => x.FeatureChildId == null).Select(x => new FeatureWiseRankingModel
        {
            Id = x.Id,
            Feature = x.Feature,
            FeatureChildId = x.FeatureChildId,
            Ranking = x.Ranking,
            FeatureChild = db.FeatureWiseRanking.Where(y => y.FeatureChildId == x.Id).Select(y => new FeatureWiseRankingModel
            {
                Id = y.Id,
                Feature = y.Feature,
                FeatureChildId = y.FeatureChildId,
                Ranking = y.Ranking,
                ParantFeature = x.Feature
            }).ToList(),
        }).ToListAsync();


        //=== Calculate Weight for perticular Feature (review paramater) ===//
        foreach (var row in rowData)
        {
            row.Weight = Math.Round(Convert.ToDecimal(row.Ranking) / Convert.ToDecimal(rowData.Sum(x => x.Ranking)) * 100, 1);

            foreach (var item in row.FeatureChild)
            {
                item.Weight = Math.Round(Convert.ToDecimal(item.Ranking) / Convert.ToDecimal(row.FeatureChild.Sum(x => x.Ranking)) * 100, 1);
            }
        }


        //=== Get Project Manager Score From Score Table ===//
        var freeLancers = await db.FreelancerDetails.ToListAsync();
        var finalRankedListData = new List<FreelancerDetailModel>();
        var freeLanceWiseScoreList = new List<FreelancerDetailModel>();
        var freeLanceRankedWiseModelList = new List<FreelancerDetailModel>();

        //=== Genrate Platform Review baised on freelancer hourly rate ===//
        var freelancerLevels = new[] { "Project Manager", "Expert", "Associate" };
        int maxRank = 10, minRange = 0, maxRange = 0;

        foreach (var level in freelancerLevels)
        {
            var filteredFreelancers = freeLancers
                .Where(x => x.FreelancerLevel == level)
                .Select(x => new FreelancerDetailModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FreelancerLevel = x.FreelancerLevel,
                    HourlyRate = x.HourlyRate
                }).ToList();

            minRange = 0;
            maxRange = 0;

            if (filteredFreelancers.Any())
            {
                var sortedFreelancers = filteredFreelancers.OrderBy(x => x.HourlyRate).ToList();
                minRange = Convert.ToInt32(sortedFreelancers.Min(x => x.HourlyRate));
                maxRange = Convert.ToInt32(sortedFreelancers.Max(x => x.HourlyRate));

                foreach (var item in sortedFreelancers)
                {
                    item.Ranking = GetRange(Convert.ToInt32(item.HourlyRate), minRange, maxRange);
                }
                freeLanceRankedWiseModelList.AddRange(sortedFreelancers);
            }
        }

        var updateList = new List<AdminToFreelancerReview>();
        var addList = new List<AdminToFreelancerReview>();

        foreach (var freelancerDetailModel in freeLanceRankedWiseModelList)
        {
            var existingRecord = db.AdminToFreelancerReview.FirstOrDefault(x => x.FreelancerId == freelancerDetailModel.UserId);
            if (existingRecord != null)
            {
                existingRecord.HourlyRate = freelancerDetailModel.Ranking;
                updateList.Add(existingRecord);
            }
            else
            {
                var newRecord = new AdminToFreelancerReview
                {
                    FreelancerId = freelancerDetailModel.UserId,
                    HourlyRate = freelancerDetailModel.Ranking,
                    UserId = clientId,
                    Availability = 0,
                    Professionalism = 0,
                    ProjectAcceptance = 0,
                    Education = 0,
                    SoftSkillsExperience = 0,
                    HardSkillsExperience = 0,
                    ProjectSuccessRate = 0,
                    CreateDateTime = DateTime.Now,
                };
                addList.Add(newRecord);
            }
        }

        if (updateList.Any())
        {
            db.AdminToFreelancerReview.UpdateRange(updateList);
            await db.SaveChangesAsync();
            freeLanceRankedWiseModelList.Clear();
            updateList.Clear();
        }

        if (addList.Any())
        {
            await db.AdminToFreelancerReview.AddRangeAsync(addList);
            await db.SaveChangesAsync();
            freeLanceRankedWiseModelList.Clear();
            addList.Clear();
        }

        //=== Genrate Project Acceptance score ===//
        var acceptFreelancerList = db.FreelancerFindProcessDetails.Where(x => x.ApproveStatus == 1)
            .GroupBy(x => x.FreelancerId)
            .Select(g => new
            {
                FreelancerId = g.Key,
                Count = g.Count(),
                FreelancerLevel = g.First().FreelancerType,
            }).ToList();

        foreach (var level in freelancerLevels)
        {
            var filteredFreelancers = acceptFreelancerList
                .Where(x => x.FreelancerLevel == level)
                .Select(x => new FreelancerDetailModel
                {
                    UserId = x.FreelancerId,
                    FreelancerLevel = x.FreelancerLevel,
                    ProjectAcceptance = x.Count
                }).ToList();

            minRange = 0;
            maxRange = 0;

            if (filteredFreelancers.Any())
            {
                var sortedFreelancers = filteredFreelancers.OrderBy(x => x.ProjectAcceptance).ToList();
                minRange = Convert.ToInt32(sortedFreelancers.Min(x => x.ProjectAcceptance));
                maxRange = Convert.ToInt32(sortedFreelancers.Max(x => x.ProjectAcceptance));

                foreach (var item in sortedFreelancers)
                {
                    item.Ranking = GetRange(Convert.ToInt32(item.HourlyRate), minRange, maxRange);
                }
                freeLanceRankedWiseModelList.AddRange(sortedFreelancers);
            }
        }

        foreach (var freelancerDetailModel in freeLanceRankedWiseModelList)
        {
            var existingRecord = db.AdminToFreelancerReview.FirstOrDefault(x => x.FreelancerId == freelancerDetailModel.UserId);
            if (existingRecord != null)
            {
                existingRecord.ProjectAcceptance = freelancerDetailModel.Ranking;
                updateList.Add(existingRecord);
            }
            else
            {
                var newRecord = new AdminToFreelancerReview
                {
                    FreelancerId = freelancerDetailModel.UserId,
                    HourlyRate = 0,
                    UserId = clientId,
                    Availability = 0,
                    Professionalism = 0,
                    ProjectAcceptance = freelancerDetailModel.Ranking,
                    Education = 0,
                    SoftSkillsExperience = 0,
                    HardSkillsExperience = 0,
                    ProjectSuccessRate = 0,
                    CreateDateTime = DateTime.Now,
                };
                addList.Add(newRecord);
            }
        }

        if (updateList.Any())
        {
            db.AdminToFreelancerReview.UpdateRange(updateList);
            await db.SaveChangesAsync();
            freeLanceRankedWiseModelList.Clear();
            updateList.Clear();
        }

        if (addList.Any())
        {
            await db.AdminToFreelancerReview.AddRangeAsync(addList);
            await db.SaveChangesAsync();
            freeLanceRankedWiseModelList.Clear();
            addList.Clear();
        }

        //=== Assign score baised on review ===//
        foreach (var freelance in freeLancers)
        {
            var oldScore = await db.FreelancerDetails.Where(x => x.UserId == freelance.UserId).ToListAsync();

            //=== Find Review which is given by client ===//
            var freeLancerReviews = await db.FreelancerReview.Where(x => x.FreelancerId == freelance.UserId).ToListAsync();
            var projectReview = await db.ProjectReview.Where(x => x.ClientId == clientId).ToListAsync();
            var freelancerTofreelancerReviews = await db.FreelancerToFreelancerReview.Where(x => x.FromFreelancerId == freelance.UserId).ToListAsync();
            var adminToFreelancerReview = await db.AdminToFreelancerReview.Where(x => x.FreelancerId == freelance.UserId).ToListAsync();

            int currentClientReview = 25;
            int newClientReview = currentClientReview + 1;

            //=== Calculate client reviews baised on Feature ratting ===//
            foreach (var row in rowData)
            {
                foreach (var item in row.FeatureChild)
                {
                    //=== Feature wise rating sum and score ===//

                    if (item.ParantFeature.TrimEnd() == "Customer Reviews / score assigned by client")
                    {
                        if (item.Feature == "Communication skills")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.CommunicationRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.CommunicationRating)) ?? 9;
                        }
                        else if (item.Feature == "Collaboration skills")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.CollaborationRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.CollaborationRating)) ?? 9;
                        }
                        else if (item.Feature == "Professionalism")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.ProfessionalismRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.ProfessionalismRating)) ?? 9;
                        }
                        else if (item.Feature == "Technical Skills")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.TechnicalRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.TechnicalRating)) ?? 9;
                        }
                        else if (item.Feature == "Client Satisfaction")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.SatisfactionRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.SatisfactionRating)) ?? 9;
                        }
                        else if (item.Feature == "Responsiveness")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.ResponsivenessRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.ResponsivenessRating)) ?? 9;
                        }
                        else if (item.Feature == "Freelancer Like To Work")
                        {
                            item.RatingSum = (freeLancerReviews.Sum(x => x.LikeToWorkRating) == 0 ? 9 : freeLancerReviews.Sum(x => x.LikeToWorkRating)) ?? 9;
                        }
                    }

                    if (item.ParantFeature.TrimEnd() == "Project Reviews / score assigned by client")
                    {
                        if (item.Feature == "Well-Defined Project Scope")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.WellDefinedProjectScope) == 0 ? 9 : projectReview.Sum(x => x.WellDefinedProjectScope)) ?? 9;
                        }
                        else if (item.Feature == "Adherence to Project Scope")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.AdherenceToProjectScope) == 0 ? 9 : projectReview.Sum(x => x.AdherenceToProjectScope)) ?? 9;
                        }
                        else if (item.Feature == "Quality of Deliverables")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.DeliverablesQuality) == 0 ? 9 : projectReview.Sum(x => x.DeliverablesQuality)) ?? 9;
                        }
                        else if (item.Feature == "Meeting Timeliness")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.MeetingTimeliness) == 0 ? 9 : projectReview.Sum(x => x.MeetingTimeliness)) ?? 9;
                        }
                        else if (item.Feature == "Client Satisfaction")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.Clientsatisfaction) == 0 ? 9 : projectReview.Sum(x => x.Clientsatisfaction)) ?? 9;
                        }
                        else if (item.Feature == "Adherence to Budget")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.AdherenceToBudget) == 0 ? 9 : projectReview.Sum(x => x.AdherenceToBudget)) ?? 9;
                        }
                        else if (item.Feature == "Would you recommend the project to a partner/affiliate company?")
                        {
                            item.RatingSum = (projectReview.Sum(x => x.LikeToRecommend) == 0 ? 9 : projectReview.Sum(x => x.LikeToRecommend)) ?? 9;
                        }
                        //else if (item.Feature == "Number of reviews")
                        //{
                        //    item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.Clientsatisfaction) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.Clientsatisfaction)) ?? 9;
                        //}
                    }

                    if (item.ParantFeature.TrimEnd() == "Score between freelancers")
                    {
                        if (item.Feature == "Collaboration and Teamwork")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.CollaborationAndTeamWork) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.CollaborationAndTeamWork));
                        }
                        else if (item.Feature == "Communication Skills")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.Communication) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.Communication));
                        }
                        else if (item.Feature == "Professionalism")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.Professionalism) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.Professionalism));
                        }
                        else if (item.Feature == "Technical Skills")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.TechnicalSkills) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.TechnicalSkills));
                        }
                        else if (item.Feature == "Project Management")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.ProjectManagement) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.ProjectManagement));
                        }
                        else if (item.Feature == "Responsiveness")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.Responsiveness) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.Responsiveness));
                        }
                        else if (item.Feature == "Well-Defined Project Scope")
                        {
                            item.RatingSum = (freelancerTofreelancerReviews.Sum(x => x.WellDefinedProjectScope) == 0 ? 9 : freelancerTofreelancerReviews.Sum(x => x.WellDefinedProjectScope));
                        }
                    }

                    if (item.ParantFeature.TrimEnd() == "Score assigned by the Platform")
                    {
                        if (item.Feature == "Professionalism")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.Professionalism) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.Professionalism)) ?? 9;
                        }
                        else if (item.Feature == "Hourly Rate")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.HourlyRate) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.HourlyRate)) ?? 9;
                        }
                        else if (item.Feature == "Availability")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.Availability) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.Availability)) ?? 9;
                        }
                        else if (item.Feature == "Project Acceptance")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.ProjectAcceptance) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.ProjectAcceptance)) ?? 9;
                        }
                        else if (item.Feature == "Education")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.Education) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.Education)) ?? 9;
                        }
                        else if (item.Feature == "Experience (Soft skills)")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.SoftSkillsExperience) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.SoftSkillsExperience)) ?? 9;
                        }
                        else if (item.Feature == "Experience (Hard skills)")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.HardSkillsExperience) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.HardSkillsExperience)) ?? 9;
                        }
                        else if (item.Feature == "Project Success Rate")
                        {
                            item.RatingSum = (adminToFreelancerReview.Sum(x => x.ProjectSuccessRate) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.ProjectSuccessRate)) ?? 9;
                        }

                        //else if (item.Feature == "Response Time")
                        //{
                        //    item.RatingSum = (adminToFreelancerReview.Sum(x => x.T) == 0 ? 9 : adminToFreelancerReview.Sum(x => x.Professionalism)) ?? 9;
                        //}
                        //else if (item.Feature == "Well-Defined Project Scope")
                        //{

                        //}
                    }

                    //=== Getting old score from database and use in calculation if null then default 0 ===//
                    var oldFeatureRanking = oldScore.FirstOrDefault();
                    item.TempScore = Math.Round(Convert.ToDecimal((currentClientReview * (item.RatingSum + (oldFeatureRanking != null ? oldFeatureRanking.Score : 1)))) / newClientReview, 2);
                    item.TempWeight = Math.Round((item.TempScore * item.Weight) / 100, 2);

                    //=== Genrate FreeLance wise score list for save data in database ===//
                    var freelanceWiseScore = new FreelancerDetailModel
                    {
                        Id = freelance.Id,
                        UserId = freelance.UserId,
                        Score = item.TempScore
                    };
                    freeLanceWiseScoreList.Add(freelanceWiseScore);
                }

                //=== Find Total Weight baised on review score calculation ===//
                row.TempScore = Math.Round(row.FeatureChild.Sum(x => x.TempWeight), 2);
                row.TempWeight = Math.Round((row.TempScore * row.Weight) / 100, 2);
            }

            //=== Genrate Final Score wise list ===//
            var finalData = new FreelancerDetailModel
            {
                Id = freelance.Id,
                UserId = freelance.UserId,
                Score = rowData.Sum(x => x.TempWeight)
            };

            finalRankedListData.Add(finalData);
        }

        //=== Save FreeLance wise score list ===// 
        var modelList = new List<FreelancerDetails>();
        foreach (var item in finalRankedListData)
        {
            var dbdata = freeLancers.Where(x => x.Id == item.Id).FirstOrDefault();
            dbdata.Score = item.Score;
            modelList.Add(dbdata);
        }
        db.FreelancerDetails.UpdateRange(modelList);
        await db.SaveChangesAsync();
    }


    //=== This function use for a genrate upper and lower boundry and assign rank baised on boundry ===//
    static int GetRange(int number, int minRange, int maxRange)
    {
        int min = minRange;
        int max = maxRange;
        int maxRank = 10;
        int rangeSize = (max - min + 1) / maxRank;

        for (int i = 1; i <= maxRank; i++)
        {
            int lowerBound = min + (i - 1) * rangeSize;
            int upperBound = min + i * rangeSize;

            if (number >= lowerBound && number <= upperBound)
            {
                return i;
            }
        }
        return 10; // Return 10 if the number doesn't fall into any range.
    }
}
