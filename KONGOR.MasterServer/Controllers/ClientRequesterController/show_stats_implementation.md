# show_stats Endpoint Implementation Guide

## Overview

The `show_stats` endpoint returns player statistics based on a `table` parameter. This document outlines the expected request/response format based on analysis of:

- **Legacy PHP API**: `C:\Users\SADS-810\Source\HON-Zend-Server-API\masterapi-international\client_controller.php` (lines 1031-1095)
- **PHP Implementation**: `C:\Users\SADS-810\Source\HON-Zend-Server-API\masterapi-international\client_request_class.php` (lines 2973-3291)
- **Lua Consumer**: `C:\Users\SADS-810\Source\HON\Heroes of Newerth\game\resources0\ui\scripts\newui\player_stats_v2.lua`

## Request Format

```
POST /client_requester.php
Content-Type: application/x-www-form-urlencoded

f=show_stats&nickname=PLAYERNAME&cookie=SESSION_COOKIE&table=TABLE_TYPE
```

### Supported Table Types

| Table | Database Table | Field Prefix | Description |
|-------|----------------|--------------|-------------|
| `player` | `player_stats` | `acc_` | Public match statistics |
| `ranked` | `ranked_stats` | `rnk_` | Ranked matchmaking statistics |
| `casual` | `casual_stats` | `cs_` | Casual matchmaking statistics |
| `campaign` | `campaign_stats` | `cam_` | Campaign (ranked season) statistics |
| `campaign_casual` | `campaign_casual_stats` | `cam_cs_` | Campaign casual statistics |
| `mastery` | N/A | N/A | Hero mastery progression |

## Response Format

All responses are PHP-serialized arrays. The client deserializes these and triggers Lua callbacks.

---

## Table: `mastery`

**Source**: `client_controller.php:1365-1514` (`show_mastery_stats` function)

### Response Structure

```php
[
    // Account Information
    'account_id' => int,
    'nickname' => string,           // With clan tag: "[TAG]Name"
    'name' => string,               // Clan name (empty if no clan)
    'rank' => string,               // Clan rank/tier
    'standing' => int,              // Account standing (moderation status)
    'create_date' => string,        // Format: "MM/dd/yyyy"
    'last_activity' => string,      // Format: "MM/dd/yyyy"
    'selected_upgrades' => array,   // Currently equipped store items
    'level' => int,                 // Account level
    'level_exp' => int,             // Current experience points

    // Hero Mastery Data
    'mastery_info' => [
        [
            'heroname' => string,   // e.g., "Hero_Pyromancer"
            'exp' => int            // Mastery experience for this hero
        ],
        // ... one entry per hero with mastery progress
    ],

    // Mastery Rewards (only if viewing own account)
    'mastery_rewards' => [
        [
            'level' => int,         // Mastery reward level (1-40)
            'alreadygot' => bool,   // Whether reward has been claimed
            'reward' => [
                'product_id' => int,
                'product_name' => string,
                'product_local_content' => string,
                'quantity' => int,
                'points' => int,        // Gold coins reward
                'mmpoints' => int,      // Silver coins reward
                'tickets' => int        // Game tokens reward
            ]
        ],
        // ... one entry per mastery level
    ]
]
```

### Notes

- `mastery_rewards` is only populated when viewing your own account (`$account_id == $that_account_id`)
- Hero mastery info comes from `MasteryService::getVerboseAccountMastery()`
- Rewards are sorted by level ascending

---

## Table: `campaign` / `campaign_casual`

**Source**: `client_request_class.php:2973-3291`

### Lua Consumer Field Order

The Lua client (`player_stats_v2.lua:1285-1435`) expects fields in a specific order via the `OnPlayerStatsNormalSeasonResult` callback. There are **149 arguments** expected.

### Response Structure

```php
[
    // Account Information
    'super_id' => string,
    'nickname' => string,           // With clan tag
    'name' => string,               // Clan name
    'rank' => string,               // Clan rank
    'standing' => string,           // "3" = good standing
    'account_type' => string,       // Account type enum value
    'account_id' => string,
    'level' => string,              // Account level
    'level_exp' => int,             // Current level experience
    'last_activity' => string,      // "MM/dd/yyyy"
    'create_date' => string,        // "MM/dd/yyyy"

    // Campaign-Specific Stats (cam_ prefix)
    'cam_games_played' => string,
    'cam_wins' => string,
    'cam_losses' => string,
    'cam_concedes' => string,
    'cam_concedevotes' => string,
    'cam_buybacks' => string,
    'cam_discos' => string,
    'cam_kicked' => string,

    // MMR Ratings
    'cam_amm_solo_rating' => string,    // Format: "1500.00"
    'cam_amm_solo_count' => string,
    'cam_amm_solo_conf' => string,
    'cam_amm_solo_prov' => string,
    'cam_amm_solo_pset' => string,
    'cam_amm_team_rating' => string,
    'cam_amm_team_count' => string,
    'cam_amm_team_conf' => string,
    'cam_amm_team_prov' => string,
    'cam_amm_team_pset' => string,

    // Combat Statistics
    'cam_herokills' => string,
    'cam_herodmg' => string,
    'cam_heroexp' => string,
    'cam_herokillsgold' => string,
    'cam_heroassists' => string,
    'cam_deaths' => string,
    'cam_goldlost2death' => string,
    'cam_secs_dead' => string,

    // Creep Statistics
    'cam_teamcreepkills' => string,
    'cam_teamcreepdmg' => string,
    'cam_teamcreepexp' => string,
    'cam_teamcreepgold' => string,
    'cam_neutralcreepkills' => string,
    'cam_neutralcreepdmg' => string,
    'cam_neutralcreepexp' => string,
    'cam_neutralcreepgold' => string,

    // Building Statistics
    'cam_bdmg' => string,
    'cam_bdmgexp' => string,
    'cam_razed' => string,
    'cam_bgold' => string,

    // Economy
    'cam_denies' => string,
    'cam_exp_denied' => string,
    'cam_gold' => string,
    'cam_gold_spent' => string,
    'cam_exp' => string,
    'cam_actions' => string,
    'cam_secs' => string,
    'cam_consumables' => string,
    'cam_wards' => string,
    'cam_em_played' => string,
    'cam_time_earning_exp' => string,

    // Multi-kills and Streaks
    'cam_bloodlust' => string,
    'cam_doublekill' => string,
    'cam_triplekill' => string,
    'cam_quadkill' => string,
    'cam_annihilation' => string,
    'cam_ks3' => string,
    'cam_ks4' => string,
    'cam_ks5' => string,
    'cam_ks6' => string,
    'cam_ks7' => string,
    'cam_ks8' => string,
    'cam_ks9' => string,
    'cam_ks10' => string,
    'cam_ks15' => string,
    'cam_smackdown' => string,
    'cam_humiliation' => string,
    'cam_nemesis' => string,
    'cam_retribution' => string,

    // Campaign Level/Experience
    'cam_level' => string,
    'cam_level_exp' => string,
    'cam_min_exp' => string,
    'cam_max_exp' => string,

    // Global Statistics (aggregates across all modes)
    'discos' => string,
    'possible_discos' => string,
    'games_played' => string,
    'num_bot_games_won' => string,
    'total_games_played' => int,
    'total_discos' => int,

    // Game Mode Breakdowns
    'acc_secs' => string,
    'acc_games_played' => string,
    'acc_discos' => string,
    'rnk_secs' => string,
    'rnk_games_played' => string,
    'rnk_discos' => string,
    'cs_secs' => string,
    'cs_games_played' => string,
    'cs_discos' => string,
    'mid_games_played' => string,
    'mid_discos' => string,
    'rift_games_played' => string,
    'rift_discos' => string,

    // Season Information (CRITICAL)
    'season' => string,                     // Current season number
    'season_id' => string,                  // Same as season
    'highest_level_current' => string,      // Highest medal achieved this season
    'highest_ranking' => int,               // Highest leaderboard rank (if immortal)
    'current_level' => string,              // Current medal rank (0-12+)
    'current_ranking' => int,               // Current leaderboard position (if immortal)
    'level_percent' => double,              // Progress to next medal (0-100)

    // Previous Season Stats
    'prev_seasons_cam_games_played' => int,
    'prev_seasons_cam_discos' => int,
    'latest_season_cam_games_played' => string,
    'latest_season_cam_discos' => string,
    'curr_season_cam_games_played' => string,
    'curr_season_cam_discos' => string,

    // Campaign Casual Previous Seasons
    'prev_seasons_cam_cs_games_played' => int,
    'prev_seasons_cam_cs_discos' => int,
    'latest_season_cam_cs_games_played' => string,
    'latest_season_cam_cs_discos' => string,
    'curr_season_cam_cs_games_played' => string,
    'curr_season_cam_cs_discos' => string,

    // Campaign Rewards (CRITICAL)
    'con_reward' => [
        'old_lvl' => int,       // Previous reward level (-2 if none)
        'curr_lvl' => int,      // Current reward level (1-6)
        'next_lvl' => int,      // Next level to unlock (0 if maxed)
        'requirement' => int,   // Medal required for next level
        'reward_taken' => int   // Whether current level reward was claimed
    ],

    // Favourite Heroes
    'favHero1' => string,       // Hero name lowercase (e.g., "pyromancer")
    'favHero1Time' => double,   // Percentage of games played (0-100)
    'favHero1_2' => string,     // Hero identifier (e.g., "Hero_Pyromancer")
    'favHero2' => string,
    'favHero2Time' => double,
    'favHero2_2' => string,
    'favHero3' => string,
    'favHero3Time' => double,
    'favHero3_2' => string,
    'favHero4' => string,
    'favHero4Time' => double,
    'favHero4_2' => string,
    'favHero5' => string,
    'favHero5Time' => double,
    'favHero5_2' => string,

    // Match History
    'matchIds' => string,       // Space-separated match IDs (last 20)
    'matchDates' => string,     // Space-separated dates

    // Calculated Averages
    'k_d_a' => string,              // Format: "5.2/3.1/8.4"
    'avgGameLength' => double,
    'avgXP_min' => double,
    'avgDenies' => double,
    'avgCreepKills' => double,
    'avgNeutralKills' => double,
    'avgActions_min' => double,
    'avgWardsUsed' => double,

    // Store Items
    'slot_id' => string,
    'my_upgrades' => array,
    'selected_upgrades' => array,
    'my_upgrades_info' => array,    // Discriminated union with StoreItemData

    // Tokens and Misc
    'dice_tokens' => string,
    'game_tokens' => int,
    'season_level' => int,
    'creep_level' => int,
    'timestamp' => int,
    'vested_threshold' => int,

    // Quest Stats
    'quest_stats' => [
        'error' => [
            'quest_status' => int,
            'leaderboard_status' => int
        ]
    ],

    // Success indicator
    0 => true   // PHP property with key 0, indicates success
]
```

---

## Table: `player` / `ranked` / `casual`

Similar structure to `campaign` but with different field prefixes:
- `player`: `acc_` prefix
- `ranked`: `rnk_` prefix
- `casual`: `cs_` prefix

Key differences:
- No `current_level`, `highest_level_current`, `current_ranking` (no medal system)
- No `con_reward` object
- No season-specific fields

---

## Medal System (Campaign)

The campaign medal system uses numeric levels:

| Medal | Level | Name |
|-------|-------|------|
| 0 | 0 | Unranked (Placement) |
| 1-5 | 1-5 | Bronze V-I |
| 6-10 | 6-10 | Silver V-I |
| 11-14 | 11-14 | Gold V-II |
| 15-17 | 15-17 | Diamond V-III |
| 18-19 | 18-19 | Diamond II-I |
| 20+ | 12+ | Immortal |

For Immortal players, `current_ranking` shows leaderboard position (1-200 displayed).

---

## Campaign Reward Levels

| Level | Requirement (Medal) |
|-------|---------------------|
| 1 | Bronze V (Medal 1) |
| 2 | Silver V (Medal 6) |
| 3 | Gold V (Medal 11) |
| 4 | Diamond V (Medal 15) |
| 5 | Diamond II (Medal 18) |
| 6 | Immortal (Medal 20+) |

---

## Error Responses

```php
['error' => 'nickname_required']    // Missing nickname parameter
['error' => 'table_required']       // Missing table parameter
['error' => 'not_found']            // Account not found
['error' => 'not_found_2']          // No stats record found
['error' => 'no_connection']        // Database connection error
['error' => 'invalid_input']        // Invalid table type
['season_id' => -1]                 // No active season (campaign tables only)
```

---

## Implementation Checklist

- [x] `PlayerStatisticsResponse` (table=player)
- [x] `RankedStatisticsResponse` (table=ranked)
- [x] `CasualStatisticsResponse` (table=casual)
- [x] `CampaignStatisticsResponse` (table=campaign)
- [x] `CampaignCasualStatisticsResponse` (table=campaign_casual)
- [ ] `ShowMasteryStatisticsResponse` (table=mastery) - **EMPTY, NEEDS IMPLEMENTATION**
- [x] `StatisticsResponseHelper` (shared helpers)
- [x] `AggregateStatistics` (cross-mode aggregation)

---

## Known Issues

1. **Mastery endpoint returns empty response** - `ShowMasteryStatisticsResponse` class is empty
2. **Missing hero mastery data source** - No mastery tracking system implemented
3. **Missing match history** - `matchIds` and `matchDates` not populated
4. **Missing favourite heroes** - Hero play time tracking not implemented

---

## Sample Network Traffic Data

### table=ranked Response (038_s.txt)

```
a:152:{
    s:8:"super_id";s:6:"195592";
    s:8:"nickname";s:7:"[K]GOPO";
    s:8:"standing";s:1:"3";
    s:12:"account_type";s:1:"4";
    s:10:"account_id";s:6:"195592";
    s:16:"rnk_games_played";s:4:"3515";
    s:8:"rnk_wins";s:4:"1729";
    s:10:"rnk_losses";s:4:"1786";
    s:12:"rnk_concedes";s:4:"1427";
    s:16:"rnk_concedevotes";s:3:"476";
    s:12:"rnk_buybacks";s:3:"113";
    s:10:"rnk_discos";s:2:"12";
    s:10:"rnk_kicked";s:1:"4";
    s:19:"rnk_amm_solo_rating";s:8:"1500.000";
    s:18:"rnk_amm_solo_count";s:1:"0";
    s:17:"rnk_amm_solo_conf";s:4:"0.00";
    s:17:"rnk_amm_solo_prov";s:1:"0";
    s:17:"rnk_amm_solo_pset";s:1:"0";
    s:19:"rnk_amm_team_rating";s:8:"1611.374";
    s:18:"rnk_amm_team_count";s:4:"3515";
    s:17:"rnk_amm_team_conf";s:4:"0.00";
    s:17:"rnk_amm_team_prov";s:4:"3515";
    s:17:"rnk_amm_team_pset";s:3:"127";
    s:13:"rnk_herokills";s:5:"15888";
    s:11:"rnk_herodmg";s:8:"39981961";
    s:11:"rnk_heroexp";s:8:"21331137";
    s:17:"rnk_herokillsgold";s:7:"9804561";
    s:15:"rnk_heroassists";s:5:"30985";
    s:10:"rnk_deaths";s:5:"21211";
    s:18:"rnk_goldlost2death";s:7:"5456511";
    s:13:"rnk_secs_dead";s:7:"3716604";
    s:18:"rnk_teamcreepkills";s:6:"235510";
    s:16:"rnk_teamcreepdmg";s:9:"106249209";
    s:16:"rnk_teamcreepexp";s:8:"20899031";
    s:17:"rnk_teamcreepgold";s:7:"9136851";
    s:21:"rnk_neutralcreepkills";s:5:"53286";
    s:19:"rnk_neutralcreepdmg";s:8:"37046592";
    s:19:"rnk_neutralcreepexp";s:7:"4081042";
    s:20:"rnk_neutralcreepgold";s:7:"2298907";
    s:8:"rnk_bdmg";s:7:"3053909";
    s:11:"rnk_bdmgexp";s:1:"0";
    s:9:"rnk_razed";s:4:"2713";
    s:9:"rnk_bgold";s:7:"4681848";
    s:10:"rnk_denies";s:5:"23752";
    s:14:"rnk_exp_denied";s:7:"1034671";
    s:8:"rnk_gold";s:8:"29223924";
    s:14:"rnk_gold_spent";s:8:"27540188";
    s:7:"rnk_exp";s:8:"46383688";
    s:11:"rnk_actions";s:8:"10479212";
    s:8:"rnk_secs";s:7:"7526246";
    s:15:"rnk_consumables";s:5:"16498";
    s:9:"rnk_wards";s:4:"3639";
    s:13:"rnk_em_played";s:1:"0";
    s:9:"rnk_level";s:2:"48";
    s:13:"rnk_level_exp";s:6:"122185";
    s:11:"rnk_min_exp";s:6:"117500";
    s:11:"rnk_max_exp";s:6:"122399";
    s:20:"rnk_time_earning_exp";s:7:"7480931";
    s:13:"rnk_bloodlust";s:3:"306";
    s:14:"rnk_doublekill";s:4:"1544";
    s:14:"rnk_triplekill";s:3:"158";
    s:12:"rnk_quadkill";s:2:"15";
    s:16:"rnk_annihilation";s:1:"0";
    s:7:"rnk_ks3";s:4:"1242";
    s:7:"rnk_ks4";s:3:"698";
    s:7:"rnk_ks5";s:3:"393";
    s:7:"rnk_ks6";s:3:"218";
    s:7:"rnk_ks7";s:3:"147";
    s:7:"rnk_ks8";s:2:"82";
    s:7:"rnk_ks9";s:2:"51";
    s:8:"rnk_ks10";s:2:"31";
    s:8:"rnk_ks15";s:1:"6";
    s:13:"rnk_smackdown";s:4:"1041";
    s:15:"rnk_humiliation";s:2:"17";
    s:11:"rnk_nemesis";s:5:"14741";
    s:15:"rnk_retribution";s:3:"362";
    s:5:"level";s:2:"58";
    s:9:"level_exp";i:173525;
    s:6:"discos";s:2:"72";
    s:15:"possible_discos";s:1:"0";
    s:12:"games_played";s:4:"7353";
    s:17:"num_bot_games_won";s:2:"10";
    s:18:"total_games_played";i:8321;
    s:12:"total_discos";i:49;
    s:8:"acc_secs";s:7:"1269820";
    s:16:"acc_games_played";s:3:"552";
    s:10:"acc_discos";s:1:"1";
    s:7:"cs_secs";s:6:"112945";
    s:15:"cs_games_played";s:2:"61";
    s:9:"cs_discos";s:1:"2";
    s:16:"mid_games_played";s:4:"3646";
    s:10:"mid_discos";s:2:"30";
    s:17:"rift_games_played";s:1:"0";
    s:11:"rift_discos";s:1:"0";
    s:13:"last_activity";s:10:"06/06/2022";
    s:11:"create_date";s:10:"08/11/2009";
    s:4:"name";s:6:"KONGOR";
    s:4:"rank";s:6:"Leader";
    s:8:"favHero1";s:6:"shaman";
    s:12:"favHero1Time";d:4.24;
    s:10:"favHero1_2";s:11:"Hero_Shaman";
    ...
    s:5:"k_d_a";s:9:"4.5/6/8.8";
    s:13:"avgGameLength";d:2141.18;
    s:9:"avgXP_min";d:372.02;
    s:9:"avgDenies";d:6.76;
    s:13:"avgCreepKills";d:67;
    s:15:"avgNeutralKills";d:15.16;
    s:14:"avgActions_min";d:83.54;
    s:12:"avgWardsUsed";d:1.04;
    s:11:"quest_stats";a:1:{s:5:"error";a:2:{s:12:"quest_status";i:0;s:18:"leaderboard_status";i:0;}}
    s:10:"con_reward";a:7:{
        s:7:"old_lvl";i:0;
        s:8:"curr_lvl";s:1:"3";
        s:8:"next_lvl";i:4;
        s:12:"require_rank";i:15;
        s:14:"need_more_play";i:4;
        s:17:"percentage_before";s:4:"0.00";
        s:10:"percentage";s:4:"0.00";
    }
    s:16:"vested_threshold";i:5;
    i:0;b:1;
}
```

### table=mastery Response (032_s.txt)

```
a:14:{
    s:10:"account_id";s:6:"195592";
    s:8:"nickname";s:7:"[K]GOPO";
    s:4:"name";s:6:"KONGOR";
    s:4:"rank";s:6:"Leader";
    s:8:"standing";s:1:"3";
    s:11:"create_date";s:10:"08/11/2009";
    s:13:"last_activity";s:10:"06/07/2022";
    s:17:"selected_upgrades";a:9:{...};
    s:5:"level";s:2:"58";
    s:9:"level_exp";s:6:"173525";
    s:12:"mastery_info";a:139:{
        i:0;a:2:{s:8:"heroname";s:12:"Hero_Armadon";s:3:"exp";s:5:"16951";}
        i:1;a:2:{s:8:"heroname";s:13:"Hero_Behemoth";s:3:"exp";s:5:"22769";}
        ...
    };
    s:15:"mastery_rewards";a:24:{
        i:0;a:3:{
            s:5:"level";i:1;
            s:10:"alreadygot";b:1;
            s:6:"reward";a:7:{
                s:10:"product_id";i:3609;
                s:12:"product_name";s:13:"Mastery Boost";
                s:21:"product_local_content";s:37:"/ui/fe2/store/icons/mastery_boost.tga";
                s:8:"quantity";i:2;
                s:6:"points";i:0;
                s:8:"mmpoints";i:0;
                s:7:"tickets";i:0;
            }
        }
        ...
    };
    s:16:"vested_threshold";i:5;
    i:0;b:1;
}
```

### table=campaign Response (033_s.txt)

```
a:158:{
    s:8:"super_id";s:6:"195592";
    s:8:"nickname";s:7:"[K]GOPO";
    s:8:"standing";s:1:"3";
    s:12:"account_type";s:1:"4";
    s:10:"account_id";s:6:"195592";
    s:5:"level";s:2:"58";
    s:9:"level_exp";i:173525;
    s:6:"season";s:2:"12";
    s:16:"cam_games_played";s:3:"341";
    s:8:"cam_wins";s:3:"180";
    s:10:"cam_losses";s:3:"160";
    s:12:"cam_concedes";s:3:"125";
    s:19:"cam_amm_team_rating";s:7:"1363.45";
    ...
    s:21:"highest_level_current";s:2:"12";
    s:13:"current_level";s:2:"12";
    s:13:"level_percent";d:36;
    s:9:"season_id";s:2:"12";
    s:10:"con_reward";a:7:{
        s:7:"old_lvl";i:0;
        s:8:"curr_lvl";s:1:"3";
        s:8:"next_lvl";i:4;
        s:12:"require_rank";i:15;
        s:14:"need_more_play";i:4;
        s:17:"percentage_before";s:4:"0.00";
        s:10:"percentage";s:4:"0.00";
    }
    ...
}
```

---

## File References

| File | Purpose |
|------|---------|
| `ClientRequesterController.Stats.cs` | Request handler |
| `PlayerStatisticsResponse.cs` | table=player response |
| `RankedStatisticsResponse.cs` | table=ranked response |
| `CasualStatisticsResponse.cs` | table=casual response |
| `CampaignStatisticsResponse.cs` | table=campaign response |
| `CampaignCasualStatisticsResponse.cs` | table=campaign_casual response |
| `ShowMasteryStatisticsResponse.cs` | table=mastery response (EMPTY) |
| `StatisticsResponseHelper.cs` | Shared helpers + AggregateStatistics |
| `MatchStatsResponse.cs` | Contains CampaignReward class |
