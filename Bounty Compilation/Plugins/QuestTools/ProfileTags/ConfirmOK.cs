﻿using Zeta.Bot.Profile;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace QuestTools.ProfileTags
{
    [XmlElement("ConfirmOK")]
    class ConfirmOkTag : ProfileBehavior
    {
        public ConfirmOkTag() { }

        private bool _isDone;
        public override bool IsDone
        {
            get { return _isDone; }
        }

        protected override Composite CreateBehavior()
        {
            return 
            new PrioritySelector(
                new Decorator(ret => GameUI.IsElementVisible(UIElements.ConfirmationDialogOkButton),
                    new Sequence(
                        new Action(ret => Logger.Log("Clicking ConfirmationDialogOkButton")),
                        new Action(ret => GameUI.SafeClickElement(UIElements.ConfirmationDialogOkButton, "ConfirmationDialogOKButton")),
                        new Action(ret => _isDone = true)
                    )
                ),
                new Decorator(ret => GameUI.IsElementVisible(GameUI.GenericOK),
                    new Sequence(
                        new Action(ret => Logger.Log("Clicking GenericOK")),
                        new Action(ret => GameUI.SafeClickElement(UIElements.ConfirmationDialogOkButton, "GenericOK")),
                        new Action(ret => _isDone = true)
                    )
                ),
                new Action(ret => _isDone = true)
            );            
        }

        public override void ResetCachedDone()
        {
            _isDone = false;
            base.ResetCachedDone();
        }

    }
}
