﻿using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using System.Linq;

namespace TabletopTweaks.NewComponents {
    [TypeId("0542dd3cbb5949a7b120f2165758db9b")]
    class IgnoreArmorMaxDexBonus: UnitFactComponentDelegate,
        IInitiatorRulebookHandler<RuleCalculateArmorMaxDexBonusLimit>,
        IRulebookHandler<RuleCalculateArmorMaxDexBonusLimit>,
        ISubscriber, IInitiatorRulebookSubscriber {

        public override void OnTurnOn() {
            base.OnTurnOn();
            if (Owner.Body.Armor.HasArmor && Owner.Body.Armor.Armor.Blueprint.IsArmor) {
                Owner.Body.Armor.Armor.RecalculateStats();
                Owner.Body.Armor.Armor.RecalculateMaxDexBonus();
            }
        }

        public void OnEventAboutToTrigger(RuleCalculateArmorMaxDexBonusLimit evt) {
        }

        public void OnEventDidTrigger(RuleCalculateArmorMaxDexBonusLimit evt) {
            if (!evt.Armor.Blueprint.IsShield && CheckCategory && Categorys.Contains(evt.Armor.ArmorType())) {
                evt.Result = null;
            }
            if (evt.Armor.Blueprint.IsShield && CheckCategory && Categorys.Contains(evt.Armor.Blueprint.ProficiencyGroup)) {
                evt.Result = null;
            }
            if (!CheckCategory) {
                evt.Result = null;
            }
        }

        public bool CheckCategory = true;
        [ShowIf("CheckCategory")]
        public ArmorProficiencyGroup[] Categorys;
    }
}