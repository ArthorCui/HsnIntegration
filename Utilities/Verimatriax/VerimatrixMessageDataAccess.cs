using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Utilities.Verimatriax
{
    public class VerimatrixMessageDataAccess
    {
        private static ConcurrentDictionary<string, VerimatrixLogEntity> ConCurrentDic = new ConcurrentDictionary<string, VerimatrixLogEntity>();

        public static void LogRequestMessage(string requestMessage, VerimatrixLogEntity entity)
        {
            try
            {
                if (!string.IsNullOrEmpty(requestMessage))
                {
                    var key = requestMessage.GetMessageNumber();
                    ConCurrentDic.TryAdd(key, entity);
                }
            }
            catch (Exception)
            {
                //Logger.Error(string.Format("Log request message to concurrent dictionary error:{0}", ex.Message));
            }
        }

        public static VerimatrixLogEntity GetRequestData(string responseMessage)
        {
            VerimatrixLogEntity entity;
            var key = responseMessage.GetMessageNumber();

            if (ConCurrentDic != null)
            {
                ConCurrentDic.TryRemove(key, out entity);
                return entity;
            }
            return null;
        }

        public static void Inspection(int expireTime)
        {
            if (ConCurrentDic == null)
            {
                ConCurrentDic = new ConcurrentDictionary<string, VerimatrixLogEntity>();
                return;
            }

            var collection = ConCurrentDic.Where(x => x.Value.sendTime.AddMilliseconds(expireTime) < DateTime.Now).ToList();
            if (collection != null && collection.Count() > 0)
            {
                foreach (var item in collection)
                {
                    VerimatrixLogEntity entity;
                    ConCurrentDic.TryRemove(item.Key, out entity);
                }
            }
        }

        public static void Reset()
        {
            ConCurrentDic = default(ConcurrentDictionary<string, VerimatrixLogEntity>);
        }
    }
}
