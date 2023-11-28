using Aephy.API.DBHelper;
using Microsoft.EntityFrameworkCore;

namespace Aephy.API.AlgorithumHelper;

public class FreelancerFinderHelper
{
    public async Task FindFreelancersAsync(AephyAppDbContext db, string? clientId, string? projectType, int solutionId, int IndustryId)
    {
        try
        {
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

            var clientDetails = users.Where(x => x.Id == clientId).FirstOrDefault();
            if (clientDetails != null)
            {
                // Pending 
                var freelancers = mergedArray.Where(x => x.StartHoursFinal.HasValue ? x.StartHoursFinal.Value.TimeOfDay == clientDetails.StartHoursFinal.Value.TimeOfDay : false &&
                                                   x.EndHoursFinal.HasValue ? x.EndHoursFinal.Value.TimeOfDay == clientDetails.EndHoursFinal.Value.TimeOfDay : false).ToArray();



                if (freelancers != null)
                {
                    var projectManagersArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Project Manager").Take(projectManager).ToArray();
                    var expertsArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Expert").Take(expert).ToArray();
                    var associatesArray = freelancers.Where(x => x.FreelancerDetail.FreelancerLevel == "Associate").Take(associate).ToArray();

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

                    var detailModelList = new List<FreelancerFindProcessDetails>();
                    foreach (var item in projectManagersArray)
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
                    foreach (var item in expertsArray)
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
                    foreach (var item in associatesArray)
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
                }
            }
        }
        catch (Exception ex)
        {

        }
    }
}
