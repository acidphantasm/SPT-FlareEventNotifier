using System;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using SPT.Reflection.Patching;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using HarmonyLib;
using UnityEngine;

namespace Terkoiz.FlareEventNotifier
{
    using EFT.GlobalEvents;

    public class FlareEventHookPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetType = typeof(AbstractGame).Assembly.GetTypes().SingleOrDefault(t => t.GetProperty("ZoneEventType") != null) ?? throw new Exception("Could not locate target type");

            return AccessTools.DeclaredMethod(targetType, "Invoke");
        }

        [PatchPrefix]
        public static void PatchPrefix(FlareEventType flareType, FlareShootZoneEvent.EZoneEventType eventType, string playerProfileID)
        {
            if (flareType != FlareEventType.ExitActivate)
            {
                return;
            }

            if (eventType != FlareShootZoneEvent.EZoneEventType.FiredPlayerAddedInShotList && eventType != FlareShootZoneEvent.EZoneEventType.PlayerByPartyAddedInShotList)
            {
                return;
            }

            var localPlayer = GetLocalPlayerFromWorld();
            if (localPlayer != null && localPlayer.ProfileId != playerProfileID)
            {
                return;
            }

            NotificationManager.DisplayNotification(new ExfilFlareSuccessNotification());
        }

        /// <summary>
        /// Gets the current <see cref="Player"/> instance if it's available
        /// </summary>
        /// <returns>Local <see cref="Player"/> instance; returns null if the game is not in raid</returns>
        private static Player GetLocalPlayerFromWorld()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null || gameWorld.MainPlayer == null)
            {
                return null;
            }

            return gameWorld.MainPlayer;
        }
    }

    public class ExfilFlareSuccessNotification : Notification
    {
        public ExfilFlareSuccessNotification()
        {
            Duration = ENotificationDurationType.Long;
        }

        public override string Description { get => "Exfil activated"; }

        public override ENotificationIconType Icon { get => ENotificationIconType.Default; }

        public override Color? TextColor { get => Color.green; }
    }
}