# flowthings-io-csharp
==================

A client libary for [flowthings.io](http://flowthings.io) written in C# for .NET 4.5.1+

### Install
add nuget info here

### Docs
Documentation will be auto-generated when this project is built in Visual Studio. Additionally,
the flowthingsWindowsDemo folder contains a working Websocket example, and the flowthingsTests folder
contains examples of calls to the REST API.

### Basics
This library is intended to be very similar to the python library, so any docs there
should be usable here.  This library is designed to be asynchronous, so you will either
have to await the responses of the calls, or you can call Task.WaitAll() on the resulting
tasks.

There are two versions of most calls.  One accepts dynamics, and the other accepts generics and 
an encoder implementation.  The latter is probably better since you ensure your objects
are properly typed, but the former is simpler and quicker to use.

### Basic REST API Usage
```c#
// using flowthings;

// setup connection params
string restHost = "api.flowthings.io";
string wsHost = "ws.flowthings.io";
string ver = "0.1";
bool secure = false;

// create your token
Token myToken = new Token("youraccount", "your token");

// connect to the api
API api = new API(myToken, restHost, wsHost, secure);
await api.flow.Read("flowid");
```

### API with Dynamics
```c#
dynamic flow = new ExpandoObject();
flow.path = "/myaccount/myflow";
flow.description = "new flow";
flow.capacity = "20";

dynamic result = await api.flow.Create(flow);
```

### API with Generics
Assuming you have classes as shown below:

```c#
public class Book
{
    public string id { get; set; }
    public string title { get; set; }
    public string isbn { get; set; }
    public double price { get; set; }
}

public class BookEncoder : IJsonEncoder<Book>
{

    public JToken Encode(Book o)
    {
        Book b = o;

        return
            new JObject(
                new JProperty("id", b.id),
                new JProperty(
                    "elems",
                    new JObject(
                        new JProperty("title", b.title),
                        new JProperty("isbn", b.isbn),
                        new JProperty("price", b.price))));

    }

    public Book Decode(JToken jt)
    {
        Book b = new Book();

        b.id = (string)jt["id"];
        b.title = (string)jt["elems"]["title"]["value"];
        b.isbn = (string)jt["elems"]["isbn"]["value"];
        b.price = (double)jt["elems"]["price"]["value"];

        return b;
    }
}
```
Then you can make calls like:
```c#
Book b = new Book();
b.title = "My book";
b.isbn = "ATFE123455111";
b.price = 10.44;

BookEncoder be = new BookEncoder();

Task<Book> t1 = api.drop("flowid").Create<Book>(b, be);
Task.WaitAll(t1);
Book d1 = t1.Result;
```

### Websocket API Usage
```c#
// using flowthings;
// this.api is returned from a call to new API(...)

private void connect()
{
    this.api.websocket.OnOpen += websocket_OnOpen;
    this.api.websocket.OnClose += websocket_onClose;
    this.api.websocket.OnError += websocket_onError;
    this.api.websocket.OnMessage += websocket_onMessage;

    this.api.websocket.Connect();
}

void websocket_onMessage(string resource, dynamic value)
{
    // handle the message
}

void websocket_onError(string message)
{
    // called on error
}

void websocket_onClose(string reason, bool wasClean)
{
    // called when the socket is closed
}

void websocket_OnOpen()
{
    // called when the socket is opened
    this.api.websocket.Subscribe("myflowid");
}
```

The flowthingsWindowsDemo folder contains a more complete example.
