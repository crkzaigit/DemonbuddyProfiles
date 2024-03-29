﻿using System;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals;

namespace Trinity
{
    public class GameUI
    {
        private const ulong mercenaryOKHash = 1591817666218338490;
        private const ulong conversationSkipHash = 0x942F41B6B5346714;
        private const ulong talkToInteractButton1Hash = 0x8EB3A93FB1E49EB8;
        private const ulong confirmTimedDungeonOKHash = 0xF9E7B8A635A4F725;
        private const ulong genericOKHash = 0x891D21408238D18E;
        private const ulong partyLeaderBossAcceptHash = 0x69B3F61C0F8490B0;
        private const ulong partyFollowerBossAcceptHash = 0xF495983BA9BE450F;
        private const ulong potionButtonHash = 0xE1F43DD874E42728;
        private const ulong bountyRewardDialogHash = 0x278249110947CA00;
        private const ulong gamePotionHash = 0xE1F43DD874E42728;

        public static UIElement GamePotion
        {
            get { return UIElement.FromHash(gamePotionHash); }
        }

        public static UIElement BountyRewardDialog
        {
            get { return UIElement.FromHash(bountyRewardDialogHash); }
        }

        public static UIElement PotionButton
        {
            get
            {
                return UIElement.FromHash(potionButtonHash);
            }
        }

        //private static UIElement _confirmTimedDungeonOK;
        //public static UIElement ConfirmTimedDungeonOK { get { try { return _confirmTimedDungeonOK ?? (_confirmTimedDungeonOK = UIElement.FromHash(confirmTimedDungeonOKHash)); } catch { return null; } } }
        public static UIElement ConfirmTimedDungeonOK
        {
            get
            {
                return UIElement.FromHash(confirmTimedDungeonOKHash);
            }
        }

        //private static UIElement _mercenaryOKButton;
        //public static UIElement MercenaryOKButton { get { try { return _mercenaryOKButton ?? (_mercenaryOKButton = UIElement.FromHash(mercenaryOKHash)); } catch { return null; } } }
        public static UIElement MercenaryOKButton
        {
            get
            {
                return UIElement.FromHash(mercenaryOKHash);
            }
        }

        //private static UIElement _conversationSkipButton;
        //public static UIElement ConversationSkipButton { get { try { return _conversationSkipButton ?? (_conversationSkipButton = UIElement.FromHash(conversationSkipHash)); } catch { return null; } } }
        public static UIElement ConversationSkipButton
        {
            get
            {
                return UIElement.FromHash(conversationSkipHash);
            }
        }

        //private static UIElement _partyLeaderBossAccept;
        //public static UIElement PartyLeaderBossAccept { get { try { return _partyLeaderBossAccept ?? (_partyLeaderBossAccept = UIElement.FromHash(partyLeaderBossAcceptHash)); } catch { return null; } } }
        public static UIElement PartyLeaderBossAccept
        {
            get
            {
                return UIElement.FromHash(partyLeaderBossAcceptHash);
            }
        }

        //private static UIElement _partyFollowerBossAccept;
        //public static UIElement PartyFollowerBossAccept { get { try { return _partyFollowerBossAccept ?? (_partyFollowerBossAccept = UIElement.FromHash(partyFollowerBossAcceptHash)); } catch { return null; } } }
        public static UIElement PartyFollowerBossAccept
        {
            get
            {
                return UIElement.FromHash(0xF495983BA9BE450F);
            }
        }

        //private static UIElement _genericOK;
        //public static UIElement GenericOK { get { try { return _genericOK ?? (_genericOK = UIElement.FromHash(genericOKHash)); } catch { return null; } } }
        public static UIElement GenericOK
        {
            get
            {
                return UIElement.FromHash(0x891D21408238D18E);
            }
        }

        //private static UIElement _talktoInteractButton1;
        //public static UIElement TalktoInteractButton1 { get { try { return _talktoInteractButton1 ?? (_talktoInteractButton1 = UIElement.FromHash(talkToInteractButton1Hash)); } catch { return null; } } }
        public static UIElement TalktoInteractButton1
        {
            get
            {
                return UIElement.FromHash(talkToInteractButton1Hash);
            }
        }

        public static bool IsElementVisible(UIElement element)
        {
            if (element == null)
                return false;
            if (!element.IsValid)
                return false;
            if (!element.IsVisible)
                return false;

            return true;
        }

        public static bool SafeClickElement(UIElement element, string name = "", bool fireWorldTransfer = false)
        {
            if (!ZetaDia.Me.IsValid)
                return false;
            if (!IsElementVisible(element))
                return false;
            if (fireWorldTransfer)
                GameEvents.FireWorldTransferStart();

            Logger.Log("Clicking UI element {0} ({1})", name, element.BaseAddress);
            element.Click();
            return true;
        }

        private static DateTime _lastCheckedUiButtons = DateTime.MinValue;
        public static void SafeClickUIButtons()
        {
            if (ZetaDia.IsLoadingWorld)
                return;

            if (ZetaDia.IsInGame && SafeClickElement(BountyRewardDialog, "Bounty Reward Dialog"))
                return;
            if (ZetaDia.IsInGame && SafeClickElement(ConversationSkipButton, "Conversation Button"))
                return;
            if (ZetaDia.IsInGame && SafeClickElement(PartyLeaderBossAccept, "Party Leader Boss Accept", true))
                return;
            if (ZetaDia.IsInGame && SafeClickElement(PartyFollowerBossAccept, "Party Follower Boss Accept", true))
                return;
            if (ZetaDia.IsInGame && SafeClickElement(TalktoInteractButton1, "Conversation Button"))
                return;

            if (DateTime.UtcNow.Subtract(_lastCheckedUiButtons).TotalMilliseconds <= 500)
                return;

            _lastCheckedUiButtons = DateTime.UtcNow;

            int loopingAnimationEndTime = 0;
            try
            {
                loopingAnimationEndTime = ZetaDia.Me.LoopingAnimationEndTime;
            }
            catch (Exception ex) { Logger.LogDebug("Error in getting LoopingAnimationEndTime {0}", ex.Message); }

            if (loopingAnimationEndTime > 0)
                return;
            if (ZetaDia.IsInGame && SafeClickElement(MercenaryOKButton, "Mercenary OK Button"))
                return;
            if (SafeClickElement(GenericOK, "GenericOK"))
                return;
            if (SafeClickElement(UIElements.ConfirmationDialogOkButton, "ConfirmationDialogOKButton", true))
                return;
            if (ZetaDia.IsInGame && SafeClickElement(ConfirmTimedDungeonOK, "Confirm Timed Dungeon OK Button", true))
                return;
        }

        public static bool IsPartyDialogVisible
        {
            get
            {
                return IsElementVisible(PartyFollowerBossAccept) || IsElementVisible(PartyLeaderBossAccept);
            }
        }
    }
}
