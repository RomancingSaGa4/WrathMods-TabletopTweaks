﻿using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using System.Linq;
using TabletopTweaks.Config;
using TabletopTweaks.Extensions;
using TabletopTweaks.NewActions;
using TabletopTweaks.NewComponents;
using TabletopTweaks.Utilities;

namespace TabletopTweaks.Bugfixes.Abilities {
    class Spells {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        static class BlueprintsCache_Init_Patch {
            static bool Initialized;

            static void Postfix() {
                if (Initialized) return;
                Initialized = true;
                if (ModSettings.Fixes.Spells.DisableAll) { return; }
                Main.LogHeader("Patching Spells");
                PatchBelieveInYourself();
                PatchBestowCurseGreater();
                PatchCrusadersEdge();
                PatchMagicalVestment();
                PatchOdeToMiraculousMagicBuff();
                PatchRemoveFear();
                PatchSecondBreath();
                PatchShadowConjuration();
                PatchShadowEvocation();
                PatchShadowEvocationGreater();
                PatchWrachingRay();
            }
            static void PatchBelieveInYourself() {
                if (!ModSettings.Fixes.Spells.Enabled["BelieveInYourself"]) { return; }
                BlueprintAbility BelieveInYourself = Resources.GetBlueprint<BlueprintAbility>("3ed3cef7c267cb847bfd44ed4708b726");
                BlueprintAbilityReference[] BelieveInYourselfVariants = BelieveInYourself
                    .GetComponent<AbilityVariants>()
                    .Variants;
                foreach (BlueprintAbility Variant in BelieveInYourselfVariants) {
                    Variant.FlattenAllActions()
                        .OfType<ContextActionApplyBuff>()
                        .ForEach(b => {
                            var ContextRankConfig = b.Buff.GetComponent<ContextRankConfig>();
                            ContextRankConfig.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                            ContextRankConfig.m_Progression = ContextRankProgression.DivStep;
                            ContextRankConfig.m_StepLevel = 4;
                            Main.LogPatch("Patched", b.Buff);
                        });
                }
            }
            static void PatchBestowCurseGreater() {
                if (!ModSettings.Fixes.Spells.Enabled["BestowCurseGreater"]) { return; }
                var BestowCurseGreaterDeterioration = Resources.GetBlueprint<BlueprintAbility>("71196d7e6d6645247a058a3c3c9bb5fd");
                var BestowCurseGreaterFeebleBody = Resources.GetBlueprint<BlueprintAbility>("c74a7dfebd7b1004a80f7e59689dfadd");
                var BestowCurseGreaterIdiocy = Resources.GetBlueprint<BlueprintAbility>("f7739a453e2138b46978e9098a29b3fb");
                var BestowCurseGreaterWeakness = Resources.GetBlueprint<BlueprintAbility>("abb2d42dd9219eb41848ec56a8726d58");

                var BestowCurseGreaterDeteriorationCast = Resources.GetBlueprint<BlueprintAbility>("54606d540f5d3684d9f7d6e2e2be9b63");
                var BestowCurseGreaterFeebleBodyCast = Resources.GetBlueprint<BlueprintAbility>("292d630a5abae64499bb18057aaa24b4");
                var BestowCurseGreaterIdiocyCast = Resources.GetBlueprint<BlueprintAbility>("e0212142d2a426f43926edd4202996bb");
                var BestowCurseGreaterWeaknessCast = Resources.GetBlueprint<BlueprintAbility>("1168f36fac0bad64f965928206df7b86");

                var BestowCurseGreaterDeteriorationBuff = Resources.GetBlueprint<BlueprintBuff>("8f8835d083f31c547a39ebc26ae42159");
                var BestowCurseGreaterFeebleBodyBuff = Resources.GetBlueprint<BlueprintBuff>("28c9db77dfb1aa54a94e8a7413b1840a");
                var BestowCurseGreaterIdiocyBuff = Resources.GetBlueprint<BlueprintBuff>("493dcc29a21abd94d9adb579e1f40318");
                var BestowCurseGreaterWeaknessBuff = Resources.GetBlueprint<BlueprintBuff>("0493a9d25687d7e4682e250ae3ccb187");

                RebuildCurse(
                    BestowCurseGreaterDeterioration,
                    BestowCurseGreaterDeteriorationCast,
                    BestowCurseGreaterDeteriorationBuff);
                RebuildCurse(
                    BestowCurseGreaterFeebleBody,
                    BestowCurseGreaterFeebleBodyCast,
                    BestowCurseGreaterFeebleBodyBuff);
                RebuildCurse(
                    BestowCurseGreaterIdiocy,
                    BestowCurseGreaterIdiocyCast,
                    BestowCurseGreaterIdiocyBuff);
                RebuildCurse(
                    BestowCurseGreaterWeakness,
                    BestowCurseGreaterWeaknessCast,
                    BestowCurseGreaterWeaknessBuff);

                void RebuildCurse(BlueprintAbility curse, BlueprintAbility curseCast, BlueprintBuff curseBuff) {
                    curseCast.GetComponent<AbilityEffectStickyTouch>().m_TouchDeliveryAbility = curse.ToReference<BlueprintAbilityReference>();
                    Main.LogPatch("Patched", curseCast);
                    curse.GetComponent<AbilityEffectRunAction>()
                        .Actions.Actions.OfType<ContextActionConditionalSaved>().First()
                        .Failed.Actions.OfType<ContextActionApplyBuff>().First()
                        .m_Buff = curseBuff.ToReference<BlueprintBuffReference>();
                    Main.LogPatch("Patched", curse);
                    curseBuff.m_Icon = curse.m_Icon;
                    Main.LogPatch("Patched", curseBuff);
                }
            }
            static void PatchCrusadersEdge() {
                if (!ModSettings.Fixes.Spells.Enabled["CrusadersEdge"]) { return; }
                BlueprintBuff CrusadersEdgeBuff = Resources.GetBlueprint<BlueprintBuff>("7ca348639a91ae042967f796098e3bc3");
                CrusadersEdgeBuff.GetComponent<AddInitiatorAttackWithWeaponTrigger>().CriticalHit = true;
                Main.LogPatch("Patched", CrusadersEdgeBuff);
            }
            static void PatchMagicalVestment() {
                if (!ModSettings.Fixes.Spells.Enabled["MagicalVestment"]) { return; }
                PatchMagicalVestmentArmor();
                PatchMagicalVestmentShield();

                void PatchMagicalVestmentShield() {
                    var MagicalVestmentShield = Resources.GetBlueprint<BlueprintAbility>("adcda176d1756eb45bd5ec9592073b09");
                    var MagicalVestmentShieldBuff = Resources.GetBlueprint<BlueprintBuff>("2e8446f820936a44f951b50d70a82b16");
                    MagicalVestmentShield.GetComponent<AbilityEffectRunAction>().AddAction(Helpers.Create<EnhanceSheild>(a => {
                        a.EnchantLevel = new ContextValue {
                            ValueType = ContextValueType.Rank,
                            Value = 1,
                            ValueRank = AbilityRankType.ProjectilesCount
                        };

                        a.DurationValue = new ContextDurationValue {
                            m_IsExtendable = true,
                            Rate = DurationRate.Hours,
                            DiceCountValue = new ContextValue(),
                            BonusValue = new ContextValue()
                        };
                        a.DurationValue.BonusValue.ValueType = ContextValueType.Rank;

                        a.m_Enchantment = new BlueprintItemEnchantmentReference[] {
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("1d9b60d57afb45c4f9bb0a3c21bb3b98").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus1
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("d45bfd838c541bb40bde7b0bf0e1b684").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus2
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("51c51d841e9f16046a169729c13c4d4f").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus3
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("a23bcee56c9fcf64d863dafedb369387").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus4
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("15d7d6cbbf56bd744b37bbf9225ea83b").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus5
                        };
                    }));
                    var RankConfig = Helpers.CreateContextRankConfig();
                    RankConfig.m_Type = AbilityRankType.ProjectilesCount;
                    RankConfig.m_Progression = ContextRankProgression.DivStep;
                    RankConfig.m_StepLevel = 4;
                    RankConfig.m_Min = 1;
                    RankConfig.m_Max = 5;

                    MagicalVestmentShield.AddComponent(RankConfig);
                    MagicalVestmentShield.FlattenAllActions()
                        .OfType<ContextActionApplyBuff>().First().IsNotDispelable = true;

                    Main.LogPatch("Patched", MagicalVestmentShield);
                    MagicalVestmentShieldBuff.RemoveComponents<BlueprintComponent>();
                    Main.LogPatch("Patched", MagicalVestmentShieldBuff);
                }
                void PatchMagicalVestmentArmor() {
                    var MagicalVestmentArmor = Resources.GetBlueprint<BlueprintAbility>("956309af83352714aa7ee89fb4ecf201");
                    var MagicalVestmentArmorBuff = Resources.GetBlueprint<BlueprintBuff>("9e265139cf6c07c4fb8298cb8b646de9");
                    MagicalVestmentArmor.GetComponent<AbilityEffectRunAction>().AddAction(Helpers.Create<EnhanceArmor>(a => {
                        a.EnchantLevel = new ContextValue {
                            ValueType = ContextValueType.Rank,
                            Value = 1,
                            ValueRank = AbilityRankType.ProjectilesCount
                        };

                        a.DurationValue = new ContextDurationValue {
                            m_IsExtendable = true,
                            Rate = DurationRate.Hours,
                            DiceCountValue = new ContextValue(),
                            BonusValue = new ContextValue()
                        };
                        a.DurationValue.BonusValue.ValueType = ContextValueType.Rank;

                        a.m_Enchantment = new BlueprintItemEnchantmentReference[] {
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("1d9b60d57afb45c4f9bb0a3c21bb3b98").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus1
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("d45bfd838c541bb40bde7b0bf0e1b684").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus2
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("51c51d841e9f16046a169729c13c4d4f").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus3
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("a23bcee56c9fcf64d863dafedb369387").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus4
                            Resources.GetBlueprint<BlueprintArmorEnchantment>("15d7d6cbbf56bd744b37bbf9225ea83b").ToReference<BlueprintItemEnchantmentReference>(), // TemporaryArmorEnhancementBonus5
                        };
                    }));
                    var RankConfig = Helpers.CreateContextRankConfig();
                    RankConfig.m_Type = AbilityRankType.ProjectilesCount;
                    RankConfig.m_Progression = ContextRankProgression.DivStep;
                    RankConfig.m_StepLevel = 4;
                    RankConfig.m_Min = 1;
                    RankConfig.m_Max = 5;

                    MagicalVestmentArmor.AddComponent(RankConfig);
                    MagicalVestmentArmor.GetComponent<AbilityEffectRunAction>().Actions.Actions.OfType<ContextActionApplyBuff>().First().IsNotDispelable = true;
                    Main.LogPatch("Patched", MagicalVestmentArmor);
                    MagicalVestmentArmorBuff.RemoveComponents<BlueprintComponent>();
                    Main.LogPatch("Patched", MagicalVestmentArmorBuff);
                }
            }
            static void PatchOdeToMiraculousMagicBuff() {
                if (!ModSettings.Fixes.Spells.Enabled["OdeToMiraculousMagic"]) { return; }
                BlueprintBuff OdeToMiraculousMagicBuff = Resources.GetBlueprint<BlueprintBuff>("f6ef0e25745114d46bf16fd5a1d93cc9");
                IncreaseCastersSavingThrowTypeDC bonusSaveDC = Helpers.Create<IncreaseCastersSavingThrowTypeDC>(c => {
                    c.Type = SavingThrowType.Will;
                    c.BonusDC = 2;
                });
                OdeToMiraculousMagicBuff.AddComponent(bonusSaveDC);
                Main.LogPatch("Patched", OdeToMiraculousMagicBuff);
            }
            static void PatchRemoveFear() {
                if (!ModSettings.Fixes.Spells.Enabled["RemoveFear"]) { return; }
                var RemoveFear = Resources.GetBlueprint<BlueprintAbility>("55a037e514c0ee14a8e3ed14b47061de");
                var RemoveFearBuff = Resources.GetBlueprint<BlueprintBuff>("c5c86809a1c834e42a2eb33133e90a28");
                var suppressFear = Helpers.Create<SuppressBuffsPersistant>(c => {
                    c.Descriptor = SpellDescriptor.Frightened | SpellDescriptor.Shaken | SpellDescriptor.Fear;
                });
                RemoveFearBuff.RemoveComponents<AddConditionImmunity>();
                RemoveFearBuff.AddComponent(suppressFear);
                Main.LogPatch("Patched", RemoveFearBuff);
            }
            static void PatchSecondBreath() {
                if (ModSettings.Fixes.Spells.IsDisabled("SecondBreath")) { return; }
                var SecondBreath = Resources.GetBlueprint<BlueprintAbility>("d7e6f8a0369530341b50987d3ebdfe57");
                SecondBreath.Range = AbilityRange.Personal;
                SecondBreath.CanTargetFriends = true;
                SecondBreath.GetComponent<AbilityEffectRunAction>()
                    .AddAction(Helpers.Create<ContextRestoreResourcesFixed>(a => {
                        a.m_IsFullRestoreAllResources = true;
                    }));
                Main.LogPatch("Patched", SecondBreath);
            }
            static void PatchShadowConjuration() {
                if (!ModSettings.Fixes.Spells.Enabled["ShadowConjuration"]) { return; }
                var ShadowConjuration = Resources.GetBlueprint<BlueprintAbility>("caac251ca7601324bbe000372a0a1005");
                ShadowConjuration.AddToSpellList(SpellTools.SpellList.WizardSpellList, 4);
                Main.LogPatch("Patched", ShadowConjuration);
            }
            static void PatchShadowEvocation() {
                if (!ModSettings.Fixes.Spells.Enabled["ShadowEvocation"]) { return; }
                var ShadowEvocation = Resources.GetBlueprint<BlueprintAbility>("237427308e48c3341b3d532b9d3a001f");
                ShadowEvocation.AvailableMetamagic |= Metamagic.Empower
                    | Metamagic.Maximize
                    | Metamagic.Quicken
                    | Metamagic.Heighten
                    | Metamagic.Reach
                    | Metamagic.CompletelyNormal
                    | Metamagic.Persistent
                    | Metamagic.Selective
                    | Metamagic.Bolstered;
                Main.LogPatch("Patched", ShadowEvocation);
            }
            static void PatchShadowEvocationGreater() {
                if (!ModSettings.Fixes.Spells.Enabled["ShadowEvocationGreater"]) { return; }
                var ShadowEvocationGreater = Resources.GetBlueprint<BlueprintAbility>("3c4a2d4181482e84d9cd752ef8edc3b6");
                ShadowEvocationGreater.AvailableMetamagic |= Metamagic.Empower
                    | Metamagic.Maximize
                    | Metamagic.Quicken
                    | Metamagic.Heighten
                    | Metamagic.Reach
                    | Metamagic.CompletelyNormal
                    | Metamagic.Persistent
                    | Metamagic.Selective
                    | Metamagic.Bolstered;
                Main.LogPatch("Patched", ShadowEvocationGreater);
            }
            static void PatchWrachingRay() {
                if (!ModSettings.Fixes.Spells.Enabled["WrackingRay"]) { return; }
                var WrackingRay = Resources.GetBlueprint<BlueprintAbility>("1cde0691195feae45bab5b83ea3f221e");
                foreach (AbilityEffectRunAction component in WrackingRay.GetComponents<AbilityEffectRunAction>()) {
                    foreach (ContextActionDealDamage action in component.Actions.Actions.OfType<ContextActionDealDamage>()) {
                        action.Value.DiceType = Kingmaker.RuleSystem.DiceType.D4;
                    }
                }
                Main.LogPatch("Patched", WrackingRay);
            }
        }
    }
}
