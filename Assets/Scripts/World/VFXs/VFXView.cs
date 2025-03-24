using System;
using UnityEngine;

namespace World.VFXs
{
    public class VFXView :MonoBehaviour
    {
        public VFX data;
        public VFXMgr owner;
        public void Init(VFX v,VFXMgr owner)
        {
            data = v;
            this.owner = owner;

            transform.position = new Vector3(v.pos.x, v.pos.y, 10);
        }


        // 用于每帧跟车移动的特效
        public void UpdatePos()
        {
            var cv = WorldContext.instance.caravan_pos;
            transform.position = new Vector3(data.pos.x + cv.x , data.pos.y + cv.y, 10);
        }
    }
}
