using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Web;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace CheckBannedIP
{
    public struct TCookie
    {
        public String item;
        public String value;
    }

    class NetworkClass
    {
        public NetworkClass(TAccount ac, TProxy p)
        {            
            _host = ac.host;
            _code = ac.code;
            _refer = ac.refer;
            _account = ac;
            _proxy = p;
        }

        public NetworkClass()
        {
            
        }

        public NetworkClass(TProxy pro)
        {
            _proxy = pro;
        }

        private String _host = "";
        private String _code = "";
        private String _refer = "";
        private String _cookie = "";
        private TProxy _proxy = new TProxy();
        private MainForm _form;
        private byte[] _rec = new byte[256];
        private Socket _socket;
        private int _file = 0;
        private TAccount _account;
        private static String _adnxshost = "ib.adnxs.com";
        private static String _gadnxshost = "g.adnxs.com";
        private static String _madnxshost = "mobile.adnxs.com";
        private List<TCookie> _CookieLst = new List<TCookie>();
        private Random _ro = new Random();

        private String Socket_get_pwd(String host, String url, String refer, String cookie)
        {
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(_proxy.username + ":" + _proxy.password);
            String auth = Convert.ToBase64String(byteArray);

            String get = "GET " + url + " HTTP/1.1\r\n";
            get += "Host: " + host + "\r\n";
            get += "User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)\r\n";
            get += "Accept:image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*\r\n";
            get += "Accept-Language: en-us\r\n";
            get += "Accept-Encoding: gzip, deflate\r\n";
            get += "Referer:" + refer + "\r\n";
            if (cookie != "")
            {
                get += _cookie + "\r\n";
            }
            get += "Proxy-Authorization: Basic " + auth + "\r\n";
            get += "Connection: keep-alive\r\n\r\n";

            //_file++;

            byte[] buf = System.Text.Encoding.ASCII.GetBytes(get);
            String data = "";
            int recsize = 256;

            try
            {
                _socket.Send(buf, 0);
            }
            catch
            {
                return data;
            }
            /*
                        FileInfo fi = new FileInfo(Application.StartupPath + "\\Data\\test" + _file.ToString() + ".php");
                        FileStream sw;
                        if (!fi.Exists)
                        {
                            sw = fi.Create();
                        }
                        else
                        {
                            sw = fi.Open(FileMode.Open, FileAccess.ReadWrite);
                        }
            */
            while ((recsize == 256) && (data.Length < 5000))
            {
                try
                {
                    recsize = _socket.Receive(_rec, 256, 0);
                    //sw.Write(_rec, 0, (int)recsize);
                }
                catch
                {
                    return "";
                }

                if ((0 == recsize) && (0 == data.Length))
                {
                    return "";
                }

                data += System.Text.Encoding.Default.GetString(_rec);
                System.Array.Clear(_rec, 0, 256);
                Thread.Sleep(300);
            }

            //sw.Close();
            return data;
        }

        private String Socket_get(String host, String url, String refer, String cookie)
        {
            String get = "GET " + url + " HTTP/1.1\r\n";
            get += "Host: " + host + "\r\n";
            get += "User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1)\r\n";
            get += "Accept:image/gif, image/jpeg, image/pjpeg, image/pjpeg, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, */*\r\n";
            get += "Accept-Language: en-us\r\n";
            //get += "Accept-Encoding: gzip, deflate\r\n";
            get += "Referer:" + refer + "\r\n";
            if (cookie != "")
            {
                get += _cookie + "\r\n";
            }
            get += "Connection: keep-alive\r\n\r\n";

            //_file++;

            byte[] buf = System.Text.Encoding.ASCII.GetBytes(get);
            String data = "";
            int recsize = 256;

            _socket.ReceiveTimeout = 10000;
            _socket.SendTimeout = 10000;

            try
            {
                _socket.Send(buf, 0);
            }
            catch
            {
                return data;
            }
/*
            FileInfo fi = new FileInfo(Application.StartupPath + "\\Data\\test" + _file.ToString() + ".php");
            FileStream sw;
            if (!fi.Exists)
            {
                sw = fi.Create();
            }
            else
            {
                sw = fi.Open(FileMode.Open, FileAccess.ReadWrite);
            }
*/
            while ((recsize == 256) && (data.Length < 5000))
            {
                try
                {
                    recsize = _socket.Receive(_rec, 256, 0);
                    //sw.Write(_rec, 0, (int)recsize);
                }
                catch
                {
                    return "";
                }

                if ((0 == recsize) && (0 == data.Length))
                {
                    return "";
                }

                data += System.Text.Encoding.Default.GetString(_rec);                
                System.Array.Clear(_rec, 0, 256);
                Thread.Sleep(300);
            }

            //sw.Close();
            return data;
        }

        /// <summary>
        /// SOCK4 握手
        /// </summary>
        /// <param name="strRemoteHost">要访问的url</param>
        /// <param name="iRemotePort">一般为80</param>
        /// <param name="sProxyServer">已经和SOCK4代理链接的Socket</param>
        /// <returns></returns>
        private bool ConnectSocks4ProxyServer(string strRemoteHost, int iRemotePort, Socket sProxyServer)
        {
            byte[] bySock4Send = new Byte[10];
            byte[] bySock4Receive = new byte[10];

            bySock4Send[0] = 4;
            bySock4Send[1] = 1;

            bySock4Send[2] = (byte)(iRemotePort / 256);
            bySock4Send[3] = (byte)(iRemotePort % 256);
            try
            {
                IPAddress ipAdd = Dns.GetHostEntry(strRemoteHost).AddressList[0];
                string strIp = ipAdd.ToString();
                string[] strAryTemp = strIp.Split(new char[] { '.' });
                bySock4Send[4] = Convert.ToByte(strAryTemp[0]);
                bySock4Send[5] = Convert.ToByte(strAryTemp[1]);
                bySock4Send[6] = Convert.ToByte(strAryTemp[2]);
                bySock4Send[7] = Convert.ToByte(strAryTemp[3]);

                bySock4Send[8] = 0;

                sProxyServer.Send(bySock4Send, bySock4Send.Length, SocketFlags.None);
                int iRecCount = sProxyServer.Receive(bySock4Receive, bySock4Receive.Length, SocketFlags.None);
            }
            catch (System.Exception ex)
            {
                return false;
            }

            if (bySock4Receive[1] == 90)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// SOCK5 握手
        /// </summary>
        /// <param name="strRemoteHost">要访问的url</param>
        /// <param name="iRemotePort">一般为80</param>
        /// <param name="sProxyServer">已经和SOCK5代理链接的Socket</param>
        /// <returns></returns>
        private bool ConnectSocks5ProxyServer(string strRemoteHost, int iRemotePort, Socket sProxyServer)
        {
            //构造Socks5代理服务器第一连接头(无用户名密码) 
            byte[] bySock5Send = new Byte[10];
            bySock5Send[0] = 5;
            bySock5Send[1] = 1;
            bySock5Send[2] = 0;
            try
            {
                //发送Socks5代理第一次连接信息 
                sProxyServer.Send(bySock5Send, 3, SocketFlags.None);

                byte[] bySock5Receive = new byte[10];
                int iRecCount = sProxyServer.Receive(bySock5Receive, bySock5Receive.Length, SocketFlags.None);

                if (iRecCount < 2)
                {
                    sProxyServer.Close();
                    throw new Exception("不能获得代理服务器正确响应。");
                }

                if (bySock5Receive[0] != 5 || (bySock5Receive[1] != 0 && bySock5Receive[1] != 2))
                {
                    sProxyServer.Close();
                    throw new Exception("代理服务其返回的响应错误。");
                }

                if (bySock5Receive[1] == 0)
                {
                    bySock5Send[0] = 5;
                    bySock5Send[1] = 1;
                    bySock5Send[2] = 0;
                    bySock5Send[3] = 1;

                    IPAddress ipAdd = Dns.GetHostEntry(strRemoteHost).AddressList[0];
                    string strIp = ipAdd.ToString();
                    string[] strAryTemp = strIp.Split(new char[] { '.' });
                    bySock5Send[4] = Convert.ToByte(strAryTemp[0]);
                    bySock5Send[5] = Convert.ToByte(strAryTemp[1]);
                    bySock5Send[6] = Convert.ToByte(strAryTemp[2]);
                    bySock5Send[7] = Convert.ToByte(strAryTemp[3]);

                    bySock5Send[8] = (byte)(iRemotePort / 256);
                    bySock5Send[9] = (byte)(iRemotePort % 256);

                    sProxyServer.Send(bySock5Send, bySock5Send.Length, SocketFlags.None);
                    iRecCount = sProxyServer.Receive(bySock5Receive, bySock5Receive.Length, SocketFlags.None);

                    if (bySock5Receive[0] != 5 || bySock5Receive[1] != 0)
                    {
                        sProxyServer.Close();
                        throw new Exception("第二次连接Socks5代理返回数据出错。");
                    }
                    return true;
                }
                else
                {
                    if (bySock5Receive[1] == 2)
                        throw new Exception("代理服务器需要进行身份确认。");
                    else return false;
                }
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }


        private String GetMid(String input, String s, String e)
        {
            int pos = input.IndexOf(s);
            if (pos == -1)
            {
                return "";
            }

            pos += s.Length;

            int pos_end = 0;
            if (e == "")
            {
                pos_end = input.Length;
            }
            else
            {
                pos_end = input.IndexOf(e, pos);
            }
            
            if (pos_end == -1)
            {
                return "";
            }

            return input.Substring(pos, pos_end - pos);
        }

        private void GetCookie(String html)
        {
            String cookie_tag = "Set-Cookie:";
            String cookie_item = "";
            int pos = 0;
            String old_cookie = _cookie;
            List<TCookie> newcookies = new List<TCookie>();

            _cookie = "";

            if (old_cookie == "")
            {
                while ((pos = html.IndexOf(cookie_tag, pos)) != -1)
                {
                    TCookie cookies = new TCookie();
                    pos += cookie_tag.Length;
                    if (html.IndexOf(";", pos) != -1)
                    {
                        cookie_item = html.Substring(pos, html.IndexOf(";", pos) - pos + 1);
                    }
                    else
                    {
                        return;
                    }

                    if (cookie_item.IndexOf("=") != -1)
                    {
                        cookies.item = cookie_item.Substring(0, cookie_item.IndexOf("=") + 1);
                    }
                    else
                    {
                        return;
                    }
                    cookies.value = GetMid(cookie_item, "=", "");
                    _CookieLst.Add(cookies);
                }

                foreach (TCookie c in _CookieLst)
                {
                    _cookie += c.item;
                    _cookie += c.value;
                }

                _cookie = "Cookie:" + _cookie.Substring(0, _cookie.Length - 1);
            }
            else
            {
                while ((pos = html.IndexOf(cookie_tag, pos)) != -1)
                {
                    TCookie cookies = new TCookie();
                    pos += cookie_tag.Length;
                    if (html.IndexOf(";", pos) != -1)
                    {
                        cookie_item = html.Substring(pos, html.IndexOf(";", pos) - pos + 1);
                    }
                    else
                    {
                        return;
                    }

                    if (cookie_item.IndexOf("=") != -1)
                    {
                        cookies.item = cookie_item.Substring(0, cookie_item.IndexOf("=") + 1);
                    }
                    else
                    {
                        return;
                    }
                    cookies.value = GetMid(cookie_item, "=", "");
                    newcookies.Add(cookies);
                    _cookie += cookie_item;
                }

                if (newcookies.Count > _CookieLst.Count)
                {
                    _CookieLst.Clear();

                    foreach (TCookie c in newcookies)
                    {
                        TCookie cookies = new TCookie();
                        cookies.item = c.item;
                        cookies.value = c.value;
                        _CookieLst.Add(cookies);
                    }
                }
                else
                {

                    foreach (TCookie cnew in newcookies)
                    {
                        for (int i = 0; i < _CookieLst.Count; i++)
                        {
                            if (cnew.item == _CookieLst[i].item)
                            {
                                _CookieLst[i] = cnew;
                            }
                        }
                    }
                }

                _cookie = "";

                foreach (TCookie c in _CookieLst)
                {
                    _cookie += c.item;
                    _cookie += c.value;
                }

                _cookie = "Cookie:" + _cookie.Substring(0, _cookie.Length - 1);
            }
        }

        private bool CheckClickTag(String code)
        {
            String tmp = code.Replace("creative_click", "");
            tmp = tmp.Replace("INSERT_CLICK_TAG", "");
            tmp = tmp.Replace("CLICK_URL", "");

            if ((tmp.IndexOf("click") != -1) || (tmp.IndexOf("clk") != -1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private String GetURI(String input)
        {
            String output = input.Replace(":", "%3A");
            output = output.Replace("/", "%2F");

            return output;
        }

        public List<TProxy> DownloadProxy()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            String data = "";

            request = (HttpWebRequest)WebRequest.Create("http://23.234.228.27/person.html");
            //request = (HttpWebRequest)WebRequest.Create("http://108.170.31.71/test/private.html");
            request.UserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.89 Safari/537.1";
            request.AllowAutoRedirect = false;
            request.Timeout = 30000;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                return null;
            }
            if (response.StatusCode == HttpStatusCode.OK && response.ContentLength < 1024 * 1024)
            {
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                data = reader.ReadToEnd();
            }

            data = GetMid(data, "\n", "");

            if (data == "")
            {
                return null;
            }

            String[] sArray = data.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<TProxy> proxylist = new List<TProxy>();
            foreach (String proxy in sArray)
            {
                if (proxy.IndexOf("\0") != -1)
                {
                    continue;
                }
                
                TProxy pr = new TProxy();
                String[] pArr = proxy.Split('|');
                if (pArr.Length == 5)
                {
                    pr.proxy = pArr[0] + ":" + pArr[1];
                    pr.country = pArr[2];
                    pr.username = pArr[3];
                    pr.password = pArr[4];
                    proxylist.Add(pr);
                }
                else if (pArr.Length == 4)
                {
                    pr.proxy = pArr[0] + ":" + pArr[1];
                    pr.type = pArr[2];
                    pr.country = pArr[3];
                    pr.username = "nousername";
                    pr.password = "nopassword";
                    proxylist.Add(pr);
                }
            }

            return proxylist;
        }

        public bool CheckProxy()
        {
            if ((_proxy.country == "AT")
                || (_proxy.country == "AU")
                || (_proxy.country == "BE")
                || (_proxy.country == "BG")
                || (_proxy.country == "CA")
                //|| (_proxy.country == "US")
                || (_proxy.country == "CH")
                || (_proxy.country == "DE")
                || (_proxy.country == "DK")
                || (_proxy.country == "ES")
                || (_proxy.country == "EU")
                || (_proxy.country == "FI")
                || (_proxy.country == "FR")
                || (_proxy.country == "GB")
                || (_proxy.country == "GR")
                || (_proxy.country == "IE")
                || (_proxy.country == "IT")
                || (_proxy.country == "NL")
                || (_proxy.country == "NO")
                || (_proxy.country == "NZ")
                || (_proxy.country == "PL")
                || (_proxy.country == "PT")
                || (_proxy.country == "ZA")
                || (_proxy.country == "HN")
                || (_proxy.country == "AE")
                || (_proxy.country == "JP")
                || (_proxy.country == "BR")
                || (_proxy.country == "CL")
                || (_proxy.country == "AR")
                || (_proxy.country == "IL")
                || (_proxy.country == "SG"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private String DoGet(String host, String code)
        {
            String data = "";
            String proxy = _proxy.proxy;
            String ip = proxy.Substring(0, proxy.IndexOf(":"));
            int port = System.Int32.Parse(GetMid(proxy, ":", ""));

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                _socket.Connect(ip, port);
            }
            catch
            {
                return "";
            }

            if (_proxy.username == "nousername")
            {
                data = Socket_get(host, "http://" + host + code, _refer, _cookie);
            }
            else
            {
                data = Socket_get_pwd(host, "http://" + host + code, _refer, _cookie);
            }
            _socket.Close();

            return data;
        }

        public bool CheckIPStartRM()
        {
            String data = "";
            int pos = 0;

            data = DoGet(_host, _code);

            if ((data == "") || (data.IndexOf("200 OK") == -1))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CheckIPStart()
        {
            String data = "";
            int pos = 0;
            String code = _code;
            String host = _host;
            String proxy = _proxy.proxy;
            String ip = proxy.Substring(0, proxy.IndexOf(":"));
            int port = System.Int32.Parse(GetMid(proxy, ":", ""));

            while (pos != -1)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    _socket.Connect(ip, port);
                }
                catch
                {
                    return false;
                }

                if (_proxy.type == "SOCKS4")
                {
                    if (ConnectSocks4ProxyServer(host, 80, _socket))
                    {
                        data = Socket_get(host, "http://" + host + code, _refer, _cookie);
                    }
                    else
                    {
                        data = "";
                    }
                }
                else if (_proxy.type == "SOCKS5")
                {
                    try
                    {
                        if (ConnectSocks5ProxyServer(host, 80, _socket))
                        {
                            data = Socket_get(host, "http://" + host + code, _refer, _cookie);
                        }
                        else
                        {
                            data = "";
                        }
                    }
                    catch (System.Exception ex)
                    {
                        data = "";
                    }
                }
                else
                {
                    //http
                    if (_proxy.username == "nousername")
                    {
                        data = Socket_get(host, "http://" + host + code, _refer, _cookie);
                    }
                    else
                    {
                        data = Socket_get_pwd(host, "http://" + host + code, _refer, _cookie);
                    }
                }
                
                _socket.Close();

                if (data == "")
                {
                    return false;
                }

                if ((pos = data.IndexOf("302 Moved Temporarily")) != -1)
                {
                    if (data.IndexOf("Location: ") != -1)
                    {
                        String new_url = GetMid(data, "Location: ", "\r\n");
                        host = GetMid(new_url, "http://", "/");

                        if ((host != _host)
                            && (host != _gadnxshost)
                            && (host != _adnxshost)
                            && (host != _madnxshost))
                        {
                            return false;
                        }

                        code = GetMid(new_url, host, "");

                        if (data.IndexOf("Set-Cookie:") != -1)
                        {
                            GetCookie(data);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((pos = data.IndexOf("302 Found")) != -1)
                {
                    if (data.IndexOf("Location: ") != -1)
                    {
                        String new_url = GetMid(data, "Location: ", "\r\n");
                        host = GetMid(new_url, "http://", "/");

                        if ((host != _host)
                            && (host != _gadnxshost)
                            && (host != _adnxshost)
                            && (host != _madnxshost))
                        {
                            return false;
                        }

                        code = GetMid(new_url, host, "");

                        if (data.IndexOf("Set-Cookie:") != -1)
                        {
                            GetCookie(data);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((pos = data.IndexOf("200 OK")) != -1)
                {
                    if (data.IndexOf("Set-Cookie:") != -1)
                    {
                        GetCookie(data);
                    }

                    if (data.IndexOf("bdref") != -1)
                    {
                        String body = GetMid(data, "Content-Length", "");
                        if (body == "")
                        {
                            body = GetMid(data, "Set-Cookie", "");
                        }

                        String url = GetMid(body, "http://", "'");
                        url += GetURI(_refer);
                        url += "&id" + GetMid(body, "&id", "'");

                        host = url.Substring(0, url.IndexOf("/"));

                        code = GetMid(url, host, "");
                    }
                    else
                    {
                        String ContentLength = GetMid(data, "Content-Length: ", "\r");

                        if (ContentLength == "")
                        {
                            return false;
                        }

                        if (ContentLength != "0")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }                   
                    }                    
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
