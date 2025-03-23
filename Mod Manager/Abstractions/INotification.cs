using System.Threading.Tasks;

namespace Mod_Manager.Abstractions;

public interface INotification
{
    public Task<NotificationResponse> ShowNotification(NotificationResponses response, string text, string title);
}