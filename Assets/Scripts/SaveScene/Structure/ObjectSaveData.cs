using UnityEngine;

[System.Serializable]
public class ObjectSaveData
{
    public string prefabName;
    public string originalTag;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float scaleX, scaleY, scaleZ;

    public ObjectSaveData(
        string prefabName,
        string originalTag,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale
    )
    {
        this.prefabName  = prefabName;
        this.originalTag = originalTag;
        this.posX        = position.x;
        this.posY        = position.y;
        this.posZ        = position.z;
        this.rotX        = rotation.x;
        this.rotY        = rotation.y;
        this.rotZ        = rotation.z;
        this.rotW        = rotation.w;
        this.scaleX      = scale.x;
        this.scaleY      = scale.y;
        this.scaleZ      = scale.z;
    }
}

