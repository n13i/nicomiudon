using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace NiCoMiudon
{
    /// <summary>
    /// ニコニコ実況API周辺のアレをアレするクラス
    /// </summary>
    public class Client
    {
        public string UserAgent { get; set; }

        private string authCookie;
        private int authUserId;
        private bool authUserIsPremium;

        private string commentServerHost;
        private int commentServerPort;
        private string commentThreadId;
        private Action<Comment> commentReceived;
        private int commentLastRes;
        private string commentTicket;
        private string commentSendMail;
        private string commentSendText;
        private string lastSentXml;

        private Thread commentReceiveThread;
        private bool isCommentReceiving;

        public bool IsLoggedIn { get { return this.authCookie != null; } }
        public Action<bool> CommentSent;

        private readonly object lockObject = new object();

        /// <summary>
        /// ニコニコ実況にログイン
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string mail, string password)
        {
            string nextLocation;
            string authCookie = "";

            string postData;

            if (mail == null || password == null)
            {
                return false;
            }

            postData = "mail=" + Utils.UrlEncode(mail, Encoding.UTF8) + "&password=" + Utils.UrlEncode(password, Encoding.UTF8) + "&site=nicojikkyo_web";

            // https://secure.nicovideo.jp/secure/login
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create("https://secure.nicovideo.jp/secure/login");
            req.AllowAutoRedirect = false;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = postData.Length;
            req.UserAgent = GetUserAgentString();

            try
            {
                Stream s = req.GetRequestStream();
                StreamWriter writer = new StreamWriter(s);
                writer.Write(postData);
                writer.Close();
                s.Close();

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                Utils.DebugLog("Set-Cookie: " + resp.Headers["Set-Cookie"]);
                Utils.DebugLog("Location: " + resp.Headers["Location"]);
                Utils.DebugLog(resp.GetResponseHeader("Set-Cookie"));

                List<Dictionary<string, string>> cookies = Utils.ParseCookie(resp.GetResponseHeader("Set-Cookie"));
                foreach (var cookie in cookies)
                {
                    Utils.DebugLog("----");
                    foreach (var key in cookie.Keys)
                    {
                        if (key == "user_session" && cookie[key] != "deleted")
                        {
                            authCookie = key + "=" + cookie[key] + ";";
                        }

                        Utils.DebugLog(key + " = " + cookie[key]);
                    }
                }

                nextLocation = resp.Headers["Location"];
                Utils.DebugLog("next => " + nextLocation);

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                Utils.DebugLog(sr.ReadToEnd());
                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return false;
            }

            if (nextLocation.IndexOf("?error=") >= 0)
            {
                Utils.DebugLog("login error");
                return false;
            }

            // http://jk.nicovideo.jp/api/auth
            req = (HttpWebRequest)HttpWebRequest.Create(nextLocation);
            req.AllowAutoRedirect = false;
            req.Method = "GET";
            req.Headers["Cookie"] = authCookie;
            req.UserAgent = GetUserAgentString();

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                nextLocation = resp.Headers["Location"];
                Utils.DebugLog("next => " + nextLocation);

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                Utils.DebugLog(sr.ReadToEnd());
                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return false;
            }

            // /session.create?
            req = (HttpWebRequest)HttpWebRequest.Create("http://jk.nicovideo.jp" + nextLocation);
            req.AllowAutoRedirect = false;
            req.Method = "GET";
            req.Headers["Cookie"] = authCookie;
            req.UserAgent = GetUserAgentString();

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                nextLocation = resp.Headers["Location"];
                Utils.DebugLog("next => " + nextLocation);
                // nextLocation should be '/'

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                Utils.DebugLog(sr.ReadToEnd());
                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return false;
            }

            // done.
            this.authCookie = authCookie;

            // ユーザIDを取得
            Regex r = new Regex("user_session_([^_]+)_");
            Match m = r.Match(authCookie);
            this.authUserId = int.Parse(m.Groups[1].Value);

            return true;
        }

        /// <summary>
        /// チャンネルのリストを取得する
        /// </summary>
        public List<Channel> GetChannels()
        {
            string apiUrl = "http://jk.nicovideo.jp/api/v2_app/getchannels";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(apiUrl);
            req.Method = "GET";
            req.UserAgent = GetUserAgentString();

            string respXml = null;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Stream s = resp.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                respXml = sr.ReadToEnd();
                sr.Close();
                s.Close();
            }
            catch (Exception e)
            {
                if (e is WebException)
                {
                    switch (((WebException)e).Status)
                    {
                        case WebExceptionStatus.ProtocolError:
                            Utils.DebugLog("Protocol Error");
                            break;
                        case WebExceptionStatus.Timeout:
                            Utils.DebugLog("Timeout");
                            break;
                        default:
                            break;
                    }
                }

                Utils.DebugLog(e.ToString());
                return null;
            }

            List<Channel> channels = Channel.CreateFromXml(respXml);

            foreach (var ch in channels)
            {
                Utils.DebugLog("channel Type=" + ch.Type.ToString() + ", Id=" + ch.Id.ToString() + ", Name=" + ch.Name + ", Video=" + ch.Video + ", LastRes=" + ch.LastRes + ", Force=" + ch.Force.ToString());
            }

            return channels;
        }

        // should be rewrote
        public Dictionary<string, string> GetFLV(string id)
        {
            string url = "http://jk.nicovideo.jp/api/v2/getflv?v=" + id;

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";
            req.Headers["Cookie"] = this.authCookie;
            req.UserAgent = GetUserAgentString();

            string data = null;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                foreach (var key in resp.Headers.AllKeys)
                {
                    Utils.DebugLog(key + "=" + resp.Headers[key]);
                }

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                data = sr.ReadToEnd();
                Utils.DebugLog(data);
                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return null;
            }

            if (data == null)
            {
                return null;
            }

            var respGetFlv = new Dictionary<string, string>();

            string[] sElem = data.Split('&');
            foreach (var e in sElem)
            {
                string[] sPair = e.Split('=');
                string key = Utils.UrlDecode(sPair[0], Encoding.UTF8);
                string value = Utils.UrlDecode(sPair[1], Encoding.UTF8);
                respGetFlv[key] = value;
                Utils.DebugLog(key + " = " + value);

                if (key == "is_premium")
                {
                    this.authUserIsPremium = (value == "1");
                }
            }

            return respGetFlv;
        }

        /// <summary>
        /// コメントの受信を開始
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="threadId"></param>
        public void StartReceiveComments(string host, int port, string threadId, Action<Comment> commentReceived)
        {
            this.commentServerHost = host;
            this.commentServerPort = port;
            this.commentThreadId = threadId;
            this.commentReceived = commentReceived;

            this.isCommentReceiving = true;
            commentReceiveThread = new Thread(new ThreadStart(ReceiveComments));
            commentReceiveThread.IsBackground = true;
            commentReceiveThread.Start();
        }

        public void StopReceiveComments()
        {
            this.isCommentReceiving = false;
        }

        public void SendComment(string mail, string comment)
        {
            lock (this.lockObject)
            {
                List<string> cmds = new List<string>();

                if (mail != null && mail != "")
                {
                    foreach (var cmd in mail.Split(' '))
                    {
                        if (IsValidCommand(cmd))
                        {
                            cmds.Add(cmd);
                        }
                    }
                }

                if (Settings.AlwaysAnonymize)
                {
                    cmds.Add("184");
                }

                this.commentSendMail = String.Join(" ", cmds.ToArray());
                this.commentSendText = comment;
            }
            Utils.DebugLog("SendComment: " + this.commentSendMail + ", " + this.commentSendText);
        }

        /// <summary>
        /// TCP Socket経由でコメントを受信
        /// 別スレッドでの動作を想定
        /// </summary>
        private void ReceiveComments()
        {
            //Utils.DebugLog("start ReceiveComments");

            using(var tc = new TcpClient())
            {
                try
                {
                    tc.Connect(this.commentServerHost, this.commentServerPort);

                    const string requestFormat = "<thread thread=\"{0}\" version=\"20061206\" res_from=\"-10\"/>\0";

                    using(var ns = tc.GetStream())
                    {
                        byte[] req = Encoding.ASCII.GetBytes(String.Format(requestFormat, this.commentThreadId));
                        ns.Write(req, 0, req.Length);
                        ns.Flush();

                        while (this.isCommentReceiving)
                        {
                            if (ns.DataAvailable)
                            {
                                // コメント受信
                                byte[] buf = new byte[4096];
                                int readBytes = ns.Read(buf, 0, buf.Length);
                                if (readBytes == 0) { break; }

                                // FIXME バッファからあふれるようだとUTF8文字の途中で切られるかも
                                // 末尾に\0がついてくる点に注意
                                string str = Encoding.UTF8.GetString(buf, 0, readBytes - 1);
                                ProcReceiveComment(str);
                            }

                            if (ns.CanWrite)
                            {
                                // 投稿待ちコメントがあれば処理
                                string xml = ProcSendComment();
                                if (xml != null)
                                {
                                    Utils.DebugLog("send: " + xml);
                                    this.lastSentXml = xml;
                                    byte[] cxml = Encoding.UTF8.GetBytes(xml);
                                    ns.Write(cxml, 0, cxml.Length);
                                    ns.Flush();
                                }
                            }

                            Thread.Sleep(50);
                        }
                    }
                }
                catch(Exception e)
                {
                    Utils.DebugLog(e.ToString());
                }
            }

            Utils.DebugLog("end ReceiveComments");
            // end thread
        }

        private void ProcReceiveComment(string commentXml)
        {
            if (commentXml.StartsWith("<"))
            {
                //Utils.DebugLog(commentXml);
                //Utils.DebugLog("read");
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.LoadXml(commentXml);
                    //Utils.DebugLog(xmlDoc.DocumentElement.OuterXml);

                    //long vpos = Utils.GetUnixTimeMS(DateTime.Now)/10 - (long.Parse(this.commentThreadId) * 100);
                    //Utils.DebugLog("vpos=" + vpos.ToString());

                    switch (xmlDoc.DocumentElement.Name)
                    {
                        case "thread":
                            foreach (XmlAttribute attr in xmlDoc.DocumentElement.Attributes)
                            {
                                switch (attr.Name)
                                {
                                    case "last_res":
                                        this.commentLastRes = int.Parse(attr.Value);
                                        break;
                                    case "ticket":
                                        this.commentTicket = attr.Value;
                                        break;
                                }
                            }
                            break;
                        case "chat":
                            Comment comment = new Comment(xmlDoc.DocumentElement);
                            this.commentLastRes = comment.Number;
                            this.commentReceived(comment);
                            break;
                        case "chat_result":
                            bool status = false;
                            int number = -1;
                            foreach (XmlAttribute attr in xmlDoc.DocumentElement.Attributes)
                            {
                                switch (attr.Name)
                                {
                                    case "status":
                                        status = (attr.Value == "0");
                                        break;
                                    case "no":
                                        if (attr.Value != null)
                                        {
                                            number = int.Parse(attr.Value);
                                        }
                                        break;
                                }
                            }
                            if (status == true)
                            {
                                // numberだけは補完
                                if (number == -1)
                                {
                                    Utils.DebugLog("warning: can't grab no");
                                }
                                else
                                {
                                    string str = this.lastSentXml;
                                    str = str.Replace("\">", String.Format("\" no=\"{0}\">", number));
                                    ProcReceiveComment(str);
                                }
                            }
                            this.CommentSent(status);
                            break;
                        case "view_counter":
                            break;
                        default:
                            Utils.DebugLog("Unknown response: " + xmlDoc.DocumentElement.Name);
                            break;
                    }
                }
                catch (Exception e)
                {
                    // FIXME
                    Utils.DebugLog(e.ToString());
                    return;
                }
            }
        }

        /// <summary>
        /// 送信用コメントXMLを作成
        /// </summary>
        /// <returns></returns>
        private string ProcSendComment()
        {
            const string commentFormat = "<chat thread=\"{0}\" ticket=\"{1}\" vpos=\"{2}\" postkey=\"{3}\" mail=\"{4}\" user_id=\"{5}\" premium=\"{6}\">{7}</chat>\0";

            if (this.commentSendMail != null && this.commentSendText != null)
            {
                long vpos = Utils.GetUnixTimeMS(DateTime.Now) / 10 - (long.Parse(this.commentThreadId) * 100);
                string sendXml = null;

                lock (this.lockObject)
                {
                    string postKey = GetPostKey();

                    sendXml = String.Format(commentFormat, this.commentThreadId, this.commentTicket, vpos, postKey, this.commentSendMail, this.authUserId.ToString(), (this.authUserIsPremium ? "1" : "0"), this.commentSendText);

                    this.commentSendMail = null;
                    this.commentSendText = null;
                }

                Utils.DebugLog("sendXml = " + sendXml);
                return sendXml;
            }
            else
            {
                return null;
            }
        }

        private bool IsValidCommand(string cmd)
        {
            switch (cmd)
            {
                case "white":
                case "red":
                case "pink":
                case "orange":
                case "yellow":
                case "green":
                case "cyan":
                case "blue":
                case "purple":
                case "black":
                case "niconicowhite":
                case "white2":
                case "truered":
                case "red2":
                case "passionorange":
                case "orange2":
                case "madyellow":
                case "yellow2":
                case "elementalgreen":
                case "green2":
                case "marineblue":
                case "blue2":
                case "nobleviolet":
                case "purple2":
                case "big":
                case "medium":
                case "small":
                case "ue":
                case "shita":
                case "naka":
                    return true;
                default:
                    break;
            }

            Utils.DebugLog("invalid command: " + cmd);
            return false;
        }

        private string GetPostKey()
        {
            // http://jk.nicovideo.jp/api/v2/getpostkey?thread=1281639601&block_no=381
            // -> postkey=jZI3nJ6Al94SlkCbZf8DGq82MLc

            int blockNo = ((int)Math.Floor(this.commentLastRes / 100));

            Utils.DebugLog("GetPostKey: lastRes=" + this.commentLastRes.ToString() + ", block_no=" + blockNo.ToString());

            string url = "http://jk.nicovideo.jp/api/v2/getpostkey?thread=" + this.commentThreadId + "&block_no=" + blockNo.ToString();

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";
            req.Headers["Cookie"] = this.authCookie;

            string postKey = null;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                postKey = sr.ReadToEnd();
                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return null;
            }

            postKey = postKey.Substring(8);
            Utils.DebugLog("got postkey: " + postKey);

            return postKey;
        }

        private string GetUserAgentString()
        {
            string ua = "NiCoMiudon/" + Utils.GetExecutingAssemblyVersion();
            Utils.DebugLog(ua);
            return ua;
        }
    }

    /// <summary>
    /// ニコニコ実況のチャンネル情報
    /// </summary>
    public class Channel
    {
        public enum ChannelType { Television, Radio, User, Event };

        public ChannelType Type { get; set; }
        public int Id { get; set; }
        public int No { get; set; }
        public string Name { get; set; }
        public string Video { get; set; }
        public string LastRes { get; set; }
        public int Force { get; set; }
        public string ProgTitle { get; set; }

        public Channel()
        {
        }

        public static List<Channel> CreateFromXml(string xml)
        {
            var channels = new List<Channel>();

            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (XmlException e)
            {
                Utils.DebugLog(e.ToString());
                return null;
            }

            for (int i = 0; i < xmlDoc.DocumentElement.ChildNodes.Count; i++)
            {
                Channel ch = new Channel();

                var node = xmlDoc.DocumentElement.ChildNodes[i];
                Utils.DebugLog(node.Name + ": " + node.InnerXml);
                switch (node.Name)
                {
                    case "channel":
                        ch.Type = ChannelType.Television;
                        break;
                    case "radio_channel":
                        ch.Type = ChannelType.Radio;
                        break;
                    case "user_channel":
                        ch.Type = ChannelType.User;
                        break;
                    case "event_channel":
                        ch.Type = ChannelType.Event;
                        break;
                    default:
                        continue;
                }

                foreach (XmlNode child in node.ChildNodes)
                {
                    switch (child.Name)
                    {
                        case "id":
                            ch.Id = int.Parse(child.FirstChild.Value);
                            break;
                        case "no":
                            ch.No = int.Parse(child.FirstChild.Value);
                            break;
                        case "name":
                            ch.Name = child.FirstChild.Value;
                            break;
                        case "video":
                            ch.Video = child.FirstChild.Value;
                            break;
                        case "thread":
                            {
                                foreach (XmlNode c in child.ChildNodes)
                                {
                                    switch (c.Name)
                                    {
                                        case "last_res":
                                            if (c.FirstChild != null)
                                            {
                                                ch.LastRes = c.FirstChild.Value;
                                            }
                                            break;
                                        case "force":
                                            ch.Force = (int)double.Parse(c.FirstChild.Value);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                channels.Add(ch);
            }

            return channels;
        }
    }

    public class TVProgram
    {
        public int ChannelNo;
        public string Title;
        //public DateTime StartTime;

        private static Dictionary<string, int> channelTable = new Dictionary<string, int>() {
            { "livenhk", 1 },
            { "liveetv", 2 },
            { "liventv", 4 },
            { "liveanb", 5 },
            { "livetbs", 6 },
            { "livetx", 7 },
            { "livecx", 8 },
            //{ "ＴＯＫＹＯ　ＭＸ", 9 },
        };

        public TVProgram()
        {
        }

        public static List<TVProgram> GetPrograms()
        {
            var progs = new List<TVProgram>();

            // Balloo.jpのAPIを使用
            string url = "http://balloo.jp/balloo/cgi/boardlist.php";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.Method = "GET";

            string xmlStr = null;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                Stream stream = resp.GetResponseStream();
                StreamReader sr = new StreamReader(stream);
                xmlStr = sr.ReadToEnd();
                sr.Close();
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return null;
            }

            if (xmlStr == null)
            {
                return null;
            }


            XmlDocument xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml(xmlStr);

                //Regex titleRegex = new Regex(@"\[([^\]]+)\](.+)$");

                // <boardlist>の子ノード<board>について
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    if (node.Name != "board")
                    {
                        continue;
                    }

                    string channelName = null;
                    string progTitle = null;
                    //DateTime pubDate = DateTime.Now;

                    foreach (XmlAttribute n in node.Attributes)
                    {
                        //<board conn="6" conn_statistics="15" filename="livenhk_.xml" futureprogram="0015 2010広州アジア大会" href="http://live23.2ch.net/test/read.cgi/livenhk/1290428214" name="livenhk" namejp="番組ch(NHK)" nextprogram="0000 [N][天]" res="16" res_statistics="19" threadname="時論公論"/>
                        switch (n.Name)
                        {
                            case "threadname":
                                progTitle = n.Value;
                                break;
                            case "name":
                                channelName = n.Value;
                                break;
                        }
                    }

                    if (channelTable.ContainsKey(channelName))
                    {
                        TVProgram prog = new TVProgram();
                        prog.ChannelNo = channelTable[channelName];
                        prog.Title = progTitle;

                        Utils.DebugLog("ChannelNo=" + prog.ChannelNo.ToString() + /*", StartTime=" + prog.StartTime.ToString() +*/ ", Title=" + prog.Title);
                        progs.Add(prog);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.DebugLog(e.ToString());
                return null;
            }

            return progs;
        }
    }

    public class Comment
    {
        public int Number;
        public string[] Commands;
        public string Text;
        public int VPos;
        private string raw;

        public Comment(XmlNode node)
        {
            this.raw = node.OuterXml;

            string mail = "";

            foreach (XmlAttribute attr in node.Attributes)
            {
                switch (attr.Name)
                {
                    case "no":
                        this.Number = int.Parse(attr.Value);
                        Utils.DebugLog("no=" + attr.Value);
                        break;
                    case "mail":
                        Utils.DebugLog("mail=" + attr.Value);
                        mail = attr.Value;
                        break;
                    case "vpos":
                        Utils.DebugLog("vpos=" + attr.Value);
                        this.VPos = int.Parse(attr.Value);
                        break;
                }
            }

            this.Commands = mail.Split(' ');

            this.Text = null;
            // deletedの場合がある
            if (node.FirstChild != null)
            {
                this.Text = node.FirstChild.Value;
            }
        }

        public override string ToString()
        {
            //return base.ToString();
            return this.Text;
        }
    }
}
