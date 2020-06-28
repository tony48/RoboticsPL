using System;
using System.Reflection;

namespace RoboticsPL
{
    public static class KJRWrapper
    {
        public static Type KJRMultiJointManager = null;
        public static Type KJRManager = null;
        public static MethodInfo RemoveAllJoints;
        public static PropertyInfo ManagerInstance;

        private static void Init()
        {
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == "KerbalJointReinforcement.KJRMultiJointManager")
                {
                    KJRMultiJointManager = t;
                }
            });
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == "KerbalJointReinforcement.KJRManager")
                {
                    KJRManager = t;
                }
            });
            ManagerInstance = KJRManager.GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic);
            RemoveAllJoints =
                KJRManager.GetMethod("RemovePartJoints", BindingFlags.Instance | BindingFlags.NonPublic);
        }
    }
}