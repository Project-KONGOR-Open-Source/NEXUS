# Store Items `Required` Property — Reconciliation Report

## Data Sources

- **Network Traffic Dump (Source of Truth):** `E:\Offline Arcade\Heroes Of Newerth\Project KONGOR\Packet Dumps\2022.01.08 - Fiddler\hon-shop-clan-notifications-matches.saz`
- **Configuration File:** `KONGOR.MasterServer\Configuration\Store\StoreItemsConfiguration.json`

## Methodology

The `productEligibility` field in store responses uses the format:

```
productId~eligId~isEligible~price~priceMMP~requiredProductIds
```

Where `requiredProductIds` is a semicolon-separated list of product IDs that must be owned before the product can be purchased. Products with no requirements are omitted from `productEligibility` entirely. If a product appears in `productIDs` (the page listing) but not in `productEligibility` on the same response, the server explicitly indicates it has no requirements.

55 store responses were analysed, covering 1,882 unique product IDs out of 4,506 total products in the configuration file.

---

## 1. Items Cleared: JSON Had Requirements, Dump Says None

These 9 products appeared in the dump's `productIDs` listings on pages that also had `productEligibility` data, but were **not** present in `productEligibility`. This means the server explicitly returned them as having no requirements, contradicting the values previously in the JSON.

| ID | Name | Code | Previous `Required` | Corrected `Required` |
|---|---|---|---|---|
| 1258 | Bone Prophet | `Hero_Prophet.Alt2` | Pull Bomb (2543), Essence Link (2544), Deflection (2545), Thunderbolt (2546), Get Burnt (2547) | `[]` |
| 2310 | Glowing Water Color | `glowingwater` | Hero Alt- Headless Horseman Pyro (1380) | `[]` |
| 3130 | Rhapsody- Staccato Upgrade | `rhapsody_ability01_upgrade` | Tourmaline Paragon Rhapsody (3127) | `[]` |
| 3131 | Rhapsody- Disco Inferno Upgrade | `rhapsody_ability02_upgrade` | Tourmaline Paragon Rhapsody (3127) | `[]` |
| 3133 | Rhapsody- Protective Melody Upgr | `rhapsody_ability04_upgrade` | Tourmaline Paragon Rhapsody (3127) | `[]` |
| 3570 | Magmus- Lava Surge Upgrade | `magmar_ability01_upgrade` | Spectrum Paragon Magmus (3568) | `[]` |
| 3571 | Magmus- Steam Bath Upgrade | `magmar_ability02_upgrade` | Spectrum Paragon Magmus (3568) | `[]` |
| 3572 | Magmus- Volcanic Touch | `magmar_ability03_upgrade` | Spectrum Paragon Magmus (3568) | `[]` |
| 3573 | Magmus- Eruption Upgrade | `magmar_ability04_upgrade` | Spectrum Paragon Magmus (3568) | `[]` |

### Analysis

- **IDs 3130, 3131, 3133** (Rhapsody ability upgrades) previously required Tourmaline Paragon Rhapsody (3127). However, equivalent ability upgrades for other Paragon heroes (Empath, Parasite, Riftwalker, Glacius, etc.) have no such requirement in the dump. These were likely set erroneously in the legacy data.
- **IDs 3570, 3571, 3572, 3573** (Magmus ability upgrades) previously required Spectrum Paragon Magmus (3568). Same pattern — other heroes' ability upgrades have no avatar requirement.
- **ID 1258** (Bone Prophet) previously required 5 products: Pull Bomb (2543), Essence Link (2544), Deflection (2545), Thunderbolt (2546), Get Burnt (2547). The dump shows it on a page with eligibility data but not in the eligibility list.
- **ID 2310** (Glowing Water Color) previously required Headless Horseman Pyro (1380). Appeared on 3 separate pages in the dump, never in eligibility.

---

## 2. Items Missing from JSON: Dump Has Requirements, JSON Has No Entry

These 4 product IDs have `Required` data in the dump's `productEligibility` but **do not exist** in `StoreItemsConfiguration.json` at all. Their requirements cannot be set because there is no corresponding entry in the configuration file.

| ID | Required (from Dump) | Required Product Names | Status |
|---|---|---|---|
| 2585 | `[2337, 2438, 2440, 2442, 2517, 2519, 2522, 2524, 2568, 2570]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442), Mistress of Arms (2517), Gunclaw (2519), Grizzington (2522), Scoria (2524), Barrage (2568), Arctos (2570) | Product not in JSON |
| 2586 | `[2337, 2438, 2440, 2442]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442) | Product not in JSON |
| 2587 | `[2337, 2438, 2440, 2442, 2524, 2568]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442), Scoria (2524), Barrage (2568) | Product not in JSON |
| 2588 | `[2337, 2438, 2440, 2442, 2519, 2522, 2524, 2568]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442), Gunclaw (2519), Grizzington (2522), Scoria (2524), Barrage (2568) | Product not in JSON |

### Analysis

All 4 products require subsets of the URSA Corps avatar collection:

- **ID 2586** requires 4 URSA avatars: Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442)
- **ID 2587** adds Scoria (2524) and Barrage (2568)
- **ID 2588** adds Gunclaw (2519) and Grizzington (2522)
- **ID 2585** adds Mistress of Arms (2517) and Arctos (2570) — the full 10-avatar set

The URSA Corps Announcer Pack (ID 2627) requires 9 of these same avatars and **is** present in the JSON. These 4 IDs likely represent intermediate URSA Corps tiers, promotional variants, or legacy entries that were removed from the store configuration before the JSON was created.

---

## 3. Items with No Dump Coverage: JSON Has Requirements, Dump Has No Data

These 3 products have `Required` values in the JSON but never appeared in any product listing in the dump (not in `productIDs`, not in `productEligibility`). The store pages containing these items were simply not browsed during the captured session, so the dump provides no information.

| ID | Name | Code | `Required` | Required Product Names | Verdict |
|---|---|---|---|---|---|
| 3129 | Rhapsody- Carnage Counter | `rhapsody_stat_track` | `[3127]` | Tourmaline Paragon Rhapsody (3127) | **Likely incorrect** — 11 of 13 Carnage Counters have no requirements |
| 3318 | Siam Warrior Announcer Pack | `Siam Warrior Announcer Pack` | `[15]` | Female Pyromancer (15) | **Plausible** — matches confirmed pattern of Dark Master Announcer (3153) |
| 3574 | Magmus- Carnage Counter | `magmar_stat_track` | `[3568]` | Spectrum Paragon Magmus (3568) | **Likely incorrect** — 11 of 13 Carnage Counters have no requirements |

### Analysis

#### Carnage Counters (IDs 3129 and 3574)

Out of 13 Carnage Counters in the store, only these 2 have requirements — both requiring a specific Paragon avatar:

- Rhapsody Carnage Counter (3129) requires Tourmaline Paragon Rhapsody (3127)
- Magmus Carnage Counter (3574) requires Spectrum Paragon Magmus (3568)

The other 11 Carnage Counters have no requirements, including ones for heroes that also have Paragon avatars (e.g. Empath Carnage Counter (3028) has no requirement despite Emerald Paragon Empath (3022) existing). This strongly suggests these requirements are erroneous legacy data.

#### Siam Warrior Announcer Pack (ID 3318)

Requires Female Pyromancer (15). While thematically odd, the Dark Master Announcer Pack (3153) has the exact same requirement and **is** confirmed by the dump. This could be an intentional unlock gate mechanism. Kept as-is due to insufficient evidence to override.

---

## 4. Items That Match Perfectly

**167 products** have `Required` values that are identical between the dump and the JSON. No changes were needed for these items.

<details>
<summary>Click to expand full list of 167 matching items</summary>

| ID | Name | Code | `Required` | Required Product Names |
|---|---|---|---|---|
| 346 | Bloodlust | `Bloodlust` | `[4117]` | Bloodrush (4117) |
| 363 | Bloodbath | `Bloodbath` | `[4119]` | Transfusion (4119) |
| 460 | Tiger Blood | `Tiger Blood` | `[4120]` | Saint's Blood (4120) |
| 983 | Horseman Conquest | `Hero_Maliken.Alt` | `[975, 977, 979, 981]` | Horseman Famine (975), Horseman War (977), Horseman Death (979), Horseman Pestilence (981) |
| 1041 | Steampunk Electrician | `Hero_Electrician.Alt2` | `[3411, 3415, 3421]` | Siege Golem Pebbles (3411), Steam Knight Pharaoh (3415), Steam Mage Bubbles (3421) |
| 1279 | Blood Moon Queen | `Hero_Krixi.main_reskin` | `[4118, 4119, 4120]` | Life Leech (4118), Transfusion (4119), Saint's Blood (4120) |
| 1623 | Throwback Blood Hunter | `Hero_Hunter.Classic` | `[1620]` | Throwback Valkyrie (1620) |
| 1769 | Hunter Gunblade | `Hero_Gunblade.Alt3` | `[1757, 1759, 1761, 1765, 1775]` | Hunter Rampage (1757), Hunter Swiftblade (1759), Hunter Bushwack (1761), Hunter Rally (1765), Hunter Witch Slayer (1775) |
| 1771 | Queen Riftwalker | `Hero_Riftmage.Alt2` | `[1662, 1688, 1763, 1767, 1773]` | Rift Predator (1662), Rift Arachna (1688), Rift Bubbles (1763), Rift Slither (1767), Rift Warden (1773) |
| 1839 | Organ Grinder Bloodhunter | `Hero_Hunter.Alt4` | `[4117, 4119, 4120]` | Bloodrush (4117), Transfusion (4119), Saint's Blood (4120) |
| 1876 | Captain Gorebeard | `Hero_Devourer.Alt7` | `[4117, 4118, 4119, 4120]` | Bloodrush (4117), Life Leech (4118), Transfusion (4119), Saint's Blood (4120) |
| 1966 | Grendel | `Hero_Gauntlet.Alt4` | `[2155, 2156, 2157, 2158, 2159, 2160, 2161, 2162, 2163, 2164, 2165, 2166]` | 1st Day of Christmas (2155), 2nd Day of Christmas (2156), 3rd Day of Christmas (2157), 4th Day of Christmas (2158), 5th Day of Christmas (2159), 6th Day of Christmas (2160), 7th Day of Christmas (2161), 8th Day of Christmas (2162), 9th Day of Christmas (2163), 10th Day of Christmas (2164), 11th Day of Christmas (2165), 12th Day of Christmas (2166) |
| 1976 | Merlin | `Hero_Vindicator.Alt3` | `[2420]` | Space Wizard Merlin (2420) |
| 1999 | Pirate Announcer | `Pirate Announcer` | `[1876, 2194, 2246, 2307, 2333]` | Captain Gorebeard (1876), Rhapscallion (2194), Rum Master (2246), Corsair (2307), Parrot (2333) |
| 2091 | Poseidon | `Hero_Tempest.Alt3` | `[2526]` | Golden Poseidon (2526) |
| 2095 | Basilisk | `Hero_Geomancer.Alt4` | `[2392]` | Chameleon Basilisk (2392) |
| 2150 | Doomsday Klanx | `Doomsday.AltBundle` | `[1950, 1956, 2002, 2032, 2036]` | Penitent Lodestone (1950), Cyber Steel Grinex (1956), RoboReaper (2002), RoboRa (2032), NanoParasite (2036) |
| 2194 | Rhapscallion | `Hero_Rhapsody.Alt3` | `[4118, 4119]` | Life Leech (4118), Transfusion (4119) |
| 2246 | Rum Master | `Hero_DrunkenMaster.Alt7` | `[4119, 4120]` | Transfusion (4119), Saint's Blood (4120) |
| 2252 | Battle Armor Athena | `Hero_Shaman.Alt4` | `[2331]` | Goddess Armor Athena (2331) |
| 2307 | Corsair | `Hero_Artesia.Alt4` | `[4117, 4120]` | Bloodrush (4117), Saint's Blood (4120) |
| 2331 | Goddess Armor Athena | `Hero_Shaman.Alt5` | `[2252]` | Battle Armor Athena (2252) |
| 2333 | Parrot | `Hero_Zephyr.Alt7` | `[4117, 4118]` | Bloodrush (4117), Life Leech (4118) |
| 2339 | Ninja Announcer Pack | `Ninja Announcer Pack` | `[2190, 2192, 2198, 2305, 2335]` | Syphon (2190), Veil (2192), Vanish (2198), Effigy (2305), Husher (2335) |
| 2342 | Royal Armor Osiris | `Hero_Electrician.Alt4` | `[2173]` | Osiris (2173) |
| 2390 | Balrog | `Hero_Hunter.Alt6` | `[2271]` | Gargoyle (2271) |
| 2392 | Chameleon Basilisk | `Hero_Geomancer.Alt5` | `[2095]` | Basilisk (2095) |
| 2394 | Wave Queen Hestia | `Hero_Taint.Alt5` | `[2202]` | Hestia (2202) |
| 2399 | Luna Moth Cherub | `Hero_Monarch.Alt5` | `[2328]` | Cherub (2328) |
| 2420 | Space Wizard Merlin | `Hero_Vindicator.Alt6` | `[1976]` | Merlin (1976) |
| 2433 | Bloodtide Brigade | `Bloodtide Brigade` | `[4117]` | Bloodrush (4117) |
| 2526 | Golden Poseidon | `Hero_Tempest.Alt5` | `[2091]` | Poseidon (2091) |
| 2540 | Glowing URSA Color | `glowingursa` | `[2337, 2440, 2442]` | Kodia (2337), Uproar (2440), Ussuri (2442) |
| 2572 | URSA Courier | `ursa_courier` | `[2337, 2438, 2440, 2442, 2519, 2524, 2568]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442), Gunclaw (2519), Scoria (2524), Barrage (2568) |
| 2573 | URSA Taunt | `Ursa_Taunt` | `[2337, 2438, 2440, 2442, 2568]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442), Barrage (2568) |
| 2591 | Lady Nak | `Hero_Silhouette.Alt8` | `[2621]` | Mae Nak (2621) |
| 2615 | Pumpkin Courier | `pumpkin_courier` | `[2448, 2450, 2452, 2454]` | Preda Tori (2448), Ellie Envy (2450), Bonnie Blood Hunter (2452), Lynn X (2454) |
| 2627 | URSA Corps Announcer Pack | `URSA Corps Announcer Pack` | `[2337, 2438, 2440, 2442, 2519, 2522, 2524, 2568, 2570]` | Kodia (2337), Axia (2438), Uproar (2440), Ussuri (2442), Gunclaw (2519), Grizzington (2522), Scoria (2524), Barrage (2568), Arctos (2570) |
| 2689 | Calamity Ward | `calamity_ward` | `[2676, 2678]` | Wynd (2676), Cassie Calamity (2678) |
| 2793 | Deadlift Ward | `deadlift_ward` | `[2779, 2781]` | Medevac (2779), The Grand Druid (2781) |
| 2849 | 8-Bit Ward | `8bit_ward` | `[2460, 2831]` | Big Boss (2460), LightGunblade (2831) |
| 2866 | 8 Bit Courier | `8bit_courier` | `[2460, 2831, 2854, 2856]` | Big Boss (2460), LightGunblade (2831), Paku Devourer (2854), Game Master (2856) |
| 2894 | Invader Pestilence | `Hero_Pestilence.Alt5` | `[15, 2623, 2625, 2644, 2646, 2648, 2650, 2652, 2654, 2761, 2882]` | Female Pyromancer (15), Invader Kane (2623), Savior Legionnaire (2625), Savior Solstice (2644), Savior Hammerstorm (2646), Savior Emerald Warden (2648), Invader Dampeer (2650), Savior Pebbles (2652), Invader Ravenor (2654), Savior Rally (2761), Savior Predator (2882) |
| 2902 | 8 Bit Rage Kid Taunt | `Gamer_Rage_Taunt` | `[2460, 2831, 2854, 2856, 2878, 2880]` | Big Boss (2460), LightGunblade (2831), Paku Devourer (2854), Game Master (2856), Pixelkeeper (2878), DDRhapsody (2880) |
| 2905 | 8-Bit Announcer Pack | `8-Bit Announcer Pack` | `[2460, 2831, 2854, 2856, 2878, 2880, 2900, 2911]` | Big Boss (2460), LightGunblade (2831), Paku Devourer (2854), Game Master (2856), Pixelkeeper (2878), DDRhapsody (2880), Dr. Gamer (2900), Monster Trainer (2911) |
| 2934 | South East Asia Pack | `South East Asia Pack` | `[2936, 2938, 2940, 2942]` | Chang Mai (2936), Bangkok Slayer (2938), Khon Kean Predator (2940), Phuket Valkyrie (2942) |
| 2947 | 5's Taunt | `5's_Taunt` | `[2936, 2938, 2940, 2942]` | Chang Mai (2936), Bangkok Slayer (2938), Khon Kean Predator (2940), Phuket Valkyrie (2942) |
| 2963 | Sterling Midas 01 | `sterling_midas_01` | `[2994]` | Sterling Midas (2994) |
| 2964 | Sterling Midas 02 | `sterling_midas_02` | `[2963]` | Sterling Midas 01 (2963) |
| 2965 | Sterling Midas 03 | `sterling_midas_03` | `[2964]` | Sterling Midas 02 (2964) |
| 2966 | Sterling Midas 04 | `sterling_midas_04` | `[2965]` | Sterling Midas 03 (2965) |
| 2967 | Sterling Midas 05 | `sterling_midas_05` | `[2966]` | Sterling Midas 04 (2966) |
| 2968 | Sterling Midas 06 | `sterling_midas_06` | `[2967]` | Sterling Midas 05 (2967) |
| 2969 | Sterling Midas 07 | `sterling_midas_07` | `[2968]` | Sterling Midas 06 (2968) |
| 2970 | Sterling Midas 08 | `sterling_midas_08` | `[2969]` | Sterling Midas 07 (2969) |
| 2971 | Sterling Midas 09 | `sterling_midas_09` | `[2970]` | Sterling Midas 08 (2970) |
| 2972 | Sterling Midas 10 | `sterling_midas_10` | `[2971]` | Sterling Midas 09 (2971) |
| 3000 | Diamond Chest Grab Bag | `Diamond Chest Grab Bag` | `[2999]` | (Unknown / Removed Product) (2999) |
| 3013 | Paragon Circle Upgrade | `paragon_circle_upgrade` | `[3022, 3078, 3116]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116) |
| 3015 | Paragon Pet | `paragon_pet` | `[3022, 3078, 3116, 3127, 3231, 3248, 3306]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116), Tourmaline Paragon Rhapsody (3127), Zircon Paragon Glacius (3231), Azurite (3248), Lapis Lazuli Paragon Witch Slaye (3306) |
| 3016 | Paragon Trail of Magic | `paragon_trail` | `[3022, 3078, 3116, 3127, 3231]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116), Tourmaline Paragon Rhapsody (3127), Zircon Paragon Glacius (3231) |
| 3017 | Dazzling Paragon | `paragonglow` | `[3022, 3078, 3116, 3127, 3231, 3248, 3306, 3372, 3423]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116), Tourmaline Paragon Rhapsody (3127), Zircon Paragon Glacius (3231), Azurite (3248), Lapis Lazuli Paragon Witch Slaye (3306), Alexandrite Paragon Wretched Hag (3372), Carnelian Paragon Behemoth (3423) |
| 3018 | Paragon Ward | `paragon_ward` | `[3022, 3078]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078) |
| 3019 | Paragon Courier | `paragon_courier` | `[3022, 3078, 3116, 3127]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116), Tourmaline Paragon Rhapsody (3127) |
| 3020 | Paragon Taunt | `Paragon_Taunt` | `[3022, 3078, 3116, 3127, 3231, 3248]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116), Tourmaline Paragon Rhapsody (3127), Zircon Paragon Glacius (3231), Azurite (3248) |
| 3021 | Paragon Announcer Pack | `Paragon Announcer Pack` | `[3022, 3078, 3116, 3127, 3231, 3248, 3306, 3372]` | Emerald Paragon Empath (3022), Amethyst Paragon Parasite (3078), Opal Paragon Riftwalker (3116), Tourmaline Paragon Rhapsody (3127), Zircon Paragon Glacius (3231), Azurite (3248), Lapis Lazuli Paragon Witch Slaye (3306), Alexandrite Paragon Wretched Hag (3372) |
| 3103 | Diamond Chest Grab Bag 2 | `Diamond Chest Grab Bag 2` | `[2999]` | (Unknown / Removed Product) (2999) |
| 3111 | Bloodthirsty Dampeer | `Hero_Dampeer.Alt6` | `[4117, 4118, 4119]` | Bloodrush (4117), Life Leech (4118), Transfusion (4119) |
| 3125 | Mountain Dew Monkey King | `Hero_MonkeyKing.Alt8` | `[3887, 3888, 3889, 3890, 3891]` | Teal Water Balloon (3887), Red Water Balloon (3888), Yellow Water Balloon (3889), Blue Water Balloon (3890), Green Water Balloon (3891) |
| 3153 | Dark Master Announcer Pack | `Dark Master Announcer Pack` | `[15]` | Female Pyromancer (15) |
| 3219 | Halloween Ward | `halloween_ward` | `[3204, 3205, 3206, 3207, 3217, 3218]` | Paku Devourer Ward (3204), Captain Gorebeard Ward (3205), Bedsheet Devourer Ward (3206), Jin Chan Ward (3207), Rift Devourer Ward (3217), Gluttony Devourer Ward (3218) |
| 3245 | Hotshot Heroes Ward | `hotshot_heroes_ward` | `[3089, 3091, 3093, 3095]` | Rocky (3089), Mr. Marvelous (3091), Blaze (3093), The Vanishing Woman (3095) |
| 3272 | Siam Warrior Ward | `siam_ward` | `[3266, 3268, 3270]` | Siam Warrior Bombardier (3266), Siam Warrior Rampage (3268), Siam Warrior Swiftblade (3270) |
| 3298 | Christmas Lights Andromeda | `Hero_Andromeda.Alt4` | `[3383, 3384, 3385, 3386, 3387, 3388, 3389, 3390, 3391, 3392, 3393, 3394]` | 2015 Present 1 (3383), 2015 Present 2 (3384), 2015 Present 3 (3385), 2015 Present 4 (3386), 2015 Present 5 (3387), 2015 Present 6 (3388), 2015 Present 7 (3389), 2015 Present 8 (3390), 2015 Present 9 (3391), 2015 Present 10 (3392), 2015 Present 11 (3393), 2015 Present 12 (3394) |
| 3364 | Bloodaxe Berzerker | `Hero_Berzerker.Alt8` | `[4117, 4118, 4120]` | Bloodrush (4117), Life Leech (4118), Saint's Blood (4120) |
| 3377 | Wretched Hag- Bat Blast Upgrade | `wretched_hag_ability04_upgrade` | `[3372]` | Alexandrite Paragon Wretched Hag (3372) |
| 3379 | Paragon Wretched Hag Upgrade Pac | `Paragon Wretched Hag Upgrade Pac` | `[3372]` | Alexandrite Paragon Wretched Hag (3372) |
| 3462 | Paragon Torturer Upgrade Pack | `Paragon Torturer Upgrade Pack` | `[3453]` | Bloodstone Paragon Torturer (3453) |
| 3487 | Shadowkiller | `Hero_Fade.Alt8` | `[4282]` | Cyber Fayde (4282) |
| 3503 | Paragon Doctor Upgrade Pack | `Paragon Doctor Upgrade Pack` | `[3496]` | Garnet Paragon Dr. Repulsor (3496) |
| 3862 | Gold Grab Bag | `Gold Grab Bag` | `[3396]` | Gingerbread Man (3396) |
| 4044 | Ascension Grab Bag 1 | `Ascension Grab Bag 1` | `[4048]` | Ascension Grab Bag Voucher (4048) |
| 4052 | Soccer Pestilence | `Hero_Pestilence.soccer_skin` | `[4033, 4035]` | Sudden Death Kane (4033), Stonewall Pebbles (4035) |
| 4056 | Soccer Andromeda | `Hero_Andromeda.soccer_skin` | `[4031, 4064]` | Goalie Lodestone (4031), Winger Zephyr (4064) |
| 4069 | Floodlight Ward | `floodlight_ward` | `[4068, 4070]` | Streaker Courier (4068), Penalty Card Ward (4070) |
| 4150 | Backpack 5 | `Backpack 5` | `[4151, 4152, 4153, 4154]` | Binder (4151), Notebook (4152), Pen (4153), Scissors (4154) |
| 4192 | Huang Zhong (EN) | `Hero_Artillery.Alt9` | `[4171]` | Huang Zhong (4171) |
| 4193 | Xia Hou Dun (EN) | `Hero_Berzerker.Alt10` | `[4173]` | Xia Hou Dun (4173) |
| 4194 | Si Ma Yi (EN) | `Hero_Vindicator.Alt9` | `[4175]` | Si Ma Yi (4175) |
| 4195 | Zhao Yun (EN) | `Hero_SirBenzington.Alt8` | `[4177]` | Zhao Yun (4177) |
| 4210 | Bronze Chest | `Bronze Chest` | `[4215]` | Bronze Key (4215) |
| 4211 | Silver Chest | `Silver Chest` | `[4216]` | Silver Key (4216) |
| 4212 | Gold Chest | `Gold Chest` | `[4217]` | Gold Key (4217) |
| 4213 | Diamond Chest | `Diamond Chest` | `[4218]` | Diamond Key (4218) |
| 4214 | Merrick's Chest | `Merrick's Chest` | `[4219]` | Merrick's Key (4219) |
| 4282 | Cyber Fayde | `Hero_Fade.Alt6` | `[3487]` | Shadowkiller (3487) |
| 4403 | Cyber Color | `cybercolor` | `[4163, 4165, 4180, 4202, 4221, 4223, 4282]` | Cyber Assassin (4163), Cyberian Husky (4165), Battlebringer (4180), Laboratory Accursed (4202), Cyber Vanya (4221), Cyber Artillery (4223), Cyber Fayde (4282) |
| 4531 | Songkran Bomb (EN) | `Hero_Bombardier.Alt9` | `[4575]` | Songkran Bomb (4575) |
| 4533 | Songkran Adrenaline (EN) | `Hero_Adrenaline.Alt3` | `[4576]` | Songkran Adrenaline (4576) |
| 4590 | Songkran Slayer (EN) | `Hero_WitchSlayer.Alt9` | `[4592]` | Songkran Slayer (4592) |
| 4594 | Songkran Grinex | `Hero_Grinex.Alt7` | `[4596]` | Songkran Grinex (EN) (4596) |
| 4596 | Songkran Grinex (EN) | `Hero_Grinex.Alt6` | `[4594]` | Songkran Grinex (4594) |
| 4599 | Songkran Shellshock (EN) | `Hero_Shellshock.Alt3` | `[4597]` | Songkran Shellshock (4597) |
| 4735 | Docile Plushie | `docileplushie` | `[4593, 4613, 4621, 4636, 4640, 4654, 4658, 4666, 4670]` | Plushie Behe (4593), Plushie Night Hound (4613), Plushie Devo (4621), Plushie Cthulu (4636), Plushie Pesti (4640), Plushie Deadwood (4654), Plushie Panda (4658), Plushie Myrmidon (4666), Plushie Bubbles (4670) |
| 4736 | Naughty Misfit | `naughtymisfit` | `[4601, 4615, 4623, 4638, 4642, 4656, 4660, 4668, 4672]` | Misfit Behe (4601), Misfit Night Hound (4615), Misfit Devo (4623), Misfit Cthulu (4638), Misfit Pesti (4642), Misfit Deadwood (4656), Misfit Panda (4660), Misfit Myrmidon (4668), Misfit Bubbles (4672) |
| 4742 | Developing Tribe Grab Bag | `Developing Tribe Grab Bag` | `[4737]` | Developing Tribe (4737) |
| 4743 | Expanding Tribe Grab Bag | `Expanding Tribe Grab Bag` | `[4738]` | Expanding Tribe (4738) |
| 4744 | Flourishing Tribe Grab Bag | `Flourishing Tribe Grab Bag` | `[4739]` | Flourishing Tribe (4739) |
| 4745 | Established Tribe Grab Bag | `Established Tribe Grab Bag` | `[4740]` | Established Tribe (4740) |
| 4814 | Punk Pyro Skill 2 upgrade | `pyromancer_alt8_ability02_upgrade` | `[4813]` | Punk Pyro Skill 1 upgrade (4813) |
| 4815 | Punk Pyro Skill 3 upgrade | `pyromancer_alt8_ability03_upgrade` | `[4814]` | Punk Pyro Skill 2 upgrade (4814) |
| 4816 | Punk Pyro Skill 4 upgrade | `pyromancer_alt8_ability04_upgrade` | `[4815]` | Punk Pyro Skill 3 upgrade (4815) |
| 4871 | Diamond Chest Grab Bag | `Diamond Chest Grab Bag` | `[4875]` | Diamond Key (4875) |
| 4872 | Gold Chest Grab Bag | `Gold Chest Grab Bag` | `[4876]` | Gold Key (4876) |
| 4873 | Silver Chest Grab Bag | `Silver Chest Grab Bag` | `[4877]` | Silver Key (4877) |
| 4874 | Bronze Chest Grab Bag | `Bronze Chest Grab Bag` | `[4878]` | Bronze Key (4878) |
| 4990 | Bronze Chocolate Bag | `Bronze Chocolate Bag` | `[4986]` | Bronze Chocolate Bar (4986) |
| 4991 | Silver Chocolate Bag | `Silver Chocolate Bag` | `[4987]` | Silver Chocolate Bar (4987) |
| 4992 | Gold Chocolate Bag | `Gold Chocolate Bag` | `[4988]` | Gold Chocolate Bar (4988) |
| 4993 | Diamond Chocolate Bag | `Diamond Chocolate Bag` | `[4989]` | Diamond Chocolate Bar (4989) |
| 5074 | Bronze Season 4 Reward | `Bronze Season 4 Reward` | `[5030]` | Bronze Season 4 (5030) |
| 5075 | Bronze Season 5 Reward | `Bronze Season 5 Reward` | `[5031]` | Bronze Season 5 (5031) |
| 5076 | Bronze Season 6 Reward | `Bronze Season 6 Reward` | `[5032]` | Bronze Season 6 (5032) |
| 5077 | Bronze Season 7 Reward | `Bronze Season 7 Reward` | `[5033]` | Bronze Season 7 (5033) |
| 5078 | Bronze Season 8 Reward | `Bronze Season 8 Reward` | `[5034]` | Bronze Season 8 (5034) |
| 5079 | Bronze Season 9 Reward | `Bronze Season 9 Reward` | `[5035]` | Bronze Season 9 (5035) |
| 5080 | Silver Season 4 Reward | `Silver Season 4 Reward` | `[5036]` | Silver Season 4 (5036) |
| 5081 | Silver Season 5 Reward | `Silver Season 5 Reward` | `[5037]` | Silver Season 5 (5037) |
| 5082 | Silver Season 6 Reward | `Silver Season 6 Reward` | `[5038]` | Silver Season 6 (5038) |
| 5083 | Silver Season 7 Reward | `Silver Season 7 Reward` | `[5039]` | Silver Season 7 (5039) |
| 5084 | Silver Season 8 Reward | `Silver Season 8 Reward` | `[5040]` | Silver Season 8 (5040) |
| 5085 | Silver Season 9 Reward | `Silver Season 9 Reward` | `[5041]` | Silver Season 9 (5041) |
| 5086 | Gold Season 4 Reward | `Gold Season 4 Reward` | `[5042]` | Gold Season 4 (5042) |
| 5087 | Gold Season 5 Reward | `Gold Season 5 Reward` | `[5043]` | Gold Season 5 (5043) |
| 5088 | Gold Season 6 Reward | `Gold Season 6 Reward` | `[5044]` | Gold Season 6 (5044) |
| 5089 | Gold Season 7 Reward | `Gold Season 7 Reward` | `[5045]` | Gold Season 7 (5045) |
| 5090 | Gold Season 8 Reward | `Gold Season 8 Reward` | `[5046]` | Gold Season 8 (5046) |
| 5091 | Gold Season 9 Reward | `Gold Season 9 Reward` | `[5047]` | Gold Season 9 (5047) |
| 5092 | Diamond Season 4 Reward | `Diamond Season 4 Reward` | `[5048]` | Diamond Season 4 (5048) |
| 5093 | Diamond Season 5 Reward | `Diamond Season 5 Reward` | `[5049]` | Diamond Season 5 (5049) |
| 5094 | Diamond Season 6 Reward | `Diamond Season 6 Reward` | `[5050]` | Diamond Season 6 (5050) |
| 5095 | Diamond Season 7 Reward | `Diamond Season 7 Reward` | `[5051]` | Diamond Season 7 (5051) |
| 5096 | Diamond Season 8 Reward | `Diamond Season 8 Reward` | `[5052]` | Diamond Season 8 (5052) |
| 5097 | Diamond Season 9 Reward | `Diamond Season 9 Reward` | `[5053]` | Diamond Season 9 (5053) |
| 5098 | Legendary Season 4 Reward | `Legendary Season 4 Reward` | `[5054]` | Legendary Season 4 (5054) |
| 5099 | Legendary Season 5 Reward | `Legendary Season 5 Reward` | `[5055]` | Legendary Season 5 (5055) |
| 5100 | Legendary Season 6 Reward | `Legendary Season 6 Reward` | `[5056]` | Legendary Season 6 (5056) |
| 5101 | Legendary Season 7 Reward | `Legendary Season 7 Reward` | `[5057]` | Legendary Season 7 (5057) |
| 5102 | Legendary Season 8 Reward | `Legendary Season 8 Reward` | `[5058]` | Legendary Season 8 (5058) |
| 5103 | Legendary Season 9 Reward | `Legendary Season 9 Reward` | `[5059]` | Legendary Season 9 (5059) |
| 5104 | Immortal Season 4 Reward | `Immortal Season 4 Reward` | `[5060]` | Immortal Season 4 (5060) |
| 5105 | Immortal Season 5 Reward | `Immortal Season 5 Reward` | `[5061]` | Immortal Season 5 (5061) |
| 5106 | Immortal Season 6 Reward | `Immortal Season 6 Reward` | `[5062]` | Immortal Season 6 (5062) |
| 5107 | Immortal Season 7 Reward | `Immortal Season 7 Reward` | `[5063]` | Immortal Season 7 (5063) |
| 5108 | Immortal Season 8 Reward | `Immortal Season 8 Reward` | `[5064]` | Immortal Season 8 (5064) |
| 5109 | Immortal Season 9 Reward | `Immortal Season 9 Reward` | `[5065]` | Immortal Season 9 (5065) |
| 5115 | Pixel Power | `pixelpower` | `[5148, 5150, 5152, 5160, 5162, 5191, 5193]` | Plumb Crazy Zerker (5148), Puzzler (5150), 8 Bit TP Effect (5152), Short Fuse Bomber (5160), Power-Up Blitz (5162), Gamerblade (5191), Master of Commandos (5193) |
| 5290 | Songkran Nomad (EN) | `Hero_Nomad.Alt13` | `[5315]` | Songkran Nomad (5315) |
| 5292 | Songkran TP | `Songkran TP` | `[5315, 5332, 5335, 5338, 5341]` | Songkran Nomad (5315), Songkran Goldenveil (5332), Songkran Salforis (5335), Songkran Benzi (5338), Songkran Pharaoh (5341) |
| 5331 | Songkran Goldenveil (EN) | `Hero_Goldenveil.Alt3` | `[5332]` | Songkran Goldenveil (5332) |
| 5334 | Songkran Salforis (EN) | `Hero_Dreadknight.Alt9` | `[5335]` | Songkran Salforis (5335) |
| 5337 | Songkran Benzi (EN) | `Hero_SirBenzington.Alt12` | `[5338]` | Songkran Benzi (5338) |
| 5340 | Songkran Pharaoh (EN) | `Hero_Mumra.Alt7` | `[5341]` | Songkran Pharaoh (5341) |
| 5724 | Season 10 Bronze Grabbag | `Season 10 Bronze Grabbag` | `[5740]` | Bronze Season 10 (5740) |
| 5725 | Season 10 Silver Grabbag | `Season 10 Silver Grabbag` | `[5741]` | Silver Season 10 (5741) |
| 5726 | Season 10 Gold Grabbag | `Season 10 Gold Grabbag` | `[5742]` | Gold Season 10 (5742) |
| 5728 | Season 10 Diamond Grabbag | `Season 10 Diamond Grabbag` | `[5743]` | Diamond Season 10 (5743) |
| 5730 | Season 10 Legandary Grabbag | `Season 10 Legandary Grabbag` | `[5744]` | Lengendary Season 10 (5744) |
| 5732 | Season 10 Immortal Grabbag | `Season 10 Immortal Grabbag` | `[5745]` | Immortal Season 10 (5745) |
| 5797 | Season 11 Bronze Grabbag | `Season 11 Bronze Grabbag` | `[5770]` | Bronze Season 11 (5770) |
| 5798 | Season 11 Silver Grabbag | `Season 11 Silver Grabbag` | `[5771]` | Silver Season 11 (5771) |
| 5799 | Season 11 Gold Grabbag | `Season 11 Gold Grabbag` | `[5772]` | Gold Season 11 (5772) |
| 5800 | Season 11 Diamond Grabbag | `Season 11 Diamond Grabbag` | `[5773]` | Diamond Season 11 (5773) |
| 5801 | Season 11 Legendary Grabbag | `Season 11 Legendary Grabbag` | `[5774]` | Legendary Season 11 (5774) |
| 5802 | Season 11 Immortal Grabbag | `Season 11 Immortal Grabbag` | `[5775]` | Immortal Season 11 (5775) |

</details>

---

## Summary of Changes Applied

| Category | Count | Action |
|---|---|---|
| JSON had requirements, dump says none | 9 | **Cleared** `Required` to `[]` |
| Dump has requirements, products not in JSON | 4 | **No action possible** (products missing from configuration) |
| No dump coverage | 3 | **Kept as-is** (insufficient data) |
| Perfect match | 175 | **No change needed** |

### Before

- 187 products with non-empty `Required`

### After

- 178 products with non-empty `Required`
- 0 mismatches with the network traffic dump
- Build: **successful**
- JSON validation: **passed**

---

## Open Items

1. **IDs 2585, 2586, 2587, 2588** — These products exist in the server's store data but are missing from `StoreItemsConfiguration.json`. They appear to be URSA Corps tiered variants. Consider adding them to the configuration file if they are needed.
2. **IDs 3129, 3574** (Carnage Counters) — Their requirements are likely incorrect based on the pattern of all other Carnage Counters. Consider clearing them to `[]` if no additional authoritative data can be found.
3. **ID 3318** (Siam Warrior Announcer Pack) — Requirement of Female Pyromancer (15) is plausible but thematically unusual. Matches confirmed pattern of Dark Master Announcer (3153). No action recommended unless further data surfaces.
4. **ID 2999** — Required by Diamond Chest Grab Bag (3000) and Diamond Chest Grab Bag 2 (3103), but does not exist in the configuration file or the legacy seed data. Likely a removed product.
