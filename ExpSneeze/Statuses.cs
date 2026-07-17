using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using System;
using UnityEngine;

namespace ExpSneeze
{
    [StatusOptions(Key = "expsneeze.sneeze", SaveEnabled = false)]
    public sealed class SneezeStatus : BodyStatus
    {
        public Body body = null;
        public float SneezeTime;
        public float SneezeTimeMax = 300f;
        public bool Sneezing;
        public float SneezeCooldown;
        public readonly AudioClip audioClip = AssetLoader.LoadEmbeddedAudio("sneeze.wav");
        public AudioSource audioSource;
        public readonly Sprite moodleSprite = AssetLoader.LoadEmbeddedSprite("sneeze.png", 80f);
        public bool WarningDialogueDone = false;
    }

    [HarmonyPatch(typeof(PlayerCamera), "Update")]
    public static class BodyUpdateStatusPatch
    {
        [HarmonyPostfix]
        private static void Postfix(PlayerCamera __instance)
        {
            SneezeStatus status = __instance.body.GetStatus<SneezeStatus>();
            status.SneezeTimeMax = PlayerPrefs.GetFloat("ExpSneeze_SneezeTimeMax", 450f);
            if (status.body == null)
            {
                status.body = __instance.body;
                status.audioSource = __instance.body.gameObject.AddComponent<AudioSource>();
                status.audioSource.clip = status.audioClip;
                status.audioSource.volume = (float)Settings.GetSetting<SettingFloat>("mastervolume").GetValue();
            }

            if (__instance.body.HasWearable("dustmask") || __instance.body.HasWearable("scubadivinggear"))
            {
                status.SneezeTime = 0f;
                status.Sneezing = false;
                status.SneezeCooldown = 0f;
            }

            if (!__instance.body.sleeping && !__instance.body.vomiter.vomiting && !__instance.body.harmer.harming && !__instance.body.inCardiacArrest && __instance.body.respiratoryRate > 0f && __instance.body.conscious && __instance.body.LimbByName("Head").muscleHealth > 48f)
            {
                if (__instance.body.HasWearable("makeshiftmask"))
                {
                    status.SneezeTime += Time.deltaTime * 0.33f;
                }
                else
                {
                    status.SneezeTime += Time.deltaTime;
                }
            }
            if (status.SneezeTime >= status.SneezeTimeMax - 5f && status.SneezeTime < status.SneezeTimeMax - 2f)
            {
                MoodleRegistry.AddMoodle(
                    1,
                    status.moodleSprite,
                    LocaleRegistry.Get("other", "expsneeze.status.sneeze", "About to sneeze"),
                    LocaleRegistry.Get("other", "expsneeze.status.sneezedsc", "Buildup of foreign particles in your nostrils has irritated your nose. Forceful nasal expulsion imminent. In other words, it's gonna blow!"),
                    holdSeconds: 0f,
                    key: "sneeze"
                );
            }
            if (status.SneezeTime >= status.SneezeTimeMax - 2f && status.SneezeTime < status.SneezeTimeMax - 1f)
            {
                if (!status.WarningDialogueDone)
                {
                    string[] rand = [
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze1", "Ah...!"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze2", "Hh..."),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze3", "I'm gonna...!"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze4", "Uh oh...!"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze5", "*sharp inhale*"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze6", "Sneeze incoming...!")
                    ];
                    __instance.body.talker.Talk(rand[UnityEngine.Random.Range(0,5)], force: true, resetTalkTimer: true);
                    status.WarningDialogueDone = true;
                }
                MoodleRegistry.AddMoodle(
                    2,
                    status.moodleSprite,
                    LocaleRegistry.Get("other", "expsneeze.status.sneeze", "About to sneeze"),
                    LocaleRegistry.Get("other", "expsneeze.status.sneezedsc", "Buildup of foreign particles in your nostrils has irritated your nose. Forceful nasal expulsion imminent. In other words, it's gonna blow!"),
                    holdSeconds: 0f,
                    key: "sneeze"
                );
            }
            if (status.SneezeTime >= status.SneezeTimeMax - 1f && status.SneezeTime < status.SneezeTimeMax)
            {
                MoodleRegistry.AddMoodle(
                    3,
                    status.moodleSprite,
                    LocaleRegistry.Get("other", "expsneeze.status.sneeze", "About to sneeze"),
                    LocaleRegistry.Get("other", "expsneeze.status.sneezedsc", "Buildup of foreign particles in your nostrils has irritated your nose. Forceful nasal expulsion imminent. In other words, it's gonna blow!"),
                    critical: true,
                    holdSeconds: 0f,
                    key: "sneeze"
                );
            }

            if (status.SneezeTime >= status.SneezeTimeMax)
            {
                status.Sneezing = true;
            }

            if (status.Sneezing)
            {
                if (status.SneezeCooldown <= 0f)
                {
                    __instance.body.consciousness = Mathf.Max(Mathf.Min(72f, __instance.body.consciousness), __instance.body.consciousness - 0.47f);
                    if (__instance.body.consciousness <= 72f)
                    {
                        //Plugin.Logger.LogInfo("ACHOO!");
                        string[] rand = [
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze1", "ACHOO!"),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze2", "CHOO!"),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze3", "CHU!"),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze4", "CHAOW!"),
                        ];

                        __instance.body.talker.Talk(rand[UnityEngine.Random.Range(0,3)], force: true, resetTalkTimer: true);
                        status.audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                        status.audioSource.Play();
                        if (UnityEngine.Random.Range(1, 11) == PlayerPrefs.GetInt("ExpSneeze_DropChance", 1))
                        {
                            __instance.body.DropItem(2);
                        }
                        __instance.body.rb.velocity += new Vector2(-Mathf.Sign(__instance.body.targetLookPos.x) * PlayerPrefs.GetFloat("ExpSneeze_SneezeForce", 5f), 0f);
                        if (PlayerPrefs.GetInt("ExpSneeze_Ragdoll", 0) == 1)
                        {
                            __instance.body.Ragdoll();
                            foreach (Limb limb in __instance.body.limbs)
                            {
                                limb.rb.velocity += new Vector2(-Mathf.Sign(__instance.body.targetLookPos.x) * PlayerPrefs.GetFloat("ExpSneeze_SneezeForce", 5f), 0f);
                            }
                        }
                        __instance.body.stamina -= 1;
                        __instance.shaker.Shake(10f);
                        __instance.body.consciousness -= 17f;
                        status.SneezeCooldown = 1f;
                        if (UnityEngine.Random.Range(1, 11) <= 5)
                        {
                            status.Sneezing = false;
                            status.SneezeTime = 0f;
                            //Plugin.Logger.LogInfo("Done sneezing :3");
                        }
                    }
                }
            }

            if (status.SneezeCooldown > 0f)
            {
                if (status.Sneezing)
                {
                    __instance.body.consciousness = Mathf.Min(90f, __instance.body.consciousness + 0.1f);
                }
                else
                {
                    __instance.body.consciousness = Mathf.Min(90f, __instance.body.consciousness + 0.58f);
                    if (status.WarningDialogueDone)
                    {
                        string[] rand = [
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend1", "Excuse me..."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend2", "Bless you..."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend3", "Fucking hell..."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend4", "That one hurt. Ow."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend5", "Whew."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend6", "Everything is so dusty, all the time, forever..."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend7", "I really need a mask or something."),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneezeend8", "*sniff*"),
                        ];

                        int rng = UnityEngine.Random.Range(0, 8);
                        while (rng == 6 && (__instance.body.HasWearable("makeshiftmask") || __instance.body.HasWearable("dustmask") || __instance.body.HasWearable("scubadivinggear")))
                        {
                            rng = UnityEngine.Random.Range(0, 8);
                        }

                        status.WarningDialogueDone = false;
                        __instance.body.talker.TalkDelayed(UnityEngine.Random.Range(1f, 2.5f), rand[rng], force: true, resetTalkTimer: true);
                        __instance.body.talker.PromptTraderResponse("sneeze");
                    }
                }
                    status.SneezeCooldown -= Time.deltaTime;
            }
        }
    }

    [HarmonyPatch(typeof(Body), "Attack")]
    public static class BodyAttackPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Body __instance, ref AttackInfo atk)
        {
            Vector2 vector = (__instance.targetLookPos - __instance.limbs[1].transform.position).normalized;
            RaycastHit2D[] array = Physics2D.RaycastAll(__instance.limbs[1].transform.position, vector, atk.distance);
            bool flag = false;
            foreach (RaycastHit2D raycastHit2D in array)
            {
                if (raycastHit2D.transform != __instance.transform)
                {
                    if (raycastHit2D.transform.CompareTag("BlockGround"))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (atk.physicalSwing && flag)
            {
                SneezeStatus status = __instance.GetStatus<SneezeStatus>();
                if (status.SneezeTime >= status.SneezeTimeMax - 5f) return;
                if (__instance.HasWearable("makeshiftmask"))
                {
                    status.SneezeTime += atk.cooldown / 4f;
                }
                else
                {
                    status.SneezeTime += atk.cooldown / 2f;
                }
                if (status.SneezeTime > status.SneezeTimeMax - 5f) status.SneezeTime = status.SneezeTimeMax - 5f;
                //Plugin.Logger.LogInfo($"SneezeTime: {status.SneezeTime}");
            }
        }
    }

    [HarmonyPatch(typeof(TraderScript), "PromptResponse")]
    public static class TraderPromptResponsePatches
    {
        [HarmonyPrefix]
        private static bool Prefix(TraderScript __instance, ref string type)
        {
            if (type == "sneeze")
            {
                if (__instance.build.health <= 200f || !__instance.startedConvo || __instance.reputation <= 30f || __instance.hostile || Vector2.Distance(__instance.transform.position, __instance.body.transform.position) > 15f)
                {
                    return false;
                }

                string _char = "_experiment";
                if (__instance.character == 1) _char = "_milky";
                if (__instance.character == 2) _char = "_dune";
                int rand = UnityEngine.Random.Range(1, 3);

                if (__instance.reputation <= 75f)
                {
                    string text = LocaleRegistry.Get("other", type + "responsebad" + _char + rand, "test");
                    __instance.StartCoroutine(__instance.DelayedTalk(text));
                    return false;
                }
                else
                {
                    string text = LocaleRegistry.Get("other", type + "response" + _char + rand, "test");
                    __instance.StartCoroutine(__instance.DelayedTalk(text));
                    return false;
                }
            }
            return true;
        }
    }
}
