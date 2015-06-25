using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

using flowthings;
using flowthings.Services;
using flowthings.Util;

namespace flowthingsTests
{
    [TestClass]
    public class UnitTests
    {
        const string REST_HOST = "api.flowthings.io";
        const string WS_HOST = "ws.flowthings.io";
        const string VER = "0.1";
        const bool SECURE = false;

        Token MY_TOKEN = new Token("youraccount", "your token");
        const string MY_ID = "your id";

        const string TEST_FLOW_ID = "a flow id";
        const string TEST_FLOW_PATH = "that flow path";
        const string TEST_BASE_PATH = "your base path";

        string c1_id, c2_id;


        [TestMethod]
        public void TestBaseService()
        {
            var bs = new BaseService(MY_TOKEN, SECURE, REST_HOST, VER);

            Task<dynamic> t = bs.RequestAsync("GET", "/flow/" + TEST_FLOW_ID);

            Task.WaitAll(t);

            JToken r = t.Result;
            Assert.IsTrue((bool)(((JObject)r)["head"]["ok"]));

            JObject jo =
                new JObject(
                    new JProperty("path", TEST_BASE_PATH + "/c1"),
                    new JProperty("description", "this is a description"));

            Task<dynamic> t2 = bs.RequestAsync("POST", "/flow", jo);

            Task.WaitAll(t2);

            JToken r2 = t2.Result;
            Assert.IsTrue((bool)(((JObject)r2)["head"]["ok"]));

            c1_id = (string)((JObject)r2)["body"]["id"];
        }

        [TestMethod]
        public void TestRead()
        {
            API api = new API(MY_TOKEN, REST_HOST, WS_HOST, SECURE);

            Task<dynamic> t1 = api.flow.Read(TEST_FLOW_ID);
            Task.WaitAll(t1);
            dynamic d1 = t1.Result;
            Assert.AreEqual((string)d1.path, TEST_FLOW_PATH);
        }

        [TestMethod]
        public void TestCreate()
        {
            API api = new API(MY_TOKEN, REST_HOST, WS_HOST, SECURE);

            // flows
            dynamic flow = new ExpandoObject();
            flow.path = TEST_BASE_PATH + "/c2";
            flow.description = "new flow";
            flow.capacity = "20";

            Task<dynamic> t1 = api.flow.Create(flow);
            Task.WaitAll(t1);
            dynamic d1 = t1.Result;
            Assert.AreEqual((int)d1.capacity, 20);

            // tracks
            dynamic track = new ExpandoObject();
            track.source = TEST_FLOW_PATH;
            track.destination = TEST_BASE_PATH + "/c2"; // doesnt accept array

            Task<dynamic> t2 = api.track.Create(track);
            Task.WaitAll(t2);
            dynamic d2 = t2.Result;
            Assert.AreEqual((string)d2.destination, track.destination);

            // group
            dynamic group = new ExpandoObject();
            group.displayName = "My group";
            group.memberIds = new string[] { MY_ID };

            Task<dynamic> t3 = api.group.Create(group);
            Task.WaitAll(t3);
            dynamic d3 = t3.Result;
            Assert.AreEqual((string)d3.displayName, group.displayName);
        
        }

        [TestMethod]
        public void TestUpdate()
        {
            c2_id = TEST_FLOW_ID;

            API api = new API(MY_TOKEN, REST_HOST, WS_HOST, SECURE);

            dynamic fields = new ExpandoObject();
            fields.description = "new flow (updated)";
            fields.capacity = "2000";

            Task<dynamic> t1 = api.flow.Update(c2_id, fields);
            Task.WaitAll(t1);
            dynamic d1 = t1.Result;
            Assert.AreEqual((int)d1.capacity, 2000);

            dynamic fields2 = new ExpandoObject();
            fields2.elems = new ExpandoObject();
            fields2.elems.name = "updated drop";

            Task<dynamic> t2 = api.drop(TEST_FLOW_ID).Update("your drop id", fields2);
            Task.WaitAll(t2);
            dynamic d2 = t2.Result;
            Assert.AreEqual((int)d1.capacity, 2000);

        }

        [TestMethod]
        public void TestFindMany()
        {
            API api = new API(MY_TOKEN, REST_HOST, WS_HOST, SECURE);

            List<Dictionary<string, object>> l = new List<Dictionary<string, object>>();
            Dictionary<string, object> d1 = new Dictionary<string, object>();

            Dictionary<string, string> d2 = new Dictionary<string, string>();
            d2.Add("filter", "EXISTS elems.name");

            d1.Add("flowId", TEST_FLOW_ID);
            d1.Add("params", d2);

            object[] targets = new object[] { d1 };

            Task<dynamic> t1 = api.drop(TEST_FLOW_ID).FindMany(targets);
            Task.WaitAll(t1);
            dynamic dx1 = t1.Result;

            Assert.IsNotNull(dx1);
        }

        [TestMethod]
        public void TestGenerics()
        {
            Book b = new Book();
            b.title = "My book";
            b.isbn = "ATFE123455111";
            b.price = 10.44;

            API api = new API(MY_TOKEN, REST_HOST, WS_HOST, SECURE);
            BookEncoder be = new BookEncoder();

            Task<Book> t1 = api.drop(TEST_FLOW_ID).Create<Book>(b, be);
            Task.WaitAll(t1);
            Book d1 = t1.Result;

            b.title = "new title";
            Task<Book> t2 = api.drop(TEST_FLOW_ID).Update<Book>(d1.id, b, be);
            Task.WaitAll(t2);
            Book d2 = t2.Result;

            Task<List<Book>> t3 = api.drop(TEST_FLOW_ID).Find<Book>("EXISTS elems.title", be);
            Task.WaitAll(t3);
            List<Book> d3 = t3.Result;


        }

        public class Book
        {
            public string id;
            public string title;
            public string isbn;
            public double price;
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
    }
}
