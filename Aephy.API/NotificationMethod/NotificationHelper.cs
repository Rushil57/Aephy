using Aephy.API.DBHelper;
using Aephy.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aephy.API.NotificationMethod
{
    public class NotificationHelper
    {
        public async Task<string> SaveNotificationData(AephyAppDbContext db, List<Notifications> model)
        {
            try
            {
                if (model.Count > 0)
                {
                    foreach (var data in model)
                    {
                        var dbModel = new Notifications
                        {
                            NotificationText = data.NotificationText,
                            FromUserId = data.FromUserId,
                            ToUserId = data.ToUserId,
                            NotificationTime = data.NotificationTime,
                            IsRead = data.IsRead,
                            NotificationTitle = data.NotificationTitle
                        };

                        await db.Notifications.AddAsync(dbModel);
                        db.SaveChanges();
                    }


                    return "Notification Saved Succesfully!";


                }

                return "Data not found!";


            }
            catch (Exception ex)
            {
                return ex.Message + ex.InnerException;
            }


        }
    }
}
