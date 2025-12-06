using UnityEngine;

namespace InstanceResetToDefault
{
    public interface IResettableUI
    {
        void ResetUI();
    }

    public interface IResettableData
    {
        void ResetData();
    }
}

