using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace ModLoader.Framework
{
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    public abstract class VtolMod : MonoBehaviour
    {
        public abstract void UnLoad();
    }
}