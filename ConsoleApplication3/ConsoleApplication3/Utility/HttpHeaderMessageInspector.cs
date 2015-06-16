using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

public class HttpHeaderMessageInspector : IClientMessageInspector
{
    private readonly Dictionary<string, string> _httpHeaders;

    public HttpHeaderMessageInspector()
    {
    
    }

    public HttpHeaderMessageInspector(Dictionary<string, string> httpHeaders)
    {
        this._httpHeaders = httpHeaders;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState) { }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        HttpRequestMessageProperty httpRequestMessage;
        object httpRequestMessageObject;
        string userName = "";
        string passWord = "";

        if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
        {
            httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;

            foreach (var httpHeader in _httpHeaders)
            {
                //httpRequestMessage.Headers[httpHeader.Key] = httpHeader.Value;
                if (httpHeader.Key.CompareTo("Username") == 0)
                {
                    userName = httpHeader.Value;
                }
                else if (httpHeader.Key.CompareTo("Password") == 0)
                {
                    passWord = httpHeader.Value;
                }
            }

            httpRequestMessage.Headers[HttpRequestHeader.Authorization] = "Basic " +
                               Convert.ToBase64String(Encoding.ASCII.GetBytes(userName + ":" + passWord));
        }
        else
        {
            httpRequestMessage = new HttpRequestMessageProperty();

            foreach (var httpHeader in _httpHeaders)
            {
                httpRequestMessage.Headers.Add(httpHeader.Key, httpHeader.Value);
            }
            request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
        }
        return null;
    }
}


/*#region Helper methods

private static void AddHeaders(Message request)
{ 
    HttpRequestMessageProperty httpRequestMessage;
    object httpRequestMessageObject;

    // Try to retrieve the HttpRequestMessageProperty object and if one doesn't 
    // exist create one
    if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
    {
        httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
    }
    else
    {
    // Paul Halupka [phalupka@infusion.com]:
    // In some cases the HttpRequestMessageProperty does not seem to
    // exist at this point. When I ran into this issue it only
    // seemed to happen when the debugger was NOT attached.
    // Adding the a new HttpRequestMessageProperty does not seem
    // to do any harm. See MQC bug 8582

    httpRequestMessage = new
        request.Properties[HttpRequestMessageProperty.Name] = httpRequestMessage;
    }

    // attach the session to the outgoing request 
    if (RequestSessionManager.Current != null)
    {
        RequestSessionManager.Current.ApplySessionIdToRequest(httpRequestMessage); 
    } 

    // Check if LoginOriginID header already exists.
    if (!httpRequestMessage.Headers.AllKeys.Contains(LoginOriginIdHeaderName))
    {
        // Create new Session-Id message header
        httpRequestMessage.Headers.Add(LoginOriginIdHeaderName, NetworkCardInformation.GetMacAddress);
    } 

}

private static void CheckReplyForSessionUpdate(Message reply)
{
    object httpResponseMessageObject;

// if there is an http response object then we check the session identifier.
    if (GetHttpResponseObject(reply, out httpResponseMessageObject))
    {
        // get the response message
        HttpResponseMessageProperty 
        httpResponseMessage = httpResponseMessageObject as HttpResponseMessageProperty;

        // get the session manager and check if the session matches
        RequestSessionManager sessionManager = RequestSessionManager.Current;

        if (sessionManager != null && sessionManager.HasSessionChanged(httpResponseMessage))
        {
            sessionManager.UpdateSessionId(httpResponseMessage);
        } 

    }
}

private static bool GetHttpResponseObject(Message reply, out object httpResponseMessageObject)
{
    return reply.Properties.TryGetValue(HttpResponseMessagePropertyName, out httpResponseMessageObject);
}
#endregion*/

internal class HttpHeadersEndpointBehavior : IEndpointBehavior
{
    private readonly Dictionary<string, string> _httpHeaders;

    public HttpHeadersEndpointBehavior(Dictionary<string, string> httpHeaders)
    {
        this._httpHeaders = httpHeaders;
    }
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        var inspector = new HttpHeaderMessageInspector(this._httpHeaders);

        clientRuntime.MessageInspectors.Add(inspector);
    }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
    public void Validate(ServiceEndpoint endpoint) { }
}