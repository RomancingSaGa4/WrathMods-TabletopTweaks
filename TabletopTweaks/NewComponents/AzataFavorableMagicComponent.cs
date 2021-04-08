﻿using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;

namespace TabletopTweaks.NewComponents {
    class AzataFavorableMagicComponent : UnitFactComponentDelegate, 
		IWasRoll, 
		IGlobalSubscriber, 
		ISubscriber {

		public override void OnActivate() {
			base.Owner.State.Features.AzataFavorableMagic.Retain();
		}

		public override void OnDeactivate() {
			base.Owner.State.Features.AzataFavorableMagic.Release();
		}

		public void WasRoll(RulebookEvent ruleEvent, RuleRollD20 ruleRoll) {
			if (ruleEvent != null) {
				if (ruleEvent is RuleCheckConcentration || ruleEvent is RuleSpellResistanceCheck || ruleEvent is RuleCheckCastingDefensively) {
					CheckReroll(ruleEvent, ruleRoll);
					return;
				}
				RuleSavingThrow ruleSavingThrow;
				if ((ruleSavingThrow = (ruleEvent as RuleSavingThrow)) != null) {
					RuleSavingThrow ruleEvent2 = ruleSavingThrow;
					CheckReroll(ruleEvent2, ruleRoll);
				}
			}
		}

		private void CheckReroll(RuleSavingThrow ruleEvent, RuleRollD20 ruleRoll) {
			if (ruleRoll.Reason.Caster == Owner) {
				ruleRoll.Reroll(Fact, false);
			}
		}

		private void CheckReroll(RulebookEvent ruleEvent, RuleRollD20 ruleRoll) {
			if (ruleEvent.Initiator == base.Owner) {
				ruleRoll.Reroll(Fact, true);
			}
		}
	}
}
