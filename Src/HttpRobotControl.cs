using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kaazing.Robot.Control.Command;

namespace Kaazing.Robot.Control
{
    public class HttpRobotControl
    {
        private HttpClient _httpClient = new HttpClient();
        private Uri _uri;

        public HttpRobotControl(Uri uri) {
            _uri = uri;
        }

        public void SendCommand(BaseCommand command)
        {
            if (!((command.CommandKind == BaseCommand.Kind.PREPARE) ||
                  (command.CommandKind == BaseCommand.Kind.START) ||
                  (command.CommandKind == BaseCommand.Kind.ABORT)))
            {
                throw new InvalidOperationException(String.Format("Invalid Command: {0}", command.CommandKind.ToString()));
            }

            Uri resourceUri = new Uri(_uri, command.CommandKind.ToString());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, resourceUri);
            request.Content = new StringContent(String.Format("name:{0}", command.Name));

            HttpResponseMessage responseMessage = _httpClient.SendAsync(request).Result;
            
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {

            }
            else if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
            {

            }
            else
            {

            }
        }

    }
}
