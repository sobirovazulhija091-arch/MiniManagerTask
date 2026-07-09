using System.Net;

public class Response<T>
{
    public HttpStatusCode StatusCode{get;set;}
    public T? Data{get;set;}
    public List<string> Description{get;set;}=[];
    public Response(HttpStatusCode httpStatus ,List<string> message,T data)
    {
        StatusCode=httpStatus;
        Description=message;
        Data=data;
    }
     public Response(HttpStatusCode httpStatus ,List<string> message)
    {
        StatusCode=httpStatus;
        Description=message;
    }
     public Response(HttpStatusCode httpStatus ,string message,T data)
    {
        StatusCode=httpStatus;
        Description.Add(message);
        Data=data;
    }
       public Response(HttpStatusCode httpStatus ,string message)
    {
        StatusCode=httpStatus;
        Description.Add(message);
    }
};