using UnityEngine;

[System.Serializable]
public class ObjectSaveData
{
    public string prefabKey;    
    public string instanceName;     
    public string originalTag;      
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float scaleX, scaleY, scaleZ;
    public string[] linkedLightNames; 

    public float range, fov;  

    public ObjectSaveData(string prefabKey, string instanceName, string tag,
                      Vector3 pos, Quaternion rot, Vector3 scale,
                      float range, float fov, string[] linkedLights = null)
    {
        this.prefabKey       = prefabKey;
        this.instanceName    = instanceName;
        this.originalTag     = tag;
        this.posX = pos.x; this.posY = pos.y; this.posZ = pos.z;
        this.rotX = rot.x; this.rotY = rot.y; this.rotZ = rot.z; this.rotW = rot.w;
        this.scaleX = scale.x; this.scaleY = scale.y; this.scaleZ = scale.z;

        this.linkedLightNames = linkedLights;
        this.range = range;
        this.fov   = fov;
    }
}