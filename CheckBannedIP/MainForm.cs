using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace CheckBannedIP
{
    public struct TAccount
    {
        public String host;
        public String code;
        public String refer;
        public int thread;
    }

    public struct TProxy
    {
        public String proxy;
        public String country;
        public String username;
        public String password;
        public String type;
    }

    public struct TProxyCheck
    {
        public String proxy;
        public String country;
        public String username;
        public String password;
        public String type;
        public String status;
    }

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            _accountList = _database.GetAccountlist();

            if (GetProxyList() == false)
            {
                return;
            }
            CheckProxy();
            RemoveBannedIP();
        }

        private List<TProxy> _proxyList = new List<TProxy>();
        private List<TProxy> _proList = new List<TProxy>();
        private List<TProxy> _proGoodList = new List<TProxy>();
        private List<TAccount> _accountList = new List<TAccount>();
        private DataClass _database = new DataClass();
        private TAccount _accountcheck = new TAccount();
        private int _accountindex = 0;
        private int judgedCount = 0;

        private void RemoveBannedIP()
        {
            judgedCount = 0;
            if (_accountindex < _accountList.Count)
            {
                _accountcheck = _accountList[_accountindex];
                _accountindex++;
            }
            else
            {
                //没有待测的account了
                MessageBox.Show("Finished");
                return;
            }
            

            ThreadPool.SetMaxThreads(5, 512);
            foreach (TProxy p in _proxyList)
            {
                ProxyJudger pj = new ProxyJudger(p);
                pj.onJudgeDone += onJudgeDone;
                ThreadPool.QueueUserWorkItem(new WaitCallback(pj.doProxyJudge), _accountcheck);
            }
        }

        private bool GetProxyList()
        {
            NetworkClass network = new NetworkClass();
            _proxyList = network.DownloadProxy();

            if (_proxyList == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void CheckProxy()
        {
            for (int i = 0; i < (_proxyList.Count / 10) + 1; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (i * 10 + j >= _proxyList.Count)
                    {
                        return;
                    }
                    NetworkClass network = new NetworkClass(_proxyList[i * 10 + j]);
                    if (network.CheckProxy() == true)
                    {
                        TProxy pro = new TProxy();

                        pro.proxy = _proxyList[i * 10 + j].proxy;
                        pro.country = _proxyList[i * 10 + j].country;
                        pro.username = _proxyList[i * 10 + j].username;
                        pro.password = _proxyList[i * 10 + j].password;
                        pro.type = _proxyList[i * 10 + j].type;
                        _proList.Add(pro);
                    }
                }
            }
        }

        private bool IsInList(TProxy pr)
        {
            foreach (TProxy p in _proGoodList)
            {
                if (p.proxy == pr.proxy)
                {
                    return true;
                }
            }

            return false;
        }

        private void onJudgeDone(TProxyCheck proxy)
        {
            if (this.InvokeRequired)
            {
                try
                {
                	this.Invoke(new ProxyJudger.onJudgeDoneDelegate(onJudgeDone), new object[] { proxy });
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                judgedCount++;
                
                if (proxy.status == "OK")
                {
                    TProxy pro = new TProxy();  
                    pro.proxy = proxy.proxy;
                    pro.country = proxy.country;
                    pro.username = proxy.username;
                    pro.password = proxy.password;
                    pro.type = proxy.type;
                    _proGoodList.Add(pro);
                }

                if (judgedCount >= _proxyList.Count)
                {
                    if (_proGoodList.Count == 0)
                    {
                        RemoveBannedIP();
                        return;
                    }

                    String file_path = "d:\\private.html";
                    FileStream fs_http = new FileStream(file_path, FileMode.Create);
                    StreamWriter sw_http = new StreamWriter(fs_http, Encoding.UTF8);
                    foreach (TProxy p in _proGoodList)
                    {
                        String[] t = p.proxy.Split(':');
                        sw_http.Write(t[0] + "|" + t[1] + "|" + p.country + "|" + p.username + "|" + p.password + "|" + p.type + "\n");
                    }
                    sw_http.Close();

                    /*
                    String fileleft_path = "C:\\private.html";
                    FileStream fsleft_http = new FileStream(fileleft_path, FileMode.Create);
                    StreamWriter swleft_http = new StreamWriter(fsleft_http, Encoding.UTF8);
                    foreach (TProxy p in _proxyList)
                    {
                        if (IsInList(p) == false)
                        {
                            String[] t = p.proxy.Split(':');
                            swleft_http.Write(t[0] + "|" + t[1] + "|" + p.country + "|" + p.username + "|" + p.password + "\n");
                        }
                    }
                    swleft_http.Close();
                    */

                    MessageBox.Show("Finished");
                }

            }
        }
    }
}
