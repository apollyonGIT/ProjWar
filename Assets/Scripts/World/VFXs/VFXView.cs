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
    }
}
