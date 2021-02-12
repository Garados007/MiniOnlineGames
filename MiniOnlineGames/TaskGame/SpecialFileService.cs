using MaxLib.WebServer;
using System.Threading.Tasks;

namespace MiniOnlineGames.TaskGame
{
    public class SpecialFileService : WebService
    {
        public SpecialFileService() 
            : base(ServerStage.CreateDocument)
        {
        }

        public override bool CanWorkWith(WebProgressTask task)
        {
            if (task.Request.Location.StartsUrlWith(new[] { "game" }))
                return true;
            if (task.Request.Location.IsUrl(new[] { "script.js" }))
                return true;
            return false;
        }

        public override async Task ProgressTask(WebProgressTask task)
        {
            await Task.CompletedTask;
            if (task.Request.Location.IsUrl(new[] { "game"}))
            {
                var (key, _) = GameController.New();
                task.Response.FieldLocation = $"/game/{key}";
                task.Response.StatusCode = HttpStateCode.TemporaryRedirect;
                task.NextStage = ServerStage.CreateResponse;
                return;
            }
            if (task.Request.Location.StartsUrlWith(new[] { "game" }))
            {
                task.Document.DataSources.Add(new HttpFileDataSource(
                    System.Diagnostics.Debugger.IsAttached ?
                    "../../../../src/index.html" :
                    "content/index.html"
                )
                {
                    MimeType = MimeType.TextHtml,
                });
                return;
            }
            if (task.Request.Location.IsUrl(new[] { "script.js" }))
            {
                task.Document.DataSources.Add(new HttpFileDataSource(
                    System.Diagnostics.Debugger.IsAttached ?
                    "../../../../script.js" :
                    "content/script.js"
                )
                {
                    MimeType = MimeType.ApplicationJs,
                });
                return;
            }
        }
    }
}
