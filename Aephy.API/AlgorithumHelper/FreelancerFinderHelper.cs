using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Aephy.API.AlgorithumHelper;

public class FreelancerFinderHelper
{
    public async Task FindFreelancersAsync(AephyAppDbContext db, string? clientId, string? projectType, int solutionId, int IndustryId)
    {
        //=== Check Algorithum stage.
        var freelancerFindProcessHeader = db.FreelancerFindProcessHeader.Where(x => x.ClientId == clientId && x.ProjectType == projectType && x.IndustryId == IndustryId && x.SolutionId == solutionId).FirstOrDefault();
        if (freelancerFindProcessHeader?.IsTeamCompleted == false || freelancerFindProcessHeader?.IsTeamCompleted == null)
        {
            //=== Update Score baised on review for freelancer ===//
            await GetRankedWiseFreeLancerList(db);
            try
            {
                //=== Count for project wise freelancer requirements. ====//
                int projectManager = 0, associate = 0, expert = 0;
                if (projectType == "small")
                {
                    associate = 1;
                    expert = 1;
                }
                else if (projectType == "medium")
                {
                    projectManager = 1;
                    associate = 1;
                    expert = 1;
                }
                else if (projectType == "large")
                {
                    projectManager = 1;
                    associate = 2;
                    expert = 1;
                }

                //=== Find All users with details and merge it. ====//
                var users = await db.Users.ToArrayAsync();
                var details = await db.FreelancerDetails.ToArrayAsync();
                var mergedArray = users.Join(details, user => user.Id, detail => detail.UserId,
                    (user, detail) => new
                    {
                        UserId = user.Id,
                        StartHoursFinal = user.StartHoursFinal,
                        EndHoursFinal = user.EndHoursFinal,
                        FreelancerDetail = detail
                    }).ToArray();


                //=== Find client details ====//
                var clientDetails = users.Where(x => x.Id == clientId).FirstOrDefault();
                if (clientDetails != null)
                {
                    Label:
                    var freelancers = mergedArray.Where(x => x.StartHoursFinal.HasValue ? x.StartHoursFinal.Value.TimeOfDay == clientDetails.StartHoursFinal.Value.TimeOfDay : false &&
                                                       x.EndHoursFinal.HasValue ? x.EndHoursFinal.Value.TimeOfDay == clientDetails.EndHoursFinal.Value.TimeOfDay : false).ToArray();
                    if (freelancers != null)
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
                        else
                        {
                            projectManager = freelancers.Count();
                            associate = freelancers.Count();
                            expert = freelancers.Count();
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
                                    Ranking = index + 1
                                })).Take(projectManager).ToList();

                        var expertsArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Expert").ToArray();
                        var rankedExpertsArray = expertsArray.GroupBy(b => b.FreelancerDetail.Score)
                            .OrderByDescending(g => g.Key)
                            .SelectMany((item, index) => item.Select(inner =>
                                new FreelancerDetailModel
                                {
                                    UserId = inner.UserId,
                                    Score = item.Key,
                                    Ranking = index + 1
                                })).Take(expert).ToList();

                        var associatesArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Associate").Take(associate).ToArray();
                        var rankedAssociatesArray = associatesArray.GroupBy(b => b.FreelancerDetail.Score)
                            .OrderByDescending(g => g.Key)
                            .SelectMany((item, index) => item.Select(inner =>
                                new FreelancerDetailModel
                                {
                                    UserId = inner.UserId,
                                    Score = item.Key,
                                    Ranking = index + 1
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
                                IsTeamCompleted = false
                            };

                            var headerModel = await db.FreelancerFindProcessHeader.AddAsync(header);
                            await db.SaveChangesAsync();

                            //=== Save header details with algorithum stage ====//
                            var detailModelList = new List<FreelancerFindProcessDetails>();
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
                            }

                            await db.FreelancerFindProcessDetails.AddRangeAsync(detailModelList);
                            await db.SaveChangesAsync();

                            //After save send mail to freelancer.
                        }
                        else
                        {
                            //=== Update header with algorithum stage ====//
                            freelancerFindProcessHeader.CurrentAlgorithumStage = algorithumStage + 1;
                            freelancerFindProcessHeader.ExecuteDate = DateTime.Now;

                            var oldFreelancerInDetails = db.FreelancerFindProcessDetails.Where(x => x.FreelancerFindProcessHeaderId == freelancerFindProcessHeader.Id).Select(x => x.FreelancerId).ToList();
                            var detailModelList = new List<FreelancerFindProcessDetails>();


                            //=== Remove freelancer which is alrady in detail section ===//
                            rankedProjectManager = rankedProjectManager.Where(rp => !oldFreelancerInDetails.Contains(rp.UserId)).Take(projectManager).ToList();
                            rankedExpertsArray = rankedExpertsArray.Where(rp => !oldFreelancerInDetails.Contains(rp.UserId)).Take(expert).ToList();
                            rankedAssociatesArray = rankedAssociatesArray.Where(rp => !oldFreelancerInDetails.Contains(rp.UserId)).Take(associate).ToList();

                            var headerModel = db.FreelancerFindProcessHeader.Update(freelancerFindProcessHeader);
                            await db.SaveChangesAsync();

                            //=== Save header details with algorithum stage ====//
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
                            }
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
                            }
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
                            }

                            await db.FreelancerFindProcessDetails.AddRangeAsync(detailModelList);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        clientDetails.StartHours.Value.AddHours(2);
                        clientDetails.EndHours.Value.AddHours(2);
                        goto Label;
                        // 5 time loop repeat.
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    public async Task GetRankedWiseFreeLancerList(AephyAppDbContext db)
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
                Ranking = y.Ranking
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


        //== Get Project Manager Score From Score Table ==//
        var freeLancers = await db.FreelancerDetails.ToListAsync();
        var finalRankedListData = new List<FreelancerDetailModel>();
        var freeLanceWiseScoreList = new List<FreelancerDetailModel>();

        foreach (var freelance in freeLancers)
        {
            var oldScore = await db.FreelancerDetails.Where(x => x.UserId == freelance.UserId).ToListAsync();
            var freeLancerReviews = await db.FreelancerReview.Where(x => x.FreelancerId == freelance.UserId).ToListAsync();
            int currentClientReview = 25;
            int newClientReview = currentClientReview + 1;

            //=== Calculate client reviews baised on Feature ratting ===//
            foreach (var row in rowData)
            {
                foreach (var item in row.FeatureChild)
                {
                    //=== Feature wise rating sum and score ===//
                    if (item.Feature == "Communication skills")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.CommunicationRating) ?? 1;
                    }
                    else if (item.Feature == "Collaboration skills")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.CollaborationRating) ?? 1;
                    }
                    else if (item.Feature == "Professionalism")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.ProfessionalismRating) ?? 1;
                    }
                    else if (item.Feature == "Technical Skills")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.TechnicalRating) ?? 1;
                    }
                    else if (item.Feature == "Client Satisfaction")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.SatisfactionRating) ?? 1;
                    }
                    else if (item.Feature == "Responsiveness")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.ResponsivenessRating) ?? 1;
                    }
                    else if (item.Feature == "Freelancer Like To Work")
                    {
                        item.RatingSum = freeLancerReviews.Sum(x => x.LikeToWorkRating) ?? 1;
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

        //=== Group by Final Score and Assign Ranking and return this list (finally got our rank wise freelancer) ===//
        //var rankedItems = finalRankedListData.GroupBy(b => b.FinalScore)
        //    .OrderByDescending(g => g.Key)
        //    .SelectMany((item, index) => item.Select(inner =>
        //        new FeatureWiseClientReviewsSumDto
        //        {
        //            FreeLanceId = inner.FreeLanceId,
        //            FreeLanceName = inner.FreeLanceName,
        //            FinalScore = item.Key,
        //            Ranking = index + 1
        //        })).ToList();

        ////=== Save FreeLance wise score list ===// 
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
}
