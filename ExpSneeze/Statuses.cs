using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using ModNamespace;
using System;
using UnityEngine;

namespace ExpSneeze
{
    [StatusOptions(Key = "expsneeze.sneeze", SaveEnabled = false)]
    public sealed class SneezeStatus : BodyStatus
    {
        public Body body = null;
        public float SneezeTime;
        public bool Sneezing;
        public float SneezeCooldown;
        public readonly AudioClip audioClip = AssetLoader.LoadEmbeddedAudio("sneeze.wav");
        public AudioSource audioSource;
        public readonly Sprite moodleSprite = AssetLoader.LoadEmbeddedSprite("sneeze.png");
        public bool WarningDialogueDone = false;
    }

    [HarmonyPatch(typeof(PlayerCamera), "Update")]
    public static class BodyUpdateStatusPatch
    {
        [HarmonyPostfix]
        private static void Postfix(PlayerCamera __instance)
        {
            SneezeStatus status = __instance.body.GetStatus<SneezeStatus>();
            if (status.body == null)
            {
                status.body = __instance.body;
                status.audioSource = __instance.body.gameObject.AddComponent<AudioSource>();
                status.audioSource.clip = status.audioClip;
                status.audioSource.volume = (float)Settings.GetSetting<SettingFloat>("mastervolume").GetValue();
            }

            if (__instance.body.HasWearable("dustmask"))
            {
                status.SneezeTime = 0f;
                status.Sneezing = false;
                status.SneezeCooldown = 0f;
            }

            if (!__instance.body.sleeping && !__instance.body.vomiter.vomiting && !__instance.body.harmer.harming && !__instance.body.inCardiacArrest && __instance.body.respiratoryRate > 0f)
            {
                status.SneezeTime += Time.deltaTime;
            }
            if (status.SneezeTime >= 295f && status.SneezeTime < 298f)
            {
                MoodleRegistry.AddMoodle(
                    1,
                    status.moodleSprite,
                    LocaleRegistry.Get("other", "expsneeze.status.sneeze", "About to sneeze"),
                    LocaleRegistry.Get("other", "expsneeze.status.sneezedsc", "Buildup of foreign particles in your nostrils has irritated your nose. Forceful nasal expulsion imminent. In other words, it's gonna blow!"),
                    holdSeconds: 0f
                );
            }
            if (status.SneezeTime >= 298f && status.SneezeTime < 299f)
            {
                if (!status.WarningDialogueDone)
                {
                    string[] rand = [
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze1", "Ah...!"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze2", "Hh..."),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze3", "I'm gonna...!"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze4", "Uh oh...!"),
                        LocaleRegistry.Get("other", "expsneeze.dialogue.presneeze5", "*sharp inhale*"),
                    ];
                    __instance.body.talker.Talk(rand[UnityEngine.Random.Range(0,5)], force: true, resetTalkTimer: true);
                    status.WarningDialogueDone = true;
                }
                MoodleRegistry.AddMoodle(
                    2,
                    status.moodleSprite,
                    LocaleRegistry.Get("other", "expsneeze.status.sneeze", "About to sneeze"),
                    LocaleRegistry.Get("other", "expsneeze.status.sneezedsc", "Buildup of foreign particles in your nostrils has irritated your nose. Forceful nasal expulsion imminent. In other words, it's gonna blow!"),
                    holdSeconds: 0f
                );
            }
            if (status.SneezeTime >= 299f && status.SneezeTime < 300f)
            {
                MoodleRegistry.AddMoodle(
                    3,
                    status.moodleSprite,
                    LocaleRegistry.Get("other", "expsneeze.status.sneeze", "About to sneeze"),
                    LocaleRegistry.Get("other", "expsneeze.status.sneezedsc", "Buildup of foreign particles in your nostrils has irritated your nose. Forceful nasal expulsion imminent. In other words, it's gonna blow!"),
                    critical: true,
                    holdSeconds: 0f
                );
            }

            if (status.SneezeTime >= 300f)
            {
                status.Sneezing = true;
            }

            if (status.Sneezing)
            {
                if (status.SneezeCooldown <= 0f)
                {
                    __instance.body.consciousness = Mathf.Max(72f, __instance.body.consciousness - 0.47f);
                    if (__instance.body.consciousness <= 72f)
                    {
                        //Plugin.Logger.LogInfo("ACHOO!");
                        string[] rand = [
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze1", "ACHOO!"),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze2", "CHOO!"),
                            LocaleRegistry.Get("other", "expsneeze.dialogue.sneeze3", "CHU!")
                        ];

                        __instance.body.talker.Talk(rand[UnityEngine.Random.Range(0,3)], force: true, resetTalkTimer: true);
                        status.audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                        status.audioSource.Play();
                        if (UnityEngine.Random.Range(1, 11) == 1)
                        {
                            __instance.body.DropItem(2);
                        }
                        __instance.body.rb.velocity += new Vector2(-Mathf.Sign(__instance.body.targetLookPos.x) * 10f, 0f);
                        __instance.body.stamina -= 1;
                        __instance.shaker.Shake(10f);
                        __instance.body.consciousness = 55f;
                        status.SneezeCooldown = 1f;
                        if (UnityEngine.Random.Range(1, 11) >= 5)
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
                        ];

                        status.WarningDialogueDone = false;
                        __instance.body.talker.TalkDelayed(UnityEngine.Random.Range(1f, 2.5f), rand[UnityEngine.Random.Range(0,4)], force: true, resetTalkTimer: true);
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
                status.SneezeTime += atk.cooldown * 1f;
                Plugin.Logger.LogInfo($"SneezeTime: {status.SneezeTime}");
            }
        }
    }
}
