namespace RoboticsPL
{
    public class KJRExclude : PartModule, IJointLockState
    {
        public bool IsJointUnlocked()
        {
            return true;
        }
    }
}