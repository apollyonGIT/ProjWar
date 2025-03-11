using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Relic
{
    public class RelicMgrView : MonoBehaviour, IRelicMgrView
    {
        RelicMgr owner;

        public List<RelicView> relic_views = new();
        public RelicView prefab;

        public Transform content;
        void IModelView<RelicMgr>.attach(RelicMgr owner)
        {
            
        }
        void IModelView<RelicMgr>.detach(RelicMgr owner)
        {
            
        }
        void IRelicMgrView.notify_add_rellic(Relic relic)
        {
            var r_obj = Instantiate(prefab, content, false);
            r_obj.Init(relic);

            r_obj.gameObject.SetActive(true);
            relic_views.Add(r_obj);
        }
        void IRelicMgrView.notify_remove_rellic(Relic relic)
        {
            for(int i = 0; i < relic_views.Count; i++)
            {
                if (relic_views[i].data == relic)
                {
                    Destroy(relic_views[i].gameObject);
                    relic_views.RemoveAt(i);
                    break;
                }
            }
        }

    }
}
