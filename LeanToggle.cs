using HarmonyLib;
using UnityEngine;

namespace DvMod.LeanToggle
{
    public static class LeanToggle
    {
        private static LocomotionInputWrapper.LeanDirection currentLean = LocomotionInputWrapper.LeanDirection.NotLeaning;
        private static float? keypressStartTime;

        private static LocomotionInputWrapper.LeanDirection KeypressDirection()
        {
            if (KeyBindings.leanLeftKeys.IsPressed())
                if (KeyBindings.leanRightKeys.IsPressed())
                    return LocomotionInputWrapper.LeanDirection.NotLeaning;
                else
                    return LocomotionInputWrapper.LeanDirection.LeaningLeft;
            else
                if (KeyBindings.leanRightKeys.IsPressed())
                    return LocomotionInputWrapper.LeanDirection.LeaningRight;
                else
                    return LocomotionInputWrapper.LeanDirection.NotLeaning;
        }

        private static bool TapTimeoutExpired() =>
            Time.time - (keypressStartTime ?? float.PositiveInfinity) > (float)Main.settings.tapThresholdMillis / 1000;

        private static void CheckLeft()
        {
            switch (KeypressDirection())
            {
            case LocomotionInputWrapper.LeanDirection.LeaningLeft:
                if (keypressStartTime == null)
                {
                    currentLean = LocomotionInputWrapper.LeanDirection.NotLeaning;
                    keypressStartTime = Time.time;
                }
                break;
            case LocomotionInputWrapper.LeanDirection.LeaningRight:
                currentLean = LocomotionInputWrapper.LeanDirection.LeaningRight;
                keypressStartTime = Time.time;
                break;
            default:
                if (TapTimeoutExpired())
                    currentLean = LocomotionInputWrapper.LeanDirection.NotLeaning;
                keypressStartTime = null;
                break;
            }
        }

        private static void CheckNotLeaning()
        {
            switch (KeypressDirection())
            {
            case LocomotionInputWrapper.LeanDirection.LeaningLeft:
                if (keypressStartTime == null)
                {
                    currentLean = LocomotionInputWrapper.LeanDirection.LeaningLeft;
                    keypressStartTime = Time.time;
                }
                break;
            case LocomotionInputWrapper.LeanDirection.LeaningRight:
                if (keypressStartTime == null)
                {
                    currentLean = LocomotionInputWrapper.LeanDirection.LeaningRight;
                    keypressStartTime = Time.time;
                }
                break;
            default:
                keypressStartTime = null;
                break;
            }
        }

        private static void CheckRight()
        {
            switch (KeypressDirection())
            {
            case LocomotionInputWrapper.LeanDirection.LeaningLeft:
                currentLean = LocomotionInputWrapper.LeanDirection.LeaningLeft;
                keypressStartTime = Time.time;
                break;
            case LocomotionInputWrapper.LeanDirection.LeaningRight:
                if (keypressStartTime == null)
                {
                    currentLean = LocomotionInputWrapper.LeanDirection.NotLeaning;
                    keypressStartTime = Time.time;
                }
                break;
            default:
                if (TapTimeoutExpired())
                    currentLean = LocomotionInputWrapper.LeanDirection.NotLeaning;
                keypressStartTime = null;
                break;
            }
        }

        [HarmonyPatch(typeof(LocomotionInputNonVr), "get_" + nameof(LocomotionInputNonVr.LeanValue))]
        public static class LeanValuePatch
        {
            public static bool Prefix(ref LocomotionInputWrapper.LeanDirection __result)
            {
                switch (currentLean)
                {
                case LocomotionInputWrapper.LeanDirection.LeaningLeft:
                    CheckLeft();
                    break;
                case LocomotionInputWrapper.LeanDirection.NotLeaning:
                    CheckNotLeaning();
                    break;
                case LocomotionInputWrapper.LeanDirection.LeaningRight:
                    CheckRight();
                    break;
                }
                __result = currentLean;
                return false;
            }
        }
    }
}