using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExpSneeze
{
    public static class Items
    {
        public static void RegisterItems()
        {
            ItemRegistry.Register("makeshiftmask", new CustomItemInfo
            {
                fullName = LocaleRegistry.Get("item", "expsneeze.item.makeshiftmask", "Makeshift mask"),
                description = LocaleRegistry.Get("item", "expsneeze.item.makeshiftmaskdsc", "A crudely made dust mask fashioned from rough canvas and string. Offers negligible head protection. Won't stop you from sneezing, but it should at least reduce the irritation somewhat."),
                category = "utility",
                weight = 0.1f,
                value = 4,
                wearable = true,
                wearableCanBeHeld = false,
                wearableArmor = 0.05f,
                wearableIsolation = 0.04f,
                wearableHitDurabilityLossMultiplier = 0.75f,
                wearSlotId = "mouth",
                desiredWearLimb = "Head",
                destroyAtZeroCondition = true,
                decayMinutes = 24f,
                rec = new Recognition(1),
                scaleWeightWithCondition = false,
                qualities = new List<CraftingQuality>(["rippable"]),
                DropPool = DropPool.Corpse | DropPool.AllTraders | DropPool.MedicalCrate | DropPool.ContainerCrate | DropPool.CapsuleContainer
            }, Plugin.makeshiftMaskSprite);
        }

        public static void RegisterRecipes()
        {
            RecipeRegistry.Register(new Recipe
            {
                INT = 7,
                result = new RecipeResult {id = "makeshiftmask", amount = 1, resultCondition = 1f },
                category = Recipes.RecipeCategory.Utilities,
                items = new List<RecipeItem>
                {
                    new RecipeItem(0f) { specific = true, specificId = "canvas" },
                    new RecipeItem(0f) { specific = true, specificId = "canvas" },
                    new RecipeItem(0f) { specific = true, specificId = "string" },
                    new RecipeItem(0f) { specific = true, specificId = "string" },
                    new RecipeItem(0f) { specific = true, specificId = "string" },
                    new RecipeItem(0f) { quality = new CraftingQuality("cutting", 1f), minimumCondition = 0f, destroyItem = false}
                }
            });

            RecipeRegistry.Register(new Recipe
            {
                INT = 10,
                result = new RecipeResult {id = "dustmask", amount = 1, resultCondition = 1f },
                category = Recipes.RecipeCategory.Utilities,
                items = new List<RecipeItem>
                {
                    new RecipeItem(0f) { specific = true, specificId = "makeshiftmask", destroyItem = true },
                    new RecipeItem(0f) { specific = true, specificId = "flexiglass" },
                    new RecipeItem(0f) { specific = true, specificId = "plasticchunk" },
                    new RecipeItem(0f) { specific = true, specificId = "charcoal" },
                    new RecipeItem(30f) { specific = true, specificId = "biochem", isLiquid = true, destroyItem = false },
                    new RecipeItem(0f) { quality = new CraftingQuality("nails", 0.5f), minimumCondition = 0f, destroyItem = false},
                    new RecipeItem(0f) { quality = new CraftingQuality("cutting", 1f), minimumCondition = 0f, destroyItem = false}
                },
            });
        }
    }
}
