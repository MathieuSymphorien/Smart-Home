using UnityEngine;

[System.Serializable]
public class ObjectSaveData
{
    public string prefabName;    // Nom du prefab
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;  // Quaternion (rotation)

    public ObjectSaveData(string prefabName, Vector3 position, Quaternion rotation)
    {
        this.prefabName = prefabName;
        this.posX = position.x;
        this.posY = position.y;
        this.posZ = position.z;
        this.rotX = rotation.x;
        this.rotY = rotation.y;
        this.rotZ = rotation.z;
        this.rotW = rotation.w;
    }
}
