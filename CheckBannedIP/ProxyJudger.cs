using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckBannedIP
{
    class ProxyJudger
    {
        public ProxyJudger(TProxy p)
        {
            _proxy = p;
        }

        private TProxy _proxy;

        public delegate void onJudgeDoneDelegate(TProxyCheck proxy);
        public event onJudgeDoneDelegate onJudgeDone;

        public void doProxyJudge(object ob)
        {
            TAccount ac = (TAccount)ob;
            TProxyCheck pro = new TProxyCheck();
            pro.proxy = _proxy.proxy;
            pro.country = _proxy.country;
            pro.username = _proxy.username;
            pro.password = _proxy.password;
            pro.type = _proxy.type;
            NetworkClass network = new NetworkClass(ac, _proxy);

            if (network.CheckIPStart())//验证代理是否被ban
            {
                pro.status = "OK";
            }

            if (onJudgeDone != null)
            {
                onJudgeDone(pro);
            }
        }
    }
}
