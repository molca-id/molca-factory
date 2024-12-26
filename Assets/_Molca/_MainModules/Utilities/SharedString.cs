using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Molca.Utils
{
    [CreateAssetMenu(fileName = "New SharedString", menuName = "Molca/Utils/Shared String")]
    public class SharedString : ScriptableObject
    {
        public string key;
        public string value;
    }
}