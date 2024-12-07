using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FramesAR.Data
{
    [System.Serializable]
    public class PiecePost
    {
        public string anchorId;
        public string pieceid;
        public string frameName;
        public string faceName;
        public string imageUrl;
        public float latitude;
        public float longitude;
        public Vector3 arPosition;
        public Quaternion arRotation;
        public long timestamp;
    }
}